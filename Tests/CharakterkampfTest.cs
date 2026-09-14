using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Charakterkampf und das Zauberduell als eigener Schritt (Regelwerk 5.3).
    ///
    /// Die Wuerfelrunde selbst prueft RueckzugsgefechtTest - sie ist dieselbe. Hier geht es um
    /// das, was nur zu 5.3 gehoert: wer mitmacht, wie die Paarungen aufgestellt werden duerfen,
    /// die automatischen Einser des Zauberduells und seine Folgen. Und darum, wann es ueberhaupt
    /// dazu kommt.
    /// </summary>
    public class CharakterkampfTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        private static Character Charakter(int nummer, int gp = 12)
            => new() { Beschriftung = $"HF{nummer}", GP_akt = gp, GP_ges = gp, Nummer = nummer };

        private static Zauberer Magier(int nummer, int zkp, int gpGes, string beschriftung = "ZA")
            => new() { Beschriftung = beschriftung, GP_akt = zkp, GP_ges = gpGes, Nummer = nummer };

        /// <summary>
        /// "Charakterzauberer nehmen nicht am Charakterkampf teil." (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void CharakterzaubererBleibenDemCharakterkampfFern() {
            Assert.True(CharakterkampfRules.NimmtAmCharakterkampfTeil(Charakter(600)));
            Assert.True(CharakterkampfRules.NimmtAmCharakterkampfTeil(Magier(500, 6, 6)));

            // ein Charakterzauberer traegt ein CZ in der Beschriftung oder einen Spielernamen
            var charakterzauberer = Magier(501, 6, 6, beschriftung: "CZB1");
            Assert.Equal(FigurType.CharakterZauberer, charakterzauberer.Typ);
            Assert.False(CharakterkampfRules.NimmtAmCharakterkampfTeil(charakterzauberer));

            Assert.False(CharakterkampfRules.NimmtAmCharakterkampfTeil(null));
        }

        /// <summary>
        /// "Jeder Verteidiger muss erst durch einen Angreifer in einen Kampf verwickelt werden,
        /// bevor zusaetzliche Angreifer zu dieser Kampfpaarung hinzukommen koennen. Es duerfen bis
        /// zu maximal 4 Personen auf einen Charakter schlagen." (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void ErstJedenVerwickelnDannZuZweitDraufschlagen() {
            var ziel1 = Charakter(601);
            var ziel2 = Charakter(602);
            var angreifer = Enumerable.Range(610, 5).Select(n => Charakter(n)).ToList();

            // zwei auf einen, waehrend der zweite Verteidiger noch frei steht: nein
            var zuFrueh = CharakterkampfRules.PrüfePaarungen(
                [new CharakterkampfRules.Paarung(ziel1, [angreifer[0], angreifer[1]])],
                [ziel1, ziel2]);
            Assert.True(zuFrueh.HasErrors);
            Assert.Contains("noch nicht verwickelt", zuFrueh.Title);

            // jeder einer: geht
            var sauber = CharakterkampfRules.PrüfePaarungen([
                new CharakterkampfRules.Paarung(ziel1, [angreifer[0]]),
                new CharakterkampfRules.Paarung(ziel2, [angreifer[1]]),
            ], [ziel1, ziel2]);
            Assert.False(sauber.HasErrors, $"{sauber.Title}: {sauber.Message}");

            // und wenn alle verwickelt sind, darf nachgelegt werden
            var nachgelegt = CharakterkampfRules.PrüfePaarungen([
                new CharakterkampfRules.Paarung(ziel1, [angreifer[0], angreifer[2]]),
                new CharakterkampfRules.Paarung(ziel2, [angreifer[1]]),
            ], [ziel1, ziel2]);
            Assert.False(nachgelegt.HasErrors, $"{nachgelegt.Title}: {nachgelegt.Message}");
        }

        /// <summary>
        /// "Es duerfen bis zu maximal 4 Personen auf einen Charakter schlagen. Dies gilt fuer
        /// Zauberduelle nicht." (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void VierAufEinenImCharakterkampfImZauberduellMehr() {
            var ziel = Charakter(601);
            var fünf = Enumerable.Range(610, 5).Select(n => Charakter(n)).ToList();

            var zuViele = CharakterkampfRules.PrüfePaarungen(
                [new CharakterkampfRules.Paarung(ziel, fünf)], [ziel]);
            Assert.True(zuViele.HasErrors);
            Assert.Contains("Zu viele Angreifer", zuViele.Title);

            var vier = CharakterkampfRules.PrüfePaarungen(
                [new CharakterkampfRules.Paarung(ziel, fünf.Take(4).ToList())], [ziel]);
            Assert.False(vier.HasErrors, $"{vier.Title}: {vier.Message}");

            // im Zauberduell gilt die Grenze nicht
            var duell = CharakterkampfRules.PrüfePaarungen(
                [new CharakterkampfRules.Paarung(ziel, fünf)], [ziel], zauberduell: true);
            Assert.False(duell.HasErrors, $"{duell.Title}: {duell.Message}");
        }

        /// <summary>
        /// Jeder Charakter schlaegt sich mit einem Gegner, und eine Paarung ohne Angreifer ist
        /// keine.
        /// </summary>
        [Fact]
        public void JederSchlaegtSichMitEinemGegner() {
            var ziel1 = Charakter(601);
            var ziel2 = Charakter(602);
            var doppelt = Charakter(610);

            var zweimal = CharakterkampfRules.PrüfePaarungen([
                new CharakterkampfRules.Paarung(ziel1, [doppelt]),
                new CharakterkampfRules.Paarung(ziel2, [doppelt]),
            ], [ziel1, ziel2]);
            Assert.True(zweimal.HasErrors);
            Assert.Contains("zwei Gegner", zweimal.Title);

            var leer = CharakterkampfRules.PrüfePaarungen(
                [new CharakterkampfRules.Paarung(ziel1, [])], [ziel1]);
            Assert.True(leer.HasErrors);

            Assert.True(CharakterkampfRules.PrüfePaarungen(null, [ziel1]).HasErrors);
            Assert.True(CharakterkampfRules.PrüfePaarungen([], [ziel1]).HasErrors);
        }

        /// <summary>
        /// "Bei einem Zauberduell bestimmt der aktuelle Zauberkraftwert die Anzahl der W6.
        /// Zusaetzlich werden die verbrauchten Zauberkraftwerte ... als W1 beruecksichtigt."
        /// (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void VerbrauchteZauberkraftWirftAutomatischeEinser() {
            // ein Zauberer mit 8 von 12 Punkten: acht geworfene Wuerfel, vier automatische Einser
            var zauberer = Magier(500, zkp: 8, gpGes: 12);
            var würfel = CharakterkampfRules.BaueZauberduellwürfel(zauberer, [6, 5, 5, 4, 3, 2, 2, 1]);

            Assert.Equal(12, würfel.Count);
            Assert.Equal(4, würfel.Count(w => w.Automatisch));
            Assert.All(würfel.Where(w => w.Automatisch), w => Assert.Equal(1, w.Augen));

            // "Bei neutralisierten Zauberern werden die neutralisierten Zauberkraftpunkte wie
            // verbrauchte behandelt."
            var neutralisiert = CharakterkampfRules.BaueZauberduellwürfel(zauberer, [6, 5, 5, 4, 3, 2, 2, 1], neutralisiert: 3);
            Assert.Equal(7, neutralisiert.Count(w => w.Automatisch));

            Assert.Empty(CharakterkampfRules.BaueZauberduellwürfel(null, null));
        }

        /// <summary>
        /// "von der Berechnung sind Treffer gegen eine 'automatische 1' ausgenommen"
        /// (Regelwerk 5.3) - sie zaehlen fuer den Ausgang, aber nicht fuer die Gutpunkte.
        /// </summary>
        [Fact]
        public void TrefferGegenEinenAutomatischenEinserBringenKeineGutpunkte() {
            CharakterkampfRules.Würfel[] angreifer = [new(6), new(5)];
            CharakterkampfRules.Würfel[] verteidiger = [new(1, Automatisch: true), new(4)];

            var ergebnis = CharakterkampfRules.WerteWürfelAus(angreifer, verteidiger);

            Assert.Equal(2, ergebnis.TrefferEines);              // beide Wuerfel treffen
            Assert.Equal(1, ergebnis.AnrechenbareTrefferEines);  // einer davon gegen einen Einser
            Assert.Equal(0, ergebnis.TrefferAnderes);

            // ohne automatische Wuerfel sind beide Zahlen gleich
            var gewöhnlich = CharakterkampfRules.WerteWürfelAus([6, 5], [1, 4]);
            Assert.Equal(gewöhnlich.TrefferEines, gewöhnlich.AnrechenbareTrefferEines);
        }

        /// <summary>
        /// "Zauberer die ein Zauberduell verloren oder unentschieden beendet haben werden in die
        /// Hauptstadt ihres Reiches zurueck geschleudert und verlieren dabei alle noch vorhandenen
        /// Zauberkraftpunkte." Und: "Jeder Zauberer stirbt im Zauberduell, sobald sein
        /// Gutpunktwert unter die Grenze von 1 sinkt!" (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void WerVerliertOderUnentschiedenBleibtWirdZurueckgeschleudert() {
            // gewonnen: bleibt stehen und behaelt seine Punkte
            var sieger = CharakterkampfRules.BestimmeZauberduellfolge(gutpunkteNachher: 5, vorsprung: 2);
            Assert.False(sieger.Gestorben);
            Assert.False(sieger.Zurückgeschleudert);
            Assert.Equal(5, sieger.Zauberkraftpunkte);

            // verloren: zurueck in die Hauptstadt, ohne Zauberkraft
            var verlierer = CharakterkampfRules.BestimmeZauberduellfolge(5, vorsprung: -2);
            Assert.True(verlierer.Zurückgeschleudert);
            Assert.Equal(0, verlierer.Zauberkraftpunkte);
            Assert.False(verlierer.Gestorben);

            // unentschieden zaehlt wie verloren
            var unentschieden = CharakterkampfRules.BestimmeZauberduellfolge(5, vorsprung: 0);
            Assert.True(unentschieden.Zurückgeschleudert);
            Assert.Equal(0, unentschieden.Zauberkraftpunkte);

            // unter 1 Gutpunkt ist er tot - auch wenn er mehr Treffer gelandet hat
            var tot = CharakterkampfRules.BestimmeZauberduellfolge(0, vorsprung: 3);
            Assert.True(tot.Gestorben);
            Assert.Equal(0, tot.Zauberkraftpunkte);
            Assert.False(CharakterkampfRules.BestimmeZauberduellfolge(1, 3).Gestorben);
        }

        /// <summary>
        /// "Zum Zauberduell kommt es, wenn Zauberer von verfeindeten Reichen in der gleichen oder
        /// in benachbarten Gemarken stehen." (Regelwerk 5.3)
        /// </summary>
        [StaFact]
        public void ZaubererDuellierenSichAuchUeberDieGemarkgrenze() {
            LadeAlles();

            var hier = SharedData.Map!.Values.First(kf => kf.IsWasser == false
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Any() ?? false));
            var nachbar = KleinfeldView.GetNachbarn(hier, 1, includeSelf: false)!.First();
            var fern = SharedData.Map!.Values.First(kf =>
                FernkampfRules.GetEntfernung(hier, kf, 2) < 0 && kf.Key != hier.Key);

            var eigenes = ProgramView.SelectedNation;
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false
                && DiplomatieRules.SindVerfeindet(n, eigenes, hier)
                && DiplomatieRules.SindVerfeindet(n, eigenes, nachbar));

            Zauberer Magier(int nummer, PhoenixModel.dbPZE.Nation reich, KleinFeld feld)
                => new() { Beschriftung = "ZA", GP_akt = 6, GP_ges = 6, Nummer = nummer,
                           Nation = reich, gf_von = feld.gf, kf_von = feld.kf };

            var meiner = Magier(500, eigenes!, hier);

            // auf derselben Gemark
            var zusammen = KonfliktRules.FindeZauberduelle([meiner, Magier(501, gegner, hier)]);
            Assert.Single(zusammen);
            Assert.Equal(0, zusammen[0].Entfernung);

            // auf der Nachbargemark
            var daneben = KonfliktRules.FindeZauberduelle([meiner, Magier(501, gegner, nachbar)]);
            Assert.Single(daneben);
            Assert.Equal(1, daneben[0].Entfernung);

            // weiter weg: kein Duell
            Assert.Empty(KonfliktRules.FindeZauberduelle([meiner, Magier(501, gegner, fern)]));

            // eigene Zauberer duellieren sich nicht
            Assert.Empty(KonfliktRules.FindeZauberduelle([meiner, Magier(502, eigenes!, hier)]));

            // und ohne Figuren passiert nichts
            Assert.Empty(KonfliktRules.FindeZauberduelle(null));
        }

        /// <summary>
        /// Charaktere sind keine Zauberer: sie tauchen in der Duellsuche nicht auf.
        /// </summary>
        [StaFact]
        public void CharaktereTauchenInDerDuellsucheNichtAuf() {
            LadeAlles();

            var hier = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var eigenes = ProgramView.SelectedNation;
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false
                && DiplomatieRules.SindVerfeindet(n, eigenes, hier));

            var meinCharakter = new Character {
                Beschriftung = "HF1", GP_akt = 12, GP_ges = 12, Nummer = 600,
                Nation = eigenes, gf_von = hier.gf, kf_von = hier.kf,
            };
            var seinCharakter = new Character {
                Beschriftung = "HF1", GP_akt = 12, GP_ges = 12, Nummer = 601,
                Nation = gegner, gf_von = hier.gf, kf_von = hier.kf,
            };

            Assert.Empty(KonfliktRules.FindeZauberduelle([meinCharakter, seinCharakter]));

            // als Charakterkampf wird das Treffen aber gemeldet
            var konflikte = KonfliktRules.FindeKonflikte([meinCharakter, seinCharakter]);
            Assert.Single(konflikte);
            Assert.Equal(KonfliktRules.Kampfart.Charakterkampf, konflikte[0].Art);
        }
    }
}
