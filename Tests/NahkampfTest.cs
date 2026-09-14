using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;

namespace Tests {

    /// <summary>
    /// Der Nahkampf von Anfang bis Ende (Regelwerk 5.5, Kampftabelle Zeilen 141 bis 209).
    ///
    /// Die Rechenbausteine prueft KampfTest einzeln. Hier geht es um ihr Zusammenspiel: wer
    /// gewinnt, wer verliert wieviel, und was davon dem Sieger in die Haende faellt.
    /// </summary>
    public class NahkampfTest {

        private static NahkampfRules.Kämpfer Krieger(int staerke, int hf, double gutpunkte, int nummer = 1)
            => new(new Krieger { staerke = staerke, hf = hf, Nummer = nummer }, gutpunkte);

        private static NahkampfRules.Kämpfer Reiter(int staerke, int hf, double gutpunkte, int nummer = 2)
            => new(new Reiter { staerke = staerke, hf = hf, Nummer = nummer }, gutpunkte);

        /// <summary>
        /// Das durchgerechnete Beispiel des Regelwerks 5.5, diesmal als ganze Schlacht.
        ///
        /// Reich A verteidigt aus einer Stadt (200 GP) mit zwei Heeren - 10.000 Kriegern mit
        /// 20 HF und 9.000 Reitern mit 80 HF, beide mit einem Burgherren (24 GP). Angegriffen
        /// wird es von 60.000 Kriegern mit 150 HF.
        ///
        /// Das Regelwerk nennt die Kampfstaerken 22.200, 22.680 und 105.000 - und als Ergebnis
        /// "ReichB_101: hat somit noch 39042 Krieger und 97HF ueber".
        /// </summary>
        [Fact]
        public void DasBeispielDesRegelwerksGehtDurch() {
            // Gutpunkte wie im Beispiel: Burgherr 24 + Heerfuehrer + 200 fuer die Stadt
            var verteidiger = new NahkampfRules.Seite([
                Krieger(10000, 20, 24 + 20 + 200),
                Reiter(9000, 80, 24 + 80 + 200),
            ]);
            var angreifer = new NahkampfRules.Seite([Krieger(60000, 150, 150, nummer: 3)]);

            Assert.Equal(22200, verteidiger.Heere[0].Kampfstärke, 6);
            Assert.Equal(22680, verteidiger.Heere[1].Kampfstärke, 6);
            Assert.Equal(44880, verteidiger.Kampfstärke, 6);
            Assert.Equal(105000, angreifer.Kampfstärke, 6);
            Assert.Equal(19000, verteidiger.Heeresstärke);
            Assert.Equal(136.21, verteidiger.Gutpunktschnitt, 2);

            var ergebnis = NahkampfRules.WerteNahkampfAus(angreifer, verteidiger);

            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
            Assert.False(ergebnis.Überrannt);

            // "Damit verliert ReichB: 13000 Basisverluste * 1,6121 = 20957 Mann."
            var sieger = ergebnis.Angreifer[0];
            Assert.Equal(20957, sieger.Verluste.Heeresstärke, 0);

            // "hat somit noch 39042 Krieger und 97HF ueber" - der letzte Mann haengt an der
            // Rundung: die Kampftabelle schneidet den Rest ab (F201), die Anwendung rundet
            // kaufmaennisch, weil der Prozentwurf der Tabelle hier nicht gewuerfelt wird.
            Assert.InRange(60000 - sieger.Verluste.Krieger, 39042, 39043);
            Assert.InRange(150 - sieger.Verluste.Heerführer, 97, 98);

            // der Verlierer bleibt nicht ungeschoren
            Assert.All(ergebnis.Verteidiger, ausgang => Assert.True(ausgang.Verluste.Heeresstärke > 0));
        }

        /// <summary>
        /// "Diese aufaddierte Gesamtkampfstaerke entscheidet ueber den Ausgang der Schlacht"
        /// (Regelwerk 5.5) - nicht die Heeresstaerke.
        /// </summary>
        [Fact]
        public void DieKampfstaerkeEntscheidetNichtDieHeeresstaerke() {
            var schwachMitVorteil = new NahkampfRules.Seite([Krieger(1000, 0, 300)]);
            var starkOhneVorteil = new NahkampfRules.Seite([Krieger(1500, 0, 0, nummer: 2)]);

            // 1000 * 2,5 = 2500 gegen 1500 * 1 = 1500
            Assert.Equal(2500, schwachMitVorteil.Kampfstärke, 6);
            Assert.Equal(1500, starkOhneVorteil.Kampfstärke, 6);

            var ergebnis = NahkampfRules.WerteNahkampfAus(schwachMitVorteil, starkOhneVorteil);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
        }

        /// <summary>
        /// "Ab einer zehnfachen Ueberlegenheit ( ohne Gutpunkte gerechnet!) gelten unterlegene
        /// Verteidiger als wehrlos (sie verteidigen sich nicht) und koennen ohne Verluste gefangen
        /// genommen oder vernichtet werden." (Regelwerk 5.5)
        /// </summary>
        [Fact]
        public void ZehnfacheUebermachtUeberrenntOhneEigeneVerluste() {
            var übermacht = new NahkampfRules.Seite([Krieger(10001, 100, 0)]);
            // der Wehrlose hat alle Gutpunkte der Welt - sie helfen ihm nicht
            var wehrlos = new NahkampfRules.Seite([Krieger(1000, 50, 500, nummer: 2)]);

            var ergebnis = NahkampfRules.WerteNahkampfAus(übermacht, wehrlos);

            Assert.True(ergebnis.Überrannt);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
            Assert.Equal(0, ergebnis.Angreifer[0].Verluste.Heeresstärke);
            Assert.Equal(0, ergebnis.Angreifer[0].Verluste.Krieger);
            Assert.True(ergebnis.Verteidiger[0].Aufgerieben);
            Assert.Equal(1000, ergebnis.Verteidiger[0].Verluste.Krieger);
            Assert.Equal(50, ergebnis.Verteidiger[0].Verluste.Heerführer);

            // die Kampftabelle prueft beide Richtungen (C171 und N171)
            var andersherum = NahkampfRules.WerteNahkampfAus(wehrlos, übermacht);
            Assert.True(andersherum.Überrannt);
            Assert.Equal(NahkampfRules.Ausgang.Verteidiger, andersherum.Sieger);
            Assert.Equal(0, andersherum.Verteidiger[0].Verluste.Heeresstärke);
            Assert.True(andersherum.Angreifer[0].Aufgerieben);

            // genau das Zehnfache reicht noch nicht
            var knapp = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([Krieger(10000, 0, 0)]),
                new NahkampfRules.Seite([Krieger(1000, 0, 0, nummer: 2)]));
            Assert.False(knapp.Überrannt);
        }

        /// <summary>
        /// Der Groessenbonus gilt nur fuer den Sieger: die Kampftabelle setzt den Faktor beim
        /// Verlierer auf null (C187). Basis sind immer die Heeresstaerken des Gegners (C188).
        /// </summary>
        [Fact]
        public void DerGroessenbonusGiltNurFuerDenSieger() {
            // gleiche Gutpunkte auf beiden Seiten, damit nur die Groesse wirkt
            var sieger = new NahkampfRules.Seite([Krieger(4000, 0, 100)]);
            var verlierer = new NahkampfRules.Seite([Krieger(1000, 0, 0, nummer: 2)]);

            var ergebnis = NahkampfRules.WerteNahkampfAus(sieger, verlierer);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);

            // Sieger: Basis 1000, Faktor -0,4 bei vierfacher Heeresstaerke, also 600 -
            // anschliessend gemindert durch seinen Gutpunktvorsprung
            double mitFaktor = KampfRules.BerechneGesamtverluste(1000, 4000);
            Assert.Equal(600, mitFaktor);
            Assert.True(ergebnis.Angreifer[0].Verluste.Heeresstärke < mitFaktor,
                "Die Gutpunkte des Siegers haben seine Verluste nicht gemindert");

            // Verlierer: Basis 4000 ohne Faktor - er verliert alles, was er hat
            Assert.True(ergebnis.Verteidiger[0].Aufgerieben);
            Assert.Equal(1000, ergebnis.Verteidiger[0].Verluste.Krieger);
        }

        /// <summary>
        /// Bei gleicher Kampfstaerke gibt es keinen Sieger - und dann auch fuer niemanden einen
        /// Groessenbonus. Beide Seiten werden gleich behandelt.
        /// </summary>
        [Fact]
        public void EinUnentschiedenBehandeltBeideSeitenGleich() {
            var eine = new NahkampfRules.Seite([Krieger(1000, 10, 50)]);
            var andere = new NahkampfRules.Seite([Krieger(1000, 10, 50, nummer: 2)]);

            var ergebnis = NahkampfRules.WerteNahkampfAus(eine, andere);

            Assert.Equal(NahkampfRules.Ausgang.Unentschieden, ergebnis.Sieger);
            Assert.Empty(ergebnis.Verlierer);
            // ohne Sieger hat niemand einen Groessenbonus: beide Seiten reiben sich auf
            Assert.True(ergebnis.Angreifer[0].Aufgerieben);
            Assert.True(ergebnis.Verteidiger[0].Aufgerieben);
            Assert.Equal(ergebnis.Angreifer[0].Verluste.Heeresstärke,
                         ergebnis.Verteidiger[0].Verluste.Heeresstärke, 6);
            Assert.Equal(ergebnis.Angreifer[0].Verluste.Krieger,
                         ergebnis.Verteidiger[0].Verluste.Krieger);
        }

        /// <summary>
        /// "Ein Heer verliert durch den Nahkampf proportional zum Verlust an Heeresstaerke auch HF
        /// und Fernkampfwaffen." (Regelwerk 5.5, Kampftabelle Zeilen 201 bis 209)
        /// </summary>
        [Fact]
        public void VerlusteTreffenAlleGattungenEinesHeeres() {
            var heer = new Krieger { staerke = 1000, hf = 20, LKP = 4, SKP = 2, Pferde = 500, Nummer = 1 };

            var verluste = KampfRules.BerechneNahkampfverluste(heer, 500);

            Assert.Equal(500, verluste.Krieger);
            Assert.Equal(10, verluste.Heerführer);
            Assert.Equal(2, verluste.LKP);
            Assert.Equal(1, verluste.SKP);
            Assert.Equal(250, verluste.Pferde);
            Assert.False(verluste.Aufgerieben);

            // mehr als vorhanden geht nicht, und dann ist das Heer aufgerieben
            var alles = KampfRules.BerechneNahkampfverluste(heer, 99999);
            Assert.Equal(1000, alles.Krieger);
            Assert.Equal(20, alles.Heerführer);
            Assert.True(alles.Aufgerieben);
        }

        /// <summary>
        /// Anders als beim Beschuss gibt es im Nahkampf kein Drittel fuer Gardeheere - die
        /// Kampftabelle liest das Kennzeichen (Zeile 199), benutzt es in den Verlustzeilen aber
        /// nicht. Ein Gardeheer bringt stattdessen 300 Gutpunkte mit.
        /// </summary>
        [Fact]
        public void ImNahkampfGibtEsKeinDrittelFuerGardeheere() {
            var garde = new Krieger { staerke = 1000, Garde = true, Nummer = 1 };
            var gewöhnlich = new Krieger { staerke = 1000, Garde = false, Nummer = 2 };

            Assert.Equal(KampfRules.BerechneNahkampfverluste(gewöhnlich, 500).Krieger,
                         KampfRules.BerechneNahkampfverluste(garde, 500).Krieger);

            // beim Beschuss ist es anders - dort zaehlt das Drittel
            Assert.True(KampfRules.BerechneVerluste(garde, 50).Krieger
                      < KampfRules.BerechneVerluste(gewöhnlich, 50).Krieger);
        }

        /// <summary>
        /// "Katapulte nehmen nicht am Nahkampf teil. Sie ergeben sich im Nahkampf also immer
        /// automatisch" (Regelwerk 5.5).
        ///
        /// Wieviel davon beim Sieger ankommt, haengt daran, wie der Kampf ausging: wird ein Heer
        /// im Nahkampf aufgerieben, "werden 50% der noch vorhandenen Katapulte als zerstoert
        /// angesehen". Wer ueberrannt wird, wird dagegen gefangen genommen, und "gefangene tote
        /// Ruestgueter ... koennen in das eigene Heereskontingent uebernommen werden" - dann geht
        /// alles ueber.
        /// </summary>
        [Fact]
        public void DieKatapulteDesVerlierersFallenDemSiegerZu() {
            var mitKatapulten = new NahkampfRules.Kämpfer(
                new Krieger { staerke = 1000, LKP = 4, SKP = 2, Nummer = 2 });

            // ueberrannt und gefangen genommen: alles geht ueber
            var überrannt = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([Krieger(100000, 0, 0)]),
                new NahkampfRules.Seite([mitKatapulten]));
            Assert.True(überrannt.Überrannt);
            Assert.Equal((4, 2), überrannt.Katapultbeute);

            // im Nahkampf aufgerieben: die Haelfte ist zerstoert
            var geschlagen = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([Krieger(2000, 0, 100)]),
                new NahkampfRules.Seite([mitKatapulten with { Gutpunkte = 0 }]));
            Assert.False(geschlagen.Überrannt);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, geschlagen.Sieger);
            Assert.True(geschlagen.Verteidiger[0].Aufgerieben);
            Assert.Equal((2, 1), geschlagen.Katapultbeute);
        }

        /// <summary>
        /// Der Verlierer einer Schlacht wird aufgerieben - nicht als Regel des Regelwerks, sondern
        /// als Folge seiner Rechnung: Basis seiner Verluste ist die volle Heeresstaerke des
        /// Siegers, ohne mindernden Groessenfaktor (Kampftabelle C187 bis C189).
        ///
        /// Im Beispiel des Regelwerks ist es genauso: Reich A mit 19.000 Heeresstaerke bekommt die
        /// 60.000 des Siegers als Basis. Wer das nicht will, muss sich zurueckziehen (5.4).
        /// </summary>
        [Fact]
        public void DerVerliererWirdAufgerieben() {
            // der Verlierer ist sogar deutlich groesser als der Sieger
            var sieger = new NahkampfRules.Seite([Krieger(3000, 0, 900)]);
            var verlierer = new NahkampfRules.Seite([Krieger(10000, 200, 100, nummer: 2)]);
            Assert.True(verlierer.Heeresstärke > sieger.Heeresstärke);

            var ergebnis = NahkampfRules.WerteNahkampfAus(sieger, verlierer);

            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
            Assert.False(ergebnis.Überrannt);
            Assert.True(ergebnis.Verteidiger[0].Aufgerieben);
            Assert.Equal(10000, ergebnis.Verteidiger[0].Verluste.Krieger);
            Assert.Equal(200, ergebnis.Verteidiger[0].Verluste.Heerführer);

            // der Sieger dagegen kommt mit einem Teil davon: Basis 10.000, gemindert durch
            // Groessenfaktor und Gutpunktvorsprung
            Assert.True(ergebnis.Angreifer[0].Verluste.Heeresstärke > 0);
            Assert.False(ergebnis.Angreifer[0].Aufgerieben);
        }

        /// <summary>
        /// Gebannte Truppen kaempfen nicht mit (Regelwerk 1.4.3) - sie zaehlen weder zur
        /// Heeresstaerke noch zur Kampfstaerke.
        /// </summary>
        [Fact]
        public void GebannteTruppenKaempfenNichtMit() {
            var ohne = new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, Nummer = 1 }, 0, 0);
            var mit = new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, Nummer = 1 }, 0, 900);

            Assert.Equal(1000, ohne.Heeresstärke);
            Assert.Equal(100, mit.Heeresstärke);

            var ergebnis = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([new NahkampfRules.Kämpfer(new Krieger { staerke = 600, Nummer = 2 })]),
                new NahkampfRules.Seite([mit]));
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
        }

        /// <summary>
        /// Ohne Gegner gibt es keine Schlacht - und es fliegt nichts.
        /// </summary>
        [Fact]
        public void OhneGegnerGibtEsNichtsZuVerlieren() {
            var allein = new NahkampfRules.Seite([Krieger(1000, 10, 50)]);

            var ergebnis = NahkampfRules.WerteNahkampfAus(allein, NahkampfRules.Seite.Leer);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, ergebnis.Sieger);
            Assert.Equal(0, ergebnis.Angreifer[0].Verluste.Heeresstärke);
            Assert.Empty(ergebnis.Verteidiger);
            Assert.Equal((0, 0), ergebnis.Katapultbeute);

            var nichts = NahkampfRules.WerteNahkampfAus(null, null);
            Assert.Equal(NahkampfRules.Ausgang.Unentschieden, nichts.Sieger);
            Assert.Empty(nichts.Angreifer);
            Assert.Empty(nichts.Verteidiger);
        }
    }
}
