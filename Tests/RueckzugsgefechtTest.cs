using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Das Rueckzugsgefecht (Regelwerk 5.4) und die Kampfrunde der Charaktere, auf der es
    /// aufsetzt (5.3).
    ///
    /// "Sollten Charaktere eines unterlegenen Heeres den Charakterkampf und den Nahkampf der
    /// Truppen ueberlebt haben, so koennen sie noch ein Rueckzugsgefecht versuchen."
    /// </summary>
    public class RueckzugsgefechtTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        private static Character Charakter(string beschriftung, int gpAkt, int gpGes = 0)
            => new() { Beschriftung = beschriftung, GP_akt = gpAkt, GP_ges = gpGes == 0 ? gpAkt : gpGes, Nummer = 600 };

        // ------------------------------------------------------------- die Wuerfelrunde

        /// <summary>
        /// "Zuerst alle 6er gegeneinander, dann die 5er, die 4er usw... Wuerfelpaarungen mit
        /// gleicher Augenzahl sind parierte Angriffe. Ist ein Ergebnis bei einem Wuerfel eines
        /// Paares hoeher, so ist der hoehere Wurf ein Treffer beim Gegner." (Regelwerk 5.3)
        /// </summary>
        [Fact]
        public void DieWuerfelWerdenVonObenHerabGepaart() {
            // gleiche Augen parieren sich
            var pariert = CharakterkampfRules.WerteWürfelAus([6, 5, 3], [6, 5, 3]);
            Assert.Equal(0, pariert.TrefferEines);
            Assert.Equal(0, pariert.TrefferAnderes);
            Assert.Equal(3, pariert.Parierte);

            // wer die geforderte Zahl nicht hat, muss eine hoehere einsetzen und verliert sie
            var gemischt = CharakterkampfRules.WerteWürfelAus([6, 4], [5, 5]);
            Assert.Equal(1, gemischt.TrefferEines);    // die 6 schlaegt die erste 5
            Assert.Equal(1, gemischt.TrefferAnderes);  // die zweite 5 schlaegt die 4
            Assert.Equal(0, gemischt.Parierte);

            // eine 6 gegen eine 1: ein Treffer
            var klar = CharakterkampfRules.WerteWürfelAus([6], [1]);
            Assert.Equal(1, klar.TrefferEines);
            Assert.Equal(0, klar.TrefferAnderes);
        }

        /// <summary>
        /// "Wuerfel, die nicht mehr zu Wuerfelpaaren zusammengefuegt werden koennen, entfallen."
        ///
        /// Im Beispiel des Regelwerks werfen sechs ZA zusammen 36 Wuerfel gegen die 16 eines ZC
        /// und erzielen 14 Treffer - keine 20 und mehr. Die ueberzaehligen Wuerfel zaehlen also
        /// nicht.
        /// </summary>
        [Fact]
        public void UeberzaehligeWuerfelEntfallen() {
            var ergebnis = CharakterkampfRules.WerteWürfelAus([6, 6, 6, 6], [1]);

            Assert.Equal(1, ergebnis.TrefferEines);
            Assert.Equal(0, ergebnis.TrefferAnderes);
            Assert.Equal(3, ergebnis.Verfallene);

            // ohne Gegenueber gibt es keinen Kampf
            var allein = CharakterkampfRules.WerteWürfelAus([6, 6], (IEnumerable<int>?)null);
            Assert.Equal(0, allein.TrefferEines);
            Assert.Equal(2, allein.Verfallene);
            Assert.Equal(CharakterkampfRules.Würfelergebnis.Nichts, CharakterkampfRules.WerteWürfelAus((IEnumerable<int>?)null, null));
        }

        /// <summary>
        /// "jeder Kaempfer erhaelt pro Gutpunkt seines Charakters 1 W6 zum Wuerfeln"
        /// (Regelwerk 5.3) - gezaehlt werden die aktuellen, nicht die maximalen Gutpunkte.
        /// </summary>
        [Fact]
        public void JederGutpunktIstEinWuerfel() {
            Assert.Equal(24, CharakterkampfRules.GetWürfelzahl(Charakter("BUH1", 24)));
            Assert.Equal(9, CharakterkampfRules.GetWürfelzahl(Charakter("BUH1", 9, gpGes: 24)));
            Assert.Equal(0, CharakterkampfRules.GetWürfelzahl(null));
        }

        /// <summary>
        /// Die Klassenstufen aus der Tabelle in Regelwerk 5.3: Zivilist 1, Heerfuehrer 2,
        /// Burgherr 3, Stadthalter 4, Festungsherr 5, Herrscher 6.
        /// </summary>
        [StaFact]
        public void DieKlassenstufenStehenSoImRegelwerk() {
            LadeAlles();

            Assert.Equal(2, CharakterkampfRules.GetKlassenstufe(Characterklasse.HF));
            Assert.Equal(3, CharakterkampfRules.GetKlassenstufe(Characterklasse.BUH));
            Assert.Equal(4, CharakterkampfRules.GetKlassenstufe(Characterklasse.STH));
            Assert.Equal(5, CharakterkampfRules.GetKlassenstufe(Characterklasse.FSH));
            Assert.Equal(6, CharakterkampfRules.GetKlassenstufe(Characterklasse.HER));
            Assert.Equal(1, CharakterkampfRules.GetKlassenstufe(Characterklasse.none));

            // Zauberer: ZA bis ZF sind dieselben sechs Stufen
            Assert.Equal(1, CharakterkampfRules.GetKlassenstufe(Zaubererklasse.ZA));
            Assert.Equal(6, CharakterkampfRules.GetKlassenstufe(Zaubererklasse.ZF));

            // und ueber die Figur: die Beschriftung sagt das Amt
            Assert.Equal(3, CharakterkampfRules.GetKlassenstufe(Charakter("BUH1", 24)));
            Assert.Equal(4, CharakterkampfRules.GetKlassenstufe(Charakter("STH1", 36)));
            Assert.Equal(6, CharakterkampfRules.GetKlassenstufe(Charakter("HER1", 60)));
        }

        /// <summary>
        /// Die beiden durchgerechneten Beispiele des Regelwerks 5.3.
        ///
        /// Beispiel 1: ein Burgherr (24 GP) gegen zwei Heerfuehrer (je 12 GP). Der BUH landet
        /// 4 Treffer auf CHF1 und bekommt 4 vom CHF2. Ergebnis: BUH +3 gegen CHF1, -6 gegen CHF2.
        ///
        /// Beispiel 2: ein Stadthalter gegen Burgherr und Heerfuehrer. STH -2 gegen den CHF,
        /// +2 gegen den BUH.
        /// </summary>
        [Fact]
        public void DieBeispieleDesRegelwerksGehenAuf() {
            // BUH (Klasse 3) schlaegt CHF1 (Klasse 2) mit 4:0 -> "+3 GP fuer BUH"
            Assert.Equal(3, CharakterkampfRules.BerechneGutpunktänderung(4, 0, klasseSieger: 3, klasseVerlierer: 2));
            Assert.Equal(-3, CharakterkampfRules.BerechneGutpunktänderung(0, 4, klasseSieger: 3, klasseVerlierer: 2));

            // CHF2 (Klasse 2) schlaegt den BUH (Klasse 3) mit 4:0 -> "+6 GP fuer CHF2"
            Assert.Equal(6, CharakterkampfRules.BerechneGutpunktänderung(4, 0, klasseSieger: 2, klasseVerlierer: 3));
            Assert.Equal(-6, CharakterkampfRules.BerechneGutpunktänderung(0, 4, klasseSieger: 2, klasseVerlierer: 3));

            // Beispiel 2: STH gegen CHF, 1:2 -> "-2 GP fuer STH", und der CHF bekommt +2
            Assert.Equal(-2, CharakterkampfRules.BerechneGutpunktänderung(1, 2, klasseSieger: 2, klasseVerlierer: 4));
            Assert.Equal(2, CharakterkampfRules.BerechneGutpunktänderung(2, 1, klasseSieger: 2, klasseVerlierer: 4));

            // STH gegen BUH, 3:1 -> "+1,5 = +2 GP fuer STH"
            Assert.Equal(1.5, CharakterkampfRules.BerechneGutpunktänderungGenau(3, 1, 4, 3), 6);
            Assert.Equal(2, CharakterkampfRules.BerechneGutpunktänderung(3, 1, klasseSieger: 4, klasseVerlierer: 3));
            Assert.Equal(-2, CharakterkampfRules.BerechneGutpunktänderung(1, 3, klasseSieger: 4, klasseVerlierer: 3));

            // "Bei NPC Figuren wird der Gutpunktzuwachs abgerundet."
            Assert.Equal(1, CharakterkampfRules.BerechneGutpunktänderung(3, 1, 4, 3, npc: true));
            Assert.Equal(2, CharakterkampfRules.BerechneGutpunktänderung(4, 0, 3, 2, npc: true));
        }

        // ------------------------------------------------------- das Rueckzugsgefecht

        /// <summary>
        /// "die Einleitung eines Rueckzugefechts ... kann nur erfolgen wenn vor dem Nahkampf der
        /// entsprechende Charakter einen Charakterkampf oder ein Zauberduell bestritten hat"
        /// (Regelwerk 5.4)
        /// </summary>
        [Fact]
        public void NurWerVorDemNahkampfGekaempftHatDarfSichZurueckziehen() {
            var charakter = Charakter("BUH1", 24);

            var ohne = RückzugsRules.Prüfe(charakter,
                new RückzugsRules.Vorgeschichte(HatCharakterkampfGeführt: false,
                    HatRitterkampfGeführt: false, HatNahkampfÜberlebt: true));
            Assert.True(ohne.HasErrors);
            Assert.Contains("nicht gekämpft", ohne.Title);

            var mit = RückzugsRules.Prüfe(charakter,
                new RückzugsRules.Vorgeschichte(HatCharakterkampfGeführt: true,
                    HatRitterkampfGeführt: false, HatNahkampfÜberlebt: true));
            Assert.False(mit.HasErrors, $"{mit.Title}: {mit.Message}");
        }

        /// <summary>
        /// "Ritter, die einen Ritterkampf durchgefuehrt haben, koennen am Rueckzugsgefecht nicht
        /// mehr teilnehmen." Und wer den Nahkampf nicht ueberlebt hat, erst recht nicht.
        /// </summary>
        [Fact]
        public void RitterUndGefalleneBleibenDraussen() {
            var charakter = Charakter("BUH1", 24);

            var ritter = RückzugsRules.Prüfe(charakter,
                new RückzugsRules.Vorgeschichte(HatCharakterkampfGeführt: true,
                    HatRitterkampfGeführt: true, HatNahkampfÜberlebt: true));
            Assert.True(ritter.HasErrors);
            Assert.Contains("Ritterkampf", ritter.Title);

            var gefallen = RückzugsRules.Prüfe(charakter,
                new RückzugsRules.Vorgeschichte(HatCharakterkampfGeführt: true,
                    HatRitterkampfGeführt: false, HatNahkampfÜberlebt: false));
            Assert.True(gefallen.HasErrors);
            Assert.Contains("Nahkampf nicht überlebt", gefallen.Title);

            Assert.True(RückzugsRules.Prüfe(null,
                new RückzugsRules.Vorgeschichte(true, false, true)).HasErrors);
        }

        /// <summary>
        /// "Lehnen alle Charaktere der siegreichen Truppen die Teilnahme ab, so ist der Rueckzug
        /// erfolgreich." (Regelwerk 5.4)
        /// </summary>
        [Fact]
        public void LehnenAlleAbIstDerRueckzugErfolgreich() {
            Assert.True(RückzugsRules.RückzugOhneGefecht(null));
            Assert.True(RückzugsRules.RückzugOhneGefecht([]));
            Assert.False(RückzugsRules.RückzugOhneGefecht([Charakter("HF1", 12)]));
        }

        /// <summary>
        /// "Zurueckgezogene Charaktere duerfen im laufenden und im kommenden Monat keine eigenen
        /// Angriffe mehr starten." (Regelwerk 5.4)
        /// </summary>
        [Fact]
        public void ZurueckgezogeneDuerfenZweiMonateNichtAngreifen() {
            Assert.False(RückzugsRules.DarfAngreifen(zugDesRückzugs: 170, aktuellerZug: 170));
            Assert.False(RückzugsRules.DarfAngreifen(zugDesRückzugs: 170, aktuellerZug: 171));
            Assert.True(RückzugsRules.DarfAngreifen(zugDesRückzugs: 170, aktuellerZug: 172));
        }

        /// <summary>
        /// "so duerfen sie sich in ein nicht von gegnerischen Truppen besetztes, angrenzende
        /// Gemark ihrer Wahl zurueckziehen." (Regelwerk 5.4)
        /// </summary>
        [StaFact]
        public void ZurueckgezogenWirdNurAufFreieNachbarfelder() {
            LadeAlles();

            // ein Charakter auf einem Landfeld mit Landnachbarn
            var start = SharedData.Map!.Values.First(kf => kf.IsWasser == false
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Count(n => n.IsWasser == false) ?? 0) >= 2);
            var charakter = new Character {
                Beschriftung = "BUH1", GP_akt = 24, GP_ges = 24, Nummer = 600,
                Nation = ProgramView.SelectedNation, gf_von = start.gf, kf_von = start.kf,
            };

            var frei = RückzugsRules.FindeRückzugsfelder(charakter, []);
            Assert.NotEmpty(frei);
            Assert.All(frei, feld => Assert.False(feld.IsWasser,
                "Ein Charakter hat sich ins Wasser zurueckgezogen"));

            // jetzt stellt ein verfeindetes Reich ein Heer auf eines der Felder
            var besetzt = frei[0];
            var gegner = SharedData.Nationen!.First(n => n.Equals(ProgramView.SelectedNation) == false
                && DiplomatieRules.SindVerfeindet(n, ProgramView.SelectedNation, besetzt));
            var feindlichesHeer = new Krieger {
                Nation = gegner, staerke = 1000, Nummer = 100,
                gf_von = besetzt.gf, kf_von = besetzt.kf,
            };

            var danach = RückzugsRules.FindeRückzugsfelder(charakter, [feindlichesHeer]);
            Assert.Equal(frei.Count - 1, danach.Count);
            Assert.DoesNotContain(danach, feld => feld.Key == besetzt.Key);

            // ein fremder Charakter allein besetzt eine Gemark nicht - das Regelwerk spricht
            // von Truppen
            var feindlicherCharakter = new Character {
                Nation = gegner, GP_akt = 24, GP_ges = 24, Nummer = 601,
                gf_von = besetzt.gf, kf_von = besetzt.kf,
            };
            Assert.Equal(frei.Count, RückzugsRules.FindeRückzugsfelder(charakter, [feindlicherCharakter]).Count);

            // und ohne Charakter gibt es nichts zu finden
            Assert.Empty(RückzugsRules.FindeRückzugsfelder(null, []));
        }

        /// <summary>
        /// Ein Heer des eigenen Reiches auf dem Nachbarfeld stoert nicht - im Gegenteil.
        /// </summary>
        [StaFact]
        public void EigeneHeereVersperrenDenRueckzugNicht() {
            LadeAlles();

            var start = SharedData.Map!.Values.First(kf => kf.IsWasser == false
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Count(n => n.IsWasser == false) ?? 0) >= 2);
            var charakter = new Character {
                Beschriftung = "BUH1", GP_akt = 24, GP_ges = 24, Nummer = 600,
                Nation = ProgramView.SelectedNation, gf_von = start.gf, kf_von = start.kf,
            };

            var frei = RückzugsRules.FindeRückzugsfelder(charakter, []);
            var eigenesHeer = new Krieger {
                Nation = ProgramView.SelectedNation, staerke = 1000, Nummer = 101,
                gf_von = frei[0].gf, kf_von = frei[0].kf,
            };

            Assert.Equal(frei.Count, RückzugsRules.FindeRückzugsfelder(charakter, [eigenesHeer]).Count);
        }

        /// <summary>
        /// Die Kategorientabelle der Charaktere war verrutscht: sie hatte vier Eintraege, die
        /// Aufzaehlung sechs Werte. Ein Stadthalter kam als Festungsherr heraus, und ein Herrscher
        /// liess die Anwendung mit einer IndexOutOfRangeException stehen.
        /// </summary>
        [StaFact]
        public void JedesAmtFindetSeinenEintrag() {
            LadeAlles();

            foreach (var klasse in new[] { Characterklasse.HF, Characterklasse.BUH,
                    Characterklasse.STH, Characterklasse.FSH, Characterklasse.HER }) {
                var eintrag = PhoenixModel.ExternalTables.CrossrefCharaktere.Get(klasse);
                Assert.True(eintrag != null, $"Zu {klasse} fehlt der Eintrag");
                Assert.Equal(klasse, eintrag!.Klasse);
            }
            Assert.Null(PhoenixModel.ExternalTables.CrossrefCharaktere.Get(Characterklasse.none));

            // und ueber die Beschriftung, ohne dass es fliegt
            Assert.Equal(Characterklasse.HER, CharacterView.GetAssumedKlasse(Charakter("HER1", 60)));
            Assert.Equal(Characterklasse.STH, CharacterView.GetAssumedKlasse(Charakter("STH1", 36)));
            Assert.Equal(Characterklasse.FSH, CharacterView.GetAssumedKlasse(Charakter("FSH1", 48)));
        }
    }
}
