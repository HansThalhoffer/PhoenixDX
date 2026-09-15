using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was sich auf einer Gemark bauen laesst - und wenn nichts, warum nicht.
    ///
    /// Die Bauregeln geben zu jeder Frage eine Antwort mit Begruendung; BauoptionenView haelt sie
    /// fest, damit die Oberflaeche sie zeigen kann statt nur die Schaltflaeche zu verstecken.
    /// </summary>
    public class BauoptionenTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            SetzePhase(Zugphase.Rüstphase);
        }

        private static void SetzePhase(Zugphase phase) {
            if (SharedData.ZugdatenSettings != null)
                SharedData.ZugdatenSettings.Last().Phase = (int)phase;
        }

        private static KleinFeld EigeneGemark() {
            var feld = SharedData.Map!.Values.FirstOrDefault(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && gemark.IsWasser == false);
            Assert.True(feld != null, "Das eigene Reich hat keine Landgemark");
            return feld!;
        }

        /// <summary>
        /// Die Liste ist immer vollstaendig: vier Kantenbauwerke in sechs Richtungen und die Burg.
        /// Auch was nicht geht, steht mit seiner Begruendung darin.
        /// </summary>
        [StaFact]
        public void DieListeIstImmerVollstaendig() {
            LadeAlles();

            foreach (var feld in new KleinFeld?[] { null, EigeneGemark() }) {
                var optionen = BauoptionenView.Bestimme(feld);
                Assert.Equal(4 * 6 + 1, optionen.Count);
                Assert.Single(optionen, option => option.Richtung == null);
                Assert.All(optionen, option => Assert.False(string.IsNullOrWhiteSpace(option.Bezeichnung)));
                Assert.All(optionen, option => Assert.False(string.IsNullOrWhiteSpace(option.Hinweis)));

                // jede Art kommt in jeder Richtung genau einmal vor
                foreach (var art in BauoptionenView.Kantenbauwerke)
                    foreach (Direction richtung in Enum.GetValues<Direction>())
                        Assert.Single(optionen, option => option.Art == art && option.Richtung == richtung);
            }
        }

        /// <summary>
        /// Ohne Auswahl geht nichts, und das steht auch da.
        /// </summary>
        [StaFact]
        public void OhneGemarkSagtDieLageDas() {
            LadeAlles();

            var lage = BauoptionenView.BeschreibeLage(null);
            Assert.True(lage.HasErrors);
            Assert.Contains("Keine Gemark", lage.Title);
            Assert.False(BauoptionenView.IstEtwasMöglich(null));
            Assert.All(BauoptionenView.Bestimme(null), option => Assert.False(option.Möglich));
        }

        /// <summary>
        /// Auf fremdem Gebiet wird nicht gebaut - und der Stern sagt das, statt zu verschwinden.
        /// </summary>
        [StaFact]
        public void AufFremdemGebietStehtDerGrundDa() {
            LadeAlles();
            var fremd = SharedData.Map!.Values.First(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation) == false);

            var lage = BauoptionenView.BeschreibeLage(fremd);
            Assert.True(lage.HasErrors);
            Assert.Contains("gehört nicht zu deinem Reich", lage.Title);
            Assert.Contains(fremd.CreateBezeichner(), lage.Title);
            Assert.False(BauoptionenView.IstEtwasMöglich(fremd));
        }

        /// <summary>
        /// In der Bewegungsphase wird nicht gebaut. Das war der Grund, aus dem der Baustern beim
        /// Auswaehlen einer Gemark leer wurde, ohne dass jemand erfuhr warum.
        /// </summary>
        [StaFact]
        public void InDerBewegungsphaseStehtDiePhaseAlsGrundDa() {
            LadeAlles();
            var eigene = EigeneGemark();
            try {
                SetzePhase(Zugphase.Bewegungsphase);

                var lage = BauoptionenView.BeschreibeLage(eigene);
                Assert.True(lage.HasErrors);
                Assert.Contains("Bewegungsphase", lage.Title);
                Assert.Contains("Rüstphase", lage.Message);
                Assert.False(BauoptionenView.IstEtwasMöglich(eigene));

                // und jede einzelne Option nennt denselben Grund
                Assert.All(BauoptionenView.Bestimme(eigene), option => {
                    Assert.False(option.Möglich);
                    Assert.Contains("Rüstphase", option.Hinweis);
                });
            }
            finally {
                SetzePhase(Zugphase.Rüstphase);
            }
        }

        /// <summary>
        /// In der Ruestphase auf eigenem Gebiet steht da, was geht - mit Namen.
        /// </summary>
        [StaFact]
        public void InDerRuestphaseStehenDieMoeglichkeitenDa() {
            LadeAlles();

            var baubar = SharedData.Map!.Values.FirstOrDefault(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && BauoptionenView.IstEtwasMöglich(gemark));
            Assert.True(baubar != null, "Auf keiner eigenen Gemark laesst sich etwas bauen");

            var lage = BauoptionenView.BeschreibeLage(baubar);
            Assert.False(lage.HasErrors, $"{lage.Title}: {lage.Message}");
            Assert.Contains(baubar!.CreateBezeichner(), lage.Title);

            var möglich = BauoptionenView.Bestimme(baubar).Where(option => option.Möglich).ToList();
            Assert.NotEmpty(möglich);
            foreach (var option in möglich) {
                // in der Aufzaehlung steht jede Moeglichkeit beim Namen
                Assert.Contains(option.Bezeichnung, lage.Message);
                // und die Kurzhilfe nennt den Preis
                Assert.Contains("GS", option.Hinweis);
            }
        }

        /// <summary>
        /// Die Bezeichnung nennt Bauwerk und Richtung im Klartext.
        /// </summary>
        [StaFact]
        public void DieBezeichnungIstLesbar() {
            LadeAlles();
            var optionen = BauoptionenView.Bestimme(EigeneGemark());

            var strasseNO = optionen.First(o => o.Art == ConstructionElementType.Strasse && o.Richtung == Direction.NO);
            Assert.Equal("Straße nach Nordosten", strasseNO.Bezeichnung);

            // der Osten hiess in den Meldungen lange "2", weil DirectionNames.Osten auf NO zeigte
            var strasseO = optionen.First(o => o.Art == ConstructionElementType.Strasse && o.Richtung == Direction.O);
            Assert.Equal("Straße nach Osten", strasseO.Bezeichnung);

            Assert.Equal("Burg", optionen.First(o => o.Richtung == null).Bezeichnung);
            Assert.Equal("Kaianlage", BauoptionenView.Benenne(ConstructionElementType.Kai));
            Assert.Equal("Brücke", BauoptionenView.Benenne(ConstructionElementType.Bruecke));
            Assert.Equal("Wall", BauoptionenView.Benenne(ConstructionElementType.Wall));
        }

        /// <summary>
        /// Jede Himmelsrichtung hat ihren eigenen Namen - sechs Richtungen, sechs Namen.
        /// </summary>
        [Fact]
        public void JedeRichtungHatIhrenEigenenNamen() {
            var namen = Enum.GetValues<Direction>()
                .Select(richtung => ((DirectionNames)richtung).ToString())
                .ToList();

            Assert.Equal(6, namen.Distinct().Count());
            Assert.All(namen, name => Assert.False(int.TryParse(name, out _), "Die Richtung hat keinen Namen: " + name));
            Assert.Equal("Osten", ((DirectionNames)Direction.O).ToString());
        }
    }
}
