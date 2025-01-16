using PhoenixModel.ViewModel;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {

    /// <summary>
    /// Interaktionslogik für ExpectedIncomePage.xaml
    /// </summary>
    public partial class ExpectedIncomePage : Page {
        List<ExpectedIncome> expectedIncomes = [] ;

        /// <summary>
        /// konstruktor
        /// </summary>
        public ExpectedIncomePage() {
            InitializeComponent();
            LoadData();
            EinkommenDataGrid.AutoGeneratingColumn += EinkommenDataGrid_AutoGeneratingColumn;
        }

        private void EinkommenDataGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e) {
            if (e.Column != null) {
                string? header = e.Column.Header.ToString();
                if (string.IsNullOrEmpty(header) == false) {
                    header = header.Replace("Felder", "");
                    header = header.Replace("Einkommen", " Gs");
                    e.Column.Header = header;
                }
            }
        }

        /// <summary>
        /// Daten laden
        /// </summary>
        private void LoadData() {
            if (SharedData.Nationen == null) {
                return;
            }
            foreach (var data in SharedData.Nationen) {
                expectedIncomes.Add(new ExpectedIncome(data));
            }
            EinkommenDataGrid.ItemsSource = expectedIncomes;
        }
    }
}
