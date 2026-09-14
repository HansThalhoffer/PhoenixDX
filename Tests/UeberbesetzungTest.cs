using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Ueberbesetzung einer Gemark (Regelwerk 5.7).
    ///
    /// "Befinden sich mehr als 100.000 Raumpunkte am Ende des Spielzuges eines Reiches in der
    /// selben Gemark, so kommt es zum Nahkampf, wenn sich Heere von zwei oder mehr Reichen in der
    /// Gemark befinden, ansonsten wird die Ueberzahl prozentual anteilig gestrichen."
    /// </summary>
    public class UeberbesetzungTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        private static Krieger Heer(int nummer, Nation reich, PhoenixModel.dbErkenfara.KleinFeld feld, int staerke)
            => new() { Nummer = nummer, Nation = reich, staerke = staerke, gf_von = feld.gf, kf_von = feld.kf };

        /// <summary>
        /// Ein Krieger belegt einen Raumpunkt - die Grenze liegt bei 100.000, und genau 100.000
        /// sind noch in Ordnung: "mehr als 100.000 Raumpunkte".
        /// </summary>
        [StaFact]
        public void ErstUeberHundertTausendIstEsZuviel() {
            LadeAlles();
            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var reich = ProgramView.SelectedNation!;

            Assert.Equal(100_000, ÜberbesetzungRules.Raumpunktgrenze);
            Assert.False(ÜberbesetzungRules.IstÜberbesetzt(100_000));
            Assert.True(ÜberbesetzungRules.IstÜberbesetzt(100_001));

            // genau an der Grenze passiert nichts
            Assert.Null(ÜberbesetzungRules.Prüfe([Heer(100, reich, feld, 100_000)]));

            // einer mehr, und es ist zuviel
            var knapp = ÜberbesetzungRules.Prüfe([Heer(100, reich, feld, 100_001)]);
            Assert.True(knapp != null);
            Assert.Equal(100_001, knapp!.Raumpunkte);
            Assert.Equal(1, knapp.Überzahl);

            Assert.Null(ÜberbesetzungRules.Prüfe(null));
            Assert.Null(ÜberbesetzungRules.Prüfe([]));
        }

        /// <summary>
        /// "so kommt es zum Nahkampf, wenn sich Heere von zwei oder mehr Reichen in der Gemark
        /// befinden" - ob sie verfeindet sind, spielt keine Rolle.
        /// </summary>
        [StaFact]
        public void MehrereReicheKaempfenUmDenPlatz() {
            LadeAlles();
            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var eigenes = ProgramView.SelectedNation!;

            // ein verfeindetes Reich: Nahkampf
            var gegner = SharedData.Nationen!.First(n => n.Equals(eigenes) == false
                && DiplomatieRules.SindVerfeindet(n, eigenes, feld));
            var mitGegner = ÜberbesetzungRules.Prüfe([
                Heer(100, eigenes, feld, 60_000),
                Heer(101, gegner, feld, 60_000),
            ]);
            Assert.True(mitGegner != null);
            Assert.Equal(ÜberbesetzungRules.Folge.Nahkampf, mitGegner!.Folge);
            Assert.Empty(mitGegner.Streichungen);

            // und ein verbuendetes Paar ebenso - auf einer ueberfuellten Gemark kaempfen auch
            // Verbuendete um den Platz
            var paar = (from einer in SharedData.Nationen!
                        from anderer in SharedData.Nationen!
                        where einer.Equals(anderer) == false
                           && DiplomatieRules.SindAlliiert(einer, anderer, feld)
                        select (einer, anderer)).FirstOrDefault();
            Assert.True(paar.einer != null, "Im Datenbestand gibt es kein verbuendetes Reichspaar");

            var mitVerbündeten = ÜberbesetzungRules.Prüfe([
                Heer(100, paar.einer, feld, 60_000),
                Heer(101, paar.anderer, feld, 60_000),
            ]);
            Assert.True(mitVerbündeten != null);
            Assert.Equal(ÜberbesetzungRules.Folge.Nahkampf, mitVerbündeten!.Folge);

            // ein Reich allein: es wird gestrichen, nicht gekaempft
            var allein = ÜberbesetzungRules.Prüfe([
                Heer(100, eigenes, feld, 60_000),
                Heer(101, eigenes, feld, 60_000),
            ]);
            Assert.True(allein != null);
            Assert.Equal(ÜberbesetzungRules.Folge.Streichung, allein!.Folge);
            Assert.NotEmpty(allein.Streichungen);
        }

        /// <summary>
        /// "ansonsten wird die Ueberzahl prozentual anteilig gestrichen" - jedes Heer gibt
        /// denselben Anteil ab.
        /// </summary>
        [StaFact]
        public void DieUeberzahlWirdAnteiligGestrichen() {
            LadeAlles();
            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var reich = ProgramView.SelectedNation!;

            // 90.000 und 30.000 Krieger: 120.000 RP, also 20.000 zuviel - ein Sechstel
            var überbesetzt = ÜberbesetzungRules.Prüfe([
                Heer(100, reich, feld, 90_000),
                Heer(101, reich, feld, 30_000),
            ]);
            Assert.True(überbesetzt != null);
            Assert.Equal(120_000, überbesetzt!.Raumpunkte);
            Assert.Equal(20_000, überbesetzt.Überzahl);
            Assert.Equal(2, überbesetzt.Streichungen.Count);

            // das grosse Heer traegt drei Viertel der Ueberzahl, das kleine ein Viertel
            Assert.Equal(15_000, überbesetzt.Streichungen[0].GestricheneRaumpunkte);
            Assert.Equal(5_000, überbesetzt.Streichungen[1].GestricheneRaumpunkte);
            Assert.Equal(20_000, überbesetzt.Streichungen.Sum(s => s.GestricheneRaumpunkte));

            // und in Kriegern gerechnet dasselbe
            Assert.Equal(15_000, überbesetzt.Streichungen[0].Gestrichen.Krieger);
            Assert.Equal(5_000, überbesetzt.Streichungen[1].Gestrichen.Krieger);
        }

        /// <summary>
        /// Charaktere belegen Platz, werden aber nicht gestrichen - einen halben Charakter gibt
        /// es nicht.
        /// </summary>
        [StaFact]
        public void CharaktereBelegenPlatzWerdenAberNichtGestrichen() {
            LadeAlles();
            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var reich = ProgramView.SelectedNation!;

            var charakter = new Character {
                Beschriftung = "HF1", GP_akt = 24, GP_ges = 24, Nummer = 600,
                Nation = reich, gf_von = feld.gf, kf_von = feld.kf,
            };
            // ein Charakter ohne Spielernamen ist eine Figur der Spielleitung und belegt
            // pauschal 600 Raumpunkte (SpielfigurRules.BerechneRaumpunkte)
            Assert.Equal(600, ÜberbesetzungRules.BerechneRaumpunkte([charakter]));

            var ergebnis = ÜberbesetzungRules.Prüfe([Heer(100, reich, feld, 100_000), charakter]);
            Assert.True(ergebnis != null);
            Assert.Equal(100_600, ergebnis!.Raumpunkte);
            Assert.Equal(600, ergebnis.Überzahl);

            // gestrichen wird nur beim Heer
            Assert.Single(ergebnis.Streichungen);
            Assert.Equal(600, ergebnis.Streichungen[0].GestricheneRaumpunkte);
            Assert.Equal(100, ergebnis.Streichungen[0].Figur.Nummer);
            Assert.Equal(600, ergebnis.Streichungen[0].Gestrichen.Krieger);
        }

        /// <summary>
        /// Gesucht wird ueber alle Gemarken - und nur die ueberbesetzten kommen zurueck.
        /// </summary>
        [StaFact]
        public void GesuchtWirdUeberAlleGemarken() {
            LadeAlles();
            var reich = ProgramView.SelectedNation!;
            var felder = SharedData.Map!.Values.Where(kf => kf.IsWasser == false).Take(2).ToList();

            var ergebnis = ÜberbesetzungRules.FindeÜberbesetzungen([
                Heer(100, reich, felder[0], 150_000),   // zuviel
                Heer(101, reich, felder[1], 50_000),    // in Ordnung
            ]);

            Assert.Single(ergebnis);
            Assert.Equal(felder[0].gf, ergebnis[0].Gemark.gf);
            Assert.Equal(felder[0].kf, ergebnis[0].Gemark.kf);
            Assert.Equal(50_000, ergebnis[0].Überzahl);

            Assert.Empty(ÜberbesetzungRules.FindeÜberbesetzungen(null));
            Assert.Empty(ÜberbesetzungRules.FindeÜberbesetzungen([]));
        }

        /// <summary>
        /// Die Raumpunkte kommen aus den Figuren, nicht aus der Spalte rp - die kann veraltet
        /// sein, wenn ein Heer geteilt oder zusammengelegt wurde.
        /// </summary>
        [StaFact]
        public void GerechnetWirdMitDenFigurenNichtMitDerSpalte() {
            LadeAlles();
            var feld = SharedData.Map!.Values.First(kf => kf.IsWasser == false);
            var reich = ProgramView.SelectedNation!;

            var heer = Heer(100, reich, feld, 120_000);
            heer.rp = 7;   // eine veraltete Angabe

            var ergebnis = ÜberbesetzungRules.Prüfe([heer]);
            Assert.True(ergebnis != null);
            Assert.Equal(120_000, ergebnis!.Raumpunkte);
        }
    }
}
