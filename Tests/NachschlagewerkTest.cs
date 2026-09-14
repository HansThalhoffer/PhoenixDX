using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Bestiarium und Personalliste - zwei Tabellen, die im Datenmodell lagen, aber nie geladen
    /// wurden.
    ///
    /// Geprueft wird, dass sie ankommen und sich nachschlagen lassen. Aus der Personalliste steht
    /// hier bewusst kein einziger Wert: darin stehen Namen, Anschriften und Kontaktdaten
    /// wirklicher Menschen, und ein Test ist ein denkbar schlechter Ort dafuer.
    /// </summary>
    public class NachschlagewerkTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        /// <summary>
        /// Das Bestiarium steht in der Kartendatenbank und wird beim Laden der Karte gelesen.
        /// </summary>
        [StaFact]
        public void DasBestiariumWirdGeladen() {
            LadeAlles();

            Assert.NotNull(SharedData.Bestiarium);
            Assert.True(SharedData.Bestiarium!.Count > 0, "Das Bestiarium ist leer");

            var kreaturen = NachschlagewerkView.GetKreaturen();
            Assert.Equal(SharedData.Bestiarium.Count, kreaturen.Count);

            // sortiert nach Namen
            var namen = kreaturen.Select(k => k.Kreaturenname ?? string.Empty).ToList();
            Assert.Equal(namen.OrderBy(n => n).ToList(), namen);

            // jede Kreatur hat einen Namen, sonst laesst sie sich nicht nachschlagen
            Assert.All(kreaturen, k => Assert.False(string.IsNullOrWhiteSpace(k.Kreaturenname)));
        }

        /// <summary>
        /// Nachgeschlagen wird ueber den Namen, ohne Ruecksicht auf Gross- und Kleinschreibung.
        /// </summary>
        [StaFact]
        public void EineKreaturLaesstSichNachschlagen() {
            LadeAlles();
            var erste = NachschlagewerkView.GetKreaturen().First();
            string name = erste.Kreaturenname!;

            Assert.Same(erste, NachschlagewerkView.GetKreatur(name));
            Assert.Same(erste, NachschlagewerkView.GetKreatur(name.ToUpperInvariant()));
            Assert.Same(erste, NachschlagewerkView.GetKreatur($"  {name}  "));

            Assert.Null(NachschlagewerkView.GetKreatur("Es war einmal ein Drache, den gab es nicht"));
            Assert.Null(NachschlagewerkView.GetKreatur(null));
            Assert.Null(NachschlagewerkView.GetKreatur("   "));
        }

        /// <summary>
        /// Die Eintraege tragen die Werte, auf die es im Spiel ankommt: Gutpunkte, Heerfuehrer,
        /// Staerke und Baupunkte.
        /// </summary>
        [StaFact]
        public void DieKreaturenTragenIhreWerte() {
            LadeAlles();
            var kreaturen = NachschlagewerkView.GetKreaturen();

            // mindestens eine Kreatur hat Gutpunkte und eine Staerke - sonst waere die Tabelle
            // leer gelesen worden
            Assert.Contains(kreaturen, k => k.GP > 0);
            Assert.Contains(kreaturen, k => k.Stärke > 0);

            // und sie lassen sich als Eigenschaften anzeigen
            Assert.NotEmpty(kreaturen.First().Eigenschaften);
            Assert.False(string.IsNullOrWhiteSpace(kreaturen.First().Bezeichner));
        }

        /// <summary>
        /// Die Personalliste steht in den Zugdaten und wird beim Laden gelesen.
        ///
        /// Geprueft wird nur, dass sie ankommt und sich anzeigen laesst - keine Inhalte.
        /// </summary>
        [StaFact]
        public void DiePersonallisteWirdGeladen() {
            LadeAlles();

            Assert.NotNull(SharedData.Personal);

            var personal = NachschlagewerkView.GetPersonal();
            Assert.Equal(SharedData.Personal!.Count, personal.Count);

            if (personal.Count > 0) {
                Assert.NotEmpty(personal[0].Eigenschaften);
                Assert.False(string.IsNullOrWhiteSpace(personal[0].Bezeichner));
            }
        }

        /// <summary>
        /// Beide Listen lassen sich an das Anzeigegitter geben.
        /// </summary>
        [StaFact]
        public void BeideListenPassenInsGitter() {
            LadeAlles();

            var kreaturen = NachschlagewerkView.AlsEigenschaftler(NachschlagewerkView.GetKreaturen());
            Assert.Equal(NachschlagewerkView.GetKreaturen().Count, kreaturen.Count);

            var personal = NachschlagewerkView.AlsEigenschaftler(NachschlagewerkView.GetPersonal());
            Assert.Equal(NachschlagewerkView.GetPersonal().Count, personal.Count);

            Assert.Empty(NachschlagewerkView.AlsEigenschaftler(null));
        }
    }
}
