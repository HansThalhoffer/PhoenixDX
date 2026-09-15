using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;

namespace Tests {

    /// <summary>
    /// Was gemacht wurde, steht unter "Aktueller Zug" - sonst laesst es sich nicht zuruecknehmen.
    ///
    /// Gemeldet wurde: die Bewegung per Klick auf ein hervorgehobenes Feld hat funktioniert, tauchte
    /// aber in der Liste nicht auf. Die Liste ist der einzige Weg zum Undo; was dort fehlt, ist
    /// endgueltig.
    /// </summary>
    public class ZuglisteTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>Eine eigene Figur, die sich tatsaechlich bewegen kann</summary>
        private static (Spielfigur Figur, PhoenixModel.dbErkenfara.KleinFeld Ziel) FindeZug() {
            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                         .Where(Plausibilität.IsValid)) {
                var eigenes = KleinfeldView.GetKleinfeld(figur);
                if (eigenes == null)
                    continue;
                var ziel = BewegungsRules.GetErreichbareFelder(figur)
                    .FirstOrDefault(feld => feld.Bezeichner != eigenes.Bezeichner
                        && TeleportRules.IstTeleportfeld(feld) == false);
                if (ziel != null)
                    return (figur, ziel);
            }
            Assert.Fail("Keine eigene Figur kann sich bewegen");
            return default;
        }

        /// <summary>
        /// Eine ausgefuehrte Bewegung steht in der Liste des laufenden Zuges und laesst sich von
        /// dort zuruecknehmen.
        /// </summary>
        [StaFact]
        public void EineBewegungStehtInDerListeDesLaufendenZuges() {
            LadeAlles();
            var (figur, ziel) = FindeZug();
            int vorher = SharedData.Commands.Count;

            var ergebnis = Bewegungssteuerung.BewegeZuZielfeld(figur, ziel);
            Assert.False(ergebnis.HasErrors, $"{ergebnis.Title}: {ergebnis.Message}");

            var bewegung = SharedData.Commands.OfType<MoveCommand>()
                .FirstOrDefault(befehl => ReferenceEquals(befehl.GetSpielfigur(), figur)
                    && befehl.IsExecuted);
            try {
                Assert.True(bewegung != null,
                    $"Die Bewegung von {figur.Bezeichner} nach {ziel.Bezeichner} steht nicht in der Liste "
                    + $"des laufenden Zuges ({vorher} Eintraege vorher, {SharedData.Commands.Count} nachher)");
                Assert.True(bewegung!.GehörtZumAktuellenZug);
                Assert.True(bewegung.CanUndo, "Die Bewegung steht in der Liste, laesst sich aber nicht zuruecknehmen");
            }
            finally {
                if (bewegung != null)
                    SharedData.Commands.Undo(bewegung);
            }
        }

        /// <summary>
        /// Auch eine Bewegung, die von der Karte aus ausgeloest wird, kommt in der Liste an.
        ///
        /// Die Karte laeuft in einem eigenen Thread (MappaMundi startet ihn), und der Klick auf ein
        /// Feld wird von dort aus gemeldet. Die Sammlung muss das aushalten.
        ///
        /// In CommandSet stand dafuer einmal ein Umweg ueber einen SynchronizationContext, der nie
        /// gesetzt wurde - haette ihn jemand mit einem Post auf einen ruhenden Kontext verdrahtet,
        /// waere der Befehl nie angekommen. Dieser Test faellt dann um.
        /// </summary>
        [StaFact]
        public void AuchVomKartenthreadAusKommtSieAn() {
            LadeAlles();
            var (figur, ziel) = FindeZug();

            CommandResult? ergebnis = null;
            Exception? geflogen = null;
            var kartenthread = new Thread(() => {
                try {
                    ergebnis = Bewegungssteuerung.BewegeZuZielfeld(figur, ziel);
                }
                catch (Exception ex) {
                    geflogen = ex;
                }
            });
            kartenthread.SetApartmentState(ApartmentState.STA);
            kartenthread.Start();
            Assert.True(kartenthread.Join(TimeSpan.FromSeconds(30)), "Der Kartenthread kam nicht zurueck");

            Assert.True(geflogen == null, $"Die Bewegung warf: {geflogen}");
            Assert.True(ergebnis != null && ergebnis.HasErrors == false,
                $"{ergebnis?.Title}: {ergebnis?.Message}");

            var bewegung = SharedData.Commands.OfType<MoveCommand>()
                .LastOrDefault(befehl => ReferenceEquals(befehl.GetSpielfigur(), figur) && befehl.IsExecuted);
            try {
                Assert.True(bewegung != null,
                    $"Die vom Kartenthread ausgeloeste Bewegung von {figur.Bezeichner} fehlt in der Liste");
                Assert.True(bewegung!.CanUndo);
            }
            finally {
                if (bewegung != null)
                    SharedData.Commands.Undo(bewegung);
            }
        }

        /// <summary>
        /// Dasselbe, nachdem die bereits gespeicherten Bewegungen wieder zu Befehlen geworden sind.
        ///
        /// Das macht die Anwendung beim Laden jedes Zuges (BewegungView.RekonstruiereAlleBewegungen),
        /// der Testlauf sonst nicht. Fast jede Figur in den Zugdaten ist in diesem Monat schon
        /// gezogen; ihr Befehl steht dann bereits in der Liste, und ein zweiter kommt dazu.
        /// </summary>
        [StaFact]
        public void AuchNebenEinerWiederhergestelltenBewegung() {
            LadeAlles();

            // so wie beim Laden: erst herstellen, dann in die Liste schieben
            BewegungView.RekonstruiereAlleBewegungen();
            while (SharedData.CommandQueue.TryDequeue(out var wiederhergestellt))
                SharedData.Commands.Add(wiederhergestellt);

            var (figur, ziel) = FindeZug();
            var ergebnis = Bewegungssteuerung.BewegeZuZielfeld(figur, ziel);
            Assert.False(ergebnis.HasErrors, $"{ergebnis.Title}: {ergebnis.Message}");

            var bewegung = SharedData.Commands.OfType<MoveCommand>()
                .LastOrDefault(befehl => ReferenceEquals(befehl.GetSpielfigur(), figur) && befehl.IsExecuted);
            try {
                Assert.True(bewegung != null,
                    $"Die Bewegung von {figur.Bezeichner} nach {ziel.Bezeichner} fehlt in der Liste des laufenden Zuges");
                Assert.True(bewegung!.CanUndo, "Die Bewegung laesst sich nicht zuruecknehmen");
            }
            finally {
                if (bewegung != null)
                    SharedData.Commands.Undo(bewegung);
            }
        }
    }
}
