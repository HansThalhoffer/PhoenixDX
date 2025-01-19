using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace PhoenixModel.View {

    public enum MarkerType {
        None, Info, Warning, Fatality
    }

    public static class KleinfeldView {

        /// <summary>
        /// ist wahr, wenn eine Nation im Abstand von 2 ein Landfeld hat, die der Nation des Users das Küstenrecht zugebilligt hat
        /// </summary>
        /// <param name="kleinfeld"></param>
        /// <returns></returns>
        public static bool UserHasKuestenrecht(KleinFeld kleinfeld) {
            if (SharedData.Map == null ||  SharedData.Map.IsAddingCompleted == false)
                return false;
            if (kleinfeld.IsKüste == false) 
                return false;
            var nachbarn = KleinfeldView.GetNachbarn(kleinfeld, 2);
            if (nachbarn == null)
                return false;
            var allowed = DiplomatieView.GetKüstenregelAllowed();
            if (allowed == null)
                return false;
            return nachbarn.Any(s => allowed.Contains(s.Nation));
        }

        /// <summary>
        /// ist wahr, wenn die Nation der Gemark der Nation des Users das Wegerecht zugebilligt haben
        /// </summary>
        /// <param name="kleinfeld"></param>
        /// <returns></returns>
        public static bool UserHasWegerecht(KleinFeld kleinfeld) {
            var allowed = DiplomatieView.GetWegerectAllowed();
            if (allowed == null)
                return false;
            return kleinfeld.IsWasser == false && allowed.Contains(kleinfeld.Nation);
        }

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
                foreach (string token in tokens) {
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
        /// <summary>
        /// holt die Nachbarn des übergebenen Kleinfeldes in der Distanz
        /// </summary>
        /// <param name="kf">kleinfeld als Ursprung der Suche</param>
        /// <param name="distance">Anzahl der Felder als Radius des Umkreises</param>
        /// <param name="includeSelf">das übergebene Feld mitnehmen</param>
        /// <returns>nichts, falls Fehler oder eine Liste aller Nachbarn und bei Bedarf des Feldes</returns>
        public static IEnumerable<KleinFeld>? GetNachbarn(KleinFeld kf, int distance = 1, bool includeSelf = true) {
            if (SharedData.Map == null || SharedData.Map.IsAddingCompleted == false)
                return null;
            try {
                Queue<KleinfeldPosition> working = [];
                Dictionary<string, KleinfeldPosition> nachbarn = [];
                List<KleinFeld> result = [];
                working.Enqueue(kf);
                int max = distance > 1 ? 1 : 0;
                for (int i = 1; i <= distance; i++) {
                    max += 6 * i;
                }
                KleinFeld? feld = null;
                while (working.Count > 0) {
                    var positionen = KartenKoordinaten.GetKleinfeldNachbarn(working.Dequeue());
                    if (SharedData.Map != null && positionen != null) {
                        foreach (var pos in positionen) {
                            string key = KleinfeldPosition.CreateBezeichner(pos);
                            if (nachbarn.ContainsKey(key) == false) {
                                nachbarn.Add(key, pos);
                                working.Enqueue(pos);
                                if (SharedData.Map.TryGetValue(key, out feld)) {
                                    result.Add(feld);
                                }
                            }
                        }
                        if (nachbarn.Count >= max)
                            break;
                    }
                }
                if (includeSelf && nachbarn.ContainsKey(kf.CreateBezeichner()) == false)
                    result.Add(kf);
                else if (!includeSelf && nachbarn.ContainsKey(kf.CreateBezeichner()) == true)
                    result.Remove(kf);
                return result;
            }
            catch (Exception e) {
                ProgramView.LogError("Beim Zählen der Nachbarn gab es einen Fehler", e.Message);
            }
            return null;
        }

        /// <summary>
        /// Die Position Am Meer ist für das Einschiffen und für ds Rüsten von Flotten hilfreich
        /// </summary>
        /// <param name="kf"></param>
        /// <returns></returns>
        public static bool IsKleinfeldAmMeer(KleinFeld kf) {
            if (SharedData.Map == null || kf.IsWasser != false)
                return false;
            try {
                if (Plausibilität.IsOnMap(kf)) {
                    // optimistisch erstmal nur distanz 1 holen
                    var nachbarn = GetNachbarn(kf, 1);
                    if (nachbarn != null)
                        return nachbarn.Where(f => f.IsWasser == true).FirstOrDefault() != null;
                }
            }
            catch (Exception ex) {
                ProgramView.LogError($"Fehler bei der Berechung der Küstengewässer für {kf}", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// gibt zurück, ob das Kleinfeld im Wasser liegt und in der gegebenen Distanz an Land grenzt
        /// </summary>
        /// <param name="kf"></param>
        /// <param name="distanz"></param>
        /// <returns></returns>
        public static bool IsKleinfeldKüstenGewässer(KleinFeld kf, int distanz = 2) {
            if (SharedData.Map == null || kf.IsWasser == false)
                return false;
            try {
                if (Plausibilität.IsOnMap(kf)) {
                    // optimistisch erstmal nur distanz 1 holen
                    for (int i = 1; i <= distanz; i++) {
                        var nachbar = GetNachbarn(kf, i);
                        if (nachbar != null)
                            foreach (var f in nachbar)
                                if (f.IsWasser == false)
                                    return true;
                    }
                }
            }
            catch (Exception ex) {
                ProgramView.LogError($"Fehler bei der Berechung der Küstengewässer für {kf}", ex.Message);
            }
            return false;
        }
    }
}
