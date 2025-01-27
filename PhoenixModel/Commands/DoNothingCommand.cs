using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    public class DoNothingCommand : BaseCommand, IPhoenixCommand, IEquatable<DoNothingCommand> {

        public FigurType Figur = FigurType.None;
        public KleinfeldPosition? Location { get; set; }
        public int UnitId{ get; set; } = 0;

        public override string ToString() {
            string result = $"{Figur} {UnitId} auf {Location} tut nichts diese Runde";
            //if (Kosten != null) result = $"{result} für {Kosten.GS}";
            return result;
        }

        public DoNothingCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            return new CommandResultSuccess("Tut nichts", "Steht einfach nur rum", this);
        }

        public override CommandResult ExecuteCommand() {
            return CheckPreconditions();            
        }

        public override CommandResult UndoCommand() {
            return CheckPreconditions();
        }

        // Implementing IEquatable<DoNothingCommand>
        public bool Equals(DoNothingCommand? other) {
            if (other is null)
                return false;

            return Figur == other.Figur &&
                   UnitId == other.UnitId &&
                   Equals(Location, other.Location); // Properly comparing nullable Location
        }

        // Override Equals for object comparison
        public override bool Equals(object? obj) {
            if (obj is DoNothingCommand otherCommand)
                return Equals(otherCommand);

            return false;
        }

        // Override GetHashCode to include all properties
        public override int GetHashCode() {
            return HashCode.Combine(Figur, UnitId, Location);
        }

    }

    public class DoNothingCommandParser : SimpleParser {
       
        private static readonly Regex DoNothingRegex = new Regex(
      // Explanation:
      // ^(?<figur>\w+)\s+         : capture figur (a single "word"), e.g. "Kreatur"
      // (?<unitId>\d+)\s+         : capture numeric unit ID, e.g. 403
      // auf\s+                    : literal "auf "
      // (?<x>\d+)\/(?<y>\d+)      : capture location as x/y, e.g. 405/22
      // \s+tut\s+nichts\s+diese\s+Runde$
    @"^(?<Figur>\w+)\s+(?<UnitId>\d+)\s+auf\s+(?<Location>[^\s]+)\s+tut\s+nichts\s+diese\s+Runde$",
RegexOptions.IgnoreCase | RegexOptions.Compiled
);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = DoNothingRegex.Match(commandString);
            if (!match.Success)
                return Fail(out command);

            try {
                command = new DoNothingCommand(commandString) {
                    Location = ParseLocation(match.Groups["Location"].Value),
                    UnitId = ParseInt(match.Groups["UnitId"].Value),
                    Figur = ParseUnitType(match.Groups["Figur"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des DoNothingCommands gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
