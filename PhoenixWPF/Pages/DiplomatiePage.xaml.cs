using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für DiplomatiePage.xaml
    /// </summary>
    public partial class DiplomatiePage : Page
    {
        public DiplomatiePage()
        {
            InitializeComponent();
            InitializeComponent();
            ViewModel.OnViewEvent += ViewModel_OnViewEvent;
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (SharedData.Diplomatie != null && ViewModel.SelectedNation != null &&
                (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded ))
            {
                EigenschaftlerList.Clear();
                var list = SharedData.Diplomatie.Values;
                if (list != null)
                    EigenschaftlerList.AddRange(list);
                LoadEigenschaftler();
            }
        }
        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];


        public void LoadEigenschaftler()
        {

            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = {  };
            // string[] toIgnore = { };
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
                    IsReadOnly = true
                };
               
                EigenschaftlerDataGrid.Columns.Add(column);
            }

            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
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
                        /*if (propertyName == _BauwerkNamenBindingPath)
                        {
                            if (e.EditingElement is TextBox tb)
                            {
                                SaveBauwerknamen(gebäude, tb.Text);
                            }
                        }*/
                    }
                }
            }
        }
    }
}
