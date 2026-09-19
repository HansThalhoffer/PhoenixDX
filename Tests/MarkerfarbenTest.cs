using Microsoft.Xna.Framework;
using PhoenixDX.Helper;
using PhoenixModel.View;

namespace Tests {

    /// <summary>
    /// Die Farben der Kartenmarkierungen.
    ///
    /// Gemeldet: "Das Hervorheben funktioniert, ist aber oft schwer erkennbar. Besonders gruen auf
    /// gruenen Feldern." Die Reichweite stand auf Hellgruen, und die Karte ist auf weiten Strecken
    /// gruen - Wald und Tiefland. Der zurueckgelegte Weg stand auf Kornblumenblau und war ueber
    /// Wasser ebenso schwer zu sehen, wo Schiffe nun einmal fahren.
    ///
    /// Gezeichnet wird das in MonoGame, und wie es aussieht laesst sich hier nicht pruefen.
    /// Pruefbar ist, dass die Tabelle vollstaendig ist, dass keine Farbe zweimal vergeben wurde
    /// und dass die beiden Bewegungsfarben nicht in den Farbbereichen des Gelaendes liegen.
    /// </summary>
    public class MarkerfarbenTest {

        /// <summary>
        /// Jeder Markertyp hat eine Farbe - ausser None, das ist keine Markierung.
        ///
        /// Fehlt eine, wirft die Kartendarstellung erst beim Zeichnen, und zwar mitten im Bild.
        /// </summary>
        [Fact]
        public void JederMarkertypHatEineFarbe() {
            foreach (MarkerType typ in Enum.GetValues<MarkerType>()) {
                if (typ == MarkerType.None)
                    continue;
                Assert.True(Markerfarben.Farben.ContainsKey(typ),
                    $"Fuer {typ} ist keine Farbe hinterlegt - die Karte wirft beim Zeichnen");
            }
            Assert.DoesNotContain(MarkerType.None, Markerfarben.Farben.Keys);
        }

        /// <summary>
        /// Keine Farbe zweimal - sonst bedeuten zwei Markierungen dasselbe Bild.
        /// </summary>
        [Fact]
        public void KeineFarbeIstZweimalVergeben() {
            var farben = Markerfarben.Farben.Values.ToList();
            Assert.Equal(farben.Count, farben.Distinct().Count());
        }

        /// <summary>
        /// Die Farben des Gelaendes, so wie die Kacheln aussehen: Wald und Tiefland gruen,
        /// Bergland und Hochland oliv-braun, Wasser blau, Wueste sandgelb.
        /// </summary>
        public static TheoryData<string, Color> Gelaende => new() {
            { "Wald", new Color(46, 110, 70) },
            { "Tiefland", new Color(104, 214, 92) },
            { "Hochland", new Color(176, 170, 116) },
            { "Bergland", new Color(120, 112, 70) },
            { "Wasser", new Color(108, 140, 196) },
            { "Wueste", new Color(246, 196, 108) },
        };

        /// <summary>
        /// Der Abstand zweier Farben im RGB-Wuerfel, 0 bis 255.
        /// </summary>
        private static double Abstand(Color a, Color b) {
            double dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return Math.Sqrt((dr * dr + dg * dg + db * db) / 3.0);
        }

        /// <summary>
        /// Die beiden Bewegungsfarben heben sich von jedem Gelaende ab.
        ///
        /// Hellgruen gegen Tiefland lag bei einem Abstand von 40 - das war der gemeldete Fehler.
        /// Verlangt wird das Doppelte. Die Zahl ist ein grober Riegel und kein Farbmodell; sie
        /// faengt den Fall, dass jemand eine Markierung in eine Gelaendefarbe legt.
        /// </summary>
        [Theory]
        [MemberData(nameof(Gelaende))]
        public void DieBewegungsfarbenHebenSichVomGelaendeAb(string gelaende, Color farbe) {
            const double Mindestabstand = 80;

            foreach (var typ in new[] { MarkerType.Bewegung, MarkerType.Weg }) {
                double abstand = Abstand(Markerfarben.Farben[typ], farbe);
                Assert.True(abstand >= Mindestabstand,
                    $"{typ} liegt mit einem Abstand von {abstand:F0} zu dicht an {gelaende} - "
                    + "auf diesem Gelaende ist die Markierung kaum zu erkennen");
            }
        }

        /// <summary>
        /// Und die alte Wahl faellt durch - sonst prueft der Test nichts.
        /// </summary>
        [Fact]
        public void DieAlteWahlWaereDurchgefallen() {
            var tiefland = new Color(104, 214, 92);
            var wasser = new Color(108, 140, 196);

            Assert.True(Abstand(Color.LightGreen, tiefland) < 80,
                "Hellgruen auf Tiefland waere nach diesem Massstab in Ordnung - dann misst er das Falsche");
            Assert.True(Abstand(Color.CornflowerBlue, wasser) < 80,
                "Kornblumenblau auf Wasser waere nach diesem Massstab in Ordnung");
        }
    }
}
