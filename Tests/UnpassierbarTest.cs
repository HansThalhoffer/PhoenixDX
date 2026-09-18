using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Unpassierbares Gelaende wird beim Namen genannt - auch beim einzelnen Schritt.
    ///
    /// Die Bewegungstabellen schreiben 99 Bewegungspunkte fuer Gelaende, das eine Figur gar nicht
    /// betreten kann. Das ist eine Schreibweise, kein Preis. PruefeSchritt hat daraus "die Einheit
    /// hat nicht genug Bewegungspunkte" gemacht, und das ist falsch: ein Reiter kommt nicht ueber
    /// das Meer, auch nicht mit vollem Vorrat.
    ///
    /// Aufgefallen ist es an der Reichweite: an der Kueste stehen vier von sechs Richtungen mit
    /// einer Begruendung da, die nach Erschoepfung aussieht. Die hervorgehobene Flaeche wirkt dann
    /// einseitig, ohne dass der Grund - das Meer - irgendwo steht.
    /// </summary>
    public class UnpassierbarTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>
        /// Sucht ein Landheer mit vollen Bewegungspunkten, das am Wasser steht.
        /// </summary>
        private static (Spielfigur Figur, Direction Richtung) FindeLandheerAmWasser() {
            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                         .Where(Plausibilität.IsValid)) {
                if (figur.Typ != FigurType.Krieger && figur.Typ != FigurType.Reiter)
                    continue;
                // volle Punkte, damit "zu wenig BP" als Erklaerung ausscheidet
                if (figur.bp < figur.bp_max)
                    continue;
                var start = KleinfeldView.GetKleinfeld(figur);
                if (start == null)
                    continue;
                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(start, richtung));
                    if (nachbar != null && nachbar.IsWasser)
                        return (figur, richtung);
                }
            }
            Assert.Fail("Kein Landheer mit vollen Bewegungspunkten am Wasser gefunden");
            return default;
        }

        /// <summary>
        /// Das Meer heisst Meer und nicht "zu wenig Bewegungspunkte".
        /// </summary>
        [StaFact]
        public void DasMeerWirdBeimNamenGenannt() {
            LadeAlles();
            var (figur, richtung) = FindeLandheerAmWasser();

            var schritt = BewegungsRules.PrüfeSchritt(figur, richtung);
            Assert.True(schritt.HasErrors);
            Assert.Contains("nicht passierbar", schritt.Title);
            Assert.Contains(figur.Typ.ToString(), schritt.Title);
            // und nicht die alte, irrefuehrende Begruendung
            Assert.DoesNotContain("Bewegungspunkte", schritt.Title);
            Assert.Contains("vollen Bewegungspunkten", schritt.Message);
        }

        /// <summary>
        /// Zu wenig Punkte bleibt zu wenig Punkte - die alte Begruendung darf nicht verschwinden,
        /// sie war nur an der falschen Stelle.
        /// </summary>
        [StaFact]
        public void ZuWenigPunkteHeisstWeiterhinZuWenigPunkte() {
            LadeAlles();
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(f => BewegungsRules.GetErreichbareFelder(f).Count > 0);
            Assert.True(figur != null, "Keine eigene Figur kann sich bewegen");

            var start = KleinfeldView.GetKleinfeld(figur!)!;
            // eine Richtung, in die es mit vollen Punkten ginge
            Direction? gangbar = null;
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                if (BewegungsRules.PrüfeSchritt(figur!, richtung).HasErrors == false) {
                    gangbar = richtung;
                    break;
                }
            }
            Assert.True(gangbar != null, $"{figur!.Bezeichner} kann in keine Richtung ziehen");

            int bpVorher = figur.bp;
            try {
                figur.bp = 1;
                var schritt = BewegungsRules.PrüfeSchritt(figur, gangbar!.Value);
                Assert.True(schritt.HasErrors);
                Assert.Contains("nicht genug Bewegungspunkte", schritt.Title);
                Assert.DoesNotContain("nicht passierbar", schritt.Title);
            }
            finally {
                figur.bp = bpVorher;
            }
        }

        /// <summary>
        /// Die Reichweite selbst aendert sich dadurch nicht - es geht um die Begruendung, nicht
        /// um die Regel. Ein unpassierbares Feld war vorher unerreichbar und ist es nachher.
        /// </summary>
        [StaFact]
        public void DieReichweiteBleibtDieselbe() {
            LadeAlles();
            var (figur, richtung) = FindeLandheerAmWasser();
            var wasser = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(
                KleinfeldView.GetKleinfeld(figur)!, richtung))!;

            var erreichbar = BewegungsRules.GetErreichbareFelder(figur)
                .Select(feld => feld.Bezeichner).ToHashSet();
            Assert.DoesNotContain(wasser.Bezeichner, erreichbar);
            Assert.True(erreichbar.Count > 0, $"{figur.Bezeichner} erreicht gar nichts");
        }
    }
}
