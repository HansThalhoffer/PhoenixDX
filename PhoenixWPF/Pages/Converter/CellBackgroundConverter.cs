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
using System.Windows.Media;

namespace PhoenixWPF.Pages.Converter
{
    public class CellBackgroundConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null | values[1] == null )
                return Brushes.Transparent;

            if (values[1] is DataGridCell cell)
            {
                // If the cell is selected, return null to let the style trigger handle the background
                if (cell.IsSelected)
                {
                    return null;
                }

                if (values[0] is NamensSpielfigur data)
                {
                    if (cell.Column.Header != null && (cell.Column.Header.ToString() == "Spielername" || cell.Column.Header.ToString() == "Charaktername" || cell.Column.Header.ToString() == "Beschriftung"))
                    {
                        return Brushes.Black;
                    }
                }
                if (values[0] is ReichCrossref diplomatie)
                {
                    if (ViewModel.SelectedNation != null && diplomatie.Referenzreich == ViewModel.SelectedNation.Reich)
                    {
                        return Brushes.Black;
                    }
                }
                if (values[0] is Gebäude gebäude)
                {
                    if (cell.Column != null && cell.Column.Header != null && cell.Column.Header.ToString() == "Bauwerknamen")
                    {
                        return Brushes.Black;
                    }
                }
            }

            return Brushes.Transparent; // Default background
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
