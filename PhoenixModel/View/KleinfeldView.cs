using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public class KleinfeldView
    {

        public static bool IsKleinfeldKüste(KleinFeld kf)
        {
            /*if (SharedData.Map != null && SharedData.Map.ContainsKey(pos.CreateBezeichner()))
            {
                var nachbar = SharedData.Map[pos.CreateBezeichner()];
                // das Wasserfeld grenzt an land und ist daher Küste
                if (nachbar.Terrain.IsWasser == false)
                {
                    kf.Gelaendetyp = (int)PhoenixModel.ExternalTables.GeländeTabelle.TerrainType.Küste;
                }
            }*/
            return false;
        }
    }
}
