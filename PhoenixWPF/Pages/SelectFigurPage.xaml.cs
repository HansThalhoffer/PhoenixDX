﻿using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für SelectFigurPage.xaml
    /// </summary>
    public partial class SelectFigurPage : Page
    {
        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];
        public SelectFigurPage()
        {
            InitializeComponent();
            ViewModel.OnViewEvent += ViewModel_OnViewEvent;
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (SharedData.Gebäude != null && ViewModel.SelectedNation != null &&
                (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded || e.EventType == ViewEventArgs.ViewEventType.UpdateSpielfigur))
            {
                EigenschaftlerList.Clear();
                var list = SpielfigurenView.GetSpielfiguren(ViewModel.SelectedNation);
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
            foreach (var eig in columns)
            {
                string name = eig.Name;
                int index = eigList.IndexOf(eig);
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = name,
                    Binding = new System.Windows.Data.Binding($"Eigenschaften[{index}].Wert"),
                    IsReadOnly = Array.Exists( new string[] { "SpielerName", "CharakterName", "Titel" }, s => s == name)
                };
                // binding path merken, um dann später safe das Property korrekt zu setzen
                // der header kann später mal umbetitelt werden, daher ist das hier häßlich aber zuverlässig
                if (name == "CharakterName")
                {
                    _SpielfigurCharakterNamenBindingPath = $"Eigenschaften[{index}].Wert";
                }
                else if (name == "SpielerName")
                {
                    _SpielfigurSpielerNamenBindingPath= $"Eigenschaften[{index}].Wert";
                }
                else if (name == "Titel")
                {
                    _SpielfigurTitelBindingPath = $"Eigenschaften[{index}].Wert";
                }
                EigenschaftlerDataGrid.Columns.Add(column);
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }

        private string _SpielfigurCharakterNamenBindingPath = string.Empty;
        private string _SpielfigurSpielerNamenBindingPath = string.Empty;
        private string _SpielfigurTitelBindingPath = string.Empty;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEigenschaftler();
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
                            string propertyName = binding.Path.Path;

                            if (propertyName == _SpielfigurCharakterNamenBindingPath)
                                SaveSpielfigurCharakterNamen(figur, tb.Text);
                            else if (propertyName == _SpielfigurSpielerNamenBindingPath)
                                SaveSpielfigurSpielerNamen(figur, tb.Text);
                            else if (propertyName == _SpielfigurTitelBindingPath)
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
                    Program.Main.Instance.Map?.Goto(figur);
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
