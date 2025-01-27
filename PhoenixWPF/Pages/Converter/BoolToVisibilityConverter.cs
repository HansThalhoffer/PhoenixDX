using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Konverter zur Umwandlung eines booleschen Wertes in eine Visibility-Enumeration.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter {
        /// <summary>
        /// Konvertiert einen booleschen Wert in eine Visibility-Enumeration.
        /// </summary>
        /// <param name="value">Der boolesche Wert.</param>
        /// <param name="targetType">Der Zieltyp (nicht verwendet).</param>
        /// <param name="parameter">Zusätzlicher Parameter (nicht verwendet).</param>
        /// <param name="culture">Die Kulturinformationen (nicht verwendet).</param>
        /// <returns>Visibility.Visible, wenn der Wert true ist, sonst Visibility.Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool booleanValue)
                return booleanValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Konvertiert eine Visibility-Enumeration zurück in einen booleschen Wert.
        /// </summary>
        /// <param name="value">Die Visibility-Enumeration.</param>
        /// <param name="targetType">Der Zieltyp (nicht verwendet).</param>
        /// <param name="parameter">Zusätzlicher Parameter (nicht verwendet).</param>
        /// <param name="culture">Die Kulturinformationen (nicht verwendet).</param>
        /// <returns>True, wenn Visibility.Visible, sonst false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value is Visibility visibility) && (visibility == Visibility.Visible);
        }
    }
}