using PhoenixModel.View;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.KartenanzeigeView;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Schaltet die zusätzlichen Kartenanzeigen ein und aus.
    ///
    /// Fünf Anzeigen, die die Altanwendung in ihren Menüs führt und von denen dort keine einzige
    /// angeschlossen ist. Was auf die Karte gehört, entscheidet
    /// <see cref="KartenanzeigeView"/>; hier steht nur, was gerade eingeschaltet ist und wie es
    /// auf die Karte kommt.
    ///
    /// Die drei Anzeigen teilen sich zwei Kanäle: die <em>Beschriftung</em> der Felder und die
    /// <em>Hervorhebung</em>. Deshalb gilt je Kanal eine Anzeige zur Zeit - zwei Texte passen
    /// nicht auf dasselbe Feld, und zwei Einfärbungen auch nicht.
    /// </summary>
    internal static class Kartenanzeigen {

        /// <summary>
        /// Was gerade auf den Feldern steht.
        /// </summary>
        public static Feldbeschriftung Beschriftung { get; private set; } = Feldbeschriftung.Keine;

        /// <summary>
        /// Welche Einfärbung gerade liegt.
        /// </summary>
        public enum Einfärbung {
            /// <summary>keine</summary>
            Keine,
            /// <summary>die beschädigten eigenen Rüstorte</summary>
            BeschädigteBauwerke,
            /// <summary>die Figuren, die in diesem Zug nicht gezogen sind</summary>
            StationierteHeere,
            /// <summary>alle Wege dieses Zuges</summary>
            Folienzug,
        }

        public static Einfärbung Färbung { get; private set; } = Einfärbung.Keine;

        /// <summary>Welche Gattungen der Folienzug zeigt</summary>
        public static Folienzugeinstellung Folienzugeinstellung { get; set; } = Folienzugeinstellung.Alles;

        /// <summary>
        /// Die Farben des Folienzugs je Gattung - dieselbe Aufteilung, die die Altanwendung
        /// vorsah: je Gattung eine Farbe, damit sich die Wege auseinanderhalten lassen.
        ///
        /// Keine davon kommt im Gelände vor; das Gelände ist grün, oliv, blau und sandgelb
        /// (siehe <see cref="PhoenixDX.Helper.Markerfarben"/>).
        /// </summary>
        private static (byte R, byte G, byte B) FarbeFür(PhoenixModel.ExternalTables.FigurType typ) => typ switch {
            PhoenixModel.ExternalTables.FigurType.Reiter => (255, 0, 255),            // Magenta
            PhoenixModel.ExternalTables.FigurType.Kreatur => (255, 128, 0),           // Orange
            PhoenixModel.ExternalTables.FigurType.Zauberer => (160, 0, 255),          // Violett
            PhoenixModel.ExternalTables.FigurType.Charakter
                or PhoenixModel.ExternalTables.FigurType.CharakterZauberer => (255, 255, 255), // Weiss
            PhoenixModel.ExternalTables.FigurType.Schiff
                or PhoenixModel.ExternalTables.FigurType.LeichtesKriegsschiff
                or PhoenixModel.ExternalTables.FigurType.SchweresKriegsschiff
                or PhoenixModel.ExternalTables.FigurType.PiratenSchiff
                or PhoenixModel.ExternalTables.FigurType.PiratenLeichtesKriegsschiff
                or PhoenixModel.ExternalTables.FigurType.PiratenSchweresKriegsschiff => (255, 0, 64), // Rot
            _ => (0, 0, 0),                                                           // Krieger: Schwarz
        };

        /// <summary>Die Deckkraft der Einfärbung - das Gelände soll durchscheinen</summary>
        private const float Deckkraft = 0.5f;

        /// <summary>
        /// Stellt die Beschriftung um. Dieselbe noch einmal schaltet sie ab.
        /// </summary>
        public static void SetzeBeschriftung(Feldbeschriftung art) {
            Beschriftung = Beschriftung == art ? Feldbeschriftung.Keine : art;
            Zeichne();
        }

        /// <summary>
        /// Stellt die Einfärbung um. Dieselbe noch einmal schaltet sie ab.
        /// </summary>
        public static void SetzeFärbung(Einfärbung art) {
            Färbung = Färbung == art ? Einfärbung.Keine : art;
            Zeichne();
        }

        /// <summary>
        /// Schaltet jede Kartenanzeige ab - für den Fall, dass man die Karte wieder nackt sehen
        /// will, ohne sich zu erinnern, was gerade an ist.
        /// </summary>
        public static void AllesAus() {
            Beschriftung = Feldbeschriftung.Keine;
            Färbung = Einfärbung.Keine;
            Zeichne();
        }

        /// <summary>
        /// Legt beides neu auf die Karte. Wird auch nach einem Zugwechsel gebraucht.
        /// </summary>
        public static void Zeichne() {
            var karte = Main.Map;
            if (karte == null)
                return;
            try {
                ZeichneBeschriftung(karte);
                ZeichneFärbung(karte);
            }
            catch (Exception ex) {
                Absturzbericht.Berichte("Die Kartenanzeige liess sich nicht auffrischen", ex, tödlich: false);
            }
        }

        private static void ZeichneBeschriftung(PhoenixDX.MappaMundi karte) {
            if (Beschriftung == Feldbeschriftung.Keine && Färbung != Einfärbung.Folienzug) {
                karte.LöscheBeschriftung();
                return;
            }

            // Der Folienzug schreibt die Figurennummern auf seine Felder - sonst sieht man zwar,
            // dass dort jemand ging, aber nicht wer.
            var texte = Beschriftung == Feldbeschriftung.Keine
                ? new Dictionary<string, string>()
                : GetBeschriftungen(Beschriftung);

            if (Färbung == Einfärbung.Folienzug) {
                foreach (var spur in FindeZugspuren(Folienzugeinstellung)) {
                    foreach (var feld in spur.Felder) {
                        string schlüssel = feld.CreateBezeichner();
                        texte[schlüssel] = texte.TryGetValue(schlüssel, out var vorhanden)
                            ? $"{vorhanden}/{spur.Figur.Nummer}"
                            : spur.Figur.Nummer.ToString();
                    }
                }
            }
            // Die Karte findet ihre Felder über die Koordinaten; der Bezeichner aus der Datenbank
            // ist dort ein anderer Text.
            var mitKoordinaten = new List<(KleinfeldPosition Feld, string Text)>();
            foreach (var eintrag in texte) {
                if (PhoenixModel.ViewModel.SharedData.Map != null
                    && PhoenixModel.ViewModel.SharedData.Map.TryGetValue(eintrag.Key, out var gemark)
                    && gemark != null)
                    mitKoordinaten.Add((new KleinfeldPosition(gemark.gf, gemark.kf), eintrag.Value));
            }
            karte.Beschrifte(mitKoordinaten);
        }

        private static void ZeichneFärbung(PhoenixDX.MappaMundi karte) {
            switch (Färbung) {
                case Einfärbung.Keine:
                    karte.LöscheHervorhebung();
                    break;

                case Einfärbung.BeschädigteBauwerke: {
                    var beschädigt = FindeBeschädigte();
                    int gezeigt = karte.HebeHervor(
                        beschädigt.Select(eintrag => new KleinfeldPosition(eintrag.Gemark.gf, eintrag.Gemark.kf)),
                        255, 0, 0, Deckkraft);
                    SpielWPF.LogInfo($"{beschädigt.Count} beschädigte Rüstorte",
                        beschädigt.Count == 0
                            ? "Alle eigenen Rüstorte tragen ihre Stufe."
                            : string.Join("\r\n", beschädigt.Select(eintrag => eintrag.Beschreibung))
                              + $"\r\nHervorgehoben sind {gezeigt} Felder.");
                    break;
                }

                case Einfärbung.StationierteHeere: {
                    var stehend = FindeStationierte();
                    karte.HebeHervor(
                        stehend.Select(figur => new KleinfeldPosition(figur.gf, figur.kf)),
                        0, 160, 255, Deckkraft);
                    SpielWPF.LogInfo($"{stehend.Count} Figuren sind in diesem Zug nicht gezogen",
                        "Hervorgehoben sind die Felder, auf denen sie stehen. Gemeint ist, wer keinen "
                        + "Schritt getan hat - nicht, wer keine Bewegungspunkte mehr hat.");
                    break;
                }

                case Einfärbung.Folienzug: {
                    var spuren = FindeZugspuren(Folienzugeinstellung);
                    // Je Gattung eine eigene Farbe. HebeHervor löscht jedes Mal zuerst, deshalb
                    // wird einmal gelöscht und danach nur noch hinzugefügt.
                    karte.LöscheHervorhebung();
                    foreach (var gruppe in spuren.GroupBy(spur => FarbeFür(spur.Gattung))) {
                        var felder = gruppe
                            .SelectMany(spur => spur.Felder)
                            .Select(feld => new KleinfeldPosition(feld.gf, feld.kf))
                            .ToList();
                        karte.FärbeEin(felder, gruppe.Key.R, gruppe.Key.G, gruppe.Key.B, Deckkraft);
                    }
                    SpielWPF.LogInfo($"{spuren.Count} Wege in diesem Zug",
                        "Jede Gattung in ihrer Farbe, die Nummer der Figur steht auf dem Feld. "
                        + "Welche Gattungen gezeigt werden, steht unter Ansicht.");
                    break;
                }
            }
        }
    }
}
