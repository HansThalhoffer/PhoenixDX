using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Überbesetzung einer Gemark (Regelwerk 5.7).
    ///
    /// "Befinden sich mehr als 100.000 Raumpunkte am Ende des Spielzuges eines Reiches in der
    /// selben Gemark, so kommt es zum Nahkampf, wenn sich Heere von zwei oder mehr Reichen in der
    /// Gemark befinden, ansonsten wird die Überzahl prozentual anteilig gestrichen."
    ///
    /// Zwei Ausgänge also, und welcher es wird, entscheidet allein die Zahl der Reiche auf dem
    /// Feld - nicht, ob sie verfeindet sind. Auf einer überfüllten Gemark kämpfen auch Verbündete
    /// um den Platz. Das passt zu 5.5.1, wo zwischen Bündnispartnern auf einer Gemark ein Krieg
    /// ausbrechen kann.
    ///
    /// Gerechnet wird mit den Raumpunkten, die sich aus den Figuren ergeben
    /// (<see cref="SpielfigurRules.BerechneRaumpunkte(Spielfigur)"/>), nicht mit der Spalte rp:
    /// die ist ein abgeleiteter Wert und kann veraltet sein, wenn ein Heer geteilt oder
    /// zusammengelegt wurde.
    /// </summary>
    public static class ÜberbesetzungRules {

        /// <summary>
        /// "mehr als 100.000 Raumpunkte ... in der selben Gemark" - genau 100.000 sind noch in
        /// Ordnung (Regelwerk 5.7).
        /// </summary>
        public const int Raumpunktgrenze = 100_000;

        /// <summary>
        /// Was aus einer überfüllten Gemark folgt.
        /// </summary>
        public enum Folge {
            /// <summary>Die Gemark ist nicht überbesetzt</summary>
            Keine,
            /// <summary>Heere mehrerer Reiche stehen dort - sie kämpfen um den Platz</summary>
            Nahkampf,
            /// <summary>Ein Reich allein - die Überzahl wird anteilig gestrichen</summary>
            Streichung,
        }

        /// <summary>
        /// Was einer Figur gestrichen wird.
        /// </summary>
        /// <param name="Figur">die betroffene Figur</param>
        /// <param name="Raumpunkte">ihre Raumpunkte</param>
        /// <param name="GestricheneRaumpunkte">wieviele davon wegfallen</param>
        /// <param name="Gestrichen">welche Rüstgüter das sind</param>
        public record class Streichung(Spielfigur Figur, int Raumpunkte, int GestricheneRaumpunkte,
                KampfRules.Verluste Gestrichen) {
            public override string ToString()
                => $"{Figur.Bezeichner}: {GestricheneRaumpunkte:n0} von {Raumpunkte:n0} RP - {Gestrichen}";
        }

        /// <summary>
        /// Eine überbesetzte Gemark.
        /// </summary>
        public record class Überbesetzung(KleinfeldPosition Gemark, int Raumpunkte, Folge Folge,
                IReadOnlyList<Streichung> Streichungen) {
            public int Überzahl => Math.Max(0, Raumpunkte - Raumpunktgrenze);

            public string Beschreibung => Folge == Folge.Nahkampf
                ? $"{Gemark.CreateBezeichner()}: {Raumpunkte:n0} RP - Nahkampf um den Platz"
                : $"{Gemark.CreateBezeichner()}: {Raumpunkte:n0} RP - {Überzahl:n0} RP werden gestrichen";
        }

        /// <summary>
        /// Die Raumpunkte, die diese Figuren belegen.
        /// </summary>
        public static int BerechneRaumpunkte(IEnumerable<Spielfigur>? figuren) {
            if (figuren == null)
                return 0;
            int summe = 0;
            foreach (var figur in figuren)
                summe += Math.Max(0, SpielfigurRules.BerechneRaumpunkte(figur));
            return summe;
        }

        /// <summary>
        /// Ist die Gemark überbesetzt? "mehr als 100.000 Raumpunkte" - die Grenze selbst ist noch
        /// erlaubt.
        /// </summary>
        public static bool IstÜberbesetzt(int raumpunkte) => raumpunkte > Raumpunktgrenze;

        /// <summary>
        /// Sucht die überbesetzten Gemarken unter diesen Figuren.
        /// </summary>
        public static List<Überbesetzung> FindeÜberbesetzungen(IEnumerable<Spielfigur>? figuren) {
            List<Überbesetzung> ergebnis = [];
            if (figuren == null)
                return ergebnis;

            Dictionary<int, List<Spielfigur>> nachGemark = [];
            foreach (var figur in figuren) {
                if (figur == null || Plausibilität.IsValid(figur) == false)
                    continue;
                if (nachGemark.TryGetValue(figur.Key, out var besatzung) == false)
                    nachGemark[figur.Key] = besatzung = [];
                besatzung.Add(figur);
            }

            foreach (var besatzung in nachGemark.Values) {
                var überbesetzung = Prüfe(besatzung);
                if (überbesetzung != null)
                    ergebnis.Add(überbesetzung);
            }
            return [.. ergebnis.OrderBy(ü => ü.Gemark.gf).ThenBy(ü => ü.Gemark.kf)];
        }

        /// <summary>
        /// Prüft eine einzelne Gemark.
        /// </summary>
        /// <param name="besatzung">alle Figuren, die dort stehen</param>
        /// <returns>die Überbesetzung, oder null wenn der Platz reicht</returns>
        public static Überbesetzung? Prüfe(IReadOnlyList<Spielfigur>? besatzung) {
            if (besatzung == null || besatzung.Count == 0)
                return null;

            int raumpunkte = BerechneRaumpunkte(besatzung);
            if (IstÜberbesetzt(raumpunkte) == false)
                return null;

            var gemark = new KleinfeldPosition(besatzung[0].gf, besatzung[0].kf);

            int reiche = besatzung
                .Where(figur => figur.Nation != null)
                .Select(figur => figur.Nation!.Reich)
                .Distinct()
                .Count();
            if (reiche > 1)
                return new Überbesetzung(gemark, raumpunkte, Folge.Nahkampf, []);

            return new Überbesetzung(gemark, raumpunkte, Folge.Streichung,
                BerechneStreichungen(besatzung, raumpunkte - Raumpunktgrenze));
        }

        /// <summary>
        /// Verteilt die Überzahl auf die Heere: "ansonsten wird die Überzahl prozentual anteilig
        /// gestrichen" (Regelwerk 5.7).
        ///
        /// Jedes Heer gibt denselben Anteil ab, gemessen an seinen Raumpunkten, und innerhalb
        /// eines Heeres trifft es jede Gattung gleichermassen.
        ///
        /// Charaktere und Zauberer belegen zwar Platz - ihre Raumpunkte zählen zur Grenze - aber
        /// gestrichen werden sie nicht: einen halben Charakter gibt es nicht. Die Überzahl tragen
        /// deshalb die Truppen.
        /// </summary>
        /// <param name="besatzung">alle Figuren auf der Gemark</param>
        /// <param name="überzahl">die Raumpunkte, die zuviel sind</param>
        public static List<Streichung> BerechneStreichungen(IEnumerable<Spielfigur>? besatzung, int überzahl) {
            List<Streichung> ergebnis = [];
            if (besatzung == null || überzahl <= 0)
                return ergebnis;

            var truppen = besatzung.OfType<TruppenSpielfigur>()
                .Select(truppe => (Truppe: truppe, Raumpunkte: Math.Max(0, SpielfigurRules.BerechneRaumpunkte(truppe))))
                .Where(eintrag => eintrag.Raumpunkte > 0)
                .ToList();

            int gesamt = truppen.Sum(eintrag => eintrag.Raumpunkte);
            if (gesamt <= 0)
                return ergebnis;

            foreach (var (truppe, raumpunkte) in truppen) {
                double anteil = Math.Min(1, (double)überzahl / gesamt);
                int gestrichen = (int)Math.Round(raumpunkte * anteil, MidpointRounding.AwayFromZero);
                var verluste = KampfRules.BerechneNahkampfverluste(truppe,
                    KampfRules.BerechneHeeresstärke(truppe) * anteil);
                ergebnis.Add(new Streichung(truppe, raumpunkte, gestrichen, verluste));
            }
            return ergebnis;
        }
    }
}
