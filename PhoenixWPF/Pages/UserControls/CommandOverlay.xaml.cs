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
            // Reset();
        }

        public void Scale(double scale) {
            this.Scaler.ScaleX = scale;
            this.Scaler.ScaleY = scale;
            InvalidateVisual();
        }

        
    }
}
