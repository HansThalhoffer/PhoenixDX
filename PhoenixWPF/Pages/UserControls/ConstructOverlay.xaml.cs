using PhoenixModel.dbErkenfara;
using PhoenixModel.Extensions;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.Windows;
using System.Windows.Controls;


namespace PhoenixWPF.Pages.UserControls {
    /// <summary>
    /// Interaktionslogik für ConstructOverlay.xaml
    /// </summary>
    public partial class ConstructOverlay : UserControl {
        public ConstructOverlay() {
            InitializeComponent();
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
            // this.Visibility = Visibility.Hidden;
        }

        private void SetKleinfeldVisibility(KleinFeld kf) {
            Visibility = Visibility.Visible;
           
        }


        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected != null && selected is KleinFeld kf) {
                SetKleinfeldVisibility(kf);
                return;
            }
            this.Visibility = Visibility.Hidden;
        }
    }
}
