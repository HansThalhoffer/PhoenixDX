using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.View.SpielfigurenView.SpielfigurenFilter;

namespace PhoenixModel.View {
    public static class KostenView {

        public static Kosten? GetKosten(Spielfigur figur) {
            string? search = null;
            switch (figur.BaseTyp) {
                case FigurType.Krieger:
                case FigurType.Kreatur:
                    search = "K";
                    break;
                case FigurType.Reiter:
                    search = "R";
                    break;
                case FigurType.Schiff:
                    search = "S";
                    break;
                case FigurType.Zauberer: {
                        if (figur is Zauberer wiz) {
                            switch (wiz.Klasse) {
                                case Zaubererklasse.ZA:
                                    search = "ZA";
                                    break;
                                default:
                                    search = "ZB";
                                    break;
                            }
                        }
                        break;
                    }
            }
            return GetKosten(search);
        }

        public static Kosten? GetKosten(BauwerkBasis ort) {
            string? search = null;
            if (ort == null)
                return null;
            if (ort.Bauwerk.StartsWith("Dorf"))
                return null;

            if (ort.Bauwerk.StartsWith("Burg"))
                search = "Burg";
            else if (ort.Bauwerk.StartsWith("Stadt"))
                search = "Stadt";
            else if (ort.Bauwerk.StartsWith("Festungshauptstadt"))
                search = "Festungshauptstadt";
            else if (ort.Bauwerk.StartsWith("Festung"))
                search = "Festung";
            else if (ort.Bauwerk.StartsWith("Hauptstadt"))
                search = "Hauptstadt";
            return GetKosten(search);
        }


        public static Kosten? GetKosten(string? search) {
            if (string.IsNullOrEmpty(search))
                return null;
            if (SharedData.Kosten == null) return null;
            if (SharedData.Kosten.TryGetValue(search, out var kosten))
                return kosten;
            return null;
        }
    }
}
