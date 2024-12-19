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
using SharpDX.Direct2D1.Effects;
using PhoenixDX.Structures;
using SharpDX.Direct3D9;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für ChartPage.xaml
    /// </summary>
    public partial class ChartPage : Page
    {
        public ChartPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();


        private void ZeigeSchatzkammer()
        {
            if (SharedData.Schatzkammer == null || SharedData.Schatzkammer.Count == 0)
                return;

            List<ChartValues<int>> lineValues = [];

            foreach (var val in Enum.GetValues(typeof(Schatzkammer.Felder)))
            {
                if (val.ToString() != "monat")
                    lineValues.Add(new ChartValues<int>());
            }

            foreach (var item in SharedData.Schatzkammer)
            {
                lineValues[(int)Schatzkammer.Felder.Reichschatz].Add((int)item.Reichschatz);
                lineValues[(int)Schatzkammer.Felder.Einahmen_land].Add((int)item.Einahmen_land);
                lineValues[(int)Schatzkammer.Felder.schenkung_bekommen].Add((int)item.schenkung_bekommen);
                lineValues[(int)Schatzkammer.Felder.GS_bei_truppen].Add((int)item.GS_bei_truppen);
                lineValues[(int)Schatzkammer.Felder.schenkung_getaetigt].Add((int)item.schenkung_getaetigt);
                lineValues[(int)Schatzkammer.Felder.Verruestet].Add((int)item.Verruestet);
                Categories.Add("Monat " + item.monat);
            }

            int i = 0;
            foreach (var val in Enum.GetValues(typeof(Schatzkammer.Felder)))
            {
                if (val.ToString() == "monat")
                    continue;
                SeriesCollection.Add(new LineSeries
                {
                    Title = val.ToString(),
                    Values = lineValues[i++],
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    DataLabels = false,
                });
            }
        }

        public void LoadData()
        {
            if (Tag != null && Tag.ToString() == "Schatzkammer")
                ZeigeSchatzkammer();

            // SeriesCollection.AddRange(series);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}

