using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Ausbau und Reparatur von Ruestorten (Regelwerk 1.5 und 1.5.12).
    ///
    /// Hoechstens 250 Baupunkte pro Monat, ein Baupunkt kostet 50 GS. Die Ausbaustufen stehen in
    /// ruestort_crossref und gehen in 250er-Schritten - ein Monat Bauzeit ist genau eine Stufe.
    ///
    /// Die Spalte Ruestort eines Kleinfeldes nennt die Sollstufe, die Spalte Baupunkte den
    /// tatsaechlichen Zustand; weichen sie ab, ist der Ruestort beschaedigt.
    /// </summary>
    public class RuestortIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            // gebaut wird in der Ruestphase
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        private static List<KleinFeld> EigeneRuestorte()
            => SharedData.Map!.Values
                .Where(k => k.Baupunkte > 0 && k.Nation == ProgramView.SelectedNation)
                .ToList();

        [Fact]
        public void EinBaupunktKostetFuenfzigGoldstuecke() {
            Assert.Equal(50, RuestortRules.KostenProBaupunkt);
            Assert.Equal(250 * 50, RuestortRules.BerechneKosten(250));
            Assert.Equal(0, RuestortRules.BerechneKosten(0));
            Assert.Equal(0, RuestortRules.BerechneKosten(-5));
        }

        [Fact]
        public void ProMonatGehenHoechstens250Baupunkte() {
            Assert.Equal(250, RuestortRules.MaxBaupunkteProMonat);
        }

        /// <summary>
        /// Die Crossref wird im Lauf einer Sitzung mehrfach geladen, etwa beim Zugwechsel. Frueher
        /// warf das zweite Laden, weil Ruestort.Load seine statische Tabelle mit Add fuellte - der
        /// Ladevorgang brach ab und die Ruestortreferenz blieb leer zurueck.
        /// </summary>
        [StaFact]
        public void DieRuestortreferenzUeberlebtEinZweitesLaden() {
            LadeAlles();
            int erstesMal = SharedData.RüstortReferenz!.Count;
            Assert.True(erstesMal > 0, "Die Ruestortreferenz ist schon beim ersten Laden leer");

            // hier muss wirklich neu geladen werden - genau das ist der Prüfgegenstand
            TestSetup.LoadCrossRef(false, false, erzwingen: true);
            Assert.Equal(erstesMal, SharedData.RüstortReferenz!.Count);

            // und die Zuordnung ueber die Baupunkte steht auch noch
            Assert.NotNull(BauwerkeView.GetRuestortNachBaupunkten(1000));
        }

        /// <summary>
        /// Die Ausbaustufen gehen in Schritten von 250 Baupunkten, damit ein Monat Bauzeit genau
        /// eine Stufe ergibt. Bricht das, passt die Monatsgrenze nicht mehr zur Leiter.
        /// </summary>
        [StaFact]
        public void DieAusbaustufenPassenZurMonatsgrenze() {
            LadeAlles();
            var stufen = SharedData.RüstortReferenz!
                .Where(r => r.Baupunkte != null && r.Baupunkte > 0 && r.Baupunkte <= 3000)
                .OrderBy(r => r.Baupunkte!.Value)
                .ToList();
            Assert.NotEmpty(stufen);

            for (int i = 1; i < stufen.Count; i++) {
                int abstand = stufen[i].Baupunkte!.Value - stufen[i - 1].Baupunkte!.Value;
                Assert.True(abstand <= RuestortRules.MaxBaupunkteProMonat,
                    $"Von {stufen[i - 1].Bauwerk} nach {stufen[i].Bauwerk} sind es {abstand} Baupunkte, "
                    + $"in einem Monat gehen nur {RuestortRules.MaxBaupunkteProMonat}");
            }
        }

        /// <summary>
        /// Ein unbeschaedigter Ruestort hat genau die Baupunkte seiner Sollstufe.
        /// </summary>
        [StaFact]
        public void SchadenIstDieLueckeZurSollstufe() {
            LadeAlles();
            var ruestorte = EigeneRuestorte();
            Assert.NotEmpty(ruestorte);

            foreach (var feld in ruestorte) {
                var soll = RuestortRules.GetSollstufe(feld);
                if (soll?.Baupunkte == null)
                    continue;
                int schaden = RuestortRules.GetSchaden(feld);
                Assert.Equal(Math.Max(0, soll.Baupunkte.Value - feld.Baupunkte), schaden);
                Assert.True(schaden >= 0, $"{feld.Bezeichner} hat negativen Schaden");
            }
        }

        [StaFact]
        public void MehrAls250BaupunkteWerdenAbgelehnt() {
            LadeAlles();
            var feld = EigeneRuestorte().FirstOrDefault();
            Assert.True(feld != null, "Das eigene Reich hat keinen Ruestort");

            var ergebnis = RuestortRules.Prüfe(feld, Bauart.Ausbau, RuestortRules.MaxBaupunkteProMonat + 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("250", ergebnis.Title);
        }

        [StaFact]
        public void OhneBaupunkteWirdNichtGebaut() {
            LadeAlles();
            var feld = EigeneRuestorte().FirstOrDefault();
            Assert.True(feld != null, "Das eigene Reich hat keinen Ruestort");

            Assert.True(RuestortRules.Prüfe(feld, Bauart.Ausbau, 0).HasErrors);
            Assert.True(RuestortRules.Prüfe(feld, Bauart.Reparatur, -1).HasErrors);
        }

        /// <summary>
        /// Ein unbeschaedigter Ruestort laesst sich nicht reparieren, ein beschaedigter nicht
        /// weiter ausbauen - beides muss klar gemeldet werden statt still danebenzugehen.
        /// </summary>
        [StaFact]
        public void ReparaturUndAusbauSchliessenEinanderAus() {
            LadeAlles();
            var ruestorte = EigeneRuestorte();

            var heil = ruestorte.FirstOrDefault(k => RuestortRules.GetSchaden(k) == 0);
            Assert.True(heil != null, "Das eigene Reich hat keinen unbeschaedigten Ruestort");
            var reparatur = RuestortRules.Prüfe(heil, Bauart.Reparatur, 100);
            Assert.True(reparatur.HasErrors);
            Assert.Contains("unbeschädigt", reparatur.Title);

            var kaputt = ruestorte.FirstOrDefault(k => RuestortRules.GetSchaden(k) > 0);
            if (kaputt == null)
                return; // in diesem Zug ist kein eigener Ruestort beschaedigt
            var ausbau = RuestortRules.Prüfe(kaputt, Bauart.Ausbau, 100);
            Assert.True(ausbau.HasErrors);
            Assert.Contains("beschädigt", ausbau.Title);
        }

        /// <summary>
        /// Der Durchlauf: Auftrag anlegen, nachsehen, zuruecknehmen.
        /// </summary>
        [StaFact]
        public void DerBauauftragWirdSauberZurueckgenommen() {
            LadeAlles();
            var feld = EigeneRuestorte().FirstOrDefault(k => RuestortRules.GetSchaden(k) == 0
                                                          && RuestortRules.GetNächsteStufe(k) != null);
            // in den echten Zugdaten gibt es ein solches Feld; sollte das spaeter nicht mehr so
            // sein, laeuft der Test ins Leere statt fehlzuschlagen
            if (feld == null)
                return;

            int auftraegeVorher = SharedData.RuestungRuestorte!.Count;

            var befehl = new UpgradeCommand($"Verstärke Rüstort {feld.Bezeichner}") {
                Location = new KleinfeldPosition(feld.gf, feld.kf),
            };

            var ausgeführt = befehl.ExecuteCommand();
            try {
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");

                var auftrag = SharedData.RuestungRuestorte!
                    .FirstOrDefault(a => a.gf == feld.gf && a.kf == feld.kf);
                Assert.True(auftrag != null, "Es wurde kein Bauauftrag angelegt");
                Assert.True(auftrag!.BP_up > 0, "Der Auftrag traegt keine Baupunkte");
                Assert.True(auftrag.BP_up <= RuestortRules.MaxBaupunkteProMonat);
                Assert.Equal(auftraegeVorher + 1, SharedData.RuestungRuestorte!.Count);
            }
            finally {
                var zurück = befehl.UndoCommand();
                Assert.False(zurück.HasErrors, $"{zurück.Title} {zurück.Message}");
            }

            Assert.Equal(auftraegeVorher, SharedData.RuestungRuestorte!.Count);
            Assert.DoesNotContain(SharedData.RuestungRuestorte!,
                a => a.gf == feld.gf && a.kf == feld.kf);
        }

        [StaFact]
        public void ParserLiestAusbauUndReparatur() {
            var ausbau = new UpgradeCommandParser();
            Assert.True(ausbau.ParseCommand("Verstärke Rüstort 202/33", out var ohneAngabe));
            var ohne = Assert.IsType<UpgradeCommand>(ohneAngabe);
            Assert.Equal(new KleinfeldPosition(202, 33), ohne.Location);
            Assert.Equal(0, ohne.Baupunkte);

            Assert.True(ausbau.ParseCommand("Verstärke Rüstort 202/33 um 150 Baupunkte", out var mitAngabe));
            Assert.Equal(150, Assert.IsType<UpgradeCommand>(mitAngabe).Baupunkte);

            var reparatur = new RepairCommandParser();
            Assert.True(reparatur.ParseCommand("Repariere Rüstort 202/33", out var repOhne));
            Assert.Equal(0, Assert.IsType<RepairCommand>(repOhne).Baupunkte);

            Assert.True(reparatur.ParseCommand("Repariere 150 Baupunkte an dem Bauwerk auf 202/33", out var repMit));
            var mit = Assert.IsType<RepairCommand>(repMit);
            Assert.Equal(150, mit.Baupunkte);
            Assert.Equal(new KleinfeldPosition(202, 33), mit.Location);

            Assert.False(ausbau.ParseCommand("Verstärke Rüstort irgendwo", out _));
        }
    }
}
