using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace PhoenixWPF.Helper {
    public class DataGridHelper {
        public static void SetCellValue(DataGrid datagrid, int rowIndex, int columnIndex, string value) {
            var row = datagrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row != null) {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter != null) {
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                    if (cell != null && cell.Content is TextBlock block) {
                        block.Text = value;
                    }
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T foundChild) {
                    return foundChild;
                }
                else {
                    T result = FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
}
