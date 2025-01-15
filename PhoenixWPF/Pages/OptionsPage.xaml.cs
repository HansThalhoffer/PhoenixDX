using PhoenixWPF.Helper;
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
    /// Interaktionslogik für OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : Page, IOptions
    {
        // Property to hold the state of "Reichszugehörigkeit sichtbar"
        bool _isReichszugehorigkeitSichtbar = false;
        public bool IsReichszugehorigkeitSichtbar { 
            get
            {
                return _isReichszugehorigkeitSichtbar;
            } 
            set
            {
                _isReichszugehorigkeitSichtbar = value;
                Main.Instance.SetReichOverlay(_isReichszugehorigkeitSichtbar?Visibility.Visible: Visibility.Hidden);
            } 
        }

        bool _isKüstenregelSichtbar = false;
        public bool IsKüstenregelSichtbar {
            get { 
                return _isKüstenregelSichtbar;
            }
            set {
                _isKüstenregelSichtbar = value;
                Main.Instance.ShowKüstenRecht(_isKüstenregelSichtbar ? Visibility.Visible : Visibility.Hidden);
            }
        }


        bool _self = false;
        public void ChangeZoomLevel(float zoomLevel)
        {
            _self = true;
            sldZoom.Value = zoomLevel *100;
            _self = false;
        }

        private void sldZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Main.Map != null && _self == false)
                Main.Map.SetZoom((float)e.NewValue / 100);
        }

        public OptionsPage()
        {
            InitializeComponent();
            DataContext = this;
            Main.Instance.Options = this;
            IsKüstenregelSichtbar = Main.Instance.Settings.UserSettings.ShowKüstenregel;
        }
    }
}
