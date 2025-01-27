using PhoenixModel.dbPZE;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {
    /// <summary>
    /// Statische Klasse zur Verwaltung und Abfrage von Nationen.
    /// </summary>
    public static class NationenView {
        /// <summary>
        /// Standard-Nation, die zurückgegeben wird, wenn keine passende Nation gefunden wird.
        /// </summary>
        private static Nation _default = new Nation("DEFAULT");

        /// <summary>
        /// Sucht eine Nation anhand eines Namens und gibt das entsprechende Nationen-Objekt zurück.
        /// </summary>
        /// <param name="name">Der Name der gesuchten Nation.</param>
        /// <returns>Das passende Nationen-Objekt oder die Standard-Nation, falls keine Übereinstimmung gefunden wurde.</returns>
        public static Nation GetNationFromString(string name) {
            // Überprüft, ob die Nationenliste existiert und der Name nicht leer ist
            if (SharedData.Nationen == null || string.IsNullOrEmpty(name))
                return _default;

            name = name.ToLower(); // Konvertiert den Namen in Kleinbuchstaben für eine case-insensitive Suche

            foreach (var nation in SharedData.Nationen) {
                if (nation.Alias == null)
                    continue;

                // Überprüft, ob der Name in der Alias-Liste der Nation vorhanden ist
                if (nation.Alias.Contains(name))
                    return nation;
            }

            return _default; // Gibt die Standard-Nation zurück, falls keine Übereinstimmung gefunden wurde
        }
    }
}