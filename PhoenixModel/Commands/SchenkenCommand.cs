using PhoenixModel.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// Schenke Yaromo 5000 
    /// Schenke Yaromo 5.000 GS
    /// Schenke Yaromo 5.000 Gold
    /// </summary>
    public class SchenkenCommand {
    }

    
    public class SchenkenCommandParser : SimpleParser {
        private static readonly Regex GiveRegex = new Regex(
             // Explanation:
             // ^Schenke\s+(?<nation>[^\s]+)\s+(?<amount>[\d\.]+)
             //   then optionally:
             //      \s+(?<currency>GS|Gold)
             //   end of string
             @"^Schenke\s+(?<nation>[^\s]+)\s+(?<amount>[\d\.]+)(?:\s+(?<currency>GS|Gold))?$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
         );

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = GiveRegex.Match(commandString);
            if (!match.Success)
                return Fail(out command);

            try {
                /* command = new SchenkenCommand(commandString) {
                    Nation = NationenView.GetNationFromString(match.Groups["nation"].Value),
                    Betrag = ParseInt(match.Groups["amount"].Value),
                };
                */
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SchenkenCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            command = null;
            return true;
        }
    }
}
