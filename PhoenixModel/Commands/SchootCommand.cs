using PhoenixModel.Commands.Parser;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Setzt eine Figur auf ein Zielfeld an: der Beschussbefehl wird in der Spalte Befehl_ang der
    /// Figur festgehalten und von der Spielleitung ausgewertet.
    ///
    /// Die Regeln stehen in <see cref="FernkampfRules"/>, das Format des Befehls in
    /// <see cref="Beschussbefehl"/>.
    /// </summary>
    public class SchootCommand : BaseCommand, IPhoenixCommand {

        public Fernkampfwaffe Waffe { get; set; } = Fernkampfwaffe.Leicht;
        public int Anzahl { get; set; }
        public KleinfeldPosition? TargetLocation { get; set; } = null;
        public KleinfeldPosition? SourceLocation { get; set; } = null;
        public int? UnitId { get; set; }

        /// <summary>
        /// Die Figur, die schiesst. Sie ergibt sich aus der Auswahl oder aus Standort und Nummer.
        /// </summary>
        public TruppenSpielfigur? Figur { get; set; } = null;

        /// <summary>
        /// Der Inhalt von Befehl_ang vor der Ausführung, damit das Zurücknehmen den Stand davor
        /// wiederherstellt statt nur den eigenen Befehl herauszuschneiden.
        /// </summary>
        private string? _befehlVorher = null;

        public SchootCommand(string commandString) : base(commandString) {
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable is TruppenSpielfigur truppe && (truppe.LKP > 0 || truppe.SKP > 0);
        }

        public override string ToString() {
            string waffe = Waffe == Fernkampfwaffe.Schwer ? "schweren" : "leichten";
            string von = SourceLocation == null ? string.Empty : $" von {SourceLocation.CreateBezeichner()}";
            string nummer = UnitId == null ? string.Empty : $" {UnitId}";
            return $"Beschieße {TargetLocation?.CreateBezeichner()} mit {Anzahl} {waffe} Fernkampfwaffen{nummer}{von}";
        }

        /// <summary>
        /// Sucht die Figur, die den Befehl ausführen soll
        /// </summary>
        private TruppenSpielfigur? BestimmeFigur() {
            if (Figur != null)
                return Figur;
            if (_Selectable is TruppenSpielfigur ausgewählt)
                return ausgewählt;
            if (SourceLocation == null)
                return null;
            var armee = SpielfigurenView.GetSpielfiguren(SourceLocation);
            if (armee == null)
                return null;
            return armee.OfType<TruppenSpielfigur>()
                        .FirstOrDefault(truppe => UnitId == null || truppe.Nummer == UnitId);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var figur = BestimmeFigur();
            if (figur == null)
                return new CommandResultError("Die schiessende Einheit wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich keine Figur bestimmen. Bitte die Einheit auswählen oder mit Nummer angeben.", this);

            var geprüft = FernkampfRules.PrüfeBeschuss(figur, Waffe, Anzahl, TargetLocation, out _);
            if (geprüft.HasErrors)
                return new CommandResultError(geprüft, this);
            return new CommandResultSuccess(geprüft, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var figur = BestimmeFigur();
            if (figur == null)
                return new CommandResultError("Die schiessende Einheit wurde nicht gefunden", CommandString, this);

            Figur = figur;
            _befehlVorher = figur.Befehl_ang;

            var befehle = Beschussbefehl.LiesAlle(figur.Befehl_ang);
            befehle.Add(new Beschussbefehl {
                Anzahl = Anzahl,
                Waffe = Waffe,
                Ziel = TargetLocation ?? new KleinfeldPosition(),
                Entfernung = FernkampfRules.GetEntfernung(
                    new KleinfeldPosition(figur.gf_nach > 0 ? figur.gf_nach : figur.gf_von,
                                          figur.gf_nach > 0 ? figur.kf_nach : figur.kf_von),
                    TargetLocation, FernkampfRules.GetReichweite(Waffe)),
            });
            figur.Befehl_ang = Beschussbefehl.Schreibe(befehle);

            if (figur is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneZielfeldNeu();
            IsExecuted = true;
            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Figur == null)
                return new CommandResultError("Der Beschuss lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist keine Figur vermerkt.", this);

            Figur.Befehl_ang = _befehlVorher;
            if (Figur is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneZielfeldNeu();
            IsExecuted = false;
            return new CommandResultSuccess("Der Beschuss wurde zurückgenommen",
                $"{Figur.Bezeichner} beschiesst {TargetLocation?.CreateBezeichner()} nicht mehr.", this);
        }

        /// <summary>
        /// Das Zielfeld wird als beschossen markiert und muss deshalb neu gezeichnet werden
        /// </summary>
        private void ZeichneZielfeldNeu() {
            if (TargetLocation == null)
                return;
            var kleinfeld = KleinfeldView.GetKleinfeld(TargetLocation);
            if (kleinfeld != null)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
        }
    }

    public class SchootCommandParser : SimpleParser {

        /// <summary>
        /// "Beschieße 603/78 mit 4 leichten Fernkampfwaffen von 603/77"
        ///
        /// Anzahl und Waffenart sind Pflicht, die Nummer der Einheit und das Feld, von dem aus
        /// geschossen wird, sind freiwillig - ist eine Figur ausgewählt, ergeben sie sich daraus.
        /// </summary>
        private static readonly Regex SchootRegex = new(
              @"^Beschieße\s+(?<targetLoc>\d+/\d+)\s+mit\s+(?<anzahl>\d+)\s+(?<waffe>[A-Za-zÄÖÜäöüß]+)"
            + @"(\s+Fernkampfwaffen?| Katapulten?| Kriegsschiffen?)?"
            + @"(\s+Nr\.?\s*(?<unitId>\d+))?"
            + @"(\s+von\s+(?<sourceLoc>\d+/\d+))?$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = SchootRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                var waffe = ParseWaffe(match.Groups["waffe"].Value);
                if (waffe == null)
                    return Fail(out command);

                command = new SchootCommand(commandString) {
                    Waffe = waffe.Value,
                    Anzahl = ParseInt(match.Groups["anzahl"].Value),
                    UnitId = match.Groups["unitId"].Success ? ParseInt(match.Groups["unitId"].Value) : null,
                    TargetLocation = ParseLocation(match.Groups["targetLoc"].Value),
                    SourceLocation = match.Groups["sourceLoc"].Success ? ParseLocation(match.Groups["sourceLoc"].Value) : null,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SchootCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Erkennt die Bauart der Fernkampfwaffe. Im Befehl steht die Bauart, nicht die Gattung -
        /// ob daraus ein Katapult oder ein Kriegsschiff wird, sagt die Figur.
        /// </summary>
        public static Fernkampfwaffe? ParseWaffe(string eingabe) {
            return eingabe.ToLower() switch {
                "leicht" or "leichte" or "leichten" or "leichtem" or "leichtes" => Fernkampfwaffe.Leicht,
                "lk" or "lkp" or "lks" => Fernkampfwaffe.Leicht,
                "schwer" or "schwere" or "schweren" or "schwerem" or "schweres" => Fernkampfwaffe.Schwer,
                "sk" or "skp" or "sks" => Fernkampfwaffe.Schwer,
                _ => null,
            };
        }
    }
}
