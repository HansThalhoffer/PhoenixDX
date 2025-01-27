using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Konverter zur Änderung der Hintergrundfarbe basierend auf der Bearbeitbarkeit eines Elements.
    /// </summary>
    public class EditableToBackgroundConverter : IValueConverter {
        /// <summary>
        /// Konvertiert einen booleschen Wert in eine Hintergrundfarbe.
        /// </summary>
        /// <param name="value">Der boolesche Wert, der angibt, ob das Element bearbeitbar ist.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Schwarze Farbe, wenn das Element bearbeitbar ist, ansonsten ein dunkler Grauton.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool isEditable) {
                return isEditable ? Brushes.Black : new SolidColorBrush(Color.FromRgb(32, 32, 32));
            }

            return Brushes.Transparent; // Standardfarbe
        }

        /// <summary>
        /// Die Rückkonvertierung ist nicht implementiert.
        /// </summary>
        /// <param name="value">Der zu konvertierende Wert.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Wirft eine NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
