using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter
{
    public class EditableToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditable)
            {
                return isEditable ? Brushes.Black : new SolidColorBrush(Color.FromRgb(32,32,32));
            }

            return Brushes.Transparent; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
