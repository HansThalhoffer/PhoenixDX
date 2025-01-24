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
    public class DoNothingCommand : SimpleCommand, ICommand {

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

    }

    public class DoNothingCommandParser : SimpleParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^(?<Figur>\w+)\s+(?<UnitId>\d+)\s+auf\s+(?<Location>[^\s]+)\s+tut\s+nichts\s+diese\s+Runde\s$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = UpgradeRegex.Match(commandString);
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
