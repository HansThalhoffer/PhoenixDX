using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {
    /// <summary>
    /// Gebe Yaromo Küstenrecht
    /// Entziehe Helborn Wegerecht
    /// </summary>
    public class DiplomacyCommand : SimpleCommand, ICommand, IEquatable<DiplomacyCommand> {
        public enum BewegungsRecht { None, Küstenrecht, Wegerecht }

        public Nation? ReferenzNation { get; set; }
        public Nation? Nation { get; set; }
        public BewegungsRecht Recht { get; set; } = BewegungsRecht.None;
        public bool? RemoveRecht { get; set; } = null;

        public override string ToString() {

            string reich = this.Nation != null && Nation.DBname != null? Nation.DBname : string.Empty;
            return RemoveRecht != null && RemoveRecht == true? $"Entziehe {reich} {Recht}": $"Gebe {reich} {Recht}";
        }

        public DiplomacyCommand(string commandString) : base(commandString) {
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

        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            if (SharedData.Diplomatiechange != null) {
                var item = SharedData.Diplomatiechange.Where(d => d.ReferenzNation == ReferenzNation && d.Nation == Nation).First();
                if (Recht == BewegungsRecht.Wegerecht) {
                    item.Wegerecht = RemoveRecht!=null && RemoveRecht == true ? 0 : 1;
                }
                else {
                    item.Kuestenrecht = RemoveRecht != null && RemoveRecht == true ? 0 : 1;
                }
                Update(item, ViewEventArgs.ViewEventType.UpdateDiplomatie);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);
            }
            return new CommandResultError($"Das {this.GetType()} konnte nicht ausgeführt werden", $"Keine Ahnung warum:\r\n {this.CommandString}", this);


        }

        public override CommandResult UndoCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
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

        public bool Equals(DiplomacyCommand? other) {
            if (other is null)
                return false;

            return EqualityComparer<Nation?>.Default.Equals(ReferenzNation, other.ReferenzNation)
                && EqualityComparer<Nation?>.Default.Equals(Nation, other.Nation)
                && Recht == other.Recht
                && RemoveRecht == other.RemoveRecht;
        }

        public override bool Equals(object? obj) {
            return obj is DiplomacyCommand other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ReferenzNation, Nation, Recht, RemoveRecht);
        }
    }


    public class DiplomacyCommandParser : SimpleParser {
        private static readonly Regex DiplomacyRegex = new Regex(
              // Explanation:
              // ^(?<verb>Gebe|Entziehe)\s+(?<nation>[^\s]+)\s+(?<recht>Küstenrecht|Wegerecht)$
              //
              //    ^                         : start of string
              //    (?<verb>Gebe|Entziehe)   : capture "Gebe" or "Entziehe" in group "verb"
              //    \s+                       : one or more spaces
              //    (?<nation>[^\s]+)        : capture any non-whitespace as nation name
              //    \s+                       : spaces
              //    (?<recht>Küstenrecht|Wegerecht) : capture the movement right
              //    $                         : end of string
              @"^(?<verb>Gebe|Entziehe)\s+(?<nation>[^\s]+)\s+(?<recht>Küstenrecht|Wegerecht)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled
          );

        public override bool ParseCommand(string commandStringUE, out ICommand? command) {
            string commandString = commandStringUE.Replace("Kuestenrecht", "Küstenrecht");
            var match = DiplomacyRegex.Match(commandString);
            if (!match.Success) {
                return Fail(out command);
            }
            try {
                DiplomacyCommand.BewegungsRecht recht = DiplomacyCommand.BewegungsRecht.None;
                switch (match.Groups["recht"].Value.ToLower()) {
                    case "küstenrecht":
                        recht = DiplomacyCommand.BewegungsRecht.Küstenrecht;
                        break;
                    case "wegerecht":
                        recht = DiplomacyCommand.BewegungsRecht.Wegerecht;
                        break;
                    default:
                        recht = DiplomacyCommand.BewegungsRecht.None; // or handle error
                        break;
                }


                command = new DiplomacyCommand(commandString) {
                    RemoveRecht = match.Groups["verb"].Value.Equals("Entziehe", StringComparison.OrdinalIgnoreCase),
                    Recht = recht,
                    Nation = NationenView.GetNationFromString(match.Groups["nation"].Value),
                    ReferenzNation = ProgramView.SelectedNation,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MoveCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
