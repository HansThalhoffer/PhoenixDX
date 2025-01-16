using PhoenixModel.ViewModel;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {

    /// <summary>
    /// Interaktionslogik für ExpectedIncomePage.xaml
    /// </summary>
    public partial class ExpectedIncomePage : Page {
        List<ExpectedIncome> expectedIncomes = [] ;
        public ExpectedIncomePage() {
            InitializeComponent();
            LoadData(); 
            //DataContext = expectedIncomes;
        }
        private void LoadData() {
            if (SharedData.Nationen == null) {
                return;
            }
            foreach (var data in SharedData.Nationen) {
                expectedIncomes.Add(new ExpectedIncome(data));
            }
            EinkommenDataGrid.ItemsSource = expectedIncomes;
        }
    }
}
