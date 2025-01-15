using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using System.Globalization;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter {
    public class AutorizedConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReichCrossref diplomatie)
            {
                if (ProgramView.SelectedNation != null && diplomatie.ReferenzNation == ProgramView.SelectedNation)
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
