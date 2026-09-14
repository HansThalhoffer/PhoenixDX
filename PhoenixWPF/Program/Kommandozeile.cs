namespace PhoenixWPF.Program {

    /// <summary>
    /// Die Schalter, mit denen die Anwendung gestartet wurde.
    ///
    /// Bewusst klein gehalten: hier steht nur, was beim Start entschieden werden muss und sich
    /// nicht aus den Einstellungen ergibt.
    /// </summary>
    public static class Kommandozeile {

        /// <summary>
        /// Raeumt beim Start die Bauwerkliste auf - siehe <see cref="Kartenbereinigung"/>.
        /// Ohne diesen Schalter wird nichts geloescht.
        /// </summary>
        public const string SchalterBereinigung = "cleanup";

        /// <summary>
        /// Vergleicht beim Start die Zugdaten des gewählten Monats mit denen des Vormonats und
        /// schreibt das Ergebnis als Text - siehe <see cref="Database.Datenbankvergleich"/>.
        /// </summary>
        public const string SchalterVergleich = "vergleich";

        /// <summary>
        /// Wurde die Anwendung mit /cleanup gestartet?
        /// </summary>
        public static bool Bereinigung { get; private set; } = false;

        /// <summary>
        /// Wurde die Anwendung mit /vergleich gestartet?
        /// </summary>
        public static bool Vergleich { get; private set; } = false;

        /// <summary>
        /// Wertet die Argumente des Programmstarts aus.
        /// </summary>
        public static void Lies(IEnumerable<string>? argumente) {
            Bereinigung = IstGesetzt(argumente, SchalterBereinigung);
            Vergleich = IstGesetzt(argumente, SchalterVergleich);
        }

        /// <summary>
        /// Ein Schalter gilt als gesetzt, wenn er als /name, -name oder --name auftaucht. Gross-
        /// und Kleinschreibung spielt keine Rolle - auf der Kommandozeile tippt jeder, was er
        /// gerade im Kopf hat, und ein uebersehener Schalter waere hier besonders aergerlich:
        /// die Bereinigung liefe dann einfach nicht.
        /// </summary>
        public static bool IstGesetzt(IEnumerable<string>? argumente, string name) {
            if (argumente == null || string.IsNullOrEmpty(name))
                return false;
            foreach (var argument in argumente) {
                if (string.IsNullOrWhiteSpace(argument))
                    continue;
                string blank = argument.Trim().TrimStart('/', '-');
                if (string.Equals(blank, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
