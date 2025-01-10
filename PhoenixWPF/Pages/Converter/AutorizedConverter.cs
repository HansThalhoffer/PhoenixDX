using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PhoenixWPF.Pages.Converter
{
    public class AutorizedConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReichCrossref diplomatie)
            {
                if (ViewModel.SelectedNation != null && diplomatie.Nation == ViewModel.SelectedNation)
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
