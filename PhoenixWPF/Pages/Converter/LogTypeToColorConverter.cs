using PhoenixModel.Program;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter
{
    public class LogTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogEntry.LogType logType)
            {
                switch (logType)
                {
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
