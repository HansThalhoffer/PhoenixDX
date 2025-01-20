using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für TruppenStatusPage.xaml
    /// </summary>
    public partial class TruppenStatusPage : Page {
        public TruppenStatus? Status { get; private set; } = null;

        public TruppenStatusPage() {
            InitializeComponent();
            var curArmy = SpielfigurenView.GetSpielfiguren(PhoenixModel.Program.ProgramView.SelectedNation);
            Status = new TruppenStatus(curArmy);
            this.DataContext = Status;
        }
    }
}
