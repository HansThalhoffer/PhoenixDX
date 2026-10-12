using PhoenixModel.Helper;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;

namespace Tests {

    /// <summary>
    /// Der gemeldete Absturz beim Ziehen einer Figur: eine ArgumentOutOfRangeException in
    /// <c>SelectFigurPage.ViewModel_OnViewEvent</c>, ausgelöst von einem Zugriff über das Ende
    /// einer Liste hinaus.
    ///
    /// Die Figurenliste frischte nach jeder Änderung eine Zeile auf. Welche Zeile, suchte sie mit
    /// <c>Items.IndexOf</c> - und das ist für Spielfiguren die falsche Frage. Hier stehen die
    /// beiden Voraussetzungen, auf denen die Berichtigung beruht; fällt eine davon weg, steht die
    /// Begründung in der Methode nicht mehr richtig da.
    /// </summary>
    public class GitterzeileTest {

        /// <summary>
        /// Die Voraussetzung, die alles ausgelöst hat: zwei verschiedene Figuren auf demselben
        /// Feld sind <c>Equals</c>, weil Spielfigur das Equals von KleinfeldPosition erbt.
        ///
        /// Das ist für die Karte richtig und für eine Figurenliste verhängnisvoll. Nach einer
        /// Bewegung steht die gesuchte Figur auf einem neuen Feld - und wenn dort schon etwas
        /// steht, liefert IndexOf dessen Zeile.
        /// </summary>
        [Fact]
        public void ZweiFigurenAufEinemFeldSindGleichOhneDieselbeZuSein() {
            var krieger = new Krieger { Nummer = 101, gf = 305, kf = 24 };
            var reiter = new Reiter { Nummer = 201, gf = 305, kf = 24 };

            Assert.Equal<KleinfeldPosition>(krieger, reiter);
            Assert.False(ReferenceEquals(krieger, reiter));

            // und damit zeigt IndexOf auf die falsche Zeile
            var zeilen = new List<IEigenschaftler> { krieger, reiter };
            Assert.Equal(0, zeilen.IndexOf(reiter));
        }

        /// <summary>
        /// Die Suche über die Identität trifft die richtige Zeile.
        /// </summary>
        [Fact]
        public void DieSucheUeberDieIdentitaetTrifftDieRichtigeZeile() {
            var krieger = new Krieger { Nummer = 101, gf = 305, kf = 24 };
            var reiter = new Reiter { Nummer = 201, gf = 305, kf = 24 };
            var zeilen = new List<IEigenschaftler> { krieger, reiter };

            Assert.Equal(0, Gitterzeile.Finde(zeilen, krieger));
            Assert.Equal(1, Gitterzeile.Finde(zeilen, reiter));
        }

        /// <summary>
        /// Eine Figur, die das Gitter gerade nicht zeigt, hat keine Zeile - und das darf kein
        /// Fehler sein. Genau so sieht es aus, sobald im Kontextmenü der Figurenliste ein Filter
        /// gesetzt ist: dann steht dort nur ein Teil der Armee, und bewegen lässt sich trotzdem
        /// jede Figur. Vorher kam dabei -1 heraus und der Zugriff auf die Liste warf.
        /// </summary>
        [Fact]
        public void EineNichtAngezeigteFigurHatKeineZeile() {
            var angezeigt = new Krieger { Nummer = 101, gf = 305, kf = 24 };
            var gefiltert = new Zauberer { Nummer = 501, gf = 305, kf = 24 };
            var zeilen = new List<IEigenschaftler> { angezeigt };

            Assert.Equal(-1, Gitterzeile.Finde(zeilen, gefiltert));
            Assert.Equal(-1, Gitterzeile.Finde(null, angezeigt));
            Assert.Equal(-1, Gitterzeile.Finde(zeilen, null));
        }

        /// <summary>
        /// Die zweite Voraussetzung: verschiedene Figurenarten haben verschieden viele
        /// Eigenschaften. Die Spalten kamen aus der <em>ersten</em> Zeile des Gitters, die Werte
        /// aber aus der aufzufrischenden Figur - und sobald die kürzer war, lief die Schleife über
        /// ihr Ende hinaus. Das war der Absturz.
        ///
        /// Der Test nennt keine Zahlen: wieviele Eigenschaften eine Art hat, darf sich ändern.
        /// Dass sie sich unterscheiden, ist der Punkt.
        /// </summary>
        [Fact]
        public void VerschiedeneFigurenartenHabenVerschiedenVieleEigenschaften() {
            var arten = new List<IEigenschaftler> {
                new Krieger { Nummer = 101 },
                new Reiter { Nummer = 201 },
                new Schiffe { Nummer = 301 },
                new Zauberer { Nummer = 501 },
                new Character { Nummer = 601 },
            };

            var anzahlen = arten.Select(art => art.Eigenschaften.Count).ToList();
            Assert.All(anzahlen, anzahl => Assert.True(anzahl > 0, "Eine Figurenart hat überhaupt keine Eigenschaften"));
            Assert.True(anzahlen.Distinct().Count() > 1,
                "Alle Figurenarten haben gleich viele Eigenschaften - dann ist die Begrenzung in "
                + "SelectFigurPage.FrischeZeileAuf nicht mehr begründet und der Kommentar dort irrt");
        }
    }
}
