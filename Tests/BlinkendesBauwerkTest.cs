using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Welches Bauwerk zu einem Befehl gehoert - und an welcher Kante es liegt.
    ///
    /// Die Auswahl auf der Karte zeigt nur, auf welchem Feld etwas passiert ist. Wer wissen will,
    /// welcher von sechs Waellen gemeint war, braucht die Richtung; die Karte laesst dann genau
    /// dieses Stueck blinken.
    /// </summary>
    public class BlinkendesBauwerkTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>
        /// Ein Bauauftrag an einer Kante nennt Art und Richtung.
        /// </summary>
        [StaFact]
        public void EinKantenbauwerkNenntArtUndRichtung() {
            LadeAlles();

            var feld = SharedData.Map!.Values.First(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && BauoptionenView.Bestimme(gemark)
                    .Any(o => o.Art == ConstructionElementType.Wall && o.Möglich));
            var richtung = BauoptionenView.Bestimme(feld)
                .First(o => o.Art == ConstructionElementType.Wall && o.Möglich).Richtung!.Value;

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Wall im {richtung} von {feld.CreateBezeichner()}", out var befehl));

            var (art, gemeldeteRichtung) = BefehlszielView.GetZielbauwerk((BaseCommand)befehl!);
            Assert.Equal(ConstructionElementType.Wall, art);
            Assert.Equal(richtung, gemeldeteRichtung);
        }

        /// <summary>
        /// Eine Burg gehoert der ganzen Gemark - dann gibt es keine Kante, und es blinkt das Feld.
        /// </summary>
        [StaFact]
        public void EineBurgHatKeineKante() {
            LadeAlles();

            var burg = new ConstructCommand("Errichte Burg auf 305/24") {
                What = ConstructionElementType.Burg,
                Direction = null,
            };

            var (art, richtung) = BefehlszielView.GetZielbauwerk(burg);
            Assert.Equal(ConstructionElementType.Burg, art);
            Assert.Null(richtung);
        }

        /// <summary>
        /// Was gar kein Bauwerk betrifft, nennt auch keines.
        /// </summary>
        [StaFact]
        public void WasKeinBauwerkIstNenntKeines() {
            LadeAlles();

            var bewegung = new MoveCommand("Bewege Heer 1 von 305/24 nach 305/25") {
                FromLocation = new KleinfeldPosition(305, 24),
                ToLocation = new KleinfeldPosition(305, 25),
            };

            var (art, richtung) = BefehlszielView.GetZielbauwerk(bewegung);
            Assert.Null(art);
            Assert.Null(richtung);

            var (keineArt, keineRichtung) = BefehlszielView.GetZielbauwerk(null);
            Assert.Null(keineArt);
            Assert.Null(keineRichtung);
        }

        /// <summary>
        /// Alle vier Kantenbauwerke werden erkannt - sonst blinkt eines davon nie.
        /// </summary>
        [StaFact]
        public void AlleVierKantenbauwerkeWerdenErkannt() {
            LadeAlles();

            foreach (var art in BauoptionenView.Kantenbauwerke) {
                var befehl = new ConstructCommand($"Errichte {art} im NW von 305/24") {
                    What = art,
                    Direction = Direction.NW,
                };
                var (gemeldeteArt, richtung) = BefehlszielView.GetZielbauwerk(befehl);
                Assert.Equal(art, gemeldeteArt);
                Assert.Equal(Direction.NW, richtung);
            }
        }
    }
}
