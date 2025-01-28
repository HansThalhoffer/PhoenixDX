using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            if (selected is Spielfigur figur) {
                this.Visibility = Visibility.Visible;
                buttonShoot.Visibility = Visibility.Collapsed;
                buttonHorse.Visibility = Visibility.Collapsed;

                if (figur is TruppenSpielfigur truppen) {
                    if (truppen.LKP > 0 || truppen.SKP > 0)
                        buttonShoot.Visibility = Visibility.Visible;
                    if (truppen.Pferde > 0) {
                        buttonHorse.Visibility = Visibility.Visible;
                        buttonHorse.Content = "Aufsitzen";
                    }
                    if (figur is Reiter) {
                        buttonHorse.Visibility = Visibility.Visible;
                        buttonHorse.Content = "Absitzen";
                    }
                    buttonSplit.Visibility = Visibility.Visible;
                    buttonBarriere.Visibility = Visibility.Collapsed;
                    buttonBannen.Visibility = Visibility.Collapsed;
                    buttonTeleport.Visibility = Visibility.Collapsed;
                    buttonDuell.Visibility = Visibility.Collapsed;
                }
                else if (figur is Zauberer){
                    buttonFusion.Visibility = Visibility.Collapsed;
                    buttonSplit.Visibility = Visibility.Collapsed;
                    buttonBarriere.Visibility = Visibility.Visible;
                    buttonBannen.Visibility = Visibility.Visible;
                    buttonTeleport.Visibility = Visibility.Visible;
                    buttonDuell.Visibility = Visibility.Visible;
                }
            }
            else
                this.Visibility = Visibility.Hidden;
        }

        public void Scale(double scale) {
            this.Scaler.ScaleX = scale;
            this.Scaler.ScaleY = scale;
            InvalidateVisual();
        }

        
    }
}
