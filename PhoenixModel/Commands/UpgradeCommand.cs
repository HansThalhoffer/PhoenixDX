using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {
    /// <summary>
    /// baut einen Rüstort aus
    ///    - "Verstärke Rüstort 202/33"
    /// </summary>
    public class UpgradeCommand : BaseCommand, IPhoenixCommand {
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;

        public UpgradeCommand(string commandString) : base(commandString) {
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable != null && selectable is KleinFeld kleinFeld && kleinFeld.Gebäude != null;
        }


        public override string ToString() {
            string result = $"Verstärke Rüstort {Location}";
            return result;
        }
        
        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
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
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }
    }

    public class UpgradeCommandParser : SimpleParser {
        private static readonly Regex UpgradeRegex = new Regex(
            @"^Verstärke\s+Rüstort\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
            
            try {
                command = new UpgradeCommand(commandString) { 
                    Location = ParseLocation(match.Groups["loc"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des UpgradeCommands gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
