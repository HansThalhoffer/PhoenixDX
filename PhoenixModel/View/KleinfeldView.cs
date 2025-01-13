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
        /// In dieser Queue werden die markierten Kleinfelder abgelegt, damit sie wieder unmarkiert werden können
        /// </summary>
        private static ConcurrentQueue<KleinFeld> _markedQueue = [];

        public static void Mark(KleinFeld kf, MarkerType type, bool append = false)
        {
            if (append == false) 
                UnMark();
            
            kf.Mark = type;
            _markedQueue.Enqueue(kf);
            SharedData.UpdateQueue.Enqueue(kf);
        }

        /// <summary>
        /// entfernt alle bisher markierten
        /// </summary>
        public static void UnMark() {
            KleinFeld? kf = null;
            while (_markedQueue.Count > 0 && _markedQueue.TryDequeue(out kf)) {
                if (kf != null) {
                    kf.Mark = MarkerType.None;
                    SharedData.UpdateQueue.Enqueue(kf);
                }
            }
        }

        public static IEnumerable<KleinFeld>? GetNachbarn(KleinFeld kf) {
            try {
                var positionen = KartenKoordinaten.GetKleinfeldNachbarn(kf);
                if (SharedData.Map != null && positionen != null) {
                    List<KleinFeld> nachbarn = [];
                    foreach (var pos in positionen) {
                        string key = KleinfeldPosition.CreateBezeichner(pos);
                        if (SharedData.Map.ContainsKey(key))
                            nachbarn.Add(SharedData.Map[key]);
                    }
                    return nachbarn;
                }
            } catch (Exception e) {
                ViewModel.LogError("Beim Zählen der Nachbarn gab es einen Fehler", e.Message);
            }
            return null;
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
