using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Zugphase - und die Frage, ob die Zugdaten ueberhaupt eine fuehren, die zu diesem Zug
    /// gehoert.
    ///
    /// Im Bestand tun sie es nicht: die settings-Zeile laeuft dem Zugverzeichnis um sieben Monate
    /// voraus (Verzeichnis 168 nennt Monat 175, 169 nennt 176, 170 nennt 177), waehrend die
    /// Schatzkammer mit dem Verzeichnis uebereinstimmt. In dieser Zeile steht ueberall Phase = 1,
    /// also Bewegungsphase. Wer das fuer bare Muenze nahm, konnte nie ruesten und nie bauen.
    /// </summary>
    public class ZugphaseTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Setzt die Sitzungsphase zurueck, indem der Zug neu bestimmt wird.
        /// </summary>
        private static void SetzeZurueck() => ZugView.BestimmeAktuellenZug(ProgramView.SelectedMonth);

        /// <summary>
        /// So sieht der Bestand aus: die settings-Zeile gehoert zu einem anderen Monat.
        /// </summary>
        [StaFact]
        public void DieSettingsZeileGehoertNichtZuDiesemZug() {
            LadeAlles();

            Assert.NotNull(ZugView.Settings);
            Assert.NotEqual(ProgramView.SelectedMonth, ZugView.Settings!.Monat);
            Assert.False(ZugView.PhaseStehtInDenZugdaten);

            // und die Schatzkammer haelt es mit dem Verzeichnis
            Assert.Equal(ProgramView.SelectedMonth, ZugView.LetzterSchatzkammerMonat);
        }

        /// <summary>
        /// Gehoert die Zeile nicht zu diesem Zug, faengt die Anwendung beim Ruesten an - auch wenn
        /// in der Zeile Bewegungsphase steht.
        /// </summary>
        [StaFact]
        public void OhnePassendeZeileBeginntDerZugMitDemRuesten() {
            LadeAlles();
            SetzeZurueck();

            Assert.Equal((int)Zugphase.Bewegungsphase, ZugView.Settings!.Phase);
            Assert.False(ZugView.PhaseStehtInDenZugdaten);

            Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
            Assert.True(ZugView.KannRüsten);
            Assert.True(ProgramView.CanConstruct());
        }

        /// <summary>
        /// Der Phasenwechsel wirkt trotzdem - sonst ginge er ins Leere, weil der gespeicherte Wert
        /// beim Lesen nicht beachtet wird.
        /// </summary>
        [StaFact]
        public void DerPhasenwechselWirktAuchOhnePassendeZeile() {
            LadeAlles();
            SetzeZurueck();
            int phaseVorher = ZugView.Settings!.Phase;
            try {
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);

                var gewechselt = ZugView.NächstePhase();
                Assert.False(gewechselt.HasErrors, $"{gewechselt.Title}: {gewechselt.Message}");

                Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);
                Assert.False(ZugView.KannRüsten);
                Assert.True(ZugView.KannBewegen);

                // ein zweiter Wechsel geht nicht mehr
                Assert.True(ZugView.NächstePhase().HasErrors);

                // ein neu geladener Zug faengt wieder beim Ruesten an - die Phase der letzten
                // Sitzung darf nicht in den naechsten Zug hinueberlecken
                SetzeZurueck();
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
            }
            finally {
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Passt die Zeile zum Zug, gilt ihr Wert.
        /// </summary>
        [StaFact]
        public void MitPassenderZeileGiltDerGespeicherteWert() {
            LadeAlles();
            var settings = ZugView.Settings!;
            int monatVorher = settings.Monat, phaseVorher = settings.Phase;
            try {
                settings.Monat = ProgramView.SelectedMonth;
                Assert.True(ZugView.PhaseStehtInDenZugdaten);

                settings.Phase = (int)Zugphase.Bewegungsphase;
                Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);

                settings.Phase = (int)Zugphase.Rüstphase;
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
            }
            finally {
                settings.Monat = monatVorher;
                settings.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Ein abgeschlossener Zug bleibt abgeschlossen, auch wenn die Zeile sonst nicht passt.
        /// Diese Angabe setzt niemand versehentlich, und zurueck geht es ohnehin nicht.
        /// </summary>
        [StaFact]
        public void EinAbgeschlossenerZugBleibtAbgeschlossen() {
            LadeAlles();
            var settings = ZugView.Settings!;
            int phaseVorher = settings.Phase;
            try {
                SetzeZurueck();
                Assert.False(ZugView.PhaseStehtInDenZugdaten);

                settings.Phase = (int)Zugphase.Abgeschlossen;
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);
                Assert.False(ZugView.KannRüsten);
                Assert.False(ZugView.KannBewegen);

                // auch alles darueber hinaus
                settings.Phase = 99;
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);
            }
            finally {
                settings.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Und das ist der Grund, warum es sich lohnt: auf einer eigenen Gemark laesst sich wieder
        /// bauen. Vorher stand dort in jedem Zug "In der Bewegungsphase wird nicht gebaut".
        /// </summary>
        [StaFact]
        public void AufDerEigenenGemarkLaesstSichWiederBauen() {
            LadeAlles();
            SetzeZurueck();

            var baubar = SharedData.Map!.Values.FirstOrDefault(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && BauoptionenView.IstEtwasMöglich(gemark));

            Assert.True(baubar != null, "Auf keiner eigenen Gemark laesst sich etwas bauen");
            var lage = BauoptionenView.BeschreibeLage(baubar);
            Assert.False(lage.HasErrors, $"{lage.Title}: {lage.Message}");
            Assert.DoesNotContain("Bewegungsphase", lage.Title);
        }
    }
}
