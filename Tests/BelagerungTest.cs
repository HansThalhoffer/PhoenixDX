using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Belagerung (Regelwerk 1.7).
    ///
    /// "Die Belagerung wird bei elektronischer Auswertung automatisch eingeleitet, wenn ein
    /// feindliches Heer oder Flotte auf einer angrenzenden Gemark steht!" - sie wird also nicht
    /// befohlen, sondern gesucht. Je Gemark, von der aus belagert wird, 15 Prozent weniger.
    /// </summary>
    public class BelagerungTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Ein eigener Ruestort mit mindestens zwei Landnachbarn, damit sich belagern laesst.
        /// </summary>
        private static KleinFeld FindeBelagerbarenRuestort() {
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation != null && kf.IsWasser == false
                && BelagerungsRules.IstBelagerbar(kf)
                && RuestRules.GetKapazität(kf).Goldstücke > 0
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Count(n => n.IsWasser == false) ?? 0) >= 2);
            Assert.True(gemark != null, "Kein belagerbarer Ruestort mit zwei Landnachbarn gefunden");
            return gemark!;
        }

        private static Krieger Heer(Nation reich, KleinFeld steht, int nummer, bool seitMonatsbeginn = true)
            => new() {
                Nummer = nummer, Nation = reich, staerke = 2000, hf = 1,
                gf_von = steht.gf, kf_von = steht.kf,
                gf_nach = seitMonatsbeginn ? 0 : steht.gf,
                kf_nach = seitMonatsbeginn ? 0 : steht.kf,
            };

        /// <summary>
        /// "Burgen, Staedte und Hauptstaedte koennen ... belagert werden" (Regelwerk 1.7.1) -
        /// Festung und Festungshauptstadt nicht (1.5.9). Dieselbe Auskunft steht in der
        /// Ruestortreferenz.
        /// </summary>
        [StaFact]
        public void FestungenLassenSichNichtBelagern() {
            LadeAlles();

            (string Name, bool Belagerbar)[] erwartet = [
                ("Burg", true), ("Stadt", true), ("Hauptstadt", true),
                ("Festung", false), ("Festungshauptstadt", false),
            ];
            foreach (var (name, belagerbar) in erwartet) {
                var rüstort = SharedData.RüstortReferenz!.First(r => r.Ruestort == name);
                Assert.Equal(belagerbar, rüstort.canSieged == true);
            }

            Assert.False(BelagerungsRules.IstBelagerbar(null));

            // und auf der Karte: eine Burg laesst sich belagern, eine Festung nicht. Gesucht
            // werden die Felder ueber den Namen des Ruestorts, nicht ueber das Kennzeichen -
            // sonst prueft der Test sich selbst.
            string? NameDesRuestorts(KleinFeld kf) => BauwerkeView.GetRüstortNachKarte(kf)?.Ruestort;
            var burg = SharedData.Map!.Values.FirstOrDefault(kf => NameDesRuestorts(kf)?.StartsWith("Burg") == true);
            var festung = SharedData.Map!.Values.FirstOrDefault(kf => NameDesRuestorts(kf)?.StartsWith("Festung") == true);
            Assert.True(burg != null, "Auf der Karte steht keine Burg");
            Assert.True(festung != null, "Auf der Karte steht keine Festung");

            Assert.True(BelagerungsRules.IstBelagerbar(burg));
            Assert.False(BelagerungsRules.IstBelagerbar(festung));
        }

        /// <summary>
        /// Gezaehlt werden Gemarken, nicht Heere: "pro Gemark von der aus belagert wird".
        /// Je Gemark 15 Prozent.
        /// </summary>
        [StaFact]
        public void JedeGemarkSchnuertUmFuenfzehnProzentEin() {
            LadeAlles();
            var rüstort = FindeBelagerbarenRuestort();
            var nachbarn = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)!
                .Where(n => n.IsWasser == false).ToList();
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort.Nation, rüstort));

            // ein Heer auf einer Nachbargemark
            var eine = BelagerungsRules.FindeBelagerung(rüstort, [Heer(gegner, nachbarn[0], 100)]);
            Assert.True(eine.Besteht);
            Assert.Equal(1, eine.AnzahlGemarken);
            Assert.Equal(0.15, eine.Minderung, 6);

            // zwei Heere auf derselben Gemark schnueren nicht doppelt
            var zweiAufEiner = BelagerungsRules.FindeBelagerung(rüstort,
                [Heer(gegner, nachbarn[0], 100), Heer(gegner, nachbarn[0], 101)]);
            Assert.Equal(1, zweiAufEiner.AnzahlGemarken);
            Assert.Equal(0.15, zweiAufEiner.Minderung, 6);

            // zwei Gemarken dagegen schon
            var zwei = BelagerungsRules.FindeBelagerung(rüstort,
                [Heer(gegner, nachbarn[0], 100), Heer(gegner, nachbarn[1], 101)]);
            Assert.Equal(2, zwei.AnzahlGemarken);
            Assert.Equal(0.30, zwei.Minderung, 6);

            // und die Minderung wirkt auf einen Wert
            Assert.Equal(8500, BelagerungsRules.Mindere(10000, eine));
            Assert.Equal(7000, BelagerungsRules.Mindere(10000, zwei));
            Assert.Equal(10000, BelagerungsRules.Mindere(10000, BelagerungsRules.Belagerung.Keine));
        }

        /// <summary>
        /// "so dass man im vorhergehenden Monat schon ... herangezogen sein muss" (Regelwerk 1.7) -
        /// wer erst in diesem Monat ankommt, belagert noch nicht.
        /// </summary>
        [StaFact]
        public void WerErstInDiesemMonatAnkommtBelagertNochNicht() {
            LadeAlles();
            var rüstort = FindeBelagerbarenRuestort();
            var nachbar = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)!
                .First(n => n.IsWasser == false);
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort.Nation, rüstort));

            // stand schon zu Monatsbeginn dort
            Assert.True(BelagerungsRules.FindeBelagerung(rüstort, [Heer(gegner, nachbar, 100)]).Besteht);

            // in diesem Monat erst hingezogen: die Ausgangsposition ist eine andere
            var angerueckt = Heer(gegner, nachbar, 100);
            angerueckt.gf_von = rüstort.gf;
            angerueckt.kf_von = rüstort.kf == 1 ? 2 : 1;
            angerueckt.gf_nach = nachbar.gf;
            angerueckt.kf_nach = nachbar.kf;
            Assert.False(BelagerungsRules.FindeBelagerung(rüstort, [angerueckt]).Besteht);
        }

        /// <summary>
        /// Belagert wird von Feinden - eigene und verbuendete Heere auf der Nachbargemark
        /// schnueren nichts ein.
        /// </summary>
        [StaFact]
        public void EigeneHeereBelagernNicht() {
            LadeAlles();
            var rüstort = FindeBelagerbarenRuestort();
            var nachbar = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)!
                .First(n => n.IsWasser == false);

            Assert.False(BelagerungsRules.FindeBelagerung(rüstort, [Heer(rüstort.Nation!, nachbar, 100)]).Besteht);

            // ein Charakter ist kein Heer
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort.Nation, rüstort));
            var charakter = new Character {
                Nummer = 600, Nation = gegner, Beschriftung = "HF1", GP_akt = 12, GP_ges = 12,
                gf_von = nachbar.gf, kf_von = nachbar.kf,
            };
            Assert.False(BelagerungsRules.FindeBelagerung(rüstort, [charakter]).Besteht);

            // und ohne Figuren ist nichts
            Assert.False(BelagerungsRules.FindeBelagerung(rüstort, []).Besteht);
            Assert.False(BelagerungsRules.FindeBelagerung(null, []).Besteht);
        }

        /// <summary>
        /// "aus denen heraus sie auch betreten werden koennen" (Regelwerk 1.7.1) - eine Flotte auf
        /// dem Nachbargewaesser kann einen Ruestort an Land nicht betreten und belagert ihn
        /// deshalb auch nicht.
        /// </summary>
        [StaFact]
        public void EineFlotteBelagertKeinenRuestortAnLand() {
            LadeAlles();
            var rüstort = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation != null && kf.IsWasser == false && BelagerungsRules.IstBelagerbar(kf)
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Any(n => n.IsWasser) ?? false));
            Assert.True(rüstort != null, "Kein belagerbarer Ruestort am Wasser gefunden");

            var wasser = KleinfeldView.GetNachbarn(rüstort!, 1, includeSelf: false)!.First(n => n.IsWasser);
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort!.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort!.Nation, rüstort));

            var flotte = new Schiffe {
                Nummer = 300, Nation = gegner, staerke = 20, hf = 1,
                gf_von = wasser.gf, kf_von = wasser.kf,
            };
            Assert.False(BelagerungsRules.FindeBelagerung(rüstort, [flotte]).Besteht);
        }

        /// <summary>
        /// Die Belagerung mindert die Ruestkapazitaet (Regelwerk 1.7.1) - und die Pruefung sagt
        /// auch, warum.
        /// </summary>
        [StaFact]
        public void DieRuestkapazitaetSinkt() {
            LadeAlles();
            var rüstort = FindeBelagerbarenRuestort();
            var nachbar = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)!
                .First(n => n.IsWasser == false);
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort.Nation, rüstort));

            int voll = RuestRules.GetKapazität(rüstort).Goldstücke;
            Assert.True(voll > 0);

            var belagerung = BelagerungsRules.FindeBelagerung(rüstort, [Heer(gegner, nachbar, 100)]);
            var gemindert = new RuestRules.Rüstkapazität(voll, 10, 2).Gemindert(belagerung);

            Assert.Equal(BelagerungsRules.Mindere(voll, belagerung), gemindert.Goldstücke);
            Assert.Equal(8, gemindert.Heerführer);
            Assert.True(gemindert.Goldstücke < voll);

            // ohne Belagerung bleibt alles
            Assert.Equal(voll, new RuestRules.Rüstkapazität(voll, 10, 2)
                .Gemindert(BelagerungsRules.Belagerung.Keine).Goldstücke);
        }

        /// <summary>
        /// "die monatlich maximal moegliche Erhoehung der Baupunkte der Grossbaustelle verringert
        /// sich pro Gemark von der aus belagert wird um 15%" (Regelwerk 1.7.2)
        /// </summary>
        [StaFact]
        public void DieGrossbaustelleWaechstLangsamer() {
            LadeAlles();

            // ohne Belagerung die volle Monatsleistung
            var frei = SharedData.Map!.Values.First(kf => kf.Nation != null && kf.IsWasser == false);
            Assert.Equal(RuestortRules.MaxBaupunkteProMonat, RuestortRules.GetMaxBaupunkteProMonat(frei));

            // 250 Baupunkte, eine Gemark Belagerung: 212
            Assert.Equal(212, BelagerungsRules.Mindere(RuestortRules.MaxBaupunkteProMonat,
                new BelagerungsRules.Belagerung(new KleinfeldPosition(1, 1),
                    [new BelagerungsRules.Belagerer(new KleinfeldPosition(1, 2), SharedData.Nationen!.First(), "Heer")])));
        }

        /// <summary>
        /// Der Spieler sieht nur seine Feindaufklaerung - auch daraus laesst sich eine Belagerung
        /// ablesen.
        /// </summary>
        [StaFact]
        public void AuchDieFeindaufklaerungZeigtEineBelagerung() {
            LadeAlles();
            var rüstort = FindeBelagerbarenRuestort();
            var nachbar = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)!
                .First(n => n.IsWasser == false);
            var gegner = SharedData.Nationen!.First(n => n.Equals(rüstort.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, rüstort.Nation, rüstort));

            Assert.False(BelagerungsRules.FindeBelagerungNachAufklärung(rüstort).Besteht);

            var vorher = SharedData.Feinde;
            try {
                // die Zeile hat das Format der Feindaufklaerungsdatei:
                // Nummer;Reich;Art;gf;kf
                var aufklärung = new System.Collections.Concurrent.BlockingCollection<Feinde> {
                    new Feinde($"101;{gegner.Reich};Krieger;{nachbar.gf};{nachbar.kf}"),
                };
                aufklärung.CompleteAdding();
                SharedData.Feinde = aufklärung;
                var belagerung = BelagerungsRules.FindeBelagerungNachAufklärung(rüstort);
                Assert.True(belagerung.Besteht);
                Assert.Equal(1, belagerung.AnzahlGemarken);
                Assert.Contains(nachbar.CreateBezeichner(), belagerung.Beschreibung);
            }
            finally {
                SharedData.Feinde = vorher;
            }
        }
    }
}
