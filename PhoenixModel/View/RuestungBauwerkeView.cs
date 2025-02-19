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
                if (bauwerk.Art.StartsWith("Burg"))
                    what = ConstructionElementType.Burg;
                else if (bauwerk.Art.StartsWith("Br"))
                    what = ConstructionElementType.Bruecke;
                else if (bauwerk.Art.StartsWith("Kai"))
                    what = ConstructionElementType.Kai;
                else if (bauwerk.Art.StartsWith("Stra"))
                    what = ConstructionElementType.Strasse;
                else if (bauwerk.Art.StartsWith("Wall"))
                    what = ConstructionElementType.Wall;

                string dir = string.Empty;
                if (what != ConstructionElementType.Burg) {
                    dir = bauwerk.Art.Split('_')[1];
                    var match = Enum.GetNames(typeof(DirectionNames))
                          .FirstOrDefault(name => dir.Contains(name, StringComparison.OrdinalIgnoreCase));
                    Direction? direction = match != null ? (Direction)Enum.Parse(typeof(DirectionNames), match) : null;
                    var command = new ConstructCommand($"Errichte {what} im {direction} von {bauwerk.CreateBezeichner()}") {
                        Direction = direction,
                        Location = bauwerk,
                        What = what,
                        Kosten = KostenView.GetKosten(bauwerk.ToString()),
                    };
                    SharedData.CommandQueue.Enqueue(command);
                }
                else {
                    var command = new ConstructCommand($"Errichte {what} auf {bauwerk.CreateBezeichner()}") {
                        Direction = null,
                        Location = bauwerk,
                        What = what,
                        Kosten = KostenView.GetKosten(bauwerk.ToString()),
                        IsExecuted = true
                    };
                    SharedData.CommandQueue.Enqueue(command);
                }
   
            }
            catch (Exception ex) {
                ProgramView.LogError($"Kann Bauauftrag {bauwerk.Art} nicht rekonstruieren", ex.Message);
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
                if (bauwerk.Art.StartsWith("Burg") == false) {
                    string fieldName = bauwerk.Art;
                    var field = kf.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(f => f.Name == fieldName);
                    if (field == null) {
                        throw new InvalidOperationException($"Das Feld {fieldName} wurde in der Klasse Kleinfeld nicht gefunden");
                    }
                    field.SetValue(kf, -1);
                }
                else {
                    BauwerkeView.AddBaustelle(kf);
                }
                SharedData.UpdateQueue.Enqueue(kf);
            }
        }
    }
}
