using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Hauptstadtverlegung (Regelwerk 1.5.13).
    ///
    /// "Von allen Ruestorten darf nur die Hauptstadt verlegt werden. Die Verlegung muss in eine
    /// bestehende Festung erfolgen und dauert 4 Monate. Im ersten Monat der Verlegung wird aus der
    /// alten Hauptstadt eine Festung ( dies ist bei der Einnahmeberechnung und bei der Ruestung zu
    /// beachten! ). Im vierten Monat wird aus der zuvor bezeichneten Festung die neue Hauptstadt.
    /// Fuer die Hauptstadtverlegung sind im ersten Monat 50.000 GS zu bezahlen."
    ///
    /// Die Tests fassen die echte Karte an - jeder stellt hinterher wieder her, was er geaendert
    /// hat. Geschrieben wird dabei nichts: die Speicherschlange wird im Testlauf nicht geleert.
    /// </summary>
    public class HauptstadtverlegungTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            // Ruestorte aendern sich in der Ruestphase
            if (SharedData.ZugdatenSettings != null)
                SharedData.ZugdatenSettings.Last().Phase = (int)Zugphase.Rüstphase;
        }

        /// <summary>
        /// Merkt sich die Ausbaustufe eines Feldes und setzt sie am Ende wieder.
        /// </summary>
        private sealed class Zustand {
            private readonly List<(KleinFeld Feld, int? Ruestort, int Baupunkte)> _stand = [];

            public Zustand Merke(params KleinFeld?[] felder) {
                foreach (var feld in felder)
                    if (feld != null)
                        _stand.Add((feld, feld.Ruestort, feld.Baupunkte));
                return this;
            }

            public void Stelle_wieder_her() {
                foreach (var (feld, ruestort, baupunkte) in _stand) {
                    feld.Ruestort = ruestort;
                    feld.Baupunkte = baupunkte;
                }
            }
        }

        /// <summary>
        /// Legt soviel in die Schatzkammer, dass die Verlegung bezahlt werden kann, und gibt den
        /// vorherigen Stand zurueck.
        ///
        /// Das Reich im Testbestand hat keine 50.000 GS uebrig - das ist richtig so und wird in
        /// <see cref="DieVerlegungWillBezahltWerden"/> auch geprueft. Fuer alle anderen Tests waere
        /// es nur eine Huerde vor der eigentlichen Regel.
        /// </summary>
        private static int FuelleDieSchatzkammer() {
            var schatz = SchatzkammerView.GetActual();
            int vorher = schatz.Reichschatz;
            schatz.Reichschatz = HauptstadtRules.Kosten * 2;
            return vorher;
        }

        private static void SetzeSchatzkammer(int stand) => SchatzkammerView.GetActual().Reichschatz = stand;

        private static KleinFeld Hauptstadt() {
            var hauptstadt = HauptstadtRules.FindeHauptstadt(ProgramView.SelectedNation);
            Assert.True(hauptstadt != null, "Das eigene Reich hat keine Hauptstadt");
            return hauptstadt!;
        }

        private static KleinFeld Festung() {
            var festungen = HauptstadtRules.FindeFestungen(ProgramView.SelectedNation);
            Assert.True(festungen.Count > 0, "Das eigene Reich hat keine bestehende Festung");
            return festungen[0];
        }

        /// <summary>
        /// Die Zahlen des Regelwerks: 50.000 GS, 4 Monate, und die Baupunkte der beiden
        /// Ausbaustufen - Festung 3.000 (1.5.7), Hauptstadt 5.000 (1.5.8).
        /// </summary>
        [StaFact]
        public void DieZahlenStehenSoImRegelwerk() {
            LadeAlles();

            Assert.Equal(50_000, HauptstadtRules.Kosten);
            Assert.Equal(4, HauptstadtRules.Dauer);

            // "dauert 4 Monate ... Im vierten Monat wird aus der zuvor bezeichneten Festung die
            // neue Hauptstadt" - zwischen dem ersten und dem vierten liegen drei Monate
            Assert.Equal(615, HauptstadtRules.GetAbschlussmonat(612));

            Assert.Equal(3000, HauptstadtRules.GetStufe(HauptstadtRules.Festung)?.Baupunkte);
            Assert.Equal(5000, HauptstadtRules.GetStufe(HauptstadtRules.Hauptstadt)?.Baupunkte);
        }

        /// <summary>
        /// "Jedes Reich darf nur eine Hauptstadt haben" (Regelwerk 1.5.8) - und nur sie darf
        /// verlegt werden.
        /// </summary>
        [StaFact]
        public void JedesReichHatGenauEineHauptstadt() {
            LadeAlles();
            var reich = ProgramView.SelectedNation;

            // Gefragt ist die Bezeichnung und nicht der Zustand: eine zusammengeschossene
            // Hauptstadt bleibt die Hauptstadt des Reiches (siehe BeschaedigterRuestortTest).
            var hauptstadt = Hauptstadt();
            Assert.True(HauptstadtRules.IstHauptstadt(RuestortRules.GetSollstufe(hauptstadt)));

            // es gibt genau eine
            int gezaehlt = SharedData.Map!.Values.Count(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(reich)
                && HauptstadtRules.IstHauptstadt(RuestortRules.GetSollstufe(gemark)));
            Assert.Equal(1, gezaehlt);

            // und jedes andere Reich auch: die Karte fuehrt fuer jedes Reich genau eine
            // Hauptstadt oder Festungshauptstadt - "Jedes Reich darf nur eine Hauptstadt ODER eine
            // Festungshauptstadt besitzen" (Regelwerk 1.5.9)
            var jeReich = SharedData.Map!.Values
                .Where(gemark => gemark.Nation != null
                    && HauptstadtRules.IstHauptstadt(RuestortRules.GetSollstufe(gemark)))
                .GroupBy(gemark => gemark.Nation!.Reich)
                .ToList();
            Assert.True(jeReich.Count > 1, "Die Karte kennt nur ein Reich mit Hauptstadt");
            Assert.All(jeReich, gruppe => Assert.Single(gruppe));

            // Festungshauptstaedte zaehlen mit - sonst faende die Suche sie nicht
            Assert.Contains(SharedData.Map.Values, gemark =>
                RuestortRules.GetSollstufe(gemark)?.Ruestort == HauptstadtRules.Festungshauptstadt
                && HauptstadtRules.IstHauptstadt(RuestortRules.GetSollstufe(gemark)));

            // eine Festung ist keine Hauptstadt, eine Hauptstadt keine Festung
            var festung = Festung();
            Assert.True(HauptstadtRules.IstFestung(BauwerkeView.GetRüstortNachKarte(festung)));
            Assert.False(HauptstadtRules.IstHauptstadt(BauwerkeView.GetRüstortNachKarte(festung)));
            Assert.False(HauptstadtRules.IstFestung(RuestortRules.GetSollstufe(hauptstadt)));
        }

        /// <summary>
        /// "Die Verlegung muss in eine bestehende Festung erfolgen."
        ///
        /// Eine Stadt ist keine, und eine zusammengeschossene Festung besteht nicht mehr: "Eine
        /// beschossene Festung wird mit 2499 Baupunkten zur Stadt" (Regelwerk 1.5.7).
        /// </summary>
        [StaFact]
        public void DasZielMussEineBestehendeFestungSein() {
            LadeAlles();
            var festung = Festung();
            var zustand = new Zustand().Merke(festung);
            int schatz = FuelleDieSchatzkammer();
            try {
                var möglich = HauptstadtRules.PrüfeBeginn(festung);
                Assert.False(möglich.HasErrors, $"{möglich.Title}: {möglich.Message}");

                // dieselbe Festung, nur beschaedigt
                festung.Baupunkte -= 1;
                Assert.False(HauptstadtRules.IstBestehendeFestung(festung));
                var beschaedigt = HauptstadtRules.PrüfeBeginn(festung);
                Assert.True(beschaedigt.HasErrors);
                Assert.Contains("bestehende Festung", beschaedigt.Title);

                // und dasselbe Feld als Stadt
                zustand.Stelle_wieder_her();
                Assert.True(HauptstadtRules.SetzeBezeichnung(festung, "Stadt"));
                Assert.True(HauptstadtRules.PrüfeBeginn(festung).HasErrors);

                // in die eigene Hauptstadt wird nicht verlegt
                var hierher = HauptstadtRules.PrüfeBeginn(Hauptstadt());
                Assert.True(hierher.HasErrors);
                Assert.Contains("schon", hierher.Title);

                Assert.True(HauptstadtRules.PrüfeBeginn(null).HasErrors);
            }
            finally {
                zustand.Stelle_wieder_her();
                SetzeSchatzkammer(schatz);
            }
        }

        /// <summary>
        /// "Fuer die Hauptstadtverlegung sind im ersten Monat 50.000 GS zu bezahlen."
        ///
        /// Wer sie nicht hat, verlegt nicht.
        /// </summary>
        [StaFact]
        public void DieVerlegungWillBezahltWerden() {
            LadeAlles();
            var festung = Festung();
            int schatz = SchatzkammerView.GetActual().Reichschatz;
            try {
                SetzeSchatzkammer(HauptstadtRules.Kosten - 1);
                var zuTeuer = HauptstadtRules.PrüfeBeginn(festung);
                Assert.True(zuTeuer.HasErrors);
                Assert.Contains("Schatzkammer", zuTeuer.Title);

                SetzeSchatzkammer(HauptstadtRules.Kosten);
                Assert.False(HauptstadtRules.PrüfeBeginn(festung).HasErrors);
            }
            finally {
                SetzeSchatzkammer(schatz);
            }
        }

        /// <summary>
        /// "Im ersten Monat der Verlegung wird aus der alten Hauptstadt eine Festung ( dies ist bei
        /// der Einnahmeberechnung und bei der Ruestung zu beachten! )".
        /// </summary>
        [StaFact]
        public void ImErstenMonatWirdAusDerAltenHauptstadtEineFestung() {
            LadeAlles();
            var hauptstadt = Hauptstadt();
            var festung = Festung();
            var zustand = new Zustand().Merke(hauptstadt, festung);
            try {
                // eine Hauptstadt in vollem Stand - die des Testbestandes ist beschaedigt und
                // traegt schon vorher nur eine Festung
                hauptstadt.Baupunkte = 5000;
                int einnahmenVorher = EinnahmenView.GetGebäudeEinnahmen(hauptstadt);
                var kapazitaetVorher = RuestRules.GetKapazität(hauptstadt);

                var alte = HauptstadtRules.Beginne(festung);
                Assert.Same(hauptstadt, alte);

                Assert.Equal("Festung", RuestortRules.GetSollstufe(hauptstadt)?.Ruestort);

                // die Baupunkte werden auf die Festung gedeckelt und nie erhoeht
                Assert.Equal(3000, hauptstadt.Baupunkte);

                // "dies ist bei der Einnahmeberechnung und bei der Ruestung zu beachten" - die
                // Anwendung beachtet es von selbst, weil beide den Ruestort der Karte lesen
                Assert.Equal(5000, einnahmenVorher);
                Assert.Equal(3000, EinnahmenView.GetGebäudeEinnahmen(hauptstadt));
                Assert.True(RuestRules.GetKapazität(hauptstadt).Goldstücke < kapazitaetVorher.Goldstücke);

                // das Ziel ist noch unveraendert - es wird erst im vierten Monat zur Hauptstadt
                Assert.True(HauptstadtRules.IstBestehendeFestung(festung));
            }
            finally {
                zustand.Stelle_wieder_her();
            }
        }

        /// <summary>
        /// Verlegt wird auf eigenes Gebiet und in der Ruestphase: Ruestorte "koennen generell nur
        /// am Ende eines Monats errichtet, ausgebaut oder repariert werden" (Regelwerk 1.5).
        /// </summary>
        [StaFact]
        public void NurAufEigenemGebietUndInDerRuestphase() {
            LadeAlles();
            int schatz = FuelleDieSchatzkammer();
            try {
                var fremdeFestung = SharedData.Map!.Values.FirstOrDefault(gemark =>
                    gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation) == false
                    && HauptstadtRules.IstFestung(BauwerkeView.GetRüstortNachKarte(gemark)));
                Assert.True(fremdeFestung != null, "Die Karte kennt keine fremde Festung");
                Assert.True(HauptstadtRules.PrüfeBeginn(fremdeFestung).HasErrors);

                var festung = Festung();
                SharedData.ZugdatenSettings!.Last().Phase = (int)Zugphase.Bewegungsphase;
                var falschePhase = HauptstadtRules.PrüfeBeginn(festung);
                Assert.True(falschePhase.HasErrors);
                Assert.Contains("Rüstphase", falschePhase.Title);

                SharedData.ZugdatenSettings.Last().Phase = (int)Zugphase.Rüstphase;
                Assert.False(HauptstadtRules.PrüfeBeginn(festung).HasErrors);
            }
            finally {
                SharedData.ZugdatenSettings!.Last().Phase = (int)Zugphase.Rüstphase;
                SetzeSchatzkammer(schatz);
            }
        }

        /// <summary>
        /// Die Baupunkte werden gedeckelt, nicht erfunden.
        ///
        /// Eine unbeschaedigte Hauptstadt hat 5.000 Baupunkte und faellt als Festung auf 3.000
        /// (Regelwerk 1.5.7, 1.5.8). Eine, der schon Baupunkte fehlen, behaelt ihren Stand - sie
        /// wird zur ebenso beschaedigten Festung.
        /// </summary>
        [StaFact]
        public void KeineBaupunkteWerdenErfunden() {
            LadeAlles();
            var hauptstadt = Hauptstadt();
            var festung = Festung();
            var zustand = new Zustand().Merke(hauptstadt, festung);
            try {
                // eine Hauptstadt in vollem Stand
                hauptstadt.Baupunkte = 5000;
                HauptstadtRules.Beginne(festung);
                Assert.Equal(3000, hauptstadt.Baupunkte);

                // und eine, der schon etwas fehlt
                zustand.Stelle_wieder_her();
                hauptstadt.Baupunkte = 2500;
                HauptstadtRules.Beginne(festung);
                Assert.Equal(2500, hauptstadt.Baupunkte);
                Assert.False(HauptstadtRules.IstBestehendeFestung(hauptstadt));
            }
            finally {
                zustand.Stelle_wieder_her();
            }
        }

        /// <summary>
        /// "Im vierten Monat wird aus der zuvor bezeichneten Festung die neue Hauptstadt."
        ///
        /// Dazwischen hat das Reich keine Hauptstadt - das steht so im Regelwerk.
        /// </summary>
        [StaFact]
        public void ImViertenMonatWirdDieFestungZurHauptstadt() {
            LadeAlles();
            var reich = ProgramView.SelectedNation;
            var hauptstadt = Hauptstadt();
            var festung = Festung();
            var zustand = new Zustand().Merke(hauptstadt, festung);
            try {
                HauptstadtRules.Beginne(festung);

                // die drei Monate dazwischen: das Reich hat keine Hauptstadt
                Assert.Null(HauptstadtRules.FindeHauptstadt(reich));
                Assert.False(HauptstadtRules.PrüfeAbschluss(festung).HasErrors);

                int baupunkteDerFestung = festung.Baupunkte;
                Assert.True(HauptstadtRules.SchliesseAb(festung));

                Assert.Equal("Hauptstadt", RuestortRules.GetSollstufe(festung)?.Ruestort);
                Assert.Same(festung, HauptstadtRules.FindeHauptstadt(reich));

                // die Bezeichnung wird verlegt, keine Baupunkte erfunden: der neuen Hauptstadt
                // fehlen die 2.000 Baupunkte, die eine Festung von einer Hauptstadt trennen
                Assert.Equal(baupunkteDerFestung, festung.Baupunkte);
                Assert.Equal(5000 - 3000, RuestortRules.GetSchaden(festung));

                // die alte ist eine Festung geblieben
                Assert.Equal("Festung", RuestortRules.GetSollstufe(hauptstadt)?.Ruestort);
            }
            finally {
                zustand.Stelle_wieder_her();
            }
        }

        /// <summary>
        /// Solange eine Hauptstadt steht, wird nicht abgeschlossen - und ohne Hauptstadt nicht
        /// begonnen.
        /// </summary>
        [StaFact]
        public void JederSchrittBrauchtSeinenZustand() {
            LadeAlles();
            var hauptstadt = Hauptstadt();
            var festung = Festung();
            var zustand = new Zustand().Merke(hauptstadt, festung);
            try {
                // vorher: der Abschluss geht nicht, das Reich hat noch seine Hauptstadt
                var zuFrueh = HauptstadtRules.PrüfeAbschluss(festung);
                Assert.True(zuFrueh.HasErrors);
                Assert.Contains("schon eine Hauptstadt", zuFrueh.Title);

                HauptstadtRules.Beginne(festung);

                // nachher: der Beginn geht nicht mehr, es gibt keine Hauptstadt zu verlegen
                var nichtsMehrDa = HauptstadtRules.PrüfeBeginn(hauptstadt);
                Assert.True(nichtsMehrDa.HasErrors);
                Assert.Contains("keine Hauptstadt", nichtsMehrDa.Title);
            }
            finally {
                zustand.Stelle_wieder_her();
            }
        }

        /// <summary>
        /// Der Befehl liest beide Schritte.
        /// </summary>
        [Fact]
        public void DerBefehlVerstehtBeideSchritte() {
            var parser = new HauptstadtverlegungCommandParser();

            Assert.True(parser.ParseCommand("Verlege die Hauptstadt nach 100/5", out var beginn));
            var ersterMonat = Assert.IsType<HauptstadtverlegungCommand>(beginn);
            Assert.False(ersterMonat.IstAbschluss);
            Assert.Equal("100/5", ersterMonat.Location?.CreateBezeichner());

            Assert.True(parser.ParseCommand("Vollende die Hauptstadtverlegung nach 100/5", out var abschluss));
            var vierterMonat = Assert.IsType<HauptstadtverlegungCommand>(abschluss);
            Assert.True(vierterMonat.IstAbschluss);

            // ohne "die" und mit anderer Praeposition
            Assert.True(parser.ParseCommand("Verlege Hauptstadt in 12/3", out _));

            Assert.False(parser.ParseCommand("Verlege die Burg nach 100/5", out _));
            Assert.False(parser.ParseCommand("Verlege die Hauptstadt", out _));
        }

        /// <summary>
        /// Der ganze Befehl: gelesen, geprueft, ausgefuehrt und zurueckgenommen.
        /// </summary>
        [StaFact]
        public void DerBefehlLaeuftDurchUndLaesstSichZuruecknehmen() {
            LadeAlles();
            var hauptstadt = Hauptstadt();
            var festung = Festung();
            var zustand = new Zustand().Merke(hauptstadt, festung);
            int schatz = FuelleDieSchatzkammer();
            try {
                var parser = new HauptstadtverlegungCommandParser();
                Assert.True(parser.ParseCommand(
                    $"Verlege die Hauptstadt nach {festung.gf}/{festung.kf}", out var befehl));
                var verlegung = Assert.IsType<HauptstadtverlegungCommand>(befehl);

                Assert.False(verlegung.CheckPreconditions().HasErrors);
                Assert.False(verlegung.ExecuteCommand().HasErrors);

                // geaendert hat sich die alte Hauptstadt, nicht das Ziel
                Assert.Equal("Festung", RuestortRules.GetSollstufe(hauptstadt)?.Ruestort);
                Assert.True(HauptstadtRules.IstBestehendeFestung(festung));

                Assert.True(verlegung.CanUndo);
                Assert.False(verlegung.UndoCommand().HasErrors);
                Assert.Equal("Hauptstadt", RuestortRules.GetSollstufe(hauptstadt)?.Ruestort);
                Assert.Same(hauptstadt, HauptstadtRules.FindeHauptstadt(ProgramView.SelectedNation));

                // und jetzt der vierte Monat, von Hand vorbereitet
                HauptstadtRules.Beginne(festung);
                Assert.True(parser.ParseCommand(
                    $"Vollende die Hauptstadtverlegung nach {festung.gf}/{festung.kf}", out var zweiter));
                var vollendung = Assert.IsType<HauptstadtverlegungCommand>(zweiter);

                Assert.False(vollendung.ExecuteCommand().HasErrors);
                Assert.Same(festung, HauptstadtRules.FindeHauptstadt(ProgramView.SelectedNation));

                Assert.False(vollendung.UndoCommand().HasErrors);
                Assert.True(HauptstadtRules.IstBestehendeFestung(festung));
            }
            finally {
                zustand.Stelle_wieder_her();
                SetzeSchatzkammer(schatz);
            }
        }
    }
}
