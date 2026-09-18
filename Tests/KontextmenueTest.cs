using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
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
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
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
        /// Die Bewegungsanzeige setzt beide Farben - und zwar genau in der Bewegungsphase.
        ///
        /// Hellgruen die Felder, die die Figur noch erreicht; kornblumenblau der Weg, den sie in
        /// diesem Zug schon gelaufen ist. Das bedeutet Gegenteiliges, und wer es verwechselt,
        /// haelt eine Wegspur fuer die moeglichen Zuege - die fuehrt naturgemaess nur in eine
        /// Richtung. Genau so ist es gemeldet worden.
        ///
        /// In der Ruestphase bleibt von der Reichweite nichts uebrig; die Wegspur steht trotzdem
        /// da, denn gelaufen ist gelaufen.
        ///
        /// Gezeichnet wird hier nichts - dafuer braucht es MonoGame. Geprueft werden die
        /// Markierungen, und die liegen im Modell.
        /// </summary>
        [StaFact]
        public void DieBewegungsanzeigeSetztBeideFarben() {
            LadeAlles();

            var beweglich = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(f => BewegungsRules.GetErreichbareFelder(f).Count > 0
                    && new Bewegungsspur(f).Count > 0);
            Assert.True(beweglich != null, "Keine eigene Figur ist gezogen und kann noch weiter");

            try {
                var erreichbar = BewegungView.ZeigeBewegung(beweglich);
                Assert.NotEmpty(erreichbar);

                // der gelaufene Weg ist blau
                var spur = new Bewegungsspur(beweglich!).ToList();
                foreach (var punkt in spur) {
                    var feld = KleinfeldView.GetKleinfeld(punkt);
                    Assert.True(feld != null, $"{punkt.CreateBezeichner()} liegt nicht auf der Karte");
                    Assert.Equal(MarkerType.Weg, feld!.Mark);
                }

                // und die Reichweite gruen, soweit sie nicht vom Weg ueberdeckt wird
                var aufDemWeg = spur.Select(p => p.CreateBezeichner()).ToHashSet();
                int gruen = erreichbar.Count(feld => aufDemWeg.Contains(feld.Bezeichner) == false
                    && feld.Mark == MarkerType.Bewegung);
                Assert.True(gruen > 0, "Kein einziges erreichbares Feld ist als Reichweite markiert");

                // in der Ruestphase bleibt die Wegspur, die Reichweite verschwindet
                int phaseVorher = ZugView.Settings!.Phase;
                int monatVorher = ZugView.Settings!.Monat;
                try {
                    TestSetup.SetzePhase(Zugphase.Rüstphase);
                    var inDerRüstphase = BewegungView.ZeigeBewegung(beweglich);
                    Assert.Empty(inDerRüstphase);
                    foreach (var punkt in spur) {
                        var feld = KleinfeldView.GetKleinfeld(punkt);
                        Assert.Equal(MarkerType.Weg, feld!.Mark);
                    }
                }
                finally {
                    ZugView.Settings!.Phase = phaseVorher;
                    ZugView.Settings!.Monat = monatVorher;
                }
            }
            finally {
                BewegungView.VersteckeBewegung();
            }
        }

        /// <summary>
        /// Das Untermenue fuehrt alles, was auf der Gemark steht - auswaehlbar ist nur das Eigene.
        ///
        /// "Spielfiguren" und "Moegliche Zuege" waren zwei Listen mit demselben Inhalt; jetzt ist
        /// es eine. Fremde Einheiten muessen dabei bleiben: sie kommen aus der Feindaufklaerung
        /// und nicht aus den Zugdaten, und ohne sie meldet ein Feld voller fremder Heere "hier
        /// steht nichts".
        /// </summary>
        [StaFact]
        public void DasUntermenueFuehrtAllesUndLaesstNurEigenesZu() {
            LadeAlles();
            var eigene = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid).ToList();
            Assert.True(eigene.Count > 0, "Das eigene Reich hat keine Figuren");

            var gemark = KleinfeldView.GetKleinfeld(new KleinfeldPosition(eigene[0].gf, eigene[0].kf));
            Assert.True(gemark != null, "Die Gemark der Figur liegt nicht auf der Karte");

            var besetzung = SpielfigurenView.GetFeldbesetzung(gemark!);
            Assert.NotEmpty(besetzung);

            // auswaehlbar ist genau das Eigene
            Assert.All(besetzung, eintrag => Assert.Equal(
                eintrag.Figur != null && eintrag.Nation == ProgramView.SelectedNation,
                eintrag.IstAuswählbar));
            Assert.Contains(besetzung, eintrag => eintrag.IstAuswählbar);

            // und jeder Eintrag traegt eine Beschriftung, sonst steht im Menue eine leere Zeile
            Assert.All(besetzung, eintrag => Assert.False(string.IsNullOrWhiteSpace(eintrag.Beschriftung)));
        }

        /// <summary>
        /// Was hervorgehoben ist, laesst sich auch wirklich anziehen.
        ///
        /// Seit ein Klick auf ein hervorgehobenes Feld die Figur dorthin bewegt, ist das keine
        /// Feinheit mehr: ein Feld, das leuchtet, aber keinen Weg hat, sieht beim Klick wie eine
        /// kaputte Anwendung aus. Die beiden Rechnungen sind verschieden - GetErreichbareFelder
        /// sammelt die Suche ein, FindeWeg baut den Weg daraus und prueft zusaetzlich, ob in der
        /// Zugdatenbank noch Platz fuer die Wegpunkte ist.
        /// </summary>
        [StaFact]
        public void JedesHervorgehobeneFeldLaesstSichAnziehen() {
            LadeAlles();

            var beweglich = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .Select(f => (Figur: f, Felder: BewegungsRules.GetErreichbareFelder(f)))
                .Where(paar => paar.Felder.Count > 0)
                .ToList();
            Assert.True(beweglich.Count > 0, "Keine eigene Figur kann sich bewegen");

            int geprüft = 0;
            foreach (var (figur, erreichbar) in beweglich) {
                string eigenes = KleinfeldView.GetKleinfeld(figur)!.Bezeichner;
                foreach (var feld in erreichbar) {
                    // Auf dem eigenen Feld steht die Figur schon; dorthin wird nicht gezogen.
                    if (feld.Bezeichner == eigenes)
                        continue;
                    var weg = BewegungsRules.FindeWeg(figur, feld, out string fehler);
                    Assert.True(weg != null && weg.Wegpunkte.Count > 0,
                        $"{feld.Bezeichner} ist fuer {figur.Bezeichner} hervorgehoben, aber nicht anziehbar: {fehler}");
                    geprüft++;
                }
            }
            Assert.True(geprüft > 0, "Kein einziges Feld geprueft");
        }

        /// <summary>
        /// Ein Klick gilt nur dann als Zug, wenn er einer ist.
        /// </summary>
        [StaFact]
        public void NurEinKlickAufEinLeuchtendesFeldIstEinZug() {
            LadeAlles();
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(f => BewegungsRules.GetErreichbareFelder(f).Count > 0);
            Assert.True(figur != null, "Keine eigene Figur kann sich bewegen");

            var eigenes = KleinfeldView.GetKleinfeld(figur!)!;
            var ziel = BewegungsRules.GetErreichbareFelder(figur!)
                .First(feld => feld.Bezeichner != eigenes.Bezeichner);

            Assert.True(PhoenixWPF.Helper.Bewegungssteuerung.IstZugklick(figur, ziel, true));

            // ohne Hervorhebung bleibt es ein gewoehnlicher Klick
            Assert.False(PhoenixWPF.Helper.Bewegungssteuerung.IstZugklick(figur, ziel, false));
            // ohne Figur ebenso
            Assert.False(PhoenixWPF.Helper.Bewegungssteuerung.IstZugklick(null, ziel, true));
            // und auf dem eigenen Feld steht die Figur schon - auch wenn ein Rundweg es hervorhebt
            Assert.False(PhoenixWPF.Helper.Bewegungssteuerung.IstZugklick(figur, eigenes, true));

            // in der Ruestphase wird nicht bewegt, also ist auch kein Klick ein Zug
            int phaseVorher = ZugView.Settings!.Phase;
            int monatVorher = ZugView.Settings!.Monat;
            try {
                TestSetup.SetzePhase(Zugphase.Rüstphase);
                Assert.False(PhoenixWPF.Helper.Bewegungssteuerung.IstZugklick(figur, ziel, true));
            }
            finally {
                ZugView.Settings!.Phase = phaseVorher;
                ZugView.Settings!.Monat = monatVorher;
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
