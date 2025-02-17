using PhoenixModel.Commands;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.SpielfigurenView.SpielfigurenFilter;

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

            switch (figur.BaseTyp) {
                case FigurType.Krieger:
                case FigurType.Kreatur:
                    return GetKosten(ConstructionElementType.K);
                case FigurType.Reiter:
                    return GetKosten(ConstructionElementType.R);
                case FigurType.Schiff:
                    return GetKosten(ConstructionElementType.S);
                case FigurType.Zauberer: {
                        if (figur is Zauberer wiz) {
                            switch (wiz.Klasse) {
                                case Zaubererklasse.ZA:
                                    return GetKosten(ConstructionElementType.ZA);
                                default:
                                    return GetKosten(ConstructionElementType.ZB);
                            }
                        }
                        break;
                    }
            }
            return null;
        }

        public static Kosten? GetKosten(ConstructionElementType element) {
            if (SharedData.Kosten == null)
                return null;
            if (SharedData.Kosten.TryGetValue(element.ToString(), out var kosten))
                return kosten;
            return null;
        }

        public static int GetGSKosten(ConstructionElementType element) {
            if (SharedData.Kosten == null)
                return 0;
            if (SharedData.Kosten.TryGetValue(element.ToString(), out var kosten))
                return kosten.GS;
            return 0;
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