using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Der Unterbau der Kampfauswertung für die Spielleitung.
    ///
    /// Hier steht, was sich prüfen lässt: die Zugdaten aller Reiche holen, die Konflikte suchen,
    /// aus einem Konflikt die beiden Seiten bauen und die Schlacht rechnen lassen. Der Dialog
    /// darüber zeigt es nur an.
    ///
    /// Die Regeln selbst stehen im Modell: <see cref="KonfliktRules"/> findet die Gemarken,
    /// <see cref="KampfablaufRules"/> rechnet den Ablauf, <see cref="KampfRules"/> weiss, was eine
    /// Gemark ihrem Verteidiger an Gutpunkten bringt.
    /// </summary>
    public static class Kampfauswertung {

        /// <summary>
        /// Holt die Zugdaten aller Reiche eines Zuges.
        /// </summary>
        public static ReichsdatenErgebnis Lade(string zugverzeichnis, int zug, string? passwort = null)
            => Reichsdaten.LadeAlleReiche(zugverzeichnis, zug, passwort);

        /// <summary>
        /// Die Konflikte, die in den geladenen Daten stecken
        /// </summary>
        public static List<KonfliktRules.Konflikt> FindeKonflikte() => KonfliktRules.FindeKonflikte();

        /// <summary>
        /// Ein Heer, wie es in der Oberfläche steht - mit den Gutpunkten, die der Auswerter noch
        /// ändern kann.
        /// </summary>
        public class Heereszeile {
            public required TruppenSpielfigur Heer { get; init; }
            public required string Reich { get; init; }
            public bool IstAngreifer { get; set; }

            /// <summary>
            /// Die Gutpunkte ohne die der Heerführer: Gelände oder Rüstort, Charaktere, Zauberer
            /// und der W20. Die Heerführer zählt der Ablauf selbst, nach dem Beschuss.
            /// </summary>
            public int Gutpunkte { get; set; }

            /// <summary>Woher die vorgeschlagenen Gutpunkte kommen</summary>
            public string Herkunft { get; set; } = string.Empty;

            public string Bezeichnung => Heer.Bezeichner;
            public int Stärke => Heer.staerke;
            public int Heerführer => Heer.hf;
            public int Katapulte => Heer.LKP + Heer.SKP;
        }

        /// <summary>
        /// Stellt die Heere eines Konflikts zusammen und schlägt ihre Gutpunkte vor.
        ///
        /// Vorgeschlagen wird, was sich aus der Karte ablesen lässt: der Vorteil des Rüstorts oder
        /// des Geländes (<see cref="KampfRules.BestimmeVerteidigungsvorteile"/>). Der Angreifer
        /// bekommt keinen davon - "Heere, die einen Rüstort angreifen oder verteidigen erhalten
        /// keine GP aus dem Geländevorteil" (Regelwerk 5.5.1), und der Angriffsvorteil hängt am
        /// Weg, den das Heer genommen hat. Den W20 und die Charaktere trägt der Auswerter nach.
        /// </summary>
        /// <param name="konflikt">der Konflikt</param>
        /// <param name="angreifer">das Reich, das angreift</param>
        public static List<Heereszeile> BaueHeereszeilen(KonfliktRules.Konflikt? konflikt, Nation? angreifer) {
            List<Heereszeile> zeilen = [];
            if (konflikt == null)
                return zeilen;

            var gemark = KleinfeldView.GetKleinfeld(konflikt.Gemark);
            foreach (var partei in konflikt.Parteien) {
                bool istAngreifer = angreifer != null && partei.Reich.Equals(angreifer);
                foreach (var figur in partei.Figuren) {
                    if (figur is not TruppenSpielfigur truppe)
                        continue;

                    var vorteile = istAngreifer
                        ? []
                        : KampfRules.BestimmeVerteidigungsvorteile(gemark, partei.Reich, truppe.BaseTyp);
                    zeilen.Add(new Heereszeile {
                        Heer = truppe,
                        Reich = partei.Reich.Reich,
                        IstAngreifer = istAngreifer,
                        Gutpunkte = KampfRules.BerechneVorteile(vorteile),
                        Herkunft = vorteile.Count == 0
                            ? (istAngreifer ? "Angreifer - kein Feldvorteil" : "kein Vorteil auf dieser Gemark")
                            : string.Join(", ", vorteile),
                    });
                }
            }
            return zeilen;
        }

        /// <summary>
        /// Rechnet die Schlacht mit dem, was in der Oberfläche steht.
        /// </summary>
        /// <param name="gemark">die umkämpfte Gemark</param>
        /// <param name="zeilen">die Heere mit ihren Gutpunkten</param>
        /// <param name="trefferpunkteGegenVerteidiger">was der Angreifer erschossen hat</param>
        /// <param name="trefferpunkteGegenAngreifer">was der Verteidiger erschossen hat</param>
        public static KampfablaufRules.Schlachtbericht Werte(KleinFeld? gemark,
                IEnumerable<Heereszeile>? zeilen,
                double trefferpunkteGegenVerteidiger = 0, double trefferpunkteGegenAngreifer = 0) {
            var alle = zeilen?.ToList() ?? [];

            var angreifer = Baue(alle.Where(zeile => zeile.IstAngreifer), gemark, verteidigt: false);
            var verteidiger = Baue(alle.Where(zeile => zeile.IstAngreifer == false), gemark, verteidigt: true);

            return KampfablaufRules.WerteSchlachtAus(gemark, angreifer, verteidiger,
                new KampfablaufRules.Würfe(trefferpunkteGegenVerteidiger, trefferpunkteGegenAngreifer));
        }

        /// <summary>
        /// Baut eine Seite. Die Feldvorteile gehören dem Verteidiger: sie sagen dem Beschuss, was
        /// das Bauwerk abfängt und ob ein Rüstort mitgetroffen wird.
        /// </summary>
        private static KampfablaufRules.Seite Baue(IEnumerable<Heereszeile> zeilen, KleinFeld? gemark, bool verteidigt) {
            var heere = zeilen
                .Select(zeile => new KampfablaufRules.Kämpfender(zeile.Heer, zeile.Gutpunkte, zeile.Heer.isbanned))
                .ToList();

            List<Kampfvorteil> vorteile = [];
            if (verteidigt && heere.Count > 0 && gemark != null) {
                var erste = zeilen.First();
                var nation = NationenView.GetNationFromString(erste.Reich);
                vorteile = KampfRules.BestimmeVerteidigungsvorteile(gemark, nation, erste.Heer.BaseTyp);
            }
            return new KampfablaufRules.Seite(heere, vorteile);
        }
    }
}
