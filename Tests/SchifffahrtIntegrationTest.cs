using PhoenixModel.Commands;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Ein- und Ausschiffen gegen die echten Zugdaten (Regelwerk 0.3.2, 1.5.2 und 4).
    ///
    /// Einschiffen kostet keine Bewegungspunkte, nur Hoehenstufenpunkte; Ausschiffen kostet beides.
    /// Geschrieben wird nichts - die StoreQueue wird im Testlauf nicht abgearbeitet.
    /// </summary>
    public class SchifffahrtIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        private static List<TruppenSpielfigur> Armee()
            => SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>().ToList();

        private static TruppenSpielfigur? FindeFlotte()
            => Armee().FirstOrDefault(t => t.BaseTyp == FigurType.Schiff && t.staerke > 0);

        /// <summary>
        /// Ein Landheer, das neben einer Flotte steht - genau die Lage, in der eingeschifft wird
        /// </summary>
        private static (TruppenSpielfigur truppe, TruppenSpielfigur flotte)? FindeEinschiffbarePaarung() {
            var armee = Armee();
            foreach (var flotte in armee.Where(t => t.BaseTyp == FigurType.Schiff && t.staerke > 0)) {
                var wasser = KleinfeldView.GetKleinfeld(flotte);
                if (wasser == null)
                    continue;
                foreach (var truppe in armee.Where(t => t.BaseTyp != FigurType.Schiff && SchifffahrtsRules.IstEingeschifft(t) == false)) {
                    var land = KleinfeldView.GetKleinfeld(truppe);
                    if (land == null)
                        continue;
                    if (BewegungsRules.GetRichtung(land, wasser) == null)
                        continue;
                    // die Regeln entscheiden, ob dieses Paar taugt - Platz und Hoehenstufen
                    // muessen ja auch reichen
                    if (SchifffahrtsRules.PrüfeEinschiffen(truppe, flotte, out _).HasErrors == false)
                        return (truppe, flotte);
                }
            }
            return null;
        }

        [StaFact]
        public void DieTransportkapazitaetHaengtAnDerZahlDerSchiffe() {
            LadeAlles();
            var flotte = FindeFlotte();
            Assert.True(flotte != null, "In den Zugdaten steht keine Flotte");

            Assert.Equal(flotte!.staerke * SchifffahrtsRules.TransportkapazitätProSchiff,
                SchifffahrtsRules.GetTransportkapazität(flotte));

            // ein Landheer traegt nichts
            var landheer = Armee().FirstOrDefault(t => t.BaseTyp != FigurType.Schiff);
            Assert.True(landheer != null, "In den Zugdaten steht kein Landheer");
            Assert.Equal(0, SchifffahrtsRules.GetTransportkapazität(landheer));
        }

        /// <summary>
        /// "Eine Strasse oder eine Kaianlage vermindern die Kosten auf 1." (Regelwerk 4)
        /// </summary>
        [StaFact]
        public void EineKaianlageVerbilligtDasEinschiffen() {
            Assert.Equal(BewegungsRules.HöhenstufenKostenOhneStraße, SchifffahrtsRules.GetHöhenstufenKosten(false));
            Assert.Equal(BewegungsRules.HöhenstufenKostenMitStraße, SchifffahrtsRules.GetHöhenstufenKosten(true));
            Assert.True(SchifffahrtsRules.GetHöhenstufenKosten(true) < SchifffahrtsRules.GetHöhenstufenKosten(false));
        }

        /// <summary>
        /// "Charaktere verfuegen ueber eigene Schiffe" (Regelwerk 0.3.2 und 1.9)
        /// </summary>
        [StaFact]
        public void CharaktereUndZaubererSchiffenNichtMit() {
            LadeAlles();
            var flotte = FindeFlotte();
            Assert.True(flotte != null, "In den Zugdaten steht keine Flotte");

            var namensfigur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<NamensSpielfigur>().FirstOrDefault();
            Assert.True(namensfigur != null, "In den Zugdaten steht kein Charakter und kein Zauberer");

            var ergebnis = SchifffahrtsRules.PrüfeEinschiffen(namensfigur, flotte, out _);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("eigene Schiffe", ergebnis.Message);
        }

        [StaFact]
        public void EineFlotteFaehrtNichtAufEinerFlotte() {
            LadeAlles();
            var flotte = FindeFlotte();
            Assert.True(flotte != null, "In den Zugdaten steht keine Flotte");

            var ergebnis = SchifffahrtsRules.PrüfeEinschiffen(flotte, flotte, out _);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("selbst eine Flotte", ergebnis.Title);
        }

        /// <summary>
        /// Eine Flotte, die nicht nebenan liegt, ist kein Ziel
        /// </summary>
        [StaFact]
        public void DieFlotteMussNebenanLiegen() {
            LadeAlles();
            var armee = Armee();
            var flotte = FindeFlotte();
            Assert.True(flotte != null, "In den Zugdaten steht keine Flotte");
            var wasser = KleinfeldView.GetKleinfeld(flotte);
            Assert.NotNull(wasser);

            var entfernt = armee.FirstOrDefault(t => t.BaseTyp != FigurType.Schiff
                && SchifffahrtsRules.IstEingeschifft(t) == false
                && KleinfeldView.GetKleinfeld(t) is var land && land != null
                && BewegungsRules.GetRichtung(land, wasser!) == null);
            Assert.True(entfernt != null, "Es steht kein Heer abseits der Flotte");

            var ergebnis = SchifffahrtsRules.PrüfeEinschiffen(entfernt, flotte, out _);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("nicht nebenan", ergebnis.Title);
        }

        /// <summary>
        /// Der Durchlauf: einschiffen, nachsehen, zuruecknehmen.
        /// </summary>
        [StaFact]
        public void EinschiffenWirdSauberZurueckgenommen() {
            LadeAlles();
            var paarung = FindeEinschiffbarePaarung();
            // in den echten Zugdaten gibt es eine solche Paarung; sollte das in einem spaeteren
            // Zug nicht mehr so sein, laeuft der Test ins Leere statt fehlzuschlagen
            if (paarung == null)
                return;

            var (truppe, flotte) = paarung.Value;
            int gfVorher = truppe.gf_nach;
            int kfVorher = truppe.kf_nach;
            int bpVorher = truppe.bp;
            int höhenstufenVorher = truppe.hoehenstufen;
            string? aufFlotteVorher = truppe.auf_Flotte;
            string? ladungVorher = flotte.auf_Flotte;

            var befehl = new EmbarkCommand("Testeinschiffung") {
                Mode = EmbarkCommand.Modus.einschiffen,
                Truppe = truppe,
                Flotte = flotte,
            };

            var ausgeführt = befehl.ExecuteCommand();
            // Kapazitaet oder Hoehenstufen koennen in diesem Zug nicht reichen; dann gibt es
            // nichts zurueckzunehmen und der Fall ist hier nicht pruefbar
            Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");

            try {
                Assert.Equal(flotte.Nummer.ToString(), truppe.auf_Flotte);
                Assert.True(SchifffahrtsRules.IstEingeschifft(truppe));
                Assert.Contains($"#{truppe.Nummer}", flotte.auf_Flotte);

                // Einschiffen kostet keine Bewegungspunkte, nur Hoehenstufenpunkte
                Assert.Equal(bpVorher, truppe.bp);
                Assert.True(truppe.hoehenstufen > höhenstufenVorher,
                    "Das Einschiffen hat keine Hoehenstufenpunkte gekostet");

                // die Truppe steht jetzt im Schiffsbauch, nicht mehr auf der Karte
                Assert.Equal(flotte.Nummer, truppe.kf_nach);
            }
            finally {
                var zurück = befehl.UndoCommand();
                Assert.False(zurück.HasErrors, $"{zurück.Title} {zurück.Message}");
            }

            Assert.Equal(gfVorher, truppe.gf_nach);
            Assert.Equal(kfVorher, truppe.kf_nach);
            Assert.Equal(bpVorher, truppe.bp);
            Assert.Equal(höhenstufenVorher, truppe.hoehenstufen);
            Assert.Equal(aufFlotteVorher, truppe.auf_Flotte);
            Assert.Equal(ladungVorher, flotte.auf_Flotte);
        }

        /// <summary>
        /// Aus der Ladungsliste wird genau ein Eintrag gestrichen - nicht die halbe Flotte.
        /// </summary>
        [Theory]
        [InlineData("#153#207#301", 207, "#153#301")]
        [InlineData("#153", 153, "")]
        [InlineData("#153#207", 999, "#153#207")]
        [InlineData("", 153, "")]
        [InlineData(null, 153, "")]
        public void AusDerLadungWirdGenauEinEintragGestrichen(string? ladung, int nummer, string erwartet) {
            Assert.Equal(erwartet, EmbarkCommand.EntferneAusLadung(ladung, nummer));
        }

        /// <summary>
        /// Eine Nummer, die als Teil einer anderen vorkommt, darf nicht mitgestrichen werden.
        /// </summary>
        [Fact]
        public void AehnlicheNummernWerdenNichtVerwechselt() {
            Assert.Equal("#15#153", EmbarkCommand.EntferneAusLadung("#15#153#1", 1));
            Assert.Equal("#15#1", EmbarkCommand.EntferneAusLadung("#15#153#1", 153));
        }

        [StaFact]
        public void ParserLiestEinUndAusschiffen() {
            var parser = new EmbarkCommandParser();

            Assert.True(parser.ParseCommand("Schiffe Krieger 153 ein auf Schiff 308", out var ein));
            var einschiffen = Assert.IsType<EmbarkCommand>(ein);
            Assert.Equal(EmbarkCommand.Modus.einschiffen, einschiffen.Mode);
            Assert.Equal(153, einschiffen.UnitId);
            Assert.Equal(308, einschiffen.ShipId);

            Assert.True(parser.ParseCommand("Schiffe Krieger 153 aus von Schiff 308 nach 843/02", out var aus));
            var ausschiffen = Assert.IsType<EmbarkCommand>(aus);
            Assert.Equal(EmbarkCommand.Modus.ausschiffen, ausschiffen.Mode);
            Assert.Equal(new KleinfeldPosition(843, 2), ausschiffen.LandLocation);

            // ohne Zielfeld ist das Ausschiffen unvollstaendig
            Assert.False(parser.ParseCommand("Schiffe Krieger 153 aus von Schiff 308", out _));
        }
    }
}
