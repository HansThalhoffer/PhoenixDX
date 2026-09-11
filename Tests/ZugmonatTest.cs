using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Zugmonat rechnet aus der fortlaufenden Zugnummer Jahr, Monat und Jahreszeit aus.
    /// Grundlage ist Regelwerk Kapitel 3.
    /// </summary>
    public class ZugmonatTest {

        [Theory]
        // Zug, Name, Jahreszeit, Rüstmonat, Einnahmemonat
        [InlineData(1, "Hawar", Jahreszeit.Winter, true, false)]
        [InlineData(2, "Rim", Jahreszeit.Frühling, false, false)]
        [InlineData(3, "Naliv", Jahreszeit.Frühling, false, false)]
        [InlineData(4, "Larn", Jahreszeit.Sommer, false, true)]
        [InlineData(5, "Hel", Jahreszeit.Sommer, true, false)]
        [InlineData(6, "Jawan", Jahreszeit.Herbst, false, false)]
        [InlineData(7, "Lud", Jahreszeit.Herbst, false, false)]
        [InlineData(8, "Agul", Jahreszeit.Winter, false, true)]
        public void ErstesJahrEntsprichtDemRegelwerk(int zug, string name, Jahreszeit jahreszeit, bool rüstmonat, bool einnahmemonat) {
            var monat = new Zugmonat(zug);
            Assert.Equal(name, monat.Name);
            Assert.Equal(jahreszeit, monat.Jahreszeit);
            Assert.Equal(rüstmonat, monat.IstRüstmonat);
            Assert.Equal(einnahmemonat, monat.IstEinnahmemonat);
            Assert.Equal(0, monat.Jahr);
        }

        /// <summary>
        /// Nach acht Monaten beginnt ein neues Jahr mit demselben Monatsnamen
        /// </summary>
        [Fact]
        public void DasJahrWiederholtSichNachAchtMonaten() {
            for (int zug = 1; zug <= 8; zug++) {
                var erstes = new Zugmonat(zug);
                var zweites = new Zugmonat(zug + Zugmonat.MonateProJahr);
                Assert.Equal(erstes.Name, zweites.Name);
                Assert.Equal(erstes.Jahreszeit, zweites.Jahreszeit);
                Assert.Equal(erstes.IstRüstmonat, zweites.IstRüstmonat);
                Assert.Equal(erstes.IstEinnahmemonat, zweites.IstEinnahmemonat);
                Assert.Equal(erstes.Jahr + 1, zweites.Jahr);
            }
        }

        /// <summary>
        /// Ein Jahr hat genau zwei Rüst- und zwei Einnahmemonate
        /// </summary>
        [Fact]
        public void JedesJahrHatZweiRuestUndZweiEinnahmemonate() {
            int rüst = 0, einnahme = 0;
            for (int zug = 17; zug < 17 + Zugmonat.MonateProJahr; zug++) {
                var monat = new Zugmonat(zug);
                if (monat.IstRüstmonat) rüst++;
                if (monat.IstEinnahmemonat) einnahme++;
                // ein Monat kann nicht beides sein
                Assert.False(monat.IstRüstmonat && monat.IstEinnahmemonat);
            }
            Assert.Equal(2, rüst);
            Assert.Equal(2, einnahme);
        }

        /// <summary>
        /// Der Rüstmonat folgt unmittelbar auf den Einnahmemonat - dort steht das Steuergeld
        /// zur Verfügung (Regelwerk 3.2)
        /// </summary>
        [Fact]
        public void AufDenEinnahmemonatFolgtDerRuestmonat() {
            for (int zug = 1; zug <= 24; zug++) {
                var monat = new Zugmonat(zug);
                if (monat.IstEinnahmemonat)
                    Assert.True(monat.Nächster.IstRüstmonat,
                        $"Auf den Einnahmemonat {monat.Name} muss ein Rüstmonat folgen, es folgt aber {monat.Nächster.Name}");
            }
        }

        /// <summary>
        /// Der aktuelle Zug 170 der Testdaten
        /// </summary>
        [Fact]
        public void Zug170IstRimImFruehling() {
            var monat = new Zugmonat(170);
            // (170 - 1) % 8 = 1 -> Rim
            Assert.Equal(21, monat.Jahr);
            Assert.Equal(1, monat.MonatImJahr);
            Assert.Equal("Rim", monat.Name);
            Assert.Equal(Jahreszeit.Frühling, monat.Jahreszeit);
            Assert.False(monat.IstRüstmonat);
            Assert.False(monat.IstEinnahmemonat);
        }

        [Fact]
        public void UngueltigeZuegeStuerzenNichtAb() {
            var monat = new Zugmonat(0);
            Assert.False(monat.IstGültig);
            Assert.False(monat.IstRüstmonat);
            Assert.False(monat.IstEinnahmemonat);
            Assert.Equal("unbekannter Zug", monat.ToString());
        }
    }
}
