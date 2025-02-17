using PhoenixModel.Helper;
using PhoenixWPF.Database;
using PhoenixWPF.Pages;
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

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für TruppenEntwicklungDialog.xaml
    /// </summary>
    public partial class TruppenEntwicklungDialog : Window
    {
        public TruppenEntwicklungDialog()
        {
            if (App.Current != null && App.Current.MainWindow != null && App.Current.MainWindow != this)
                Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            Mobilisierung.Navigated += Mobilisierung_Navigated;
        }
        private void Mobilisierung_Navigated(object sender, NavigationEventArgs e) {
            if (Mobilisierung.Content is EigenschaftlerListGridPage page) {
                var collection = Zugdaten.LoadMobilisierungHistory();
                if (page.EigenschaftlerList != null) {
                    page.EigenschaftlerList.Clear();
                    page.EigenschaftlerList = null;
                }
                List<IEigenschaftler> list = [];
                list.AddRange(collection);
                page.EigenschaftlerList = list;
            }
        }
        public void Show (string page) {
            this.Loaded += (s, e) => {
                foreach (TabItem tab in Tabulator.Items) {
                    if (tab != null && tab.Tag != null && tab.Tag.ToString() == page)
                        Tabulator.SelectedItem = tab;
                }
            };
            Show();
            
        }
      
    }
}
