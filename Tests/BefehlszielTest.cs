using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Zu welcher Gemark ein Befehl gehoert.
    ///
    /// Gebraucht fuer die Liste des laufenden Zuges: wer dort einen Eintrag anklickt, springt auf
    /// der Karte dorthin, wo der Befehl gewirkt hat. Bei einer Bewegung ist das das Ziel und nicht
    /// der Ausgangspunkt - dort steht das Heer, wenn man nachsehen will.
    /// </summary>
    public class BefehlszielTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>
        /// Eine Bewegung zeigt auf ihr Ziel.
        /// </summary>
        [StaFact]
        public void EineBewegungZeigtAufIhrZiel() {
            LadeAlles();

            var von = new KleinfeldPosition(305, 24);
            var nach = new KleinfeldPosition(305, 25);
            var bewegung = new MoveCommand("Bewege Heer 1 von 305/24 nach 305/25") {
                FromLocation = von,
                ToLocation = nach,
            };

            Assert.Equal(nach, BefehlszielView.GetZielfeld(bewegung));
            Assert.True(BefehlszielView.HatZielfeld(bewegung));

            // ohne Ziel bleibt der Ausgangspunkt - besser dorthin als nirgendwohin
            bewegung.ToLocation = null;
            Assert.Equal(von, BefehlszielView.GetZielfeld(bewegung));
        }

        /// <summary>
        /// Ein Bauauftrag zeigt auf die Gemark, auf der gebaut wird.
        /// </summary>
        [StaFact]
        public void EinBauauftragZeigtAufSeineGemark() {
            LadeAlles();

            var feld = SharedData.Map!.Values.First(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && BauoptionenView.Bestimme(gemark)
                    .Any(o => o.Art == ConstructionElementType.Strasse && o.Möglich));
            var richtung = BauoptionenView.Bestimme(feld)
                .First(o => o.Art == ConstructionElementType.Strasse && o.Möglich).Richtung!.Value;

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Straße im {richtung} von {feld.CreateBezeichner()}", out var befehl));
            var bau = Assert.IsType<ConstructCommand>(befehl);

            var ziel = BefehlszielView.GetZielfeld(bau);
            Assert.NotNull(ziel);
            Assert.Equal(feld.CreateBezeichner(), ziel!.CreateBezeichner());
        }

        /// <summary>
        /// Ein Befehl, der auf keine Gemark zeigt, sagt das - dann springt die Karte nicht.
        /// </summary>
        [StaFact]
        public void OhneGemarkGibtEsKeinZiel() {
            LadeAlles();

            Assert.Null(BefehlszielView.GetZielfeld(null));
            Assert.False(BefehlszielView.HatZielfeld(null));

            // ein frischer Befehl ohne Ort und ohne bearbeitetes Element
            var ohneOrt = new ConstructCommand("Errichte Straße im NW von 1/1");
            Assert.Null(BefehlszielView.GetZielfeld(ohneOrt));
        }

        /// <summary>
        /// Die Befehle des laufenden Zuges zeigen ueberwiegend auf eine Gemark - sonst waere die
        /// Spalte in der Liste leer.
        ///
        /// Geprueft wird gegen die Befehlsliste, wie sie gerade aussieht. Was darin steht, haengt
        /// davon ab, was vorher lief - die Liste ist geteilter Zustand. Der Test verlangt deshalb
        /// nur dort ein Zielfeld, wo es eines geben muss: bei einem Befehl, der ein Feld
        /// mitbringt. Vorher verlangte er es von irgendeinem Befehl der Liste und fiel um, sobald
        /// zufaellig nur solche darin standen, die auf kein Feld zeigen - einmal in vier
        /// Durchlaeufen.
        /// </summary>
        [StaFact]
        public void DieBefehleDesZugesZeigenAufGemarken() {
            LadeAlles();

            var befehle = SharedData.Commands.ToList();

            // kein einziger Aufruf wirft, egal welcher Befehlstyp
            foreach (var befehl in befehle)
                BefehlszielView.GetZielfeld(befehl);

            // Ein Bauauftrag bringt sein Feld mit - der muss eines nennen. Steht keiner in der
            // Liste, wird einer gebaut, damit der Test etwas prueft statt auszusetzen.
            var mitFeld = befehle.OfType<ConstructCommand>().FirstOrDefault(bau => bau.Location != null);
            if (mitFeld != null) {
                Assert.True(BefehlszielView.HatZielfeld(mitFeld),
                    $"{mitFeld.CommandString} bringt ein Feld mit, nennt aber keines");
                return;
            }

            var feld = SharedData.Map!.Values.First(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation));
            var eigener = new ConstructCommand($"Errichte Burg auf {feld.CreateBezeichner()}") {
                What = ConstructionElementType.Burg,
                Location = new KleinfeldPosition(feld.gf, feld.kf),
            };
            Assert.True(BefehlszielView.HatZielfeld(eigener));
            Assert.Equal(feld.CreateBezeichner(), BefehlszielView.GetZielfeld(eigener)!.CreateBezeichner());
        }
    }
}
