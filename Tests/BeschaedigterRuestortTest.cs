using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Ein beschaedigter Ruestort zaehlt als das, was seine Baupunkte tragen (Regelwerk 1.5).
    ///
    /// "Eine von Grund auf neu errichtete Stadt, die erst 1750 Baupunkte enthaelt, bietet nur die
    /// Ruestkapazitaet der schon fertigen Burg!" und "Eine Festung, die innerhalb einer Runde von
    /// ihren 3000 Baupunkten 1500 verliert, ist nur noch eine halbfertige Stadt ... man kann bis
    /// zur erneuten Fertigstellung der Stadt nur die Ruestkapazitaet einer Burg nutzen."
    ///
    /// Die Karte fuehrt beides: die Spalte Ruestort sagt, was dort stehen soll, die Spalte
    /// Baupunkte, was dort steht. Einnahmen, Ruestung, Kampf und Beute fragen nach dem zweiten.
    ///
    /// Die Tests fassen die echte Karte an und stellen hinterher wieder her, was sie geaendert
    /// haben.
    /// </summary>
    public class BeschaedigterRuestortTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Sucht eine unbeschaedigte Festung des eigenen Reiches.
        /// </summary>
        private static KleinFeld Festung() {
            var festung = SharedData.Map!.Values.FirstOrDefault(gemark =>
                gemark.Nation != null && gemark.Nation.Equals(ProgramView.SelectedNation)
                && RuestortRules.GetSollstufe(gemark)?.Ruestort == "Festung"
                && RuestortRules.GetSchaden(gemark) == 0);
            Assert.True(festung != null, "Das eigene Reich hat keine unbeschaedigte Festung");
            return festung!;
        }

        /// <summary>
        /// Die Zwischenstufen tragen den Namen der Stufe, aus der sie wachsen - und die Werte der
        /// fertigen Stufe darunter.
        /// </summary>
        [StaFact]
        public void DieZwischenstufenGehoerenZurFertigenStufeDarunter() {
            LadeAlles();

            Assert.Equal("Stadt", RuestortRules.GetGrundstufe("Stadt-II"));
            Assert.Equal("Burg", RuestortRules.GetGrundstufe("Burg-III"));
            Assert.Equal("Festung", RuestortRules.GetGrundstufe("Festung"));
            Assert.Null(RuestortRules.GetGrundstufe((string?)null));
            Assert.Null(RuestortRules.GetGrundstufe("  "));

            // "man kann bis zur erneuten Fertigstellung der Stadt nur die Ruestkapazitaet einer
            // Burg nutzen" - so steht es auch in der Referenztabelle
            var burg = BauwerkeView.GetErreichteStufe(1000);
            var burgIII = BauwerkeView.GetErreichteStufe(1750);
            Assert.Equal("Burg", burg?.Ruestort);
            Assert.Equal("Burg-III", burgIII?.Ruestort);
            Assert.Equal(burg?.KapazitätTruppen, burgIII?.KapazitätTruppen);
        }

        /// <summary>
        /// Die erreichte Stufe ist die hoechste, die die Baupunkte tragen.
        /// </summary>
        [StaFact]
        public void DieErreichteStufeHaengtAnDenBaupunkten() {
            LadeAlles();

            Assert.Equal("Burg", BauwerkeView.GetErreichteStufe(1000)?.Ruestort);
            Assert.Equal("Burg", BauwerkeView.GetErreichteStufe(1249)?.Ruestort);
            Assert.Equal("Stadt", BauwerkeView.GetErreichteStufe(2000)?.Ruestort);
            Assert.Equal("Stadt-II", BauwerkeView.GetErreichteStufe(2500)?.Ruestort);
            Assert.Equal("Festung", BauwerkeView.GetErreichteStufe(3000)?.Ruestort);
            Assert.Equal("Festung", BauwerkeView.GetErreichteStufe(4999)?.Ruestort);
            Assert.Equal("Hauptstadt", BauwerkeView.GetErreichteStufe(5000)?.Ruestort);

            // unterhalb der Burg liegen die Baustufen des Dorfes - eine auf 750 Baupunkte
            // zusammengeschossene Burg ist ein Dorf
            Assert.Equal("Dorf-III", BauwerkeView.GetErreichteStufe(999)?.Ruestort);
            Assert.Equal("Dorf-I", BauwerkeView.GetErreichteStufe(250)?.Ruestort);

            // was fuer keine Stufe reicht, ist zerstoert
            Assert.Null(BauwerkeView.GetErreichteStufe(249));
            Assert.Null(BauwerkeView.GetErreichteStufe(0));
            Assert.Null(BauwerkeView.GetErreichteStufe(-1));
        }

        /// <summary>
        /// Eine beschaedigte Festung bringt die Einnahmen, die Ruestkapazitaet, den Kampfvorteil
        /// und die Beute einer Stadt - und sie laesst sich belagern, was eine Festung nicht kann
        /// (Regelwerk 1.5.7).
        /// </summary>
        [StaFact]
        public void EineBeschaedigteFestungIstEineStadt() {
            LadeAlles();
            var festung = Festung();
            int baupunkte = festung.Baupunkte;
            try {
                Assert.Equal("Festung", BauwerkeView.GetRüstortNachKarte(festung)?.Ruestort);
                Assert.Equal(3000, EinnahmenView.GetGebäudeEinnahmen(festung));
                Assert.Equal(40000, RuestRules.GetKapazität(festung).Goldstücke);
                Assert.Equal(Kampfvorteil.AusFestung,
                    KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(festung)));
                Assert.Equal(6000, BeuteRules.GetRüstortbeute(festung));
                Assert.False(BelagerungsRules.IstBelagerbar(festung));

                // "Eine beschossene Festung wird mit 2499 Baupunkten zur Stadt"
                festung.Baupunkte = 2499;

                Assert.Equal("Stadt", RuestortRules.GetGrundstufe(BauwerkeView.GetRüstortNachKarte(festung)));
                Assert.Equal(2000, EinnahmenView.GetGebäudeEinnahmen(festung));
                Assert.Equal(25000, RuestRules.GetKapazität(festung).Goldstücke);
                Assert.Equal(Kampfvorteil.AusStadt,
                    KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(festung)));
                Assert.Equal(4000, BeuteRules.GetRüstortbeute(festung));

                // "Eine Festung kann NICHT belagert werden!" - eine Stadt schon
                Assert.True(BelagerungsRules.IstBelagerbar(festung));

                // die Sollstufe bleibt, sonst waere der Schaden nicht mehr zu sehen und die
                // Reparatur nicht mehr moeglich
                Assert.Equal("Festung", RuestortRules.GetSollstufe(festung)?.Ruestort);
                Assert.Equal(3000 - 2499, RuestortRules.GetSchaden(festung));
            }
            finally {
                festung.Baupunkte = baupunkte;
            }
        }

        /// <summary>
        /// "Eine Festung, die innerhalb einer Runde von ihren 3000 Baupunkten 1500 verliert, ist
        /// nur noch eine halbfertige Stadt ... man kann bis zur erneuten Fertigstellung der Stadt
        /// nur die Ruestkapazitaet einer Burg nutzen." (Regelwerk 1.5)
        /// </summary>
        [StaFact]
        public void DasBeispielDesRegelwerks() {
            LadeAlles();
            var festung = Festung();
            int baupunkte = festung.Baupunkte;
            try {
                festung.Baupunkte = 3000 - 1500;

                Assert.Equal("Burg", RuestortRules.GetGrundstufe(BauwerkeView.GetRüstortNachKarte(festung)));
                Assert.Equal(10000, RuestRules.GetKapazität(festung).Goldstücke);

                // die Reparatur dauert "6 Zuege a 250 Baupunkte"
                Assert.Equal(1500, RuestortRules.GetSchaden(festung));
                Assert.Equal(6, RuestortRules.GetSchaden(festung) / RuestortRules.MaxBaupunkteProMonat);
            }
            finally {
                festung.Baupunkte = baupunkte;
            }
        }

        /// <summary>
        /// Ein Ruestort ohne Baupunkte steht nicht mehr.
        ///
        /// Die Karte kennt solche Felder: ein Ruestort-Eintrag ohne Baupunkte. Bisher zaehlte er
        /// als vollstaendiger Ruestort mit allen Einnahmen.
        /// </summary>
        [StaFact]
        public void WasKeineBaupunkteHatStehtNichtMehr() {
            LadeAlles();
            var festung = Festung();
            int baupunkte = festung.Baupunkte;
            try {
                festung.Baupunkte = 0;

                Assert.Null(BauwerkeView.GetRüstortNachKarte(festung));
                Assert.Equal(0, EinnahmenView.GetGebäudeEinnahmen(festung));
                Assert.Equal(RuestRules.Rüstkapazität.Keine, RuestRules.GetKapazität(festung));
                Assert.Equal(0, BeuteRules.GetRüstortbeute(festung));
                Assert.Null(KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(festung)));

                // die Sollstufe bleibt: der Wiederaufbau weiss, was dort stand
                Assert.Equal("Festung", RuestortRules.GetSollstufe(festung)?.Ruestort);
            }
            finally {
                festung.Baupunkte = baupunkte;
            }
        }

        /// <summary>
        /// Ein unbeschaedigter Ruestort bleibt, was er ist - und ein Dorf bleibt ein Dorf, obwohl
        /// es mit null Baupunkten in der Referenz steht.
        /// </summary>
        [StaFact]
        public void WasHeilIstBleibtWieEsIst() {
            LadeAlles();

            var heile = SharedData.Map!.Values
                .Where(gemark => RuestortRules.GetSollstufe(gemark) != null
                              && RuestortRules.GetSchaden(gemark) == 0)
                .ToList();
            Assert.True(heile.Count > 100, "Die Karte hat kaum unbeschaedigte Ruestorte");

            // jeder von ihnen liefert genau seine Sollstufe
            Assert.All(heile, gemark => Assert.Same(
                RuestortRules.GetSollstufe(gemark), BauwerkeView.GetRüstortNachKarte(gemark)));

            // Doerfer stehen mit null Baupunkten in der Referenz und bleiben trotzdem stehen
            var dörfer = heile.Where(gemark =>
                RuestortRules.GetGrundstufe(RuestortRules.GetSollstufe(gemark)) == "Dorf").ToList();
            Assert.True(dörfer.Count > 0, "Die Karte kennt kein Dorf");
            Assert.All(dörfer, dorf => Assert.Equal("Dorf",
                RuestortRules.GetGrundstufe(BauwerkeView.GetRüstortNachKarte(dorf))));

            // eine leere Gemark ohne Ruestort bleibt leer
            var leer = SharedData.Map.Values.First(gemark => gemark.Ruestort is null or 0 && gemark.Baupunkte == 0);
            Assert.Null(BauwerkeView.GetRüstortNachKarte(leer));
        }

        /// <summary>
        /// Eine beschaedigte Hauptstadt bleibt die Hauptstadt des Reiches, bringt aber nur noch,
        /// was ihre Baupunkte tragen.
        ///
        /// Im Datenbestand gibt es das: die Hauptstadt von Theostelos steht mit 3.000 statt 5.000
        /// Baupunkten in der Karte.
        /// </summary>
        [StaFact]
        public void EineBeschaedigteHauptstadtBleibtDieHauptstadt() {
            LadeAlles();
            var hauptstadt = HauptstadtRules.FindeHauptstadt(ProgramView.SelectedNation);
            Assert.True(hauptstadt != null, "Das eigene Reich hat keine Hauptstadt");

            int baupunkte = hauptstadt!.Baupunkte;
            try {
                hauptstadt.Baupunkte = 3000;

                // sie bleibt die Hauptstadt - das ist die Bezeichnung, nicht der Zustand
                Assert.Same(hauptstadt, HauptstadtRules.FindeHauptstadt(ProgramView.SelectedNation));
                Assert.Equal("Hauptstadt", RuestortRules.GetSollstufe(hauptstadt)?.Ruestort);

                // aber sie traegt nur eine Festung
                Assert.Equal("Festung", BauwerkeView.GetRüstortNachKarte(hauptstadt)?.Ruestort);
                Assert.Equal(3000, EinnahmenView.GetGebäudeEinnahmen(hauptstadt));
                Assert.Equal(Kampfvorteil.AusFestung,
                    KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(hauptstadt)));

                // und sie ist kein Ziel fuer eine Hauptstadtverlegung
                Assert.False(HauptstadtRules.IstBestehendeFestung(hauptstadt));
            }
            finally {
                hauptstadt.Baupunkte = baupunkte;
            }
        }
    }
}
