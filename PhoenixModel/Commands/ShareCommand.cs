using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// Verschiebe 100 Pferde von Krieger 101 zu Krieger 103 auf 701/11
    /// Verschiebe 4 Leichte Katapulte von Krieger 101 zu Krieger 103 auf 701/11
    /// Verschiebe 2 Heerführer von Krieger 101 zu Krieger 103 auf 701/11
    /// Verschiebe 2000 Gold von Krieger 101 zu Krieger 103 auf 701/11
    /// Verschiebe 2000 Gold von Krieger 101 in Rüstort 708/33
    /// Verschiebe 2000 Kampfeinnahmen von Krieger 101 in Rüstort 708/33
    /// </summary>
    public class ShareCommand {
    }

    public class ShareCommandParser : SimpleParser {
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

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = SchootRegex.Match(commandString);
            if (!match.Success)
                return Fail(out command);

            try {
                /* command = new ShareCommand(commandString) {
                    With = ParseUnitType(match.Groups["unitType"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    TargetLocation = ParseLocation(match.Groups["targetLoc"].Value),
                    SourceLocation = ParseLocation(match.Groups["sourceLoc"].Value),
                    
                };
                */
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SchootCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            command = null;
            return true;
        }
    }
}
