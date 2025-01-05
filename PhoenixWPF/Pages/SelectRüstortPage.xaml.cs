using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für SelectRüstortPage.xaml
    /// </summary>
    public partial class SelectRüstortPage : Page
    {
        public SelectRüstortPage()
        {
            InitializeComponent();
            ViewModel.OnViewEvent += ViewModel_OnViewEvent;
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (e.EventType == ViewEventArgs.ViewEventType.UpdateEverything && SharedData.Gebäude != null && ViewModel.SelectedNation != null)
            {
                var list = BauwerkeView.GetGebäude(ViewModel.SelectedNation);
                if (list != null) 
                    EigenschaftlerList.AddRange(list);
            }
        }

        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];

        
        public void LoadEigenschaftler()
        {

            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = { "Nation", "Reich", "Nation.Farbname", "Nation.Reich", "Rüstort.Bauwerk", "Rüstort.Nummer"};
            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;
            List<Eigenschaft> columns = eigList.Where(prop => !toIgnore.Contains(prop.Name)).ToList();


            EigenschaftlerDataGrid.Columns.Clear();

            // Add 'Bezeichner' column
            DataGridTextColumn bezeichnerColumn = new DataGridTextColumn
            {
                Header = "Bezeichner",
                Binding = new System.Windows.Data.Binding("Bezeichner")
            };
            EigenschaftlerDataGrid.Columns.Add(bezeichnerColumn);

            // Add dynamic columns for Eigenschaften
            foreach (var eig in columns)
            {
                string name = eig.Name;
                int index = eigList.IndexOf(eig);
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = name,
                    Binding = new System.Windows.Data.Binding($"Eigenschaften[{index}].Wert"),
                    IsReadOnly = name != "Bauwerknamen"
                };
                EigenschaftlerDataGrid.Columns.Add(column);
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEigenschaftler();
        }

        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is Eigenschaft eigenschaft)
                {
                    PropertyProcessor.UpdateSource(eigenschaft);
                }
            }
        }

        /*private void EigenschaftlerDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EigenschaftlerDataGrid.SelectedIndex > -1 && EigenschaftlerDataGrid.SelectedItem != null)
            {
                if (EigenschaftlerDataGrid.SelectedItem is IEigenschaftler eigenschaftler)
                {

                }
            }
            
        }*/

        private void EigenschaftlerDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the position of the mouse relative to the DataGrid
            Point mousePosition = e.GetPosition(EigenschaftlerDataGrid);

            // Hit test to get the element under the mouse
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(EigenschaftlerDataGrid, mousePosition);

            if (hitTestResult != null)
            {
                // Check if the hit test result is a DataGridRow
                DataGridRow? row = GetParentDataGridRow(hitTestResult.VisualHit);
                if (row != null && row.Item != null && row.Item is Gebäude gebäude)
                {
                    // Get the data item corresponding to the row
                    Program.Main.Instance.Map?.Goto(gebäude);

                }
            }
        }

        // Helper method to find the DataGridRow from a Visual element
        private DataGridRow? GetParentDataGridRow(DependencyObject visual)
        {
            // Traverse up the visual tree to find the DataGridRow
            while (visual != null && !(visual is DataGridRow))
            {
                visual = VisualTreeHelper.GetParent(visual);
            }
            return visual as DataGridRow;
        }
    }
}
