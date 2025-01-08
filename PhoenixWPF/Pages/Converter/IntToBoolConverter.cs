using System.Globalization;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 1; // 1 -> true, 0 -> false
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0; // true -> 1, false -> 0
            }
            return 0;
        }
    }
}
