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

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null && !(parentObject is T))
            {
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return parentObject as T;
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
            const int maxFelder = 2;

            List<ChartValues<double>> lineValues = [];
            for (int i = 0; i < maxFelder; i++)
                lineValues.Add(new ChartValues<double>());
            foreach (TruppenStatistik item in statistik)
            {
                lineValues[0].Add(item.Krieger);
                lineValues[1].Add(item.Reiter);
               
                Labels.Add("Monat " + item.Monat);
            }



            for (int i = 0; i < maxFelder; i++)
            {
                var rls = new LineSeries
                {
                    Title = ((TruppenStatistik.Felder)i + 1).ToString(),
                    Values = lineValues[i],
                    LineSmoothness = 0


                };
                SeriesCollection?.Add(rls);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frame = FindParent<Frame>(this);
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

