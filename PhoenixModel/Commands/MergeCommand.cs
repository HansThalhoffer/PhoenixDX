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
    /// Vereinige Reiter 221 mit 3 Heerführern und 244 mit 2 Heerführern zu 221 mit 5 Heerführern
    /// </summary>
    public class MergeCommand : SimpleCommand, ICommand {
        public FigurType Figur { get; set; }
        public int TargetUnitId { get; set; }
        public int TargetHeerführerCount { get; set; }
        public int SourceUnitId_0 { get; set; }
        public int SourceHeerführerCount_0 { get; set; }
        public int SourceUnitId_1 { get; set; }
        public int SourceHeerführerCount_1 { get; set; }

        public override string ToString() {
            return $"Vereinige {Figur} {SourceUnitId_0} mit {SourceHeerführerCount_0} und {SourceUnitId_0} mit {SourceHeerführerCount_0} zu {TargetUnitId} mit {TargetHeerführerCount}";
        }

        public MergeCommand(string commandString) : base(commandString) {
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

    public class MergeCommandParser : SimpleParser {
        private static readonly Regex MergeCommandRegex = new Regex(
             @"^Vereinige\s+(?<type>\w+)\s+(?<sourceID_0>\d+)\s+mit\s+(?<sourceCount_0>\d+)\s+Heerführern\s+mit\s+(?<sourceID_1>\d+)\s+und\s+(?<sourceCount_1>\d+)\s+Heerführern\s+zu\s+(?<targetID>\d+)\s+mit\s+(?<targetCount>\d+)\s+Heerführern$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
         );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = MergeCommandRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                int? nummer = null;
                try { nummer = int.Parse(match.Groups["unitId"].Value); } catch { };
                command = new MergeCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["type"].Value),
                    TargetUnitId = ParseInt(match.Groups["targetID"].Value),
                    TargetHeerführerCount = ParseInt(match.Groups["targetCount"].Value),
                    SourceUnitId_0 = ParseInt(match.Groups["sourceID_0"].Value),
                    SourceHeerführerCount_0 = ParseInt(match.Groups["sourceCount_0"].Value),
                    SourceUnitId_1 = ParseInt(match.Groups["sourceID_1"].Value),
                    SourceHeerführerCount_1 = ParseInt(match.Groups["sourceCount_1"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MergeCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
