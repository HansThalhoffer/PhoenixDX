using PhoenixModel.Commands.Parser;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;
using static PhoenixModel.Commands.SchootCommand;

namespace PhoenixModel.Commands {

    public class SchootCommand : SimpleCommand, ICommand {
        
        public FigurType With { get; set; } 
        public KleinfeldPosition? TargetLocation { get; set; } = null;
        public KleinfeldPosition? SourceLocation { get; set; } = null;
        public int? UnitId { get; set; }


        public SchootCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            string result = $"Beschieße {TargetLocation} mit {With} {UnitId} von {SourceLocation}";
            return result;
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }
    }

    public class SchootCommandParser : SimpleParser {
        private static readonly Regex SchootRegex = new Regex(
              // Explanation:
              // ^Beschieße\s+(?<targetLoc>[^\s]+)\s+      : "Beschieße <LOC>"
              // mit\s+                                   : "mit"
              // (?<weaponName>[^\d]+)                    : weapon name (consume non-digit chars, e.g. "Schweren Katapulten ")
              // (\s+(?<weaponId>\d+))?                   : optional space + numeric ID group (e.g. "105")
              // \s+von\s+(?<sourceLoc>[^\s]+)$           : "von <LOC>" at the end
              @"^Beschieße\s+(?<targetLoc>[^\s]+)\s+mit\s+(?<equipment>[^\d]+)(\s+(?<unitId>\d+))?\s+von\s+(?<sourceLoc>[^\s]+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = SchootRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new SchootCommand(commandString) {
                    With = ParseUnitType(match.Groups["unitType"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    TargetLocation = ParseLocation(match.Groups["targetLoc"].Value),
                    SourceLocation = ParseLocation(match.Groups["sourceLoc"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SchootCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
