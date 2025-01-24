using PhoenixModel.EventsAndArgs;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für SelectFigurPage.xaml
    /// </summary>
    public partial class SelectFigurPage : Page
    {
        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];
        public SelectFigurPage()
        {
            InitializeComponent();
            ProgramView.OnViewEvent += ViewModel_OnViewEvent;
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
        }

        // zuerst erfolgt die Auswahl des Kleinfeldes und dann einer Figur
        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SynchSelected();
        }

        // zuerst erfolgt die Auswahl des Kleinfeldes und dann einer Figur
        private void SynchSelected()
        {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected != null && selected is Spielfigur figur)
            {
                if (EigenschaftlerList.Contains(figur))
                {
                    EigenschaftlerDataGrid.SelectedItem = figur;
                    EigenschaftlerDataGrid.ScrollIntoView(figur);
                }
            }
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (SharedData.Gebäude != null && ProgramView.SelectedNation != null &&
                (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded || e.EventType == ViewEventArgs.ViewEventType.UpdateSpielfiguren))
            {
                EigenschaftlerDataGrid.ItemsSource = null;
                EigenschaftlerList.Clear();
                var list = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                if (list != null)
                    EigenschaftlerList.AddRange(list);
                LoadEigenschaftler();
            }
        }

        public void LoadEigenschaftler()
        {
            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = {  };
            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;
            List<Eigenschaft> columns = eigList.Where(prop => !toIgnore.Contains(prop.Name)).ToList();

            EigenschaftlerDataGrid.Columns.Clear();

            // Add dynamic columns for Eigenschaften
            for (int dex = 0; dex < columns.Count; ++dex)
            {
                var eig = columns[dex];
                string name = eig.Name;
                DataGridTextColumn column;
                if (eig.SortValue == int.MinValue)
                {
                    column = new DataGridTextColumn
                    {
                        Header = name,
                        Binding = new System.Windows.Data.Binding($"Eigenschaften[{dex}].Wert"),
                        IsReadOnly = !eig.IsEditable
                    };
                }
                else
                {
                    column = new DataGridTextColumn
                    {
                        Header = name,
                        Binding = new System.Windows.Data.Binding($"Eigenschaften[{dex}].Wert"),
                        IsReadOnly = !eig.IsEditable,
                        SortMemberPath = $"Eigenschaften[{dex}].SortValue" 
                    };
                }                
                EigenschaftlerDataGrid.Columns.Add(column);
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }       

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEigenschaftler();
            SynchSelected();
        }

        private void SaveSpielfigur(Spielfigur figur)
        {
            if (figur is PhoenixModel.dbZugdaten.Character character)
                SharedData.StoreQueue.Enqueue(character);
            else if (figur is PhoenixModel.dbZugdaten.Kreaturen kreatur)
                SharedData.StoreQueue.Enqueue(kreatur);
            else if (figur is PhoenixModel.dbZugdaten.Krieger krieger)
                SharedData.StoreQueue.Enqueue(krieger);
            else if (figur is PhoenixModel.dbZugdaten.Reiter reiter)
                SharedData.StoreQueue.Enqueue(reiter);
            else if (figur is PhoenixModel.dbZugdaten.Schiffe schiff)
                SharedData.StoreQueue.Enqueue(schiff);
            else if (figur is PhoenixModel.dbZugdaten.Zauberer zauberer)
                SharedData.StoreQueue.Enqueue(zauberer);
            else
                throw new NotImplementedException();
        }

        private void SaveSpielfigurCharakterNamen(Spielfigur figur, string neuerNamen)
        {
            figur.CharakterName = neuerNamen;
            // nur speichern, wenn das eine zulässige Änderung war
            if (figur.CharakterName == neuerNamen)
                SaveSpielfigur(figur);
        }
        private void SaveSpielfigurSpielerNamen(Spielfigur figur, string neuerNamen)
        {
            figur.SpielerName = neuerNamen;
            // nur speichern, wenn das eine zulässige Änderung war
            if (figur.SpielerName == neuerNamen)
                SaveSpielfigur(figur);
        }
        private void SaveSpielfigurTitel(Spielfigur figur, string neuerNamen)
        {
            figur.Titel = neuerNamen;
            // nur speichern, wenn das eine zulässige Änderung war
            if (figur.Titel == neuerNamen)
                SaveSpielfigur(figur);
        }

        /// <summary>
        /// es wurde ein Wert geändert, nun versuchen wir ihn zu speichern
        /// Das ist aber nicht immer erfolgreich, editierbare Werte gibt es nur bei Charakteren und Zauberern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is Spielfigur figur)
                {
                    // Get the binding path of the edited column
                    if (e.Column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                    {
                        if (e.EditingElement is TextBox tb)
                        {
                            string wert = tb.Text;
                            string? propertyName = (e.Column != null && e.Column.Header!= null) ? e.Column.Header.ToString():string.Empty;

                            if (propertyName == NamensSpielfigur.HeaderCharakterName)
                                SaveSpielfigurCharakterNamen(figur, tb.Text);
                            else if (propertyName == NamensSpielfigur.HeaderSpielerName)
                                SaveSpielfigurSpielerNamen(figur, tb.Text);
                            else if (propertyName == NamensSpielfigur.HeaderBeschriftung)
                                SaveSpielfigurTitel(figur, tb.Text);
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
                if (row != null && row.Item != null && row.Item is Spielfigur figur)
                {
                    // auch die Spielfigur ist eine Kleinfeldposition
                    Program.Main.Instance.Spiel?.SelectGemark(figur);
                }
            }
        }

        // Ensure the context menu only opens when clicking a column header
        private void EigenschaftlerDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            // Get the clicked element
            DependencyObject depObj = e.OriginalSource as DependencyObject;

            // Traverse up the Visual Tree to find a DataGridColumnHeader
            while (depObj != null && !(depObj is DataGridColumnHeader)) {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            // If we found a column header, show the ContextMenu
            if (depObj is DataGridColumnHeader columnHeader) {
                columnHeader.ContextMenu.IsOpen = true;
                e.Handled = true; // Prevents default behavior
            }
        }

        // Context Menu: Filter Item Click Handler
        private void FilterMenuItem_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem menuItem) {
                string? selectedCategory = menuItem.Header.ToString();
                if (selectedCategory == null || selectedCategory.StartsWith("Alle ")) {
                    EigenschaftlerDataGrid.ItemsSource = null;
                    EigenschaftlerList.Clear();
                    var list = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                    if (list != null)
                        EigenschaftlerList.AddRange(list);
                    LoadEigenschaftler();
                    return;
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

        private void EigenschaftlerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                var item = e.AddedItems[0];
                if (Main.Instance.SelectionHistory.Current == item)
                    return;
                if (item is Spielfigur figur)
                    Program.Main.Instance.Spiel?.SelectGemark(figur);
            }
        }
    }
}
