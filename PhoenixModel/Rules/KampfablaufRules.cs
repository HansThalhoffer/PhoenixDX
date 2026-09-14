using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Klammer um den Kampf: ein Durchlauf über eine Gemark, Schritt für Schritt
    /// (Regelwerk Kapitel 5).
    ///
    /// "Dann wird der Fernkampf, anschließend der ( ggf. Ritterkampf, dann ) Charakterkampf und
    /// dann der Nahkampf ausgewürfelt und berechnet." (Regelwerk 5.5)
    ///
    /// Hier laufen die Teile zusammen, die einzeln schon da sind:
    ///
    /// 1. Der Beschuss beider Seiten (<see cref="FernkampfRules"/>). Er wirkt vor dem Nahkampf und
    ///    nimmt Truppen weg - die Kampftabelle nennt das Ergebnis "Werte nach Katabeschuss".
    /// 2. Der Nahkampf mit dem, was übrig ist (<see cref="NahkampfRules"/>). Die Gutpunkte der
    ///    Heerführer werden dabei neu gezählt: wer unter Beschuss gefallen ist, bringt keine mehr
    ///    ein (Kampftabelle C146 liest die Zeile nach dem Beschuss).
    /// 3. Beute und Kampfeinnahmen des Siegers (<see cref="BeuteRules"/>,
    ///    <see cref="KampfeinnahmenRules"/>).
    ///
    /// Ritterkampf, Charakterkampf und Zauberduell stehen dazwischen, brauchen aber Würfel und
    /// Entscheidungen der Beteiligten (wer schlägt auf wen, wer lehnt ab). Sie laufen deshalb
    /// nicht mit, sondern gehen als Gutpunkte in die Seiten ein - so wie der W20.
    ///
    /// Gerechnet wird auf Kopien: die Figuren des Spielers bleiben unberührt, bis jemand
    /// <see cref="Übernimm"/> aufruft. Ein Bericht lässt sich also ansehen, bevor er wahr wird.
    /// </summary>
    public static class KampfablaufRules {

        /// <summary>
        /// Ein Heer, wie es in die Schlacht geht.
        /// </summary>
        /// <param name="Heer">das Heer - es wird nicht verändert</param>
        /// <param name="GutpunkteOhneHeerführer">
        /// alles, was an Gutpunkten feststeht: Geländevorteile, Charaktere, Zauberer und der W20
        /// (Kampftabelle C145, C147, C148, C144). Die Gutpunkte der Heerführer kommen erst nach
        /// dem Beschuss dazu, weil dann feststeht, wieviele noch leben.
        /// </param>
        /// <param name="Gebannt">wieviele seiner Truppen gebannt sind</param>
        public record class Kämpfender(TruppenSpielfigur Heer, double GutpunkteOhneHeerführer = 0, int Gebannt = 0);

        /// <summary>
        /// Eine Seite der Schlacht.
        /// </summary>
        /// <param name="Heere">die Heere dieser Seite</param>
        /// <param name="Vorteile">
        /// was die Gemark selbst schützt - Rüstort, Wall, Fluss. Daraus ergibt sich der Schutz
        /// gegen Beschuss und die Frage, ob ein Rüstort verteidigt wird.
        /// </param>
        public record class Seite(IReadOnlyList<Kämpfender> Heere, IReadOnlyList<Kampfvorteil>? Vorteile = null) {
            public static readonly Seite Leer = new([]);
            public IReadOnlyList<Kampfvorteil> Feldvorteile => Vorteile ?? [];
        }

        /// <summary>
        /// Was die Spielleitung würfelt und hereingibt.
        /// </summary>
        /// <param name="TrefferpunkteGegenVerteidiger">was der Angreifer erschossen hat</param>
        /// <param name="TrefferpunkteGegenAngreifer">was der Verteidiger erschossen hat</param>
        public record class Würfe(double TrefferpunkteGegenVerteidiger = 0, double TrefferpunkteGegenAngreifer = 0) {
            public static readonly Würfe Keine = new();
        }

        /// <summary>
        /// Der Bericht über eine Schlacht.
        /// </summary>
        /// <param name="Gemark">wo</param>
        /// <param name="BeschussGegenAngreifer">was der Beschuss beim Angreifer angerichtet hat</param>
        /// <param name="BeschussGegenVerteidiger">und was beim Verteidiger</param>
        /// <param name="Nahkampf">der Ausgang des Nahkampfes</param>
        /// <param name="Rüstortbeute">was die Eroberung des Rüstorts einbringt</param>
        /// <param name="Ladung">die Ladung der vernichteten Heere</param>
        /// <param name="Kampfeinnahmen">die Kampfeinnahmen des Siegers</param>
        public record class Schlachtbericht(KleinfeldPosition Gemark,
                FernkampfRules.Beschussergebnis BeschussGegenAngreifer,
                FernkampfRules.Beschussergebnis BeschussGegenVerteidiger,
                NahkampfRules.Schlachtergebnis Nahkampf,
                int Rüstortbeute, BeuteRules.Ladung Ladung, int Kampfeinnahmen) {

            /// <summary>Hat der Angreifer die Gemark genommen?</summary>
            public bool GemarkErobert => Nahkampf.Sieger == NahkampfRules.Ausgang.Angreifer;

            /// <summary>
            /// Der Bericht als Text - eine Zeile je Schritt, für den Infotab und die Zwischenablage.
            /// </summary>
            public string AlsText() {
                var zeilen = new List<string> { $"Schlacht auf {Gemark.CreateBezeichner()}" };

                if (BeschussGegenVerteidiger.Trefferpunkte > 0)
                    zeilen.Add($"  Beschuss gegen den Verteidiger: {BeschussGegenVerteidiger.Trefferpunkte:n0} Trefferpunkte, "
                             + $"davon {BeschussGegenVerteidiger.AufRüstort:n0} auf den Rüstort");
                if (BeschussGegenAngreifer.Trefferpunkte > 0)
                    zeilen.Add($"  Beschuss gegen den Angreifer: {BeschussGegenAngreifer.Trefferpunkte:n0} Trefferpunkte");

                zeilen.Add($"  {Nahkampf.Beschreibung}");
                foreach (var ausgang in Nahkampf.Angreifer.Concat(Nahkampf.Verteidiger)) {
                    if (ausgang.Verluste.Heeresstärke > 0)
                        zeilen.Add($"    {ausgang}");
                }

                if (Rüstortbeute > 0)
                    zeilen.Add($"  Rüstortbeute: {Rüstortbeute:n0} GS als besondere Einnahme");
                if (Ladung.Gesamt > 0)
                    zeilen.Add($"  Ladung der vernichteten Heere: {Ladung.Gold:n0} GS und {Ladung.Kampfeinnahmen:n0} GS besondere Einnahmen");
                if (Nahkampf.Katapultbeute.Leichte > 0 || Nahkampf.Katapultbeute.Schwere > 0)
                    zeilen.Add($"  Erbeutete Katapulte: {Nahkampf.Katapultbeute.Leichte} leichte, {Nahkampf.Katapultbeute.Schwere} schwere");
                if (Kampfeinnahmen > 0)
                    zeilen.Add($"  Kampfeinnahmen: {Kampfeinnahmen:n0} GS");

                return string.Join(Environment.NewLine, zeilen);
            }
        }

        /// <summary>
        /// Wertet eine Schlacht auf einer Gemark aus.
        ///
        /// Verändert nichts: gerechnet wird auf Kopien der Heere, und der Bericht nennt zu jedem
        /// Heer die Figur des Spielers. Erst <see cref="Übernimm"/> schreibt die Verluste fort.
        /// </summary>
        /// <param name="gemark">die umkämpfte Gemark - sie sagt, was es dort zu erobern gibt</param>
        /// <param name="angreifer">die angreifende Seite</param>
        /// <param name="verteidiger">die verteidigende Seite</param>
        /// <param name="würfe">was die Spielleitung erwürfelt hat</param>
        public static Schlachtbericht WerteSchlachtAus(KleinFeld? gemark, Seite? angreifer, Seite? verteidiger, Würfe? würfe) {
            var anSeite = angreifer ?? Seite.Leer;
            var verSeite = verteidiger ?? Seite.Leer;
            var wurf = würfe ?? Würfe.Keine;

            // 1. Kopien anlegen - die Figuren des Spielers bleiben, wie sie sind
            var anKopien = LegeKopienAn(anSeite);
            var verKopien = LegeKopienAn(verSeite);

            // 2. Der Beschuss, beide Richtungen gleichzeitig und vor dem Nahkampf
            var gegenVerteidiger = FernkampfRules.WerteBeschussAus(wurf.TrefferpunkteGegenVerteidiger,
                [.. verKopien.Select(k => k.AlsBeschossener())], verSeite.Feldvorteile);
            var gegenAngreifer = FernkampfRules.WerteBeschussAus(wurf.TrefferpunkteGegenAngreifer,
                [.. anKopien.Select(k => k.AlsBeschossener())], anSeite.Feldvorteile);

            ÜbernimmBeschuss(anKopien, gegenAngreifer);
            ÜbernimmBeschuss(verKopien, gegenVerteidiger);

            // 3. Der Nahkampf mit dem, was übrig ist
            var nahkampf = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([.. anKopien.Select(k => k.AlsKämpfer())]),
                new NahkampfRules.Seite([.. verKopien.Select(k => k.AlsKämpfer())]));

            // 4. Beute und Kampfeinnahmen
            var verlierer = nahkampf.Verlierer.Select(ausgang => ausgang.Kämpfer.Heer).ToList();
            var ladung = BeuteRules.BerechneLandbeute(verlierer);
            int rüstortbeute = nahkampf.Sieger == NahkampfRules.Ausgang.Angreifer
                ? BeuteRules.GetRüstortbeute(gemark)
                : 0;
            int kampfeinnahmen = KampfeinnahmenRules.Berechne(nahkampf);

            var position = gemark != null
                ? new KleinfeldPosition(gemark.gf, gemark.kf)
                : Standort(anKopien, verKopien);

            return new Schlachtbericht(position, gegenAngreifer, gegenVerteidiger,
                ErsetzeDurchOriginale(nahkampf, anKopien, verKopien),
                rüstortbeute, ladung, kampfeinnahmen);
        }

        /// <summary>
        /// Schreibt einen Bericht fort: die Verluste werden von den Figuren abgezogen, Beute und
        /// Kampfeinnahmen dem Sieger gutgeschrieben.
        ///
        /// Gespeichert wird hier nichts - das ist Sache der Anwendung.
        /// </summary>
        /// <returns>wieviele Heere verändert wurden</returns>
        public static int Übernimm(Schlachtbericht? bericht) {
            if (bericht == null)
                return 0;

            // Erst der Beschuss, dann der Nahkampf - in derselben Reihenfolge, in der gerechnet
            // wurde. Der Nahkampf hat seine Verluste auf dem Stand nach dem Beschuss ermittelt,
            // also muessen beide abgezogen werden.
            int verändert = 0;
            verändert += ZieheAb(bericht.Nahkampf.Angreifer, bericht.BeschussGegenAngreifer.Verluste);
            verändert += ZieheAb(bericht.Nahkampf.Verteidiger, bericht.BeschussGegenVerteidiger.Verluste);
            foreach (var ausgang in bericht.Nahkampf.Angreifer.Concat(bericht.Nahkampf.Verteidiger)) {
                if (ZieheAb(ausgang.Kämpfer.Heer, ausgang.Verluste))
                    verändert++;
            }

            var sieger = bericht.Nahkampf.Sieger switch {
                NahkampfRules.Ausgang.Angreifer => bericht.Nahkampf.Angreifer,
                NahkampfRules.Ausgang.Verteidiger => bericht.Nahkampf.Verteidiger,
                _ => [],
            };
            // Beute und Kampfeinnahmen gehen an das erste Heer, das noch steht - das Regelwerk
            // lässt offen, wer sie trägt, und ein aufgeriebenes Heer kann nichts mehr tragen.
            var träger = sieger.FirstOrDefault(ausgang => ausgang.Aufgerieben == false)?.Kämpfer.Heer;
            if (träger != null) {
                BeuteRules.SchreibeGut(träger, bericht.Ladung, bericht.Rüstortbeute);
                träger.Kampfeinnahmen += bericht.Kampfeinnahmen;
            }
            return verändert;
        }

        /// <summary>
        /// Ein Heer mit seiner Rechenkopie.
        /// </summary>
        private sealed class Kopie {
            public required Kämpfender Vorbild { get; init; }
            public required TruppenSpielfigur Rechenfigur { get; init; }

            public FernkampfRules.Beschossen AlsBeschossener()
                => new(Rechenfigur, Vorbild.GutpunkteOhneHeerführer, Vorbild.Gebannt);

            /// <summary>
            /// Für den Nahkampf kommen die Gutpunkte der Heerführer dazu - gezählt wird, wer den
            /// Beschuss überstanden hat (Kampftabelle C146).
            /// </summary>
            public NahkampfRules.Kämpfer AlsKämpfer()
                => new(Rechenfigur,
                       Vorbild.GutpunkteOhneHeerführer + KampfRules.BerechneGutpunkteAusHeerführern(Rechenfigur.hf),
                       Vorbild.Gebannt);
        }

        private static List<Kopie> LegeKopienAn(Seite seite) {
            List<Kopie> ergebnis = [];
            foreach (var kämpfender in seite.Heere) {
                var kopie = SpielfigurRules.KopiereFürBerechnung(kämpfender.Heer);
                if (kopie != null)
                    ergebnis.Add(new Kopie { Vorbild = kämpfender, Rechenfigur = kopie });
            }
            return ergebnis;
        }

        private static void ÜbernimmBeschuss(List<Kopie> kopien, FernkampfRules.Beschussergebnis ergebnis) {
            for (int i = 0; i < kopien.Count && i < ergebnis.Verluste.Count; i++)
                ZieheAb(kopien[i].Rechenfigur, ergebnis.Verluste[i]);
        }

        /// <summary>
        /// Zieht die Verluste einer ganzen Seite ab - die Listen stehen in derselben Reihenfolge.
        /// </summary>
        private static int ZieheAb(IReadOnlyList<NahkampfRules.Heeresausgang> ausgänge,
                IReadOnlyList<KampfRules.Verluste> verluste) {
            int verändert = 0;
            for (int i = 0; i < ausgänge.Count && i < verluste.Count; i++) {
                if (ZieheAb(ausgänge[i].Kämpfer.Heer, verluste[i]))
                    verändert++;
            }
            return verändert;
        }

        /// <summary>
        /// Zieht Verluste von einem Heer ab. Unter null geht nichts.
        /// </summary>
        private static bool ZieheAb(TruppenSpielfigur? heer, KampfRules.Verluste? verluste) {
            if (heer == null || verluste == null)
                return false;

            int vorher = heer.staerke + heer.hf + heer.LKP + heer.SKP + heer.Pferde;
            heer.staerke = Math.Max(0, heer.staerke - (verluste.Krieger + verluste.Reiter + verluste.Schiffe));
            heer.hf = Math.Max(0, heer.hf - verluste.Heerführer);
            heer.LKP = Math.Max(0, heer.LKP - (verluste.LKP + verluste.LKS));
            heer.SKP = Math.Max(0, heer.SKP - (verluste.SKP + verluste.SKS));
            heer.Pferde = Math.Max(0, heer.Pferde - verluste.Pferde);
            return vorher != heer.staerke + heer.hf + heer.LKP + heer.SKP + heer.Pferde;
        }

        /// <summary>
        /// Tauscht im Ergebnis die Rechenfiguren gegen die Figuren des Spielers - der Bericht soll
        /// von den Heeren sprechen, die es wirklich gibt.
        ///
        /// Das Beschussergebnis braucht das nicht: seine Verluste stehen für sich, ohne Figur.
        /// </summary>
        private static NahkampfRules.Schlachtergebnis ErsetzeDurchOriginale(
                NahkampfRules.Schlachtergebnis ergebnis, List<Kopie> angreifer, List<Kopie> verteidiger) {
            return ergebnis with {
                Angreifer = [.. Tausche(ergebnis.Angreifer, angreifer)],
                Verteidiger = [.. Tausche(ergebnis.Verteidiger, verteidiger)],
            };
        }

        private static IEnumerable<NahkampfRules.Heeresausgang> Tausche(
                IReadOnlyList<NahkampfRules.Heeresausgang> ausgänge, List<Kopie> kopien) {
            for (int i = 0; i < ausgänge.Count; i++) {
                if (i < kopien.Count)
                    yield return ausgänge[i] with {
                        Kämpfer = ausgänge[i].Kämpfer with { Heer = kopien[i].Vorbild.Heer },
                    };
                else
                    yield return ausgänge[i];
            }
        }

        /// <summary>
        /// Wo die Schlacht stattfindet, wenn keine Gemark übergeben wurde
        /// </summary>
        private static KleinfeldPosition Standort(List<Kopie> angreifer, List<Kopie> verteidiger) {
            var figur = angreifer.Concat(verteidiger).FirstOrDefault()?.Vorbild.Heer;
            return figur == null ? new KleinfeldPosition() : new KleinfeldPosition(figur.gf, figur.kf);
        }
    }
}
