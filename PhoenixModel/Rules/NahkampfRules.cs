using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Der Nahkampf, von Anfang bis Ende (Regelwerk 5.5, Kampftabelle Zeilen 141 bis 209).
    ///
    /// Die Bausteine dafür liegen in <see cref="KampfRules"/> - Gutpunkte, Heeresstärke,
    /// Kampfstärke, Grössenfaktor, Verlustverteilung. Hier werden sie in die Reihenfolge gebracht,
    /// in der die Kampftabelle sie abarbeitet, und zu einem Ergebnis zusammengefasst.
    ///
    /// Der Ablauf:
    ///
    /// 1. Die Heeresstärken beider Seiten aufsummieren. Steht eine Seite zehnfach über der
    ///    anderen, wird überrannt statt gekämpft - die unterlegene Seite ist wehrlos.
    /// 2. Sonst entscheidet die Kampfstärke über Sieg und Niederlage (Kampftabelle B180).
    /// 3. Die Verluste: Basis ist die Heeresstärke des Gegners. Beim Sieger verschiebt der
    ///    Grössenfaktor sie, beim Verlierer nicht (Kampftabelle C187: der Faktor gilt nur, wenn
    ///    die Seite gewonnen hat).
    /// 4. Die Verluste einer Seite verteilen sich auf ihre Heere nach Heeresstärke und werden je
    ///    Heer an der Gutpunktdifferenz zum Schnitt der Gegenseite gemessen.
    /// 5. Innerhalb eines Heeres trifft der Verlustanteil jede Gattung - auch Heerführer und
    ///    Fernkampfwaffen.
    ///
    /// Eine Folge dieser Rechnung ist bemerkenswert genug, um sie festzuhalten: der Verlierer
    /// einer Schlacht wird praktisch immer aufgerieben. Seine Verluste haben die volle
    /// Heeresstärke des Siegers als Basis, ohne mindernden Grössenfaktor, und die
    /// Gutpunktdifferenz zum Schnitt des Siegers vervielfacht sie meistens noch. So rechnet die
    /// Kampftabelle (C187 bis C189), und so steht es im durchgerechneten Beispiel des Regelwerks.
    /// Wer nicht untergehen will, muss sich zurückziehen - dafür gibt es das Rückzugsgefecht.
    ///
    /// Was hier nicht passiert: gewürfelt wird nicht. Der W20, den beide Seiten auf ihre
    /// Gutpunkte addieren (Regelwerk 5.5, Kampftabelle C144), gehört zur Spielleitung; er gehört
    /// in die Gutpunkte, die hier hereingereicht werden. Auch der Prozentwurf auf angebrochene
    /// Fernkampfwaffen bleibt aussen vor - <see cref="KampfRules.RundeMitWurf"/> bildet ihn nach,
    /// wo er gebraucht wird.
    ///
    /// Ebenfalls nicht hier: das Rückzugsgefecht (5.4), der Charakterkampf (5.3) und die
    /// Kampfeinnahmen (5.6).
    /// </summary>
    public static class NahkampfRules {

        /// <summary>
        /// Ein Heer im Nahkampf.
        /// </summary>
        /// <param name="Heer">das Heer, so wie es in den Nahkampf geht - also nach dem Beschuss</param>
        /// <param name="Gutpunkte">
        /// seine Gutpunkte: die Vorteile aus Gelände und Rüstort, die Heerführer, Charaktere und
        /// Zauberer und der W20 der Spielleitung (Kampftabelle C145 bis C149)
        /// </param>
        /// <param name="Gebannt">wieviele seiner Truppen gebannt sind - sie kämpfen nicht mit</param>
        public record class Kämpfer(TruppenSpielfigur Heer, double Gutpunkte = 0, int Gebannt = 0) {
            public double Heeresstärke => KampfRules.BerechneHeeresstärke(Heer, Gebannt);
            public double Kampfstärke => KampfRules.BerechneKampfstärke(Heeresstärke, Gutpunkte);
        }

        /// <summary>
        /// Eine Seite: die Heere, die zusammen kämpfen.
        ///
        /// "Die Kampfstärken aller eigenen und Verbündeten Einheiten werden auf diesem Feld
        /// addiert. Diese aufaddierte Gesamtkampfstärke entscheidet über den Ausgang der
        /// Schlacht." (Regelwerk 5.5)
        /// </summary>
        public record class Seite(IReadOnlyList<Kämpfer> Heere) {
            public static readonly Seite Leer = new([]);

            public double Heeresstärke => Heere.Sum(heer => heer.Heeresstärke);
            public double Kampfstärke => Heere.Sum(heer => heer.Kampfstärke);

            /// <summary>
            /// Der Gutpunktschnitt der Seite: "GP(schnitt) = (KS(gesamt) / HS(gesamt) - 1) * 100"
            /// (Regelwerk 5.5, Kampftabelle C191)
            /// </summary>
            public double Gutpunktschnitt => KampfRules.BerechneGutpunktschnitt(Kampfstärke, Heeresstärke);
        }

        /// <summary>
        /// Wer die Schlacht gewonnen hat.
        /// </summary>
        public enum Ausgang {
            /// <summary>Der Angreifer hat die höhere Kampfstärke</summary>
            Angreifer,
            /// <summary>Der Verteidiger hat die höhere Kampfstärke</summary>
            Verteidiger,
            /// <summary>Beide Seiten sind gleich stark</summary>
            Unentschieden,
        }

        /// <summary>
        /// Was aus einem einzelnen Heer geworden ist.
        /// </summary>
        public record class Heeresausgang(Kämpfer Kämpfer, KampfRules.Verluste Verluste) {
            public bool Aufgerieben => Verluste.Aufgerieben;
            public override string ToString() => $"{Kämpfer.Heer.Bezeichner}: {Verluste}";
        }

        /// <summary>
        /// Das Ergebnis einer Schlacht.
        /// </summary>
        /// <param name="Sieger">wer gewonnen hat</param>
        /// <param name="Überrannt">
        /// wurde überrannt statt gekämpft? Dann war die unterlegene Seite wehrlos und der Sieger
        /// hat keine Verluste.
        /// </param>
        /// <param name="Angreifer">je angreifendem Heer sein Ausgang</param>
        /// <param name="Verteidiger">je verteidigendem Heer sein Ausgang</param>
        /// <param name="Katapultbeute">die Katapulte, die der Sieger übernimmt</param>
        public record class Schlachtergebnis(Ausgang Sieger, bool Überrannt,
                IReadOnlyList<Heeresausgang> Angreifer, IReadOnlyList<Heeresausgang> Verteidiger,
                (int Leichte, int Schwere) Katapultbeute) {

            /// <summary>Die unterlegenen Heere, oder eine leere Liste bei einem Unentschieden</summary>
            public IReadOnlyList<Heeresausgang> Verlierer => Sieger switch {
                Ausgang.Angreifer => Verteidiger,
                Ausgang.Verteidiger => Angreifer,
                _ => [],
            };

            public string Beschreibung {
                get {
                    string ausgang = Sieger switch {
                        Ausgang.Angreifer => "Der Angreifer gewinnt",
                        Ausgang.Verteidiger => "Der Verteidiger gewinnt",
                        _ => "Unentschieden",
                    };
                    return Überrannt ? $"{ausgang} - überrannt" : ausgang;
                }
            }
        }

        /// <summary>
        /// Wertet eine Schlacht aus.
        ///
        /// Die Heere gehen so hinein, wie sie nach dem Beschuss dastehen - die Kampftabelle nennt
        /// diese Zwischenwerte "Werte nach Katabeschuss" und rechnet den Nahkampf mit ihnen.
        /// Verändert wird hier nichts: das Ergebnis sagt, was jedes Heer verliert.
        /// </summary>
        public static Schlachtergebnis WerteNahkampfAus(Seite? angreifer, Seite? verteidiger) {
            var anSeite = angreifer ?? Seite.Leer;
            var verSeite = verteidiger ?? Seite.Leer;

            double hsAngreifer = anSeite.Heeresstärke;
            double hsVerteidiger = verSeite.Heeresstärke;

            // 1. Überrennen: "Ab einer zehnfachen Überlegenheit ( ohne Gutpunkte gerechnet!)
            //    gelten unterlegene Verteidiger als wehrlos (sie verteidigen sich nicht) und
            //    können ohne Verluste gefangen genommen oder vernichtet werden." (Regelwerk 5.5)
            //    Die Kampftabelle prüft das in beide Richtungen (C171 und N171).
            if (KampfRules.IstÜbermacht(hsAngreifer, hsVerteidiger))
                return Überrenne(Ausgang.Angreifer, anSeite, verSeite);
            if (KampfRules.IstÜbermacht(hsVerteidiger, hsAngreifer))
                return Überrenne(Ausgang.Verteidiger, anSeite, verSeite);

            // 2. Die Kampfstärke entscheidet (Kampftabelle B180)
            double ksAngreifer = anSeite.Kampfstärke;
            double ksVerteidiger = verSeite.Kampfstärke;
            Ausgang sieger = ksAngreifer > ksVerteidiger ? Ausgang.Angreifer
                           : ksVerteidiger > ksAngreifer ? Ausgang.Verteidiger
                           : Ausgang.Unentschieden;

            // 3. und 4. die Verluste beider Seiten
            var verlusteAngreifer = BerechneSeitenverluste(anSeite, verSeite, sieger == Ausgang.Angreifer, sieger);
            var verlusteVerteidiger = BerechneSeitenverluste(verSeite, anSeite, sieger == Ausgang.Verteidiger, sieger);

            var ergebnis = new Schlachtergebnis(sieger, false, verlusteAngreifer, verlusteVerteidiger, (0, 0));
            return ergebnis with { Katapultbeute = SammleKatapultbeute(ergebnis.Verlierer) };
        }

        /// <summary>
        /// Das Überrennen: die wehrlose Seite verliert alles, die überlegene nichts.
        ///
        /// Was überrannt wird, wird gefangen genommen, nicht zerrieben: "gefangene tote Rüstgüter
        /// dagegen können in das eigene Heereskontingent übernommen ... werden" (Regelwerk 5.5).
        /// Die Katapulte gehen deshalb vollständig an den Sieger, während ein im Nahkampf
        /// aufgeriebenes Heer die Hälfte von ihnen an die Zerstörung verliert.
        ///
        /// "Das Überrennen kostet 7 zusätzliche Bewegungspunkte" - das betrifft die Bewegung und
        /// steht deshalb nicht hier.
        /// </summary>
        private static Schlachtergebnis Überrenne(Ausgang sieger, Seite angreifer, Seite verteidiger) {
            var wehrlos = sieger == Ausgang.Angreifer ? verteidiger : angreifer;
            var überlegen = sieger == Ausgang.Angreifer ? angreifer : verteidiger;

            var verlusteDerWehrlosen = wehrlos.Heere
                .Select(kämpfer => new Heeresausgang(kämpfer,
                    KampfRules.BerechneNahkampfverluste(kämpfer.Heer, kämpfer.Heeresstärke, kämpfer.Gebannt)))
                .ToList();
            var ohneVerluste = überlegen.Heere
                .Select(kämpfer => new Heeresausgang(kämpfer, new KampfRules.Verluste()))
                .ToList();

            var ergebnis = sieger == Ausgang.Angreifer
                ? new Schlachtergebnis(sieger, true, ohneVerluste, verlusteDerWehrlosen, (0, 0))
                : new Schlachtergebnis(sieger, true, verlusteDerWehrlosen, ohneVerluste, (0, 0));
            var gefangen = BeuteRules.BerechneKatapultbeute(wehrlos.Heere.Select(k => k.Heer), aufgerieben: false);
            return ergebnis with { Katapultbeute = gefangen };
        }

        /// <summary>
        /// Die Verluste einer Seite.
        ///
        /// Basis ist immer die Heeresstärke des Gegners (Kampftabelle C188). Der Grössenfaktor
        /// gilt nur für den Sieger: die Kampftabelle setzt ihn beim Verlierer auf null (C187).
        /// Gemessen wird jedes Heer an der Gutpunktdifferenz zum Gutpunktschnitt der Gegenseite
        /// (C191 bis C194).
        ///
        /// Bei einem Unentschieden gibt es keinen Sieger, der einen Grössenvorteil hätte - beide
        /// Seiten rechnen dann wie ein Verlierer. Das Regelwerk sagt zu diesem Fall nichts; so
        /// bleiben wenigstens beide Seiten gleich behandelt.
        /// </summary>
        private static List<Heeresausgang> BerechneSeitenverluste(Seite seite, Seite gegner, bool istSieger, Ausgang sieger) {
            double basis = gegner.Heeresstärke;
            double gesamtverluste = istSieger
                ? KampfRules.BerechneGesamtverluste(gegner.Heeresstärke, seite.Heeresstärke)
                : Math.Max(0, Math.Truncate(basis));

            var heere = seite.Heere.Select(kämpfer => (kämpfer.Heeresstärke, kämpfer.Gutpunkte)).ToList();
            double[] jeHeer = KampfRules.VerteileVerluste(gesamtverluste, heere, gegner.Gutpunktschnitt);

            var ergebnis = new List<Heeresausgang>(seite.Heere.Count);
            for (int i = 0; i < seite.Heere.Count; i++) {
                var kämpfer = seite.Heere[i];
                ergebnis.Add(new Heeresausgang(kämpfer,
                    KampfRules.BerechneNahkampfverluste(kämpfer.Heer, jeHeer[i], kämpfer.Gebannt)));
            }
            return ergebnis;
        }

        /// <summary>
        /// Die Katapulte der unterlegenen Seite, die der Sieger übernimmt.
        ///
        /// Gezählt wird, was in den Nahkampf gegangen ist, nicht was danach übrig ist: Katapulte
        /// nehmen am Nahkampf nicht teil, sie ergeben sich (Regelwerk 5.5). Die Kampftabelle
        /// verteilt die Verluste allerdings auch auf die Katapultzeilen - siehe die Nebenbefunde
        /// in Offene-Regelfragen.md. Ein aufgeriebenes Heer verliert die Hälfte seiner Katapulte
        /// an die Zerstörung.
        /// </summary>
        private static (int Leichte, int Schwere) SammleKatapultbeute(IReadOnlyList<Heeresausgang> verlierer) {
            int leichte = 0, schwere = 0;
            foreach (var ausgang in verlierer) {
                var beute = BeuteRules.BerechneKatapultbeute([ausgang.Kämpfer.Heer], ausgang.Aufgerieben);
                leichte += beute.Leichte;
                schwere += beute.Schwere;
            }
            return (leichte, schwere);
        }
    }
}
