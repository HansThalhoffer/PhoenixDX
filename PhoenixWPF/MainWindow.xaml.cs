using System.Windows;
using System.Windows.Controls;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.View;
using PhoenixWPF.Database;
using PhoenixWPF.Database.Generatoren;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;

namespace PhoenixWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Closing += OnClosing; ;
            ProgramView.OnViewEvent += ViewModel_OnViewEvent;
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
            ProgramView.OnViewEvent -= ViewModel_OnViewEvent;
            Main.Instance.StopInstance();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Main.Instance.StartInstance();
            AktualisiereTitel();
            this.Loaded -= OnLoaded;
        }

        /// <summary>
        /// Der Fenstertitel zeigt den Spielstand: Reich, Zug, Monat und Phase. Er wird
        /// nachgezogen, wenn Daten geladen wurden oder sich im Grossen etwas geaendert hat -
        /// etwa beim Phasenwechsel.
        /// </summary>
        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e) {
            if (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded
             || e.EventType == ViewEventArgs.ViewEventType.UpdateEverything)
                AktualisiereTitel();
        }

        /// <summary>
        /// Setzt den Fenstertitel neu.
        ///
        /// Die Ereignisse kommen aus dem Modell und damit oft von einem Ladethread; der Titel
        /// gehoert aber dem Fenster. Deshalb der Umweg ueber den Dispatcher.
        /// </summary>
        private void AktualisiereTitel() {
            if (Dispatcher.CheckAccess())
                Title = ZugView.Titelzeile;
            else
                Dispatcher.BeginInvoke(new Action(AktualisiereTitel));
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? tag = menuItem.Tag as string;

                // Show the corresponding tab
                if (FindName(tag) is TabItem tabItem)
                {
                    tabItem.Visibility = Visibility.Visible;
                }
            }
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? tag = menuItem.Tag as string;

                // Hide the corresponding tab
                if (FindName(tag) is TabItem tabItem)
                {
                    tabItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        
        /// <summary>
        /// Zeigt im Zug-Menü an, in welcher Phase der Zug gerade ist, und bietet den Wechsel nur an,
        /// wenn er auch möglich ist.
        /// </summary>
        private void ZugMenu_SubmenuOpened(object sender, RoutedEventArgs e) {
            var zug = ZugView.AktuellerZug;
            MenuPhaseAnzeige.Header = $"Zug {zug.Zug} - {zug.Beschreibung} - {ZugView.PhasenBeschreibung}";
            MenuNaechstePhase.IsEnabled = ZugView.Phase == Zugphase.Rüstphase;
            MenuZugAbgeben.IsEnabled = ZugView.Phase != Zugphase.Abgeschlossen;
        }

        /// <summary>
        /// Gibt den Zug ab: der laufende Zug wird abgeschlossen und die Datenbank des Folgezuges
        /// angelegt. Vorher bekommt der Benutzer zu sehen, was das mit seinen Figuren macht.
        /// </summary>
        private void ZugAbgeben() {
            var bericht = ZugabgabeView.ErstelleBericht();
            if (bericht.KannAbgegebenWerden == false) {
                SpielWPF.LogError("Der Zug kann nicht abgegeben werden", string.Join(" ", bericht.Hindernisse));
                MessageBox.Show(bericht.Zusammenfassung, "Zugabgabe nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var antwort = MessageBox.Show(
                bericht.Zusammenfassung + "\r\nDen Zug jetzt abgeben? Danach lässt sich in diesem Zug nichts mehr ändern.",
                "Zug abgeben", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (antwort != MessageBoxResult.Yes)
                return;

            var einstellungen = Main.Instance.Settings.UserSettings;
            string quelle = einstellungen.DatabaseLocationZugdaten;
            int zug = bericht.AktuellerZug.Zug;

            bool überschreiben = false;
            if (Zugabgabe.ZielExistiert(quelle, bericht.NächsterZug.Zug)) {
                var nachfrage = MessageBox.Show(
                    $"Für Zug {bericht.NächsterZug.Zug} gibt es bereits Zugdaten.\r\n\r\n"
                    + "Wenn du den Zug erneut abgibst, geht alles verloren, was in diesem Folgezug schon gemacht wurde. Wirklich überschreiben?",
                    "Zugdaten überschreiben", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (nachfrage != MessageBoxResult.Yes)
                    return;
                überschreiben = true;
            }

            // alles Offene wegschreiben, sonst fehlt es in der Kopie
            Main.Instance.SpeichereJetzt();

            var ergebnis = Zugabgabe.Durchführen(quelle, einstellungen.PasswordReich, zug, überschreiben);
            if (ergebnis.Erfolgreich == false) {
                SpielWPF.LogError(ergebnis.Meldung, ergebnis.Details);
                MessageBox.Show($"{ergebnis.Meldung}\r\n\r\n{ergebnis.Details}", "Zugabgabe fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SpielWPF.LogInfo(ergebnis.Meldung, ergebnis.Details);
            MessageBox.Show(
                $"{ergebnis.Details}\r\n\r\n"
                + $"Übernommen wurden {ergebnis.ÜbernommeneFiguren} Figuren"
                + (ergebnis.AufgelösteFiguren > 0 ? $", {ergebnis.AufgelösteFiguren} sind aufgelöst worden" : string.Empty) + ".\r\n"
                + $"Der Reichsschatz für Zug {bericht.NächsterZug.Zug} beträgt {ergebnis.NeuerReichsschatz} GS.\r\n\r\n"
                + $"Für die Spielleitung liegt bereit:\r\n{ergebnis.ÜbergabeArchiv}\r\n\r\n"
                + "Über \"Extras / Zug Wechseln\" geht es in den neuen Zug.",
                "Zug abgegeben", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Holt die von der Spielleitung ausgewertete Zugdatenbank.
        ///
        /// Der Normalfall ist die Netzwerkfreigabe. Ist sie nicht eingetragen oder der Server nicht
        /// erreichbar - auf dem Gelände kommt das vor - wird nach einem Verzeichnis oder Archiv
        /// gefragt, das die Spielleitung per Datenträger herausgegeben hat.
        /// </summary>
        private void ZugHolen() {
            var einstellungen = Main.Instance.Settings.UserSettings;
            int zug = ZugView.AktuellerZug.Zug;
            string ziel = einstellungen.DatabaseLocationZugdaten;
            if (string.IsNullOrEmpty(ziel)) {
                SpielWPF.LogError("Es ist kein Reich geladen", "Ohne geladene Zugdaten ist nicht bekannt, wohin der Zug gehört.");
                return;
            }
            string reich = System.IO.Path.GetFileNameWithoutExtension(ziel);

            string? quelle = BestimmeQuelleFürRückgabe(einstellungen.ServerFreigabe, reich, zug);
            if (quelle == null)
                return;

            if (System.IO.File.Exists(ziel)) {
                var nachfrage = MessageBox.Show(
                    $"Die Zugdaten für Zug {zug} liegen lokal bereits.\r\n\r\n"
                    + "Wenn du sie durch die Fassung der Spielleitung ersetzt, geht alles verloren, was du in diesem Zug schon gemacht hast. Wirklich ersetzen?",
                    "Zugdaten ersetzen", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (nachfrage != MessageBoxResult.Yes)
                    return;
            }

            // vor dem Überschreiben alles Offene wegschreiben, sonst geht es beim Ersetzen verloren
            Main.Instance.SpeichereJetzt();

            var ergebnis = quelle.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? SpielleitungsRückgabe.PackeArchivAus(quelle, reich, ziel, überschreiben: true)
                : SpielleitungsRückgabe.HoleZug(quelle, reich, zug, ziel, überschreiben: true);

            if (ergebnis.Erfolgreich == false) {
                SpielWPF.LogError(ergebnis.Meldung, ergebnis.Details);
                MessageBox.Show($"{ergebnis.Meldung}\r\n\r\n{ergebnis.Details}", "Der Zug kam nicht zurück", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SpielWPF.LogInfo(ergebnis.Meldung, ergebnis.Details);
            MessageBox.Show($"{ergebnis.Details}\r\n\r\nÜber \"Extras / Zug Wechseln\" werden die neuen Daten geladen.",
                ergebnis.Meldung, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Bestimmt, woher der Zug geholt wird: vom Server, oder - wenn der nicht mitspielt - aus
        /// einem Archiv, das der Benutzer aussucht.
        /// </summary>
        /// <returns>ein Verzeichnis, ein Archiv, oder null wenn abgebrochen wurde</returns>
        private static string? BestimmeQuelleFürRückgabe(string freigabe, string reich, int zug) {
            if (string.IsNullOrWhiteSpace(freigabe) == false) {
                if (SpielleitungsRückgabe.IstErreichbar(freigabe, out string fehler)) {
                    string verzeichnis = SpielleitungsRückgabe.BestimmeSerververzeichnis(freigabe, reich, zug);
                    if (System.IO.Directory.Exists(verzeichnis))
                        return verzeichnis;
                    SpielWPF.LogWarning($"Auf dem Server gibt es {verzeichnis} nicht",
                        "Hat die Spielleitung den Zug schon abgelegt? Andernfalls kann er auch aus einem Archiv geholt werden.");
                }
                else {
                    SpielWPF.LogWarning("Der Server der Spielleitung ist nicht erreichbar", fehler);
                }
            }

            var antwort = MessageBox.Show(
                string.IsNullOrWhiteSpace(freigabe)
                    ? "Es ist keine Netzwerkfreigabe der Spielleitung eingetragen (Einstellungen).\r\n\r\nStattdessen ein Archiv von der Spielleitung auswählen?"
                    : "Der Zug liess sich nicht über das Netz holen.\r\n\r\nStattdessen ein Archiv von der Spielleitung auswählen?",
                "Zug holen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (antwort != MessageBoxResult.Yes)
                return null;

            var dialog = new Microsoft.Win32.OpenFileDialog {
                Title = $"Archiv der Spielleitung für {reich}, Zug {zug}",
                Filter = "Archive der Spielleitung (*.zip)|*.zip|Zugdatenbank (*.mdb)|*.mdb|Alle Dateien (*.*)|*.*",
                CheckFileExists = true,
            };
            if (dialog.ShowDialog() != true)
                return null;

            // eine ausgewählte Datenbank wird über ihr Verzeichnis geholt, ein Archiv direkt
            return dialog.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? dialog.FileName
                : System.IO.Path.GetDirectoryName(dialog.FileName);
        }

        /// <summary>
        /// Beendet die Rüstphase. Danach kann nicht mehr gerüstet, dafür aber bewegt werden,
        /// deshalb wird vorher nachgefragt.
        /// </summary>
        private void NächstePhase() {
            if (ZugView.Phase != Zugphase.Rüstphase) {
                var abgelehnt = ZugView.NächstePhase();
                SpielWPF.LogWarning(abgelehnt.Title, abgelehnt.Message);
                return;
            }

            var antwort = MessageBox.Show(
                "Die Rüstphase wirklich beenden?\r\n\r\nDanach kann in diesem Zug nicht mehr gerüstet oder gebaut werden. Zurück geht es nicht.",
                "Rüstphase beenden", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (antwort != MessageBoxResult.Yes)
                return;

            var ergebnis = ZugView.NächstePhase();
            if (ergebnis.HasErrors)
                SpielWPF.LogError(ergebnis.Title, ergebnis.Message);
            else
                SpielWPF.LogInfo(ergebnis.Title, ergebnis.Message);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? tag = menuItem.Tag as string;
                if (tag == null)
                    return;
                switch (tag)
                {
                    // Militär
                    case "Truppen":
                        new TruppenEntwicklungDialog().Show("Status");
                        break;
                    case "TruppenEntwicklung":
                        new TruppenEntwicklungDialog().Show("Entwicklung");
                        break;
                    case "Mobilisierung":
                        new TruppenEntwicklungDialog().Show("Mobilisierung");
                        break;
                    // Hofhaltung
                    case "LehenVerwalten":
                        new LehenDialog().Show("LehenVerwalten");
                        break;
                    case "LehenAnlegen":
                        new LehenDialog().Show("LehenAnlegen");
                        break;

                    // Schatzkammer
                    case "ErwarteteEinkommen":
                        new SchatzkammerDialog().Show("ErwarteteEinkommen");
                        break;
                    case "Entwicklung":
                        new SchatzkammerDialog().Show("Entwicklung");
                        break;
                    case "Baukosten":
                        new SchatzkammerDialog().Show("Baukosten");
                        break;
                    case "Schenkungen":
                        new SchatzkammerDialog().Show("Schenkungen");
                        break;
                    case "Schenken":
                        new SchatzkammerDialog().Show("Schenken");
                        break;
                        

                    // Zug
                    case "NaechstePhase":
                        NächstePhase();
                        break;
                    case "ZugAbgeben":
                        ZugAbgeben();
                        break;
                    case "ZugHolen":
                        ZugHolen();
                        break;
                    case "Zugreihenfolge":
                        new ZugreihenfolgeDialog().Show();
                        break;

                    // Spielleitung
                    case "Kampfauswertung":
                        new KampfauswertungDialog().Show();
                        break;

                    // Nachschlagewerk
                    case "Bestiarium":
                        new NachschlagewerkDialog().Show("Bestiarium");
                        break;
                    case "Personal":
                        new NachschlagewerkDialog().Show("Personal");
                        break;

                    // Extras
                    case "Zugwechsel":
                        Main.Instance.Zugwechsel();
                        break;
                    case "InstallUSB":
                        Main.Instance.CreateInstallUSBStick();
                        break;
                    case "Zug 999 (Testdaten)":
                        TestDataGenerator.GeneriereTestdatenFürZug999();
                        break;

                }
            }
        }
    }
}
