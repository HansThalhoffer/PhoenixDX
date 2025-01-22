using PhoenixModel.Commands.Parser;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// Teile Reiter 220 mit 10 Heerführern auf in 220 mit 7 Heerführern und 221 mit 3 Heerführern
    /// </summary>
    public class SplitCommand : SimpleCommand, ICommand {
        public FigurType Figur { get; set; }
        public int OriginalUnitId { get; set; } = 0;
        public int OriginalHeerführerCount { get; set; } = 0;
        public int NewUnitId_0 { get; set; } = 0;
        public int NewHeerführerCount_0 { get; set; } = 0;
        public int NewUnitId_1 { get; set; } = 0;
        public int NewHeerführerCount_1 { get; set; } = 0;

        public SplitCommand(string commandString) : base(commandString) {
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

    public class SplitCommandParser : SimpleParser {
        private static readonly Regex SplitCommandRegex = new Regex(
             @"^Teile\s+(?<type>\w+)\s+(?<origId>\d+)\s+mit\s+(?<origCount>\d+)\s+Heerführern\s+auf\s+in\s+(?<newId1>\d+)\s+mit\s+(?<newCount1>\d+)\s+Heerführern\s+und\s+(?<newId2>\d+)\s+mit\s+(?<newCount2>\d+)\s+Heerführern$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
         );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = SplitCommandRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new SplitCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["type"].Value),
                    OriginalUnitId = ParseInt(match.Groups["origId"].Value),
                    OriginalHeerführerCount = ParseInt(match.Groups["origCount"].Value),
                    NewUnitId_0 = ParseInt(match.Groups["newId1"].Value),
                    NewHeerführerCount_0 = ParseInt(match.Groups["newCount1"].Value),
                    NewUnitId_1 = ParseInt(match.Groups["newId2"].Value),
                    NewHeerführerCount_1 = ParseInt(match.Groups["newCount2"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SplitCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
