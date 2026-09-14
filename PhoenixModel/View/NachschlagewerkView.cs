using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Zwei Listen, die nachgeschlagen und nicht gerechnet werden: das Bestiarium der Karte und
    /// die Personalliste des Reiches.
    ///
    /// Das Bestiarium beschreibt die Kreaturen Erkenfaras mit ihren Werten (Regelwerk 6.3). Es
    /// steht in der Kartendatenbank und gilt für alle Reiche.
    ///
    /// Achtung bei der Personalliste: darin stehen Namen, Anschriften, Telefonnummern und
    /// Mailadressen wirklicher Menschen. Sie gehören in das Fenster, in dem sie stehen - nicht in
    /// eine Meldung, nicht in die Zwischenablage und nicht in einen Fehlerbericht. Deshalb gibt es
    /// hier auch keine Methode, die sie zu Text macht.
    /// </summary>
    public static class NachschlagewerkView {

        /// <summary>
        /// Alle Kreaturen des Bestiariums, nach Namen sortiert
        /// </summary>
        public static List<Bestiarium> GetKreaturen() {
            if (SharedData.Bestiarium == null)
                return [];
            return [.. SharedData.Bestiarium.OrderBy(kreatur => kreatur.Kreaturenname ?? string.Empty)];
        }

        /// <summary>
        /// Schlägt eine Kreatur im Bestiarium nach.
        ///
        /// Gesucht wird ohne Rücksicht auf Gross- und Kleinschreibung: die Namen kommen aus einer
        /// von Hand gepflegten Tabelle.
        /// </summary>
        /// <param name="name">der Name der Kreatur</param>
        /// <returns>der Eintrag, oder null wenn es ihn nicht gibt</returns>
        public static Bestiarium? GetKreatur(string? name) {
            if (SharedData.Bestiarium == null || string.IsNullOrWhiteSpace(name))
                return null;
            return SharedData.Bestiarium.FirstOrDefault(kreatur =>
                string.Equals(kreatur.Kreaturenname?.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Die Personalliste des Reiches.
        ///
        /// Sortiert nach der Position, damit die Liste sich lesen lässt wie ein Hofstaat.
        /// </summary>
        public static List<Personal> GetPersonal() {
            if (SharedData.Personal == null)
                return [];
            return [.. SharedData.Personal
                .OrderBy(eintrag => eintrag.Pos ?? string.Empty)
                .ThenBy(eintrag => eintrag.Nachname ?? string.Empty)];
        }

        /// <summary>
        /// Die Einträge beider Listen als <see cref="IEigenschaftler"/>, für die Anzeige in einem
        /// Gitter.
        /// </summary>
        public static List<IEigenschaftler> AlsEigenschaftler(IEnumerable<IEigenschaftler>? einträge)
            => einträge == null ? [] : [.. einträge];
    }
}
