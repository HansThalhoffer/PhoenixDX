using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Das Format der Spalte Befehl_ang.
    ///
    /// Die ersten drei Fälle sind die Tests aus UnitTests/BeschiessenTest.vb der Altanwendung,
    /// unverändert übernommen: die Spielleitung wertet diese Zeichenkette aus, das Format darf sich
    /// also nicht verschieben. Der Rest prüft, was dort nicht geprüft war - vor allem, dass ein
    /// geschriebener Befehl wieder als derselbe gelesen wird.
    /// </summary>
    public class BeschussbefehlTest {

        [Fact]
        public void ZerlegeLiefertNichtsFuerEinenLeerenBefehl() {
            Assert.Empty(Beschussbefehl.Zerlege(string.Empty));
            Assert.Empty(Beschussbefehl.Zerlege(null));
        }

        [Fact]
        public void ZerlegeLiefertEinenEinzelnenBefehl() {
            var ergebnis = Beschussbefehl.Zerlege("#4LK#603/78");
            Assert.Single(ergebnis);
            Assert.Equal("4LK#603/78", ergebnis[0]);
        }

        [Fact]
        public void ZerlegeTrenntMehrereBefehle() {
            var ergebnis = Beschussbefehl.Zerlege("#4LK#603/78#3SK#1002/87#2KF#100SK#94/22#1KF");
            Assert.Equal(3, ergebnis.Length);
            Assert.Equal("4LK#603/78", ergebnis[0]);
            Assert.Equal("3SK#1002/87#2KF", ergebnis[1]);
            Assert.Equal("100SK#94/22#1KF", ergebnis[2]);
        }

        [Fact]
        public void LiestAnzahlWaffeUndZiel() {
            var befehl = Beschussbefehl.Lies("4LK#603/78");
            Assert.NotNull(befehl);
            Assert.Equal(4, befehl!.Anzahl);
            Assert.Equal(Fernkampfwaffe.Leicht, befehl.Waffe);
            Assert.Equal(603, befehl.Ziel.gf);
            Assert.Equal(78, befehl.Ziel.kf);
            Assert.Equal(1, befehl.Entfernung);
        }

        [Fact]
        public void LiestDieEntfernungWennSieDabeiSteht() {
            var befehl = Beschussbefehl.Lies("3SK#1002/87#2KF");
            Assert.NotNull(befehl);
            Assert.Equal(3, befehl!.Anzahl);
            Assert.Equal(Fernkampfwaffe.Schwer, befehl.Waffe);
            Assert.Equal(1002, befehl.Ziel.gf);
            Assert.Equal(87, befehl.Ziel.kf);
            Assert.Equal(2, befehl.Entfernung);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Unsinn")]
        [InlineData("4LK")]
        [InlineData("LK#603/78")]
        [InlineData("4LK#603")]
        public void LiestUnbrauchbaresNichtAlsBefehl(string eingabe) {
            Assert.Null(Beschussbefehl.Lies(eingabe));
        }

        /// <summary>
        /// Die Entfernung 1 steht nicht im Befehl - so hat es die Altanwendung geschrieben, und so
        /// sehen die Befehle in den echten Zugdaten aus.
        /// </summary>
        [Fact]
        public void SchreibtDieStandardentfernungNichtMit() {
            var befehl = new Beschussbefehl {
                Anzahl = 4, Waffe = Fernkampfwaffe.Leicht, Ziel = new KleinfeldPosition(603, 78), Entfernung = 1,
            };
            Assert.Equal("4LK#603/78", befehl.ToString());
        }

        [Fact]
        public void SchreibtDieEntfernungAbZweiMit() {
            var befehl = new Beschussbefehl {
                Anzahl = 3, Waffe = Fernkampfwaffe.Schwer, Ziel = new KleinfeldPosition(1002, 87), Entfernung = 2,
            };
            Assert.Equal("3SK#1002/87#2KF", befehl.ToString());
        }

        /// <summary>
        /// Der eigentliche Punkt: was die Anwendung schreibt, muss sie auch wieder lesen können -
        /// und die Spielleitung liest dieselbe Zeichenkette.
        /// </summary>
        [Fact]
        public void GeschriebeneBefehleWerdenWiederGelesen() {
            List<Beschussbefehl> befehle = [
                new() { Anzahl = 4, Waffe = Fernkampfwaffe.Leicht, Ziel = new KleinfeldPosition(603, 78) },
                new() { Anzahl = 3, Waffe = Fernkampfwaffe.Schwer, Ziel = new KleinfeldPosition(1002, 87), Entfernung = 2 },
                new() { Anzahl = 100, Waffe = Fernkampfwaffe.Schwer, Ziel = new KleinfeldPosition(94, 22) },
            ];

            string gespeichert = Beschussbefehl.Schreibe(befehle);
            Assert.Equal("#4LK#603/78#3SK#1002/87#2KF#100SK#94/22", gespeichert);

            var gelesen = Beschussbefehl.LiesAlle(gespeichert);
            Assert.Equal(befehle.Count, gelesen.Count);
            for (int i = 0; i < befehle.Count; i++) {
                Assert.Equal(befehle[i].Anzahl, gelesen[i].Anzahl);
                Assert.Equal(befehle[i].Waffe, gelesen[i].Waffe);
                Assert.Equal(befehle[i].Ziel, gelesen[i].Ziel);
                Assert.Equal(befehle[i].Entfernung, gelesen[i].Entfernung);
            }
        }

        /// <summary>
        /// Die Altanwendung hat die Entfernung als "1KF" mitgeschrieben. Solche Befehle stehen in
        /// den Zugdaten und müssen weiterhin gelesen werden.
        /// </summary>
        [Fact]
        public void LiestAuchDieAeltereSchreibweiseMitEntfernungEins() {
            var gelesen = Beschussbefehl.LiesAlle("#100SK#94/22#1KF");
            Assert.Single(gelesen);
            Assert.Equal(100, gelesen[0].Anzahl);
            Assert.Equal(1, gelesen[0].Entfernung);
        }
    }
}
