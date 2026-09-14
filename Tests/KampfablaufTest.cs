using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Klammer um den Kampf: ein Durchlauf ueber eine Gemark (Regelwerk Kapitel 5).
    ///
    /// "Dann wird der Fernkampf, anschliessend der ( ggf. Ritterkampf, dann ) Charakterkampf und
    /// dann der Nahkampf ausgewuerfelt und berechnet."
    ///
    /// Die einzelnen Schritte pruefen ihre eigenen Tests. Hier geht es um ihr Zusammenspiel: die
    /// Reihenfolge, die Rechenkopien und das Fortschreiben.
    /// </summary>
    public class KampfablaufTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        private static KampfablaufRules.Kämpfender Heer(int nummer, int staerke, int hf = 0,
                double gutpunkte = 0, int lkp = 0) {
            return new(new Krieger { Nummer = nummer, staerke = staerke, hf = hf, LKP = lkp }, gutpunkte);
        }

        /// <summary>
        /// Der Beschuss wirkt vor dem Nahkampf: was er wegnimmt, kaempft nicht mehr mit.
        /// </summary>
        [Fact]
        public void DerBeschussWirktVorDemNahkampf() {
            var angreifer = new KampfablaufRules.Seite([Heer(100, 2000)]);
            var verteidiger = new KampfablaufRules.Seite([Heer(200, 2000)]);

            // ohne Beschuss stehen beide gleich: unentschieden
            var ohne = KampfablaufRules.WerteSchlachtAus(null, angreifer, verteidiger,
                KampfablaufRules.Würfe.Keine);
            Assert.Equal(NahkampfRules.Ausgang.Unentschieden, ohne.Nahkampf.Sieger);

            // der Angreifer beschiesst den Verteidiger: 100 BP kosten ihn 1000 Krieger
            var mit = KampfablaufRules.WerteSchlachtAus(null, angreifer, verteidiger,
                new KampfablaufRules.Würfe(TrefferpunkteGegenVerteidiger: 100));
            Assert.Equal(1000, mit.BeschussGegenVerteidiger.Verluste[0].Krieger);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, mit.Nahkampf.Sieger);
        }

        /// <summary>
        /// Gerechnet wird auf Kopien: die Figuren des Spielers bleiben unberuehrt, bis jemand den
        /// Bericht fortschreibt.
        /// </summary>
        [Fact]
        public void DieFigurenBleibenUnberuehrtBisManSieFortschreibt() {
            var angreifer = new KampfablaufRules.Seite([Heer(100, 4000, hf: 10)]);
            var verteidiger = new KampfablaufRules.Seite([Heer(200, 1000, hf: 5)]);

            var siegerHeer = angreifer.Heere[0].Heer;
            var verliererHeer = verteidiger.Heere[0].Heer;

            var bericht = KampfablaufRules.WerteSchlachtAus(null, angreifer, verteidiger,
                new KampfablaufRules.Würfe(TrefferpunkteGegenVerteidiger: 50));

            // nichts hat sich geaendert
            Assert.Equal(4000, siegerHeer.staerke);
            Assert.Equal(1000, verliererHeer.staerke);
            Assert.Equal(10, siegerHeer.hf);

            // der Bericht spricht von den Figuren des Spielers, nicht von den Kopien
            Assert.Same(siegerHeer, bericht.Nahkampf.Angreifer[0].Kämpfer.Heer);
            Assert.Same(verliererHeer, bericht.Nahkampf.Verteidiger[0].Kämpfer.Heer);

            // erst das Fortschreiben zieht ab
            int verändert = KampfablaufRules.Übernimm(bericht);
            Assert.True(verändert > 0);
            Assert.True(siegerHeer.staerke < 4000, "Der Sieger hat keine Verluste erlitten");
            Assert.Equal(0, verliererHeer.staerke);
            Assert.Equal(0, verliererHeer.hf);

            Assert.Equal(0, KampfablaufRules.Übernimm(null));
        }

        /// <summary>
        /// Die Gutpunkte der Heerfuehrer werden nach dem Beschuss gezaehlt: wer gefallen ist,
        /// bringt keine mehr ein (Kampftabelle C146 liest die Zeile nach dem Katabeschuss).
        /// </summary>
        [Fact]
        public void GefalleneHeerfuehrerBringenKeineGutpunkteMehr() {
            // ein Heer mit 100 Heerfuehrern - das sind 100 Gutpunkte
            var mitAllen = KampfablaufRules.WerteSchlachtAus(null,
                new KampfablaufRules.Seite([Heer(100, 1000, hf: 100)]),
                new KampfablaufRules.Seite([Heer(200, 1000)]),
                KampfablaufRules.Würfe.Keine);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, mitAllen.Nahkampf.Sieger);

            // derselbe Kampf, aber die Haelfte des Heeres faellt vorher unter Beschuss -
            // anteilig auch die Haelfte der Heerfuehrer
            var nachBeschuss = KampfablaufRules.WerteSchlachtAus(null,
                new KampfablaufRules.Seite([Heer(100, 1000, hf: 100)]),
                new KampfablaufRules.Seite([Heer(200, 1000)]),
                new KampfablaufRules.Würfe(TrefferpunkteGegenAngreifer: 50));

            Assert.Equal(50, nachBeschuss.BeschussGegenAngreifer.Verluste[0].Heerführer);
            // die Kampfstaerke des Angreifers ist dadurch gesunken
            Assert.True(nachBeschuss.Nahkampf.Angreifer[0].Kämpfer.Kampfstärke
                      < mitAllen.Nahkampf.Angreifer[0].Kämpfer.Kampfstärke,
                "Die gefallenen Heerfuehrer haben die Kampfstaerke nicht gemindert");
        }

        /// <summary>
        /// Beute und Kampfeinnahmen stehen im Bericht und gehen beim Fortschreiben an ein Heer,
        /// das noch steht.
        /// </summary>
        [StaFact]
        public void BeuteUndKampfeinnahmenGehenAnDenSieger() {
            LadeAlles();

            var sieger = Heer(100, 4000);
            var verlierer = Heer(200, 1000);
            // der Verlierer fuehrt Ladung mit
            verlierer.Heer.GS = 3000;
            verlierer.Heer.Kampfeinnahmen = 2000;

            var bericht = KampfablaufRules.WerteSchlachtAus(null,
                new KampfablaufRules.Seite([sieger]), new KampfablaufRules.Seite([verlierer]),
                KampfablaufRules.Würfe.Keine);

            Assert.Equal(NahkampfRules.Ausgang.Angreifer, bericht.Nahkampf.Sieger);
            Assert.Equal(3000, bericht.Ladung.Gold);
            Assert.Equal(2000, bericht.Ladung.Kampfeinnahmen);
            Assert.True(bericht.Kampfeinnahmen > 0, "Es gab keine Kampfeinnahmen");

            int goldVorher = sieger.Heer.GS;
            int einnahmenVorher = sieger.Heer.Kampfeinnahmen;
            KampfablaufRules.Übernimm(bericht);

            // die Art der Ladung bleibt erhalten - Gold bleibt Gold
            Assert.Equal(goldVorher + 3000, sieger.Heer.GS);
            Assert.Equal(einnahmenVorher + 2000 + bericht.Kampfeinnahmen, sieger.Heer.Kampfeinnahmen);
        }

        /// <summary>
        /// Wer einen Ruestort erobert, bekommt die Ruestortbeute - als besondere Einnahme
        /// (Regelwerk 5.8.1).
        /// </summary>
        [StaFact]
        public void WerDenRuestortNimmtBekommtDieBeute() {
            LadeAlles();

            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                BeuteRules.GetRüstortbeute(kf) > 0);
            Assert.True(gemark != null, "Auf der Karte gibt es keinen Ruestort mit Beute");

            var sieger = Heer(100, 4000);
            var bericht = KampfablaufRules.WerteSchlachtAus(gemark,
                new KampfablaufRules.Seite([sieger]),
                new KampfablaufRules.Seite([Heer(200, 1000)]),
                KampfablaufRules.Würfe.Keine);

            Assert.True(bericht.GemarkErobert);
            Assert.Equal(BeuteRules.GetRüstortbeute(gemark), bericht.Rüstortbeute);
            Assert.Equal(gemark!.gf, bericht.Gemark.gf);

            int vorher = sieger.Heer.Kampfeinnahmen;
            KampfablaufRules.Übernimm(bericht);
            Assert.Equal(vorher + bericht.Rüstortbeute + bericht.Kampfeinnahmen, sieger.Heer.Kampfeinnahmen);

            // verliert der Angreifer, gibt es keine Ruestortbeute
            var verloren = KampfablaufRules.WerteSchlachtAus(gemark,
                new KampfablaufRules.Seite([Heer(100, 1000)]),
                new KampfablaufRules.Seite([Heer(200, 4000)]),
                KampfablaufRules.Würfe.Keine);
            Assert.False(verloren.GemarkErobert);
            Assert.Equal(0, verloren.Rüstortbeute);
        }

        /// <summary>
        /// Der Bericht laesst sich lesen - er ist das, was die Spielleitung in der Hand haelt.
        /// </summary>
        [Fact]
        public void DerBerichtLaesstSichLesen() {
            var bericht = KampfablaufRules.WerteSchlachtAus(null,
                new KampfablaufRules.Seite([Heer(100, 4000)]),
                new KampfablaufRules.Seite([Heer(200, 1000)]),
                new KampfablaufRules.Würfe(TrefferpunkteGegenVerteidiger: 20));

            string text = bericht.AlsText();
            Assert.Contains("Schlacht auf", text);
            Assert.Contains("Beschuss gegen den Verteidiger", text);
            Assert.Contains("Der Angreifer gewinnt", text);
        }

        /// <summary>
        /// Ohne Gegner und ohne Wuerfe faellt der Durchlauf nicht auseinander.
        /// </summary>
        [Fact]
        public void OhneAllesPassiertNichts() {
            var leer = KampfablaufRules.WerteSchlachtAus(null, null, null, null);

            Assert.Empty(leer.Nahkampf.Angreifer);
            Assert.Empty(leer.Nahkampf.Verteidiger);
            Assert.Equal(0, leer.Rüstortbeute);
            Assert.Equal(0, leer.Kampfeinnahmen);
            Assert.Equal(BeuteRules.Ladung.Nichts, leer.Ladung);
            Assert.Equal(0, KampfablaufRules.Übernimm(leer));
        }
    }
}
