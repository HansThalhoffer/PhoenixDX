using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;

namespace Tests {

    /// <summary>
    /// Der Unterbau der Kampfauswertung: was die Oberflaeche der Spielleitung vorschlaegt und
    /// woraus sie die beiden Seiten baut.
    ///
    /// Die Regeln dahinter pruefen KampfTest, NahkampfTest und KampfablaufTest. Hier geht es um
    /// die Vorbelegung - und um die Gutpunkte, die eine Gemark ihrem Verteidiger bringt
    /// (Regelwerk 5.5.1).
    /// </summary>
    public class KampfauswertungTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        private static KleinFeld FindeRuestort()
            => SharedData.Map!.Values.First(kf => kf.Nation != null
                && KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(kf)) != null);

        /// <summary>
        /// "Eigene Heere, die einen Ruestort verteidigen, erhalten die GP des eigenen Ruestortes.
        /// Heere, die einen fremden Ruestort verteidigen, erhalten 50 GP durch den Ruestort."
        /// (Regelwerk 5.5.1)
        /// </summary>
        [StaFact]
        public void EinRuestortGibtSeineGutpunkteNurDemEigenenReich() {
            LadeAlles();
            var gemark = FindeRuestort();
            var eigentümer = gemark.Nation!;
            var vorteilDesOrts = KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(gemark))!.Value;

            var eigene = KampfRules.BestimmeVerteidigungsvorteile(gemark, eigentümer, FigurType.Krieger);
            Assert.Equal([vorteilDesOrts], eigene);

            var fremdes = SharedData.Nationen!.First(n => n.Equals(eigentümer) == false);
            var fremde = KampfRules.BestimmeVerteidigungsvorteile(gemark, fremdes, FigurType.Krieger);
            Assert.Equal([Kampfvorteil.AusFremdemRüstort], fremde);
            Assert.Equal(50, KampfRules.BerechneVorteile(fremde));
        }

        /// <summary>
        /// "Heere, die einen Ruestort angreifen oder verteidigen erhalten keine GP aus dem
        /// Gelaendevorteil fuer Reiter oder Krieger." (Regelwerk 5.5.1)
        /// </summary>
        [StaFact]
        public void WoEinRuestortStehtZaehltDasGelaendeNicht() {
            LadeAlles();

            // ein Ruestort in einem Gelaende, das Reitern sonst 140 Gutpunkte braechte
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf => kf.Nation != null
                && KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(kf)) != null
                && KampfRules.GetGeländevorteil(FigurType.Reiter, kf.TerrainType) != null);
            Assert.True(gemark != null, "Kein Ruestort in Reitergelaende gefunden");

            var vorteile = KampfRules.BestimmeVerteidigungsvorteile(gemark, gemark!.Nation, FigurType.Reiter);
            Assert.DoesNotContain(Kampfvorteil.GeländeReiter, vorteile);
            Assert.Single(vorteile);
        }

        /// <summary>
        /// Auf freiem Feld zaehlt das Gelaende - und zwar fuer die passende Gattung.
        /// </summary>
        [StaFact]
        public void AufFreiemFeldZaehltDasGelaende() {
            LadeAlles();

            var tiefland = SharedData.Map!.Values.First(kf => kf.TerrainType == TerrainType.Tiefland
                && BauwerkeView.GetRüstortNachKarte(kf) == null);

            Assert.Equal([Kampfvorteil.GeländeReiter],
                KampfRules.BestimmeVerteidigungsvorteile(tiefland, tiefland.Nation, FigurType.Reiter));
            Assert.Empty(KampfRules.BestimmeVerteidigungsvorteile(tiefland, tiefland.Nation, FigurType.Krieger));
            Assert.Empty(KampfRules.BestimmeVerteidigungsvorteile(null, null, FigurType.Reiter));
        }

        /// <summary>
        /// Aus einem Konflikt werden die Heere beider Seiten - der Angreifer bekommt keinen
        /// Feldvorteil vorgeschlagen, der Verteidiger den seiner Gemark.
        /// </summary>
        [StaFact]
        public void DieZeilenTrennenAngreiferUndVerteidiger() {
            LadeAlles();
            var gemark = FindeRuestort();
            var verteidiger = gemark.Nation!;
            var angreifer = SharedData.Nationen!.First(n => n.Equals(verteidiger) == false
                && DiplomatieRules.SindVerfeindet(n, verteidiger, gemark));

            var konflikte = KonfliktRules.FindeKonflikte([
                new Krieger { Nummer = 100, Nation = angreifer, staerke = 1000, gf_von = gemark.gf, kf_von = gemark.kf },
                new Krieger { Nummer = 200, Nation = verteidiger, staerke = 1000, gf_von = gemark.gf, kf_von = gemark.kf },
            ]);
            Assert.Single(konflikte);

            var zeilen = Kampfauswertung.BaueHeereszeilen(konflikte[0], angreifer);
            Assert.Equal(2, zeilen.Count);

            var angreiferzeile = zeilen.First(z => z.IstAngreifer);
            var verteidigerzeile = zeilen.First(z => z.IstAngreifer == false);

            Assert.Equal(0, angreiferzeile.Gutpunkte);
            Assert.Contains("Angreifer", angreiferzeile.Herkunft);

            int erwartet = KampfRules.GetGutpunkte(
                KampfRules.GetRüstortvorteil(BauwerkeView.GetRüstortNachKarte(gemark))!.Value);
            Assert.Equal(erwartet, verteidigerzeile.Gutpunkte);
            Assert.Equal(1000, verteidigerzeile.Stärke);

            Assert.Empty(Kampfauswertung.BaueHeereszeilen(null, angreifer));
        }

        /// <summary>
        /// Gerechnet wird mit dem, was in den Zeilen steht - auch wenn der Auswerter die
        /// Gutpunkte von Hand aendert, etwa um den W20 nachzutragen.
        /// </summary>
        [StaFact]
        public void GerechnetWirdMitDemWasInDenZeilenSteht() {
            LadeAlles();
            var gemark = SharedData.Map!.Values.First(kf => kf.IsWasser == false
                && BauwerkeView.GetRüstortNachKarte(kf) == null);

            var eigenes = ProgramView.SelectedNation!;
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false);

            List<Kampfauswertung.Heereszeile> zeilen = [
                new() { Heer = new Krieger { Nummer = 100, staerke = 1000, gf_von = gemark.gf, kf_von = gemark.kf },
                        Reich = eigenes.Reich, IstAngreifer = true, Gutpunkte = 0 },
                new() { Heer = new Krieger { Nummer = 200, staerke = 1000, gf_von = gemark.gf, kf_von = gemark.kf },
                        Reich = gegner.Reich, IstAngreifer = false, Gutpunkte = 0 },
            ];

            // gleich stark, gleiche Gutpunkte: unentschieden
            var gleichstand = Kampfauswertung.Werte(gemark, zeilen);
            Assert.Equal(NahkampfRules.Ausgang.Unentschieden, gleichstand.Nahkampf.Sieger);

            // der Auswerter traegt dem Angreifer den W20 und einen Charakter nach
            zeilen[0].Gutpunkte = 40;
            var mitVorteil = Kampfauswertung.Werte(gemark, zeilen);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, mitVorteil.Nahkampf.Sieger);

            // und der Beschuss wird ebenfalls uebernommen
            var mitBeschuss = Kampfauswertung.Werte(gemark, zeilen, trefferpunkteGegenVerteidiger: 50);
            Assert.Equal(500, mitBeschuss.BeschussGegenVerteidiger.Verluste[0].Krieger);

            // ohne Zeilen faellt nichts um
            var leer = Kampfauswertung.Werte(gemark, null);
            Assert.Empty(leer.Nahkampf.Angreifer);
        }
    }
}
