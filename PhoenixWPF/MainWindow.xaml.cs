using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string tag = menuItem.Tag as string;

                // Show the corresponding tab
                if (FindName(tag) is TabItem tabItem)
                {
                    tabItem.Visibility = Visibility.Visible;
                }
            }
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string tag = menuItem.Tag as string;

                // Hide the corresponding tab
                if (FindName(tag) is TabItem tabItem)
                {
                    tabItem.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
