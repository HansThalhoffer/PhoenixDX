using PhoenixModel.Helper;
using PhoenixWPF.Pages;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            Schenkungen.Navigated += Schenkungen_Navigated;
        }
        private void Schenkungen_Navigated(object sender, NavigationEventArgs e)
        {
            if (Schenkungen.Content is PropertyGridPage page)
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
