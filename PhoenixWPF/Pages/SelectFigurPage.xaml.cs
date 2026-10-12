using PhoenixModel.Commands.Parser;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.ComponentModel;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static PhoenixModel.View.SpielfigurenView;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für SelectFigurPage.xaml
    /// </summary>
    public partial class SelectFigurPage : Page {
        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];
        public SelectFigurPage() {
            InitializeComponent();
            ProgramView.OnViewEvent += ViewModel_OnViewEvent;
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
        }

        // zuerst erfolgt die Auswahl des Kleinfeldes und dann einer Figur
        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            SynchSelected();
        }

        // zuerst erfolgt die Auswahl des Kleinfeldes und dann einer Figur
        private void SynchSelected() {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected != null && selected is Spielfigur figur) {
                if (EigenschaftlerList.Contains(figur)) {
                    EigenschaftlerDataGrid.SelectedItem = figur;
                    EigenschaftlerDataGrid.ScrollIntoView(figur);
                }
            }
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e) {
            if (ProgramView.SelectedNation == null)
                return;
            switch (e.EventType) {

                case ViewEventArgs.ViewEventType.EverythingLoaded: {
                        EigenschaftlerDataGrid.ItemsSource = null;
                        EigenschaftlerList.Clear();
                        var list = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                        if (list != null)
                            EigenschaftlerList.AddRange(list);
                        LoadEigenschaftler();
                        break;
                    }

                case ViewEventArgs.ViewEventType.UpdateSpielfiguren: {
                        if (e.Data != null && e.Data is Spielfigur figur)
                            FrischeZeileAuf(figur);
                        break;
                    }
            }
        }

        /// <summary>
        /// Schreibt die Werte einer Figur in ihre Zeile des Gitters.
        ///
        /// <b>Gemeldet als Absturz beim Ziehen einer Figur</b> (ArgumentOutOfRangeException in
        /// dieser Methode). Drei Fehler steckten in der alten Fassung übereinander:
        ///
        /// <list type="number">
        /// <item>Die Zeile wurde über <c>Items.IndexOf</c> gesucht. Spielfigur erbt von
        /// KleinfeldPosition ein Equals nach gf und kf, also lieferte das die erste Figur
        /// <em>des Feldes</em> statt der gesuchten - und nach einer Bewegung steht die gesuchte
        /// Figur auf einem neuen Feld, auf dem schon etwas anderes stehen kann. Jetzt wird über
        /// die Identität gesucht (<see cref="Gitterzeile.Finde"/>).</item>
        /// <item>Stand die Figur nicht im Gitter, war die Zeilennummer -1 und der Zugriff auf die
        /// Liste warf. Das ist der Normalfall, sobald im Kontextmenü ein Filter gesetzt ist -
        /// dann zeigt das Gitter nur einen Teil der Armee. Jetzt gibt es nichts aufzufrischen.</item>
        /// <item>Die Spalten kamen aus <c>EigenschaftlerList[0]</c>, die Werte aber aus der
        /// aufzufrischenden Figur. Krieger, Zauberer, Charaktere und Schiffe haben
        /// unterschiedlich viele Eigenschaften; die Schleife lief über das Ende der kürzeren
        /// hinaus. Genau an dieser Zeile ist der Absturz entstanden. Jetzt werden nur so viele
        /// Spalten bedient, wie die Figur Eigenschaften hat.</item>
        /// </list>
        ///
        /// Die Werte werden von Hand in die Zellen geschrieben, weil Eigenschaft keine Änderung
        /// meldet und die Liste bei jedem Lesen neu erzeugt wird. Deshalb wird sie hier genau
        /// einmal gelesen.
        /// </summary>
        private void FrischeZeileAuf(Spielfigur figur) {
            int zeile = Gitterzeile.Finde(EigenschaftlerDataGrid.Items, figur);
            if (zeile < 0)
                return;

            var eigenschaften = figur.Eigenschaften;
            int anzahl = Math.Min(eigenschaften.Count, EigenschaftlerDataGrid.Columns.Count);
            for (int spalte = 0; spalte < anzahl; spalte++)
                DataGridHelper.SetCellValue(EigenschaftlerDataGrid, zeile, spalte, eigenschaften[spalte].Wert ?? string.Empty);
        }

        public void LoadEigenschaftler() {
            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = { };
            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;
            List<Eigenschaft> columns = eigList.Where(prop => !toIgnore.Contains(prop.Name)).ToList();

            EigenschaftlerDataGrid.Columns.Clear();

            // Add dynamic columns for Eigenschaften
            for (int dex = 0; dex < columns.Count; ++dex) {
                var eig = columns[dex];
                string name = eig.Name;
                DataGridTextColumn column;
                if (eig.SortValue == int.MinValue) {
                    column = new DataGridTextColumn {
                        Header = name,
                        Binding = new System.Windows.Data.Binding($"Eigenschaften[{dex}].Wert"),
                        IsReadOnly = !eig.IsEditable
                    };                   
                }
                else {
                    column = new DataGridTextColumn {
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

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            LoadEigenschaftler();
            SynchSelected();
        }

        private void SaveSpielfigur(Spielfigur figur) {
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

        private void SaveSpielfigurCharakterNamen(Spielfigur figur, string neuerNamen) {
            if (string.IsNullOrEmpty(neuerNamen))
                return;
            if (figur.CharakterName == neuerNamen)
                return;
            string commandString = $"Nenne {figur.BaseTyp} {figur.Nummer} {neuerNamen} ({figur.CharakterName}) ";
            if (CommandParser.ParseCommand(commandString, out var cmd) && cmd != null) {
                var result = cmd.ExecuteCommand();
                if (result.HasErrors)
                    SpielWPF.LogError(result.Title, result.Message);
            }
            else
                SpielWPF.LogError("Der Name konnte nicht gespeichert werden", "Keine Ahnung warum");
        }
        private void SaveSpielfigurSpielerNamen(Spielfigur figur, string neuerNamen) {
              if (figur.SpielerName == neuerNamen)
                return; 
            string commandString = $"{figur.BaseTyp} {figur.Nummer} wird gespielt von {neuerNamen} ({figur.SpielerName})";
            if (CommandParser.ParseCommand(commandString, out var cmd) && cmd != null) {
                var result = cmd.ExecuteCommand();
                if (result.HasErrors)
                    SpielWPF.LogError(result.Title, result.Message);
            }
            else
                SpielWPF.LogError("Der Spielername konnte nicht gespeichert werden", "Keine Ahnung warum");
        }
        private void SaveSpielfigurTitel(Spielfigur figur, string neuerNamen) {
            if (string.IsNullOrEmpty(neuerNamen))
                return;
            if (figur.Bezeichner == neuerNamen)
                return; 
            string commandString = $"Bezeichne {figur.BaseTyp} {figur.Nummer} {neuerNamen} ({figur.Bezeichner})";
            if (CommandParser.ParseCommand(commandString, out var cmd) && cmd != null) {
                var result = cmd.ExecuteCommand();
                if (result.HasErrors)
                    SpielWPF.LogError(result.Title, result.Message);
            }
            else
                SpielWPF.LogError("Die Bezeichnung konnte nicht gespeichert werden", "Keine Ahnung warum");
        }

        /// <summary>
        /// es wurde ein Wert geändert, nun versuchen wir ihn zu speichern
        /// Das ist aber nicht immer erfolgreich, editierbare Werte gibt es nur bei Charakteren und Zauberern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            if (e.EditAction == DataGridEditAction.Commit) {
                if (e.Row.DataContext is Spielfigur figur) {
                    // Get the binding path of the edited column
                    if (e.Column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding) {
                        if (e.EditingElement is TextBox tb) {
                            string wert = tb.Text;
                            string? propertyName = (e.Column != null && e.Column.Header != null) ? e.Column.Header.ToString() : string.Empty;

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

        private void EigenschaftlerDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            // Get the position of the mouse relative to the DataGrid
            Point mousePosition = e.GetPosition(EigenschaftlerDataGrid);

            // Hit test to get the element under the mouse
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(EigenschaftlerDataGrid, mousePosition);

            if (hitTestResult != null) {
                // Check if the hit test result is a DataGridRow
                DataGridRow? row = GetParentDataGridRow(hitTestResult.VisualHit);
                if (row != null && row.Item != null && row.Item is Spielfigur figur) {
                    // auch die Spielfigur ist eine Kleinfeldposition
                    Program.Main.Instance.Spiel?.SelectGemark(figur);
                }
            }
        }

        // Ensure the context menu only opens when clicking a column header
        private void EigenschaftlerDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            // Get the clicked element
            DependencyObject? depObj = e.OriginalSource as DependencyObject;

            // Traverse up the Visual Tree to find a DataGridColumnHeader
            while (depObj != null && !(depObj is DataGridColumnHeader)) {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            // If we found a column header, show the ContextMenu
            if (depObj is DataGridColumnHeader columnHeader) {
                EigenschaftlerDataGrid.ContextMenu.IsOpen = true;
            }
            e.Handled = true; // Prevents default behavior
        }

        // Context Menu: Filter Item Click Handler
        private void FilterMenuItem_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem menuItem) {
                string? selectedCategory = menuItem.Header.ToString();
                foreach (var item in EigenschaftlerDataGrid.ContextMenu.Items) {
                    if (item is MenuItem mnuItem && mnuItem != menuItem)
                        mnuItem.IsChecked = false;
                }
                if (selectedCategory == null)
                    return;
                Armee? armee = null;
                if (selectedCategory.EndsWith("zeigen")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                }
                else if (selectedCategory.EndsWith("ohne Befehl")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(SpielfigurenFilter.Search.OhneBefehl));
                }
                else if (selectedCategory.EndsWith("Gold")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(SpielfigurenFilter.Search.Gold));
                }
                else if (selectedCategory.EndsWith("Fernkämpfer")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(SpielfigurenFilter.Search.Fernkampf));
                }
                else if (selectedCategory.EndsWith("Charaktere")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Charakter));
                }
                else if (selectedCategory.EndsWith("Kreaturen")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Kreatur));
                }
                else if (selectedCategory.EndsWith("Krieger")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Krieger));
                }
                else if (selectedCategory.EndsWith("Reiter")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Reiter));
                }
                else if (selectedCategory.EndsWith("Schiffe")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Schiff));
                }
                else if (selectedCategory.EndsWith("Zauberer")) {
                    armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation, new SpielfigurenFilter(FigurType.Zauberer));
                }
                if (armee != null) {
                    EigenschaftlerDataGrid.ItemsSource = null;
                    EigenschaftlerList.Clear();
                    EigenschaftlerList.AddRange(armee);
                    LoadEigenschaftler();
                }
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
