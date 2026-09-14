using PhoenixModel.dbPZE;
using PhoenixModel.Rules;
using PhoenixModel.View;
using System.Collections.Generic;
using System.Linq;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Sicht auf die Diplomatie Datenklassen.
    ///
    /// Beantwortet wird immer dieselbe Frage: welche Reiche haben diesem Reich ein Recht
    /// eingeräumt? Wer gefragt wird, entscheidet der Parameter - und wer wem etwas eingeräumt hat,
    /// entscheidet <see cref="DiplomatieRules"/>, das dazu beide Zeilen eines Reichspaares liest.
    /// </summary>
    public static class DiplomatieView {

        /// <summary>
        /// Holt die Liste aller Nationen, die der Nation des Users das Küstenrecht zugebilligt haben.
        ///
        /// Das Ergebnis wird gemerkt, aber nur für das Reich, für das es ermittelt wurde: wechselt
        /// die Auswahl, wird neu nachgesehen.
        /// </summary>
        static List<Nation>? _NationenMitKüstenrechtErlaubt = null;
        static Nation? _KüstenrechtFür = null;
        public static IEnumerable<Nation>? GetKüstenregelAllowed() {
            var nation = ProgramView.SelectedNation;
            if (_NationenMitKüstenrechtErlaubt == null || IstFür(_KüstenrechtFür, nation) == false) {
                _NationenMitKüstenrechtErlaubt = GetKüstenregelAllowed(nation) as List<Nation>;
                _KüstenrechtFür = _NationenMitKüstenrechtErlaubt == null ? null : nation;
            }
            return _NationenMitKüstenrechtErlaubt ?? Enumerable.Empty<Nation>();
        }

        /// <summary>
        /// Holt die Liste aller Nationen, die der Nation des Users das Wegerecht zugebilligt haben
        /// </summary>
        static List<Nation>? _NationenMitWegerechtErlaubt = null;
        static Nation? _WegerechtFür = null;
        public static IEnumerable<Nation>? GetWegerectAllowed() {
            var nation = ProgramView.SelectedNation;
            if (_NationenMitWegerechtErlaubt == null || IstFür(_WegerechtFür, nation) == false) {
                _NationenMitWegerechtErlaubt = GetWegerectAllowed(nation) as List<Nation>;
                _WegerechtFür = _NationenMitWegerechtErlaubt == null ? null : nation;
            }
            return _NationenMitWegerechtErlaubt ?? Enumerable.Empty<Nation>();
        }

        /// <summary>
        /// Holt die Liste aller Nationen, die der übergebenen Nation das Küstenrecht zugebilligt haben
        /// </summary>
        public static IEnumerable<Nation>? GetKüstenregelAllowed(Nation? nation)
            => SammleGeber(nation, DiplomatieRules.HatKüstenrecht);

        /// <summary>
        /// Holt die Liste aller Nationen, die der übergebenen Nation das Wegerecht zugebilligt haben
        /// </summary>
        public static IEnumerable<Nation>? GetWegerectAllowed(Nation? nation)
            => SammleGeber(nation, DiplomatieRules.HatWegerecht);

        /// <summary>
        /// Vergisst die gemerkten Listen, etwa wenn sich die Diplomatie geändert hat
        /// </summary>
        public static void Vergiss() {
            _NationenMitKüstenrechtErlaubt = null;
            _NationenMitWegerechtErlaubt = null;
            _KüstenrechtFür = null;
            _WegerechtFür = null;
        }

        private static bool IstFür(Nation? gemerkt, Nation? gefragt)
            => gemerkt == null ? gefragt == null : gemerkt.Equals(gefragt);

        /// <summary>
        /// Sammelt die Reiche, die dem Empfänger das Recht eingeräumt haben.
        ///
        /// Durchgesehen werden alle Reiche, die in der Tabelle überhaupt mit dem Empfänger in
        /// einer Zeile stehen - gleich ob als Geber oder als Empfänger der Zeile. Welche Zeile den
        /// Eintrag trägt, ist im Datenbestand nicht verlässlich; das Nachsehen in beide Richtungen
        /// übernimmt <see cref="DiplomatieRules"/>.
        /// </summary>
        /// <returns>die Geber, oder null solange die Diplomatie nicht geladen ist</returns>
        private static List<Nation>? SammleGeber(Nation? empfänger, Func<Nation?, Nation?, bool> hatRecht) {
            if (empfänger == null || SharedData.Diplomatie == null || SharedData.Diplomatie.IsAddingCompleted == false)
                return null;

            List<Nation> ergebnis = [];
            foreach (var zeile in SharedData.Diplomatie.Values) {
                Nation? geber = null;
                if (zeile.Nation.Equals(empfänger))
                    geber = zeile.ReferenzNation;
                else if (zeile.ReferenzNation.Equals(empfänger))
                    geber = zeile.Nation;

                if (geber == null || geber.Equals(empfänger))
                    continue;
                if (ergebnis.Any(bekannt => bekannt.Equals(geber)))
                    continue;
                if (hatRecht(geber, empfänger))
                    ergebnis.Add(geber);
            }
            return ergebnis;
        }
    }
}
