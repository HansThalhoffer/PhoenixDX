using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Wer ist Gegner, wer ist Alliierter (Regelwerk 5).
    ///
    /// "Es gibt nur „Gegner“ oder „Alliierte“. „Neutral“ gibt es nicht. Ein Gegner wird zum
    /// Alliierten wenn die Küstengewässerregel, bzw. Wegerecht ausgesprochen wurde, in
    /// Abhängigkeit des jeweiligen Geländes (Wasser oder Land)."
    ///
    /// Das Beispiel des Regelwerks macht die Geländeabhängigkeit deutlich: hat Reich A dem Reich B
    /// das Küstengewässerrecht eingeräumt, ist eine Flotte von B im Küstengewässer ein Alliierter,
    /// ein Landheer von B auf Land von A aber weiterhin ein Gegner. Dasselbe Reichspaar kann also
    /// auf dem Wasser verbündet und an Land verfeindet sein.
    ///
    /// Die Rechte stehen in der Tabelle Reich_crossref. Jede Zeile gehört einem Reich und hält den
    /// Stand zu einem anderen fest: was es vergeben hat (Wegerecht, Kuestenrecht) und was es
    /// erhalten hat (Wegerecht_von, Kuestenrecht_von). Beide Seiten führen eigene Zeilen, und im
    /// Datenbestand ist regelmässig nur eine davon gepflegt - von 132 Zeilen tragen 59 überhaupt
    /// einen Wert. Deshalb wird immer in beide Richtungen nachgesehen: ein Recht gilt, wenn der
    /// Geber es vergeben oder der Empfänger es erhalten hat.
    /// </summary>
    public static class DiplomatieRules {

        /// <summary>
        /// Hat der Geber dem Empfänger das Wegerecht eingeräumt?
        /// </summary>
        public static bool HatWegerecht(Nation? geber, Nation? empfänger)
            => Gewährt(geber, empfänger, zeile => zeile.Wegerecht, zeile => zeile.Wegerecht_von);

        /// <summary>
        /// Hat der Geber dem Empfänger das Küstenrecht eingeräumt?
        /// </summary>
        public static bool HatKüstenrecht(Nation? geber, Nation? empfänger)
            => Gewährt(geber, empfänger, zeile => zeile.Kuestenrecht, zeile => zeile.Kuestenrecht_von);

        /// <summary>
        /// Das Recht, auf das es auf dieser Gemark ankommt: auf dem Wasser das Küstenrecht, an
        /// Land das Wegerecht.
        /// </summary>
        public static bool HatRecht(Nation? geber, Nation? empfänger, KleinFeld? gemark)
            => gemark != null && gemark.IsWasser
                ? HatKüstenrecht(geber, empfänger)
                : HatWegerecht(geber, empfänger);

        /// <summary>
        /// Stehen diese beiden Reiche auf dieser Gemark als Alliierte?
        ///
        /// Das Recht gehört dem, dem die Gemark gehört: "Reich A hat Reich B Küstengewässerrecht
        /// eingeräumt" - gemeint ist das Gewässer von A. Gehört die Gemark keinem der beiden, gibt
        /// es dort auch kein Hausrecht zu gewähren; dann zählt ein Recht in eine beliebige
        /// Richtung, weil das Regelwerk nur "Gegner oder Alliierte" kennt und ein ausgesprochenes
        /// Recht die beiden zu Alliierten macht.
        /// </summary>
        public static bool SindAlliiert(Nation? eines, Nation? anderes, KleinFeld? gemark) {
            if (eines == null || anderes == null)
                return false;
            if (eines.Equals(anderes))
                return true;

            if (gemark?.Nation != null) {
                if (gemark.Nation.Equals(eines))
                    return HatRecht(eines, anderes, gemark);
                if (gemark.Nation.Equals(anderes))
                    return HatRecht(anderes, eines, gemark);
            }
            return HatRecht(eines, anderes, gemark) || HatRecht(anderes, eines, gemark);
        }

        /// <summary>
        /// Stehen diese beiden Reiche auf dieser Gemark als Gegner? "Neutral gibt es nicht."
        /// </summary>
        public static bool SindVerfeindet(Nation? eines, Nation? anderes, KleinFeld? gemark)
            => eines != null && anderes != null && SindAlliiert(eines, anderes, gemark) == false;

        /// <summary>
        /// Sieht in beide Richtungen nach: hat der Geber es vergeben, oder der Empfänger es
        /// erhalten?
        /// </summary>
        private static bool Gewährt(Nation? geber, Nation? empfänger,
                Func<ReichCrossref, int> vergeben, Func<ReichCrossref, int> erhalten) {
            if (geber == null || empfänger == null)
                return false;
            if (geber.Equals(empfänger))
                return true;
            if (SharedData.Diplomatie == null)
                return false;

            foreach (var zeile in SharedData.Diplomatie.Values) {
                if (zeile.ReferenzNation.Equals(geber) && zeile.Nation.Equals(empfänger) && vergeben(zeile) > 0)
                    return true;
                if (zeile.ReferenzNation.Equals(empfänger) && zeile.Nation.Equals(geber) && erhalten(zeile) > 0)
                    return true;
            }
            return false;
        }
    }
}
