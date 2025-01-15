using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
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

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für EigenschaftlerListGridPage.xaml
    /// </summary>
    public partial class EigenschaftlerListGridPage : Page
    {
        public EigenschaftlerListGridPage()
        {
            InitializeComponent();
        }

        public List<IEigenschaftler>? EigenschaftlerList { get; set; }

   
        public void LoadEigenschaftler()
        {
     
            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;

            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;

            EigenschaftlerDataGrid.Columns.Clear();

            // Add 'Bezeichner' column
            DataGridTextColumn bezeichnerColumn = new DataGridTextColumn
            {
                Header = "Bezeichner",
                Binding = new System.Windows.Data.Binding("Bezeichner")
            };
            EigenschaftlerDataGrid.Columns.Add(bezeichnerColumn);

            // Add dynamic columns for Eigenschaften
            for (int i = 0; i < eigList.Count; i++)
            {
                var eig = eigList[i];
                string name = eig.Name;
                name = name.Replace('_', ' ');
                name = name.Replace("ID",string.Empty);
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = name,
                    Binding = new System.Windows.Data.Binding($"Eigenschaften[{i}].Wert")
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
