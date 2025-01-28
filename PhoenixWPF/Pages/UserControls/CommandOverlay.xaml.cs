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
            this.Visibility = Visibility.Visible;
            buttonShoot.Visibility = figur.CanShoot() ? Visibility.Visible : Visibility.Collapsed;
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


            if (figur is Zauberer) {
                buttonFusion.Visibility = Visibility.Collapsed;
                buttonSplit.Visibility = Visibility.Collapsed;
                buttonBarriere.Visibility = Visibility.Visible;
                buttonBannen.Visibility = Visibility.Visible;
                buttonTeleport.Visibility = Visibility.Visible;
                buttonDuell.Visibility = Visibility.Visible;
            }
            else {
                buttonBarriere.Visibility = Visibility.Collapsed;
                buttonBannen.Visibility = Visibility.Collapsed;
                buttonTeleport.Visibility = Visibility.Collapsed;
                buttonDuell.Visibility = Visibility.Collapsed;
            }

            if (figur is TruppenSpielfigur truppen) {
                if (figur.CanSattleUp()) {
                    buttonHorse.Visibility = Visibility.Visible;
                    buttonHorse.Content = "Aufsitzen";
                }
                if (figur.CanSattleDown()) {
                    buttonHorse.Visibility = Visibility.Visible;
                    buttonHorse.Content = "Absitzen";
                }
                buttonSplit.Visibility = Visibility.Visible;
            }
            else
                buttonSplit.Visibility = Visibility.Collapsed;

        }



        public void Scale(double scale) {
            this.Scaler.ScaleX = scale;
            this.Scaler.ScaleY = scale;
            InvalidateVisual();
        }


    }
}
