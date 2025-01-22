using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using PhoenixModel.Program;
using PhoenixModel.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    public class DiplomacyCommand : SimpleCommand, ICommand, IEquatable<DiplomacyCommand> {
        public enum BewegungsRecht { None, Küstenrecht, Wegerecht }

        public Nation? ReferenzNation { get; set; }
        public Nation? Nation { get; set; }
        public BewegungsRecht Recht { get; set; } = BewegungsRecht.None;
        public bool? RemoveRecht { get; set; } = null;


        public DiplomacyCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        public override CommandResult ExecuteCommand() {
            throw new NotImplementedException();
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
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

        public override bool ParseCommand(string commandString, out ICommand? command) {
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
