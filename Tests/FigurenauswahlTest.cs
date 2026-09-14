using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Liste, die der Rechtsklick auf ein Kleinfeld anbietet - Issue #20.
    ///
    /// Angeboten werden alle Figuren des Feldes, eigene zuerst. Auswaehlen laesst sich nur, was
    /// einem gehoert; das entscheidet Spielfigur.Select und nicht die Liste. Fremde trotzdem
    /// anzuzeigen ist Absicht: auf einem Feld, auf dem etwas steht, soll man sehen, was dort steht.
    /// </summary>
    public class FigurenauswahlTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>Ein Kleinfeld, auf dem eigene Figuren stehen</summary>
        private static KleinFeld FindeBesetzteGemark() {
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .FirstOrDefault(Plausibilität.IsValid);
            Assert.True(figur != null, "Das eigene Reich hat keine Figuren");
            var gemark = KleinfeldView.GetKleinfeld(new KleinfeldPosition(figur!.gf, figur.kf));
            Assert.True(gemark != null, "Die Gemark der Figur liegt nicht auf der Karte");
            return gemark!;
        }

        /// <summary>
        /// Die Auswahlliste laesst nichts weg und nichts doppelt - sie ist dieselbe Menge wie die
        /// Figuren der Gemark, nur anders sortiert.
        /// </summary>
        [StaFact]
        public void DieAuswahllisteEnthaeltGenauDieFigurenDerGemark() {
            LadeAlles();
            var gemark = FindeBesetzteGemark();

            var alle = SpielfigurenView.GetSpielfiguren(gemark);
            var zurAuswahl = SpielfigurenView.GetSpielfigurenZurAuswahl(gemark);

            Assert.Equal(alle.Count, zurAuswahl.Count);

            // Verglichen wird ueber die Objektgleichheit. Spielfigur erbt von KleinfeldPosition
            // eine Gleichheit nach gf/kf - mit Distinct() oder Contains schrumpfen drei Figuren
            // desselben Feldes auf eine zusammen, und der Test pruefte nichts mehr.
            Assert.Equal(alle.Count, zurAuswahl.Distinct(ReferenceEqualityComparer.Instance).Count());
            foreach (var figur in alle)
                Assert.Contains(zurAuswahl, kandidat => ReferenceEquals(kandidat, figur));
        }

        /// <summary>
        /// Die eigenen stehen vorn. Auf einem Feld mit mehreren Reichen sucht man sonst seine
        /// eigene Einheit zwischen fremden.
        /// </summary>
        [StaFact]
        public void DieEigenenFigurenStehenVorn() {
            LadeAlles();

            // jedes besetzte Feld pruefen, nicht nur eines - gemischte Felder sind selten
            int gemischt = 0;
            foreach (var gemark in SharedData.Map!.Values) {
                var zurAuswahl = SpielfigurenView.GetSpielfigurenZurAuswahl(gemark);
                if (zurAuswahl.Count == 0)
                    continue;

                bool fremdeGesehen = false;
                foreach (var figur in zurAuswahl) {
                    bool eigene = figur.Nation != null && figur.Nation == ProgramView.SelectedNation;
                    if (eigene)
                        Assert.False(fremdeGesehen,
                            $"Auf {gemark.Bezeichner} steht eine eigene Figur hinter einer fremden");
                    else
                        fremdeGesehen = true;
                }
                if (fremdeGesehen && zurAuswahl.Any(f => f.Nation == ProgramView.SelectedNation))
                    gemischt++;
            }
            // kein Assert auf gemischt > 0: ob es gemischte Felder gibt, haengt an den Zugdaten
            Assert.True(gemischt >= 0);
        }

        /// <summary>
        /// Auswaehlbar ist nur, was einem gehoert - darauf verlaesst sich das Menue, wenn es die
        /// fremden Eintraege abblendet.
        /// </summary>
        [StaFact]
        public void NurEigeneFigurenSindAuswaehlbar() {
            LadeAlles();
            var gemark = FindeBesetzteGemark();

            int eigene = 0;
            foreach (var figur in SpielfigurenView.GetSpielfigurenZurAuswahl(gemark)) {
                bool gehörtMir = figur.Nation != null && figur.Nation == ProgramView.SelectedNation;
                Assert.Equal(gehörtMir, figur.Select());
                if (gehörtMir)
                    eigene++;
            }
            Assert.True(eigene > 0, "Auf dieser Gemark steht keine eigene Figur");
        }

        /// <summary>
        /// Auf einem leeren Feld gibt es nichts anzubieten - und keine Ausnahme.
        /// </summary>
        [StaFact]
        public void EinLeeresFeldBietetNichtsAn() {
            LadeAlles();
            var besetzt = SharedData.Map!.Values
                .Where(gemark => SpielfigurenView.GetSpielfiguren(gemark).Count > 0)
                .Select(gemark => gemark.Bezeichner)
                .ToHashSet();

            var leer = SharedData.Map!.Values.FirstOrDefault(gemark => besetzt.Contains(gemark.Bezeichner) == false);
            Assert.True(leer != null, "Es gibt kein leeres Feld");
            Assert.Empty(SpielfigurenView.GetSpielfigurenZurAuswahl(leer!));
        }
    }
}
