using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Fenstertitel zeigt den Spielstand: fuer welches Reich gespielt wird, welcher Zug laeuft,
    /// in welchem Monat er liegt, was fuer ein Monat das ist und in welcher Phase der Zug steckt.
    ///
    /// "Was fuer ein Monat" hat zwei Seiten: Ruest- und Einnahmemonat kommen aus dem Jahreslauf
    /// (Regelwerk 3.2 und 3.3), die Phase aus dem eigenen Zug.
    /// </summary>
    public class TitelzeileTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Alles vier steht drin - und in dieser Reihenfolge.
        /// </summary>
        [StaFact]
        public void DerTitelNenntReichZugMonatUndPhase() {
            LadeAlles();
            var zug = ZugView.AktuellerZug;
            Assert.True(zug.IstGültig, "Es ist kein gueltiger Zug geladen");

            string titel = ZugView.Titelzeile;

            Assert.StartsWith(ZugView.Anwendungsname, titel);
            Assert.Contains(ProgramView.SelectedNation!.Reich, titel);
            Assert.Contains($"Zug {zug.Zug}", titel);
            Assert.Contains(zug.Name, titel);
            Assert.Contains(ZugView.PhasenBeschreibung, titel);

            // die Reihenfolge: Reich vor Zug, Zug vor Phase
            Assert.True(titel.IndexOf(ProgramView.SelectedNation.Reich) < titel.IndexOf($"Zug {zug.Zug}"));
            Assert.True(titel.IndexOf($"Zug {zug.Zug}") < titel.LastIndexOf(ZugView.PhasenBeschreibung));
        }

        /// <summary>
        /// Die Art der Runde steht dabei: ein Ruestmonat heisst Ruestmonat, ein Einnahmemonat
        /// Einnahmemonat, und die Phase sagt, was der Spieler gerade tun darf.
        /// </summary>
        [StaFact]
        public void DerTitelNenntDieArtDerRunde() {
            LadeAlles();
            var zug = ZugView.AktuellerZug;
            string titel = ZugView.Titelzeile;

            if (zug.IstRüstmonat)
                Assert.Contains("Rüstmonat", titel);
            else if (zug.IstEinnahmemonat)
                Assert.Contains("Einnahmemonat", titel);
            else
                Assert.DoesNotContain("monat", titel);

            // die Jahreszeit gehoert zum Monat und steht deshalb auch da
            Assert.Contains(zug.Jahreszeit.ToString(), titel);

            // und die Phase wechselt mit
            var vorher = ZugView.Phase;
            try {
                SharedData.ZugdatenSettings!.Last().Phase = (int)Zugphase.Rüstphase;
                Assert.Contains("Rüstphase", ZugView.Titelzeile);

                SharedData.ZugdatenSettings.Last().Phase = (int)Zugphase.Bewegungsphase;
                Assert.Contains("Bewegungsphase", ZugView.Titelzeile);
                Assert.DoesNotContain("Rüstphase", ZugView.Titelzeile);
            }
            finally {
                SharedData.ZugdatenSettings!.Last().Phase = (int)vorher;
            }
        }

        /// <summary>
        /// Was nicht geladen ist, steht auch nicht im Titel - dann sagt er das.
        /// </summary>
        [StaFact]
        public void OhneDatenSagtDerTitelDas() {
            LadeAlles();
            var reich = ProgramView.SelectedNation;
            int monat = ProgramView.SelectedMonth;
            try {
                ProgramView.SelectedNation = null;
                ProgramView.SelectedMonth = 0;

                string titel = ZugView.Titelzeile;
                Assert.Equal($"{ZugView.Anwendungsname} - keine Zugdaten geladen", titel);
            }
            finally {
                ProgramView.SelectedNation = reich;
                ProgramView.SelectedMonth = monat;
            }
        }

        /// <summary>
        /// Ein Reich ohne Zugdaten - etwa direkt nach der Anmeldung - nennt schon das Reich.
        /// </summary>
        [StaFact]
        public void OhneZugStehtWenigstensDasReichDa() {
            LadeAlles();
            var reich = ProgramView.SelectedNation;
            int monat = ProgramView.SelectedMonth;
            try {
                ProgramView.SelectedMonth = 0;

                string titel = ZugView.Titelzeile;
                Assert.Contains(reich!.Reich, titel);
                Assert.DoesNotContain("Zug ", titel);
                Assert.DoesNotContain("keine Zugdaten geladen", titel);
            }
            finally {
                ProgramView.SelectedNation = reich;
                ProgramView.SelectedMonth = monat;
            }
        }
    }
}
