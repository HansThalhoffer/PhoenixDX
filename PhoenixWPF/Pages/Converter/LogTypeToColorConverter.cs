using PhoenixModel.Program;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Konverter zur Umwandlung eines Log-Typs in eine entsprechende Hintergrundfarbe.
    /// </summary>
    public class LogTypeToColorConverter : IValueConverter {
        /// <summary>
        /// Konvertiert einen Log-Typ in eine entsprechende Hintergrundfarbe.
        /// </summary>
        /// <param name="value">Der Log-Typ, der konvertiert werden soll.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Zusätzliche Parameter für die Konvertierung.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Eine passende Brush-Farbe für den Log-Typ.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is LogEntry.LogType logType) {
                switch (logType) {
                    case LogEntry.LogType.Error:
                        return Brushes.DarkRed;
                    case LogEntry.LogType.Warning:
                        return Brushes.DarkBlue;
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
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