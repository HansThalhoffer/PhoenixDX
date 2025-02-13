using PhoenixModel.dbZugdaten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    public static class SchatzkammerView {

        /// <summary>
        /// die Funktion berechnet den aktuellen Stand der Schatzkammer für den Zug
        /// </summary>
        /// <returns></returns>
        public static Schatzkammer GetActual() {
            return new Schatzkammer();
        }

    }
}
