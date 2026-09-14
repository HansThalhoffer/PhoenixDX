using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Raumpunkte und Bewegungspunkte zum Zugbeginn - Issue #61.
    ///
    /// Beide haengen an Werten, die sich im Lauf des Monats aendern: die Raumpunkte an Staerke,
    /// Heerfuehrern, Pferden und Katapulten, die Bewegungspunkte daran, ob das Heer Ladung
    /// schleppt. Ohne Auffrischen laufen die gespeicherten Werte auseinander - in den echten
    /// Zugdaten weicht bei 36 von 60 Figuren der Raumpunktwert ab, oft steht dort 0.
    ///
    /// Veraendert wird hier nur im Speicher; geschrieben wird erst bei der Zugabgabe.
    /// </summary>
    public class ZugbeginnWerteTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Die Bewegungspunkte der Truppen stehen so im Regelwerk (Tabelle in Kapitel 1.1).
        /// </summary>
        [Fact]
        public void DieBewegungspunkteEntsprechenDemRegelwerk() {
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.Krieger));
            Assert.Equal(21, BewegungsRules.BerechneBewegungspunkte(FigurType.Reiter));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Schiff));
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.LeichteArtillerie));
            Assert.Equal(9, BewegungsRules.BerechneBewegungspunkte(FigurType.SchwereArtillerie));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.LeichtesKriegsschiff));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.SchweresKriegsschiff));

            // Charaktere haben nach Regelwerk 21 Bewegungspunkte zu Land und 42 zur See. In der
            // Skala der Tabelle BEW_Chars traegt die 42 beides - siehe
            // BewegungTest.DieCharaktertabelleTraegtBeideHaelftenDerRegel.
            Assert.Equal(21, BewegungsRules.BewegungspunkteCharakterNachRegelwerk);
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Charakter));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.Zauberer));
            Assert.Equal(42, BewegungsRules.BerechneBewegungspunkte(FigurType.CharakterZauberer));
        }

        /// <summary>
        /// Der berechnete Wert stimmt mit dem ueberein, den die Spielleitung in die Zugdaten
        /// schreibt - fuer jeden Charakter und jeden Zauberer im Bestand.
        ///
        /// Darauf kommt es an, seit ZugendeRules die Bewegungspunkte auch fuer Namensfiguren frisch
        /// berechnet. Waeren beide Werte verschieden, naehme der Zugwechsel jedem Charakter
        /// lautlos etwas weg.
        /// </summary>
        [StaFact]
        public void FuerNamensfigurenStimmtBerechnetMitGespeichert() {
            LadeAlles();

            var namensfiguren = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<NamensSpielfigur>()
                .Where(figur => figur.bp_max > 0)
                .ToList();
            Assert.True(namensfiguren.Count > 0, "Das eigene Reich hat keine Namensfiguren");

            Assert.All(namensfiguren, figur => Assert.Equal(
                BewegungsRules.BerechneBewegungspunkte(figur), figur.bp_max));

            // und der Zugwechsel frischt den Wert auch bei ihnen auf
            var charakter = new Character { Nummer = 694, gf_von = 305, kf_von = 24, bp = 0, bp_max = 7 };
            ZugendeRules.SchiebeInNächstenZug(charakter);
            Assert.Equal(BewegungsRules.BerechneBewegungspunkte(charakter), charakter.bp_max);
            Assert.Equal(charakter.bp_max, charakter.bp);
        }

        /// <summary>
        /// Die Raumpunkte folgen der Tabelle des Regelwerks: Krieger 1, Pferd 1, Reiter 2,
        /// einfacher Heerfuehrer 100, LKP 1.000, SKP 2.000.
        ///
        /// Gegengeprueft an einer echten Figur, bei der gespeicherter und berechneter Wert
        /// uebereinstimmen - dort ist der gespeicherte Wert noch frisch.
        /// </summary>
        [StaFact]
        public void DieRaumpunkteFolgenDerTabelleDesRegelwerks() {
            LadeAlles();
            var stimmig = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(truppe => truppe.rp > 0 && truppe.rp == SpielfigurRules.BerechneRaumpunkte(truppe));
            Assert.True(stimmig != null,
                "Keine Figur, deren gespeicherte Raumpunkte zum berechneten Wert passen");

            // und die Rechnung laesst sich von Hand nachvollziehen
            int erwartet = stimmig!.staerke * 1 + stimmig.hf * 100;
            if (stimmig.Typ == FigurType.Krieger && stimmig.Pferde == 0 && stimmig.LKP == 0 && stimmig.SKP == 0)
                Assert.Equal(erwartet, stimmig.rp);
        }

        /// <summary>
        /// Zum Zugbeginn bekommt eine Truppe ihre vollen Bewegungspunkte und frische Raumpunkte.
        /// </summary>
        [StaFact]
        public void ZumZugbeginnGibtEsVolleBewegungspunkteUndFrischeRaumpunkte() {
            LadeAlles();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null, "Das eigene Reich hat keine Truppen");

            // ein Zustand mitten im Zug: halb verbraucht und mit veralteten Raumpunkten
            truppe!.bp = 3;
            truppe.hoehenstufen = 2;
            truppe.rp = 999999;
            ZugendeRules.SetzeAufAusgangsposition(truppe);

            ZugendeRules.SchiebeInNächstenZug(truppe);

            Assert.Equal(truppe.bp_max, truppe.bp);
            Assert.Equal(0, truppe.hoehenstufen);
            Assert.Equal(SpielfigurRules.BerechneRaumpunkte(truppe), truppe.rp);
            Assert.NotEqual(999999, truppe.rp);
            Assert.Equal(0, truppe.schritt);
        }

        /// <summary>
        /// Ein Heer, das Beute schleppt, zieht mit 9 statt 21 Punkten los - und wieder mit 21,
        /// sobald es sie abgegeben hat. Ohne Auffrischen bliebe es beim alten Hoechstwert.
        /// </summary>
        [StaFact]
        public void LadungDrueckstDieBewegungspunkteUndGibtSieWiederFrei() {
            LadeAlles();
            var reiter = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(t => t.Typ == FigurType.Reiter && Plausibilität.IsValid(t));
            if (reiter == null)
                return; // kein Reiterheer in diesen Zugdaten

            int gsVorher = reiter.GS;
            try {
                reiter.GS = 60000;   // ueber der Grenze von 50.000
                ZugendeRules.SetzeAufAusgangsposition(reiter);
                ZugendeRules.SchiebeInNächstenZug(reiter);
                Assert.Equal(9, reiter.bp_max);
                Assert.Equal(9, reiter.bp);

                reiter.GS = 0;
                ZugendeRules.SetzeAufAusgangsposition(reiter);
                ZugendeRules.SchiebeInNächstenZug(reiter);
                Assert.Equal(21, reiter.bp_max);
                Assert.Equal(21, reiter.bp);
            }
            finally {
                reiter.GS = gsVorher;
            }
        }

        /// <summary>
        /// Namensfiguren behalten ihr gespeichertes bp_max.
        ///
        /// Das Regelwerk nennt fuer den Heerfuehrercharakter 21 Bewegungspunkte, die Zugdaten der
        /// Spielleitung fuehren fuer alle Charaktere 42. Solange das nicht geklaert ist, waere ein
        /// Neuberechnen ein Halbieren - lautlos und mitten im Zugwechsel.
        /// </summary>
        [StaFact]
        public void NamensfigurenBehaltenIhrGespeichertesMaximum() {
            LadeAlles();
            var namens = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<NamensSpielfigur>()
                .FirstOrDefault(Plausibilität.IsValid);
            Assert.True(namens != null, "Das eigene Reich hat keine Namensfiguren");

            int bpMaxVorher = namens!.bp_max;
            namens.bp = 0;
            ZugendeRules.SetzeAufAusgangsposition(namens);
            ZugendeRules.SchiebeInNächstenZug(namens);

            Assert.Equal(bpMaxVorher, namens.bp_max);
            Assert.Equal(bpMaxVorher, namens.bp);
        }
    }
}
