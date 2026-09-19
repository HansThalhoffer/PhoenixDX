using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PhoenixModel.View;

namespace PhoenixDX.Helper {

    /// <summary>
    /// Welche Farbe zu welcher Markierung auf der Karte gehört.
    ///
    /// Die Tabelle steht für sich und nicht im Laden der Texturen, damit sie ohne Grafik geprüft
    /// werden kann: dass jeder Markertyp eine Farbe hat und dass keine zweimal vergeben ist.
    ///
    /// <b>Die Farben müssen sich vom Gelände abheben, nicht nur voneinander.</b> Die Karte ist
    /// grün (Wald, Tiefland), oliv-braun (Bergland, Hochland), blau (Wasser) und sandgelb (Wüste).
    /// Die Reichweite stand auf Hellgrün und war auf Tiefland und im Wald kaum zu erkennen -
    /// gemeldet als "besonders grün auf grünen Feldern". Der zurückgelegte Weg stand auf
    /// Kornblumenblau und hatte dasselbe Problem über Wasser, wo Schiffe nun einmal fahren.
    ///
    /// Magenta kommt in keinem Gelände vor; das ist die Farbe der Reichweite. Der zurückgelegte
    /// Weg ist weiss - das ist die Farbe mit dem grössten Abstand zu Magenta und zugleich mit
    /// genug Abstand zu jedem Gelände. Schwarz waere naheliegend gewesen, liegt aber zu dicht am
    /// dunklen Waldgrün; der Test hat es gefangen.
    ///
    /// Der knappste Fall ist Weiss auf Wüste. Wüste ist selten, und der Weg ist eine
    /// zusammenhängende Kette von Feldern - er wird auch dann als Linie erkannt.
    ///
    /// Gezeichnet wird eine getönte Hexkachel über dem Gelände, die Tönung multipliziert. Helle
    /// Farben bleiben deshalb hell und blasse bleiben blass; auf sandfarbenem Gelände nützt das
    /// nichts. Wer hier etwas ändert, prüfe es gegen Wald, Tiefland, Wasser und Wüste.
    /// </summary>
    public static class Markerfarben {

        /// <summary>
        /// Die Farbe je Markertyp. <see cref="MarkerType.None"/> steht nicht darin - das ist
        /// keine Markierung, sondern ihre Abwesenheit.
        /// </summary>
        public static readonly IReadOnlyDictionary<MarkerType, Color> Farben =
            new Dictionary<MarkerType, Color> {
                { MarkerType.User, Color.Turquoise },
                { MarkerType.Info, Color.Yellow },
                { MarkerType.Warning, Color.Orange },
                { MarkerType.Fatality, Color.Red },
                { MarkerType.Bewegung, Color.Magenta },
                { MarkerType.Weg, Color.White },
            };
    }
}
