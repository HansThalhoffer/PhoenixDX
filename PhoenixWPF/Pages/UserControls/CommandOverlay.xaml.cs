using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoenixModel.Extensions;

namespace PhoenixWPF.Pages.UserControls {
    /// <summary>
    /// Interaktionslogik für CommandOverlay.xaml
    /// </summary>
    public partial class CommandOverlay : UserControl {
        public CommandOverlay() {
            InitializeComponent();
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
            //this.Visibility = Visibility.Hidden;
        }

        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected == null || selected is Spielfigur figur == false) {
                this.Visibility = Visibility.Hidden;
                return;
            }
            Visibility = Visibility.Visible;
            buttonShoot.Visibility = figur.CanShoot() ? Visibility.Visible : Visibility.Collapsed;
            buttonBarriere.Visibility = figur.CanCastBarriere() ? Visibility.Visible : Visibility.Collapsed;
            buttonBannen.Visibility = figur.CanCastBannen() ? Visibility.Visible : Visibility.Collapsed;
            buttonTeleport.Visibility = figur.CanCastTeleport() ? Visibility.Visible : Visibility.Collapsed;
            buttonDuell.Visibility = figur.CanCastDuell() ? Visibility.Visible : Visibility.Collapsed;
            buttonFusion.Visibility = figur.CanFustion() ? Visibility.Visible : Visibility.Collapsed;
            buttonSplit.Visibility = figur.CanSplit() ? Visibility.Visible : Visibility.Collapsed;

            buttonHorse.Visibility = Visibility.Collapsed;
            if (figur.CanEmbark()) {
                buttonEmbark.Visibility = Visibility.Visible;
                buttonEmbark.Content = "Einschiffen";
            }
            else if (figur.CanDisEmbark()) {
                buttonEmbark.Visibility = Visibility.Visible;
                buttonEmbark.Content = "Ausschiffen";
            }
            else
                buttonEmbark.Visibility = Visibility.Collapsed;
   

            if (figur.CanSattleUp()) {
                buttonHorse.Visibility = Visibility.Visible;
                buttonHorse.Content = "Aufsitzen";
            }
            if (figur.CanSattleDown()) {
                buttonHorse.Visibility = Visibility.Visible;
                buttonHorse.Content = "Absitzen";
            }
        }
    }
}
