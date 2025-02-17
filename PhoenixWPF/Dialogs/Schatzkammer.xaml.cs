using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using PhoenixWPF.Pages;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
namespace PhoenixWPF.Dialogs
{
   
    /// <summary>
    /// Interaktionslogik für SchatzkammerDialog.xaml
    /// </summary>
    public partial class SchatzkammerDialog : System.Windows.Window
    {
        public SchatzkammerDialog()
        {
            if (App.Current != null && App.Current.MainWindow != null && App.Current.MainWindow != this)
                Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            Schenkungen.Navigated += Schenkungen_Navigated;
            Baukosten.Navigated += Baukosten_Navigated;
                
        }

        private void Baukosten_Navigated(object sender, NavigationEventArgs e) {
            if (Baukosten.Content is EigenschaftlerListGridPage page) {
                var collection = Zugdaten.LoadBaukostenHistory();
                if (page.EigenschaftlerList != null) {
                    page.EigenschaftlerList.Clear();
                    page.EigenschaftlerList = null;
                }                
                List<IEigenschaftler> list = [];
                list.AddRange(collection);
                page.EigenschaftlerList = list;
            }
        }

        public void Show(string page) {
            this.Loaded += (s, e) => {
                foreach (TabItem tab in Tabulator.Items) {
                    if (tab != null && tab.Tag != null && tab.Tag.ToString() == page)
                        Tabulator.SelectedItem = tab;
                }
            };
            Show();
        }

        private void Schenkungen_Navigated(object sender, NavigationEventArgs e)
        {
            if (Schenkungen.Content is EigenschaftlerListGridPage page)
            {
                if (SharedData.Schenkungen != null)
                {
                    List<IEigenschaftler> list = [];
                    list.AddRange(SharedData.Schenkungen);
                    page.EigenschaftlerList = list;
                }
            }
        }
    }
}
