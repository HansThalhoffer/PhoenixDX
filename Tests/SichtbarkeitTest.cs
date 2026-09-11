using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was ein Reich von den anderen erfahren darf.
    ///
    /// Erkenfara wird auf einem gemeinsamen Spielbrett im Kartenzelt gespielt. Die Reiche stellen
    /// ihre Einheiten und Bauwerke dort auf, Positionen und Arten sind damit oeffentlich. Die Zahlen
    /// dahinter sind es nicht:
    ///
    /// Regelwerk 1.5.12: "Die Zahl der aktuellen Baupunkte wird auf dem Spielbrett nicht eingetragen
    /// und wird nur dem Besitzer bekannt gegeben; das Symbol des Ruestortes ist gegebenenfalls auf
    /// der Karte zu aendern."
    /// </summary>
    public class SichtbarkeitTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        private static bool ZeigtBaupunkte(PhoenixModel.dbErkenfara.KleinFeld feld)
            => feld.Eigenschaften.Any(e => e.Name == "Baupunkte");

        [StaFact]
        public void FremdeBaupunkteBleibenVerborgen() {
            LadeAlles();
            Assert.NotNull(ProgramView.SelectedNation);

            var fremderRuestort = SharedData.Map!.Values.FirstOrDefault(
                feld => feld.Baupunkte > 0 && feld.Nation != null && feld.Nation != ProgramView.SelectedNation);
            Assert.True(fremderRuestort != null, "In den Kartendaten steht kein fremder Ruestort");

            Assert.False(ZeigtBaupunkte(fremderRuestort!),
                $"Das fremde Feld {fremderRuestort!.Bezeichner} gibt seine Baupunkte preis");
        }

        [StaFact]
        public void EigeneBaupunkteBleibenSichtbar() {
            LadeAlles();
            Assert.NotNull(ProgramView.SelectedNation);

            var eigenerRuestort = SharedData.Map!.Values.FirstOrDefault(
                feld => feld.Baupunkte > 0 && feld.Nation == ProgramView.SelectedNation);
            Assert.True(eigenerRuestort != null, "Das eigene Reich hat keinen Ruestort in den Kartendaten");

            Assert.True(ZeigtBaupunkte(eigenerRuestort!),
                $"Das eigene Feld {eigenerRuestort!.Bezeichner} zeigt seine Baupunkte nicht mehr");
        }

        /// <summary>
        /// Die Ausbaustufe ist dagegen oeffentlich - sie steht als Symbol auf dem Brett. Ein fremder
        /// Ruestort muss also weiterhin als solcher erkennbar sein.
        /// </summary>
        [StaFact]
        public void DieAusbaustufeFremderRuestorteBleibtErkennbar() {
            LadeAlles();
            var fremderRuestort = SharedData.Map!.Values.FirstOrDefault(
                feld => feld.Baupunkte > 0 && feld.Nation != null && feld.Nation != ProgramView.SelectedNation);
            Assert.True(fremderRuestort != null, "In den Kartendaten steht kein fremder Ruestort");

            Assert.NotNull(fremderRuestort!.Gebäude);
            Assert.NotNull(fremderRuestort.Gebäude!.Rüstort);
            Assert.False(string.IsNullOrEmpty(fremderRuestort.Gebäude.Rüstort!.Bauwerk),
                "Zu dem fremden Ruestort laesst sich keine Ausbaustufe mehr bestimmen");
        }

        /// <summary>
        /// Fremde Figuren und Gebaeude duerfen angesehen, aber nicht angefasst werden.
        /// </summary>
        [StaFact]
        public void FremdesLaesstSichNichtBearbeiten() {
            LadeAlles();
            var fremdesFeld = SharedData.Map!.Values.FirstOrDefault(
                feld => feld.Nation != null && feld.Nation != ProgramView.SelectedNation);
            Assert.True(fremdesFeld != null, "In den Kartendaten steht kein fremdes Feld");

            Assert.False(fremdesFeld!.Edit(), $"Das fremde Feld {fremdesFeld.Bezeichner} laesst sich bearbeiten");
        }
    }
}
