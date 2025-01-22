using Microsoft.VisualBasic;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für StartDialog.xaml
    /// </summary>
    public partial class StartDialog : Window
    {
        // Public properties to access dialog values
        public string Password { get; private set; } = string.Empty;
        public bool IsSaveChecked { get; private set; } = true;

        private List<string>? ZugDatenListe = null;


        public int SelectedReich
        {
            get => SelectedNation?.Nummer ?? -1;
        }

        Dictionary<string, Nation> _nationen = [];

        public StartDialog(Nation[] nationen, int selectedReich, string? password, string pathToZugdaten)
        {
            string selectedNation = string.Empty;
            InitializeComponent();
            foreach (Nation nation in nationen)
            {
                if (selectedReich == nation.Nummer)
                    selectedNation = nation.Bezeichner;
                _nationen.Add(nation.Bezeichner, nation);
            }
            ReichsAuswahl.ItemsSource = _nationen.Keys;
            if (App.Current != null && App.Current.MainWindow != null && App.Current.MainWindow != this)
                Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ReichsAuswahl.SelectedItem = selectedNation;
            PasswordBox.Password = password;
            if (string.IsNullOrEmpty(password))
            {
                Speichern.IsChecked = false;
            }
            else 
            {
                if (selectedReich >= 0)
                {
                    CheckPassword();
                }
            }

            ZugDatenListe = Helper.StorageSystem.GetNumericDirectories(pathToZugdaten);
            ZugAuswahl.ItemsSource = ZugDatenListe;
            ZugAuswahl.SelectedItem = ZugDatenListe[ZugDatenListe.Count-1];
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog and set the result to true
            DialogResult = false;
            Close();
        }

        public Nation? SelectedNation
        {
            get
            {
                string? reich = ReichsAuswahl.SelectedItem == null ? null : ReichsAuswahl.SelectedItem.ToString();
                if (reich != null)
                {
                    return _nationen[reich];
                }
                return null;
            }
        }
        
        void CheckPassword()
        {
            if (SelectedNation != null)
            {
                string pw = PasswordBox.Password;
                if (pw != null)
                {
                    string pwOrg = SelectedNation?.DBpass ?? "hannebambel";
                    if (pwOrg == pw)
                    {
                        OKButton.IsEnabled = true;
                        return;
                    }
                }
            }
           OKButton.IsEnabled = false;
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            CheckPassword();
        }

        private void ReichsAuswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckPassword();
        }


        public int? SelectedZug
        {
            get
            {
                string? zug = ZugAuswahl.SelectedItem == null ? null : ZugAuswahl.SelectedItem.ToString();
                if (zug != null)
                {
                    return int.Parse(zug);
                }
                return null;
            }
        }

        private void ZugAuswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckPassword();
        }
    }
}
