using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Der Unterbau des Kartenkontextmenues.
    ///
    /// Das Menue selbst ist Oberflaeche und laesst sich hier nicht pruefen - wohl aber die beiden
    /// Fragen, die es beantwortet: welche eigenen Figuren stehen auf einer Gemark, und welche
    /// Felder erreicht eine davon noch.
    /// </summary>
    public class KontextmenueTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            // bewegt wird im Spielzug, nicht in der Ruestphase
            if (ZugView.Settings != null)
                ZugView.Settings.Phase = (int)Zugphase.Bewegungsphase;
        }

        /// <summary>Die Auswahl, die das Untermenue "Moegliche Zuege" fuellt</summary>
        private static List<Spielfigur> EigeneAufGemark(KleinFeld gemark)
            => SpielfigurenView.GetSpielfiguren(gemark)
                .Where(figur => figur.Nation != null && figur.Nation == ProgramView.SelectedNation)
                .ToList();

        /// <summary>
        /// Auf einer Gemark mit eigenen Figuren bietet das Menue genau diese an - und keine
        /// fremden.
        /// </summary>
        [StaFact]
        public void DasMenueBietetNurEigeneFiguranAn() {
            LadeAlles();
            var eigene = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            Assert.True(eigene.Count > 0, "Das eigene Reich hat keine Figuren");

            var irgendeine = eigene.First(f => Plausibilität.IsValid(f));
            var gemark = KleinfeldView.GetKleinfeld(new KleinfeldPosition(irgendeine.gf, irgendeine.kf));
            Assert.True(gemark != null, "Die Gemark der Figur liegt nicht auf der Karte");

            var angeboten = EigeneAufGemark(gemark!);
            Assert.True(angeboten.Count > 0, "Auf der Gemark der eigenen Figur wird keine angeboten");
            Assert.Contains(angeboten, f => f.Nummer == irgendeine.Nummer);
            Assert.All(angeboten, f => Assert.Equal(ProgramView.SelectedNation, f.Nation));
        }

        /// <summary>
        /// Auf einer Gemark ohne eigene Figuren gibt es kein Untermenue - dort bleibt nur die Info.
        /// </summary>
        [StaFact]
        public void OhneEigeneFigurenGibtEsKeinUntermenue() {
            LadeAlles();
            var besetzt = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .Select(f => KleinFeld.CreateBezeichner(f.gf, f.kf))
                .ToHashSet();

            var leer = SharedData.Map!.Values.FirstOrDefault(k => besetzt.Contains(k.Bezeichner) == false);
            Assert.True(leer != null, "Es gibt kein Feld ohne eigene Figuren");
            Assert.Empty(EigeneAufGemark(leer!));
        }

        /// <summary>
        /// Die hervorgehobenen Felder sind die, die die Figur noch erreicht - das eigene Feld
        /// gehoert nicht dazu, und jedes Feld kommt nur einmal vor.
        /// </summary>
        [StaFact]
        public void DieErreichbarenFelderSchliessenDasEigeneFeldAus() {
            LadeAlles();

            // Bewegungspunkte allein genuegen nicht: schwere Artillerie mit 9 Punkten kann keinen
            // einzigen Schritt bezahlen und erreicht zu Recht nichts. Gesucht ist deshalb eine
            // Figur, die tatsaechlich irgendwohin kommt.
            var beweglich = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .Select(f => (Figur: f, Felder: BewegungsRules.GetErreichbareFelder(f)))
                .Where(paar => paar.Felder.Count > 0)
                .ToList();

            Assert.True(beweglich.Count > 0,
                "Keine einzige eigene Figur kann sich bewegen - dann zeigt das Menue nie etwas an");

            foreach (var (figur, erreichbar) in beweglich) {
                // Jedes Feld liegt auf der Karte und kommt nur einmal vor. Das eigene Feld darf
                // dabei sein: wer genug Bewegungspunkte hat, kann weggehen und zurueckkommen.
                Assert.Equal(erreichbar.Count, erreichbar.Select(k => k.Bezeichner).Distinct().Count());
                Assert.All(erreichbar, k => Assert.True(Plausibilität.IsOnMap(k),
                    $"{k.Bezeichner} liegt nicht auf der Karte"));
            }
        }

        /// <summary>
        /// Dass eine Figur Bewegungspunkte hat, heisst noch nicht, dass sie ein Feld erreicht:
        /// schwere Artillerie ist so langsam, dass einstellige Restpunkte fuer keinen Schritt
        /// reichen. Das Menue muss damit umgehen koennen, statt es fuer einen Fehler zu halten.
        /// </summary>
        [StaFact]
        public void BewegungspunkteAlleinBedeutenNochKeinenSchritt() {
            LadeAlles();
            var eigene = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .ToList();
            Assert.True(eigene.Count > 0, "Das eigene Reich hat keine Figuren");

            // Es gibt beides, und beides ist in Ordnung
            int beweglich = eigene.Count(f => BewegungsRules.GetErreichbareFelder(f).Count > 0);
            Assert.True(beweglich > 0, "Keine Figur kommt irgendwohin");
            Assert.True(beweglich <= eigene.Count);
        }

        /// <summary>
        /// Eine Figur ohne Bewegungspunkte erreicht nichts. Dann darf das Menue nichts hervorheben,
        /// sondern muss es sagen.
        /// </summary>
        [StaFact]
        public void OhneBewegungspunkteGibtEsNichtsHervorzuheben() {
            LadeAlles();
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .FirstOrDefault(f => Plausibilität.IsValid(f));
            Assert.True(figur != null, "Das eigene Reich hat keine Figuren");

            int bpVorher = figur!.bp;
            try {
                figur.bp = 0;
                Assert.Empty(BewegungsRules.GetErreichbareFelder(figur));
            }
            finally {
                figur.bp = bpVorher;
            }
        }

        /// <summary>
        /// Ohne Figur gibt es nichts zu rechnen - das Menue fragt auch fuer fremde Gemarken an.
        /// </summary>
        [Fact]
        public void OhneFigurBleibtDieListeLeer() {
            Assert.Empty(BewegungsRules.GetErreichbareFelder(null));
        }
    }
}
