using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {
    /// <summary>
    /// Statische Klasse zur Verarbeitung und Abfrage von Kosten.
    /// </summary>
    public static class KostenView {
        /// <summary>
        /// Ermittelt die Kosten für eine bestimmte Spielfigur.
        /// </summary>
        /// <param name="figur">Die Spielfigur, für die die Kosten ermittelt werden sollen.</param>
        /// <returns>Die zugehörigen Kosten oder null, falls nicht gefunden.</returns>
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

        /// <summary>
        /// Ermittelt die Kosten für ein bestimmtes Bauwerk.
        /// </summary>
        /// <param name="ort">Das Bauwerk, für das die Kosten ermittelt werden sollen.</param>
        /// <returns>Die zugehörigen Kosten oder null, falls nicht gefunden.</returns>
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

        /// <summary>
        /// Sucht die Kosten anhand eines gegebenen Suchstrings in der Kosten-Datenbank.
        /// </summary>
        /// <param name="search">Der Suchstring für die Kostenabfrage.</param>
        /// <returns>Die entsprechenden Kosten oder null, falls nicht gefunden.</returns>
        public static Kosten? GetKosten(string? search) {
            if (string.IsNullOrEmpty(search))
                return null;

            if (SharedData.Kosten == null)
                return null;

            if (SharedData.Kosten.TryGetValue(search, out var kosten))
                return kosten;

            return null;
        }
    }
}