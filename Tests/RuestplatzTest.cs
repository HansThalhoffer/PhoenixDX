using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.RuestplatzView;

namespace Tests {

    /// <summary>
    /// Der Unterbau der Ruestoberflaeche.
    ///
    /// Die Oberflaeche selbst ist WPF und laesst sich hier nicht pruefen - wohl aber alles, was
    /// sie anzeigt und ausrechnet: welche Orte zur Auswahl stehen, was dort frei ist, was ein Korb
    /// kostet, und vor allem, dass aus dem Korb ein Befehl wird, den der Parser wieder genauso
    /// liest.
    ///
    /// Der letzte Punkt ist der wichtige. Die Oberflaeche schreibt nicht selbst in die
    /// Ruestungstabelle, sondern erzeugt denselben Befehl wie die Befehlseingabe - so landet die
    /// Ruestung unter "Aktueller Zug" und laesst sich zuruecknehmen. Geht dabei etwas verloren,
    /// ruestet die Oberflaeche still etwas anderes als angezeigt.
    /// </summary>
    public class RuestplatzTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>Ein eigener Ruestort, in dem sich tatsaechlich ruesten laesst</summary>
        private static PhoenixModel.dbErkenfara.KleinFeld FindeRüstort() {
            var orte = FindeRüstorte();
            Assert.True(orte.Count > 0, "Das eigene Reich hat keinen Ruestort");
            return orte[0];
        }

        /// <summary>
        /// Zur Auswahl stehen nur eigene Gemarken mit einem Ruestort - ein Dorf ist keiner.
        /// </summary>
        [StaFact]
        public void ZurAuswahlStehenNurEigeneRuestorte() {
            LadeAlles();
            var orte = FindeRüstorte();
            Assert.NotEmpty(orte);

            Assert.All(orte, ort => Assert.Equal(ProgramView.SelectedNation, ort.Nation));
            // und jeder hat wirklich Kapazitaet - sonst waere es ein Dorf oder gar nichts
            Assert.All(orte, ort => Assert.True(RuestRules.GetKapazität(ort).Goldstücke > 0,
                $"{ort.Bezeichner} steht in der Auswahl, hat aber keine Ruestkapazitaet"));

            // nach Bezeichnung sortiert, damit die Liste wiederfindbar ist
            var sortiert = orte.Select(ort => ort.Bezeichner).ToList();
            Assert.Equal(sortiert.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), sortiert);
        }

        /// <summary>
        /// Die Uebersicht rechnet frei = Kapazitaet minus vergeben, und nie unter null.
        /// </summary>
        [StaFact]
        public void DieUebersichtZeigtWasNochFreiIst() {
            LadeAlles();
            var ort = FindeRüstort();
            var übersicht = GetÜbersicht(ort);

            Assert.True(übersicht.IstRüstort);
            Assert.Equal(ort, übersicht.Gemark);
            Assert.False(string.IsNullOrWhiteSpace(übersicht.Rüstortname));

            Assert.Equal(übersicht.Kapazität.Goldstücke - übersicht.Vergeben.Goldstücke,
                übersicht.Frei.Goldstücke);
            Assert.True(übersicht.Frei.Goldstücke >= 0);
            Assert.True(übersicht.Frei.Heerführer >= 0);
            Assert.True(übersicht.Frei.Zauberer >= 0);

            Assert.Equal(übersicht.ReichsschatzAlt - übersicht.VerrüstetGesamt, übersicht.ReichsschatzNeu);

            // ohne Gemark gibt es nichts anzuzeigen, und es wirft auch nichts
            var keine = GetÜbersicht(null);
            Assert.False(keine.IstRüstort);
            Assert.Equal(0, keine.Frei.Goldstücke);
        }

        /// <summary>
        /// Aus besonderen Einnahmen halbiert sich die Kapazitaet (Regelwerk 3.3.1).
        /// </summary>
        [StaFact]
        public void AusBesonderenEinnahmenIstDieKapazitaetHalbiert() {
            LadeAlles();
            var ort = FindeRüstort();

            var voll = GetÜbersicht(ort, ausBesonderenEinnahmen: false);
            var halb = GetÜbersicht(ort, ausBesonderenEinnahmen: true);

            Assert.Equal(voll.Kapazität.Goldstücke / 2, halb.Kapazität.Goldstücke);
            Assert.Equal(voll.Kapazität.Heerführer / 2, halb.Kapazität.Heerführer);
            Assert.Equal(voll.Kapazität.Zauberer / 2, halb.Kapazität.Zauberer);
        }

        /// <summary>
        /// Der Korb kostet, was die Kostentabelle sagt - und bindet Heerfuehrer und Zauberer.
        /// </summary>
        [StaFact]
        public void DerKorbKostetWasDieKostentabelleSagt() {
            LadeAlles();

            int preisKrieger = KostenView.GetGSKosten(ConstructionElementType.K);
            int preisHF = KostenView.GetGSKosten(ConstructionElementType.HF);
            Assert.True(preisKrieger > 0, "Die Kostentabelle kennt keinen Preis fuer Krieger");

            var korb = new List<Posten> {
                new(ConstructionElementType.K, 100),
                new(ConstructionElementType.HF, 2),
                new(ConstructionElementType.ZA, 1),
            };
            var umfang = BerechneKorb(korb);

            Assert.Equal(100 * preisKrieger + 2 * preisHF
                + KostenView.GetGSKosten(ConstructionElementType.ZA), umfang.Goldstücke);
            Assert.Equal(2, umfang.Heerführer);
            Assert.Equal(1, umfang.Zauberer);

            // ein leerer Korb kostet nichts und wirft nicht
            Assert.Equal(0, BerechneKorb(null).Goldstücke);
            Assert.Equal(0, BerechneKorb([]).Goldstücke);
            Assert.Equal(0, BerechneKorb([new Posten(ConstructionElementType.K, 0)]).Goldstücke);
        }

        /// <summary>
        /// Aus dem Korb wird ein Befehl, und der Parser liest genau denselben Korb wieder heraus.
        ///
        /// Das ist die Naht zwischen Oberflaeche und Befehlsweg. Reisst sie, ruestet die Anwendung
        /// etwas anderes als angezeigt - und niemand saehe es.
        /// </summary>
        [StaFact]
        public void AusDemKorbWirdEinBefehlUndZurueck() {
            LadeAlles();
            var ort = FindeRüstort();

            var korb = new List<Posten> {
                new(ConstructionElementType.K, 500),
                new(ConstructionElementType.P, 10),
                new(ConstructionElementType.HF, 3),
                new(ConstructionElementType.SKP, 2),
            };

            string befehl = BaueBefehl(korb, ort);
            Assert.False(string.IsNullOrWhiteSpace(befehl));
            Assert.StartsWith("Rüste ", befehl);
            Assert.Contains($"in Rüstort {ort.CreateBezeichner()}", befehl);

            Assert.True(CommandParser.ParseCommand(befehl, out var gelesen),
                $"Der eigene Befehl liess sich nicht wieder lesen: {befehl}");
            var rüstbefehl = Assert.IsType<EquipCommand>(gelesen);

            // dieselben Guetter in denselben Mengen
            Assert.Equal(korb.Count, rüstbefehl.Equipment.Count);
            foreach (var posten in korb) {
                var teil = rüstbefehl.Equipment.FirstOrDefault(e => e.ConstructionElementType == posten.Art);
                Assert.True(teil.ConstructionElementType == posten.Art,
                    $"{posten.Art} fehlt im gelesenen Befehl: {befehl}");
                Assert.Equal(posten.Anzahl, teil.Count);
            }
            Assert.Equal(ort.gf, rüstbefehl.Location?.gf);
            Assert.Equal(ort.kf, rüstbefehl.Location?.kf);
        }

        /// <summary>
        /// Auch der Befehl, der zu einer bestehenden Einheit ruestet, kommt heil zurueck.
        /// </summary>
        [StaFact]
        public void AuchDasRuestenZuEinerEinheitKommtHeilZurueck() {
            LadeAlles();
            var ort = FindeRüstort();

            string befehl = BaueBefehl([new Posten(ConstructionElementType.LKP, 6)], ort,
                ConstructionElementType.K, 115);

            Assert.True(CommandParser.ParseCommand(befehl, out var gelesen), befehl);
            var rüstbefehl = Assert.IsType<EquipCommand>(gelesen);
            Assert.Equal(ConstructionElementType.K, rüstbefehl.Target);
            Assert.Equal(115, rüstbefehl.TargetID);
            Assert.Single(rüstbefehl.Equipment);
            Assert.Equal(ConstructionElementType.LKP, rüstbefehl.Equipment[0].ConstructionElementType);
            Assert.Equal(6, rüstbefehl.Equipment[0].Count);
        }

        /// <summary>
        /// Jedes angebotene Ruestgut ueberlebt den Weg durch den Befehl.
        ///
        /// Die Namen im Befehl kommen aus ConstructionElementTypeToString, gelesen werden sie von
        /// parseConstructionElement - zwei Tabellen, die zueinander passen muessen. Bei den
        /// Leichten Kriegsschiffen endet der Name auf ein Leerzeichen; das faellt nur auf, wenn
        /// man es prueft.
        /// </summary>
        [StaFact]
        public void JedesRuestgutUeberlebtDenBefehl() {
            LadeAlles();
            var ort = FindeRüstort();

            foreach (var gut in GetRüstgüter()) {
                string befehl = BaueBefehl([new Posten(gut.Art, 7)], ort);
                Assert.True(CommandParser.ParseCommand(befehl, out var gelesen),
                    $"{gut.Name}: '{befehl}' liess sich nicht lesen");
                var rüstbefehl = Assert.IsType<EquipCommand>(gelesen);
                Assert.Single(rüstbefehl.Equipment);
                Assert.Equal(gut.Art, rüstbefehl.Equipment[0].ConstructionElementType);
                Assert.Equal(7, rüstbefehl.Equipment[0].Count);
            }
        }

        /// <summary>
        /// Ein leerer Korb ergibt keinen Befehl - und keine halbe Zeile.
        /// </summary>
        [StaFact]
        public void EinLeererKorbErgibtKeinenBefehl() {
            LadeAlles();
            var ort = FindeRüstort();

            Assert.Equal(string.Empty, BaueBefehl(null, ort));
            Assert.Equal(string.Empty, BaueBefehl([], ort));
            Assert.Equal(string.Empty, BaueBefehl([new Posten(ConstructionElementType.K, 0)], ort));
            Assert.Equal(string.Empty, BaueBefehl([new Posten(ConstructionElementType.K, 5)], null));
        }

        /// <summary>
        /// Die Pruefung antwortet, bevor jemand auf "Ruesten" drueckt - und sie sagt, woran es
        /// liegt.
        /// </summary>
        [StaFact]
        public void DiePruefungAntwortetVorDemRuesten() {
            LadeAlles();
            var ort = FindeRüstort();

            // nichts im Korb
            var leer = Prüfe(ort, []);
            Assert.True(leer.HasErrors);
            Assert.Contains("nichts zum Rüsten", leer.Title);

            // mehr, als der Ort hergibt
            var übersicht = GetÜbersicht(ort);
            int zuviel = übersicht.Kapazität.Goldstücke / KostenView.GetGSKosten(ConstructionElementType.K) + 1000;
            var gesprengt = Prüfe(ort, [new Posten(ConstructionElementType.K, zuviel)]);
            Assert.True(gesprengt.HasErrors);

            // eine fremde Gemark ist kein eigener Ruestort
            var fremd = SharedData.Map!.Values.FirstOrDefault(gemark =>
                gemark.Nation != null && gemark.Nation != ProgramView.SelectedNation);
            Assert.True(fremd != null, "Es gibt keine fremde Gemark");
            var fremdeGemark = Prüfe(fremd, [new Posten(ConstructionElementType.K, 1)]);
            Assert.True(fremdeGemark.HasErrors);
            Assert.Contains("eigene", fremdeGemark.Message);
        }

        /// <summary>
        /// In der Bewegungsphase wird nicht geruestet - die Pruefung sagt es, statt es geschehen
        /// zu lassen.
        /// </summary>
        [StaFact]
        public void InDerBewegungsphaseWirdNichtGeruestet() {
            LadeAlles();
            var ort = FindeRüstort();
            int phaseVorher = ZugView.Settings!.Phase;
            int monatVorher = ZugView.Settings!.Monat;
            try {
                TestSetup.SetzePhase(Zugphase.Bewegungsphase);
                var ergebnis = Prüfe(ort, [new Posten(ConstructionElementType.K, 1)]);
                Assert.True(ergebnis.HasErrors);
                Assert.Contains("wird nicht gerüstet", ergebnis.Title);
            }
            finally {
                ZugView.Settings!.Phase = phaseVorher;
                ZugView.Settings!.Monat = monatVorher;
            }
        }

        /// <summary>
        /// Was schon eingestellt ist, laesst sich lesen - und steht in einer Zeile, die ein Mensch
        /// versteht.
        /// </summary>
        [StaFact]
        public void WasEingestelltIstLaesstSichLesen() {
            LadeAlles();
            var ort = FindeRüstort();

            // ohne Ort nichts, und es wirft nicht
            Assert.Empty(GetEingestellte(null));

            var auftrag = BaueAuftrag([
                new Posten(ConstructionElementType.K, 300),
                new Posten(ConstructionElementType.HF, 2),
            ], ort);
            string zeile = Beschreibe(auftrag);
            Assert.Contains("300 Krieger", zeile);
            Assert.Contains("2 Heerführer", zeile);

            Assert.Equal(string.Empty, Beschreibe(null));
            Assert.Equal("(leer)", Beschreibe(BaueAuftrag([], ort)));
        }
    }
}
