using PhoenixDX.Structures;
using PhoenixModel.dbPZE;
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
    public class ExpectedIncome {
        int GesamtEinkommen { get; set; }
        int TieflandFelder { get; set; }
        int TieflandEinkommem { get; set; }
        int TieflandwaldFelder { get; set; }
        int TieflandwaldEinkommen { get; set; }
        int TieflandwüsteFelder { get; set; }
        int TieflandwüsteEinkommen { get; set; }
        int TieflandsumpfFelder { get; set; }
        int TieflandsumpfEinkommen { get; set; }
        int HochlandFelder { get; set; }
        int HochlandEinkommen { get; set; }
        int BergFelder { get; set; }
        int BergEinkommen { get; set; }
        int GebirgFelder { get; set; }
        int GebirgEinkommen { get; set; }
        int Burgen { get; set; }
        int BurgenEinkommen { get; set; }
        int Städte { get; set; }
        int StädteEinkommen { get; set; }
        int Festungen { get; set; }
        int FestungenEinkommen { get; set; }
        int Hauptstädte { get; set; }
        int HauptstädteEinkommen { get; set; }
        int FestungsHauptstädte { get; set; }
        int FestungsHauptstädteEinkommen { get; set; }
        string Reich { get; set; } = string.Empty;

        public ExpectedIncome(Nation reich) {
            this.Reich = reich.Reich;
            if (SharedData.Map != null) {
                var kleinfelder = SharedData.Map.Values.Where(k => k.Nation == reich);
                this.BergFelder = kleinfelder.Where(k => k.Terrain.Typ == PhoenixModel.ExternalTables.GeländeTabelle.TerrainType.Bergland).Count();
                this.BergEinkommen = BergFelder * 500;
            }

        }
    }
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
