using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Tests für die Bewegungsregeln, die ohne geladene Datenbanken auskommen.
    /// </summary>
    public class BewegungTest {

        /// <summary>
        /// Die Gegenrichtung muss zur Nachbarschaftsberechnung der Karte passen:
        /// wer von A aus in Richtung X nach B geht, kommt von B aus in der Gegenrichtung wieder nach A.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(24)]
        [InlineData(40)]
        public void GegenrichtungPasstZurNachbarschaft(int kf) {
            var start = new KleinfeldPosition(305, kf);
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KartenKoordinaten.GetNachbar(start, richtung);
                Assert.NotNull(nachbar);
                var zurück = KartenKoordinaten.GetNachbar(nachbar, BewegungsRules.Gegenrichtung(richtung));
                Assert.NotNull(zurück);
                Assert.Equal(start.gf, zurück.gf);
                Assert.Equal(start.kf, zurück.kf);
            }
        }

        /// <summary>
        /// GetRichtung muss für jeden Nachbarn genau die Richtung liefern, über die er erreicht wurde
        /// </summary>
        [Fact]
        public void GetRichtungFindetJedenNachbarn() {
            var start = new KleinfeldPosition(305, 24);
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KartenKoordinaten.GetNachbar(start, richtung);
                Assert.NotNull(nachbar);
                Assert.Equal(richtung, BewegungsRules.GetRichtung(start, nachbar));
            }
        }

        /// <summary>
        /// Ein Feld, das kein Nachbar ist, darf keine Richtung liefern
        /// </summary>
        [Fact]
        public void GetRichtungLiefertNullFuerEntfernteFelder() {
            var start = new KleinfeldPosition(305, 24);
            Assert.Null(BewegungsRules.GetRichtung(start, start));
            Assert.Null(BewegungsRules.GetRichtung(start, new KleinfeldPosition(901, 3)));
        }

        /// <summary>
        /// Die Bewegungsart bestimmt, welche BEW_* Tabelle gelesen wird.
        /// Katapulte und Kriegsschiffe verlangsamen bzw. verändern die Einheit.
        /// </summary>
        [Fact]
        public void BewegungsartFolgtDerAusruestung() {
            var krieger = new Krieger { Nummer = 101, staerke = 100, hf = 1 };
            Assert.Equal(BewegungsArt.Krieger, BewegungsRules.GetBewegungsArt(krieger));

            krieger.LKP = 1;
            Assert.Equal(BewegungsArt.LKP, BewegungsRules.GetBewegungsArt(krieger));

            krieger.SKP = 1;
            Assert.Equal(BewegungsArt.SKP, BewegungsRules.GetBewegungsArt(krieger));

            var reiter = new Reiter { Nummer = 201, staerke = 50, hf = 1 };
            Assert.Equal(BewegungsArt.Reiter, BewegungsRules.GetBewegungsArt(reiter));

            var schiffe = new Schiffe { Nummer = 301, staerke = 10, hf = 1 };
            Assert.Equal(BewegungsArt.Schiffe, BewegungsRules.GetBewegungsArt(schiffe));

            schiffe.LKP = 1;
            Assert.Equal(BewegungsArt.LKS, BewegungsRules.GetBewegungsArt(schiffe));

            // schwere Kriegsschiffe bewegen sich nach BEW_SKS und nicht nach BEW_LKS
            schiffe.SKP = 1;
            Assert.Equal(BewegungsArt.SKS, BewegungsRules.GetBewegungsArt(schiffe));
        }

        /// <summary>
        /// Bewegungspunkte nach Regelwerk Kapitel 1
        /// </summary>
        [Fact]
        public void BewegungspunkteNachRegelwerk() {
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.Krieger));
            Assert.Equal(21, BewegungsRules.BerechneBewegungspunkte(FigurType.Reiter));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Schiff));
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.LeichteArtillerie));
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.SchwereArtillerie));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.SchweresKriegsschiff));

            // Alle drei Charaktertypen gleich. Die Zahl heisst 42 und meint die 21 des
            // Regelwerks - warum, steht im naechsten Test.
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Charakter));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Zauberer));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.CharakterZauberer));
            Assert.Equal(21, BewegungsRules.BewegungspunkteCharakterNachRegelwerk);
        }

        /// <summary>
        /// Die Bewegungstabelle der Charaktere traegt beide Haelften der Regel: 21 Punkte zu Land,
        /// 42 zur See (Entscheidung der Spielleitung, September 2026).
        ///
        /// Sie schafft das mit einer einzigen Zahl, weil BEW_Chars die Landkosten verdoppelt und
        /// die Wasserkosten auf Schiffsniveau laesst. Geprueft wird deshalb nicht die Zahl, sondern
        /// die Reichweite: soweit wie ein Reiter zu Land, soweit wie ein Schiff zur See.
        ///
        /// "Charaktere verfuegen ueber eigene Schiffe" (Regelwerk 0.3.2 und 1.9 in der Fassung der
        /// Korrektur 22 vom Treffen am 21.06.2014, die ausdruecklich auf diese Tabelle verweist).
        /// </summary>
        [StaFact]
        public void DieCharaktertabelleTraegtBeideHaelftenDerRegel() {
            TestSetup.LadeMitDiplomatie();

            int Kosten(BewegungsArt art, TerrainType gelaende)
                => BewegungsRules.GetBewegungsdaten(art, (int)gelaende)?.standart ?? -1;

            // zu Land genau das Doppelte der Reiterkosten
            Assert.Equal(2 * Kosten(BewegungsArt.Reiter, TerrainType.Tiefland),
                             Kosten(BewegungsArt.Chars, TerrainType.Tiefland));
            Assert.Equal(2 * Kosten(BewegungsArt.Reiter, TerrainType.Wald),
                             Kosten(BewegungsArt.Chars, TerrainType.Wald));
            Assert.Equal(2 * Kosten(BewegungsArt.Reiter, TerrainType.Bergland),
                             Kosten(BewegungsArt.Chars, TerrainType.Bergland));

            // zu Wasser dagegen die Kosten eines Schiffes
            Assert.Equal(Kosten(BewegungsArt.Schiffe, TerrainType.Wasser),
                         Kosten(BewegungsArt.Chars, TerrainType.Wasser));
            Assert.Equal(Kosten(BewegungsArt.Schiffe, TerrainType.Tiefsee),
                         Kosten(BewegungsArt.Chars, TerrainType.Tiefsee));

            // und damit die Reichweite: drei Felder Tiefland wie ein Reiter mit 21 ...
            int charakter = BewegungsRules.BerechneBewegungspunkte(FigurType.Charakter);
            int reiter = BewegungsRules.BerechneBewegungspunkte(FigurType.Reiter);
            Assert.Equal(reiter / Kosten(BewegungsArt.Reiter, TerrainType.Tiefland),
                      charakter / Kosten(BewegungsArt.Chars, TerrainType.Tiefland));
            Assert.Equal(3, charakter / Kosten(BewegungsArt.Chars, TerrainType.Tiefland));

            // ... und sechs Felder Wasser wie ein Schiff mit 42
            int schiff = BewegungsRules.BerechneBewegungspunkte(FigurType.Schiff);
            Assert.Equal(schiff / Kosten(BewegungsArt.Schiffe, TerrainType.Wasser),
                      charakter / Kosten(BewegungsArt.Chars, TerrainType.Wasser));
            Assert.Equal(6, charakter / Kosten(BewegungsArt.Chars, TerrainType.Wasser));

            // ein Reiter kommt dort ueberhaupt nicht hin - 99 heisst unpassierbar
            Assert.Equal(99, Kosten(BewegungsArt.Reiter, TerrainType.Wasser));

            Assert.True(BewegungsRules.IstCharakter(FigurType.Charakter));
            Assert.True(BewegungsRules.IstCharakter(FigurType.Zauberer));
            Assert.True(BewegungsRules.IstCharakter(FigurType.CharakterZauberer));
            Assert.False(BewegungsRules.IstCharakter(FigurType.Reiter));
        }

        /// <summary>
        /// Ein Landheer mit mehr als 50.000 GS transportiert und hat nur noch 9 BP (Regelwerk 4.3)
        /// </summary>
        [Fact]
        public void TransportierendesLandheerIstLangsam() {
            var reiter = new Reiter { Nummer = 201, staerke = 50, hf = 1 };
            Assert.Equal(21, BewegungsRules.BerechneBewegungspunkte(reiter));

            reiter.GS = 50000;
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(reiter));
        }

        /// <summary>
        /// Die Bewegungsspur schreibt die Wegpunkte in die Felder x1/y1 bis x19/y19
        /// </summary>
        [Fact]
        public void BewegungsspurSchreibtWegpunkte() {
            var krieger = new Krieger { Nummer = 101, gf_von = 305, kf_von = 24 };
            var spur = new Bewegungsspur(krieger);

            Assert.Equal(0, spur.Count);

            spur.Add(new KleinfeldPosition(305, 25));
            spur.Add(new KleinfeldPosition(305, 26));

            Assert.Equal(2, spur.Count);
            Assert.Equal(2, krieger.schritt);
            Assert.Equal(305, krieger.x1);
            Assert.Equal(25, krieger.y1);
            Assert.Equal(305, krieger.x2);
            Assert.Equal(26, krieger.y2);

            var entfernt = spur.RemoveLast();
            Assert.NotNull(entfernt);
            Assert.Equal(26, entfernt.kf);
            Assert.Equal(1, spur.Count);
            Assert.Equal(1, krieger.schritt);
            Assert.Equal(0, krieger.x2);

            spur.Clear();
            Assert.Equal(0, spur.Count);
            Assert.Equal(0, krieger.schritt);
            Assert.Equal(0, krieger.x1);
        }

        /// <summary>
        /// Charaktere und Zauberer können in der Datenbank nur 9 Wegpunkte speichern
        /// </summary>
        [Fact]
        public void NamensfigurenHabenWenigerWegpunkte() {
            var zauberer = new Zauberer { Nummer = 501 };
            Assert.Equal(Bewegungsspur.MaxWegpunkteNamensfigur, new Bewegungsspur(zauberer).MaxWegpunkte);

            var krieger = new Krieger { Nummer = 101 };
            Assert.Equal(Bewegungsspur.MaxWegpunkteTruppe, new Bewegungsspur(krieger).MaxWegpunkte);
        }

        /// <summary>
        /// Der gesicherte Zustand muss die Figur exakt wiederherstellen - das ist die Grundlage
        /// dafür, dass ein Undo einer Bewegung keine Truppen verdoppelt oder verliert.
        /// </summary>
        [Fact]
        public void BewegungsZustandStelltFigurExaktWiederHer() {
            var krieger = new Krieger {
                Nummer = 101,
                gf_von = 305,
                kf_von = 24,
                bp = 9,
                bp_max = 9,
                staerke = 100,
                hf = 1,
            };

            var zustand = BewegungsZustand.Sichern(krieger);

            // Bewegung simulieren
            krieger.gf_nach = 305;
            krieger.kf_nach = 25;
            krieger.bp = 2;
            krieger.hoehenstufen = 2;
            krieger.ph_xy = "irgendwas";
            krieger.Befehl_erobert = "E:305/25;";
            new Bewegungsspur(krieger).Add(new KleinfeldPosition(305, 25));

            Assert.Equal(25, krieger.kf);

            zustand.Wiederherstellen();

            Assert.Equal(0, krieger.gf_nach);
            Assert.Equal(0, krieger.kf_nach);
            Assert.Equal(9, krieger.bp);
            Assert.Equal(0, krieger.hoehenstufen);
            Assert.Equal(0, krieger.schritt);
            Assert.Equal(0, krieger.x1);
            Assert.Equal(string.Empty, krieger.Befehl_erobert);
            // die Figur steht wieder auf dem Ausgangsfeld
            Assert.Equal(305, krieger.gf);
            Assert.Equal(24, krieger.kf);
            // Stärke und Heerführer dürfen durch ein Undo nicht verändert werden
            Assert.Equal(100, krieger.staerke);
            Assert.Equal(1, krieger.hf);
        }

        /// <summary>
        /// Die Route ist die Textfassung der Bewegungsspur und wird in den Eigenschaften angezeigt
        /// </summary>
        [Fact]
        public void RouteBeschreibtDenZurueckgelegtenWeg() {
            var krieger = new Krieger { Nummer = 101, gf_von = 305, kf_von = 24, bp = 9, bp_max = 9 };
            Assert.Equal("steht auf 305/24", krieger.Route);
            Assert.Equal("9 von 9", krieger.Bewegungspunkte);

            var spur = new Bewegungsspur(krieger);
            spur.Add(new KleinfeldPosition(305, 25));
            spur.Add(new KleinfeldPosition(305, 26));
            krieger.bp = 2;

            Assert.Equal("305/24 → 305/25 → 305/26", krieger.Route);
            Assert.Equal("2 von 9", krieger.Bewegungspunkte);
        }

        /// <summary>
        /// Der erzeugte Befehl muss sich auch wieder einlesen lassen
        /// </summary>
        [Fact]
        public void ErzeugterBewegungsbefehlIstWiederLesbar() {
            var krieger = new Krieger { Nummer = 101, gf_von = 305, kf_von = 24, LKP = 1 };
            string befehl = MoveCommandParser.GenerateCommand(krieger, new KleinfeldPosition(305, 25));

            var parser = new MoveCommandParser();
            Assert.True(parser.ParseCommand(befehl, out var command));
            var move = Assert.IsType<MoveCommand>(command);

            Assert.Equal(FigurType.LeichteArtillerie, move.Figur);
            Assert.Equal(101, move.UnitId);
            Assert.NotNull(move.FromLocation);
            Assert.Equal(305, move.FromLocation.gf);
            Assert.Equal(24, move.FromLocation.kf);
            Assert.NotNull(move.ToLocation);
            Assert.Equal(25, move.ToLocation.kf);
        }

        /// <summary>
        /// Auch ein mehrschrittiger Befehl muss wieder lesbar sein
        /// </summary>
        [Fact]
        public void MehrschrittigerBewegungsbefehlIstWiederLesbar() {
            var reiter = new Reiter { Nummer = 205, gf_von = 305, kf_von = 24 };
            List<KleinfeldPosition> weg = [
                new KleinfeldPosition(305, 25),
                new KleinfeldPosition(305, 26),
                new KleinfeldPosition(305, 27),
            ];
            string befehl = MoveCommandParser.GenerateCommand(reiter, weg);

            var parser = new MoveCommandParser();
            Assert.True(parser.ParseCommand(befehl, out var command));
            var move = Assert.IsType<MoveCommand>(command);

            Assert.Equal(FigurType.Reiter, move.Figur);
            Assert.Equal(205, move.UnitId);
            Assert.NotNull(move.ToLocation);
            Assert.Equal(27, move.ToLocation.kf);
            Assert.NotNull(move.ViaLocations);
            Assert.Equal(2, move.ViaLocations.Count);
            Assert.Equal(3, move.GetWegpunkte().Count);
        }
    }
}
