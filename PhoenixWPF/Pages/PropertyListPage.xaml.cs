using PhoenixModel.Helper;
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
    /// Zeigt alle Properties eines IEigenschaftler an
    /// </summary>
    public partial class PropertyListPage : Page, IPropertyDisplay
    {
        public PropertyListPage()
        {
            InitializeComponent();
            Main.Instance.PropertyDisplay = this;
        }

        public void Display(List<Eigenschaft> eigenschaften)
        {
            this.PropertyDataGrid.ItemsSource = eigenschaften;
        }

        public void Display(IEigenschaftler eigenschaftler)
        {
            Display(eigenschaftler.Eigenschaften);
        }

        private void PropertyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is Eigenschaft eigenschaft)
                {
                    PropertyProcessor.UpdateSource(eigenschaft);
                }
            }
        }
    }
}
