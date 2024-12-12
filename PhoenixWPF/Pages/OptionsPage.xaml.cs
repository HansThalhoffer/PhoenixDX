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
    public partial class OptionsPage : Page
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

        public OptionsPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
