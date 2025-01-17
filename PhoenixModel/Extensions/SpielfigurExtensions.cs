using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Extensions {
    public static class SpielfigurExtensions {

        /// <summary>
        /// Setzt eine Figur auf ein Kleinfeld - keine Bewegung
        /// </summary>
        /// <param name="spielfigur">Die Spielfigur, die gesetzt wird.</param>
        /// <param name="kleinfeld">Das Kleinfeld, auf das die Figur gesetzt werden soll.</param>
        public static void PutOnKleinfeld(this Spielfigur spielfigur, KleinFeld kleinfeld) {
            if (spielfigur.gf > 0 || !string.IsNullOrEmpty(spielfigur.ph_xy) || spielfigur.kf > 0) {
                ProgramView.LogError(
                    $"Fehler bei dem Setzen der Figur {spielfigur.Bezeichner} auf {kleinfeld.Bezeichner}",
                    "Eine bereits auf dem Spielfeld befindliche Figur darf nicht erneut gesetzt werden, sondern muss bewegt werden."
                );
                return;
            }

            spielfigur.ph_xy = kleinfeld.ph_xy;
            spielfigur.gf = kleinfeld.gf;
            spielfigur.kf = kleinfeld.kf;
        }
    }
}
