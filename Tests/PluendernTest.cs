using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Pluenderung einer Gemark (Regelwerk 3.2.2.1).
    ///
    /// "Ein eroberungsfaehiges Heer, welches sich in fremdem Gebiet befindet, hat neben dem
    /// Erobern die Moeglichkeit, diese Gemark stattdessen zu pluendern ... Fuer diese
    /// gepluenderte Gemark erhaelt das Heer die doppelten Einnahmen dieser Gemark ...
    /// Gepluenderte Gemarken bringen 8 Monate keine Einnahmen mehr."
    /// </summary>
    public class PluendernTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Eine fremde Gemark, die etwas einbringt und nicht schon gepluendert ist.
        /// </summary>
        private static KleinFeld FindeFremdeGemarkMitEinnahmen() {
            var eigenes = ProgramView.SelectedNation;
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation != null && kf.Nation.Equals(eigenes) == false
                && kf.IsWasser == false && kf.gepluendert == 0
                && EinnahmenView.GetGesamtEinnahmen(kf) > 0);
            Assert.True(gemark != null, "Keine fremde Gemark mit Einnahmen gefunden");
            return gemark!;
        }

        private static Krieger Heer(KleinFeld steht, int nummer = 100)
            => new() {
                Nummer = nummer, Nation = ProgramView.SelectedNation, staerke = 2000, hf = 1,
                gf_von = steht.gf, kf_von = steht.kf,
            };

        /// <summary>
        /// "erhaelt das Heer die doppelten Einnahmen dieser Gemark"
        /// </summary>
        [StaFact]
        public void DiePluenderungBringtDasDoppelteDerEinnahmen() {
            LadeAlles();
            var gemark = FindeFremdeGemarkMitEinnahmen();

            int einnahmen = EinnahmenView.GetGesamtEinnahmen(gemark);
            Assert.Equal(2, PlünderRules.Beutefaktor);
            Assert.Equal(2 * einnahmen, PlünderRules.BerechneBeute(gemark));

            Assert.Equal(0, PlünderRules.BerechneBeute(null));
        }

        /// <summary>
        /// "Gepluenderte Gemarken bringen 8 Monate keine Einnahmen mehr" - und was nichts
        /// einbringt, laesst sich auch nicht pluendern.
        /// </summary>
        [StaFact]
        public void NachDerPluenderungAchtMonateNichts() {
            LadeAlles();
            var gemark = FindeFremdeGemarkMitEinnahmen();
            int vorher = gemark.gepluendert;
            try {
                int einnahmen = EinnahmenView.GetGesamtEinnahmen(gemark);
                Assert.True(einnahmen > 0);

                int beute = PlünderRules.Plündere(gemark);
                Assert.Equal(2 * einnahmen, beute);
                Assert.Equal(8, PlünderRules.MonateOhneEinnahmen);
                Assert.Equal(8, gemark.gepluendert);
                Assert.True(PlünderRules.IstGeplündert(gemark));

                // ab jetzt bringt sie nichts mehr
                Assert.Equal(0, EinnahmenView.GetGesamtEinnahmen(gemark));
                Assert.Equal(0, PlünderRules.BerechneBeute(gemark));

                // und das Herunterzaehlen bringt sie nach acht Monaten zurueck
                for (int monat = 1; monat <= 8; monat++) {
                    Assert.True(PlünderRules.ZähleHerunter(gemark));
                    Assert.Equal(8 - monat, PlünderRules.GetRestmonate(gemark));
                }
                Assert.False(PlünderRules.IstGeplündert(gemark));
                Assert.False(PlünderRules.ZähleHerunter(gemark));
                Assert.Equal(einnahmen, EinnahmenView.GetGesamtEinnahmen(gemark));
            }
            finally {
                gemark.gepluendert = vorher;
            }
        }

        /// <summary>
        /// Nur ein eroberungsfaehiges Heer pluendert, und nur in fremdem Gebiet.
        /// </summary>
        [StaFact]
        public void NurEroberungsfaehigUndNurInFremdemGebiet() {
            LadeAlles();
            var fremd = FindeFremdeGemarkMitEinnahmen();

            var heer = Heer(fremd);
            var erlaubt = PlünderRules.Prüfe(heer, fremd);
            Assert.False(erlaubt.HasErrors, $"{erlaubt.Title}: {erlaubt.Message}");

            // zu klein
            var klein = Heer(fremd, nummer: 101);
            klein.staerke = 100;
            var zuKlein = PlünderRules.Prüfe(klein, fremd);
            Assert.True(zuKlein.HasErrors);
            Assert.Contains("nicht eroberungsfähig", zuKlein.Title);

            // eigenes Gebiet
            var eigen = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation != null && kf.Nation.Equals(ProgramView.SelectedNation) && kf.IsWasser == false);
            Assert.True(eigen != null, "Das eigene Reich hat keine Landgemark");
            var imEigenen = PlünderRules.Prüfe(Heer(eigen!, nummer: 102), eigen);
            Assert.True(imEigenen.HasErrors);
            Assert.Contains("kein fremdes Gebiet", imEigenen.Title);

            Assert.True(PlünderRules.Prüfe(null, fremd).HasErrors);
            Assert.True(PlünderRules.Prüfe(heer, null).HasErrors);
        }

        /// <summary>
        /// "Da die Pluenderung einer Gemark 1 Monat dauert, kann sich das pluendernde Heer diesen
        /// Monat nicht bewegen."
        /// </summary>
        [StaFact]
        public void WerSichBewegtHatPluendertNicht() {
            LadeAlles();
            var fremd = FindeFremdeGemarkMitEinnahmen();
            var nachbar = KleinfeldView.GetNachbarn(fremd, 1, includeSelf: false)!.First();

            // von woanders hergezogen: die Ausgangsposition ist eine andere
            var gezogen = Heer(fremd);
            gezogen.gf_von = nachbar.gf;
            gezogen.kf_von = nachbar.kf;
            gezogen.gf_nach = fremd.gf;
            gezogen.kf_nach = fremd.kf;

            var bewegt = PlünderRules.Prüfe(gezogen, fremd);
            Assert.True(bewegt.HasErrors);
            Assert.Contains("bewegt", bewegt.Title);

            // stehen geblieben: geht
            var steht = Heer(fremd, nummer: 103);
            Assert.False(PlünderRules.Prüfe(steht, fremd).HasErrors);
        }

        /// <summary>
        /// Eine bereits gepluenderte Gemark bringt nichts mehr - auch keinem zweiten Pluenderer.
        /// </summary>
        [StaFact]
        public void ZweimalPluendernBringtNichts() {
            LadeAlles();
            var gemark = FindeFremdeGemarkMitEinnahmen();
            int vorher = gemark.gepluendert;
            try {
                PlünderRules.Plündere(gemark);

                var nochmal = PlünderRules.Prüfe(Heer(gemark), gemark);
                Assert.True(nochmal.HasErrors);
                Assert.Contains("bereits geplündert", nochmal.Title);
            }
            finally {
                gemark.gepluendert = vorher;
            }
        }

        /// <summary>
        /// Das Herunterzaehlen ueber die ganze Karte laeuft, ohne etwas kaputtzumachen.
        /// </summary>
        [StaFact]
        public void DasHerunterzaehlenLaeuftUeberDieGanzeKarte() {
            LadeAlles();

            var betroffen = SharedData.Map!.Values.Where(kf => kf.gepluendert > 0).ToList();
            var vorher = betroffen.ToDictionary(kf => kf.Key, kf => kf.gepluendert);
            try {
                int übrig = PlünderRules.ZähleAlleHerunter();

                foreach (var gemark in betroffen)
                    Assert.Equal(vorher[gemark.Key] - 1, gemark.gepluendert);
                Assert.Equal(betroffen.Count(kf => vorher[kf.Key] > 1), übrig);
            }
            finally {
                foreach (var gemark in betroffen)
                    gemark.gepluendert = vorher[gemark.Key];
            }
        }
    }
}
