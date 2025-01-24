using PhoenixModel.ViewModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
                // Check if the column is bound to an integer property
                if (e.PropertyType == typeof(int) || e.PropertyType == typeof(long)) {
                    // Create a new binding with formatting
                    var binding = new Binding(e.PropertyName) {
                        StringFormat = "N0", // German thousands separator (e.g., 1.234.567)
                        ConverterCulture = new CultureInfo("de-DE") // Ensure proper localization
                    };

                    // Apply binding to the column
                    if (e.Column is DataGridTextColumn textColumn) {
                        textColumn.Binding = binding;
                        // Right-align cell content
                        Style style = new Style(typeof(TextBlock));
                        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                        textColumn.ElementStyle = style;
                    }
                    
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
