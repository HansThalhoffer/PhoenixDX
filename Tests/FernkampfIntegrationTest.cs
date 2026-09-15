using PhoenixModel.Commands;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Fernkampf gegen die echten Zugdaten.
    ///
    /// Die Anwendung wertet den Beschuss nicht aus - sie nimmt den Befehl auf und legt ihn in
    /// Befehl_ang ab. Geprüft wird deshalb, ob die Regeln greifen und ob der Befehl sauber
    /// geschrieben und wieder zurückgenommen wird.
    /// </summary>
    public class FernkampfIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
            // der Fernkampf gehört zum Spielzug, nicht zur Rüstphase
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>
        /// Eine eigene Einheit, die Fernkampfwaffen der gesuchten Bauart führt
        /// </summary>
        private static TruppenSpielfigur? FindeSchuetze(Fernkampfwaffe waffe) {
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(truppe => FernkampfRules.GetVorhanden(truppe, waffe) > 0);
        }

        private static KleinfeldPosition Standort(TruppenSpielfigur figur)
            => new(figur.gf_nach > 0 ? figur.gf_nach : figur.gf_von,
                   figur.gf_nach > 0 ? figur.kf_nach : figur.kf_von);

        [StaFact]
        public void EntfernungZumNachbarfeldIstEins() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");

            var standort = Standort(figur!);
            Assert.Equal(0, FernkampfRules.GetEntfernung(standort, standort, 2));

            var nachbarn = KartenKoordinaten.GetKleinfeldNachbarn(standort);
            Assert.NotNull(nachbarn);
            foreach (var nachbar in nachbarn!)
                Assert.Equal(1, FernkampfRules.GetEntfernung(standort, nachbar, 2));
        }

        /// <summary>
        /// Ein Nachbar eines Nachbarn, der nicht selbst Nachbar ist, liegt zwei Gemarken entfernt -
        /// in Reichweite der schweren, aber nicht der leichten Fernkampfwaffen.
        /// </summary>
        [StaFact]
        public void ZweiteReiheIstNurFuerSchwereWaffenErreichbar() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");

            var standort = Standort(figur!);
            var nachbarn = KartenKoordinaten.GetKleinfeldNachbarn(standort)!.ToList();
            var zweiteReihe = nachbarn
                .SelectMany(n => KartenKoordinaten.GetKleinfeldNachbarn(n) ?? [])
                .FirstOrDefault(f => f.Equals(standort) == false && nachbarn.Any(n => n.Equals(f)) == false);
            Assert.True(zweiteReihe != null, "Es liess sich kein Feld in zweiter Reihe finden");

            Assert.Equal(2, FernkampfRules.GetEntfernung(standort, zweiteReihe, FernkampfRules.ReichweiteSchwer));
            Assert.Equal(-1, FernkampfRules.GetEntfernung(standort, zweiteReihe, FernkampfRules.ReichweiteLeicht));
        }

        [StaFact]
        public void AufDasEigeneFeldWirdNichtGeschossen() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");
            var waffe = FernkampfRules.GetVorhanden(figur, Fernkampfwaffe.Leicht) > 0
                ? Fernkampfwaffe.Leicht : Fernkampfwaffe.Schwer;

            var ergebnis = FernkampfRules.PrüfeBeschuss(figur, waffe, 1, Standort(figur!), out _);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("eigene Feld", ergebnis.Title);
        }

        [StaFact]
        public void MehrWaffenAlsVorhandenWerdenAbgelehnt() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");
            var waffe = FernkampfRules.GetVorhanden(figur, Fernkampfwaffe.Leicht) > 0
                ? Fernkampfwaffe.Leicht : Fernkampfwaffe.Schwer;

            int vorhanden = FernkampfRules.GetVorhanden(figur, waffe);
            var ziel = KartenKoordinaten.GetKleinfeldNachbarn(Standort(figur!))!.First();

            var ergebnis = FernkampfRules.PrüfeBeschuss(figur, waffe, vorhanden + 1, ziel, out _);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("hat", ergebnis.Title);
        }

        /// <summary>
        /// Der eigentliche Durchlauf: Befehl geben, nachsehen, was in Befehl_ang steht, und
        /// zurücknehmen. Nach dem Zurücknehmen muss exakt der Ausgangszustand dastehen - im alten
        /// PZE war genau das die Stelle, an der sich Dinge verdoppelt haben.
        /// </summary>
        [StaFact]
        public void BeschussWirdGeschriebenUndSauberZurueckgenommen() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");
            var waffe = FernkampfRules.GetVorhanden(figur, Fernkampfwaffe.Leicht) > 0
                ? Fernkampfwaffe.Leicht : Fernkampfwaffe.Schwer;

            string? vorher = figur!.Befehl_ang;
            int verplantVorher = FernkampfRules.GetVerplant(figur, waffe);
            var ziel = KartenKoordinaten.GetKleinfeldNachbarn(Standort(figur))!.First();

            var befehl = new SchootCommand($"Beschieße {ziel.CreateBezeichner()} mit 1 {waffe}") {
                Waffe = waffe,
                Anzahl = 1,
                TargetLocation = ziel,
                Figur = figur,
            };

            var ausgeführt = befehl.ExecuteCommand();
            Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title} {ausgeführt.Message}");

            var geschrieben = Beschussbefehl.LiesAlle(figur.Befehl_ang);
            Assert.Contains(geschrieben, b => b.Waffe == waffe && b.Anzahl == 1 && b.Ziel.Equals(ziel));
            Assert.Equal(verplantVorher + 1, FernkampfRules.GetVerplant(figur, waffe));

            var zurück = befehl.UndoCommand();
            Assert.False(zurück.HasErrors, $"{zurück.Title} {zurück.Message}");
            Assert.Equal(vorher, figur.Befehl_ang);
            Assert.Equal(verplantVorher, FernkampfRules.GetVerplant(figur, waffe));
        }

        /// <summary>
        /// "Katapulte und Kriegsschiffe können generell nur einmal pro Monat schießen"
        /// (Regelwerk 1.2): über alle Befehle zusammen darf die Einheit nicht mehr Geschütze
        /// ansetzen, als sie hat.
        /// </summary>
        [StaFact]
        public void JedeWaffeSchiesstNurEinmalImMonat() {
            LadeAlles();
            var figur = FindeSchuetze(Fernkampfwaffe.Leicht) ?? FindeSchuetze(Fernkampfwaffe.Schwer);
            Assert.True(figur != null, "In den Zugdaten steht keine Einheit mit Fernkampfwaffen");
            var waffe = FernkampfRules.GetVorhanden(figur, Fernkampfwaffe.Leicht) > 0
                ? Fernkampfwaffe.Leicht : Fernkampfwaffe.Schwer;

            string? vorher = figur!.Befehl_ang;
            try {
                int alle = FernkampfRules.GetVerfügbar(figur, waffe);
                Assert.True(alle > 0, "Die Einheit hat keine freien Fernkampfwaffen");
                var ziel = KartenKoordinaten.GetKleinfeldNachbarn(Standort(figur))!.First();

                // erst alle verfügbaren Waffen auf ein Ziel ansetzen
                var ersterBefehl = new SchootCommand("alle auf ein Ziel") {
                    Waffe = waffe, Anzahl = alle, TargetLocation = ziel, Figur = figur,
                };
                Assert.False(ersterBefehl.ExecuteCommand().HasErrors);
                Assert.Equal(0, FernkampfRules.GetVerfügbar(figur, waffe));

                // dann ist keine mehr frei
                var nachschlag = FernkampfRules.PrüfeBeschuss(figur, waffe, 1, ziel, out _);
                Assert.True(nachschlag.HasErrors);
                Assert.Contains("nur einmal im Monat", nachschlag.Message);
            }
            finally {
                figur.Befehl_ang = vorher;
            }
        }

        [StaFact]
        public void ParserLiestDenBeschussbefehl() {
            var parser = new SchootCommandParser();
            Assert.True(parser.ParseCommand("Beschieße 603/78 mit 4 leichten Fernkampfwaffen von 603/77", out var befehl));
            var schuss = Assert.IsType<SchootCommand>(befehl);
            Assert.Equal(Fernkampfwaffe.Leicht, schuss.Waffe);
            Assert.Equal(4, schuss.Anzahl);
            Assert.Equal(new KleinfeldPosition(603, 78), schuss.TargetLocation);
            Assert.Equal(new KleinfeldPosition(603, 77), schuss.SourceLocation);

            Assert.True(parser.ParseCommand("Beschieße 1002/87 mit 3 schweren Katapulten", out var ohneHerkunft));
            var zweiter = Assert.IsType<SchootCommand>(ohneHerkunft);
            Assert.Equal(Fernkampfwaffe.Schwer, zweiter.Waffe);
            Assert.Equal(3, zweiter.Anzahl);
            Assert.Null(zweiter.SourceLocation);

            Assert.False(parser.ParseCommand("Beschieße 603/78 mit vielen Katapulten", out _));
        }
    }
}
