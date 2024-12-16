using PhoenixModel.dbPZE;
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
        public string Password { get; private set; } = string.Empty;
        public bool IsSaveChecked { get; private set; } = false;
        Dictionary<string, Nation> _nationen = [];

        public StartDialog(Nation[] nationen)
        {
            InitializeComponent();
            foreach (Nation nation in nationen)
                _nationen.Add(nation.Bezeichner, nation);
            ReichsAuswahl.ItemsSource = _nationen.Keys;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract values from the dialog controls
            Password = PasswordBox.Password;
            IsSaveChecked = Speichern.IsChecked ?? false;

            // Close the dialog and set the result to true
            DialogResult = true;
            Close();
        }

        // Public method to prepopulate control values (optional, if needed)
        public void SetInitialValues(int selectedReich, string password, bool isSaveChecked)
        {
            ReichsAuswahl.SelectedIndex = selectedReich;
            PasswordBox.Password = password;
            Speichern.IsChecked = isSaveChecked;
        }

        public EncryptedString ProvidePassword()
        {
            return string.IsNullOrEmpty(Password) ? "" : Password;
        }

        Nation? GetSelectedReich()
        {
            string? reich = ReichsAuswahl.SelectedItem.ToString();
            if (reich != null)
            {
                return _nationen[reich];
            }
            return null;
        }


        public int ProvideReich()
        {
            return GetSelectedReich()?.Nummer ?? -1;
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (GetSelectedReich() != null)
            {
                string pw = PasswordBox.Password;
                if (pw != null)
                {
                    string pwOrg = GetSelectedReich()?.DBpass ?? "hannebambel";
                    if (pwOrg == pw)
                    {
                        OKButton.IsEnabled = true;
                    }
                    else
                    {
                        OKButton.IsEnabled = false;
                    }
                }
            }
        }
    }
}
