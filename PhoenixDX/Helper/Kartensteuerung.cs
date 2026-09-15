using System;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ViewModel;

namespace PhoenixDX.Helper {

    /// <summary>
    /// Womit die Karte geschoben wird.
    ///
    /// Früher mit der rechten Maustaste. Die öffnet aber auch das Kontextmenü, und beides auf
    /// derselben Taste ist ungünstig: jedes Schieben musste erst als Schieben erkannt werden,
    /// bevor feststand, dass kein Menü aufgeht - und wer die Maus beim Loslassen ruhig hielt,
    /// bekam das Menü trotzdem.
    ///
    /// Jetzt schiebt die <b>mittlere</b> Taste, also die unter dem Mausrad. Wer keine hat, hält
    /// <b>Strg</b> und zieht mit der linken. Die rechte Taste tut nur noch eines: das Kontextmenü
    /// öffnen.
    ///
    /// Strg und linke Taste sind schon vergeben - Strg+Klick setzt eine Markierung auf einem Feld
    /// des eigenen Reiches. Beides verträgt sich, weil erst das Loslassen entscheidet: wurde
    /// dazwischen geschoben, war es kein Klick.
    /// </summary>
    public static class Kartensteuerung {

        /// <summary>
        /// Ab wieviel Bildpunkten Bewegung ein Schieben gemeint ist und kein Klick. Ein Zittern
        /// der Hand soll die Markierung nicht verschlucken.
        /// </summary>
        public const int SchiebeSchwelle = 3;

        /// <summary>
        /// Schiebt dieser Mauszustand gerade die Karte?
        /// </summary>
        /// <param name="maus">der aktuelle Mauszustand</param>
        /// <param name="strgGedrückt">ist eine der beiden Strg-Tasten unten?</param>
        public static bool SchiebtDieKarte(MausEventArgs? maus, bool strgGedrückt) {
            if (maus == null)
                return false;
            if (maus.MiddleButton == MausEventArgs.MouseButtonState.Pressed)
                return true;
            return strgGedrückt && maus.LeftButton == MausEventArgs.MouseButtonState.Pressed;
        }

        /// <summary>
        /// War diese Mausbewegung ein Schieben - oder nur ein Zittern?
        /// </summary>
        public static bool IstSchieben(Position? delta) {
            if (delta == null)
                return false;
            return Math.Abs(delta.X) + Math.Abs(delta.Y) > SchiebeSchwelle;
        }

        /// <summary>
        /// Beginnt mit dieser gedrückten linken Taste ein Schieben statt einer Auswahl?
        ///
        /// Nur mit Strg. Ohne Strg wählt die linke Taste wie bisher sofort aus - daran soll sich
        /// nichts ändern, das ist der häufigste Griff überhaupt.
        /// </summary>
        public static bool BeginntSchiebenMitLinks(bool strgGedrückt) => strgGedrückt;
    }
}
