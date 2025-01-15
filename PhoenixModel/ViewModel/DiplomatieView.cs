using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    public static class DiplomatieView {

        /// <summary>
        /// holt die Liste aller Nationen, die der Nation des Users das Küstenrecht zugebilligt haben
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        static List<Nation>? _NationenMitKüstenrechtErlaubt = null;
        public static IEnumerable<Nation>? GetKüstenregelAllowed() {
            if (_NationenMitKüstenrechtErlaubt == null) 
                _NationenMitKüstenrechtErlaubt = GetKüstenregelAllowed(ProgramView.SelectedNation) as List<Nation>;
            if (_NationenMitKüstenrechtErlaubt == null)
                return Enumerable.Empty<Nation>();
            return _NationenMitKüstenrechtErlaubt;
        }

        /// <summary>
        /// holt die Liste aller Nationen, die der Nation des Users das Küstenrecht zugebilligt haben
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        static List<Nation>? _NationenMitWegerechtErlaubt = [];
        public static IEnumerable<Nation>? GetWegerectAllowed() {
            if (_NationenMitWegerechtErlaubt == null)
                _NationenMitWegerechtErlaubt = GetWegerectAllowed(ProgramView.SelectedNation) as List<Nation>;
            if (_NationenMitWegerechtErlaubt == null)
                return Enumerable.Empty<Nation>();
            return _NationenMitWegerechtErlaubt;
        }

        /// <summary>
        /// holt die Liste aller Nationen, die der übergebenen Nation das Küstenrecht zugebilligt haben
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        public static IEnumerable<Nation>? GetKüstenregelAllowed(Nation? nation) {
            if (nation == null || SharedData.Diplomatie == null || SharedData.Diplomatie.IsAddingCompleted == false)
                return null;
            var crossref = SharedData.Diplomatie.Values.Where(d => d.Nation == ProgramView.SelectedNation && d.Kuestenrecht_von > 0);
            List<Nation> result = [];
            foreach (var c in crossref) {
                result.Add(c.ReferenzNation);
            }
            return result;
        }

        /// <summary>
        /// holt die Liste aller Nationen, die der übergebenen Nation das Küstenrecht zugebilligt haben
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        public static IEnumerable<Nation>? GetWegerectAllowed(Nation? nation) {
            if (nation == null || SharedData.Diplomatie == null || SharedData.Diplomatie.IsAddingCompleted == false)
                return null;
            var crossref = SharedData.Diplomatie.Values.Where(d => d.Nation == ProgramView.SelectedNation && d.Kuestenrecht_von > 0);
            List<Nation> result = [];
            foreach (var c in crossref) {
                result.Add(c.ReferenzNation);
            }
            return result;
        }
    }
}
