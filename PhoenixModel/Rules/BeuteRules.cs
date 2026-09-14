using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln der Beute (Regelwerk 5.8).
    ///
    /// Es gibt drei Quellen: die Eroberung eines Rüstorts, die Kaperung zur See und die Ladung
    /// vernichteter Heere zu Land. Dazu kommen die Katapulte, die sich im Nahkampf immer ergeben.
    ///
    /// Gemeinsam ist allem, dass die Art der Ladung erhalten bleibt: "wobei die Art der Ladung
    /// (z.B. 'Besondere Einnahmen') bestehen bleibt" (5.8.3). Erbeutetes Gold aus einem Rüstort
    /// ist ausdrücklich eine besondere Einnahme (5.8.1) und unterliegt damit den Regeln aus 6.6 -
    /// es reist mit dem Heer und steht erst in einem eigenen Rüstort zum Rüsten bereit.
    /// </summary>
    public static class BeuteRules {

        /// <summary>
        /// Die Beute für die Eroberung eines Rüstorts (Regelwerk 5.8.1).
        ///
        /// "Für die Eroberung von Rüstorten erhält man Beute. Die Goldstücke aus der Beute sind
        /// besondere Einnahmen."
        ///
        /// Die Sprünge sind gewollt: Hauptstadt und Festungshauptstadt liegen um zwei
        /// Größenordnungen über den übrigen. So steht es im Regelwerk.
        /// </summary>
        /// <returns>die Goldstücke, oder 0 wenn dort nichts zu erobern war</returns>
        public static int GetRüstortbeute(Rüstort? rüstort) {
            if (rüstort == null)
                return 0;
            return rüstort.Ruestort switch {
                "Burg" => 2000,
                "Stadt" => 4000,
                "Festung" => 6000,
                "Hauptstadt" => 110000,
                "Festungshauptstadt" => 112000,
                _ => 0,
            };
        }

        /// <summary>
        /// Die Beute für die Eroberung des Rüstorts auf dieser Gemark.
        ///
        /// Ein beschädigter Rüstort zählt als das, was aus seinen Baupunkten wird - dieselbe
        /// Zuordnung wie beim Rüsten. Ein Dorf bringt nichts ein.
        /// </summary>
        public static int GetRüstortbeute(KleinFeld? gemark) {
            return gemark == null ? 0 : GetRüstortbeute(BauwerkeView.GetRüstortNachKarte(gemark));
        }

        /// <summary>
        /// Der Anteil der Ladung, der zur See gekapert wird (Regelwerk 5.8.2).
        ///
        /// "Anteilig von der Überlegenheit der Heeresstärke wird Ladung gekapert.
        ///  10 : 1 = 100%, 9:1 = 90%, 8:1 = 80% ect."
        ///
        /// Zehn Prozent je Vielfachem der gegnerischen Heeresstärke, höchstens alles. Ohne
        /// Gegner gibt es nichts zu kapern - wo kein Heer ist, ist auch keine Ladung.
        /// </summary>
        /// <returns>ein Anteil zwischen 0 und 1</returns>
        public static double BerechneKaperanteil(double eigeneHeeresstärke, double fremdeHeeresstärke) {
            if (eigeneHeeresstärke <= 0 || fremdeHeeresstärke <= 0)
                return 0;
            double verhältnis = eigeneHeeresstärke / fremdeHeeresstärke;
            return Math.Clamp(verhältnis / KampfRules.ÜbermachtVerhältnis, 0, 1);
        }

        /// <summary>
        /// Was von einer Ladung zur See gekapert wird.
        /// </summary>
        public static int BerechneSeebeute(double eigeneHeeresstärke, double fremdeHeeresstärke, int ladung) {
            if (ladung <= 0)
                return 0;
            return (int)Math.Floor(ladung * BerechneKaperanteil(eigeneHeeresstärke, fremdeHeeresstärke));
        }

        /// <summary>
        /// Was ein Heer an Ladung mit sich führt: mitgeführtes Gold und besondere Einnahmen.
        /// </summary>
        /// <param name="Gold">mitgeführtes Gold - gewöhnliche Mittel</param>
        /// <param name="Kampfeinnahmen">besondere Einnahmen, die ihren Charakter behalten</param>
        public record class Ladung(int Gold, int Kampfeinnahmen) {
            public static readonly Ladung Nichts = new(0, 0);
            public int Gesamt => Gold + Kampfeinnahmen;
            public Ladung Plus(Ladung andere) => new(Gold + andere.Gold, Kampfeinnahmen + andere.Kampfeinnahmen);
        }

        /// <summary>
        /// Die Ladung, die der Sieger zu Land übernimmt (Regelwerk 5.8.3).
        ///
        /// "Der Gewinner kann die mitgeführte Ladung aller vernichteten Heere (egal ob eigene,
        /// alliierte oder feindlich) als eigene Ladung weiter führen, wobei die Art der Ladung
        /// (z.B. 'Besondere Einnahmen') bestehen bleibt."
        ///
        /// Deshalb werden Gold und Kampfeinnahmen getrennt gezählt und nicht zu einer Summe
        /// verrührt: aus besonderen Einnahmen darf ausserhalb der Rüstmonate gerüstet werden,
        /// aus gewöhnlichem Gold nicht.
        /// </summary>
        public static Ladung BerechneLandbeute(IEnumerable<TruppenSpielfigur>? vernichtete) {
            if (vernichtete == null)
                return Ladung.Nichts;
            int gold = 0, einnahmen = 0;
            foreach (var heer in vernichtete) {
                gold += heer.GS;
                einnahmen += heer.Kampfeinnahmen;
            }
            return new Ladung(gold, einnahmen);
        }

        /// <summary>
        /// Die Katapulte, die dem Sieger in die Hände fallen.
        ///
        /// "Katapulte nehmen nicht am Nahkampf teil. Sie ergeben sich im Nahkampf also immer
        /// automatisch (unabhängig von einer 10:1 Übermacht)." (Regelwerk 5.5)
        ///
        /// Ist das Heer aufgerieben, bleibt nur die Hälfte übrig: "Wird ein LKP/SKP Heer
        /// aufgerieben so werden 50% der noch vorhandenen Katapulte als zerstört angesehen. Die
        /// restlichen Katapulte werden vom Sieger erobert." (Regelwerk 5.5) Angebrochene Katapulte
        /// gibt es nicht, deshalb wird abgerundet.
        ///
        /// Das gilt für die Katapulte zu Land. Kriegsschiffe sind keine Katapulte in diesem Sinne:
        /// sie verteidigen sich, solange sie nicht wehrlos sind.
        /// </summary>
        /// <param name="verlierer">die unterlegenen Heere</param>
        /// <param name="aufgerieben">ist von den Heeren nichts übrig geblieben?</param>
        /// <returns>die leichten und die schweren Katapulte, die der Sieger übernimmt</returns>
        public static (int Leichte, int Schwere) BerechneKatapultbeute(IEnumerable<TruppenSpielfigur>? verlierer, bool aufgerieben = false) {
            if (verlierer == null)
                return (0, 0);
            int leichte = 0, schwere = 0;
            foreach (var heer in verlierer) {
                if (heer.BaseTyp == PhoenixModel.ExternalTables.FigurType.Schiff)
                    continue;
                leichte += heer.LKP;
                schwere += heer.SKP;
            }
            return aufgerieben ? (leichte / 2, schwere / 2) : (leichte, schwere);
        }

        /// <summary>
        /// Schreibt die Beute einem Heer gut.
        ///
        /// Erbeutetes Gold aus einem Rüstort ist eine besondere Einnahme (5.8.1) und wird deshalb
        /// den Kampfeinnahmen zugeschlagen, nicht dem mitgeführten Gold. Die Art der Ladung bleibt
        /// erhalten: was als Gold kam, bleibt Gold; was als besondere Einnahme kam, bleibt eine.
        /// </summary>
        public static void SchreibeGut(TruppenSpielfigur? sieger, Ladung beute, int rüstortbeute = 0) {
            if (sieger == null)
                return;
            sieger.GS += beute.Gold;
            sieger.Kampfeinnahmen += beute.Kampfeinnahmen + rüstortbeute;
        }
    }
}
