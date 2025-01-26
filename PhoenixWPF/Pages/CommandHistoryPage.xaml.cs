using PhoenixModel.Commands.Parser;
using PhoenixModel.ViewModel;
using SharpDX;
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
            CommandDataGrid.ItemsSource = SharedData.Commands;
            
        }

        private void Refresh() {
            CommandDataGrid.Items.Refresh();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button && button.DataContext is SimpleCommand command) {
                var result = command.UndoCommand();
                SharedData.Commands.Remove(command);
                if (result.HasErrors == false) {
                    Refresh();
                }
            }
        }
      
    }
}
