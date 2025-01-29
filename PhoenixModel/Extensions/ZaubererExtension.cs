using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Extensions {
    public static class ZaubererExtension {

        /// <summary>
        /// Ermittelt ob der Zauberer eine Barriere errichten kann
        /// </summary>
        public static bool CanCastBarriere(this Zauberer figur, KleinfeldPosition? kf = null, Direction? direction = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer auf ein benachbartes Kleinfeld bannen kann
        /// </summary>
        public static bool CanCastBannen(this Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer teleportieren kann und damit einige Leute mitnehmen
        /// </summary>
        public static bool CanCastTeleport(this Zauberer figur, KleinfeldPosition? kf = null, List<Spielfigur>? teleportPayLoad = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer einen benachbarten Zauberer zum Duell auffordern kann
        /// </summary>
        public static bool CanCastDuell(this Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }
    }
}
