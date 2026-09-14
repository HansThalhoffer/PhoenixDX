using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Das eroberungsfaehige Heer (Regelwerk 1.8) und die vier Regeln, die daran haengen.
    ///
    /// "Ein Landheer mit 1000 Raumpunkten plus Heerfuehrer oder Adeliger gilt als
    /// eroberungsfaehiges Heer."
    /// </summary>
    public class EroberungsfaehigkeitTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        private static Krieger Heer(int staerke, int hf, KleinFeld steht, Nation reich, int nummer = 100)
            => new() {
                Nummer = nummer, Nation = reich, staerke = staerke, hf = hf,
                gf_von = steht.gf, kf_von = steht.kf,
            };

        /// <summary>
        /// Ein Landfeld mit zwei Landnachbarn, auf dem kein eigener Charakter steht - sonst
        /// ersetzt der Adelige den fehlenden Heerfuehrer und die Pruefung faellt anders aus.
        /// </summary>
        private static KleinFeld FindeLandfeldMitNachbarn() {
            var eigene = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<Character>().Select(c => c.Key).ToHashSet();
            return SharedData.Map!.Values.First(kf => kf.Nation != null && kf.IsWasser == false
                && eigene.Contains(kf.Key) == false
                && (KleinfeldView.GetNachbarn(kf, 1, includeSelf: false)?.Count(n => n.IsWasser == false) ?? 0) >= 2);
        }

        /// <summary>
        /// Tausend Raumpunkte und ein Heerfuehrer - ein Krieger belegt einen Raumpunkt, also
        /// braucht es tausend davon.
        /// </summary>
        [StaFact]
        public void TausendRaumpunkteUndEinHeerfuehrer() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var reich = ProgramView.SelectedNation!;

            Assert.Equal(1000, HeeresRules.RaumpunkteFürEroberung);

            Assert.True(HeeresRules.IstEroberungsfähig(Heer(1000, 1, feld, reich)));

            // zu klein - die Raumpunkte des Heerfuehrers zaehlen mit, deshalb steht die Praemisse
            // hier ausdruecklich da
            var klein = Heer(100, 1, feld, reich);
            Assert.True(SpielfigurRules.BerechneRaumpunkte(klein) < HeeresRules.RaumpunkteFürEroberung,
                $"Das kleine Heer hat {SpielfigurRules.BerechneRaumpunkte(klein)} Raumpunkte");
            Assert.False(HeeresRules.IstEroberungsfähig(klein));

            // ohne Heerfuehrer und ohne Adeligen
            Assert.False(HeeresRules.IstEroberungsfähig(Heer(5000, 0, feld, reich)));

            Assert.False(HeeresRules.IstEroberungsfähig(null));
        }

        /// <summary>
        /// Eine Flotte ist kein Landheer: "Wassergemarken koennen nicht erobert werden" (0.3).
        /// </summary>
        [StaFact]
        public void EineFlotteErobertNichts() {
            LadeAlles();
            var wasser = SharedData.Map!.Values.First(kf => kf.IsWasser);
            var reich = ProgramView.SelectedNation!;

            var flotte = new Schiffe {
                Nummer = 300, Nation = reich, staerke = 100, hf = 5,
                gf_von = wasser.gf, kf_von = wasser.kf,
            };
            Assert.True(SpielfigurRules.BerechneRaumpunkte(flotte) >= HeeresRules.RaumpunkteFürEroberung);
            Assert.False(HeeresRules.IstEroberungsfähig(flotte));
        }

        /// <summary>
        /// "plus Heerfuehrer oder Adeliger" - ein Adeliger auf derselben Gemark ersetzt den
        /// Heerfuehrer. Ein Zivilist nicht: adelig sind Heerfuehrer, Burgherr, Stadthalter,
        /// Festungsherr und Herrscher (Regelwerk 1.9.1).
        /// </summary>
        [StaFact]
        public void EinAdeligerErsetztDenHeerfuehrer() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var reich = ProgramView.SelectedNation!;
            var heer = Heer(5000, 0, feld, reich);

            Assert.False(HeeresRules.IstEroberungsfähig(heer));
            Assert.False(HeeresRules.BegleitetEinAdliger(heer));

            // die eigenen Charaktere stehen woanders; einen davon auf die Gemark stellen
            var charakter = SpielfigurenView.GetSpielfiguren(reich).OfType<Character>()
                .FirstOrDefault(c => CharakterkampfRules.GetKlassenstufe(c) > CharakterkampfRules.KlasseZivilist);
            Assert.True(charakter != null, "Das eigene Reich hat keinen adeligen Charakter");

            int gf = charakter!.gf_von, kf = charakter.kf_von;
            int gfNach = charakter.gf_nach, kfNach = charakter.kf_nach;
            try {
                charakter.gf_von = feld.gf;
                charakter.kf_von = feld.kf;
                charakter.gf_nach = 0;
                charakter.kf_nach = 0;

                Assert.True(HeeresRules.BegleitetEinAdliger(heer));
                Assert.True(HeeresRules.IstEroberungsfähig(heer));
            }
            finally {
                charakter.gf_von = gf; charakter.kf_von = kf;
                charakter.gf_nach = gfNach; charakter.kf_nach = kfNach;
            }

            // ein Zivilist ist kein Adeliger
            var zivilist = SpielfigurenView.GetSpielfiguren(reich).OfType<Character>()
                .FirstOrDefault(c => CharakterkampfRules.GetKlassenstufe(c) == CharakterkampfRules.KlasseZivilist);
            Assert.True(zivilist != null, "Das eigene Reich hat keinen Zivilisten");

            int zgf = zivilist!.gf_von, zkf = zivilist.kf_von;
            int zgfNach = zivilist.gf_nach, zkfNach = zivilist.kf_nach;
            try {
                zivilist.gf_von = feld.gf;
                zivilist.kf_von = feld.kf;
                zivilist.gf_nach = 0;
                zivilist.kf_nach = 0;

                Assert.False(HeeresRules.BegleitetEinAdliger(heer));
                Assert.False(HeeresRules.IstEroberungsfähig(heer));
            }
            finally {
                zivilist.gf_von = zgf; zivilist.kf_von = zkf;
                zivilist.gf_nach = zgfNach; zivilist.kf_nach = zkfNach;
            }
        }

        /// <summary>
        /// "Unterstuetzen kann nur ein eroberungsfaehiges Heer. Pro Gemark kann ein Heer ein
        /// angegriffenes Heer mit 10 Gutpunkten unterstuetzen." (Regelwerk 5.5.1)
        /// </summary>
        [StaFact]
        public void NurEroberungsfaehigeHeereUnterstuetzen() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var nachbarn = KleinfeldView.GetNachbarn(feld, 1, includeSelf: false)!
                .Where(n => n.IsWasser == false).ToList();
            var reich = ProgramView.SelectedNation!;

            var angegriffen = Heer(2000, 2, feld, reich, nummer: 100);

            // ein zu kleines Heer auf der Nachbargemark unterstuetzt nicht
            var klein = Heer(500, 1, nachbarn[0], reich, nummer: 101);
            Assert.Empty(KampfRules.FindeUnterstützer(angegriffen, [klein]));

            // ein eroberungsfaehiges schon
            var gross = Heer(2000, 1, nachbarn[0], reich, nummer: 102);
            var unterstützer = KampfRules.FindeUnterstützer(angegriffen, [gross]);
            Assert.Single(unterstützer);
            Assert.Equal(nachbarn[0].gf, unterstützer[0].Gemark.gf);

            // zwei Heere auf derselben Gemark zaehlen einmal
            var zweitesAufDerselben = Heer(2000, 1, nachbarn[0], reich, nummer: 103);
            Assert.Single(KampfRules.FindeUnterstützer(angegriffen, [gross, zweitesAufDerselben]));

            // zwei Gemarken zaehlen zweimal - und bringen zusammen 20 Gutpunkte
            var aufDerZweiten = Heer(2000, 1, nachbarn[1], reich, nummer: 104);
            var beide = KampfRules.FindeUnterstützer(angegriffen, [gross, aufDerZweiten]);
            Assert.Equal(2, beide.Count);
            Assert.Equal(20, KampfRules.BerechneVorteile([Kampfvorteil.NachbarUnterstützung], beide.Count));

            // das angegriffene Heer unterstuetzt sich nicht selbst
            Assert.Empty(KampfRules.FindeUnterstützer(angegriffen, [angegriffen]));
            Assert.Empty(KampfRules.FindeUnterstützer(null, [gross]));
        }

        /// <summary>
        /// "Transportierte Heere koennen nicht unterstuetzen." (Regelwerk 5.5.1)
        /// </summary>
        [StaFact]
        public void TransportierteHeereUnterstuetzenNicht() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var nachbar = KleinfeldView.GetNachbarn(feld, 1, includeSelf: false)!.First(n => n.IsWasser == false);
            var reich = ProgramView.SelectedNation!;

            var angegriffen = Heer(2000, 2, feld, reich, nummer: 100);
            var eingeschifft = Heer(2000, 1, nachbar, reich, nummer: 105);
            eingeschifft.auf_Flotte = "301";

            Assert.Empty(KampfRules.FindeUnterstützer(angegriffen, [eingeschifft]));
        }

        /// <summary>
        /// "Der Bau eines Bauwerks kann von einem nicht alliierten, eroberungsfaehigen Heer
        /// verhindert werden, wenn es in einer benachbarten Gemark steht und die Baustelle von
        /// dort aus ungehindert betreten kann" (Regelwerk 1.5)
        /// </summary>
        [StaFact]
        public void EinFeindlichesHeerStoertDieBaustelle() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var nachbar = KleinfeldView.GetNachbarn(feld, 1, includeSelf: false)!.First(n => n.IsWasser == false);
            var gegner = SharedData.Nationen!.First(n => n.Equals(feld.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, feld.Nation, feld));

            // ohne fremde Heere ist alles ruhig
            Assert.Null(ConstructRules.WirdGestört(feld, null, []));

            // ein eroberungsfaehiges feindliches Heer stoert
            var störer = Heer(2000, 1, nachbar, gegner, nummer: 200);
            var störung = ConstructRules.WirdGestört(feld, null, [störer]);
            Assert.True(störung != null && störung.HasErrors);
            Assert.Contains("gestört", störung!.Title);

            // ein zu kleines nicht
            Assert.Null(ConstructRules.WirdGestört(feld, null, [Heer(500, 1, nachbar, gegner, nummer: 201)]));

            // und ein eigenes auch nicht
            Assert.Null(ConstructRules.WirdGestört(feld, null, [Heer(2000, 1, nachbar, feld.Nation!, nummer: 202)]));
        }

        /// <summary>
        /// "Bei Bauwerken AN einer Gemarkseite: In diesem angrenzenden Gemark." (Regelwerk 1.5)
        /// Ein Heer auf einer anderen Seite stoert dort nicht.
        /// </summary>
        [StaFact]
        public void AnEinerGemarkseiteZaehltNurDieseSeite() {
            LadeAlles();
            var feld = FindeLandfeldMitNachbarn();
            var gegner = SharedData.Nationen!.First(n => n.Equals(feld.Nation) == false
                && DiplomatieRules.SindVerfeindet(n, feld.Nation, feld));

            // die Richtung zum ersten Landnachbarn suchen
            Direction? richtung = null;
            KleinFeld? dort = null;
            foreach (Direction kandidat in Enum.GetValues<Direction>()) {
                var position = KartenKoordinaten.GetNachbar(feld, kandidat);
                var nachbar = position == null ? null : KleinfeldView.GetKleinfeld(position);
                if (nachbar != null && nachbar.IsWasser == false) {
                    richtung = kandidat;
                    dort = nachbar;
                    break;
                }
            }
            Assert.True(richtung != null && dort != null, "Kein Landnachbar mit Richtung gefunden");

            var störer = Heer(2000, 1, dort!, gegner, nummer: 200);

            // auf dieser Seite stoert es
            var störung = ConstructRules.WirdGestört(feld, richtung, [störer]);
            Assert.True(störung != null && störung.HasErrors);

            // auf einer anderen Seite nicht
            var andere = Enum.GetValues<Direction>().First(d => d != richtung!.Value);
            Assert.Null(ConstructRules.WirdGestört(feld, andere, [störer]));
        }
    }
}
