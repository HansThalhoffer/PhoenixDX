using PhoenixModel.dbZugdaten;
using PhoenixModel.Extensions;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Regeln des Ruestens - Issue #65 (Regelwerk 3.3 und 3.3.1).
    ///
    /// Geprueft wird gegen die echten Karten- und Zugdaten; veraendert wird nur im Speicher.
    /// </summary>
    public class RuestenTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>Ein eigener Ruestort, in dem sich ruesten laesst</summary>
        private static PhoenixModel.dbErkenfara.KleinFeld FindeRuestort() {
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation == ProgramView.SelectedNation && RuestRules.GetKapazität(kf).Goldstücke > 0);
            Assert.True(gemark != null, "Das eigene Reich hat keinen Rüstort");
            return gemark!;
        }

        /// <summary>
        /// Die Ruestkapazitaeten stehen so im Regelwerk 3.3. Sie kommen aus der Crossreferenz -
        /// dieser Test haelt fest, dass die Daten dem Regelwerk entsprechen.
        /// </summary>
        [StaFact]
        public void DieRuestkapazitaetenEntsprechenDemRegelwerk() {
            LadeAlles();

            (string Name, int GS, int HF, int Z)[] erwartet = [
                ("Burg", 10000, 2, 0),
                ("Stadt", 25000, 5, 1),
                ("Festung", 40000, 8, 2),
                ("Hauptstadt", 50000, 10, 3),
            ];

            foreach (var (name, gs, hf, z) in erwartet) {
                var rüstort = SharedData.RüstortReferenz!.FirstOrDefault(r => r.Ruestort == name);
                Assert.True(rüstort != null, $"In der Crossreferenz fehlt der Rüstort {name}");
                Assert.Equal(gs, rüstort!.KapazitätTruppen);
                Assert.Equal(hf, rüstort.KapazitätHF);
                Assert.Equal(z, rüstort.KapazitätZ);
            }
        }

        /// <summary>
        /// Aus besonderen Einnahmen wird halbiert und abgerundet geruestet (Regelwerk 3.3.1).
        /// Bei der Burg heisst das: kein Zauberer, denn 0 halbiert bleibt 0 - und bei der Stadt
        /// faellt der eine Zauberer weg, weil abgerundet wird.
        /// </summary>
        [StaFact]
        public void BesondereEinnahmenHalbierenUndRundenAb() {
            LadeAlles();
            var stadt = SharedData.RüstortReferenz!.First(r => r.Ruestort == "Stadt");
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation == ProgramView.SelectedNation
                && BauwerkeView.GetRüstortNachKarte(kf)?.Ruestort == "Stadt");
            if (gemark == null)
                return; // das eigene Reich hat gerade keine Stadt

            var voll = RuestRules.GetKapazität(gemark, ausBesonderenEinnahmen: false);
            var halb = RuestRules.GetKapazität(gemark, ausBesonderenEinnahmen: true);

            Assert.Equal(25000, voll.Goldstücke);
            Assert.Equal(12500, halb.Goldstücke);
            Assert.Equal(5, voll.Heerführer);
            Assert.Equal(2, halb.Heerführer);
            // 1 Zauberer halbiert und abgerundet ist keiner
            Assert.Equal(1, voll.Zauberer);
            Assert.Equal(0, halb.Zauberer);
        }

        /// <summary>
        /// Auf einem Feld ohne Ruestort laesst sich nicht ruesten - ein Dorf ist keiner.
        /// </summary>
        [StaFact]
        public void OhneRuestortGehtNichts() {
            LadeAlles();
            var ohne = SharedData.Map!.Values.First(kf =>
                kf.Nation == ProgramView.SelectedNation && RuestRules.GetKapazität(kf).Goldstücke == 0);

            var ergebnis = RuestRules.PrüfeRüstort(ohne);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("kein Rüstort", ergebnis.Title);

            Assert.True(RuestRules.PrüfeRüstort(null).HasErrors);
        }

        /// <summary>
        /// In einem fremden Ruestort laesst sich nicht ruesten.
        /// </summary>
        [StaFact]
        public void InFremdenRuestortenGehtNichts() {
            LadeAlles();
            var fremd = SharedData.Map!.Values.First(kf =>
                kf.Nation != null && kf.Nation != ProgramView.SelectedNation
                && RuestRules.GetKapazität(kf).Goldstücke > 0);

            var ergebnis = RuestRules.PrüfeRüstort(fremd);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("eigenen Reich", ergebnis.Title);
        }

        /// <summary>
        /// Ein Auftrag, der die Kapazitaet sprengt, wird abgelehnt - und die Meldung nennt die Zahlen.
        /// </summary>
        [StaFact]
        public void WerZuVielRuestetBekommtEineAbsage() {
            LadeAlles();
            var gemark = FindeRuestort();
            var kapazität = RuestRules.GetKapazität(gemark);

            // so viele Krieger, dass die Goldgrenze sicher gerissen ist
            int proKrieger = KostenView.GetGSKosten(PhoenixModel.Commands.ConstructionElementType.K);
            Assert.True(proKrieger > 0, "Ein Krieger kostet laut Kostentabelle nichts");
            var zuViel = new Ruestung { gf = gemark.gf, kf = gemark.kf, K = (kapazität.Goldstücke / proKrieger) + 1000 };

            var ergebnis = RuestRules.Prüfe(gemark, zuViel);
            Assert.True(ergebnis.HasErrors, "Ein zu grosser Auftrag wurde durchgelassen");
            Assert.Contains("Rüstkapazität", ergebnis.Title);
        }

        /// <summary>
        /// Zu viele Heerfuehrer werden ebenso abgelehnt - unabhaengig vom Geld.
        /// </summary>
        [StaFact]
        public void ZuVieleHeerfuehrerWerdenAbgelehnt() {
            LadeAlles();
            var gemark = FindeRuestort();
            var kapazität = RuestRules.GetKapazität(gemark);

            var zuViele = new Ruestung { gf = gemark.gf, kf = gemark.kf, HF = kapazität.Heerführer + 1 };
            var ergebnis = RuestRules.Prüfe(gemark, zuViele);

            Assert.True(ergebnis.HasErrors);
            Assert.Contains("Heerführer", ergebnis.Title);
        }

        /// <summary>
        /// Mit dem Reichsschatz wird nur in der Ruestphase geruestet; mit besonderen Einnahmen in
        /// jedem Monat (Regelwerk 3.3.1).
        /// </summary>
        [StaFact]
        public void MitDemReichsschatzNurInDerRuestphase() {
            LadeAlles();
            var gemark = FindeRuestort();
            var kleiner = new Ruestung { gf = gemark.gf, kf = gemark.kf, K = 1 };

            int phaseVorher = ZugView.Settings!.Phase;
            try {
                TestSetup.SetzePhase(Zugphase.Bewegungsphase);

                var ausSchatz = RuestRules.Prüfe(gemark, kleiner, ausBesonderenEinnahmen: false);
                Assert.True(ausSchatz.HasErrors, "In der Bewegungsphase darf der Reichsschatz nicht verrüstet werden");
                Assert.Contains("wird nicht gerüstet", ausSchatz.Title);

                // aus besonderen Einnahmen scheitert es jedenfalls nicht an der Phase
                var ausEinnahmen = RuestRules.Prüfe(gemark, kleiner, ausBesonderenEinnahmen: true);
                Assert.DoesNotContain("wird nicht gerüstet", ausEinnahmen.Title);
            }
            finally {
                TestSetup.SetzePhase((Zugphase)phaseVorher);
            }
        }

        /// <summary>
        /// Schiffe brauchen Wasser neben einem eigenen Ruestort in der Hoehenstufe 1
        /// (Regelwerk 3.3).
        /// </summary>
        [StaFact]
        public void SchiffeBrauchenWasserNebenEinemRuestort() {
            LadeAlles();

            // Land geht nie
            var land = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var aufLand = RuestRules.PrüfeSchiffsplatz(land);
            Assert.True(aufLand.HasErrors);
            Assert.Contains("kein Wasser", aufLand.Title);

            Assert.True(RuestRules.PrüfeSchiffsplatz(null).HasErrors);

            // und irgendwo im offenen Wasser, weit weg von allem, auch nicht
            var offenesWasser = SharedData.Map!.Values.FirstOrDefault(kf => kf.IsWasser
                && Enum.GetValues<Direction>().All(r => {
                    var n = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(kf, r));
                    return n == null || RuestRules.PrüfeRüstort(n).HasErrors;
                }));
            Assert.True(offenesWasser != null, "Es gibt kein Wasserfeld ohne Rüstort in der Nachbarschaft");
            Assert.True(RuestRules.PrüfeSchiffsplatz(offenesWasser).HasErrors);
        }

        /// <summary>
        /// Was schon geruestet wurde, zaehlt gegen die Kapazitaet. Sonst liesse sich die Grenze
        /// durch mehrere kleine Auftraege umgehen.
        /// </summary>
        [StaFact]
        public void BereitsGeruestetesZaehltGegenDieKapazitaet() {
            LadeAlles();
            var gemark = FindeRuestort();
            var kapazität = RuestRules.GetKapazität(gemark);
            int proKrieger = KostenView.GetGSKosten(PhoenixModel.Commands.ConstructionElementType.K);

            // ein Auftrag, der die Kapazitaet fast ausschoepft
            int fast = (kapazität.Goldstücke / proKrieger) - 1;
            var vorhanden = new Ruestung { gf = gemark.gf, kf = gemark.kf, K = fast };

            var vorher = RuestRules.BerechneBereitsGerüstet(gemark);
            SharedData.Ruestung!.ReopenSharedData().Add(vorhanden);
            try {
                var jetzt = RuestRules.BerechneBereitsGerüstet(gemark);
                Assert.Equal(vorher.Goldstücke + fast * proKrieger, jetzt.Goldstücke);

                // jetzt passt nicht einmal mehr eine Handvoll Krieger hinein
                var nachschlag = new Ruestung { gf = gemark.gf, kf = gemark.kf, K = 100 };
                Assert.True(RuestRules.Prüfe(gemark, nachschlag).HasErrors,
                    "Der Nachschlag sprengt die Kapazität, wurde aber durchgelassen");
            }
            finally {
                // Die Ruestungstabelle laesst sich nicht einzeln zuruecksetzen; einmal frisch
                // laden stellt den Ausgangszustand her.
                TestSetup.LoadZugdaten(false, false);
            }
        }
    }
}
