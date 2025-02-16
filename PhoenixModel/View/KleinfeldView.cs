using PhoenixModel.Commands;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static PhoenixModel.ExternalTables.EinwohnerUndEinnahmenTabelle;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhoenixModel.View {

    /// <summary>
    /// Für die Maarkierung der Kleinfelder gibt es verschiedene Typen. 
    /// Die Kartendarstellung generiert daraus Farben, sofern das dort implementiert wurde
    /// </summary>
    public enum MarkerType {
        None, User, Info, Warning, Fatality
    }

    /// <summary>
    /// Die Sicht auf die Karte und einzelne Kleinfelder
    /// </summary>
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
            if (type != MarkerType.None) {
                _markedQueue.Enqueue(kf);
            }
            else {
                // aus der Queue raus, was nicht mehr markiert ist
               var temp = _markedQueue.Where(kf => kf.Mark != MarkerType.None).ToArray();
                _markedQueue.Clear();
                foreach (var f in temp)
                    _markedQueue.Enqueue(f);
            }

            SharedData.UpdateQueue.Enqueue(kf);
        }

        /// <summary>
        /// entfernt alle bisher markierten
        /// </summary>
        public static void UnMark(KleinFeld kleinFeld) {
            Mark(kleinFeld, MarkerType.None);
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
        /// holte alle Markierten
        /// </summary>
        public static IEnumerable<KleinFeld> GetMarked(MarkerType mark) {
            return _markedQueue.Where(item => item.Mark == mark);
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

        public static KleinFeld? GetKleinfeld (KleinfeldPosition? kf) {
            if (SharedData.Map != null && kf != null && SharedData.Map.TryGetValue(kf.CreateBezeichner(), out KleinFeld? feld))
                return feld;
            return null;
        }


        /// <summary>
        /// holt die Nachbarn des übergebenen Kleinfeldes in der Distanz
        /// </summary>
        /// <param name="kf">kleinfeld als Ursprung der Suche</param>
        /// <param name="distance">Anzahl der Felder als Radius des Umkreises</param>
        /// <param name="includeSelf">das übergebene Feld mitnehmen</param>
        /// <returns>nichts, falls Fehler oder eine Liste aller Nachbarn und bei Bedarf des Feldes</returns>
        public static IEnumerable<KleinFeld>? GetNachbarn(KleinfeldPosition kfpos, int distance = 1, bool includeSelf = true) {
            if (SharedData.Map == null || SharedData.Map.IsAddingCompleted == false)
                return null;
            try {
                Queue<KleinfeldPosition> working = [];
                Dictionary<string, KleinfeldPosition> nachbarn = [];
                List<KleinFeld> result = [];
                KleinFeld? kf = GetKleinfeld(kfpos);
                if (kf != null) {
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
                }
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

        /// <summary>
        /// gibt true zurück, wenn auf dem Kleinfeld das Element Fluss in der gegebenen Richtung vorhanden ist
        /// </summary>
        /// <param name="kf">Kleinfed</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool HasRiver(KleinFeld kf, Direction direction) {
            switch (direction) {
                case Direction.NW:
                    return kf.Fluss_NW != 0;
                case Direction.NO:
                    return kf.Fluss_NO != 0;
                case Direction.O:
                    return kf.Fluss_O != 0;
                case Direction.SO:
                    return kf.Fluss_SO != 0;
                case Direction.SW:
                    return kf.Fluss_SW != 0;
                case Direction.W:
                    return kf.Fluss_W != 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        /// <summary>
        /// gibt true zurück, wenn auf dem Kleinfeld das Element Brücke in der gegebenen Richtung vorhanden ist
        /// </summary>
        /// <param name="kf">Kleinfed</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool HasBridge(KleinFeld kf, Direction direction) {
            switch (direction) {
                case Direction.NW:
                    return kf.Bruecke_NW != 0;
                case Direction.NO:
                    return kf.Bruecke_NO != 0;
                case Direction.O:
                    return kf.Bruecke_O != 0;
                case Direction.SO:
                    return kf.Bruecke_SO != 0;
                case Direction.SW:
                    return kf.Bruecke_SW != 0;
                case Direction.W:
                    return kf.Bruecke_W != 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        /// <summary>
        /// gibt true zurück, wenn auf dem Kleinfeld das Element Brücke in der gegebenen Richtung vorhanden ist
        /// </summary>
        /// <param name="kf">Kleinfed</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool HasWall(KleinFeld kf, Direction direction) {
            switch (direction) {
                case Direction.NW:
                    return kf.Wall_NW != 0;
                case Direction.NO:
                    return kf.Wall_NO != 0;
                case Direction.O:
                    return kf.Wall_O != 0;
                case Direction.SO:
                    return kf.Wall_SO != 0;
                case Direction.SW:
                    return kf.Wall_SW != 0;
                case Direction.W:
                    return kf.Wall_W != 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        /// <summary>
        /// gibt true zurück, wenn auf dem Kleinfeld das Element Strasse in der gegebenen Richtung vorhanden ist
        /// </summary>
        /// <param name="kf">Kleinfed</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool HasRoad(KleinFeld kf, Direction direction) {
            switch (direction) {
                case Direction.NW:
                    return kf.Strasse_NW != 0;
                case Direction.NO:
                    return kf.Strasse_NO != 0;
                case Direction.O:
                    return kf.Strasse_O != 0;
                case Direction.SO:
                    return kf.Strasse_SO != 0;
                case Direction.SW:
                    return kf.Strasse_SW != 0;
                case Direction.W:
                    return kf.Strasse_W != 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        /// <summary>
        /// gibt true zurück, wenn auf dem Kleinfeld das Element Kaianlagen in der gegebenen Richtung vorhanden ist
        /// </summary>
        /// <param name="kf">Kleinfed</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool HasKai(KleinFeld kf, Direction direction) {
            switch (direction) {
                case Direction.NW:
                    return kf.Kai_NW != 0;
                case Direction.NO:
                    return kf.Kai_NO != 0;
                case Direction.O:
                    return kf.Kai_O != 0;
                case Direction.SO:
                    return kf.Kai_SO != 0;
                case Direction.SW:
                    return kf.Kai_SW != 0;
                case Direction.W:
                    return kf.Kai_W != 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), "Invalid direction");
            }
        }

        
     
    }
}
