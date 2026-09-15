using PhoenixModel.View;
using PhoenixWPF.Helper;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Fängt ab, was sonst niemand auffängt.
    ///
    /// Eine Ausnahme, die bis zur Ereignisschleife durchschlägt, beendet eine WPF-Anwendung ohne
    /// ein Wort - der Anwender sieht das Fenster verschwinden und hat nichts in der Hand. Genau so
    /// ist ein Absturz beim Bauen eines Walls gemeldet worden, und es gab hinterher nichts
    /// nachzulesen.
    ///
    /// Deshalb hier drei Netze:
    ///
    /// * Was auf dem Oberflächenfaden hochkommt, wird gemeldet und abgefangen - die Anwendung
    ///   läuft weiter. Ein misslungener Knopfdruck ist kein Grund, einen halben Spielzug zu
    ///   verlieren.
    /// * Was auf einem anderen Faden hochkommt, lässt sich nicht mehr abfangen. Es wird
    ///   wenigstens berichtet, bevor der Prozess endet.
    /// * Aufgaben, deren Ausnahme nie jemand abgeholt hat, werden ebenfalls gemeldet.
    ///
    /// Alles landet zusätzlich in einer Datei neben den Einstellungen, denn ein Protokoll im
    /// Fenster ist weg, sobald das Fenster weg ist.
    /// </summary>
    public static class Absturzbericht {

        /// <summary>Die Datei, in der die Berichte gesammelt werden</summary>
        public const string Dateiname = "absturz.log";

        private static bool _eingerichtet = false;

        /// <summary>
        /// Hängt die Netze ein. Mehrfaches Aufrufen schadet nicht.
        /// </summary>
        public static void Richte_ein(Application anwendung) {
            if (_eingerichtet)
                return;
            _eingerichtet = true;

            anwendung.DispatcherUnhandledException += AufDerOberfläche;
            AppDomain.CurrentDomain.UnhandledException += AufEinemAnderenFaden;
            TaskScheduler.UnobservedTaskException += InEinerAufgabe;
        }

        /// <summary>
        /// Auf dem Oberflächenfaden: melden und weiterlaufen.
        /// </summary>
        private static void AufDerOberfläche(object sender, DispatcherUnhandledExceptionEventArgs e) {
            Berichte("Ein Fehler in der Bedienung", e.Exception, tödlich: false);
            // Abgefangen - ein misslungener Knopfdruck beendet die Anwendung nicht.
            e.Handled = true;
        }

        /// <summary>
        /// Auf einem anderen Faden: hier ist nichts mehr zu retten, nur noch zu berichten.
        /// </summary>
        private static void AufEinemAnderenFaden(object sender, UnhandledExceptionEventArgs e) {
            Berichte("Ein Fehler ausserhalb der Bedienung", e.ExceptionObject as Exception, tödlich: e.IsTerminating);
        }

        /// <summary>
        /// Eine Aufgabe ist mit einer Ausnahme geendet, die niemand abgeholt hat.
        /// </summary>
        private static void InEinerAufgabe(object? sender, UnobservedTaskExceptionEventArgs e) {
            Berichte("Ein Fehler in einer Hintergrundaufgabe", e.Exception, tödlich: false);
            e.SetObserved();
        }

        /// <summary>
        /// Schreibt den Bericht ins Protokoll, in die Datei und - wenn es ernst ist - auf den Schirm.
        /// </summary>
        public static void Berichte(string überschrift, Exception? fehler, bool tödlich) {
            string text = Beschreibe(fehler);
            SchreibeInDatei(überschrift, text);

            try {
                ProgramView.LogError(überschrift, text);
            }
            catch {
                // Wenn nicht einmal das Protokoll mehr geht, bleibt die Datei.
            }

            try {
                MessageBox.Show(
                    $"{überschrift}.\r\n\r\n{text}\r\n\r\nDer Bericht steht in {Berichtsdatei()}."
                    + (tödlich ? "\r\n\r\nDie Anwendung wird beendet." : "\r\n\r\nDie Anwendung läuft weiter."),
                    tödlich ? "Phoenix wird beendet" : "Phoenix - Fehler",
                    MessageBoxButton.OK, tödlich ? MessageBoxImage.Error : MessageBoxImage.Warning);
            }
            catch {
                // Ohne Oberfläche kein Hinweisfenster - die Datei steht trotzdem.
            }
        }

        /// <summary>
        /// Die Ausnahme in lesbarer Form, samt ihrer Ursachen.
        /// </summary>
        public static string Beschreibe(Exception? fehler) {
            if (fehler == null)
                return "Es ist kein Fehler übergeben worden.";

            var text = new System.Text.StringBuilder();
            for (var aktuell = fehler; aktuell != null; aktuell = aktuell.InnerException) {
                text.AppendLine($"{aktuell.GetType().Name}: {aktuell.Message}");
                if (string.IsNullOrWhiteSpace(aktuell.StackTrace) == false)
                    text.AppendLine(aktuell.StackTrace);
                if (aktuell.InnerException != null)
                    text.AppendLine("--- ausgelöst durch ---");
            }
            return text.ToString().TrimEnd();
        }

        /// <summary>
        /// Wo der Bericht liegt - neben den Einstellungen der Anwendung
        /// </summary>
        public static string Berichtsdatei() {
            try {
                return StorageSystem.AppSettingsFile(Dateiname);
            }
            catch {
                return Path.Combine(Path.GetTempPath(), Dateiname);
            }
        }

        private static void SchreibeInDatei(string überschrift, string text) {
            try {
                File.AppendAllText(Berichtsdatei(),
                    $"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} {überschrift} ==={Environment.NewLine}"
                    + text + Environment.NewLine + Environment.NewLine);
            }
            catch {
                // Wenn sich die Datei nicht schreiben lässt, ist das kein Grund, hier zu scheitern.
            }
        }
    }
}
