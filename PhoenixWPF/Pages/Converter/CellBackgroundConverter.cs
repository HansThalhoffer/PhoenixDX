using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Ein Konverter zur Bestimmung der Hintergrundfarbe einer DataGrid-Zelle basierend auf deren Inhalt.
    /// </summary>
    public class CellBackgroundConverter : IMultiValueConverter {
        /// <summary>
        /// Konvertiert mehrere Werte in eine Hintergrundfarbe für eine DataGrid-Zelle.
        /// </summary>
        /// <param name="values">Ein Array mit Werten: das Zellobjekt und die DataGridCell.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Eine Brush-Instanz zur Darstellung der Hintergrundfarbe.</returns>
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Brushes.Transparent;

            if (values[1] is DataGridCell cell) {
                // Falls die Zelle ausgewählt ist, wird die Hintergrundfarbe durch den Style-Trigger gesteuert
                if (cell.IsSelected) {
                    return null;
                }

                if (values[0] is NamensSpielfigur data) {
                    if (cell.Column.Header != null && (cell.Column.Header.ToString() == "Spielername" || cell.Column.Header.ToString() == "Charaktername" || cell.Column.Header.ToString() == "Beschriftung")) {
                        return Brushes.Black;
                    }
                }
                if (values[0] is ReichCrossref diplomatie) {
                    if (ProgramView.SelectedNation != null && diplomatie.ReferenzNation == ProgramView.SelectedNation) {
                        return Brushes.Black;
                    }
                }
                if (values[0] is Gebäude gebäude) {
                    if (cell.Column != null && cell.Column.Header != null && cell.Column.Header.ToString() == "Bauwerknamen") {
                        return Brushes.Black;
                    }
                }
            }

            return Brushes.Transparent; // Standardhintergrundfarbe
        }

        /// <summary>
        /// Die Rückkonvertierung ist nicht implementiert.
        /// </summary>
        /// <param name="value">Der zu konvertierende Wert.</param>
        /// <param name="targetTypes">Die Zieltypen der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Wirft eine NotImplementedException.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
