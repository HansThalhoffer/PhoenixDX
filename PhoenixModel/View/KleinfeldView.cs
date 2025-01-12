using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class KleinfeldView
    {
        /// <summary>
        /// In dieser Queue werden die Objekte abgelegt, die in der Datenbank gespeichert werden sollen. Das geschieht asynchron
        /// </summary>
        public static ConcurrentQueue<KleinFeld> MarkedQueue = [];

        public static void Mark(KleinFeld kf, MarkerType type)
        {
            kf.Mark = type;
            MarkedQueue.Enqueue(kf);
            //ViewModel.Update()
        }




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
