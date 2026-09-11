using PhoenixModel.Commands;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Teilen und Fusionieren von Heeren gegen die echten Zugdaten (Regelwerk 1.8).
    ///
    /// Der Schwerpunkt liegt auf dem Zuruecknehmen: im alten PZE war das die Stelle, an der sich
    /// Truppen verdoppelt haben. Geschrieben wird nichts - die StoreQueue wird im Testlauf nicht
    /// abgearbeitet.
    /// </summary>
    public class HeeresIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Ein Heer, das sich teilen laesst: genug Heerfuehrer und genug Masse fuer beide Haelften
        /// </summary>
        private static TruppenSpielfigur? FindeTeilbaresHeer() {
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(truppe => truppe.hf >= 2 && truppe.staerke >= 400)
                .OrderByDescending(truppe => truppe.staerke)
                .FirstOrDefault();
        }

        private static int ZaehleHeere(TruppenSpielfigur vorbild) {
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Count(truppe => truppe.BaseTyp == vorbild.BaseTyp);
        }

        [StaFact]
        public void EineFreieNummerLiegtImNummernkreisDerGattung() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            int? nummer = HeeresRules.FindeFreieNummer(heer!);
            Assert.NotNull(nummer);

            int start = HeeresRules.GetStartNummer(heer!.BaseTyp);
            Assert.InRange(nummer!.Value, start + 1, start + HeeresRules.HeereProGattung);

            // und sie ist wirklich frei
            Assert.DoesNotContain(SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>(),
                truppe => truppe.BaseTyp == heer.BaseTyp && truppe.Nummer == nummer.Value);
        }

        /// <summary>
        /// "Jedes Heer muss mindestens 100 Raumpunkte plus einen Heerfuehrer oder Adeligen haben"
        /// </summary>
        [StaFact]
        public void BeideHaelftenBrauchenEinenHeerfuehrer() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            var ohneHeerführer = HeeresRules.PrüfeTeilung(heer, new Heeresanteil { Stärke = 100, Heerführer = 0 });
            Assert.True(ohneHeerführer.HasErrors);
            Assert.Contains("Heerführer", ohneHeerführer.Title);

            var alleHeerführer = HeeresRules.PrüfeTeilung(heer, new Heeresanteil { Stärke = 100, Heerführer = heer!.hf });
            Assert.True(alleHeerführer.HasErrors);
            Assert.Contains("ohne Heerführer", alleHeerführer.Title);
        }

        [StaFact]
        public void MehrAlsVorhandenLaesstSichNichtAbspalten() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            var zuviel = HeeresRules.PrüfeTeilung(heer, new Heeresanteil { Stärke = heer!.staerke + 1, Heerführer = 1 });
            Assert.True(zuviel.HasErrors);
            Assert.Contains("nicht", zuviel.Title);
        }

        /// <summary>
        /// "Jedes Heer muss mindestens 100 Raumpunkte ... haben" (Regelwerk 1.8).
        ///
        /// Mit der aktuellen Kostentabelle bringt ein Heerfuehrer allein bereits 100 Raumpunkte
        /// mit, waehrend ein Krieger einen einzigen beitraegt. Jede Abspaltung mit Heerfuehrer
        /// erfuellt die Mindestgroesse damit automatisch - die Regel greift erst, wenn sich die
        /// Kosten aendern. Der Test haelt fest, worauf das beruht.
        /// </summary>
        [StaFact]
        public void DieMindestgroesseErgibtSichAusDenRaumpunkten() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            int mitHeerführer = SpielfigurRules.BerechneRaumpunkte(heer!, 1, 0, 1, 0, 0);
            Assert.True(mitHeerführer >= HeeresRules.MinRaumpunkte,
                $"Ein Heerfuehrer und ein Krieger ergeben nur {mitHeerführer} Raumpunkte");

            int ohneHeerführer = SpielfigurRules.BerechneRaumpunkte(heer!, 50, 0, 0, 0, 0);
            Assert.True(ohneHeerführer < HeeresRules.MinRaumpunkte,
                $"50 Krieger ohne Heerfuehrer ergeben schon {ohneHeerführer} Raumpunkte - die Kosten haben sich geaendert");

            // Folge daraus: ein Heer darf seine gesamte Staerke abgeben und bleibt zulaessig,
            // solange ihm ein Heerfuehrer bleibt. Das ist mit dem Regelwerk vereinbar - die
            // Mindestgroesse ist erfuellt - und wird hier festgehalten, weil es ueberrascht.
            var alleStärkeAbgeben = HeeresRules.PrüfeTeilung(heer, new Heeresanteil { Stärke = heer!.staerke, Heerführer = 1 });
            Assert.False(alleStärkeAbgeben.HasErrors,
                $"{alleStärkeAbgeben.Title}: {alleStärkeAbgeben.Message}");
        }

        /// <summary>
        /// Der Durchlauf: abspalten, nachsehen, zuruecknehmen. Danach muss der Ausgangszustand
        /// exakt wiederhergestellt sein - und vor allem darf kein Heer uebrig bleiben.
        /// </summary>
        [StaFact]
        public void TeilungWirdSauberZurueckgenommen() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            int stärkeVorher = heer!.staerke;
            int hfVorher = heer.hf;
            int rpVorher = heer.rp;
            string? spaltetabVorher = heer.spaltetab;
            int anzahlVorher = ZaehleHeere(heer);

            var anteil = new Heeresanteil { Stärke = 200, Heerführer = 1 };
            var befehl = new SplitCommand("Testteilung") { Heer = heer, Anteil = anteil };

            var ausgeführt = befehl.ExecuteCommand();
            try {
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title} {ausgeführt.Message}");
                Assert.NotNull(befehl.Abgespalten);

                var neu = befehl.Abgespalten!;
                Assert.Equal(200, neu.staerke);
                Assert.Equal(1, neu.hf);
                Assert.Equal(stärkeVorher - 200, heer.staerke);
                Assert.Equal(hfVorher - 1, heer.hf);

                // das neue Heer steht da, wo das alte steht, und hat sich noch nicht bewegt
                Assert.Equal(HeeresRules.GetStandort(heer).gf, neu.gf_von);
                Assert.Equal(HeeresRules.GetStandort(heer).kf, neu.kf_von);
                Assert.Equal(0, neu.gf_nach);

                // und es ist in den Zugdaten angekommen
                Assert.Equal(anzahlVorher + 1, ZaehleHeere(heer));
                Assert.Contains(SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>(),
                    truppe => truppe.Nummer == neu.Nummer && truppe.BaseTyp == neu.BaseTyp);
            }
            finally {
                var zurück = befehl.UndoCommand();
                Assert.False(zurück.HasErrors, $"{zurück.Title} {zurück.Message}");
            }

            Assert.Equal(stärkeVorher, heer.staerke);
            Assert.Equal(hfVorher, heer.hf);
            Assert.Equal(rpVorher, heer.rp);
            Assert.Equal(spaltetabVorher, heer.spaltetab);
            Assert.Equal(anzahlVorher, ZaehleHeere(heer));
        }

        /// <summary>
        /// Krieger, Reiter und Schiffe duerfen nicht gemischt werden (Regelwerk 1.8).
        /// </summary>
        [StaFact]
        public void VerschiedeneGattungenFusionierenNicht() {
            LadeAlles();
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>().ToList();
            var eine = armee.FirstOrDefault();
            Assert.True(eine != null, "In den Zugdaten steht keine Truppe");
            var andere = armee.FirstOrDefault(truppe => truppe.BaseTyp != eine!.BaseTyp);
            Assert.True(andere != null, "Es gibt keine zweite Gattung in den Zugdaten");

            var ergebnis = HeeresRules.PrüfeFusion(eine, andere);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("Gattungen", ergebnis.Title);
        }

        [StaFact]
        public void EinHeerFusioniertNichtMitSichSelbst() {
            LadeAlles();
            var heer = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>().FirstOrDefault();
            Assert.True(heer != null, "In den Zugdaten steht keine Truppe");

            var ergebnis = HeeresRules.PrüfeFusion(heer, heer);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("sich selbst", ergebnis.Title);
        }

        /// <summary>
        /// Teilen und danach wieder fusionieren muss den Ausgangszustand ergeben - eine gute Probe
        /// darauf, dass beide Befehle dieselbe Buchhaltung fuehren.
        /// </summary>
        [StaFact]
        public void TeilenUndWiederVereinigenErgibtDenAusgangszustand() {
            LadeAlles();
            var heer = FindeTeilbaresHeer();
            Assert.True(heer != null, "In den Zugdaten steht kein teilbares Heer");

            int stärkeVorher = heer!.staerke;
            int hfVorher = heer.hf;
            int gsVorher = heer.GS;

            var teilung = new SplitCommand("Testteilung") {
                Heer = heer,
                Anteil = new Heeresanteil { Stärke = 200, Heerführer = 1 },
            };
            Assert.False(teilung.ExecuteCommand().HasErrors);
            var neu = teilung.Abgespalten!;

            var fusion = new MergeCommand("Testfusion") { Bleibt = heer, GehtAuf = neu };
            var verschmolzen = fusion.ExecuteCommand();
            try {
                Assert.False(verschmolzen.HasErrors, $"{verschmolzen.Title} {verschmolzen.Message}");
                Assert.Equal(stärkeVorher, heer.staerke);
                Assert.Equal(hfVorher, heer.hf);
                Assert.Equal(gsVorher, heer.GS);

                // das aufgenommene Heer ist leer und verschwindet beim Zuguebergang von selbst
                Assert.Equal(0, neu.staerke);
                Assert.Equal(0, neu.hf);
                Assert.True(ZugendeRules.IstAufgelöst(neu), "Das leere Heer gilt nicht als aufgeloest");
                Assert.Contains($"#FM:{neu.Nummer}", heer.fusmit);
                Assert.Contains($"#FZ:{heer.Nummer}", neu.fusmit);
            }
            finally {
                fusion.UndoCommand();
                teilung.UndoCommand();
            }

            Assert.Equal(stärkeVorher, heer.staerke);
            Assert.Equal(hfVorher, heer.hf);
        }

        [StaFact]
        public void ParserLiestTeilenUndVereinigen() {
            var teilen = new SplitCommandParser();
            Assert.True(teilen.ParseCommand("Spalte von Reiter 220 ab 400 Reiter mit 4 Heerführern", out var geteilt));
            var teilung = Assert.IsType<SplitCommand>(geteilt);
            Assert.Equal(220, teilung.OriginalUnitId);
            Assert.Equal(400, teilung.Anteil.Stärke);
            Assert.Equal(4, teilung.Anteil.Heerführer);

            Assert.True(teilen.ParseCommand("Spalte von Krieger 101 ab 500 mit 2 Heerführern und 100 Pferden", out var mitPferden));
            var mit = Assert.IsType<SplitCommand>(mitPferden);
            Assert.Equal(100, mit.Anteil.Pferde);

            Assert.False(teilen.ParseCommand("Spalte von Reiter 220 ab viele mit 4 Heerführern", out _));

            var vereinigen = new MergeCommandParser();
            Assert.True(vereinigen.ParseCommand("Vereinige Reiter 221 mit 244", out var vereinigt));
            var fusion = Assert.IsType<MergeCommand>(vereinigt);
            Assert.Equal(221, fusion.BleibtUnitId);
            Assert.Equal(244, fusion.GehtAufUnitId);
        }
    }
}
