using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Collections.Concurrent;

namespace PhoenixModel.View {
    public static class KleinfeldView {
        /// <summary>
        /// In dieser Queue werden die markierten Kleinfelder abgelegt, damit sie wieder unmarkiert werden können
        /// </summary>
        private static ConcurrentQueue<KleinFeld> _markedQueue = [];

        public static void Mark(KleinFeld kf, MarkerType type, bool append = false) {
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

        /// <summary>
        /// verfolgt einen Pfad wie  "NO N SO S W SO O"
        /// </summary>
        /// <param name="kf"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<KleinFeld>? GetPath(KleinFeld kf, string path) {
            try {
                List<KleinFeld> list = [];
                var tokens = path.Split(' ');
                foreach(string token in tokens) {
                    if (Enum.TryParse<Direction>(token, out Direction direction)) {
                        // hol den nachbar in der Windrichtung
                        var pos = KartenKoordinaten.GetNachbar(kf, direction);
                        // verwende den Nachbar, falls es geht
                        if (SharedData.Map != null && pos != null && SharedData.Map.ContainsKey(pos.CreateBezeichner())) {
                            kf = SharedData.Map[pos.CreateBezeichner()];
                            list.Add(kf);
                        }
                        else {
                            // der Pfad verlässt bekanntes Gebiet
                            break;
                        }
                    }
                    else {
                        ProgramView.LogError($"Fehlerhaftes direction token '{token}' in einem Pfad verwendt", $"Die Funktion GetPath wurde mit dem Pfad {path} aufgerufen.");
                    }
                }
                return list;
            }
            catch (Exception e) {
                ProgramView.LogError("Beim Zählen der Nachbarn gab es einen Fehler", e.Message);
            }
            return null;
        }

        public static IEnumerable<KleinFeld>? GetNachbarn(KleinFeld kf, int distance = 1) {
            try {
                Queue<KleinFeld> working = [];
                Dictionary<string, KleinFeld> nachbarn = [];
                working.Enqueue(kf);
                int max = distance > 1?1:0;
                for (int i = 1; i <= distance; i++) {
                    max += 6 * i;
                }
                while (working.Count > 0) {
                    var positionen = KartenKoordinaten.GetKleinfeldNachbarn(working.Dequeue());
                    if (SharedData.Map != null && positionen != null) {
                        foreach (var pos in positionen) {
                            string key = KleinfeldPosition.CreateBezeichner(pos);
                            if (SharedData.Map.ContainsKey(key) && nachbarn.ContainsKey(key) == false) {
                                var nab = SharedData.Map[key];
                                nachbarn.Add(key, nab);
                                working.Enqueue(nab);
                            }
                        }
                        if (nachbarn.Count >= max)
                            break;
                    }
                }
                return nachbarn.Values;
            } catch (Exception e) {
                ProgramView.LogError("Beim Zählen der Nachbarn gab es einen Fehler", e.Message);
            }
            return null;
        }

        public static bool IsKleinfeldKüste(KleinFeld kf) {
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
