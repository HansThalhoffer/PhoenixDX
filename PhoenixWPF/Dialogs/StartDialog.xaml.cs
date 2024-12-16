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
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für StartDialog.xaml
    /// </summary>
    public partial class StartDialog : Window
    {
        // Public properties to access dialog values
        public string SelectedReich { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool IsSaveChecked { get; private set; } = false;

        public StartDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract values from the dialog controls
            SelectedReich = ReichsAuswahl.SelectedItem?.ToString() ?? string.Empty;
            Password = PasswordBox.Password;
            IsSaveChecked = Speichern.IsChecked ?? false;

            // Close the dialog and set the result to true
            DialogResult = true;
            Close();
        }

        // Public method to prepopulate control values (optional, if needed)
        public void SetInitialValues(string selectedReich, string password, bool isSaveChecked)
        {
            ReichsAuswahl.SelectedItem = selectedReich;
            PasswordBox.Password = password;
            Speichern.IsChecked = isSaveChecked;
        }

        public EncryptedString ProvidePassword()
        {
            return string.IsNullOrEmpty(Password) ? "" : Password;
        }
    }
}
