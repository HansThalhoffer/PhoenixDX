using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Gruende, warum eine Figur ein Feld nicht erreicht.
    ///
    /// Die Wegsuche verwirft einen Schritt und behaelt den Grund fuer sich; genau der ist aber die
    /// haeufigere Frage. Geprueft wird gegen die echten Karten- und Zugdaten.
    /// </summary>
    public class UnerreichbarkeitTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            BewegungsRules.ResetCache();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>Eine eigene Figur, die sich tatsaechlich bewegen kann</summary>
        private static Spielfigur FindeBeweglicheFigur() {
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(f => BewegungsRules.GetErreichbareFelder(f).Count > 0);
            Assert.True(figur != null, "Keine eigene Figur kann sich bewegen");
            return figur!;
        }

        /// <summary>
        /// Was erreichbar ist, braucht keine Erklaerung. Sonst widerspraechen sich Hervorhebung und
        /// Hinweis gegenseitig.
        /// </summary>
        [StaFact]
        public void WasErreichbarIstBrauchtKeineErklaerung() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();

            foreach (var feld in BewegungsRules.GetErreichbareFelder(figur))
                Assert.True(BewegungsRules.ErkläreUnerreichbarkeit(figur, feld).Count == 0,
                    $"{feld.Bezeichner} ist erreichbar, wird aber erklaert: "
                    + string.Join(" / ", BewegungsRules.ErkläreUnerreichbarkeit(figur, feld)));
        }

        /// <summary>
        /// Umgekehrt: zu einem Feld, das nicht erreichbar ist, muss mindestens ein Grund kommen -
        /// und keiner davon darf leer sein.
        /// </summary>
        [StaFact]
        public void EinUnerreichbaresFeldNenntMindestensEinenGrund() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var erreichbar = BewegungsRules.GetErreichbareFelder(figur)
                .Select(feld => feld.Bezeichner).ToHashSet();

            var start = KleinfeldView.GetKleinfeld(figur);
            int geprüft = 0;
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(start!, richtung));
                if (nachbar == null || erreichbar.Contains(nachbar.Bezeichner))
                    continue;

                var gründe = BewegungsRules.ErkläreUnerreichbarkeit(figur, nachbar);
                Assert.True(gründe.Count > 0, $"{nachbar.Bezeichner} ist nicht erreichbar, aber unerklaert");
                Assert.All(gründe, grund => Assert.False(string.IsNullOrWhiteSpace(grund)));
                geprüft++;
            }
            Assert.True(geprüft > 0, "Alle Nachbarfelder sind erreichbar - dann prueft dieser Test nichts");
        }

        /// <summary>
        /// Gelaende, das die Figur gar nicht betreten kann, wird beim Namen genannt - nicht als
        /// "zu wenig Bewegungspunkte" getarnt. In den Bewegungstabellen steht dafuer 99, und das
        /// sah bisher aus wie eine sehr teure Strecke.
        /// </summary>
        [StaFact]
        public void UnpassierbaresGelaendeWirdBeimNamenGenannt() {
            LadeAlles();

            // ein Landheer mit einem Wasserfeld als Nachbarn
            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).Where(Plausibilität.IsValid)) {
                if (figur.Typ != FigurType.Krieger && figur.Typ != FigurType.Reiter)
                    continue;
                var start = KleinfeldView.GetKleinfeld(figur);
                if (start == null)
                    continue;

                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(start, richtung));
                    if (nachbar == null || nachbar.IsWasser == false)
                        continue;

                    var gründe = BewegungsRules.ErkläreUnerreichbarkeit(figur, nachbar);
                    Assert.Contains(gründe, grund => grund.Contains("nicht passierbar"));
                    Assert.Contains(gründe, grund => grund.Contains(figur.Typ.ToString()));
                    // und dabei bleibt es: die Nachbarfelder durchzugehen braechte nur noch
                    // "nicht genug Bewegungspunkte" - dieselbe Sache ein zweites Mal
                    Assert.Single(gründe);
                    return;
                }
            }
            Assert.Fail("Kein Landheer mit einem Wasserfeld als Nachbarn gefunden");
        }

        /// <summary>
        /// Ohne Bewegungspunkte gibt es genau einen Grund, und der steht fuer sich - eine Liste
        /// mit sechs Nachbarfeldern waere hier nur Laerm.
        /// </summary>
        [StaFact]
        public void OhneBewegungspunkteGibtEsGenauEinenGrund() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var start = KleinfeldView.GetKleinfeld(figur)!;
            var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(start, Direction.O));

            int bpVorher = figur.bp;
            try {
                figur.bp = 0;
                var gründe = BewegungsRules.ErkläreUnerreichbarkeit(figur, nachbar ?? start);
                Assert.Single(gründe);
                Assert.Contains("keine Bewegungspunkte", gründe[0]);
            }
            finally {
                figur.bp = bpVorher;
            }
        }

        /// <summary>
        /// In der Ruestphase wird nicht bewegt - dann ist die Phase der Grund und nicht das Feld.
        /// </summary>
        [StaFact]
        public void InDerRuestphaseIstDiePhaseDerGrund() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var ziel = BewegungsRules.GetErreichbareFelder(figur).First();

            int phaseVorher = ZugView.Settings!.Phase;
            try {
                TestSetup.SetzePhase(Zugphase.Rüstphase);
                var gründe = BewegungsRules.ErkläreUnerreichbarkeit(figur, ziel);
                Assert.Single(gründe);
                Assert.Contains("wird nicht bewegt", gründe[0]);
            }
            finally {
                TestSetup.SetzePhase((Zugphase)phaseVorher);
            }
        }

        /// <summary>
        /// Wenn gar kein Feld erreichbar ist, nennt die Regel den Grund - und in der Ruestphase ist
        /// das die Phase und nicht die Bewegungspunkte.
        ///
        /// Das Kontextmenue hat frueher geraten, es laege an den Punkten. Seit die Anwendung jeden
        /// Zug in der Ruestphase beginnt, ist das fast immer falsch.
        /// </summary>
        [StaFact]
        public void OhneErreichbaresFeldNenntDieRegelDenGrund() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();

            int phaseVorher = ZugView.Settings!.Phase;
            try {
                TestSetup.SetzePhase(Zugphase.Rüstphase);
                Assert.Empty(BewegungsRules.GetErreichbareFelder(figur));

                var grund = BewegungsRules.ErkläreWarumNichtsErreichbar(figur);
                Assert.True(grund.HasErrors);
                Assert.Contains("Rüstphase", grund.Title);
                Assert.Contains("Zug", grund.Message);
                // und nicht das, was frueher geraten wurde
                Assert.DoesNotContain("Bewegungspunkte mehr", grund.Title);
            }
            finally {
                TestSetup.SetzePhase((Zugphase)phaseVorher);
            }

            // in der Bewegungsphase liegt es an der Figur
            var erschoepft = BewegungsRules.ErkläreWarumNichtsErreichbar(null);
            Assert.True(erschoepft.HasErrors);
            Assert.Contains("keine Spielfigur", erschoepft.Title);
        }

        /// <summary>
        /// Eine Einheit, die sich in diesem Zug schon bewegt hat, erfaehrt genau das - mit den
        /// beiden Zahlen, um die es geht.
        ///
        /// Gemeldet wurde ein Reiter mit 1 von 21 Bewegungspunkten, der keine moeglichen Zuege
        /// mehr anzeigte. Die Anwendung hatte recht und sah trotzdem kaputt aus: dass jeder
        /// Nachbarschritt 4 Punkte kostet und die Einheit sechs Felder hinter sich hat, stand
        /// nirgends.
        /// </summary>
        [StaFact]
        public void EineErschoepfteEinheitNenntDieZahlen() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var spur = new Bewegungsspur(figur);

            int bpVorher = figur.bp;
            int schrittVorher = figur.schritt;
            int spurVorher = spur.Count;
            try {
                // ein Restpunkt - damit ist kein Schritt mehr zu bezahlen, aber es ist auch nicht
                // der eigene Fall "gar keine Punkte mehr"
                figur.bp = 1;
                if (spurVorher == 0)
                    spur.Add(KleinfeldView.GetKleinfeld(figur)!);
                Assert.Empty(BewegungsRules.GetErreichbareFelder(figur));

                var grund = BewegungsRules.ErkläreWarumNichtsErreichbar(figur);
                Assert.True(grund.HasErrors);
                // die Ueberschrift nennt den Stand
                Assert.Contains($"1 von {figur.bp_max}", grund.Title);
                // und der Text, woran es scheitert und was schon gelaufen ist
                Assert.Contains("kostet", grund.Message);
                Assert.Contains("In diesem Zug", grund.Message);
                // nicht die Behauptung, es gaebe ueberhaupt keine Punkte mehr
                Assert.DoesNotContain("keine Bewegungspunkte", grund.Title);
            }
            finally {
                figur.bp = bpVorher;
                while (new Bewegungsspur(figur).Count > spurVorher)
                    new Bewegungsspur(figur).RemoveLast();
                figur.schritt = schrittVorher;
            }
        }

        /// <summary>
        /// Ganz ohne Punkte bleibt es bei der eigenen, kuerzeren Auskunft.
        /// </summary>
        [StaFact]
        public void OhneJedenPunktBleibtEsBeiDerKurzenAuskunft() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();

            int bpVorher = figur.bp;
            try {
                figur.bp = 0;
                var grund = BewegungsRules.ErkläreWarumNichtsErreichbar(figur);
                Assert.Contains("keine Bewegungspunkte mehr", grund.Title);
                Assert.Contains($"{figur.bp_max} Bewegungspunkten", grund.Message);
            }
            finally {
                figur.bp = bpVorher;
            }
        }

        /// <summary>Ohne Figur und ohne Feld gibt es trotzdem eine Antwort, keine Ausnahme.</summary>
        [Fact]
        public void OhneFigurOderFeldGibtEsEineAntwort() {
            Assert.Single(BewegungsRules.ErkläreUnerreichbarkeit(null, null));
        }

        /// <summary>
        /// Es sollen alle Gruende kommen, nicht nur der erste.
        ///
        /// Ein Feld ein Stueck abseits scheitert meist an mehreren Stellen zugleich: ueber den
        /// einen Nachbarn fehlen die Hoehenstufen, ueber den naechsten die Bewegungspunkte, und
        /// die uebrigen sind selbst nicht erreichbar. Genau das ist der Sinn der Sache - die
        /// Wegsuche verwirft sonst still den ersten Schritt und schweigt zum Rest.
        /// </summary>
        [StaFact]
        public void EinFeldKannAusMehrerenGruendenZugleichAusfallen() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var erreichbar = BewegungsRules.GetErreichbareFelder(figur);
            var erreichbareNamen = erreichbar.Select(feld => feld.Bezeichner).ToHashSet();
            // Das eigene Feld gehoert nicht dazu, es sei denn ein Rundweg fuehrt zurueck - dort
            // steht die Figur ja schon, und erklaert werden muss nichts.
            string eigenes = KleinfeldView.GetKleinfeld(figur)!.Bezeichner;

            // die Nachbarn der erreichbaren Felder, die selbst nicht mehr dazugehoeren - dort
            // liegt der Rand, und dort treffen mehrere Gruende aufeinander
            int mitMehrerenGründen = 0;
            foreach (var feld in erreichbar) {
                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(feld, richtung));
                    if (nachbar == null || erreichbareNamen.Contains(nachbar.Bezeichner))
                        continue;
                    if (nachbar.Bezeichner == eigenes)
                        continue;
                    var gründe = BewegungsRules.ErkläreUnerreichbarkeit(figur, nachbar);
                    Assert.NotEmpty(gründe);
                    // kein Grund darf doppelt vorkommen
                    Assert.Equal(gründe.Count, gründe.Distinct().Count());
                    if (gründe.Count > 1)
                        mitMehrerenGründen++;
                }
            }

            Assert.True(mitMehrerenGründen > 0,
                "Kein einziges Feld nennt mehr als einen Grund - dann faellt die Erklaerung zu frueh ab");
        }

        /// <summary>
        /// Auf dem eigenen Feld steht die Figur bereits - dazu gibt es nichts zu erklaeren.
        ///
        /// Es faellt sonst durchs Raster: in der Liste der erreichbaren Felder taucht es nur auf,
        /// wenn ein Rundweg zurueckfuehrt. Ohne Ausnahme haette der Hinweis dort behauptet, das
        /// Feld sei unerreichbar - direkt unter der Figur, die daraufsteht.
        /// </summary>
        [StaFact]
        public void DasEigeneFeldWirdNichtErklaert() {
            LadeAlles();
            var figur = FindeBeweglicheFigur();
            var eigenes = KleinfeldView.GetKleinfeld(figur);
            Assert.True(eigenes != null);

            Assert.Empty(BewegungsRules.ErkläreUnerreichbarkeit(figur, eigenes));
        }
    }
}
