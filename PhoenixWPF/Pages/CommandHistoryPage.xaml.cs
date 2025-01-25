using PhoenixModel.Commands.Parser;
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
    /// Interaktionslogik für CommanndHistoryPage.xaml
    /// </summary>
    public partial class CommanndHistoryPage : Page
    {
        public CommanndHistoryPage()
        {
            InitializeComponent();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button && button.DataContext is SimpleCommand command) {
                command.UndoCommand();
                // Refresh UI
                ((DataGrid)button.Parent).Items.Refresh();
            }
        }
      
    }
}
