using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {
    /// <summary>
    /// Repräsentiert einen diplomatischen Befehl zur Vergabe oder Entziehung von Bewegungsrechten zwischen Nationen.
    /// </summary>
    public class DiplomacyCommand : BaseCommand, IPhoenixCommand, IEquatable<DiplomacyCommand> {
        /// <summary>
        /// Definiert die möglichen Bewegungsrechte einer Nation.
        /// </summary>
        public enum BewegungsRecht {
            /// <summary>
            /// Kein Bewegungsrecht.
            /// </summary>
            None,

            /// <summary>
            /// Erlaubt einer Nation, Küstenregionen zu betreten.
            /// </summary>
            Küstenrecht,

            /// <summary>
            /// Erlaubt einer Nation, über das Landgebiet einer anderen Nation zu reisen.
            /// </summary>
            Wegerecht
        }

        /// <summary>
        /// Die Referenznation, die das Bewegungsrecht vergibt oder entzieht.
        /// </summary>
        public Nation? ReferenzNation { get; set; }

        /// <summary>
        /// Die betroffene Nation, die das Bewegungsrecht erhält oder verliert.
        /// </summary>
        public Nation? Nation { get; set; }

        /// <summary>
        /// Das Bewegungsrecht, das gewährt oder entzogen wird.
        /// </summary>
        public BewegungsRecht Recht { get; set; } = BewegungsRecht.None;

        /// <summary>
        /// Gibt an, ob das Bewegungsrecht entzogen werden soll (true) oder gewährt wird (false/null).
        /// </summary>
        public bool? RemoveRecht { get; set; } = null;

        /// <summary>
        /// Erstellt eine textuelle Darstellung des Befehls in der Form:
        /// "Gebe [Nation] [Recht]" oder "Entziehe [Nation] [Recht]".
        /// </summary>
        /// <returns>Eine Zeichenfolge, die den Diplomatiebefehl beschreibt.</returns>
        public override string ToString() {
            string reich = this.Nation != null && Nation.DBname != null ? Nation.DBname : string.Empty;
            return RemoveRecht != null && RemoveRecht == true
                ? $"Entziehe {reich} {Recht}"
                : $"Gebe {reich} {Recht}";
        }

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="DiplomacyCommand"/>-Klasse mit einem Befehlsstring.
        /// </summary>
        /// <param name="commandString">Der Befehlsstring, der den Diplomatiebefehl beschreibt.</param>
        public DiplomacyCommand(string commandString) : base(commandString) {
        }


        /// <summary>
        /// Wenn das Kommando das Selectable betrifft, gibt es true zurück
        /// Die Basisimplementierung schaut nach einem direkten Vergleich
        /// der funktioniert aber nur, wenn das Selectable nach Programmstart verändert wurde
        /// daher ist ein Vergleich der Werte immer notwendig
        /// </summary>
        /// <param name="selectable"></param>
        /// <returns></returns>
        public override bool HasEffectOn(ISelectable selectable) {

            return (base.HasEffectOn(selectable) == true ||
                (selectable != null  && selectable is Diplomatiechange diplomatiechange &&
                diplomatiechange.ReferenzNation == this.ReferenzNation && diplomatiechange.Nation == this.Nation));
        }

        private bool IsLastCommand() {
            if (SharedData.Diplomatiechange != null) {
                var item = SharedData.Diplomatiechange.Where(d => d.ReferenzNation == ReferenzNation && d.Nation == Nation).First();
                if (item != null) {
                    var diplomacyCommands = SharedData.Commands.GetCommands(item);
                    if (diplomacyCommands != null && diplomacyCommands.Last() == this)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// nur der jeweils letzte Diplomatie Befehl kann wiederrufen werden
        /// </summary>
        public override bool CanUndo {
            get{
                CommandResult result = CheckPreconditions();
                if (result.HasErrors)
                    return false;
            
                return IsLastCommand();
            }
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (ReferenzNation == null)
                return new CommandResultError("Es wurde keine Referenznation angegeben", $"Normalerweise ist die Referenznation das aktuell angemeldete oder von der Spielleitung festgelegte Reich.\r\n {this.CommandString}", this);
            if (Nation == null)
                return new CommandResultError("Es wurde keine Nation als Ziel angegeben", $"In dem Befehl konnte das betreffende Element nicht gefunden werden \r\n {this.CommandString}", this);
            if (Recht == BewegungsRecht.None)
                return new CommandResultError("Es wurde kein Recht angegeben", $"In dem Befehl konnte das betreffende Element nicht gefunden werden \r\n {this.CommandString}", this);
            if (RemoveRecht == null)
                return new CommandResultError("Es wurde kein Befehl (Gebe/Entziehe) angegeben", $"In dem Befehl konnte das betreffende Element nicht gefunden werden \r\n {this.CommandString}", this);
            if (SharedData.Diplomatiechange == null)
                return new CommandResultError("Die Tabelle Diplomatiechange wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die betreffende Tabelle aus der Datenbank nicht geladen wurde \r\n {this.CommandString}", this);
            var item = SharedData.Diplomatiechange.Where(d => d.ReferenzNation == ReferenzNation && d.Nation == Nation);
            if (item == null || item.Any() == false)
                return new CommandResultError("Der Eintrag in der Tabelle Diplomatiechange wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die betreffende Tabelle aus der Datenbank nicht den entsprechenden Eintrag besitzt\r\n {this.CommandString}", this);
            return new CommandResultSuccess($"Das {this.GetType()} kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}", this);
        }
        /// <summary>
        /// Führt den Befehl aus.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>-Objekt mit dem Ausführungsergebnis.</returns>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            if (SharedData.Diplomatiechange != null) {
                var item = SharedData.Diplomatiechange.Where(d => d.ReferenzNation == ReferenzNation && d.Nation == Nation).First();
                if (Recht == BewegungsRecht.Wegerecht) {
                    item.Wegerecht = RemoveRecht != null && RemoveRecht == true ? 0 : 1;
                }
                else {
                    item.Kuestenrecht = RemoveRecht != null && RemoveRecht == true ? 0 : 1;
                }
                Update(item, ViewEventArgs.ViewEventType.UpdateDiplomatie);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);
            }
            return new CommandResultError($"Das {this.GetType()} konnte nicht ausgeführt werden", $"Keine Ahnung warum:\r\n {this.CommandString}", this);


        }

        /// <summary>
        /// Macht die Ausführung des Befehls rückgängig.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>-Objekt mit dem Ergebnis des Rückgängig-Machens.</returns>
        public override CommandResult UndoCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            if (IsLastCommand() == false)
                return new CommandResultError($"Das Undo von {this.GetType()} konnte nicht ausgeführt werden", $"Das Kommando muss der letzte Diplomatie Befehl sein, damit er fehlerfrei wiederrufen werden kann:\r\n {this.CommandString}", this);


            if (SharedData.Diplomatiechange != null) {
                var item = SharedData.Diplomatiechange.Where(d => d.ReferenzNation == ReferenzNation && d.Nation == Nation).First();
                if (Recht == BewegungsRecht.Wegerecht) {
                    item.Wegerecht = RemoveRecht != null && RemoveRecht == true ? 1 : 0;
                }
                else {
                    item.Kuestenrecht = RemoveRecht != null && RemoveRecht == true ? 1 : 0;
                }
                Update(item, ViewEventArgs.ViewEventType.UpdateDiplomatie);
                return new CommandResultSuccess($"Undo von {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);
            }
            return new CommandResultError($"Undo {this.GetType()} konnte nicht ausgeführt werden", $"Keine Ahnung warum:\r\n {this.CommandString}", this);


        }
        /// <summary>
        /// Überprüft, ob das aktuelle DiplomacyCommand-Objekt einem anderen Objekt entspricht.
        /// </summary>
        /// <param name="other">Das zu vergleichende DiplomacyCommand-Objekt.</param>
        /// <returns>Gibt <c>true</c> zurück, wenn beide Objekte gleich sind, andernfalls <c>false</c>.</returns>
        public bool Equals(DiplomacyCommand? other) {
            if (other is null)
                return false;

            return EqualityComparer<Nation?>.Default.Equals(ReferenzNation, other.ReferenzNation)
                && EqualityComparer<Nation?>.Default.Equals(Nation, other.Nation)
                && Recht == other.Recht
                && RemoveRecht == other.RemoveRecht;
        }
        /// <summary>
        /// Überprüft, ob das aktuelle Objekt einem anderen Objekt entspricht.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Objekt.</param>
        /// <returns>Gibt <c>true</c> zurück, wenn das Objekt ein DiplomacyCommand ist und gleich ist, andernfalls <c>false</c>.</returns>
        public override bool Equals(object? obj) {
            return obj is DiplomacyCommand other && Equals(other);
        }
        /// <summary>
        /// Berechnet den Hashcode für das aktuelle DiplomacyCommand-Objekt.
        /// </summary>
        /// <returns>Der berechnete Hashcode.</returns>
        public override int GetHashCode() {
            return HashCode.Combine(ReferenzNation, Nation, Recht, RemoveRecht);
        }
    }

    /// <summary>
    /// Parser für Diplomatiebefehle. Analysiert Befehle zum Gewähren oder Entziehen von Rechten.
    /// </summary>
    public class DiplomacyCommandParser : SimpleParser {
        /// <summary>
        /// Regulärer Ausdruck zur Analyse von Diplomatiebefehlen.
        /// Unterstützte Befehle: "Gebe [Nation] [Recht]" oder "Entziehe [Nation] [Recht]".
        /// 
        /// Erklärung des Regex:
        /// ^(?<verb>Gebe|Entziehe)\s+(?<nation>[^\s]+)\s+(?<recht>Küstenrecht|Wegerecht)$
        ///
        /// - `^`                         : Beginn der Zeichenkette
        /// - `(?<verb>Gebe|Entziehe)`     : Speichert "Gebe" oder "Entziehe" in der Gruppe "verb"
        /// - `\s+`                        : Ein oder mehrere Leerzeichen
        /// - `(?<nation>[^\s]+)`         : Erfasst einen beliebigen nicht-leeren Wert als Nation
        /// - `\s+`                        : Leerzeichen
        /// - `(?<recht>Küstenrecht|Wegerecht)` : Erfasst das Recht (Küstenrecht oder Wegerecht)
        /// - `$`                         : Ende der Zeichenkette
        /// </summary>
        private static readonly Regex DiplomacyRegex = new Regex(
            @"^(?<verb>Gebe|Entziehe)\s+(?<nation>[^\s]+)\s+(?<recht>Küstenrecht|Wegerecht)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Analysiert einen gegebenen Diplomatiebefehl und erstellt das entsprechende <see cref="IPhoenixCommand"/>-Objekt.
        /// </summary>
        /// <param name="commandStringUE">Der zu analysierende Befehl als Zeichenkette.</param>
        /// <param name="command">Ausgabeparameter: Der erstellte Diplomatiebefehl.</param>
        /// <returns>Gibt <c>true</c> zurück, wenn der Befehl erfolgreich analysiert wurde, andernfalls <c>false</c>.</returns>
        public override bool ParseCommand(string commandStringUE, out IPhoenixCommand? command) {
            // Ersetzt "Kuestenrecht" durch "Küstenrecht", um alternative Schreibweisen zu unterstützen.
            string commandString = commandStringUE.Replace("Kuestenrecht", "Küstenrecht");
            var match = DiplomacyRegex.Match(commandString);

            // Falls keine Übereinstimmung gefunden wurde, schlägt das Parsen fehl.
            if (!match.Success) {
                return Fail(out command);
            }

            try {
                // Standardwert für Bewegungsrecht setzen.
                DiplomacyCommand.BewegungsRecht recht = DiplomacyCommand.BewegungsRecht.None;

                // Bestimmt das erkannte Bewegungsrecht.
                switch (match.Groups["recht"].Value.ToLower()) {
                    case "küstenrecht":
                        recht = DiplomacyCommand.BewegungsRecht.Küstenrecht;
                        break;
                    case "wegerecht":
                        recht = DiplomacyCommand.BewegungsRecht.Wegerecht;
                        break;
                    default:
                        recht = DiplomacyCommand.BewegungsRecht.None; // Sollte eigentlich nicht auftreten
                        break;
                }

                // Erstellt das Diplomatiebefehl-Objekt.
                command = new DiplomacyCommand(commandString) {
                    // Setzt die Eigenschaften basierend auf den Analyseergebnissen.
                    RemoveRecht = match.Groups["verb"].Value.Equals("Entziehe", StringComparison.OrdinalIgnoreCase),
                    Recht = recht,
                    Nation = NationenView.GetNationFromString(match.Groups["nation"].Value),
                    ReferenzNation = ProgramView.SelectedNation,
                };
            }
            catch (Exception ex) {
                // Fehlerbehandlung: Loggt die Fehlermeldung.
                ProgramView.LogError("Beim Lesen des Diplomatiebefehls gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }

            return true;
        }
    }

}
