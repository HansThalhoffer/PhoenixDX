using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Zuruecknehmen laesst sich nur, was im laufenden Zug gemacht wurde.
    ///
    /// Ein Befehl aus einem vergangenen Monat ist ausgewertet: die Spielleitung hat ihn gerechnet,
    /// das Ergebnis steht in den Daten des Folgemonats. Ihn hier zurueckzunehmen wuerde die eigene
    /// Karte gegen die Auswertung verschieben, ohne dass es jemandem auffiele.
    ///
    /// Die Befehle kennen deshalb ihren Zug. Wiederhergestellte Bauauftraege tragen den Monat
    /// ihres Auftrags nach, nicht den Augenblick, in dem sie wiederhergestellt wurden.
    /// </summary>
    public class ZurueknehmenTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        private static ConstructCommand BaueEineStrasse() {
            var feld = SharedData.Map!.Values.First(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && BauoptionenView.Bestimme(gemark)
                    .Any(o => o.Art == ConstructionElementType.Strasse && o.Möglich));
            var richtung = BauoptionenView.Bestimme(feld)
                .First(o => o.Art == ConstructionElementType.Strasse && o.Möglich).Richtung!.Value;

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Straße im {richtung} von {feld.CreateBezeichner()}", out var befehl));
            var bau = Assert.IsType<ConstructCommand>(befehl);
            Assert.False(bau.ExecuteCommand().HasErrors);
            return bau;
        }

        /// <summary>
        /// Ein neuer Befehl gehoert dem Zug, in dem er gegeben wurde.
        /// </summary>
        [StaFact]
        public void EinNeuerBefehlGehoertDemLaufendenZug() {
            LadeAlles();
            var bau = BaueEineStrasse();
            try {
                Assert.Equal(ProgramView.SelectedMonth, bau.Zug);
                Assert.True(bau.GehörtZumAktuellenZug);
            }
            finally {
                bau.UndoCommand();
            }
        }

        /// <summary>
        /// Was im laufenden Zug gemacht wurde, laesst sich zuruecknehmen.
        /// </summary>
        [StaFact]
        public void AusDemLaufendenZugLaesstSichZuruecknehmen() {
            LadeAlles();
            var bau = BaueEineStrasse();

            Assert.True(SharedData.Commands.Undo(bau));
        }

        /// <summary>
        /// Was aus einem vergangenen Zug stammt, nicht.
        /// </summary>
        [StaFact]
        public void AusEinemVergangenenZugNicht() {
            LadeAlles();
            var bau = BaueEineStrasse();
            try {
                // derselbe Befehl, nur einen Monat aelter
                bau.Zug = ProgramView.SelectedMonth - 1;
                Assert.False(bau.GehörtZumAktuellenZug);

                Assert.False(SharedData.Commands.Undo(bau));

                // und er ist auch nicht heimlich doch zurueckgenommen worden
                Assert.True(bau.IsExecuted);
            }
            finally {
                bau.Zug = ProgramView.SelectedMonth;
                bau.UndoCommand();
            }
        }

        /// <summary>
        /// Die Liste "Aktueller Zug" nimmt nur auf, was zum laufenden Zug gehoert.
        /// </summary>
        [StaFact]
        public void DieListeNimmtNurDenLaufendenZugAuf() {
            LadeAlles();
            int vorher = SharedData.Commands.Count;

            var ausAnderemZug = new ConstructCommand("Errichte Straße im NW von 1/1") {
                Zug = ProgramView.SelectedMonth - 1,
            };
            SharedData.Commands.Add(ausAnderemZug);
            Assert.Equal(vorher, SharedData.Commands.Count);
            Assert.DoesNotContain(ausAnderemZug, SharedData.Commands);

            var ausDiesemZug = new ConstructCommand("Errichte Straße im NO von 1/1");
            try {
                SharedData.Commands.Add(ausDiesemZug);
                Assert.Contains(ausDiesemZug, SharedData.Commands);
            }
            finally {
                SharedData.Commands.Remove(ausDiesemZug);
            }
        }

        /// <summary>
        /// Und was schon drinsteht, fliegt beim Zugwechsel heraus.
        /// </summary>
        [StaFact]
        public void BeimZugwechselFliegenFremdeBefehleHeraus() {
            LadeAlles();

            // ein Befehl des laufenden Zuges kommt hinein und wird nachtraeglich einem anderen
            // Monat zugeschlagen - so, wie es beim Wechsel des Zuges passiert
            var befehl = new ConstructCommand("Errichte Straße im O von 1/1");
            SharedData.Commands.Add(befehl);
            Assert.Contains(befehl, SharedData.Commands);

            befehl.Zug = ProgramView.SelectedMonth - 1;
            int gegangen = SharedData.Commands.EntferneFremdeZüge();

            Assert.True(gegangen >= 1);
            Assert.DoesNotContain(befehl, SharedData.Commands);
            Assert.All(SharedData.Commands, b => Assert.True(b.GehörtZumAktuellenZug));
        }

        /// <summary>
        /// Auch ein Befehl aus einem kuenftigen Monat gehoert nicht hierher - das faellt an, wenn
        /// jemand einen alten Zug oeffnet, nachdem er im neuen schon gearbeitet hat.
        /// </summary>
        [StaFact]
        public void AusEinemAnderenZugUeberhauptNicht() {
            LadeAlles();
            var bau = BaueEineStrasse();
            try {
                bau.Zug = ProgramView.SelectedMonth + 1;
                Assert.False(bau.GehörtZumAktuellenZug);
                Assert.False(SharedData.Commands.Undo(bau));
            }
            finally {
                bau.Zug = ProgramView.SelectedMonth;
                bau.UndoCommand();
            }
        }
    }
}
