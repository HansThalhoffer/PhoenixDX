using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    public static class TruppenSpielfigurView {
        /// <summary>
        /// Ermittelt ob die Truppe sich aufteilen kann
        /// </summary>
        public static bool CanSplit( TruppenSpielfigur truppe) {
            return truppe.Heerführer > 1 && truppe.staerke > 1;
        }

        /// <summary>
        /// Ermittelt ob die Figur mit einer auf dem gleichen Kleinfeld vorhandenen Truppe fusionieren kann
        /// </summary>
        public static bool CanFusion( TruppenSpielfigur truppe) {
            var kf = KleinfeldView.GetKleinfeld(truppe);
            if (kf == null) 
                return false;
            // eine weitere Spielfigur gleichen Typs auf dem Kleinfeld?
            return kf.Truppen.Where( t => t.BaseTyp == truppe.BaseTyp && t != truppe).Any();
        }
    }
}
