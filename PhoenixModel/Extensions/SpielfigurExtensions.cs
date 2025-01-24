using PhoenixModel.Commands;
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

        /// <summary>
        /// Ermittelt den aktuellen Wert von gf (aus gf_nach oder gf_von)
        /// </summary>
        public static int GetGf(this Spielfigur spielfigur) {
            return spielfigur.gf_nach > 0 ? spielfigur.gf_nach : spielfigur.gf_von;
        }

        /// <summary>
        /// Setzt den Wert für gf, wobei gf_von initialisiert wird
        /// </summary>
        public static void SetGf(this Spielfigur spielfigur, int value) {
            if (spielfigur.gf_von == 0)
                spielfigur.gf_von = value;
            spielfigur.gf_nach = value;
        }

        /// <summary>
        /// Ermittelt den aktuellen Wert von kf (aus kf_nach oder kf_von).
        /// </summary>
        public static int GetKf(this Spielfigur spielfigur) {
            return spielfigur.kf_nach > 0 ? spielfigur.kf_nach : spielfigur.kf_von;
        }

        /// <summary>
        /// Setzt den Wert für kf, wobei kf_von initialisiert wird.
        /// </summary>
        public static void SetKf(this Spielfigur spielfigur, int value) {
            if (spielfigur.kf_von == 0)
                spielfigur.kf_von = value;
            spielfigur.kf_nach = value;
        }

        /// <summary>
        /// Holt den Wert von gf_nach.
        /// </summary>
        public static int GetGfNach(this Spielfigur spielfigur) {
            return spielfigur._gfNach;
        }

        /// <summary>
        /// Setzt den Wert von gf_nach und löst OnPropertyChanged aus.
        /// </summary>
        public static void SetGfNach(this Spielfigur spielfigur, int value) {
            if (spielfigur._gfNach != value) {
                spielfigur._gfNach = value;
                spielfigur.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Holt den Wert von kf_nach.
        /// </summary>
        public static int GetKfNach(this Spielfigur spielfigur) {
            return spielfigur._kfNach;
        }

        /// <summary>
        /// Setzt den Wert von kf_nach und löst OnPropertyChanged aus.
        /// </summary>
        public static void SetKfNach(this Spielfigur spielfigur, int value) {
            if (spielfigur._kfNach != value) {
                spielfigur._kfNach = value;
                spielfigur.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Weist die Spielfigur der ausgewählten Nation zu.
        /// </summary>
        /// <param name="spielfigur">Die Spielfigur, die zugewiesen wird.</param>
        /// <exception cref="InvalidOperationException">Wird ausgelöst, wenn keine Nation ausgewählt ist.</exception>
        public static void AssignToSelectedReich(this Spielfigur spielfigur) {
            if (ProgramView.SelectedNation == null)
                throw new InvalidOperationException("Zuerst muss eine Nation ausgewählt sein.");

            spielfigur.Nation = ProgramView.SelectedNation;
        }

        public static IEnumerable<ICommand> GetCommands(this Spielfigur spielfigur) {
            return SharedData.Commands.GetValues(spielfigur);
        }
    }
}
