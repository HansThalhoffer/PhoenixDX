using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Ritterkampf (Regelwerk 5.2).
    ///
    /// "Der Ritterkampf ist der Charakterkampf der Ritter des Ritterordens ... der anstatt eines
    /// Charakterkampfes und Rueckzugsgefechtes durchgefuehrt wird."
    ///
    /// Gerechnet wird er wie eine Runde Charakterkampf - geprueft wird hier, was ihm eigen ist.
    /// </summary>
    public class RitterkampfTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Die Anwendung kennt keinen Ritterorden - deshalb bekommt die Suche eine Erkennung
        /// uebergeben. Hier sind es die Charaktere mit "RIT" in der Beschriftung.
        /// </summary>
        private static bool IstRitter(NamensSpielfigur figur)
            => figur.Beschriftung.StartsWith("RIT", StringComparison.OrdinalIgnoreCase);

        private static Character Ritter(int nummer, Nation reich, PhoenixModel.dbErkenfara.KleinFeld feld)
            => new() {
                Beschriftung = $"RIT{nummer}", GP_akt = 24, GP_ges = 24, Nummer = nummer,
                Nation = reich, gf_von = feld.gf, kf_von = feld.kf,
            };

        private static Character Charakter(int nummer, Nation reich, PhoenixModel.dbErkenfara.KleinFeld feld)
            => new() {
                Beschriftung = $"HF{nummer}", GP_akt = 12, GP_ges = 12, Nummer = nummer,
                Nation = reich, gf_von = feld.gf, kf_von = feld.kf,
            };

        /// <summary>
        /// "Befinden sich in den gegnerischen Parteien Charaktere des Ritterordens, so fuehren
        /// diese einen Ritterkampf durch" - es braucht auf beiden Seiten einen.
        /// </summary>
        [StaFact]
        public void EsBrauchtRitterAufBeidenSeiten() {
            LadeAlles();

            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var eigenes = ProgramView.SelectedNation!;
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false
                && DiplomatieRules.SindVerfeindet(n, eigenes, feld));

            // ein Ritter allein: kein Kampf
            Assert.Empty(RitterkampfRules.FindeRitterkämpfe([Ritter(600, eigenes, feld)], IstRitter));

            // ein Ritter gegen einen gewoehnlichen Charakter: auch kein Ritterkampf
            Assert.Empty(RitterkampfRules.FindeRitterkämpfe(
                [Ritter(600, eigenes, feld), Charakter(601, gegner, feld)], IstRitter));

            // zwei Ritter verfeindeter Reiche: jetzt schon
            var kämpfe = RitterkampfRules.FindeRitterkämpfe(
                [Ritter(600, eigenes, feld), Ritter(601, gegner, feld)], IstRitter);
            Assert.Single(kämpfe);
            Assert.Equal(2, kämpfe[0].Ritter.Count);
            Assert.Equal(feld.gf, kämpfe[0].Gemark.gf);
            Assert.Equal(feld.kf, kämpfe[0].Gemark.kf);

            // zwei Ritter desselben Reiches kaempfen nicht gegeneinander
            Assert.Empty(RitterkampfRules.FindeRitterkämpfe(
                [Ritter(600, eigenes, feld), Ritter(602, eigenes, feld)], IstRitter));

            // ohne Erkennung und ohne Figuren passiert nichts
            Assert.Empty(RitterkampfRules.FindeRitterkämpfe(
                [Ritter(600, eigenes, feld), Ritter(601, gegner, feld)], null));
            Assert.Empty(RitterkampfRules.FindeRitterkämpfe(null, IstRitter));
        }

        /// <summary>
        /// Ein gewoehnlicher Charakter auf derselben Gemark gehoert nicht zum Ritterkampf - er
        /// hat seinen eigenen Charakterkampf.
        /// </summary>
        [StaFact]
        public void GewoehnlicheCharaktereBleibenDraussen() {
            LadeAlles();

            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var eigenes = ProgramView.SelectedNation!;
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false
                && DiplomatieRules.SindVerfeindet(n, eigenes, feld));

            var kämpfe = RitterkampfRules.FindeRitterkämpfe([
                Ritter(600, eigenes, feld),
                Ritter(601, gegner, feld),
                Charakter(610, eigenes, feld),
                Charakter(611, gegner, feld),
            ], IstRitter);

            Assert.Single(kämpfe);
            Assert.Equal(2, kämpfe[0].Ritter.Count);
            Assert.All(kämpfe[0].Ritter, r => Assert.StartsWith("RIT", r.Beschriftung));
        }

        /// <summary>
        /// "Das heisst, Ritter ... koennen am Charterkampf der anderen nicht mehr teilnehmen."
        /// (Regelwerk 5.2) Und am Rueckzugsgefecht auch nicht (5.4).
        /// </summary>
        [StaFact]
        public void WerEinenRitterkampfGefuehrtHatIstAusDemRestRaus() {
            LadeAlles();

            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var ritter = Ritter(600, ProgramView.SelectedNation!, feld);

            Assert.False(RitterkampfRules.NimmtAmCharakterkampfTeil(ritter, hatRitterkampfGeführt: true));
            Assert.True(RitterkampfRules.NimmtAmCharakterkampfTeil(ritter, hatRitterkampfGeführt: false));

            // das Rueckzugsgefecht weist ihn ebenfalls ab
            var rückzug = RückzugsRules.Prüfe(ritter,
                new RückzugsRules.Vorgeschichte(HatCharakterkampfGeführt: true,
                    HatRitterkampfGeführt: true, HatNahkampfÜberlebt: true));
            Assert.True(rückzug.HasErrors);
            Assert.Contains("Ritterkampf", rückzug.Title);
        }

        /// <summary>
        /// "Ritter bringen nur den Rest ihrer nach dem Ritterkampf verbliebenen Gutpunkte in den
        /// Nahkampf ein." (Regelwerk 5.2)
        /// </summary>
        [Fact]
        public void NurDerRestGehtInDenNahkampf() {
            // 24 Gutpunkte, im Ritterkampf 9 verloren
            Assert.Equal(15, RitterkampfRules.GetGutpunkteFürNahkampf(15));
            // mehr als alles verloren: nichts bleibt uebrig, aber negativ wird es nicht
            Assert.Equal(0, RitterkampfRules.GetGutpunkteFürNahkampf(-3));
        }

        /// <summary>
        /// "Ungeachtet des Ausgangs der Schlacht kann der am Ende unterlegene Ritter ohne
        /// Rueckzugsgefecht in ein benachbartes Gemark abziehen." (Regelwerk 5.2)
        ///
        /// Entscheidend ist der Ausgang des Ritterkampfes, nicht der der Schlacht.
        /// </summary>
        [Fact]
        public void DerUnterlegeneRitterDarfGehen() {
            Assert.True(RitterkampfRules.DarfAbziehen(vorsprungImRitterkampf: -2));
            Assert.False(RitterkampfRules.DarfAbziehen(vorsprungImRitterkampf: 2));
            // unentschieden ist nicht unterlegen
            Assert.False(RitterkampfRules.DarfAbziehen(vorsprungImRitterkampf: 0));
        }

        /// <summary>
        /// Abgezogen wird auf dieselben Felder wie beim Rueckzugsgefecht - und danach gilt
        /// dieselbe Angriffssperre (Regelwerk 5.4).
        /// </summary>
        [StaFact]
        public void AbgezogenWirdAufFreieNachbarfelder() {
            LadeAlles();

            var start = SharedData.Map!.Values.First(kf => kf.IsWasser == false
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Count(n => n.IsWasser == false) ?? 0) >= 2);
            var ritter = Ritter(600, ProgramView.SelectedNation!, start);

            var felder = RitterkampfRules.FindeAbzugsfelder(ritter, []);
            Assert.NotEmpty(felder);
            Assert.Equal(RückzugsRules.FindeRückzugsfelder(ritter, []).Count, felder.Count);

            // ein feindliches Heer nimmt ein Feld aus der Auswahl
            var gegner = SharedData.Nationen!.First(n => n.Equals(ProgramView.SelectedNation) == false
                && DiplomatieRules.SindVerfeindet(n, ProgramView.SelectedNation, felder[0]));
            var heer = new Krieger {
                Nation = gegner, staerke = 1000, Nummer = 100,
                gf_von = felder[0].gf, kf_von = felder[0].kf,
            };
            Assert.Equal(felder.Count - 1, RitterkampfRules.FindeAbzugsfelder(ritter, [heer]).Count);

            // und im kommenden Zug greift er nicht an
            Assert.False(RitterkampfRules.DarfAngreifen(zugDesAbzugs: 170, aktuellerZug: 171));
            Assert.True(RitterkampfRules.DarfAngreifen(zugDesAbzugs: 170, aktuellerZug: 172));
        }
    }
}
