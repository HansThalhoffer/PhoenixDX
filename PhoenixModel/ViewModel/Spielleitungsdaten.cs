using PhoenixModel.dbPZE;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Die Figuren aller Reiche eines Zuges, wie sie nur die Spielleitung sieht.
    ///
    /// Bewusst getrennt von <see cref="SharedData"/>: dort stehen die Daten des einen Reiches, mit
    /// dem der Spieler arbeitet, und daran hängt alles - Befehle, Speichern, Karte. Dieser Speicher
    /// hier ist rein lesend und rührt nichts davon an.
    ///
    /// Zwei Fallstricke, die das nötig machen:
    ///
    /// Erstens ist <c>DatabaseName</c> eine statische Eigenschaft je Tabellenklasse, die beim Laden
    /// gesetzt wird, und der Speicherlauf entscheidet an genau diesem Wert, in welche Datenbank ein
    /// Datensatz geht. Würde man ein zweites Reich über den gewöhnlichen Ladeweg holen, schriebe
    /// die Anwendung danach die Züge des Spielers in die fremde Datenbank.
    ///
    /// Zweitens bekommt jede Figur in <c>Spielfigur.Load</c> das gerade ausgewählte Reich
    /// zugewiesen. Ohne Vorkehrung trügen die Heere aller Reiche den Namen des eigenen.
    ///
    /// Der Leser in der Anwendungsschicht kümmert sich um beides.
    /// </summary>
    public static class Spielleitungsdaten {

        private static readonly object _sperre = new();
        private static readonly Dictionary<string, Armee> _nachReich = [];

        /// <summary>
        /// Der Zug, zu dem die geladenen Daten gehören. 0, solange nichts geladen ist.
        /// </summary>
        public static int Zug { get; private set; }

        /// <summary>
        /// Sind Daten geladen?
        /// </summary>
        public static bool IstGeladen {
            get { lock (_sperre) return _nachReich.Count > 0; }
        }

        /// <summary>
        /// Die Reiche, zu denen Figuren vorliegen
        /// </summary>
        public static IReadOnlyList<string> GeladeneReiche {
            get { lock (_sperre) return _nachReich.Keys.OrderBy(name => name).ToList(); }
        }

        /// <summary>
        /// Nimmt die Figuren eines Reiches auf. Ein erneutes Ablegen ersetzt den vorherigen Stand.
        /// </summary>
        public static void Lege(Nation reich, Armee figuren, int zug) {
            if (reich == null)
                return;
            lock (_sperre) {
                _nachReich[reich.Reich] = figuren;
                Zug = zug;
            }
        }

        /// <summary>
        /// Die Figuren eines Reiches, oder eine leere Armee
        /// </summary>
        public static Armee GetFiguren(Nation? reich) {
            if (reich == null)
                return [];
            lock (_sperre) {
                return _nachReich.TryGetValue(reich.Reich, out var armee) ? armee : [];
            }
        }

        /// <summary>
        /// Alle Figuren aller geladenen Reiche
        /// </summary>
        public static Armee GetAlleFiguren() {
            Armee alle = [];
            lock (_sperre) {
                foreach (var armee in _nachReich.Values)
                    alle.AddRange(armee);
            }
            return alle;
        }

        /// <summary>
        /// Alle Figuren, die auf dieser Gemark stehen - über alle Reiche hinweg.
        ///
        /// Das ist die Grundlage der Konflikterkennung: wo Figuren verfeindeter Reiche auf
        /// derselben Gemark stehen, kommt es zum Kampf.
        /// </summary>
        public static Armee GetFigurenAufGemark(KleinfeldPosition? gemark) {
            Armee ergebnis = [];
            if (gemark == null)
                return ergebnis;
            foreach (var figur in GetAlleFiguren()) {
                if (Plausibilität.IsValid(figur) && figur.gf == gemark.gf && figur.kf == gemark.kf)
                    ergebnis.Add(figur);
            }
            return ergebnis;
        }

        /// <summary>
        /// Wirft alles weg, etwa beim Zugwechsel
        /// </summary>
        public static void Leere() {
            lock (_sperre) {
                _nachReich.Clear();
                Zug = 0;
            }
        }
    }
}
