using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Windows;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Zeigt, was über eine Gemark bekannt ist: die Eigenschaften des Kleinfeldes und die Figuren,
    /// die darauf stehen.
    ///
    /// Was hier zu sehen ist, hängt an der Sichtbarkeit: fremde Figuren stehen nur in der Liste,
    /// soweit sie die Feindaufklärung aufgedeckt hat. Der Dialog zeigt also nicht die Wahrheit,
    /// sondern den eigenen Kenntnisstand - und sagt das auch.
    /// </summary>
    public partial class GemarkInfoDialog : Window {

        /// <summary>Eine Zeile der Figurenliste</summary>
        private class Figurenzeile {
            public string Reich { get; init; } = string.Empty;
            public int Nummer { get; init; }
            public string Art { get; init; } = string.Empty;
            public string Stärke { get; init; } = string.Empty;
        }

        public GemarkInfoDialog(KleinFeld gemark) {
            InitializeComponent();
            if (Application.Current?.MainWindow != null && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;

            Title = $"Gemark {gemark.Bezeichner}";
            KopfLabel.Text = $"{gemark.Bezeichner} - {gemark.TerrainType}"
                           + (string.IsNullOrWhiteSpace(gemark.Bauwerknamen) ? string.Empty : $" - {gemark.Bauwerknamen}");

            string reich = gemark.Nation?.Reich ?? "niemandem";
            string eigen = gemark.Nation == ProgramView.SelectedNation ? " (eigenes Reich)" : string.Empty;
            UnterLabel.Text = $"Gehört {reich}{eigen}.";

            EigenschaftenGrid.ItemsSource = gemark.Eigenschaften;

            var figuren = SpielfigurenView.GetSpielfiguren(gemark);
            FigurenGrid.ItemsSource = figuren
                .Select(figur => new Figurenzeile {
                    Reich = figur.Nation?.Reich ?? string.Empty,
                    Nummer = figur.Nummer,
                    Art = figur.Typ.ToString(),
                    Stärke = figur.Stärke,
                })
                .OrderBy(zeile => zeile.Reich)
                .ThenBy(zeile => zeile.Nummer)
                .ToList();

            FigurenLabel.Text = figuren.Count == 0
                ? "Auf dieser Gemark steht nichts, was bekannt wäre."
                : $"{figuren.Count} Figuren auf dieser Gemark - eigene vollständig, fremde nur soweit aufgeklärt.";
        }

        private void SchliessenButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
