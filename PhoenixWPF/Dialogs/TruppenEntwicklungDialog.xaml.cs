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
            Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
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
