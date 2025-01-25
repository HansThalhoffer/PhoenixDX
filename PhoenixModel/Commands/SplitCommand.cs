using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
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
    /// Spalte von Reiter 220 ab 400 Reiter mit 4 Heerführern
    /// </summary>
    public class SplitCommand : SimpleCommand, ICommand {
        public FigurType Figur { get; set; }
        public int OriginalUnitId { get; set; } = 0;
        public int SeparatedUnitId { get; set; } = 0;
        public int SeparatedCount{ get; set; } = 0;
        public int SeparatedHeerführerCount { get; set; } = 0;

        public override string ToString() {
            string result = $"Spalte von {Figur} {OriginalUnitId} ab {SeparatedCount} mit {SeparatedHeerführerCount} Heerführern als {SeparatedUnitId}";
            return result;
        }

        public SplitCommand(string commandString) : base(commandString) {
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

    public class SplitCommandParser : SimpleParser {
        private static readonly Regex SplitCommandRegex = new Regex(
             @"^Spalte\s+von\s+(?<Figur>\w+)\s+(?<OriginalUnitId>\d+)\s+ab\s+(?<SeparatedCount>\d+)\s+mit\s+auf\s+in\s+(?<SeparatedCount>\d+)\s+mit\s+(?<SeparatedHeerführerCount>\d+)\s+Heerführern\s+als\s+(?<SeparatedUnitId>\d+)$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
         );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = SplitCommandRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new SplitCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["Figur"].Value),
                    OriginalUnitId = ParseInt(match.Groups["OriginalUnitId"].Value),
                    SeparatedUnitId = ParseInt(match.Groups["SeparatedUnitId"].Value),
                    SeparatedCount = ParseInt(match.Groups["SeparatedCount"].Value),
                    SeparatedHeerführerCount = ParseInt(match.Groups["SeparatedHeerführerCount"].Value),                 
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
