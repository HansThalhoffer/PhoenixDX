using PhoenixModel.Helper;
using PhoenixModel.Program;
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
                var list = SharedData.Gebäude.Values.Where(geb => geb.Nation == ViewModel.SelectedNation);
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
    }
}
