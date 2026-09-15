using PhoenixDX.Helper;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Womit die Karte geschoben wird.
    ///
    /// Frueher mit der rechten Maustaste - die oeffnet aber auch das Kontextmenue, und beides auf
    /// derselben Taste ist unguenstig. Jetzt schiebt die mittlere Taste, und fuer Maeuse ohne Rad
    /// zusaetzlich Strg mit der linken.
    ///
    /// Das Zusammenspiel mit der Kartendarstellung laesst sich hier nicht pruefen - das ist
    /// MonoGame. Geprueft wird die Entscheidung, und die liegt ausserhalb davon.
    /// </summary>
    public class KartensteuerungTest {

        private static MausEventArgs Maus(MausEventArgs.MouseButtonState links = MausEventArgs.MouseButtonState.Released,
                                          MausEventArgs.MouseButtonState mitte = MausEventArgs.MouseButtonState.Released,
                                          MausEventArgs.MouseButtonState rechts = MausEventArgs.MouseButtonState.Released) {
            return new MausEventArgs { LeftButton = links, MiddleButton = mitte, RightButton = rechts };
        }

        private const MausEventArgs.MouseButtonState Gedrückt = MausEventArgs.MouseButtonState.Pressed;

        /// <summary>
        /// Die mittlere Taste schiebt - mit und ohne Strg.
        /// </summary>
        [Fact]
        public void DieMittlereTasteSchiebt() {
            Assert.True(Kartensteuerung.SchiebtDieKarte(Maus(mitte: Gedrückt), strgGedrückt: false));
            Assert.True(Kartensteuerung.SchiebtDieKarte(Maus(mitte: Gedrückt), strgGedrückt: true));
        }

        /// <summary>
        /// Die linke Taste schiebt nur mit Strg - ohne waehlt sie aus, wie eh und je.
        /// </summary>
        [Fact]
        public void DieLinkeTasteSchiebtNurMitStrg() {
            Assert.True(Kartensteuerung.SchiebtDieKarte(Maus(links: Gedrückt), strgGedrückt: true));
            Assert.False(Kartensteuerung.SchiebtDieKarte(Maus(links: Gedrückt), strgGedrückt: false));
            Assert.True(Kartensteuerung.BeginntSchiebenMitLinks(true));
            Assert.False(Kartensteuerung.BeginntSchiebenMitLinks(false));
        }

        /// <summary>
        /// Die rechte Taste schiebt gar nicht mehr. Sie oeffnet das Kontextmenue, und das soll
        /// nicht mehr davon abhaengen, ob die Hand dabei ruhig war.
        /// </summary>
        [Fact]
        public void DieRechteTasteSchiebtNichtMehr() {
            Assert.False(Kartensteuerung.SchiebtDieKarte(Maus(rechts: Gedrückt), strgGedrückt: false));
            Assert.False(Kartensteuerung.SchiebtDieKarte(Maus(rechts: Gedrückt), strgGedrückt: true));
        }

        /// <summary>
        /// Ohne gedrueckte Taste wird nicht geschoben, und ohne Mauszustand erst recht nicht.
        /// </summary>
        [Fact]
        public void OhneTasteWirdNichtGeschoben() {
            Assert.False(Kartensteuerung.SchiebtDieKarte(Maus(), strgGedrückt: false));
            Assert.False(Kartensteuerung.SchiebtDieKarte(Maus(), strgGedrückt: true));
            Assert.False(Kartensteuerung.SchiebtDieKarte(null, strgGedrückt: true));
        }

        /// <summary>
        /// Ein Zittern der Hand ist noch kein Schieben - sonst verschluckte die kleinste Bewegung
        /// die Markierung, die der Strg-Klick setzt.
        /// </summary>
        [Fact]
        public void EinZitternIstNochKeinSchieben() {
            Assert.False(Kartensteuerung.IstSchieben(null));
            Assert.False(Kartensteuerung.IstSchieben(new Position(0, 0)));
            Assert.False(Kartensteuerung.IstSchieben(new Position(Kartensteuerung.SchiebeSchwelle, 0)));
            Assert.False(Kartensteuerung.IstSchieben(new Position(-2, 1)));

            Assert.True(Kartensteuerung.IstSchieben(new Position(Kartensteuerung.SchiebeSchwelle + 1, 0)));
            Assert.True(Kartensteuerung.IstSchieben(new Position(0, -(Kartensteuerung.SchiebeSchwelle + 1))));
            // in der Summe beider Richtungen
            Assert.True(Kartensteuerung.IstSchieben(new Position(2, 3)));
        }
    }
}
