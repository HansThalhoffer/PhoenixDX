using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class Plausibilität
    {
        public static bool IsValid(Spielfigur spielfigur)
        {
            return IsValid(spielfigur as GemarkPosition);
        }

        public static bool IsValid(GemarkPosition? position)
        {
            return position != null && position.gf > 0 && position.kf > 0 && position.kf <=48;
        }
    }
}
