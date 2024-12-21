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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Frame = System.Windows.Controls.Frame;
using static PhoenixWPF.Database.Zugdaten;

namespace PhoenixWPF.Pages
{

    /// <summary>
    /// Interaktionslogik für ChartPage.xaml
    /// </summary>
    public partial class ChartPage : Page
    {
        public ChartPage()
        {
             InitializeComponent();
            
        }

   
        public SeriesCollection? SeriesCollection { get; set; } = [];
        public ObservableCollection<string> Labels { get; set; } = [];

        private void ZeigeSchatzkammer()
        {
            if (SharedData.Schatzkammer == null || SharedData.Schatzkammer.Count == 0)
                return;

            List<ChartValues<double>> lineValues = [];
            for (int i = 0; i <6;  i++) 
                lineValues.Add(new ChartValues<double>());

            int count = 0;
            foreach (Schatzkammer item in SharedData.Schatzkammer)
            {
                lineValues[0].Add(item.Reichschatz);
                lineValues[1].Add(item.Einahmen_land== 0? double.NaN : item.Einahmen_land);
                lineValues[2].Add(item.schenkung_bekommen == 0 ? double.NaN : item.schenkung_bekommen);
                lineValues[3].Add(item.GS_bei_truppen == 0 ? double.NaN : item.GS_bei_truppen);
                lineValues[4].Add(item.schenkung_getaetigt == 0 ? double.NaN : item.schenkung_getaetigt);
                lineValues[5].Add(item.Verruestet == 0 ? double.NaN : item.Verruestet);
              
                Labels.Add("Monat " + item.monat);
                count ++;
            }

            for (int i = 0; i < 6; i++)
            {
                var rls = new LineSeries
                {
                    Title = ((Schatzkammer.Felder)i+1).ToString(),
                    Values = lineValues[i],
                    LineSmoothness = 0

                    
                };
                SeriesCollection?.Add(rls);
            }
        }

        public void ZeigeTruppenStatistik()
        {
            var statistik = Zugdaten.LoadTruppenStatistik();
            const int maxFelder = 9;

            List<ChartValues<double>> lineValues = [];
            for (int i = 0; i < maxFelder; i++)
                lineValues.Add(new ChartValues<double>());
            
            foreach (TruppenStatistik item in statistik)
            {
                lineValues[0].Add(item.Krieger/2000);
                lineValues[1].Add(item.KriegerHF);
                lineValues[2].Add(item.Reiter/500);
                lineValues[3].Add(item.ReiterHF);
                lineValues[4].Add(item.Schiffe/10);
                lineValues[5].Add(item.LKS);
                lineValues[6].Add(item.SKS);
                lineValues[7].Add(item.LKP);
                lineValues[8].Add(item.SKP);

                Labels.Add("Monat " + item.Monat);
            }

            // Krieger
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Krieger (2000)",
                Values = lineValues[0],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 183, 189, 74),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Krieger Heerführer",
                Values = lineValues[1],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 121, 125, 52),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });

            // Reiter
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Reiter (500)",
                Values = lineValues[2],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 3, 252, 98),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Reiter Heerführer",
                Values = lineValues[3],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 67, 143, 56),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });

            // schiffe
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schiffe (50)",
                Values = lineValues[4],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 24, 137, 186),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Leichte Kriegsschiffe",
                Values = lineValues[5],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 3, 103, 252),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schwere Kriegsschiffe",
                Values = lineValues[6],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 24, 105, 186),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });


            SeriesCollection?.Add(new LineSeries
            {
                Title = "Leichte Katapulte",
                Values = lineValues[7],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 186, 24, 143),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schwere Katapulte",
                Values = lineValues[8],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 145, 32, 115),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 0
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Frame? frame = Helper.VisualTreeHelperExtensions.FindParent<Frame>(this);
            if (frame != null && frame.Tag != null)
            {
                if (frame.Tag.ToString() == "Schatzkammer")
                    ZeigeSchatzkammer();
                else
                    ZeigeTruppenStatistik();
                DataContext = this;
            }
            
        }
    }
}

