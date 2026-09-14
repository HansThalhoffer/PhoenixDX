using PhoenixModel.Commands;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Kaianlage - Issue #67.
    ///
    /// Sie kann dreierlei: eine weitere Hoehenstufe wie eine Strasse ermoeglichen, das Ein- und
    /// Ausschiffen an dieser Gemarkseite verbilligen, und dem Verteidiger dahinter Gutpunkte geben
    /// (Regelwerk 1.5.2 und Anhang 10.1).
    /// </summary>
    public class KaianlageTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
        }

        /// <summary>
        /// Kosten und Baupunkte stehen so im Regelwerk 1.5.2: 60 Baupunkte, 3.000 GS.
        /// </summary>
        [StaFact]
        public void EineKaianlageKostet60BaupunkteUnd3000Goldstuecke() {
            LadeAlles();
            var kosten = KostenView.GetKosten(ConstructionElementType.Kai);
            Assert.True(kosten != null, "Für die Kaianlage gibt es keinen Eintrag in der Kostentabelle");
            Assert.Equal(60, kosten!.BauPunkte);
            Assert.Equal(3000, kosten.GS);
        }

        /// <summary>
        /// Eine Kaianlage verbilligt die Hoehenstufe wie eine Strasse - das ist ihr eigentlicher
        /// Zweck (Regelwerk 1.5.2).
        /// </summary>
        [StaFact]
        public void EineKaianlageVerbilligtDieHoehenstufeWieEineStrasse() {
            Assert.Equal(BewegungsRules.HöhenstufenKostenMitStraße, SchifffahrtsRules.GetHöhenstufenKosten(true));
            Assert.Equal(BewegungsRules.HöhenstufenKostenOhneStraße, SchifffahrtsRules.GetHöhenstufenKosten(false));
            Assert.True(SchifffahrtsRules.GetHöhenstufenKosten(true) < SchifffahrtsRules.GetHöhenstufenKosten(false));
        }

        /// <summary>
        /// In den echten Kartendaten stehen Kaianlagen - sie sind also keine graue Theorie.
        /// </summary>
        [StaFact]
        public void InDerKarteStehenKaianlagen() {
            LadeAlles();
            int kanten = SharedData.Map!.Values.Sum(kf =>
                new[] { kf.Kai_NW, kf.Kai_NO, kf.Kai_O, kf.Kai_SO, kf.Kai_SW, kf.Kai_W }
                    .Count(wert => (wert ?? 0) > 0));
            Assert.True(kanten > 0, "In der Karte steht keine einzige Kaianlage");

            // und die Bewegung erkennt sie beidseitig
            foreach (var kf in SharedData.Map!.Values) {
                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    if (KleinfeldView.HasKai(kf, richtung) == false)
                        continue;
                    var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(kf, richtung));
                    if (nachbar == null)
                        continue;
                    Assert.True(BewegungsRules.HatKai(kf, nachbar, richtung),
                        $"Der Kai zwischen {kf.Bezeichner} und {nachbar.Bezeichner} wird nicht erkannt");
                    return;
                }
            }
        }

        /// <summary>
        /// Der Vorteil der Verteidigung hinter einer Kaianlage: 30 Gutpunkte, aber nur ohne
        /// Ruestort auf dem Feld. Steht einer, zaehlt dessen Vorteil - die beiden addieren sich
        /// nicht (Regelwerk Anhang 10.1).
        /// </summary>
        [Fact]
        public void HinterEinerKaianlageGibtEs30GutpunkteAberNichtZusaetzlichZumRuestort() {
            Assert.Equal(30, KampfRules.GetGutpunkte(Kampfvorteil.HinterKaianlage));
            Assert.Equal(20, KampfRules.GetGutpunkte(Kampfvorteil.GegenAusschiffendenAngreifer));

            // freies Feld: der Vorteil kommt dazu
            var aufFreiemFeld = KampfRules.ErgänzeKaianlage([Kampfvorteil.GeländeKrieger], true);
            Assert.Contains(Kampfvorteil.HinterKaianlage, aufFreiemFeld);

            // mit Ruestort: er bleibt weg
            var mitBurg = KampfRules.ErgänzeKaianlage([Kampfvorteil.AusBurg], true);
            Assert.DoesNotContain(Kampfvorteil.HinterKaianlage, mitBurg);
            Assert.Contains(Kampfvorteil.AusBurg, mitBurg);

            // ohne Kaianlage sowieso nicht
            var ohneKai = KampfRules.ErgänzeKaianlage([Kampfvorteil.GeländeKrieger], false);
            Assert.DoesNotContain(Kampfvorteil.HinterKaianlage, ohneKai);

            // und er wird nicht doppelt aufgenommen
            var zweimal = KampfRules.ErgänzeKaianlage(aufFreiemFeld, true);
            Assert.Single(zweimal.Where(v => v == Kampfvorteil.HinterKaianlage));
        }

        /// <summary>
        /// Der Vorteil zaehlt in der Summe mit, sobald er in der Liste steht.
        /// </summary>
        [Fact]
        public void DerVorteilZaehltInDerSummeMit() {
            int ohne = KampfRules.BerechneVorteile([Kampfvorteil.GeländeKrieger]);
            int mit = KampfRules.BerechneVorteile([Kampfvorteil.GeländeKrieger,
                                                   Kampfvorteil.HinterKaianlage]);
            Assert.Equal(ohne + 30, mit);
        }
    }
}
