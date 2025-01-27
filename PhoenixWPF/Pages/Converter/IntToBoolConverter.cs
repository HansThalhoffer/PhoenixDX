using System.Globalization;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Konverter zur Umwandlung eines Integer-Werts in einen booleschen Wert und umgekehrt.
    /// </summary>
    public class IntToBoolConverter : IValueConverter {
        /// <summary>
        /// Konvertiert eine Ganzzahl in einen booleschen Wert.
        /// </summary>
        /// <param name="value">Der zu konvertierende Integer-Wert.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>True, wenn der Wert 1 ist, andernfalls false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int intValue) {
                return intValue == 1; // 1 -> true, 0 -> false
            }
            return false;
        }

        /// <summary>
        /// Konvertiert einen booleschen Wert in eine Ganzzahl.
        /// </summary>
        /// <param name="value">Der zu konvertierende boolesche Wert.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>1, wenn der Wert true ist, sonst 0.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolValue) {
                return boolValue ? 1 : 0; // true -> 1, false -> 0
            }
            return 0;
        }
    }
}