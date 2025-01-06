using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
            if (SharedData.Gebäude != null && ViewModel.SelectedNation != null && 
                (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded || e.EventType == ViewEventArgs.ViewEventType.UpdateGebäude ))
            {
                EigenschaftlerList.Clear();
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
            string[] toIgnore = { "Nation", "Reich", "Nation.Farbname", "Nation.Reich", "Rüstort.Bauwerk", "Rüstort.Nummer", "Zerstört" , "InBau", "IsNew"};
            // string[] toIgnore = { };
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
                if (name == "Bauwerknamen")
                {
                    _BauwerkNamenBindingPath = $"Eigenschaften[{index}].Wert";
                }
               EigenschaftlerDataGrid.Columns.Add(column);
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }

        private string _BauwerkNamenBindingPath = string.Empty;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEigenschaftler();
        }

        private void SaveBauwerknamen( Gebäude gebäude, string neuerNamen)
        {
            gebäude.Bauwerknamen = neuerNamen;
            if (SharedData.Map != null)
            {
                var gemark = SharedData.Map[gebäude.Bezeichner];
                gemark.Bauwerknamen = neuerNamen;
                SharedData.StoreQueue.Enqueue(gebäude);
                SharedData.StoreQueue.Enqueue(gemark);
            }
        }

        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is Gebäude gebäude)
                {
                    // Get the binding path of the edited column
                    if (e.Column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                    {
                        string propertyName = binding.Path.Path;
                        if (propertyName == _BauwerkNamenBindingPath)
                        {
                            if (e.EditingElement is TextBox tb) {
                                SaveBauwerknamen(gebäude, tb.Text);
                            }
                        }
                    }
                }
            }
        }

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
