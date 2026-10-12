using PhoenixModel.View;
using System.Data;
using System.Data.Common;

namespace PhoenixModel.Database {

    /// <summary>
    /// Der Besitzer der Verbindung zu <em>einer</em> Datenbankdatei: er hält sie offen und gibt sie
    /// nur einem Ablauf zur Zeit.
    ///
    /// <b>Warum es ihn gibt.</b> Der Access-Treiber stürzt nicht an der Menge der Abfragen, sondern
    /// an der Zahl der <em>Verbindungen</em>: gemessen etwa eine Zugriffsverletzung in
    /// mso99Lwin32client.dll je 400 Verbindungen, die den Prozess ohne verwertbare Ausnahme
    /// beendet. Ein voller Testlauf öffnete 758 Verbindungen und stürzte entsprechend. Bleibt je
    /// Datei eine Verbindung offen, sinkt das auf eine Handvoll je Prozess.
    ///
    /// <b>Was er besser macht als der Verbindungscache zuvor.</b> Der hatte einen Notausgang: war
    /// die gemeinsame Verbindung länger als die <see cref="Geduld"/> belegt, öffnete er eine
    /// eigene. Das war falsch herum - unter Last wich er genau in die Betriebsart aus, die den
    /// Treiber umbringt, und zwar dann, wenn ohnehin schon viel los war. Hier wird statt dessen
    /// gewartet und am Ende <em>abgebrochen</em>: der Aufrufer bekommt null, meldet es und schreibt
    /// nichts. Ein gemeldeter Fehlschlag ist besser als ein Absturz des ganzen Prozesses und besser
    /// als eine halb geschriebene Datei.
    ///
    /// <b>Der Preis.</b> Eine offene Verbindung hält die Datei; Access legt daneben eine .ldb an.
    /// Wer die Datei kopieren, packen, verschieben oder ersetzen will - die Zugabgabe etwa -, muss
    /// vorher <see cref="SchliesseAlle"/> rufen. Das ist der Grund, warum es diese Methode gibt,
    /// und nicht bloss Aufräumen.
    ///
    /// <b>Anbieterfrei.</b> Der Wächter kennt nur <see cref="DbConnection"/> und einen Erzeuger.
    /// Womit die Verbindung gebaut wird - OLE DB, SQLite - weiss allein der Erzeuger. Damit gilt
    /// alles hier Beschriebene auch für einen späteren Unterbau, ohne dass es zweimal dasteht.
    /// </summary>
    public sealed class Datenbankwaechter {

        /// <summary>
        /// Je Datei ein Wächter. Die Einträge werden <b>nie entfernt</b>, auch von
        /// <see cref="SchliesseAlle"/> nicht - nur ihre Verbindung wird geschlossen.
        ///
        /// Das ist wichtig: am Wächter hängt das Schloss. Würde er weggeworfen und später neu
        /// gebaut, bekäme die Datei ein zweites Schloss, und zwei Abläufe hielten sich für die
        /// jeweils einzigen Benutzer derselben Datei.
        /// </summary>
        private static readonly Dictionary<string, Datenbankwaechter> _wächter = [];

        /// <summary>
        /// Schützt das Verzeichnis selbst - nicht die Verbindungen darin.
        /// </summary>
        private static readonly object _verzeichnisSchloss = new();

        /// <summary>
        /// Wie lange auf eine belegte Verbindung gewartet wird, bevor der Zugriff aufgegeben wird.
        ///
        /// Eine Grenze braucht es: zwei Abläufe, die zwei Dateien in verschiedener Reihenfolge
        /// belegen, blockieren sich sonst gegenseitig, die Anwendung stünde still und niemand wüsste
        /// warum. Nach Ablauf scheitert der Zugriff mit einer Meldung, die sagt, welche Datei es war.
        ///
        /// Veränderbar, damit der Test den belegten Fall in Millisekunden prüfen kann, statt eine
        /// halbe Minute zu warten.
        /// </summary>
        public static TimeSpan Geduld { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Beim Ende des Prozesses werden die offen gehaltenen Verbindungen geschlossen.
        ///
        /// Sonst bleibt neben jeder Datenbank ihre Sperrdatei liegen - Access legt eine .ldb an,
        /// solange jemand verbunden ist, und räumt sie erst beim Schliessen der letzten Verbindung
        /// weg. Vor dem Offenhalten fiel das nicht auf, weil jede Verbindung sofort wieder zuging.
        /// </summary>
        static Datenbankwaechter() {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => SchliesseAlle();
        }

        private readonly string _schlüssel;
        private readonly Func<DbConnection> _erzeuger;

        /// <summary>
        /// Solange ein Ablauf das Schloss hält, gehört ihm die Verbindung allein. Eine
        /// <see cref="DbConnection"/> verträgt keine zwei Benutzer zugleich, und die Anwendung
        /// greift aus mehreren Threads zu (Karte, Oberfläche, Laden und Speichern im Hintergrund).
        ///
        /// Ein Monitor und nicht ein Semaphor: er ist je Thread wiedereintrittsfähig. Das ist nicht
        /// Bequemlichkeit, sondern notwendig - die Lader öffnen innerhalb eines Ladevorgangs
        /// mehrere Verbindungen zur selben Datei nacheinander und teils verschachtelt.
        /// </summary>
        private readonly object _schloss = new();

        private DbConnection? _verbindung;

        private Datenbankwaechter(string schlüssel, Func<DbConnection> erzeuger) {
            _schlüssel = schlüssel;
            _erzeuger = erzeuger;
        }

        /// <summary>
        /// Der Wächter zu dieser Datei - beim ersten Mal wird er angelegt.
        /// </summary>
        /// <param name="schlüssel">
        /// was die Datei eindeutig bezeichnet. In der Praxis die Verbindungszeichenfolge: sie
        /// enthält den Pfad <em>und</em> das Passwort, und zwei Zugänge mit verschiedenen
        /// Passwörtern sind nicht dieselbe Verbindung.
        /// </param>
        /// <param name="erzeuger">baut eine neue Verbindung, wenn keine offen ist</param>
        public static Datenbankwaechter Hole(string schlüssel, Func<DbConnection> erzeuger) {
            lock (_verzeichnisSchloss) {
                if (_wächter.TryGetValue(schlüssel, out var vorhanden))
                    return vorhanden;
                var neu = new Datenbankwaechter(schlüssel, erzeuger);
                _wächter[schlüssel] = neu;
                return neu;
            }
        }

        /// <summary>
        /// Schliesst alle offen gehaltenen Verbindungen und gibt damit die Dateien frei.
        ///
        /// Muss gerufen werden, bevor eine Datenbankdatei kopiert, gepackt, verschoben oder ersetzt
        /// wird. Danach arbeitet alles weiter wie zuvor - die nächste Benutzung öffnet eine neue.
        /// </summary>
        public static void SchliesseAlle() {
            List<Datenbankwaechter> alle;
            lock (_verzeichnisSchloss) {
                alle = [.. _wächter.Values];
            }
            foreach (var wächter in alle)
                wächter.Schliesse();
        }

        /// <summary>
        /// Wieviele Verbindungen gerade offen gehalten werden - für die Prüfung.
        /// </summary>
        public static int AnzahlOffenerVerbindungen {
            get {
                List<Datenbankwaechter> alle;
                lock (_verzeichnisSchloss) {
                    alle = [.. _wächter.Values];
                }
                return alle.Count(wächter => wächter.IstOffen);
            }
        }

        /// <summary>
        /// Hat dieser Wächter gerade eine offene Verbindung?
        /// Fragt ohne das Schloss zu nehmen - für Zählung und Anzeige, nicht für Entscheidungen.
        /// </summary>
        public bool IstOffen {
            get {
                var verbindung = _verbindung;
                return verbindung != null && verbindung.State != ConnectionState.Closed;
            }
        }

        /// <summary>
        /// Leiht die Verbindung aus. Bis zum <see cref="Verlasse"/> gehört sie dem Aufrufer allein.
        /// </summary>
        /// <param name="fehler">die Begründung, falls null zurückkommt</param>
        /// <returns>die offene Verbindung, oder null, wenn sie nicht zu bekommen war</returns>
        public DbConnection? Betrete(out string fehler) {
            fehler = string.Empty;

            if (System.Threading.Monitor.TryEnter(_schloss, Geduld) == false) {
                fehler = $"Die Datenbank {Dateiname} war länger als {Geduld.TotalSeconds:0} Sekunden belegt. "
                    + "Es greifen zwei Abläufe gleichzeitig zu.";
                return null;
            }

            // Ab hier gehört das Schloss diesem Thread. Jeder Ausgang, der nicht die Verbindung
            // liefert, muss es wieder hergeben - sonst bleibt die Datei bis zum Prozessende dicht.
            try {
                var verbindung = _verbindung;

                // Eine kaputte Verbindung wird nicht weiterbenutzt. Sie entsteht, wenn der Treiber
                // unter uns wegbricht; ein Zugriff darauf wirft, und zwar an einer Stelle, die
                // nichts dafür kann.
                if (verbindung != null && verbindung.State == ConnectionState.Broken) {
                    try { verbindung.Dispose(); } catch { /* sie ist schon kaputt */ }
                    verbindung = null;
                    _verbindung = null;
                }

                if (verbindung == null) {
                    verbindung = _erzeuger();
                    _verbindung = verbindung;
                }

                if (verbindung.State != ConnectionState.Open)
                    verbindung.Open();

                return verbindung;
            }
            catch (Exception ex) {
                fehler = $"Die Datenbank {Dateiname} liess sich nicht öffnen: {ex.Message}";
                // Eine Verbindung, die sich nicht öffnen liess, bleibt nicht als Leiche im Wächter
                // liegen - der nächste Versuch soll es mit einer frischen probieren.
                try { _verbindung?.Dispose(); } catch { /* egal */ }
                _verbindung = null;
                System.Threading.Monitor.Exit(_schloss);
                return null;
            }
        }

        /// <summary>
        /// Gibt die Verbindung zurück. <b>Geschlossen wird sie nicht</b> - genau darum geht es.
        /// </summary>
        public void Verlasse() {
            if (System.Threading.Monitor.IsEntered(_schloss))
                System.Threading.Monitor.Exit(_schloss);
        }

        /// <summary>
        /// Schliesst die Verbindung dieses Wächters und gibt die Datei frei.
        ///
        /// Wartet dabei auf das Schloss: sonst schlösse man die Verbindung jemandem unter den
        /// Händen weg, der gerade liest. Ist sie zu lange belegt, wird sie <em>nicht</em>
        /// geschlossen und das gemeldet - die Datei bleibt dann eben gehalten, was unangenehm ist,
        /// aber niemandem den Boden wegzieht.
        /// </summary>
        public void Schliesse() {
            if (System.Threading.Monitor.TryEnter(_schloss, Geduld) == false) {
                ProgramView.LogWarning($"Die Datenbank {Dateiname} liess sich nicht freigeben",
                    "Die Verbindung ist noch in Benutzung. Die Datei bleibt belegt, solange der Zugriff läuft.");
                return;
            }
            try {
                var verbindung = _verbindung;
                _verbindung = null;
                if (verbindung == null)
                    return;
                try {
                    if (verbindung.State != ConnectionState.Closed)
                        verbindung.Close();
                    verbindung.Dispose();
                }
                catch (Exception ex) {
                    ProgramView.LogWarning("Eine Datenbankverbindung liess sich nicht schliessen", ex.Message);
                }
            }
            finally {
                System.Threading.Monitor.Exit(_schloss);
            }
        }

        /// <summary>
        /// Der Dateiname aus dem Schlüssel, für Meldungen. Der Schlüssel selbst ist die
        /// Verbindungszeichenfolge und <b>enthält das Passwort</b> - er darf nirgends in eine
        /// Meldung oder ein Protokoll geraten.
        /// </summary>
        private string Dateiname {
            get {
                // "Data Source=C:\...\Zugdaten.mdb;" - alles zwischen dem Gleichheitszeichen und
                // dem nächsten Semikolon. Steht es anders da, wird nichts geraten.
                const string marke = "Data Source=";
                int anfang = _schlüssel.IndexOf(marke, StringComparison.OrdinalIgnoreCase);
                if (anfang < 0)
                    return "(unbekannt)";
                anfang += marke.Length;
                int ende = _schlüssel.IndexOf(';', anfang);
                string pfad = ende < 0 ? _schlüssel[anfang..] : _schlüssel[anfang..ende];
                pfad = pfad.Trim().Trim('"');
                try { return System.IO.Path.GetFileName(pfad); }
                catch { return "(unbekannt)"; }
            }
        }
    }
}
