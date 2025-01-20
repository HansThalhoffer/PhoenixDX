using LiveCharts;
using LiveCharts.Wpf;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PhoenixWPF.Database.Zugdaten;
using Frame = System.Windows.Controls.Frame;

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

            SeriesCollection?.Add(new LineSeries
            {
                Title = "Reichsschatz",
                Values = lineValues[0],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 64, 119, 207),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "EinnahmenView",
                Values = lineValues[1],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 44, 163, 110),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1
            });

            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schenkung erhalten",
                Values = lineValues[2],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 99, 201, 104),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 12
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Plünderungen",
                Values = lineValues[3],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 235, 229, 56),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Verschenkt",
                Values = lineValues[4],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 204, 90, 49),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 12
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Verrüstet",
                Values = lineValues[5],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 208, 17, 214),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1,
                PointGeometry = DefaultGeometries.Triangle,
                PointGeometrySize = 12

            });
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
                lineValues[4].Add(item.Schiffe/100);
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
                LineSmoothness = 1
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
                LineSmoothness = 1
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
                LineSmoothness = 1
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
                LineSmoothness = 1
            });

            // schiffe
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schiffe (100)",
                Values = lineValues[4],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 24, 137, 186),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1
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
                LineSmoothness = 1
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
                LineSmoothness = 1
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
                LineSmoothness = 1
            });
            SeriesCollection?.Add(new LineSeries
            {
                Title = "Schwere Katapulte",
                Values = lineValues[8],
                Stroke = new SolidColorBrush
                {
                    Color = Color.FromArgb(255, 145, 32, 112),
                    Opacity = 1
                },
                Fill = Brushes.Transparent,
                LineSmoothness = 1
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Frame? frame = Helper.VisualTreeHelperExtensions.FindParent<Frame>(this);
            if (frame != null && frame.Tag != null)
            {
                if (frame.Tag.ToString() == "FinanzEntwicklung")
                    ZeigeSchatzkammer();
                else
                    ZeigeTruppenStatistik();
                DataContext = this;
            }
            
        }
    }
}

