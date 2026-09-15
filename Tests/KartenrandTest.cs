using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Am Kartenrand gibt es den Nachbarn nicht.
    ///
    /// Die Bauregeln fuer Bruecke und Kai griffen dort ueber den Indexer in die Karte -
    /// SharedData.Map[...] - und warfen eine KeyNotFoundException. Die kam mitten im Klick hoch,
    /// niemand fing sie auf, und die Anwendung war weg. Jetzt fragen sie nach, statt zu greifen,
    /// und antworten wie jede andere Regel auch: mit einer Begruendung.
    ///
    /// Die Karte kennt 684 solcher Kanten.
    /// </summary>
    public class KartenrandTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>
        /// Sucht eine Kante, hinter der die Karte aufhoert.
        /// </summary>
        private static (KleinFeld Feld, Direction Richtung) FindeRandkante(bool nurLand = false) {
            foreach (var feld in SharedData.Map!.Values) {
                if (nurLand && feld.IsWasser)
                    continue;
                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var nachbar = KartenKoordinaten.GetNachbar(feld, richtung);
                    if (nachbar != null && SharedData.Map.ContainsKey(nachbar.CreateBezeichner()) == false)
                        return (feld, richtung);
                }
            }
            Assert.Fail("Die Karte hat keinen Rand");
            return default;
        }

        /// <summary>
        /// Die Karte hat einen Rand - sonst pruefte dieser Test nichts.
        /// </summary>
        [StaFact]
        public void DieKarteHatEinenRand() {
            LadeAlles();
            var (feld, richtung) = FindeRandkante();

            var jenseits = KartenKoordinaten.GetNachbar(feld, richtung);
            Assert.NotNull(jenseits);
            Assert.Null(KleinfeldView.GetKleinfeld(jenseits));
        }

        /// <summary>
        /// Keine Bauregel wirft am Rand - alle antworten mit einer Begruendung.
        /// </summary>
        [StaFact]
        public void AmRandWirftKeineBauregel() {
            LadeAlles();
            var (feld, richtung) = FindeRandkante();

            // Die Regeln antworten, statt zu werfen - auch wenn das Feld einem anderen Reich
            // gehoert und die Antwort dann schon vorher feststeht.
            foreach (var art in BauoptionenView.Kantenbauwerke) {
                var antwort = BauoptionenView.Prüfe(feld, art, richtung);
                Assert.NotNull(antwort);
                Assert.False(string.IsNullOrWhiteSpace(antwort.Title));
            }

            // und die ganze Liste laesst sich bestimmen
            var optionen = BauoptionenView.Bestimme(feld);
            Assert.Equal(4 * 6 + 1, optionen.Count);
            Assert.NotNull(BauoptionenView.BeschreibeLage(feld));
        }

        /// <summary>
        /// Auf einem eigenen Feld am Rand - dem Fall, der die Anwendung beendet haette - nennt die
        /// Regel das Ende der Karte als Grund.
        /// </summary>
        [StaFact]
        public void AmRandIstDasEndeDerKarteDerGrund() {
            LadeAlles();
            // Am Wasser greift schon die Wasserregel - gesucht ist eine Landgemark am Rand
            var (rand, richtung) = FindeRandkante(nurLand: true);
            Assert.False(rand.IsWasser);

            // Ein eigenes Feld am Rand gibt es im Bestand nicht zwingend; geprueft wird deshalb an
            // einem Feld, das voruebergehend dem eigenen Reich zugeschlagen wird.
            var reichVorher = rand.Reich;
            try {
                rand.Reich = ProgramView.SelectedNation!.Nummer;
                Assert.True(ProgramView.BelongsToUser(rand));

                var brücke = ConstructRules.CanConstructBridge(rand, richtung);
                Assert.True(brücke.HasErrors);
                Assert.Contains("endet die Karte", brücke.Title);

                var kai = ConstructRules.CanConstructKai(rand, richtung);
                Assert.True(kai.HasErrors);

                // Wall und Strasse kennen keinen Nachbarn und antworten wie immer
                Assert.NotNull(ConstructRules.CanConstructWall(rand, richtung));
                Assert.NotNull(ConstructRules.CanConstructRoad(rand, richtung));
            }
            finally {
                rand.Reich = reichVorher;
            }
        }
    }
}
