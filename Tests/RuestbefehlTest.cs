using PhoenixModel.Commands;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Ruestbefehl - Issue #65, der Weg ueber die Befehlseingabe.
    ///
    /// Der Parser und der Auftrag waren beide Ruempfe: der Parser las zwei Regex-Gruppen aus, die
    /// es in keinem der beiden Ausdruecke gibt, und machte damit aus jedem Ruestgut (None, 0);
    /// CreateRuestung erzeugte einen Auftrag aus lauter Nullen. Der Befehl liess sich lesen,
    /// ausfuehren und speichern - und ruestete nichts.
    /// </summary>
    public class RuestbefehlTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        private static EquipCommand Lies(string befehl) {
            var parser = new EquipCommandParser();
            Assert.True(parser.ParseCommand(befehl, out IPhoenixCommand? command), $"'{befehl}' liess sich nicht lesen");
            var equip = Assert.IsType<EquipCommand>(command);
            return equip;
        }

        /// <summary>
        /// Ein Befehl mit mehreren Ruestguetern wird vollstaendig gelesen - mit Anzahl und Art.
        /// </summary>
        [StaFact]
        public void DerBefehlWirdMitAnzahlUndArtGelesen() {
            LadeAlles();
            var equip = Lies("Rüste 1000 Krieger und 3 Heerführer und 2 Leichte Katapulte in Rüstort 506/17");

            Assert.Equal(3, equip.Equipment.Count);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.K && e.Count == 1000);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.HF && e.Count == 3);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.LKP && e.Count == 2);
            Assert.Equal(506, equip.Location!.gf);
            Assert.Equal(17, equip.Location.kf);

            // kein Ruestgut bleibt unerkannt und keine Anzahl null - genau das war der Fehler
            Assert.DoesNotContain(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.None);
            Assert.DoesNotContain(equip.Equipment, e => e.Count == 0);
        }

        /// <summary>
        /// Auch die gebeugten Formen aus den Beispielen des Parsers werden erkannt.
        /// </summary>
        [StaFact]
        public void GebeugteFormenWerdenErkannt() {
            LadeAlles();
            var equip = Lies("Rüste 500 Krieger und 10 Pferden und 3 Heerführern und 2 Schweren Katapulten in Rüstort 123/45");

            Assert.Equal(4, equip.Equipment.Count);
            Assert.DoesNotContain(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.None);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.P && e.Count == 10);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.SKP && e.Count == 2);
        }

        /// <summary>
        /// Wird zu einer vorhandenen Einheit gerüstet, traegt der Auftrag deren Nummer.
        /// </summary>
        [StaFact]
        public void ZuEinerVorhandenenEinheitGeruestetTraegtDerAuftragDerenNummer() {
            LadeAlles();
            var equip = Lies("Rüste 6 Leichte Katapulte in Rüstort 506/17 zu Krieger 115");

            Assert.Equal(ConstructionElementType.K, equip.Target);
            Assert.Equal(115, equip.TargetID);
            Assert.Contains(equip.Equipment, e => e.ConstructionElementType == ConstructionElementType.LKP && e.Count == 6);
        }

        /// <summary>
        /// Der Auftrag landet mit Stueckzahlen und Position in der Ruestungstabelle - und wird
        /// nicht als Reihe von Nullen gespeichert.
        /// </summary>
        [StaFact]
        public void DerAuftragTraegtDieStueckzahlenUndDiePosition() {
            LadeAlles();
            var rüstort = SharedData.Map!.Values.First(kf =>
                kf.Nation == ProgramView.SelectedNation && RuestRules.GetKapazität(kf).Goldstücke > 0);

            var equip = Lies($"Rüste 100 Krieger und 1 Heerführer in Rüstort {rüstort.gf}/{rüstort.kf}");
            Assert.False(equip.CheckPreconditions().HasErrors, "Ein kleiner Auftrag im eigenen Rüstort wurde abgelehnt");

            int vorher = SharedData.Ruestung!.Count;
            try {
                var ergebnis = equip.ExecuteCommand();
                Assert.False(ergebnis.HasErrors, $"{ergebnis.Title}: {ergebnis.Message}");

                var auftrag = SharedData.Ruestung!.FirstOrDefault(r => r.gf == rüstort.gf && r.kf == rüstort.kf && r.K == 100);
                Assert.True(auftrag != null, "Der Auftrag steht nicht in der Rüstungstabelle");
                Assert.Equal(100, auftrag!.K);
                Assert.Equal(1, auftrag.HF);
                Assert.Equal(rüstort.gf, auftrag.gf);
                Assert.Equal(rüstort.kf, auftrag.kf);

                // und er laesst sich wieder zuruecknehmen
                var zurück = equip.UndoCommand();
                Assert.False(zurück.HasErrors, $"{zurück.Title}: {zurück.Message}");
                Assert.Equal(vorher, SharedData.Ruestung!.Count);
            }
            finally {
                SharedData.StoreQueue.Clear();
                TestSetup.LoadZugdaten(false, false);
            }
        }

        /// <summary>
        /// Der Befehl prueft die Regeln des Ruestens - ein fremder Ruestort geht nicht.
        /// </summary>
        [StaFact]
        public void DerBefehlPrueftDieRegeln() {
            LadeAlles();
            var fremd = SharedData.Map!.Values.First(kf =>
                kf.Nation != null && kf.Nation != ProgramView.SelectedNation
                && RuestRules.GetKapazität(kf).Goldstücke > 0);

            var equip = Lies($"Rüste 100 Krieger in Rüstort {fremd.gf}/{fremd.kf}");
            var ergebnis = equip.CheckPreconditions();

            Assert.True(ergebnis.HasErrors, "In einem fremden Rüstort wurde das Rüsten erlaubt");
            Assert.Contains("eigenen Reich", ergebnis.Title);
        }

        /// <summary>
        /// Ein unbekanntes Ruestgut wird abgelehnt, statt still als Nichts durchzugehen.
        /// </summary>
        [StaFact]
        public void EinUnbekanntesRuestgutWirdAbgelehnt() {
            LadeAlles();
            var rüstort = SharedData.Map!.Values.First(kf =>
                kf.Nation == ProgramView.SelectedNation && RuestRules.GetKapazität(kf).Goldstücke > 0);

            var equip = Lies($"Rüste 5 Drachen in Rüstort {rüstort.gf}/{rüstort.kf}");
            var ergebnis = equip.CheckPreconditions();

            Assert.True(ergebnis.HasErrors, "Ein unbekanntes Rüstgut wurde durchgelassen");
            Assert.Contains("nicht erkannt", ergebnis.Title);
        }
    }
}
