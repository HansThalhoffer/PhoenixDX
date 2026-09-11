using PhoenixModel.Commands.Parser;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Repariert einen beschädigten Rüstort.
    ///
    /// - "Repariere Rüstort 202/33"
    /// - "Repariere 150 Baupunkte an dem Bauwerk auf 202/33"
    ///
    /// Ohne Angabe wird repariert, was der Monat hergibt: höchstens 250 Baupunkte und nie mehr,
    /// als kaputt ist. Die Regeln stehen in <see cref="RuestortRules"/>.
    /// </summary>
    public class RepairCommand : RuestortBaubefehl {

        protected override Bauart Bauart => Bauart.Reparatur;

        public RepairCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            return Baupunkte > 0
                ? $"Repariere {Baupunkte} Baupunkte an dem Bauwerk auf {Location?.CreateBezeichner()}"
                : $"Repariere Rüstort {Location?.CreateBezeichner()}";
        }

        /// <summary>
        /// Ohne ausdrückliche Angabe wird repariert, soviel in einem Monat geht
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (Baupunkte <= 0)
                Baupunkte = Math.Min(RuestortRules.MaxBaupunkteProMonat, RuestortRules.GetSchaden(BestimmeKleinfeld()));
            return base.CheckPreconditions();
        }
    }

    public class RepairCommandParser : SimpleParser {

        private static readonly Regex RepairRegex = new(
              @"^Repariere\s+(?:(?<bp>\d+)\s+Baupunkte?\s+an\s+dem\s+Bauwerk\s+auf|Rüstort)\s+(?<loc>\d+/\d+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = RepairRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                command = new RepairCommand(commandString) {
                    Location = ParseLocation(match.Groups["loc"].Value),
                    Baupunkte = match.Groups["bp"].Success ? ParseInt(match.Groups["bp"].Value) : 0,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des RepairCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
