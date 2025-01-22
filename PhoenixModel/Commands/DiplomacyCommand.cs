using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    public class DiplomacyCommand : SimpleCommand, ICommand {
       public enum BewegungsRecht { None, Küstenrecht, Wegerecht}
        
        public Nation? ReferenzNation { get; set; }
        public Nation? Nation { get; set; }
        public BewegungsRecht Recht { get; set; } = BewegungsRecht.None;
        public bool? RemoveRecht { get; set; } = null;


        public DiplomacyCommand(string commandString) : base(commandString) {
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


    public class DiplomacyCommandParser : SimpleParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^Verstärke\s+Rüstort\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (!match.Success) {
                return Fail(out command);
            }

            int? nummer = null;
            try { nummer = int.Parse(match.Groups["unitId"].Value); } catch { };
            command = new UpgradeCommand(commandString, ParseLocation(match.Groups["loc"].Value)) {
            };
            return true;
        }
    }
}
