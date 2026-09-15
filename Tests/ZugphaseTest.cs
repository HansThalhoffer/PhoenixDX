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
        /// Der Abstand, um den die settings-Zeile im Bestand vorauslief: sieben Monate.
        /// </summary>
        private const int Vorlauf = 7;

        /// <summary>
        /// Versetzt die settings-Zeile in den Zustand, in dem die Zugdaten der Spielleitung
        /// ankommen: ihr Monat gehoert zu einem anderen Zug, und ihre Phase steht auf
        /// Bewegungsphase.
        ///
        /// Frueher war das der Zustand des Bestandes und einfach vorzufinden. Inzwischen stellt
        /// die Anwendung den Monat beim ersten Phasenwechsel richtig, und in den Testdaten ist das
        /// bereits geschehen. Der Fall muss deshalb hergestellt werden - sonst prueft ihn niemand
        /// mehr, und genau er hatte die Anwendung lahmgelegt.
        /// </summary>
        private static void ZeileGehoertZuEinemAnderenMonat() {
            var settings = ZugView.Settings!;
            settings.Monat = ProgramView.SelectedMonth + Vorlauf;
            settings.Phase = (int)Zugphase.Bewegungsphase;
            SetzeZurueck();
        }

        /// <summary>
        /// Gehoert die Zeile zu einem anderen Monat, sagt ihre Phase nichts ueber diesen Zug aus.
        ///
        /// So kamen die Zugdaten an: Verzeichnis 168 nannte Monat 175, 169 nannte 176, 170 nannte
        /// 177, waehrend die Schatzkammer mit dem Verzeichnis uebereinstimmte.
        /// </summary>
        [StaFact]
        public void EineZeileAusEinemAnderenMonatZaehltNicht() {
            LadeAlles();
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();

                Assert.NotEqual(ProgramView.SelectedMonth, ZugView.Settings!.Monat);
                Assert.False(ZugView.PhaseStehtInDenZugdaten);

                // und die Schatzkammer haelt es mit dem Verzeichnis
                Assert.Equal(ProgramView.SelectedMonth, ZugView.LetzterSchatzkammerMonat);
            }
            finally {
                ZugView.Settings!.Monat = monatVorher;
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Gehoert die Zeile nicht zu diesem Zug, faengt die Anwendung beim Ruesten an - auch wenn
        /// in der Zeile Bewegungsphase steht.
        /// </summary>
        [StaFact]
        public void OhnePassendeZeileBeginntDerZugMitDemRuesten() {
            LadeAlles();
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();

                Assert.Equal((int)Zugphase.Bewegungsphase, ZugView.Settings!.Phase);
                Assert.False(ZugView.PhaseStehtInDenZugdaten);

                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
                Assert.True(ZugView.KannRüsten);
                Assert.True(ProgramView.CanConstruct());
            }
            finally {
                ZugView.Settings!.Monat = monatVorher;
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Der Phasenwechsel wirkt trotzdem - sonst ginge er ins Leere, weil der gespeicherte Wert
        /// beim Lesen nicht beachtet wird.
        /// </summary>
        [StaFact]
        public void DerPhasenwechselWirktAuchOhnePassendeZeile() {
            LadeAlles();
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);

                var gewechselt = ZugView.NächstePhase();
                Assert.False(gewechselt.HasErrors, $"{gewechselt.Title}: {gewechselt.Message}");

                Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);
                Assert.False(ZugView.KannRüsten);
                Assert.True(ZugView.KannBewegen);

                // ein zweiter Wechsel geht nicht mehr
                Assert.True(ZugView.NächstePhase().HasErrors);
            }
            finally {
                ZugView.Settings!.Monat = monatVorher;
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// Die beendete Ruestphase uebersteht das Schliessen der Anwendung.
        ///
        /// Vorher lag sie nur in einem statischen Feld dieser Sitzung; beim naechsten Start stand
        /// der Zug wieder in der Ruestphase, obwohl es laut Regelwerk kein Zurueck gibt. Gemeldet
        /// wurde es als "Zuege 168, 169, 170: immer nur Ruestphase" - und damit war keine Figur zu
        /// bewegen.
        ///
        /// Das Neuladen wird hier durch BestimmeAktuellenZug nachgestellt: es setzt die
        /// Sitzungsphase zurueck, genau wie ein Neustart.
        /// </summary>
        [StaFact]
        public void DieBeendeteRuestphaseUeberlebtDasNeuladen() {
            LadeAlles();
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
                Assert.False(ZugView.NächstePhase().HasErrors);

                // die Zeile gehoert jetzt zu diesem Zug - das ist der Ort, an dem die Phase bleibt
                Assert.True(ZugView.PhaseStehtInDenZugdaten);
                Assert.Equal(ProgramView.SelectedMonth, ZugView.Settings!.Monat);

                SetzeZurueck();
                Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);
                Assert.True(ZugView.KannBewegen);
            }
            finally {
                ZugView.Settings!.Monat = monatVorher;
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        /// <summary>
        /// In einen anderen Zug leckt sie dagegen nicht hinueber - dort faengt es wieder beim
        /// Ruesten an.
        /// </summary>
        [StaFact]
        public void InEinenAnderenZugLecktDiePhaseNicht() {
            LadeAlles();
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            int zugVorher = ProgramView.SelectedMonth;
            try {
                ZeileGehoertZuEinemAnderenMonat();
                Assert.False(ZugView.NächstePhase().HasErrors);
                Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);

                // der naechste Zug wird geoeffnet
                ProgramView.SelectedMonth = zugVorher + 1;
                SetzeZurueck();

                Assert.False(ZugView.PhaseStehtInDenZugdaten);
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
            }
            finally {
                ProgramView.SelectedMonth = zugVorher;
                ZugView.Settings!.Monat = monatVorher;
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
            int monatVorher = settings.Monat, phaseVorher = settings.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();
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
                settings.Monat = monatVorher;
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
            int monatVorher = ZugView.Settings!.Monat, phaseVorher = ZugView.Settings!.Phase;
            try {
                ZeileGehoertZuEinemAnderenMonat();
                PruefeDassGebautWerdenKann();
            }
            finally {
                ZugView.Settings!.Monat = monatVorher;
                ZugView.Settings!.Phase = phaseVorher;
                SetzeZurueck();
            }
        }

        private static void PruefeDassGebautWerdenKann() {
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
