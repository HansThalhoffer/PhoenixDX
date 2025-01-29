using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Extensions {
    public static class TruppenSpielfigurExtension {

        /// <summary>
        /// Ermittelt ob die Truppe sich aufteilen kann
        /// </summary>
        public static bool CanSplit(this TruppenSpielfigur truppe) {
            return truppe.Heerführer > 1 && truppe.staerke > 1; 
        }

        /// <summary>
        /// Ermittelt ob die Figur mit einer auf dem gleichen Kleinfeld vorhandenen Truppe fusionieren kann
        /// </summary>
        public static bool CanFusion(this TruppenSpielfigur truppe, KleinfeldPosition? kf = null) {
            return true;
        }
    }
}
