using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Beute (Regelwerk 5.8).
    ///
    /// Drei Quellen: eroberte Ruestorte, Kaperung zur See, Ladung vernichteter Heere zu Land.
    /// Dazu die Katapulte, die sich im Nahkampf immer ergeben.
    /// </summary>
    public class BeuteTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Die Beute je Ruestort steht so im Regelwerk 5.8.1.
        /// </summary>
        [StaFact]
        public void DieRuestortbeuteStehtSoImRegelwerk() {
            LadeAlles();

            (string Name, int GS)[] erwartet = [
                ("Burg", 2000),
                ("Stadt", 4000),
                ("Festung", 6000),
                ("Hauptstadt", 110000),
                ("Festungshauptstadt", 112000),
            ];

            foreach (var (name, gs) in erwartet) {
                var rüstort = SharedData.RüstortReferenz!.FirstOrDefault(r => r.Ruestort == name);
                Assert.True(rüstort != null, $"In der Crossreferenz fehlt {name}");
                Assert.Equal(gs, BeuteRules.GetRüstortbeute(rüstort));
            }

            // ein Dorf bringt nichts ein, und ohne Ruestort gibt es auch nichts
            var dorf = SharedData.RüstortReferenz!.FirstOrDefault(r => r.Ruestort == "Dorf");
            if (dorf != null)
                Assert.Equal(0, BeuteRules.GetRüstortbeute(dorf));
            Assert.Equal(0, BeuteRules.GetRüstortbeute((PhoenixModel.dbCrossRef.Rüstort?)null));
            Assert.Equal(0, BeuteRules.GetRüstortbeute((PhoenixModel.dbErkenfara.KleinFeld?)null));
        }

        /// <summary>
        /// Zur See wird anteilig gekapert: 10:1 alles, 9:1 neunzig Prozent, 8:1 achtzig
        /// (Regelwerk 5.8.2).
        /// </summary>
        [Fact]
        public void ZurSeeWirdAnteiligGekapert() {
            Assert.Equal(1.0, BeuteRules.BerechneKaperanteil(10000, 1000), 6);
            Assert.Equal(0.9, BeuteRules.BerechneKaperanteil(9000, 1000), 6);
            Assert.Equal(0.8, BeuteRules.BerechneKaperanteil(8000, 1000), 6);
            Assert.Equal(0.1, BeuteRules.BerechneKaperanteil(1000, 1000), 6);

            // mehr als alles gibt es nicht
            Assert.Equal(1.0, BeuteRules.BerechneKaperanteil(100000, 1000), 6);

            // und ohne Gegner oder ohne eigenes Heer nichts
            Assert.Equal(0, BeuteRules.BerechneKaperanteil(1000, 0));
            Assert.Equal(0, BeuteRules.BerechneKaperanteil(0, 1000));
        }

        /// <summary>
        /// Auf eine Ladung angewendet wird abgerundet - halbe Goldstuecke gibt es nicht.
        /// </summary>
        [Fact]
        public void DieSeebeuteWirdAbgerundet() {
            Assert.Equal(5000, BeuteRules.BerechneSeebeute(5000, 1000, 10000));
            Assert.Equal(10000, BeuteRules.BerechneSeebeute(10000, 1000, 10000));
            // 3333 * 0.1 = 333.3 -> 333
            Assert.Equal(333, BeuteRules.BerechneSeebeute(1000, 1000, 3333));
            Assert.Equal(0, BeuteRules.BerechneSeebeute(5000, 1000, 0));
        }

        /// <summary>
        /// Zu Land bleibt die Art der Ladung erhalten: Gold bleibt Gold, besondere Einnahmen
        /// bleiben besondere Einnahmen (Regelwerk 5.8.3). Das ist der Kern der Regel - aus
        /// besonderen Einnahmen darf ausserhalb der Ruestmonate gerueste werden.
        /// </summary>
        [StaFact]
        public void ZuLandBleibtDieArtDerLadungErhalten() {
            LadeAlles();
            var eins = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().First(Plausibilität.IsValid);
            var zwei = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().Where(Plausibilität.IsValid).Skip(1).First();

            int gs1 = eins.GS, ke1 = eins.Kampfeinnahmen;
            int gs2 = zwei.GS, ke2 = zwei.Kampfeinnahmen;
            try {
                eins.GS = 1000; eins.Kampfeinnahmen = 2000;
                zwei.GS = 300; zwei.Kampfeinnahmen = 400;

                var beute = BeuteRules.BerechneLandbeute([eins, zwei]);
                Assert.Equal(1300, beute.Gold);
                Assert.Equal(2400, beute.Kampfeinnahmen);
                Assert.Equal(3700, beute.Gesamt);
            }
            finally {
                eins.GS = gs1; eins.Kampfeinnahmen = ke1;
                zwei.GS = gs2; zwei.Kampfeinnahmen = ke2;
            }

            Assert.Equal(BeuteRules.Ladung.Nichts, BeuteRules.BerechneLandbeute(null));
        }

        /// <summary>
        /// Gutgeschrieben wird getrennt: erbeutetes Ruestortgold ist eine besondere Einnahme
        /// (Regelwerk 5.8.1) und landet bei den Kampfeinnahmen, nicht beim mitgefuehrten Gold.
        /// </summary>
        [StaFact]
        public void ErbeutetesRuestortgoldIstEineBesondereEinnahme() {
            LadeAlles();
            var sieger = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().First(Plausibilität.IsValid);

            int gsVorher = sieger.GS, keVorher = sieger.Kampfeinnahmen;
            try {
                sieger.GS = 0;
                sieger.Kampfeinnahmen = 0;

                BeuteRules.SchreibeGut(sieger, new BeuteRules.Ladung(1000, 500), rüstortbeute: 4000);

                Assert.Equal(1000, sieger.GS);
                Assert.Equal(4500, sieger.Kampfeinnahmen);
            }
            finally {
                sieger.GS = gsVorher;
                sieger.Kampfeinnahmen = keVorher;
            }

            // ohne Sieger passiert nichts und es fliegt nichts
            BeuteRules.SchreibeGut(null, new BeuteRules.Ladung(100, 100), 100);
        }

        /// <summary>
        /// Katapulte ergeben sich im Nahkampf immer - Schiffe nicht, die verteidigen sich
        /// (Regelwerk 5.4).
        /// </summary>
        [StaFact]
        public void KatapulteErgebenSichSchiffeNicht() {
            LadeAlles();
            var landheer = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .First(t => t.BaseTyp != PhoenixModel.ExternalTables.FigurType.Schiff && Plausibilität.IsValid(t));
            var flotte = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(t => t.BaseTyp == PhoenixModel.ExternalTables.FigurType.Schiff && Plausibilität.IsValid(t));

            int lkp = landheer.LKP, skp = landheer.SKP;
            try {
                landheer.LKP = 3;
                landheer.SKP = 2;

                var beute = BeuteRules.BerechneKatapultbeute([landheer]);
                Assert.Equal(3, beute.Leichte);
                Assert.Equal(2, beute.Schwere);

                if (flotte != null) {
                    // Kriegsschiffe der Flotte fallen nicht als Katapultbeute an
                    int flkp = flotte.LKP, fskp = flotte.SKP;
                    try {
                        flotte.LKP = 5; flotte.SKP = 5;
                        var mitFlotte = BeuteRules.BerechneKatapultbeute([landheer, flotte]);
                        Assert.Equal(3, mitFlotte.Leichte);
                        Assert.Equal(2, mitFlotte.Schwere);
                    }
                    finally {
                        flotte.LKP = flkp; flotte.SKP = fskp;
                    }
                }
            }
            finally {
                landheer.LKP = lkp;
                landheer.SKP = skp;
            }

            Assert.Equal((0, 0), BeuteRules.BerechneKatapultbeute(null));
        }
    }
}
