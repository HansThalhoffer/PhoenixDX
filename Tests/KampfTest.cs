using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;

namespace Tests {

    /// <summary>
    /// Der Rechenkern des Kampfes, geprueft gegen die Kampftabelle der Spielleitung
    /// (k_tabelle_2010.xlsm, Blatt "Kampfauswertung"). Die Zellbezuege stehen jeweils dabei,
    /// damit sich jede Zahl nachschlagen laesst.
    /// </summary>
    public class KampfTest {

        /// <summary>
        /// Die Bonustabelle, Zelle fuer Zelle aus Spalte H des Blattes.
        /// </summary>
        [Theory]
        [InlineData(Kampfvorteil.GeländeReiter, 140)]          // H26
        [InlineData(Kampfvorteil.GeländeKrieger, 140)]         // H27
        [InlineData(Kampfvorteil.Gardeheer, 300)]              // H28
        [InlineData(Kampfvorteil.Standardgelände, 50)]         // H29
        [InlineData(Kampfvorteil.AngriffStraßeHinauf, 20)]     // H30
        [InlineData(Kampfvorteil.AngriffAusWald, 20)]          // H31
        [InlineData(Kampfvorteil.AngriffInSumpf, 20)]          // H32
        [InlineData(Kampfvorteil.AngriffInWüste, 20)]          // H33
        [InlineData(Kampfvorteil.KampfBergab, 20)]             // H34
        [InlineData(Kampfvorteil.HinterBrücke, 30)]            // H35
        [InlineData(Kampfvorteil.HinterFluß, 50)]              // H36
        [InlineData(Kampfvorteil.HinterWall, 50)]              // H37
        [InlineData(Kampfvorteil.AusBurg, 100)]                // H38
        [InlineData(Kampfvorteil.AusStadt, 200)]               // H39
        [InlineData(Kampfvorteil.AusFestung, 300)]             // H40
        [InlineData(Kampfvorteil.AusHauptstadt, 400)]          // H41
        [InlineData(Kampfvorteil.AusFestungshauptstadt, 500)]  // H42
        [InlineData(Kampfvorteil.AusFremdemRüstort, 50)]       // H43
        [InlineData(Kampfvorteil.NachbarUnterstützung, 10)]    // H80
        public void DieBonustabelleStimmtMitDerKampftabelleUeberein(Kampfvorteil vorteil, int gutpunkte) {
            Assert.Equal(gutpunkte, KampfRules.GetGutpunkte(vorteil));
        }

        /// <summary>
        /// Die Nachbarunterstuetzung zaehlt je Kleinfeld (Kampftabelle J80: der Wert wird mit der
        /// Zahl der Felder multipliziert), alle anderen Vorteile zaehlen einmal.
        /// </summary>
        [Fact]
        public void DieNachbarunterstuetzungZaehltJeKleinfeld() {
            Assert.Equal(30, KampfRules.BerechneVorteile([Kampfvorteil.NachbarUnterstützung], 3));
            Assert.Equal(0, KampfRules.BerechneVorteile([Kampfvorteil.NachbarUnterstützung], 0));

            // andere Vorteile bleiben von der Zahl der Felder unberuehrt
            Assert.Equal(300, KampfRules.BerechneVorteile([Kampfvorteil.AusFestung], 7));
            Assert.Equal(350, KampfRules.BerechneVorteile(
                [Kampfvorteil.AusFestung, Kampfvorteil.HinterFluß], 0));
        }

        /// <summary>
        /// "Jedes Heer wird danach von seinen eigenen GP nochmals geschuetzt.
        /// Gutpunkte Einheit /100+1" (Errata 52 zu Regelwerk 5.1, Kampftabelle C85).
        /// </summary>
        [Fact]
        public void GutpunkteSchuetzenNachDerFormelDurchHundertPlusEins() {
            Assert.Equal(1.0, KampfRules.BerechneSchutzfaktor(0));
            Assert.Equal(2.0, KampfRules.BerechneSchutzfaktor(100));
            Assert.Equal(4.0, KampfRules.BerechneSchutzfaktor(300));

            // ohne Gutpunkte kommt der Schaden ungemindert an
            Assert.Equal(1000, KampfRules.MindereSchaden(1000, 0));
            // mit 300 Gutpunkten nur noch ein Viertel
            Assert.Equal(250, KampfRules.MindereSchaden(1000, 300));
        }

        /// <summary>
        /// Die Umrechnung in Baupunkte, die gemeinsame Waehrung des Kampfes
        /// (Kampftabelle B99 bis B107).
        /// </summary>
        [Fact]
        public void EinheitenWerdenNachDerTabelleInBaupunkteUmgerechnet() {
            var krieger = new Krieger { staerke = 1000, LKP = 2, SKP = 1, Pferde = 500 };
            // 1000 * 0,1 + 2 * 200 + 1 * 400 + 500 * 0,1 = 100 + 400 + 400 + 50
            Assert.Equal(950, KampfRules.BerechneBaupunkte(krieger));

            var reiter = new Reiter { staerke = 1000 };
            Assert.Equal(200, KampfRules.BerechneBaupunkte(reiter));   // 1000 * 0,2

            // Schiffe zaehlen ihre Geschuetze als Kriegsschiffe, beide mit 200
            var schiff = new Schiffe { staerke = 10, LKP = 3, SKP = 2 };
            Assert.Equal(10 * 10 + 3 * 200 + 2 * 200, KampfRules.BerechneBaupunkte(schiff));

            Assert.Equal(0, KampfRules.BerechneBaupunkte(null));
        }

        /// <summary>
        /// Der Schaden verteilt sich nach dem Baupunktanteil auf die Heere einer Seite und wird
        /// dann je Heer von dessen Gutpunkten gemindert (Kampftabelle E91 bis E93).
        /// </summary>
        [Fact]
        public void SchadenVerteiltSichNachBaupunktenUndWirdJeHeerGemindert() {
            // zwei Heere, 300 und 100 Baupunkte, ohne Gutpunkte: 3 zu 1
            var ohneGP = KampfRules.VerteileSchaden(400, [(300, 0), (100, 0)]);
            Assert.Equal(300, ohneGP[0]);
            Assert.Equal(100, ohneGP[1]);

            // dasselbe, aber das erste Heer hat 100 Gutpunkte: sein Anteil halbiert sich
            var mitGP = KampfRules.VerteileSchaden(400, [(300, 100), (100, 0)]);
            Assert.Equal(150, mitGP[0]);
            Assert.Equal(100, mitGP[1]);

            // ein Heer ohne Baupunkte bekommt nichts ab
            var leer = KampfRules.VerteileSchaden(400, [(400, 0), (0, 500)]);
            Assert.Equal(400, leer[0]);
            Assert.Equal(0, leer[1]);

            // und ohne Baupunkte auf der ganzen Seite passiert gar nichts
            Assert.All(KampfRules.VerteileSchaden(400, [(0, 0)]), wert => Assert.Equal(0, wert));
        }

        /// <summary>
        /// Der Schaden wird ueber den Baupunktanteil in Stueck zurueckgerechnet
        /// (Kampftabelle C99 bis D107).
        /// </summary>
        [Fact]
        public void SchadenWirdInVerloreneEinheitenZurueckgerechnet() {
            var krieger = new Krieger { staerke = 1000 };   // 100 BP
            var halb = KampfRules.BerechneVerluste(krieger, 50);
            Assert.Equal(500, halb.Krieger);

            // mehr Schaden als das Heer wiegt kostet hoechstens das ganze Heer
            var alles = KampfRules.BerechneVerluste(krieger, 10000);
            Assert.Equal(1000, alles.Krieger);

            // kein Schaden, keine Verluste
            Assert.Equal(0, KampfRules.BerechneVerluste(krieger, 0).Krieger);
            Assert.Equal(0, KampfRules.BerechneVerluste(null, 100).Krieger);
        }

        /// <summary>
        /// Ein Gardeheer verliert nur ein Drittel (Kampftabelle D99: bei Garde durch 3).
        /// </summary>
        [Fact]
        public void EinGardeheerVerliertNurEinDrittel() {
            var normal = new Krieger { staerke = 900 };             // 90 BP
            var garde = new Krieger { staerke = 900, Garde = true };

            Assert.Equal(900, KampfRules.BerechneVerluste(normal, 90).Krieger);
            Assert.Equal(300, KampfRules.BerechneVerluste(garde, 90).Krieger);
        }

        /// <summary>
        /// Die Verluste treffen alle Gattungen eines Heeres nach ihrem Baupunktanteil - auch
        /// Heerfuehrer und Pferde.
        /// </summary>
        [Fact]
        public void VerlusteTreffenAlleGattungenEinesHeeres() {
            var heer = new Krieger { staerke = 1000, hf = 10, LKP = 4, SKP = 2, Pferde = 1000 };
            double gesamt = KampfRules.BerechneBaupunkte(heer);
            var verluste = KampfRules.BerechneVerluste(heer, gesamt / 2);

            Assert.Equal(500, verluste.Krieger);
            Assert.Equal(5, verluste.Heerführer);
            Assert.Equal(2, verluste.LKP);
            Assert.Equal(1, verluste.SKP);
            Assert.Equal(500, verluste.Pferde);
        }

        /// <summary>
        /// Der Prozentwurf der Kampftabelle entscheidet ueber die Nachkommastelle
        /// (Kampftabelle G103 und I103):
        /// TRUNC(rest*100) - (100-wurf) > 0 bedeutet aufrunden.
        ///
        /// Aufgerundet wird also, wenn der Wurf hoch genug ausfaellt: bei 0,40 Rest ab einem Wurf
        /// von 61. Je groesser die Nachkommastelle, desto niedriger darf der Wurf sein.
        /// </summary>
        [Fact]
        public void DerProzentwurfEntscheidetUeberDieNachkommastelle() {
            // 3,40 Verluste: 40 - (100-wurf) > 0 gilt ab Wurf 61
            Assert.Equal(3, KampfRules.RundeMitWurf(3.40, 60));
            Assert.Equal(4, KampfRules.RundeMitWurf(3.40, 61));
            Assert.Equal(3, KampfRules.RundeMitWurf(3.40, 30));
            Assert.Equal(4, KampfRules.RundeMitWurf(3.40, 100));

            // 3,90 Verluste: schon ab Wurf 11
            Assert.Equal(3, KampfRules.RundeMitWurf(3.90, 10));
            Assert.Equal(4, KampfRules.RundeMitWurf(3.90, 11));

            // ohne Nachkommastelle aendert kein Wurf etwas
            Assert.Equal(3, KampfRules.RundeMitWurf(3.0, 1));
            Assert.Equal(3, KampfRules.RundeMitWurf(3.0, 100));
            Assert.Equal(0, KampfRules.RundeMitWurf(0, 100));
        }

        /// <summary>
        /// Wird ein Ruestort verteidigt, traegt er 67 Prozent des Fernkampfschadens und die
        /// Truppen 33 (Kampftabelle D86 und D87, Errata 52 zu Regelwerk 5.1).
        /// </summary>
        [Fact]
        public void EinRuestortTraegtDenGroesstenTeilDesFernkampfschadens() {
            var mit = KampfRules.TeileFernkampfschaden(1000, rüstort: true);
            Assert.Equal(670, mit.AufRüstort);
            Assert.Equal(330, mit.AufTruppen, 6);

            var ohne = KampfRules.TeileFernkampfschaden(1000, rüstort: false);
            Assert.Equal(0, ohne.AufRüstort);
            Assert.Equal(1000, ohne.AufTruppen);
        }

        /// <summary>
        /// Ein Ruestort liegt vor, wenn einer der Ruestortvorteile gilt - ein Wall oder ein Fluss
        /// macht noch keinen (Kampftabelle C83).
        /// </summary>
        [Fact]
        public void NurRuestorteZaehlenAlsRuestort() {
            Assert.True(KampfRules.VerteidigtRüstort([Kampfvorteil.AusBurg]));
            Assert.True(KampfRules.VerteidigtRüstort([Kampfvorteil.HinterFluß, Kampfvorteil.AusFestungshauptstadt]));
            Assert.False(KampfRules.VerteidigtRüstort([Kampfvorteil.HinterWall, Kampfvorteil.HinterFluß]));
            Assert.False(KampfRules.VerteidigtRüstort([]));

            // der fremde Ruestort bringt Gutpunkte, schuetzt aber nicht wie ein eigener
            Assert.False(KampfRules.VerteidigtRüstort([Kampfvorteil.AusFremdemRüstort]));
        }

        /// <summary>
        /// Reiter sind im Tiefland, Hochland und in der Wueste im Vorteil, Krieger im Wald und im
        /// Sumpf (Kampftabelle Zeilen 26 und 27).
        /// </summary>
        [Theory]
        [InlineData(FigurType.Reiter, TerrainType.Tiefland, Kampfvorteil.GeländeReiter)]
        [InlineData(FigurType.Reiter, TerrainType.Hochland, Kampfvorteil.GeländeReiter)]
        [InlineData(FigurType.Reiter, TerrainType.Wüste, Kampfvorteil.GeländeReiter)]
        [InlineData(FigurType.Krieger, TerrainType.Wald, Kampfvorteil.GeländeKrieger)]
        [InlineData(FigurType.Krieger, TerrainType.Sumpf, Kampfvorteil.GeländeKrieger)]
        public void DasGelaendeBevorzugtDiePassendeGattung(FigurType gattung, TerrainType gelände, Kampfvorteil erwartet) {
            Assert.Equal(erwartet, KampfRules.GetGeländevorteil(gattung, gelände));
        }

        [Theory]
        [InlineData(FigurType.Reiter, TerrainType.Wald)]
        [InlineData(FigurType.Reiter, TerrainType.Sumpf)]
        [InlineData(FigurType.Krieger, TerrainType.Tiefland)]
        [InlineData(FigurType.Krieger, TerrainType.Wüste)]
        [InlineData(FigurType.Krieger, TerrainType.Gebirge)]
        public void ImFalschenGelaendeGibtEsKeinenVorteil(FigurType gattung, TerrainType gelände) {
            Assert.Null(KampfRules.GetGeländevorteil(gattung, gelände));
        }
    }
}
