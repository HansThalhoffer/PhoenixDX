using PhoenixModel.Commands;
using PhoenixModel.View;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Kampfeinnahmen des Siegers (Regelwerk 5.6).
    ///
    /// "Die Kampfeinnahmen des Siegers bestehen aus 50 % der Summe aller im Nahkampf dieser
    /// Schlacht gefallenen gegnerischen Truppen und 25 % aller in diesem Nahkampf gefallenen
    /// eigenen Truppen."
    ///
    /// Gemeint ist der Wert der Gefallenen in Goldstücken, gerechnet mit den Rüstkosten aus der
    /// Kostentabelle - genau das tut auch die Kampftabelle (D326 und D327), die dieselben Preise
    /// als Faktoren einsetzt und die Summe durch 4 beziehungsweise durch 2 teilt.
    ///
    /// Nur der Nahkampf zählt: "Für im Fernkampf vernichtete Rüstgüter gibt es keine
    /// Kampfeinnahmen!" (Regelwerk 5.1) Wer vor dem Nahkampf unter Beschuss fällt, bringt also
    /// nichts ein. Deshalb gehen hier die Verluste aus der Schlacht ein und nicht die des
    /// Beschusses; die Kampftabelle kommt auf demselben Weg dorthin, indem sie mit den Werten
    /// nach Katabeschuss weiterrechnet.
    ///
    /// Kampfeinnahmen sind besondere Einnahmen: sie reisen mit dem Heer und stehen erst in einem
    /// eigenen Rüstort zum Rüsten bereit (Regelwerk 6.6). Gutgeschrieben werden sie über
    /// <see cref="BeuteRules.SchreibeGut"/>.
    ///
    /// Was die Kampftabelle unter derselben Überschrift noch aufaddiert - die Ladung des Gegners
    /// und die eigene verlorene Ladung (D324 und D328) - gehört zur Beute und steht in
    /// <see cref="BeuteRules"/>.
    /// </summary>
    public static class KampfeinnahmenRules {

        /// <summary>
        /// "50 % der Summe aller im Nahkampf dieser Schlacht gefallenen gegnerischen Truppen"
        /// (Kampftabelle D327: die Summe wird durch 2 geteilt)
        /// </summary>
        public const double AnteilGegner = 0.5;

        /// <summary>
        /// "25 % aller in diesem Nahkampf gefallenen eigenen Truppen"
        /// (Kampftabelle D326: die Summe wird durch 4 geteilt)
        /// </summary>
        public const double AnteilEigene = 0.25;

        /// <summary>
        /// Was ein Verlust in Goldstücken wert ist - zu Rüstkosten.
        ///
        /// Die Preise stehen in der Kostentabelle der Crossreferenz; die Kampftabelle setzt
        /// dieselben Zahlen als Faktoren ein (Zeilen 326 und 327). Ohne geladene Kostentabelle
        /// kommt 0 heraus, nicht ein geratener Wert.
        /// </summary>
        public static int BerechneWert(KampfRules.Verluste? verluste) {
            if (verluste == null)
                return 0;
            return verluste.Krieger * KostenView.GetGSKosten(ConstructionElementType.K)
                 + verluste.Reiter * KostenView.GetGSKosten(ConstructionElementType.R)
                 + verluste.Pferde * KostenView.GetGSKosten(ConstructionElementType.P)
                 + verluste.Schiffe * KostenView.GetGSKosten(ConstructionElementType.S)
                 + verluste.Heerführer * KostenView.GetGSKosten(ConstructionElementType.HF)
                 + verluste.LKP * KostenView.GetGSKosten(ConstructionElementType.LKP)
                 + verluste.SKP * KostenView.GetGSKosten(ConstructionElementType.SKP)
                 + verluste.LKS * KostenView.GetGSKosten(ConstructionElementType.LKS)
                 + verluste.SKS * KostenView.GetGSKosten(ConstructionElementType.SKS);
        }

        /// <summary>
        /// Der Wert mehrerer Verluste zusammen
        /// </summary>
        public static int BerechneWert(IEnumerable<KampfRules.Verluste>? verluste)
            => verluste == null ? 0 : verluste.Sum(BerechneWert);

        /// <summary>
        /// Die Kampfeinnahmen aus den Verlusten beider Seiten.
        ///
        /// Abgerundet, wie die Kampftabelle es tut (D329: TRUNC).
        /// </summary>
        /// <param name="gefalleneGegner">die Nahkampfverluste der unterlegenen Seite</param>
        /// <param name="eigeneGefallene">die eigenen Nahkampfverluste</param>
        public static int Berechne(IEnumerable<KampfRules.Verluste>? gefalleneGegner,
                                   IEnumerable<KampfRules.Verluste>? eigeneGefallene) {
            double summe = BerechneWert(gefalleneGegner) * AnteilGegner
                         + BerechneWert(eigeneGefallene) * AnteilEigene;
            return (int)Math.Max(0, Math.Truncate(summe));
        }

        /// <summary>
        /// Die Kampfeinnahmen des Siegers einer Schlacht.
        ///
        /// Ohne Sieger gibt es nichts: die Kampftabelle rechnet den Betrag nur aus, wenn die Seite
        /// gewonnen hat (D329), und das Regelwerk spricht ausdrücklich von den "Kampfeinnahmen des
        /// Siegers".
        /// </summary>
        /// <returns>die Goldstücke, die dem Sieger zustehen</returns>
        public static int Berechne(NahkampfRules.Schlachtergebnis? schlacht) {
            if (schlacht == null || schlacht.Sieger == NahkampfRules.Ausgang.Unentschieden)
                return 0;

            var sieger = schlacht.Sieger == NahkampfRules.Ausgang.Angreifer
                ? schlacht.Angreifer
                : schlacht.Verteidiger;

            return Berechne(schlacht.Verlierer.Select(ausgang => ausgang.Verluste),
                            sieger.Select(ausgang => ausgang.Verluste));
        }
    }
}
