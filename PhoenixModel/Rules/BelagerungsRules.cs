using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Belagerung (Regelwerk 1.7).
    ///
    /// "Eine Belagerung dauert einen ganzen Monat, so dass man im vorhergehenden Monat schon an den
    /// zu belagernden Rüstort oder die zu belagernde Großbaustelle herangezogen sein muss! Die
    /// Belagerung wird bei elektronischer Auswertung automatisch eingeleitet, wenn ein feindliches
    /// Heer oder Flotte auf einer angrenzenden Gemark steht!"
    ///
    /// Sie wird also nicht befohlen, sondern ergibt sich aus der Lage der Heere - deshalb gibt es
    /// hier keinen Befehl, sondern eine Suche.
    ///
    /// Was sie bewirkt, steht in 1.7.1 und 1.7.2 und ist beide Male dasselbe: je Gemark, von der
    /// aus belagert wird, 15 Prozent weniger. Beim Rüstort trifft es die Rüstkapazität, bei einer
    /// Großbaustelle - dem Neuaufbau oder der Reparatur eines Rüstorts - die Baupunkte, die in
    /// einem Monat dazukommen dürfen.
    ///
    /// Zwei Wege führen zu den Belagerern, weil zwei Seiten fragen:
    ///
    /// * Die Spielleitung sieht alle Reiche und kann auch prüfen, ob ein Heer schon im
    ///   vorhergehenden Monat dort stand (<see cref="FindeBelagerung"/>).
    /// * Ein Spieler sieht nur seine Feindaufklärung. Sie sagt, wer wo steht, aber nicht, seit
    ///   wann (<see cref="FindeBelagerungNachAufklärung"/>). Die Belagerung, die er sich damit
    ///   ausrechnet, kann deshalb zu gross oder zu klein sein - genau wie seine Aufklärung.
    /// </summary>
    public static class BelagerungsRules {

        /// <summary>
        /// "die maximal mögliche Rüstung im belagerten Rüstort verringert sich pro Gemark von der
        /// aus belagert wird um 15%" (Regelwerk 1.7.1)
        /// </summary>
        public const double MinderungProGemark = 0.15;

        /// <summary>
        /// Ein Heer, das von einer Nachbargemark aus belagert.
        /// </summary>
        public record class Belagerer(KleinfeldPosition Gemark, Nation Reich, string Bezeichnung) {
            public override string ToString() => $"{Bezeichnung} auf {Gemark.CreateBezeichner()}";
        }

        /// <summary>
        /// Was einen Rüstort oder eine Großbaustelle einschnürt.
        /// </summary>
        public record class Belagerung(KleinfeldPosition Ort, IReadOnlyList<Belagerer> Belagerer) {
            public static readonly Belagerung Keine = new(new KleinfeldPosition(), []);

            /// <summary>
            /// Gezählt werden Gemarken, nicht Heere: "pro Gemark von der aus belagert wird".
            /// Zwei Heere auf demselben Feld schnüren nicht doppelt.
            /// </summary>
            public int AnzahlGemarken => Belagerer.Select(b => b.Gemark.Key).Distinct().Count();

            public bool Besteht => AnzahlGemarken > 0;

            /// <summary>Der Anteil, der wegfällt - höchstens alles</summary>
            public double Minderung => Math.Clamp(AnzahlGemarken * MinderungProGemark, 0, 1);

            public string Beschreibung => Besteht == false
                ? $"{Ort.CreateBezeichner()} ist nicht belagert"
                : $"{Ort.CreateBezeichner()} wird von {AnzahlGemarken} Gemark(en) aus belagert "
                  + $"({Minderung:P0} weniger): {string.Join(", ", Belagerer)}";
        }

        /// <summary>
        /// Lässt sich dieser Rüstort überhaupt belagern?
        ///
        /// Das Regelwerk nennt in 1.7.1 Burgen, Städte und Hauptstädte; Festung und
        /// Festungshauptstadt bleiben aussen vor ("jedoch kann sie nicht belagert werden", 1.5.9).
        /// Die Rüstortreferenz führt dieselbe Auskunft in der Spalte canSieged - sie ist die
        /// Quelle, damit eine Änderung an der Tabelle auch hier ankommt.
        ///
        /// Dörfer stehen in der Tabelle ebenfalls als belagerbar. Das ändert an der Rüstung
        /// nichts, weil ein Dorf keine Kapazität hat, wohl aber an einer Großbaustelle auf diesem
        /// Feld - und das ist genau, was 1.7.2 meint.
        /// </summary>
        public static bool IstBelagerbar(KleinFeld? gemark) {
            if (gemark == null)
                return false;
            return BauwerkeView.GetRüstortNachKarte(gemark)?.canSieged == true;
        }

        /// <summary>
        /// Sucht die Belagerer eines Rüstorts aus den Figuren aller Reiche (Weg der Spielleitung).
        ///
        /// Gezählt wird ein Heer, wenn alles davon zutrifft:
        ///
        /// * Es steht auf einer direkt angrenzenden Gemark.
        /// * Sein Reich ist mit dem Eigentümer des Rüstorts verfeindet.
        /// * Es stand dort schon zu Monatsbeginn - "so dass man im vorhergehenden Monat schon ...
        ///   herangezogen sein muss". Dafür zählt die Ausgangsposition, nicht die Endposition.
        /// * Der Rüstort liesse sich von dort aus betreten - "aus denen heraus sie auch betreten
        ///   werden können" (1.7.1). Geprüft wird das Gelände: wo eine Figur nicht hintreten kann,
        ///   belagert sie auch nicht.
        /// </summary>
        /// <param name="rüstort">die Gemark mit dem Rüstort</param>
        /// <param name="figuren">die Figuren aller Reiche; ohne Angabe die der Spielleitung</param>
        public static Belagerung FindeBelagerung(KleinFeld? rüstort, IEnumerable<Spielfigur>? figuren = null) {
            if (IstBelagerbar(rüstort) == false || rüstort!.Nation == null)
                return Belagerung.Keine;

            var nachbarn = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)?.ToList() ?? [];
            if (nachbarn.Count == 0)
                return Belagerung.Keine;

            List<Belagerer> belagerer = [];
            foreach (var figur in figuren ?? Spielleitungsdaten.GetAlleFiguren()) {
                if (figur is not TruppenSpielfigur truppe || truppe.Nation == null)
                    continue;
                // Die Ausgangsposition zählt: wer erst in diesem Monat herangezogen ist, belagert
                // noch nicht.
                var stand = new KleinfeldPosition(truppe.gf_von, truppe.kf_von);
                var nachbar = nachbarn.FirstOrDefault(feld => feld.Key == stand.Key);
                if (nachbar == null)
                    continue;
                if (DiplomatieRules.SindVerfeindet(truppe.Nation, rüstort.Nation, rüstort) == false)
                    continue;
                if (KannDenRüstortBetreten(truppe, rüstort) == false)
                    continue;

                belagerer.Add(new Belagerer(stand, truppe.Nation, truppe.Bezeichner));
            }
            return belagerer.Count == 0
                ? Belagerung.Keine
                : new Belagerung(new KleinfeldPosition(rüstort.gf, rüstort.kf), belagerer);
        }

        /// <summary>
        /// Sucht die Belagerer aus der eigenen Feindaufklärung (Weg des Spielers).
        ///
        /// Die Aufklärung sagt, wer wo steht - nicht, seit wann. Die Bedingung aus 1.7, dass das
        /// Heer schon im vorhergehenden Monat herangezogen sein muss, lässt sich damit nicht
        /// prüfen; hier zählt, was gesehen wurde. Und sie ist so gut wie ihr Alter: ist sie
        /// veraltet, ist es diese Rechnung auch. Deshalb nennt <see cref="Belagerung.Beschreibung"/>
        /// die Felder, aus denen sie stammt.
        ///
        /// Flotten bleiben aussen vor: einen Rüstort an Land können sie nicht betreten.
        /// </summary>
        public static Belagerung FindeBelagerungNachAufklärung(KleinFeld? rüstort) {
            if (IstBelagerbar(rüstort) == false || rüstort!.Nation == null)
                return Belagerung.Keine;

            var nachbarn = KleinfeldView.GetNachbarn(rüstort, 1, includeSelf: false)?.ToList() ?? [];
            List<Belagerer> belagerer = [];
            foreach (var nachbar in nachbarn) {
                foreach (var feind in nachbar.Fremd) {
                    if (feind.Nation == null || feind.Nummer == Feinde.Zeitstempeleintrag)
                        continue;
                    if (IstHeerOderFlotte(feind.Typ) == false)
                        continue;
                    if (rüstort.IsWasser == false && feind.Typ == FigurType.Schiff)
                        continue;
                    if (DiplomatieRules.SindVerfeindet(feind.Nation, rüstort.Nation, rüstort) == false)
                        continue;

                    belagerer.Add(new Belagerer(new KleinfeldPosition(nachbar.gf, nachbar.kf),
                        feind.Nation, $"{feind.Nation.Reich} {feind.Typ} {feind.Nummer}"));
                }
            }
            return belagerer.Count == 0
                ? Belagerung.Keine
                : new Belagerung(new KleinfeldPosition(rüstort.gf, rüstort.kf), belagerer);
        }

        /// <summary>
        /// Mindert einen Wert um die Belagerung und rundet ab - angefangene Rüstgüter und
        /// angefangene Baupunkte gibt es nicht.
        /// </summary>
        public static int Mindere(int wert, Belagerung? belagerung) {
            if (wert <= 0 || belagerung == null || belagerung.Besteht == false)
                return Math.Max(0, wert);
            return (int)Math.Floor(wert * (1 - belagerung.Minderung));
        }

        /// <summary>
        /// Nur Heere und Flotten belagern - ein Charakter oder ein Zauberer allein schnürt nichts
        /// ein.
        /// </summary>
        private static bool IstHeerOderFlotte(FigurType typ) => typ switch {
            FigurType.Charakter or FigurType.CharakterZauberer or FigurType.Zauberer or FigurType.None => false,
            _ => true,
        };

        /// <summary>
        /// "aus denen heraus sie auch betreten werden können" - geprüft am Gelände des Rüstorts.
        /// </summary>
        private static bool KannDenRüstortBetreten(Spielfigur figur, KleinFeld rüstort) {
            var verbrauch = BewegungsRules.GetVerbrauch(figur, rüstort.Gelaendetyp ?? 0, wegerecht: true, straße: false);
            return verbrauch != null && verbrauch.BP < BewegungsRules.BPUnpassierbar;
        }
    }
}
