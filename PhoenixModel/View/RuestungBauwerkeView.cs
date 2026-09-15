using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {

    public static class RuestungBauwerkeView {

        /// <summary>
        /// erzeugt aus einem Rüstungsauftrag ein ConstructCommand und legt es in der SharedData.Commands ab
        /// Die Funktion wird beim Load aus der Datenbank benötigt
        /// </summary>
        /// <param name="bauwerk"></param>
        public static void ReconstructCommand(RuestungBauwerke bauwerk) {
            if (bauwerk == null)
                return;
            try {
                ConstructionElementType what = ConstructionElementType.None;
                if (bauwerk.Art.StartsWith("Dorf")) 
                    what = ConstructionElementType.Dorf;
                if (bauwerk.Art.StartsWith("Burg"))
                    what = ConstructionElementType.Burg;
                if (bauwerk.Art.StartsWith("Stadt"))
                    what = ConstructionElementType.Stadt;
                if (bauwerk.Art.StartsWith("Festungshauptstadt"))
                     what = ConstructionElementType.Festungshauptstadt;
                if (bauwerk.Art.StartsWith("Festung"))
                    what = ConstructionElementType.Festung;
                if (bauwerk.Art.StartsWith("Hauptstadt"))
                    what = ConstructionElementType.Hauptstadt;
                else if (bauwerk.Art.StartsWith("Br")) 
                    what = ConstructionElementType.Bruecke;
                else if (bauwerk.Art.StartsWith("Kai"))
                    what = ConstructionElementType.Kai;
                else if (bauwerk.Art.StartsWith("Stra"))
                    what = ConstructionElementType.Strasse;
                else if (bauwerk.Art.StartsWith("Wall"))
                    what = ConstructionElementType.Wall;
                // hat es eine Richtung?
                if (what == ConstructionElementType.Bruecke || what == ConstructionElementType.Kai || what == ConstructionElementType.Strasse || what == ConstructionElementType.Wall) {
                    string dir = bauwerk.Art.Split('_')[1];
                    Direction? direction = dir != null ? (Direction)Enum.Parse(typeof(Direction), dir) : null;
                    var command = new ConstructCommand($"Errichte {what} im {direction} von {bauwerk.CreateBezeichner()}") {
                        Direction = direction,
                        Location = bauwerk,
                        What = what,
                        Kosten = KostenView.GetKosten(what),
                        IsExecuted = true,
                    };
                    SharedData.CommandQueue.Enqueue(command);
                }
                else {
                    var kosten = KostenView.GetKosten(what);
                    if (kosten == null) {
                        kosten = new dbCrossRef.Kosten() {
                            Unittyp = bauwerk.Art,
                            BauPunkte = bauwerk.BP_neu > bauwerk.BP_rep ? bauwerk.BP_neu : bauwerk.BP_rep,
                            GS = bauwerk.Kosten,
                            RP = 0
                        };
                    }

                    var command = new ConstructCommand($"Errichte {what} auf {bauwerk.CreateBezeichner()}") {
                        Direction = null,
                        Location = bauwerk,
                        What = what,
                        Kosten = kosten,
                        IsExecuted = true,
                        // Der Befehl gehört dem Monat seines Bauauftrags, nicht dem Augenblick,
                        // in dem er wiederhergestellt wird. Nur so bleibt erkennbar, was aus dem
                        // laufenden Zug stammt und sich zurücknehmen lässt.
                        Zug = bauwerk.ZugMonat,
                    };
                    SharedData.CommandQueue.Enqueue(command);
                }
   
            }
            catch (Exception ex) {
                ProgramView.LogError($"Kann Bauauftrag {bauwerk.Art} nicht rekonstruieren", ex.Message);
            }

            
        }

        /// <summary>
        /// Wendet die Bauauftraege eines Zuges auf die Karte an und stellt die
        /// zugehoerigen Befehle in der Zughistorie wieder her.
        ///
        /// Gehoert zum Laden der Zugdaten des laufenden Zuges. Nicht aufrufen fuer
        /// vergangene Zuege - deren Bauauftraege gehoeren nicht auf die aktuelle Karte.
        /// </summary>
        public static void WendeAlleAn(IEnumerable<RuestungBauwerke>? bauwerke) {
            if (bauwerke == null)
                return;
            foreach (var bauwerk in bauwerke) {
                UpdateKleinFeld(bauwerk);
                ReconstructCommand(bauwerk);
            }
        }

        /// <summary>
        /// Das Kleinfeld wird anhand eine Bauauftrags aktualisiert, so dass die ursprüngliche Karte in der Datenbank nicht angepasst werden muss
        /// </summary>
        /// <param name="bauwerk"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void UpdateKleinFeld(RuestungBauwerke bauwerk) {
            if (bauwerk != null && SharedData.Map != null) {
                var kf = SharedData.Map[bauwerk.CreateBezeichner()];
                string fieldName = bauwerk.Art;
                var field = kf.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(f => f.Name == fieldName);
                if (field == null) {
                    BauwerkeView.AddBaustelle(kf);
                }
                else { 
                    field.SetValue(kf, -1);
                }
                
                SharedData.UpdateQueue.Enqueue(kf);
            }
        }
    }
}
