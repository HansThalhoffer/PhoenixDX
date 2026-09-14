using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Wo kommt es zum Kampf? (Regelwerk 5)
    ///
    /// Zwei Fragen stecken darin: wer steht ueberhaupt zusammen auf einer Gemark, und wer davon
    /// ist verfeindet. Die zweite entscheidet das Regelwerk so: "Es gibt nur Gegner oder
    /// Alliierte. Neutral gibt es nicht. Ein Gegner wird zum Alliierten wenn die
    /// Kuestengewaesserregel, bzw. Wegerecht ausgesprochen wurde, in Abhaengigkeit des jeweiligen
    /// Gelaendes (Wasser oder Land)."
    /// </summary>
    public class KonflikterkennungTest {

        /// <summary>
        /// Der Schluessel der Diplomatietabelle wird beim Laden aus den aufgeloesten Reichsnamen
        /// gebildet. Sind die Reiche noch nicht geladen, loesen alle Zeilen auf dasselbe
        /// Ersatzreich auf und die ganze Tabelle faellt auf eine einzige Zeile zusammen.
        ///
        /// Die Anwendung laedt in der richtigen Reihenfolge (Main.StartInstance: erst PZE, dann
        /// die Hintergrunddaten der Karte). Im Testlauf haengt es davon ab, welcher Test zuerst
        /// geladen hat - deshalb hier notfalls noch einmal in der richtigen Reihenfolge.
        /// </summary>
        private static void LadeMitDiplomatie() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);

            if (SharedData.Diplomatie != null && SharedData.Diplomatie.Count > 1)
                return;

            TestSetup.LoadKarte(true);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Die Diplomatietabelle traegt mehr als eine Zeile - sonst ist sie beim Laden
        /// zusammengefallen und jede Aussage ueber Rechte waere wertlos.
        /// </summary>
        [StaFact]
        public void DieDiplomatietabelleIstVollstaendigGeladen() {
            LadeMitDiplomatie();
            Assert.NotNull(SharedData.Diplomatie);
            Assert.True(SharedData.Diplomatie!.Count > 1,
                $"Die Diplomatietabelle hat nur {SharedData.Diplomatie.Count} Zeile(n) - "
                + "vermutlich wurde sie vor den Reichen geladen");
        }

        /// <summary>
        /// Ein Paar Reiche, das sich Kuestenrecht, aber kein Wegerecht eingeraeumt hat - der Fall
        /// aus dem Beispiel des Regelwerks.
        /// </summary>
        private static (Nation Geber, Nation Empfänger)? FindeKüstenrechtOhneWegerecht() {
            foreach (var zeile in SharedData.Diplomatie!.Values) {
                if (zeile.ReferenzNation.Equals(zeile.Nation))
                    continue;
                if (zeile.Kuestenrecht <= 0 || zeile.Wegerecht > 0 || zeile.Wegerecht_von > 0)
                    continue;

                var gegenrichtung = SharedData.Diplomatie.Values.FirstOrDefault(
                    g => g.ReferenzNation.Equals(zeile.Nation) && g.Nation.Equals(zeile.ReferenzNation));
                if (gegenrichtung != null && (gegenrichtung.Wegerecht > 0 || gegenrichtung.Wegerecht_von > 0))
                    continue;

                return (zeile.ReferenzNation, zeile.Nation);
            }
            return null;
        }

        private static KleinFeld FindeGemark(bool wasser, Nation? eigentümer = null) {
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf => kf.IsWasser == wasser
                && (eigentümer == null || (kf.Nation != null && kf.Nation.Equals(eigentümer))));
            Assert.True(gemark != null, $"Auf der Karte fehlt eine Gemark ({(wasser ? "Wasser" : "Land")})");
            return gemark!;
        }

        private static Krieger Heer(Nation reich, KleinFeld gemark, int nummer)
            => new() { Nation = reich, gf_von = gemark.gf, kf_von = gemark.kf, staerke = 1000, Nummer = nummer };

        private static Schiffe Flotte(Nation reich, KleinFeld gemark, int nummer)
            => new() { Nation = reich, gf_von = gemark.gf, kf_von = gemark.kf, staerke = 10, Nummer = nummer };

        /// <summary>
        /// Das Recht gilt nur im passenden Gelaende: "Eine Flotte von B im Kuestengewaesser ist
        /// somit ein Alliierter, ein Landheer von B auf Land von A aber weiterhin ein Gegner."
        /// </summary>
        [StaFact]
        public void DasRechtGiltNurImPassendenGelaende() {
            LadeMitDiplomatie();
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null,
                "Im Datenbestand gibt es kein Reichspaar mit Kuestenrecht, aber ohne Wegerecht");

            var (geber, empfänger) = paar!.Value;
            var wasser = FindeGemark(wasser: true);
            var land = FindeGemark(wasser: false);

            Assert.True(DiplomatieRules.SindAlliiert(geber, empfänger, wasser),
                "Auf dem Wasser greift das Kuestenrecht nicht");
            Assert.True(DiplomatieRules.SindVerfeindet(geber, empfänger, land),
                "An Land gilt das Kuestenrecht - dort braucht es das Wegerecht");
        }

        /// <summary>
        /// Beide Seiten fuehren eigene Zeilen, und im Datenbestand ist regelmaessig nur eine davon
        /// gepflegt. Ein Recht zaehlt deshalb auch dann, wenn nur der Empfaenger es vermerkt hat.
        /// </summary>
        [StaFact]
        public void EinRechtZaehltAuchWennNurDerEmpfaengerEsFuehrt() {
            LadeMitDiplomatie();

            // eine Zeile, in der ein Reich ein Wegerecht erhalten hat, ohne dass der Geber es in
            // seiner eigenen Zeile vermerkt haette
            var zeile = SharedData.Diplomatie!.Values.FirstOrDefault(z => {
                if (z.ReferenzNation.Equals(z.Nation) || z.Wegerecht_von <= 0)
                    return false;
                var desGebers = SharedData.Diplomatie.Values.FirstOrDefault(
                    g => g.ReferenzNation.Equals(z.Nation) && g.Nation.Equals(z.ReferenzNation));
                return desGebers == null || desGebers.Wegerecht <= 0;
            });
            Assert.True(zeile != null,
                "Im Datenbestand gibt es kein Wegerecht, das nur der Empfaenger fuehrt");

            // der Geber ist das Reich, von dem es kam - die Zeile gehoert dem Empfaenger
            Assert.True(DiplomatieRules.HatWegerecht(zeile!.Nation, zeile.ReferenzNation),
                "Das erhaltene Wegerecht wurde nicht gefunden");
        }

        /// <summary>
        /// Ein Reich ist mit sich selbst nie verfeindet, und ohne Gegenueber gibt es keine Aussage.
        /// </summary>
        [StaFact]
        public void MitSichSelbstIstNiemandVerfeindet() {
            LadeMitDiplomatie();
            var eines = SharedData.Nationen!.First();
            var land = FindeGemark(wasser: false);

            Assert.True(DiplomatieRules.SindAlliiert(eines, eines, land));
            Assert.False(DiplomatieRules.SindVerfeindet(eines, eines, land));
            Assert.False(DiplomatieRules.SindVerfeindet(eines, null, land));
            Assert.False(DiplomatieRules.SindVerfeindet(null, eines, land));
        }

        /// <summary>
        /// Zwei verfeindete Reiche auf einer Gemark ergeben einen Konflikt - mit beiden Parteien
        /// und ihren Figuren.
        /// </summary>
        [StaFact]
        public void ZweiVerfeindeteReicheAufEinerGemarkErgebenEinenKonflikt() {
            LadeMitDiplomatie();
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null);
            var (eines, anderes) = paar!.Value;
            var land = FindeGemark(wasser: false);

            var konflikte = KonfliktRules.FindeKonflikte([
                Heer(eines, land, 1),
                Heer(anderes, land, 2),
            ]);

            Assert.Single(konflikte);
            var konflikt = konflikte[0];
            Assert.Equal(land.gf, konflikt.Gemark.gf);
            Assert.Equal(land.kf, konflikt.Gemark.kf);
            Assert.Equal(KonfliktRules.Kampfart.Nahkampf, konflikt.Art);
            Assert.Equal(2, konflikt.Parteien.Count);
            Assert.Single(konflikt.Gegnerschaften);
            Assert.All(konflikt.Parteien, partei => Assert.Single(partei.Figuren));
            Assert.All(konflikt.Parteien, partei => Assert.Equal(1000, partei.Heeresstärke));

            // dasselbe Paar auf dem Wasser ist verbuendet: dort gilt das Kuestenrecht
            var wasser = FindeGemark(wasser: true);
            Assert.Empty(KonfliktRules.FindeKonflikte([
                Flotte(eines, wasser, 1),
                Flotte(anderes, wasser, 2),
            ]));
        }

        /// <summary>
        /// Auf dem Wasser wird es eine Seeschlacht, an Land ein Nahkampf.
        /// </summary>
        [StaFact]
        public void AufDemWasserWirdEsEineSeeschlacht() {
            LadeMitDiplomatie();
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null);
            var (eines, _) = paar!.Value;

            // ein drittes Reich, das mit dem ersten weder Wege- noch Kuestenrecht teilt
            var fremdes = SharedData.Nationen!.FirstOrDefault(n => n.Equals(eines) == false
                && DiplomatieRules.HatKüstenrecht(n, eines) == false
                && DiplomatieRules.HatKüstenrecht(eines, n) == false);
            Assert.True(fremdes != null, "Es gibt kein Reich ohne Kuestenrecht zum ersten");

            var wasser = FindeGemark(wasser: true);
            var konflikte = KonfliktRules.FindeKonflikte([
                Flotte(eines, wasser, 1),
                Flotte(fremdes!, wasser, 2),
            ]);

            Assert.Single(konflikte);
            Assert.Equal(KonfliktRules.Kampfart.Seeschlacht, konflikte[0].Art);
        }

        /// <summary>
        /// Wer allein auf seiner Gemark steht, kaempft nicht - und zwei Heere desselben Reiches
        /// auch nicht gegeneinander.
        /// </summary>
        [StaFact]
        public void OhneGegenueberGibtEsKeinenKonflikt() {
            LadeMitDiplomatie();
            var eines = SharedData.Nationen!.First();
            var land = FindeGemark(wasser: false);
            var anderswo = SharedData.Map!.Values.First(kf => kf.IsWasser == false && kf.Key != land.Key);

            Assert.Empty(KonfliktRules.FindeKonflikte([Heer(eines, land, 1)]));
            Assert.Empty(KonfliktRules.FindeKonflikte([Heer(eines, land, 1), Heer(eines, land, 2)]));
            Assert.Empty(KonfliktRules.FindeKonflikte(null));

            // und auf verschiedenen Gemarken treffen sie sich nicht
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null);
            Assert.Empty(KonfliktRules.FindeKonflikte([
                Heer(paar!.Value.Geber, land, 1),
                Heer(paar!.Value.Empfänger, anderswo, 2),
            ]));
        }

        /// <summary>
        /// Auf einer Gemark koennen Reiche stehen, die untereinander verbuendet und mit einem
        /// dritten verfeindet sind. Gezaehlt wird deshalb jedes Paar einzeln.
        /// </summary>
        [StaFact]
        public void JedesPaarWirdEinzelnGeprueft() {
            LadeMitDiplomatie();
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null);
            var (eines, anderes) = paar!.Value;
            var land = FindeGemark(wasser: false);

            // drei Reiche an Land, wo das Kuestenrecht nichts nuetzt: drei Gegnerschaften
            var drittes = SharedData.Nationen!.First(n => n.Equals(eines) == false && n.Equals(anderes) == false
                && DiplomatieRules.SindVerfeindet(n, eines, land)
                && DiplomatieRules.SindVerfeindet(n, anderes, land));

            var konflikte = KonfliktRules.FindeKonflikte([
                Heer(eines, land, 1),
                Heer(anderes, land, 2),
                Heer(drittes, land, 3),
            ]);

            Assert.Single(konflikte);
            Assert.Equal(3, konflikte[0].Parteien.Count);
            Assert.Equal(3, konflikte[0].Gegnerschaften.Count);
        }

        /// <summary>
        /// Stehen sich nur Charaktere gegenueber, ist es ein Charakterkampf: "Zum Charakterkampf
        /// kommt es, wenn sich Charaktere verfeindeter Reiche in der selben Gemark befinden."
        /// (Regelwerk 5.3)
        /// </summary>
        [StaFact]
        public void NurCharaktereErgebenEinenCharakterkampf() {
            LadeMitDiplomatie();
            var paar = FindeKüstenrechtOhneWegerecht();
            Assert.True(paar != null);
            var (eines, anderes) = paar!.Value;
            var land = FindeGemark(wasser: false);

            var konflikte = KonfliktRules.FindeKonflikte([
                new Character { Nation = eines, gf_von = land.gf, kf_von = land.kf, Nummer = 1 },
                new Character { Nation = anderes, gf_von = land.gf, kf_von = land.kf, Nummer = 2 },
            ]);

            Assert.Single(konflikte);
            Assert.Equal(KonfliktRules.Kampfart.Charakterkampf, konflikte[0].Art);

            // steht auf einer Seite ein Heer, ist es ein Nahkampf
            var gemischt = KonfliktRules.FindeKonflikte([
                new Character { Nation = eines, gf_von = land.gf, kf_von = land.kf, Nummer = 1 },
                Heer(eines, land, 2),
                Heer(anderes, land, 3),
            ]);
            Assert.Single(gemischt);
            Assert.Equal(KonfliktRules.Kampfart.Nahkampf, gemischt[0].Art);
        }
    }
}
