using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für SelectRüstortPage.xaml
    /// </summary>
    public partial class SelectRüstortPage : Page {
        public SelectRüstortPage() {
            InitializeComponent();
            ProgramView.OnViewEvent += ViewModel_OnViewEvent;
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
        }

        // zuerst erfolgt die Auswahl des Kleinfeldes und dann einer Figur
        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            SynchSelected();
        }

        private void SynchSelected() {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected != null && selected is Gebäude gebäude) {
                if (EigenschaftlerList.Contains(gebäude)) {
                    EigenschaftlerDataGrid.SelectedItem = gebäude;
                    EigenschaftlerDataGrid.ScrollIntoView(gebäude);
                }
            }
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e) {
            if (SharedData.Gebäude != null && ProgramView.SelectedNation != null &&
                (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded || e.EventType == ViewEventArgs.ViewEventType.UpdateGebäude)) {
                EigenschaftlerList.Clear();
                var list = BauwerkeView.GetGebäude(ProgramView.SelectedNation);
                if (list != null)
                    EigenschaftlerList.AddRange(list);
                LoadEigenschaftler();
            }
        }

        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];


        public void LoadEigenschaftler() {

            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = { "Nation", "Reich", "Nation.Farbname", "Nation.Reich", "Rüstort.Bauwerk", "Rüstort.Nummer", "Zerstört", "IsNew" };
            // string[] toIgnore = { };
            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;
            List<Eigenschaft> columns = eigList.Where(prop => !toIgnore.Contains(prop.Name)).ToList();


            EigenschaftlerDataGrid.Columns.Clear();

            // Add 'Bezeichner' column
            DataGridTextColumn bezeichnerColumn = new DataGridTextColumn {
                Header = "Bezeichner",
                Binding = new System.Windows.Data.Binding("Bezeichner")
            };
            EigenschaftlerDataGrid.Columns.Add(bezeichnerColumn);

            // Add dynamic columns for Eigenschaften
            foreach (var eig in columns) {
                string name = eig.Name;
                int index = eigList.IndexOf(eig);
                if (eig.Name == "InBau")  {
                    DataGridTemplateColumn imageColumn = new DataGridTemplateColumn {
                        Header = "Baustelle"
                    };

                    // Define a template that contains an Image
                    DataTemplate template = new DataTemplate();
                    FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));

                    // Set image source
                    imageFactory.SetValue(Image.SourceProperty, new BitmapImage(new Uri("pack://application:,,,/Resources/baustelle.png")));
                    // Set image size
                    imageFactory.SetValue(Image.WidthProperty, 16.0);   // Adjust as needed
                    imageFactory.SetValue(Image.HeightProperty, 16.0);  // Adjust as needed

                    // Optional: Align image to center
                    imageFactory.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
                    imageFactory.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                    // Bind Visibility to the boolean property
                    Binding visibilityBinding = new Binding(name) {
                        Converter = new  PhoenixWPF.Pages.Converter.BoolToVisibilityConverter()
                    };
                    imageFactory.SetBinding(Image.VisibilityProperty, visibilityBinding);

                    template.VisualTree = imageFactory;
                    imageColumn.CellTemplate = template;

                    EigenschaftlerDataGrid.Columns.Add(imageColumn);
                }
                else {
                    DataGridTextColumn column = new DataGridTextColumn {
                        Header = name,
                        Binding = new System.Windows.Data.Binding(name),
                        IsReadOnly = name != "Bauwerknamen"
                    };
                    if (name == "Bauwerknamen") {
                        _BauwerkNamenBindingPath = $"Eigenschaften[{index}].Wert";
                    }
                    else if (name == "Rüstort.Baupunkte")
                        column.Header = "BP Soll";
                    EigenschaftlerDataGrid.Columns.Add(column);
                }
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }

        private string _BauwerkNamenBindingPath = string.Empty;

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            LoadEigenschaftler();
            SynchSelected();
        }

        private void SaveBauwerknamen(Gebäude gebäude, string neuerNamen) {
            gebäude.Bauwerknamen = neuerNamen;
            if (SharedData.Map != null) {
                var gemark = SharedData.Map[gebäude.Bezeichner];
                gemark.Bauwerknamen = neuerNamen;
                SharedData.StoreQueue.Enqueue(gebäude);
                SharedData.StoreQueue.Enqueue(gemark);
            }
        }

        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            if (e.EditAction == DataGridEditAction.Commit) {
                if (e.Row.DataContext is Gebäude gebäude) {
                    // Get the binding path of the edited column
                    if (e.Column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding) {
                        string propertyName = binding.Path.Path;
                        if (propertyName == _BauwerkNamenBindingPath) {
                            if (e.EditingElement is TextBox tb) {
                                SaveBauwerknamen(gebäude, tb.Text);
                            }
                        }
                    }
                }
            }
        }

        private void EigenschaftlerDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            // Get the position of the mouse relative to the DataGrid
            Point mousePosition = e.GetPosition(EigenschaftlerDataGrid);

            // Hit test to get the element under the mouse
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(EigenschaftlerDataGrid, mousePosition);

            if (hitTestResult != null) {
                // Check if the hit test result is a DataGridRow
                DataGridRow? row = GetParentDataGridRow(hitTestResult.VisualHit);
                if (row != null && row.Item != null && row.Item is Gebäude gebäude) {
                    // Get the data item corresponding to the row
                    Program.Main.Instance.Spiel?.SelectGemark(gebäude);

                }
            }
        }

        private void EigenschaftlerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                var item = e.AddedItems[0];
                if (Main.Instance.SelectionHistory.Current == item)
                    return;
                if (item is Gebäude figur)
                    Program.Main.Instance.Spiel?.SelectGemark(figur);
            }
        }

        // Helper method to find the DataGridRow from a Visual element
        private DataGridRow? GetParentDataGridRow(DependencyObject visual) {
            // Traverse up the visual tree to find the DataGridRow
            while (visual != null && !(visual is DataGridRow)) {
                visual = VisualTreeHelper.GetParent(visual);
            }
            return visual as DataGridRow;
        }
    }
}
