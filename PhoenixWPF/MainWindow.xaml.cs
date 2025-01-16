using System.Windows;
using System.Windows.Controls;
using PhoenixWPF.Database.Generatoren;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;

namespace PhoenixWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Closing += OnClosing; ;
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
            Main.Instance.StopInstance();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Main.Instance.StartInstance();
            this.Loaded -= OnLoaded;
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? tag = menuItem.Tag as string;

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
                string? tag = menuItem.Tag as string;

                // Hide the corresponding tab
                if (FindName(tag) is TabItem tabItem)
                {
                    tabItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? tag = menuItem.Tag as string;
                if (tag == null)
                    return;
                switch (tag)
                {
                    case "Schatzkammer":
                        new SchatzkammerDialog().Show();
                        break;
                    case "Truppen":
                        new TruppenEntwicklungDialog().Show();
                        break;

                    case "Zugwechsel":
                        Main.Instance.Zugwechsel();
                        break;
                    case "InstallUSB":
                        Main.Instance.CreateInstallUSBStick();
                        break;
                   
                    case "Zug 999 (Testdaten)":
                        TestDataGenerator.GeneriereTestdatenFürZug999();
                        break;

                }
            }
        }
    }
}
