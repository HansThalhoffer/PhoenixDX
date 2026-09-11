using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Auswahl des Feldes, auf dem eine Figur nach einem Teleport erscheint.
    ///
    /// Der Auftauchpunkt selbst wird laut Regelwerk 6.6.3 von der Spielleitung vier Züge im Voraus
    /// ausgewürfelt und bekanntgegeben - die Anwendung würfelt ihn nicht, sondern lässt den
    /// vorgegebenen Punkt auswählen. Auf welchem der sechs Felder ringsherum die Figur dann
    /// erscheint, sucht sich der Spieler aus.
    /// </summary>
    public partial class TeleportDialog : Window {

        /// <summary>
        /// Das gewählte Feld, auf dem die Figur erscheint, oder null bei Abbruch
        /// </summary>
        public KleinfeldPosition? Auftauchfeld { get; private set; } = null;

        public TeleportDialog(Spielfigur figur, KleinFeld teleportfeld) {
            InitializeComponent();
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;

            var art = TeleportRules.GetArt(teleportfeld);
            PromptLabel.Text = $"{figur.Bezeichner} betritt das Teleportfeld {teleportfeld.CreateBezeichner()} ({art}). "
                + "Auf einem Teleportfeld bleibt niemand stehen - bitte auswählen, wo die Figur wieder erscheint.";

            HinweisLabel.Text = TeleportRules.KostetAuftauchen(art)
                ? "Das Auftauchen zählt als weiteres Wasserfeld und kostet zusätzliche Bewegungspunkte (Regelwerk 6.6.4)."
                : "Das Auftauchen selbst kostet keine weiteren Bewegungspunkte (Regelwerk 6.6.5).";

            foreach (var punkt in TeleportRules.GetMöglicheZiele(teleportfeld))
                PunkteListe.Items.Add(punkt);

            if (PunkteListe.Items.Count == 1)
                PunkteListe.SelectedIndex = 0;
        }

        private void PunkteListe_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            FelderListe.Items.Clear();
            OkButton.IsEnabled = false;
            if (PunkteListe.SelectedItem is not Teleportfeld punkt)
                return;
            foreach (var feld in TeleportRules.GetAuftauchfelder(punkt.Position))
                FelderListe.Items.Add(feld);
            if (FelderListe.Items.Count == 1)
                FelderListe.SelectedIndex = 0;
        }

        private void FelderListe_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            OkButton.IsEnabled = FelderListe.SelectedItem is KleinFeld;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (FelderListe.SelectedItem is not KleinFeld feld)
                return;
            Auftauchfeld = new KleinfeldPosition(feld.gf, feld.kf);
            DialogResult = true;
            Close();
        }
    }
}
