using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    public class RepairCommand : SimpleCommand, ICommand {

        public Kosten? Kosten = null;
        public KleinfeldPosition? Location { get; set; }
        public int Baupunkte { get; set; } = 0;

        public override string ToString() {            
            string result = $"Repariere {Baupunkte} an dem Bauwerk auf {Location}";
            //if (Kosten != null) result = $"{result} für {Kosten.GS}";
            return result ; 
        }

        public RepairCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            throw new NotImplementedException();
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }
        
    }

    public class RepairCommandParser : SimpleParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^Repariere\s+(?<Baupunkte>\d+)\s+Baupunktet\s+an\s+dem\s+Bauwerk\s+auf\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new RepairCommand(commandString) {
                    Location = ParseLocation(match.Groups["loc"].Value),
                    Baupunkte = ParseInt(match.Groups["Baupunkte"].Value),
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
