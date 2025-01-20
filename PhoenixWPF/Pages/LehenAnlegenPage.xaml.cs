using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
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

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für LehenAnlegenPage.xaml
    /// </summary>
    public partial class LehenAnlegenPage : Page {

        List<KleinFeld> _kleinfelder = [];
        List<NamensSpielfigur> _charaktere = [];
        
        public LehenAnlegenPage() {
            InitializeComponent();
            this.Loaded += LehenAnlegenPage_Loaded;
        }

        private void LehenAnlegenPage_Loaded(object sender, RoutedEventArgs e) {
            LoadData();
        }

        private void LoadData() {
            if (SharedData.Map == null)
                return;
            _kleinfelder.Clear();
            var marked = KleinfeldView.GetMarked(MarkerType.User);
            if (marked != null && marked.Count() > 0) {
                _kleinfelder.AddRange(marked);
                DataGridLehen.ItemsSource = _kleinfelder;
            }
            var charaktere = LehenView.HoleSpielerfigurenOhneLehen();
            if (charaktere != null && charaktere.Count() > 0) {
                _charaktere.AddRange(charaktere);
            }
            cmbCharacter.ItemsSource = _charaktere;
        }

        private void BtnAddLehen_Click(object sender, RoutedEventArgs e) {
            var character = cmbCharacter.SelectedItem as NamensSpielfigur;
            if (character != null) {
                LehenView.CreateLehen(character, _kleinfelder, txtLehenName.Text);
            }
        }

        private void txtLehenName_TextChanged(object sender, TextChangedEventArgs e) {
            CheckPreconditions();
        }
        private void cmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CheckPreconditions();
        }

        private void CheckPreconditions() {
            if (_kleinfelder.Count == 0) {
                BtnAddLehen.IsEnabled = false;
                return;
            }
            if (txtLehenName.Text.Length == 0) { 
                BtnAddLehen.IsEnabled = false;
                return;
            }
            if (_charaktere.Count == 0) {
                BtnAddLehen.IsEnabled = false;
                return;
            }
            if (cmbCharacter.Text.Length == 0) {
                BtnAddLehen.IsEnabled = false;
                return;
            }
            BtnAddLehen.IsEnabled = true;

        }

        
    }
}
