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
        /// </summary>
        [StaFact]
        public void DieBefehleDesZugesZeigenAufGemarken() {
            LadeAlles();

            var befehle = SharedData.Commands.ToList();
            if (befehle.Count == 0)
                return; // in diesem Zug wurde nichts gemacht, dann ist hier nichts zu pruefen

            // kein einziger Aufruf wirft, egal welcher Befehlstyp
            foreach (var befehl in befehle)
                BefehlszielView.GetZielfeld(befehl);

            Assert.Contains(befehle, befehl => BefehlszielView.HatZielfeld(befehl));
        }
    }
}
