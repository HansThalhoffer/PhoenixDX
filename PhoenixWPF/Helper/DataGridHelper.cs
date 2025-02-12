using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

/// <summary>
/// Hilfsklasse zur Manipulation von DataGrid-Zellen in einer WPF-Anwendung.
/// </summary>
public class DataGridHelper {
    /// <summary>
    /// Setzt den Wert einer bestimmten Zelle in einem DataGrid.
    /// </summary>
    /// <param name="datagrid">Das DataGrid, in dem der Wert gesetzt werden soll.</param>
    /// <param name="rowIndex">Der Index der Zeile.</param>
    /// <param name="columnIndex">Der Index der Spalte.</param>
    /// <param name="value">Der neue Wert, der in die Zelle eingefügt wird.</param>
    public static void SetCellValue(DataGrid datagrid, int rowIndex, int columnIndex, string value) {
        var row = datagrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
        if (row != null) {
            DataGridCellsPresenter? presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter != null) {
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell != null && cell.Content is TextBlock block) {
                    block.Text = value;
                }
            }
        }
    }

    /// <summary>
    /// Findet ein untergeordnetes visuelles Element eines bestimmten Typs in der Hierarchie eines übergeordneten Elements.
    /// </summary>
    /// <typeparam name="T">Der Typ des gesuchten visuellen Elements.</typeparam>
    /// <param name="parent">Das übergeordnete DependencyObject, in dem gesucht wird.</param>
    /// <returns>Das gefundene untergeordnete Element oder null, wenn kein Element gefunden wurde.</returns>
    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T foundChild) {
                return foundChild;
            }
            else {
                T? result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
        }
        return null;
    }
}
