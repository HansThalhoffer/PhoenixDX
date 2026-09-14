using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was auf einem Kleinfeld steht - aus beiden Quellen.
    ///
    /// Auf einem Feld koennen zwei ganz verschiedene Dinge stehen: eigene Figuren aus den Zugdaten
    /// und fremde Einheiten aus der Feindaufklaerung. KleinFeld.Truppen kennt nur die erste Quelle,
    /// KleinFeld.Fremd nur die zweite. Die Karte zeichnet beide.
    ///
    /// Die erste Fassung der Figurenauswahl fragte nur die Zugdaten ab und meldete auf 608/44 -
    /// wo laut Karte ein Magier und Fusstruppen von Yaromo stehen - "hier steht nichts". Kein Test
    /// hat das bemerkt, weil der Testlauf die Feindaufklaerung gar nicht geladen hatte.
    /// </summary>
    public class Feldbesetzungstest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.LoadFeinderkennung();
        }

        /// <summary>
        /// Ein Feld, auf dem ausschliesslich aufgeklaerte fremde Einheiten stehen, meldet nicht
        /// "hier steht nichts".
        /// </summary>
        [StaFact]
        public void EinNurFremdBesetztesFeldIstNichtLeer() {
            LadeAlles();
            // Kein stilles Aussteigen: genau dieser Test soll scheitern, wenn die
            // Feindaufklaerung fehlt. Die erste Fassung hatte hier ein "dann eben nicht" und ging
            // deshalb auch dann durch, als die fremden Einheiten gar nicht mitgezaehlt wurden.
            Assert.True(SharedData.Feinde != null && SharedData.Feinde.Count > 0,
                $"Die Feindaufklaerung wurde nicht geladen: {TestSetup.Feindaufklärungsdatei}");

            var nurFremd = SharedData.Feinde
                .Select(fremd => new KleinfeldPosition(fremd.gf, fremd.kf))
                .Where(pos => SharedData.Map!.ContainsKey(pos.CreateBezeichner()))
                .Where(pos => SpielfigurenView.GetSpielfiguren(pos).Count == 0)
                .FirstOrDefault();
            Assert.True(nurFremd != null, "Kein Feld, auf dem nur fremde Einheiten stehen");

            var besetzung = SpielfigurenView.GetFeldbesetzung(nurFremd!);
            Assert.NotEmpty(besetzung);
            // und alles davon ist fremd, also nicht auswaehlbar
            Assert.All(besetzung, eintrag => Assert.Null(eintrag.Figur));
            Assert.All(besetzung, eintrag => Assert.False(eintrag.IstAuswählbar));
            Assert.All(besetzung, eintrag => Assert.False(string.IsNullOrWhiteSpace(eintrag.Beschriftung)));
        }

        /// <summary>
        /// Die Feldbesetzung ist die Summe beider Quellen - nichts faellt weg.
        /// </summary>
        [StaFact]
        public void DieFeldbesetzungEnthaeltBeideQuellen() {
            LadeAlles();

            int geprüft = 0;
            foreach (var gemark in SharedData.Map!.Values) {
                int eigene = SpielfigurenView.GetSpielfiguren(gemark).Count;
                int fremde = gemark.Fremd.Count;
                if (eigene == 0 && fremde == 0)
                    continue;

                var besetzung = SpielfigurenView.GetFeldbesetzung(gemark);
                // Doppelungen werden bewusst entfernt, deshalb hoechstens die Summe - aber nie
                // weniger als jede einzelne Quelle
                Assert.True(besetzung.Count <= eigene + fremde, $"{gemark.Bezeichner}: mehr als beide Quellen");
                Assert.True(besetzung.Count >= Math.Max(eigene, fremde), $"{gemark.Bezeichner}: weniger als eine Quelle");
                Assert.Equal(eigene, besetzung.Count(eintrag => eintrag.Figur != null));
                geprüft++;
            }
            Assert.True(geprüft > 0, "Es steht nirgends etwas - dann prueft dieser Test nichts");
        }

        /// <summary>
        /// Eigene Figuren stehen vorn und sind auswaehlbar, fremde nicht.
        /// </summary>
        [StaFact]
        public void EigeneStehenVornUndNurSieSindAuswaehlbar() {
            LadeAlles();

            foreach (var gemark in SharedData.Map!.Values) {
                var besetzung = SpielfigurenView.GetFeldbesetzung(gemark);
                if (besetzung.Count == 0)
                    continue;

                bool fremdesGesehen = false;
                foreach (var eintrag in besetzung) {
                    bool eigen = eintrag.Figur != null && eintrag.Figur.Nation == ProgramView.SelectedNation;
                    if (eigen)
                        Assert.False(fremdesGesehen, $"Auf {gemark.Bezeichner} steht Eigenes hinter Fremdem");
                    else
                        fremdesGesehen = true;
                    Assert.Equal(eigen, eintrag.IstAuswählbar);
                }
            }
        }

        /// <summary>
        /// Auf einem wirklich leeren Feld steht nichts - auch nicht aus der Feindaufklaerung.
        /// </summary>
        [StaFact]
        public void EinLeeresFeldBleibtLeer() {
            LadeAlles();
            var leer = SharedData.Map!.Values.FirstOrDefault(gemark =>
                SpielfigurenView.GetSpielfiguren(gemark).Count == 0 && gemark.Fremd.Count == 0);
            Assert.True(leer != null, "Es gibt kein leeres Feld");
            Assert.Empty(SpielfigurenView.GetFeldbesetzung(leer!));
        }
    }
}
