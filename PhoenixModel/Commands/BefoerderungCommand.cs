using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Befördert oder degradiert einen Charakter (Regelwerk 1.9.1).
    ///
    /// "Befördere Charakter 601 zum Burgherrn"
    /// "Degradiere Charakter 607 zum Burgherrn"
    ///
    /// Die Ämter lassen sich ausschreiben oder abkürzen: Heerführer/HF, Burgherr/BUH,
    /// Stadthalter/STH, Festungsherr/FSH, Herrscher/HER.
    ///
    /// Was dabei passiert, steht in <see cref="BeförderungsRules"/>: nach oben steigt nur das
    /// Gutpunktmaximum, nach unten fallen Maximum und aktueller Wert. Das Amt selbst steht in der
    /// Beschriftung der Figur.
    /// </summary>
    public class BeförderungCommand : BaseCommand, IPhoenixCommand, IEquatable<BeförderungCommand> {

        /// <summary>Die Nummer des Charakters</summary>
        public int UnitId { get; set; } = 0;

        /// <summary>Das Amt, in das befördert oder degradiert wird</summary>
        public Characterklasse Amt { get; set; } = Characterklasse.none;

        /// <summary>Geht es nach oben oder nach unten?</summary>
        public bool IstDegradierung { get; set; } = false;

        /// <summary>Das Amt vorher - für das Zurücknehmen</summary>
        public Characterklasse AmtVorher { get; set; } = Characterklasse.none;

        /// <summary>Die Gutpunkte vorher - für das Zurücknehmen</summary>
        public int GesamtVorher { get; set; } = 0;
        public int AktuellVorher { get; set; } = 0;

        /// <summary>
        /// Die Beschriftung vorher, wörtlich.
        ///
        /// Zurückgesetzt wird sie und nicht das Amt: nicht jede Beschriftung lässt sich aus einem
        /// Amt wieder zusammensetzen, und was der Spieler dort stehen hatte, gehört ihm.
        /// </summary>
        public string BeschriftungVorher { get; set; } = string.Empty;

        public BeförderungCommand(string commandString) : base(commandString) { }

        public override string ToString()
            => $"{(IstDegradierung ? "Degradiere" : "Befördere")} Charakter {UnitId} zum {Amt}";

        public override bool CanAppliedTo(ISelectable selectable) => selectable is Character;

        private Character? GetCharakter()
            => SpielfigurenView.GetSpielfigur(FigurType.Charakter, UnitId) as Character;

        public override CommandResult CheckPreconditions() {
            if (CommonCommandErrors.MissingUnitID(UnitId, CommandString, this, out CommandResult? fehlendeId) && fehlendeId != null)
                return fehlendeId;

            var charakter = GetCharakter();
            if (charakter == null)
                return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein Charakter",
                    $"Der Befehl kann nicht ausgeführt werden:\r\n {CommandString}", this);

            var geprüft = IstDegradierung
                ? BeförderungsRules.PrüfeDegradierung(charakter, Amt)
                : BeförderungsRules.PrüfeBeförderung(charakter, Amt);
            if (geprüft.HasErrors)
                return new CommandResultError(geprüft.Title, geprüft.Message, this);

            return new CommandResultSuccess(geprüft.Title, geprüft.Message, this);
        }

        public override CommandResult ExecuteCommand() {
            var result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var charakter = GetCharakter();
            if (charakter == null)
                return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein Charakter",
                    $"Der Befehl kann nicht ausgeführt werden:\r\n {CommandString}", this);

            AmtVorher = CharacterView.GetAssumedKlasse(charakter);
            BeschriftungVorher = charakter.Beschriftung;
            GesamtVorher = charakter.GP_ges;
            AktuellVorher = charakter.GP_akt;

            if (IstDegradierung)
                BeförderungsRules.Degradiere(charakter, Amt);
            else
                BeförderungsRules.Befördere(charakter, Amt);

            Update(charakter, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            return new CommandResultSuccess($"{charakter.Bezeichner} ist jetzt {Amt}",
                $"Gutpunkte {charakter.GP_akt} von {charakter.GP_ges}.", this);
        }

        public override bool CanUndo => AmtVorher != Characterklasse.none && GetCharakter() != null;

        public override CommandResult UndoCommand() {
            var charakter = GetCharakter();
            if (CanUndo == false || charakter == null)
                return new CommandResultError($"Die Beförderung von Charakter {UnitId} lässt sich nicht zurücknehmen",
                    "Der Stand vor der Beförderung ist nicht bekannt.", this);

            charakter.GP_ges = GesamtVorher;
            charakter.GP_akt = AktuellVorher;
            charakter.Beschriftung = BeschriftungVorher;

            Update(charakter, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            return new CommandResultSuccess($"{charakter.Bezeichner} ist wieder {AmtVorher}",
                $"Gutpunkte {charakter.GP_akt} von {charakter.GP_ges}.", this);
        }

        public bool Equals(BeförderungCommand? other)
            => other != null && UnitId == other.UnitId && Amt == other.Amt
            && IstDegradierung == other.IstDegradierung;

        public override bool Equals(object? obj) => obj is BeförderungCommand anderer && Equals(anderer);

        public override int GetHashCode() => HashCode.Combine(UnitId, Amt, IstDegradierung);
    }

    /// <summary>
    /// Liest Beförderungen und Degradierungen.
    ///
    /// "Befördere Charakter 601 zum Burgherrn"
    /// "Degradiere 607 zum Heerführer"
    /// </summary>
    public class BeförderungCommandParser : SimpleParser {

        private static readonly Regex BefehlRegex = new(
            @"^(?<art>Befördere|Befoerdere|Degradiere)\s+(?:Charakter\s+)?(?<UnitId>\d+)\s+(?:zum|zur|als)\s+(?<amt>[\wäöüÄÖÜß]+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Die Ämter, ausgeschrieben und abgekürzt. Die gebeugten Formen kommen mit: wer "zum
        /// Burgherrn" schreibt, meint den Burgherren.
        /// </summary>
        public static Characterklasse LiesAmt(string? wort) {
            string amt = (wort ?? string.Empty).Trim().ToLowerInvariant();
            if (amt.StartsWith("heerführer") || amt.StartsWith("heerfuehrer") || amt == "hf")
                return Characterklasse.HF;
            if (amt.StartsWith("burgherr") || amt == "buh")
                return Characterklasse.BUH;
            if (amt.StartsWith("stadthalter") || amt.StartsWith("statthalter") || amt == "sth")
                return Characterklasse.STH;
            if (amt.StartsWith("festungsherr") || amt == "fsh")
                return Characterklasse.FSH;
            if (amt.StartsWith("herrscher") || amt == "her")
                return Characterklasse.HER;
            return Characterklasse.none;
        }

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            command = null;
            var treffer = BefehlRegex.Match(commandString.Trim());
            if (treffer.Success == false)
                return false;

            try {
                var amt = LiesAmt(treffer.Groups["amt"].Value);
                if (amt == Characterklasse.none)
                    return false;

                command = new BeförderungCommand(commandString) {
                    UnitId = ParseInt(treffer.Groups["UnitId"].Value),
                    Amt = amt,
                    IstDegradierung = treffer.Groups["art"].Value.StartsWith("Degradiere", StringComparison.OrdinalIgnoreCase),
                };
                return true;
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des Beförderungsbefehls gab es einen Fehler", ex.Message);
                return false;
            }
        }
    }
}
