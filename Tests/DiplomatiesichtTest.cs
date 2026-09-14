using PhoenixModel.dbPZE;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Welche Reiche haben uns ein Recht eingeraeumt?
    ///
    /// An dieser Frage haengt, wohin die Karte eine Bewegung erlaubt (BewegungsRules.HatWegerecht
    /// ueber KleinFeld.HasWegeRecht) und was sie als Kuestenrecht einfaerbt. Geprueft wird gegen
    /// die Rohspalten der Tabelle Reich_crossref, unabhaengig davon, wie die Sicht sie liest.
    /// </summary>
    public class DiplomatiesichtTest {

        private static void LadeMitDiplomatie() {
            TestSetup.LadeMitDiplomatie();
            DiplomatieView.Vergiss();
        }

        /// <summary>
        /// Die Geber eines Rechts, direkt aus den Rohspalten gelesen.
        ///
        /// Jede Zeile gehoert dem Reich, das in Referenzreich steht: "Wegerecht" ist, was es
        /// vergeben hat, "Wegerecht_von", was es erhalten hat. Ein Recht kann deshalb in der Zeile
        /// des Gebers oder in der des Empfaengers stehen.
        /// </summary>
        private static List<Nation> GeberLautTabelle(Nation empfänger, bool wasser) {
            List<Nation> ergebnis = [];
            foreach (var zeile in SharedData.Diplomatie!.Values) {
                if (zeile.ReferenzNation.Equals(zeile.Nation))
                    continue;
                int vergeben = wasser ? zeile.Kuestenrecht : zeile.Wegerecht;
                int erhalten = wasser ? zeile.Kuestenrecht_von : zeile.Wegerecht_von;

                Nation? geber = null;
                if (zeile.Nation.Equals(empfänger) && vergeben > 0)
                    geber = zeile.ReferenzNation;
                else if (zeile.ReferenzNation.Equals(empfänger) && erhalten > 0)
                    geber = zeile.Nation;

                if (geber != null && ergebnis.Any(bekannt => bekannt.Equals(geber)) == false)
                    ergebnis.Add(geber);
            }
            return ergebnis;
        }

        private static List<string> Namen(IEnumerable<Nation>? reiche)
            => [.. (reiche ?? []).Select(reich => reich.Reich).OrderBy(name => name)];

        /// <summary>
        /// Gefragt wird nach dem uebergebenen Reich - nicht nach dem, das gerade ausgewaehlt ist.
        ///
        /// Die Sicht hat ihren Parameter frueher ignoriert und immer auf ProgramView.SelectedNation
        /// gefiltert. Fuer die Spielleitung, die alle Reiche vor sich hat, war die Antwort damit
        /// immer die des eigenen Reiches.
        /// </summary>
        [StaFact]
        public void DasGefragteReichEntscheidet() {
            LadeMitDiplomatie();

            // ein fremdes Reich, das ueberhaupt Rechte erhalten hat
            var fremdes = SharedData.Nationen!.FirstOrDefault(n => n.Equals(ProgramView.SelectedNation) == false
                && GeberLautTabelle(n, wasser: false).Count > 0);
            Assert.True(fremdes != null, "Kein fremdes Reich mit erhaltenem Wegerecht gefunden");

            Assert.Equal(Namen(GeberLautTabelle(fremdes!, wasser: false)),
                         Namen(DiplomatieView.GetWegerectAllowed(fremdes)));
            Assert.Equal(Namen(GeberLautTabelle(fremdes!, wasser: true)),
                         Namen(DiplomatieView.GetKüstenregelAllowed(fremdes)));

            // und das eigene Reich bekommt weiterhin seine eigene Antwort
            var eigenes = ProgramView.SelectedNation;
            Assert.True(eigenes != null);
            Assert.Equal(Namen(GeberLautTabelle(eigenes!, wasser: false)),
                         Namen(DiplomatieView.GetWegerectAllowed()));
            Assert.Equal(Namen(GeberLautTabelle(eigenes!, wasser: true)),
                         Namen(DiplomatieView.GetKüstenregelAllowed()));
        }

        /// <summary>
        /// Ein Recht wird auch gefunden, wenn nur der Geber es in seiner Zeile fuehrt.
        ///
        /// Frueher wurde ausschliesslich in der Zeile des Empfaengers nachgesehen. Gepflegt ist im
        /// Datenbestand aber regelmaessig nur eine der beiden Zeilen.
        /// </summary>
        [StaFact]
        public void EinRechtZaehltAuchWennNurDerGeberEsFuehrt() {
            LadeMitDiplomatie();

            var zeile = SharedData.Diplomatie!.Values.FirstOrDefault(z => {
                if (z.ReferenzNation.Equals(z.Nation) || z.Wegerecht <= 0)
                    return false;
                var desEmpfängers = SharedData.Diplomatie.Values.FirstOrDefault(
                    e => e.ReferenzNation.Equals(z.Nation) && e.Nation.Equals(z.ReferenzNation));
                return desEmpfängers == null || desEmpfängers.Wegerecht_von <= 0;
            });
            Assert.True(zeile != null, "Im Datenbestand gibt es kein Wegerecht, das nur der Geber fuehrt");

            var geber = zeile!.ReferenzNation;
            var empfänger = zeile.Nation;
            Assert.Contains(Namen(DiplomatieView.GetWegerectAllowed(empfänger)), name => name == geber.Reich);
        }

        /// <summary>
        /// Die Richtung zaehlt: wer ein Recht vergeben hat, hat damit noch keines bekommen.
        ///
        /// Frueher lieferte die Sicht die Reiche, die das Recht erhalten hatten, wurde aber als
        /// "die es uns zugebilligt haben" gelesen und in KleinfeldView.UserHasWegerecht auch so
        /// benutzt - die Karte erlaubte damit Bewegungen genau in die falsche Richtung.
        /// </summary>
        [StaFact]
        public void WerVergibtHatDeswegenNochKeinRecht() {
            LadeMitDiplomatie();

            // ein einseitiges Wegerecht: der Geber hat vergeben, aber nichts zurueckbekommen
            var paar = SharedData.Diplomatie!.Values.FirstOrDefault(z => {
                if (z.ReferenzNation.Equals(z.Nation) || z.Wegerecht <= 0 || z.Wegerecht_von > 0)
                    return false;
                var gegenrichtung = SharedData.Diplomatie.Values.FirstOrDefault(
                    g => g.ReferenzNation.Equals(z.Nation) && g.Nation.Equals(z.ReferenzNation));
                return gegenrichtung == null || (gegenrichtung.Wegerecht <= 0 && gegenrichtung.Wegerecht_von <= 0);
            });
            Assert.True(paar != null, "Im Datenbestand gibt es kein einseitig vergebenes Wegerecht");

            var geber = paar!.ReferenzNation;
            var empfänger = paar.Nation;

            // der Empfaenger darf durch das Gebiet des Gebers
            Assert.Contains(Namen(DiplomatieView.GetWegerectAllowed(empfänger)), name => name == geber.Reich);
            // der Geber aber nicht durch das Gebiet des Empfaengers
            Assert.DoesNotContain(Namen(DiplomatieView.GetWegerectAllowed(geber)), name => name == empfänger.Reich);
        }

        /// <summary>
        /// Ohne Reich gibt es keine Antwort - null, nicht eine leere Liste, damit die Aufrufer
        /// "nicht geladen" von "nichts erlaubt" unterscheiden koennen.
        /// </summary>
        [StaFact]
        public void OhneReichGibtEsKeineAntwort() {
            LadeMitDiplomatie();
            Assert.Null(DiplomatieView.GetWegerectAllowed(null));
            Assert.Null(DiplomatieView.GetKüstenregelAllowed(null));
        }
    }
}
