using PhoenixModel.dbZugdaten;
using PhoenixModel.Extensions;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was als eingeschifft gilt.
    ///
    /// In den echten Zugdaten steht in auf_Flotte bei nicht eingeschifften Truppen nicht immer
    /// nichts, sondern manchmal ein einzelnes Leerzeichen - so kommen die Daten von der
    /// Spielleitung. Eine Pruefung auf IsNullOrEmpty haelt das fuer eine Flottennummer und legt die
    /// Einheit fuer den ganzen Zug lahm. Auffallen kann das kaum: eine eingeschiffte Truppe darf
    /// sich zu Recht nicht bewegen, die Einheit steht einfach da und niemand weiss warum.
    /// </summary>
    public class EinschiffungTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            BewegungsRules.ResetCache();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>
        /// Ein Leerzeichen ist keine Flotte.
        /// </summary>
        [StaFact]
        public void EinLeerzeichenInAufFlotteIstKeineEinschiffung() {
            LadeAlles();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(t => Plausibilität.IsValid(t));
            Assert.True(truppe != null, "Das eigene Reich hat keine Truppen");

            string? vorher = truppe!.auf_Flotte;
            try {
                truppe.auf_Flotte = " ";
                Assert.False(truppe.IsOnShip(), "Ein Leerzeichen wird als Flotte gelesen");

                truppe.auf_Flotte = "   ";
                Assert.False(truppe.IsOnShip());

                truppe.auf_Flotte = string.Empty;
                Assert.False(truppe.IsOnShip());

                // eine echte Flottennummer dagegen schon
                truppe.auf_Flotte = "201";
                Assert.True(truppe.IsOnShip(), "Eine Flottennummer wird nicht erkannt");
            }
            finally {
                truppe.auf_Flotte = vorher;
            }
        }

        /// <summary>
        /// Und die Bewegung richtet sich danach: mit einem Leerzeichen in auf_Flotte muss dieselbe
        /// Figur dieselben Felder erreichen wie ohne.
        /// </summary>
        [StaFact]
        public void EinLeerzeichenLegtKeineEinheitLahm() {
            LadeAlles();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(Plausibilität.IsValid)
                .FirstOrDefault(t => BewegungsRules.GetErreichbareFelder(t).Count > 0);
            Assert.True(truppe != null, "Keine bewegliche eigene Truppe gefunden");

            int ohne = BewegungsRules.GetErreichbareFelder(truppe!).Count;
            string? vorher = truppe!.auf_Flotte;
            try {
                truppe.auf_Flotte = " ";
                Assert.Equal(ohne, BewegungsRules.GetErreichbareFelder(truppe).Count);

                // eine echte Flotte haelt sie dagegen fest
                truppe.auf_Flotte = "201";
                Assert.Empty(BewegungsRules.GetErreichbareFelder(truppe));
            }
            finally {
                truppe.auf_Flotte = vorher;
            }
        }

        /// <summary>
        /// In den echten Zugdaten steckt genau dieser Fall - und er kostet Einheiten ihren Zug.
        /// Scheitert dieser Test, weil es die Leerzeichen nicht mehr gibt, ist das kein Grund zur
        /// Sorge: dann hat die Spielleitung die Daten bereinigt.
        /// </summary>
        [StaFact]
        public void InDenEchtenZugdatenStehenSolcheLeerzeichen() {
            LadeAlles();
            var mitLeerzeichen = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(t => string.IsNullOrEmpty(t.auf_Flotte) == false && string.IsNullOrWhiteSpace(t.auf_Flotte))
                .ToList();

            if (mitLeerzeichen.Count == 0)
                return; // die Daten sind sauber, dann ist hier nichts zu zeigen

            // keine davon gilt als eingeschifft
            Assert.All(mitLeerzeichen, t => Assert.False(t.IsOnShip()));
            // und mindestens eine davon kommt tatsaechlich irgendwohin
            Assert.Contains(mitLeerzeichen, t => BewegungsRules.GetErreichbareFelder(t).Count > 0);
        }
    }
}
