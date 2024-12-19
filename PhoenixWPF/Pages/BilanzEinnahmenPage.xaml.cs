using LiveCharts.Wpf;
using LiveCharts;
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
using PhoenixModel.dbZugdaten;
using PhoenixWPF.Database;
using System.Collections.ObjectModel;
using PhoenixModel.Helper;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für BilanzEinnahmenPage.xaml
    /// </summary>
    public partial class BilanzEinnahmenPage : Page
    {
        public BilanzEinnahmenPage()
        {
            DataContext = this;
            
            List<Schatzkammer> bilanzData = new List<Schatzkammer>
            {
            
            };

            CreateSeriesCollection(bilanzData);
            Categories = new ObservableCollection<string>();
          
            InitializeComponent();

        }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

        private void CreateSeriesCollection(List<Schatzkammer>? bilanzData)
        {
            if (bilanzData == null || bilanzData.Count == 0)
                return;

            var properties = typeof(Schatzkammer).GetProperties();

            foreach (var prop in properties)
            {
                if (prop.Name != "monat")
                {
                    Categories.Add("Monat " + item.monat);
                }
                if (prop.PropertyType == typeof(int))
                {
                    var lineValues = new ChartValues<int>();

                    foreach (var item in bilanzData)
                    {
                        var value = prop.GetValue(item);
                        if (value is int)
                            lineValues.Add((int)value);
                    }

                    var lineSeries = new LineSeries
                    {
                        Title = prop.Name,
                        Values = lineValues,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        DataLabels = false,                        
                    };

                    SeriesCollection.Add(lineSeries);
                }
            }
        }

        public void UpdateData()
        {
           var einnahmen = SharedData.Schatzkammer?.ToList();

            var series = CreateSeriesCollection(einnahmen);
            // SeriesCollection.AddRange(series);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }
    }
}

