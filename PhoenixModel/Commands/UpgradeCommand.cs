using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// baut einen Rüstort aus
    ///    - "Verstärke Rüstort 202/33"
    /// </summary>
    public class UpgradeCommand : DefaultCommand, ICommand {
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;

        public UpgradeCommand(string commandString, KleinfeldPosition? pos) : base(commandString) {
            Location = pos;
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult ExecuteCommand() {
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

    public class UpgradeCommandParser : ICommandParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^Verstärke\s+Rüstort\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }
        public bool ParseCommand(string commandString, out ICommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (!match.Success) {
                return Fail(out command);
            }

            int? nummer = null;
            try { nummer = int.Parse(match.Groups["unitId"].Value); } catch { };
            command = new UpgradeCommand(commandString, CommandParser.ParseLocation(match.Groups["loc"].Value)) {
            };
            return true;
        }
    }
}
