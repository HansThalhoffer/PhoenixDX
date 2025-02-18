using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {

    public static class RuestungBauwerkeView {
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
