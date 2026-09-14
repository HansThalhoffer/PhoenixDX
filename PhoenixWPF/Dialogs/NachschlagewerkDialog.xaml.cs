using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixWPF.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Zwei Listen zum Nachschlagen: das Bestiarium der Karte und die Personalliste des Reiches.
    ///
    /// Das Bestiarium beschreibt die Kreaturen Erkenfaras (Regelwerk 6.3) und gilt für alle
    /// Reiche; es steht in der Kartendatenbank. Die Personalliste steht in den Zugdaten des
    /// eigenen Reiches.
    ///
    /// Beide Tabellen lagen im Datenmodell, wurden aber nie geladen. Jetzt werden sie es - und
    /// hier sind sie zu sehen.
    ///
    /// Zur Personalliste gehört ein Hinweis, der im Fenster steht: darin stehen Namen,
    /// Anschriften, Telefonnummern und Mailadressen wirklicher Menschen.
    /// </summary>
    public partial class NachschlagewerkDialog : Window {

        public NachschlagewerkDialog() {
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;
            InitializeComponent();
            Bestiarium.Navigated += Bestiarium_Navigated;
            Personal.Navigated += Personal_Navigated;
            Tabulator.SelectionChanged += Tabulator_SelectionChanged;
        }

        private void Bestiarium_Navigated(object sender, NavigationEventArgs e) {
            if (Bestiarium.Content is EigenschaftlerListGridPage seite)
                seite.EigenschaftlerList = NachschlagewerkView.AlsEigenschaftler(NachschlagewerkView.GetKreaturen());
        }

        private void Personal_Navigated(object sender, NavigationEventArgs e) {
            if (Personal.Content is EigenschaftlerListGridPage seite)
                seite.EigenschaftlerList = NachschlagewerkView.AlsEigenschaftler(NachschlagewerkView.GetPersonal());
        }

        /// <summary>
        /// Der Hinweis zur Personalliste steht nur, solange sie zu sehen ist.
        /// </summary>
        private void Tabulator_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bool personal = Tabulator.SelectedItem is TabItem tab && (tab.Tag as string) == "Personal";
            HinweisLabel.Text = personal
                ? "Diese Liste enthält Namen, Anschriften, Telefonnummern und Mailadressen wirklicher "
                  + "Menschen. Bitte nicht in Meldungen, Zwischenablagen oder Fehlerberichte übernehmen."
                : string.Empty;
        }

        /// <summary>
        /// Öffnet das Fenster auf dem gewünschten Reiter
        /// </summary>
        public void Show(string seite) {
            Loaded += (s, e) => {
                foreach (TabItem tab in Tabulator.Items) {
                    if (tab?.Tag?.ToString() == seite)
                        Tabulator.SelectedItem = tab;
                }
            };
            Show();
        }
    }
}
