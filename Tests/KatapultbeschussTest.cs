using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Auswertung des Katapultbeschusses - der Schritt vor dem Nahkampf
    /// (Regelwerk 5.1, Kampftabelle C2 bis E93, Blatt "Werte nach Katabeschuss").
    ///
    /// Gewuerfelt wird bei der Spielleitung; hier faengt die Rechnung bei den Trefferpunkten an.
    /// </summary>
    public class KatapultbeschussTest {

        /// <summary>
        /// "Alle Trefferpunkte in diesem Zielfeld werden addiert und durch die beim Verteidiger
        /// vorhandenen Gutpunkte/100+1 dividiert" - und steht ein Ruestort auf dem Feld, traegt
        /// er 67 Prozent davon (Regelwerk 5.1).
        /// </summary>
        [Fact]
        public void ZuerstSchuetztDasBauwerkDannTraegtEsDenGroesstenTeil() {
            var heer = new Krieger { staerke = 1000 };
            var ergebnis = FernkampfRules.WerteBeschussAus(4000,
                [new FernkampfRules.Beschossen(heer)],
                [Kampfvorteil.AusFestung]);

            // 300 Gutpunkte der Festung: 4000 / (300/100 + 1) = 1000
            Assert.Equal(1000, ergebnis.Schaden, 6);
            Assert.Equal(670, ergebnis.AufRüstort, 6);
            Assert.Equal(330, ergebnis.AufTruppen, 6);

            // ohne Ruestort tragen die Truppen alles
            var imFreien = FernkampfRules.WerteBeschussAus(4000,
                [new FernkampfRules.Beschossen(heer)],
                [Kampfvorteil.HinterFluß]);
            Assert.Equal(0, imFreien.AufRüstort);
            Assert.Equal(4000 / 1.5, imFreien.AufTruppen, 6);
        }

        /// <summary>
        /// "Die angerichteten Verluste an Baupunkten werden der prozentualen Zusammensetzung
        /// des/der Heere/s entsprechend verteilt" (Regelwerk 5.1, Kampftabelle E91 bis E93).
        /// </summary>
        [Fact]
        public void DerSchadenVerteiltSichNachDenBaupunktenDerHeere() {
            var gross = new Krieger { staerke = 2000 };   // 200 BP
            var klein = new Krieger { staerke = 1000 };   // 100 BP

            var ergebnis = FernkampfRules.WerteBeschussAus(30, [
                new FernkampfRules.Beschossen(gross),
                new FernkampfRules.Beschossen(klein),
            ]);

            // 30 BP auf 300 BP Gesamtstaerke: jeder verliert ein Zehntel, das grosse Heer doppelt
            // soviele Koepfe wie das kleine
            Assert.Equal(20, ergebnis.Verluste[0].Baupunkte, 6);
            Assert.Equal(10, ergebnis.Verluste[1].Baupunkte, 6);
            Assert.Equal(200, ergebnis.Verluste[0].Krieger);
            Assert.Equal(100, ergebnis.Verluste[1].Krieger);
        }

        /// <summary>
        /// "Jedes Heer wird danach von seinen eigenen GP nochmals geschuetzt.
        /// Gutpunkte Einheit /100+1" (Errata 52 zu Regelwerk 5.1).
        /// </summary>
        [Fact]
        public void DieEigenenGutpunkteDesHeeresSchuetzenZusaetzlich() {
            var ohne = new Krieger { staerke = 1000 };
            var mit = new Krieger { staerke = 1000 };

            var ergebnis = FernkampfRules.WerteBeschussAus(20, [
                new FernkampfRules.Beschossen(ohne),
                new FernkampfRules.Beschossen(mit, Gutpunkte: 100),
            ]);

            // beide Heere sind gleich stark, bekommen also je 10 BP zugeteilt - das geschuetzte
            // Heer nimmt davon nur die Haelfte
            Assert.Equal(10, ergebnis.Verluste[0].Baupunkte, 6);
            Assert.Equal(5, ergebnis.Verluste[1].Baupunkte, 6);
            Assert.Equal(100, ergebnis.Verluste[0].Krieger);
            Assert.Equal(50, ergebnis.Verluste[1].Krieger);
        }

        /// <summary>
        /// "Werden gebannte und ungebannte Truppen von Fernkampfwaffen beschossen, so werden nur
        /// die ungebannten Truppen getroffen. Die ungebannten Truppen ziehen den gesamten Schaden
        /// auf sich." (Regelwerk 5.1)
        /// </summary>
        [Fact]
        public void GebannteTruppenZiehenKeinenBeschussAufSich() {
            var gebannt = new Krieger { staerke = 1000, isbanned = 1000 };
            var frei = new Krieger { staerke = 1000 };

            var ergebnis = FernkampfRules.WerteBeschussAus(10, [
                new FernkampfRules.Beschossen(gebannt, Gebannt: gebannt.isbanned),
                new FernkampfRules.Beschossen(frei),
            ]);

            Assert.Equal(0, ergebnis.Verluste[0].Baupunkte);
            Assert.Equal(0, ergebnis.Verluste[0].Krieger);
            Assert.Equal(10, ergebnis.Verluste[1].Baupunkte, 6);
            Assert.Equal(100, ergebnis.Verluste[1].Krieger);

            // teilweise gebannt heisst: nur der ungebannte Teil zaehlt als Ziel
            Assert.Equal(100, KampfRules.BerechneWirksameBaupunkte(new Krieger { staerke = 1000 }, 0), 6);
            Assert.Equal(60, KampfRules.BerechneWirksameBaupunkte(new Krieger { staerke = 1000 }, 400), 6);
            // die Fernkampfwaffen des Heeres bleiben im Ziel, auch wenn alle Krieger gebannt sind
            Assert.Equal(200, KampfRules.BerechneWirksameBaupunkte(new Krieger { staerke = 1000, LKP = 1 }, 1000), 6);
        }

        /// <summary>
        /// Ohne Treffer oder ohne Ziel passiert nichts - und es fliegt nichts.
        /// </summary>
        [Fact]
        public void OhneTrefferOderZielPassiertNichts() {
            var heer = new Krieger { staerke = 1000 };

            var ohneTreffer = FernkampfRules.WerteBeschussAus(0, [new FernkampfRules.Beschossen(heer)]);
            Assert.Single(ohneTreffer.Verluste);
            Assert.Equal(0, ohneTreffer.Verluste[0].Krieger);
            Assert.Equal(0, ohneTreffer.Schaden);

            Assert.Empty(FernkampfRules.WerteBeschussAus(1000, null).Verluste);
            Assert.Empty(FernkampfRules.WerteBeschussAus(1000, []).Verluste);
        }

        /// <summary>
        /// "Waelle - dabei wird im dahinter liegenden Gemark kein Schaden verursacht. Es muss jeder
        /// Wall einzeln anvisiert werden; ueberschuessige Trefferpunkte verfallen."
        /// (Regelwerk 5.1)
        /// </summary>
        [Fact]
        public void UeberschuessigeTrefferpunkteAmWallVerfallen() {
            Assert.Equal(300, FernkampfRules.BerechneWallschaden(500, 300));
            Assert.Equal(200, FernkampfRules.BerechneWallschaden(200, 300));
            Assert.Equal(0, FernkampfRules.BerechneWallschaden(0, 300));
            Assert.Equal(0, FernkampfRules.BerechneWallschaden(500, 0));
        }

        /// <summary>
        /// "Die Beschaedigung einer Einheit wird in % umgerechnet und dies ergibt die Chance mit
        /// welcher die Einheit zerstoert wird" - beide Beispiele des Regelwerks 1.5.11.
        /// </summary>
        [Fact]
        public void DieZerstoerungschanceStehtSoImRegelwerk() {
            // Beispiel 1: ein LKP mit 30 von 100 moeglichen Schadenspunkten
            Assert.Equal(200, FernkampfRules.GetBaupunkte(Fernkampfwaffe.Leicht));
            Assert.Equal(100, FernkampfRules.GetZerstörungsschwelle(Fernkampfwaffe.Leicht));
            Assert.Equal(0.30, FernkampfRules.BerechneZerstörungschance(Fernkampfwaffe.Leicht, 30), 6);

            // Beispiel 2: ein SKS mit 180 von 200 moeglichen Schadenspunkten
            Assert.Equal(400, FernkampfRules.GetBaupunkte(Fernkampfwaffe.Schwer));
            Assert.Equal(200, FernkampfRules.GetZerstörungsschwelle(Fernkampfwaffe.Schwer));
            Assert.Equal(0.90, FernkampfRules.BerechneZerstörungschance(Fernkampfwaffe.Schwer, 180), 6);

            // ab der Schwelle ist die Waffe sicher hin, darunter gibt es keine negative Chance
            Assert.Equal(1.0, FernkampfRules.BerechneZerstörungschance(Fernkampfwaffe.Schwer, 200), 6);
            Assert.Equal(1.0, FernkampfRules.BerechneZerstörungschance(Fernkampfwaffe.Leicht, 5000), 6);
            Assert.Equal(0, FernkampfRules.BerechneZerstörungschance(Fernkampfwaffe.Leicht, 0));
        }
    }
}
