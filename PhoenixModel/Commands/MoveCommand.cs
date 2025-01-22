using PhoenixModel.Commands.Parser;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Bewege Reiter 220 [von 503/22] nach 504/07 [via 503/21, 503/16, 503/4]
    /// </summary>
    public class MoveCommand : SimpleCommand {

        public FigurType Figur { get; set; }
        public int UnitId { get; set; }
        public KleinfeldPosition? FromLocation { get; set; }
        public KleinfeldPosition? ToLocation { get; set; }
        public List<KleinfeldPosition>? ViaLocations { get; set; } = null;

        public MoveCommand(string commandString) : base(commandString) {
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

    public class MoveCommandParser : SimpleParser {
        // Regex pattern explanation:
        //
        // ^Bewege\s+               : must start with "Bewege "
        // (?<type>\w+)\s+          : capture the unit type (e.g. "Reiter", "Krieger", etc.)
        // (?<id>\d+)               : capture the numeric unit ID
        // (?:\s+von\s+(?<from>[^\s]+))? : optionally capture something after "von " as "from" (e.g. "503/22")
        // \s+nach\s+(?<to>[^\s]+)  : capture the "nach" location
        // (?:\s+via\s+(?<via>.*))?$: optionally capture everything after "via" as a single string
        //
        private static readonly Regex MoveCommandRegex = new Regex(
            @"^Bewege\s+(?<type>\w+)\s+(?<unitId>\d+)(?:\s+von\s+(?<from>[^\s]+))?\s+nach\s+(?<to>[^\s]+)(?:\s+via\s+(?<via>.*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = MoveCommandRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
           
            try {
                List<KleinfeldPosition> via = [];
                if (match.Groups["via"].Success) {
                    string viaPart = match.Groups["via"].Value;
                    // split by comma
                    var viaLocations = viaPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in viaLocations) {
                        var loc = ParseLocation(v.Trim());
                        if (loc != null) via.Add(loc);
                    }
                }

                command = new MoveCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["type"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    FromLocation = ParseLocation(match.Groups["from"].Value),
                    ToLocation = ParseLocation(match.Groups["to"].Value),
                    ViaLocations = via,
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
