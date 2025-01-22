using PhoenixModel.Commands.Parser;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    public class RepairCommand : SimpleCommand, ICommand {
        public RepairCommand(string commandString) : base(commandString) {
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
    }

    public class RepairCommandParser : SimpleParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^Verstärke\s+Rüstort\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new UpgradeCommand(commandString, ParseLocation(match.Groups["loc"].Value)) {
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des RepairCommands gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
