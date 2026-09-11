using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Windows;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Zeigt die von der Spielleitung bekanntgegebene Zugreihenfolge eines Monats und den dazu
    /// ausgewürfelten Auftauchpunkt (Regelwerk 6.6.3).
    ///
    /// Die Tabelle steht in der Kartendatenbank und enthält auch die kommenden Monate, deshalb
    /// lässt sich vor- und zurückblättern.
    /// </summary>
    public partial class ZugreihenfolgeDialog : Window {

        private int _zug;

        public ZugreihenfolgeDialog() {
            InitializeComponent();
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;
            _zug = ZugView.AktuellerZug.Zug;
            Zeige();
        }

        private void Zeige() {
            var monat = new Zugmonat(_zug);
            MonatLabel.Text = monat.ToString();

            var reihenfolge = ZugView.GetZugreihenfolge(_zug);
            ReihenfolgeGrid.ItemsSource = reihenfolge;

            if (reihenfolge.Count == 0) {
                AuftauchpunktLabel.Text = "Für diesen Zug ist keine Zugreihenfolge eingetragen.";
                return;
            }

            var punkt = ZugView.GetAuftauchpunkt(_zug);
            if (punkt == null) {
                AuftauchpunktLabel.Text = "Für diesen Zug ist kein Auftauchpunkt eingetragen.";
                return;
            }
            var art = TeleportRules.GetArt(punkt);
            AuftauchpunktLabel.Text = $"Auftauchpunkt für die Reise von der Pirateninsel nach Erkenfara: "
                + $"{punkt.CreateBezeichner()} ({art}). Er gilt für den ganzen Monat und wird von der Spielleitung vier Züge im Voraus ausgewürfelt.";
        }

        private void VorherButton_Click(object sender, RoutedEventArgs e) {
            if (_zug <= 1)
                return;
            _zug--;
            Zeige();
        }

        private void NachherButton_Click(object sender, RoutedEventArgs e) {
            _zug++;
            Zeige();
        }
    }
}
