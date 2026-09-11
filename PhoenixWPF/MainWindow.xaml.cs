using System.Windows;
using System.Windows.Controls;
using PhoenixModel.View;
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
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
            Main.Instance.StopInstance();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Main.Instance.StartInstance();
            this.Loaded -= OnLoaded;
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
                    case "Zugreihenfolge":
                        new ZugreihenfolgeDialog().Show();
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
