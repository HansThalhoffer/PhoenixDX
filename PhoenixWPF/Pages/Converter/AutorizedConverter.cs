using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using System.Globalization;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter {
    /// <summary>
    /// Konverter zur Überprüfung der Autorisierung basierend auf der Diplomatie eines Reiches.
    /// </summary>
    public class AutorizedConverter : IValueConverter {
        /// <summary>
        /// Konvertiert ein ReichCrossref-Objekt in einen booleschen Wert, um die Autorisierung zu bestimmen.
        /// </summary>
        /// <param name="value">Das zu konvertierende Objekt.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Ein optionaler Parameter.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>True, wenn die ausgewählte Nation mit der Referenznation übereinstimmt, sonst false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is ReichCrossref diplomatie) {
                if (ProgramView.SelectedNation != null && diplomatie.ReferenzNation == ProgramView.SelectedNation) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Die Rückkonvertierung ist nicht implementiert.
        /// </summary>
        /// <param name="value">Der zu konvertierende Wert.</param>
        /// <param name="targetType">Der Zieltyp der Konvertierung.</param>
        /// <param name="parameter">Ein optionaler Parameter.</param>
        /// <param name="culture">Die aktuelle Kultur.</param>
        /// <returns>Wirft eine NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}