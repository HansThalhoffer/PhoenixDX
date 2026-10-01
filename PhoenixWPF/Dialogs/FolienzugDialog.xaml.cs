using System.Windows;
using PhoenixWPF.Program;
using static PhoenixModel.View.KartenanzeigeView;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Welche Gattungen der Folienzug zeigt.
    ///
    /// Die Altanwendung führte dafür ein eigenes Formular mit je Gattung einer Farbe und einem
    /// Schalter - angeschlossen war es nie, und der Folienzug selbst stand im Quelltext als
    /// "wird erstmal nicht mehr unterstützt". Übernommen ist die Aufteilung, nicht der Code; die
    /// Farben liegen fest, weil sie sich vom Gelände abheben müssen und nicht bloss voneinander.
    /// </summary>
    public partial class FolienzugDialog : Window {

        public FolienzugDialog() {
            InitializeComponent();

            var jetzt = Kartenanzeigen.Folienzugeinstellung;
            CbKrieger.IsChecked = jetzt.Krieger;
            CbReiter.IsChecked = jetzt.Reiter;
            CbSchiffe.IsChecked = jetzt.Schiffe;
            CbKreaturen.IsChecked = jetzt.Kreaturen;
            CbZauberer.IsChecked = jetzt.Zauberer;
            CbCharaktere.IsChecked = jetzt.Charaktere;
            CbStartfeld.IsChecked = jetzt.MitStartfeld;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            Kartenanzeigen.Folienzugeinstellung = new Folienzugeinstellung(
                Krieger: CbKrieger.IsChecked == true,
                Reiter: CbReiter.IsChecked == true,
                Schiffe: CbSchiffe.IsChecked == true,
                Kreaturen: CbKreaturen.IsChecked == true,
                Zauberer: CbZauberer.IsChecked == true,
                Charaktere: CbCharaktere.IsChecked == true,
                MitStartfeld: CbStartfeld.IsChecked == true);

            // Liegt der Folienzug gerade auf der Karte, wird er sofort neu gezeichnet.
            if (Kartenanzeigen.Färbung == Kartenanzeigen.Einfärbung.Folienzug)
                Kartenanzeigen.Zeichne();

            DialogResult = true;
            Close();
        }
    }
}
