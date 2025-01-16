using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using PhoenixWPF.Pages;
using System.Windows;
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
            Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            Schenkungen.Navigated += Schenkungen_Navigated;
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
