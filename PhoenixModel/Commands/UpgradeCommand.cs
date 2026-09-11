using PhoenixModel.Commands.Parser;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Baut einen Rüstort auf die nächste Stufe aus.
    ///
    /// - "Verstärke Rüstort 202/33"
    /// - "Verstärke Rüstort 202/33 um 150 Baupunkte"
    ///
    /// Ohne Angabe wird gebaut, was bis zur nächsten Stufe fehlt - höchstens die 250 Baupunkte,
    /// die ein Monat hergibt. Die Regeln stehen in <see cref="RuestortRules"/>.
    /// </summary>
    public class UpgradeCommand : RuestortBaubefehl {

        protected override Bauart Bauart => Bauart.Ausbau;

        public UpgradeCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            return Baupunkte > 0
                ? $"Verstärke Rüstort {Location?.CreateBezeichner()} um {Baupunkte} Baupunkte"
                : $"Verstärke Rüstort {Location?.CreateBezeichner()}";
        }

        /// <summary>
        /// Ohne ausdrückliche Angabe wird so weit gebaut, wie es der Monat und die nächste Stufe
        /// hergeben.
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (Baupunkte <= 0)
                Baupunkte = BestimmeBaupunkteBisZurNächstenStufe();
            return base.CheckPreconditions();
        }

        private int BestimmeBaupunkteBisZurNächstenStufe() {
            var kleinfeld = BestimmeKleinfeld();
            var nächste = RuestortRules.GetNächsteStufe(kleinfeld);
            if (kleinfeld == null || nächste?.Baupunkte == null)
                return 0;
            return Math.Min(RuestortRules.MaxBaupunkteProMonat, nächste.Baupunkte.Value - kleinfeld.Baupunkte);
        }
    }

    public class UpgradeCommandParser : SimpleParser {

        private static readonly Regex UpgradeRegex = new(
              @"^Verstärke\s+Rüstort\s+(?<loc>\d+/\d+)(?:\s+um\s+(?<bp>\d+)\s+Baupunkte?)?$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = UpgradeRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                command = new UpgradeCommand(commandString) {
                    Location = ParseLocation(match.Groups["loc"].Value),
                    Baupunkte = match.Groups["bp"].Success ? ParseInt(match.Groups["bp"].Value) : 0,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des UpgradeCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
