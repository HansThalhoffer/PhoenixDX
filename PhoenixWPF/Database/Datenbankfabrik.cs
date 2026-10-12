using PhoenixModel.Database;
using System.IO;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Entscheidet, welcher Unterbau eine Datenbankdatei bedient.
    ///
    /// <b>Wofür:</b> damit kein Lader und kein Speicherlauf mehr <c>AccessDatabase</c> beim Namen
    /// nennt. Vorher stand der Klassenname an rund zwanzig Stellen; ein zweiter Unterbau hätte
    /// jede davon angefasst. Jetzt steht er einmal hier.
    ///
    /// <b>Wie entschieden wird:</b> an der Dateiendung. Das ist bewusst das einfachste mögliche
    /// Verfahren - kein Schalter in den Einstellungen, keine Erkennung am Inhalt. Eine .mdb wird
    /// mit dem Access-Treiber geöffnet, weil sie eine ist; dasselbe Spiel kann gleichzeitig eine
    /// .mdb für den Austausch mit der Spielleitung und etwas anderes für die eigenen Daten
    /// benutzen, ohne dass jemand etwas umstellen muss.
    /// </summary>
    public static class Datenbankfabrik {

        /// <summary>
        /// Die Endungen, die der Access-Treiber bedient.
        /// </summary>
        private static readonly string[] AccessEndungen = [".mdb", ".accdb"];

        /// <summary>
        /// Erzeugt den Zugang zu einer Datenbankdatei. <b>Geöffnet wird dabei nichts</b> - der
        /// Aufrufer ruft <see cref="IDatenbank.Open"/> und prüft dessen Ergebnis.
        /// </summary>
        /// <param name="datei">der Pfad zur Datenbankdatei</param>
        /// <param name="passwort">das Passwort im Klartext, oder null</param>
        /// <exception cref="NotSupportedException">
        /// wenn die Endung zu keinem Unterbau gehört. Hier kommt der nächste dazu, und nur hier.
        /// </exception>
        public static IDatenbank Erzeuge(string datei, string? passwort) {
            if (string.IsNullOrWhiteSpace(datei))
                throw new ArgumentException("Es wurde keine Datenbankdatei angegeben.", nameof(datei));

            string endung = Path.GetExtension(datei);
            if (AccessEndungen.Contains(endung, StringComparer.OrdinalIgnoreCase))
                return new AccessDatabase(datei, passwort);

            throw new NotSupportedException(
                $"Für die Datei {Path.GetFileName(datei)} gibt es keinen Unterbau. "
                + $"Bedient werden derzeit {string.Join(" und ", AccessEndungen)}.");
        }

        /// <summary>
        /// Klopft an eine Datenbankdatei, ohne etwas zu laden und ohne einen Fehler zu
        /// protokollieren. Damit lässt sich vor dem Laden feststellen, ob ein gespeichertes
        /// Passwort überhaupt noch passt.
        ///
        /// Die Probe benutzt ausdrücklich eine eigene Verbindung und hinterlässt beim
        /// <see cref="PhoenixModel.Database.Datenbankwaechter"/> nichts - sie wird auch mit einem
        /// falschen Passwort gerufen.
        /// </summary>
        /// <returns>true, wenn sich die Datei mit diesem Passwort öffnen liess</returns>
        public static bool PrüfeZugang(string datei, string? passwort, out string fehler) {
            string endung = Path.GetExtension(datei);
            if (AccessEndungen.Contains(endung, StringComparer.OrdinalIgnoreCase))
                return AccessDatabase.TestConnection(datei, passwort, out fehler);

            fehler = $"Für die Datei {Path.GetFileName(datei)} gibt es keinen Unterbau.";
            return false;
        }
    }
}
