using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    public static class Plausibilität {
       
        public static bool IsValid(Spielfigur spielfigur) {
            return IsValid(spielfigur as KleinfeldPosition);
        }

        public static bool IsOnMap(KleinfeldPosition? pos) {
            if (!IsValid(pos) || pos == null) 
                return false;
            if (SharedData.Map == null)
                return false;
            return SharedData.Map.ContainsKey(pos.CreateBezeichner());
        }


        public static bool IsValid(KleinfeldPosition? position) {
            return position != null && position.gf > 0 && position.kf > 0 && position.kf <= 48;
        }
    }
}
