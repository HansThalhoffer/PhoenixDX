using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Was zusätzlich auf der Karte stehen kann.
    ///
    /// Fünf Anzeigen, die die Altanwendung in ihren Menüs führt - und von denen dort keine
    /// einzige angeschlossen ist: die Schalter stehen im Formular, werden aber nirgends gelesen,
    /// und der Folienzug ist im Quelltext ausdrücklich als "wird erstmal nicht mehr unterstützt"
    /// vermerkt. Übernommen ist deshalb die Absicht, nicht der Code.
    ///
    /// Hier stehen die Entscheidungen - welcher Text auf welches Feld gehört, welche Bauwerke
    /// beschädigt sind, welche Heere stehengeblieben sind und welche Wege in diesem Zug gelaufen
    /// wurden. Das Zeichnen ist Sache der Kartendarstellung und bleibt dünn.
    /// </summary>
    public static class KartenanzeigeView {

        #region Beschriftung der Felder

        /// <summary>
        /// Was auf einem Feld stehen soll.
        /// </summary>
        public enum Feldbeschriftung {
            /// <summary>nichts - der Normalfall</summary>
            Keine,
            /// <summary>die Stärke der eigenen Figuren auf dem Feld</summary>
            Stärke,
            /// <summary>die Nummern der eigenen Figuren auf dem Feld</summary>
            Nummer,
        }

        /// <summary>
        /// Der Text, der auf diesem Feld stehen soll - oder ein leerer Text.
        ///
        /// Gezeigt wird nur, was einem gehört. Von fremden Einheiten kennt man aus der
        /// Feindaufklärung die Nummer, aber nicht die Stärke; beides nebeneinander zu stellen
        /// hiesse, eine Vermutung wie eine Tatsache aussehen zu lassen.
        /// </summary>
        public static string GetBeschriftung(KleinFeld? gemark, Feldbeschriftung art) {
            if (gemark == null || art == Feldbeschriftung.Keine)
                return string.Empty;

            var eigene = SpielfigurenView.GetSpielfiguren(gemark)
                .Where(figur => figur.Nation != null && figur.Nation == ProgramView.SelectedNation
                                && Plausibilität.IsValid(figur))
                .ToList();
            if (eigene.Count == 0)
                return string.Empty;

            if (art == Feldbeschriftung.Nummer)
                return string.Join("/", eigene.Select(figur => figur.Nummer));

            // Stärke: die Truppen summiert, Namensfiguren zählen als eine Person
            int stärke = 0;
            int namensfiguren = 0;
            foreach (var figur in eigene) {
                if (figur is TruppenSpielfigur truppe)
                    stärke += truppe.staerke;
                else
                    namensfiguren++;
            }
            if (stärke > 0 && namensfiguren > 0)
                return $"{stärke}+{namensfiguren}";
            if (stärke > 0)
                return $"{stärke}";
            return $"+{namensfiguren}";
        }

        /// <summary>
        /// Die Beschriftungen aller Felder, auf denen etwas steht - für die Kartendarstellung.
        /// </summary>
        public static Dictionary<string, string> GetBeschriftungen(Feldbeschriftung art) {
            Dictionary<string, string> beschriftungen = [];
            if (art == Feldbeschriftung.Keine || SharedData.Map == null)
                return beschriftungen;

            foreach (var gemark in SharedData.Map.Values) {
                string text = GetBeschriftung(gemark, art);
                if (string.IsNullOrEmpty(text) == false)
                    beschriftungen[gemark.Bezeichner] = text;
            }
            return beschriftungen;
        }

        #endregion

        #region Beschädigte Rüstorte und Bauwerke

        /// <summary>
        /// Ein beschädigtes Bauwerk und was ihm fehlt.
        /// </summary>
        /// <param name="Gemark">wo es steht</param>
        /// <param name="Soll">die Stufe, die dort stehen soll</param>
        /// <param name="Ist">die Stufe, die die Baupunkte noch tragen - leer, wenn gar keine mehr</param>
        /// <param name="Baupunkte">die vorhandenen Baupunkte</param>
        public record class Beschädigung(KleinFeld Gemark, string Soll, string Ist, int Baupunkte) {
            /// <summary>Eine Zeile, wie sie in einer Liste steht</summary>
            public string Beschreibung => Ist.Length == 0
                ? $"{Gemark.Bezeichner}: {Soll} zerstört ({Baupunkte} Baupunkte)"
                : $"{Gemark.Bezeichner}: {Soll} auf {Ist} herunter ({Baupunkte} Baupunkte)";
        }

        /// <summary>
        /// Die eigenen Rüstorte, deren Baupunkte ihre Sollstufe nicht mehr tragen.
        ///
        /// Zwei Spalten der Karte beschreiben einen Rüstort: <c>Ruestort</c> sagt, was dort stehen
        /// soll, <c>Baupunkte</c>, was davon noch steht. Fallen die Baupunkte unter die Stufe,
        /// zählt der Ort nur noch als das, was sie tragen (Regelwerk 1.5) - und genau das rechnet
        /// <see cref="BauwerkeView.GetRüstortNachKarte"/> aus.
        /// </summary>
        public static List<Beschädigung> FindeBeschädigte() {
            List<Beschädigung> beschädigt = [];
            if (SharedData.Map == null)
                return beschädigt;

            foreach (var gemark in SharedData.Map.Values) {
                if (gemark.Nation == null || gemark.Nation != ProgramView.SelectedNation)
                    continue;

                string soll = RuestortRules.GetGrundstufe(gemark.Ruestort == null
                    ? null
                    : BauwerkeView.GetRuestortReferenz(gemark.Ruestort)?.Ruestort) ?? string.Empty;
                if (string.IsNullOrEmpty(soll))
                    continue;

                var erreicht = BauwerkeView.GetRüstortNachKarte(gemark);
                string ist = RuestortRules.GetGrundstufe(erreicht?.Ruestort) ?? string.Empty;
                if (string.Equals(soll, ist, StringComparison.OrdinalIgnoreCase))
                    continue;

                beschädigt.Add(new Beschädigung(gemark, soll, ist, gemark.Baupunkte));
            }
            beschädigt.Sort((eines, anderes) => string.Compare(eines.Gemark.Bezeichner,
                anderes.Gemark.Bezeichner, StringComparison.OrdinalIgnoreCase));
            return beschädigt;
        }

        #endregion

        #region Fest stationierte Heere

        /// <summary>
        /// Die eigenen Figuren, die in diesem Zug nicht gezogen sind.
        ///
        /// <b>Was "fest stationiert" heisst, legt diese Anwendung fest.</b> Die Altanwendung führt
        /// den Menüpunkt, aber keine Erklärung und keinen Code dazu, und das Regelwerk kennt den
        /// Begriff nicht. Genommen ist die naheliegende Lesart: eine Figur, die in diesem Monat
        /// keinen Schritt getan hat - die Besatzung, die steht, während der Rest marschiert.
        ///
        /// Figuren ohne Bewegungspunkte sind damit nicht gemeint; die sind erschöpft, nicht
        /// stationiert. Unterscheiden lässt sich das an der Bewegungsspur.
        /// </summary>
        public static List<Spielfigur> FindeStationierte() {
            List<Spielfigur> stehengeblieben = [];
            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)) {
                if (Plausibilität.IsValid(figur) == false)
                    continue;
                if (new Bewegungsspur(figur).Count > 0)
                    continue;
                stehengeblieben.Add(figur);
            }
            return stehengeblieben;
        }

        #endregion

        #region Folienzug - alle Bewegungen des Zuges auf einen Blick

        /// <summary>
        /// Welche Figurengattungen der Folienzug zeigt.
        ///
        /// Die Altanwendung führt dafür je Gattung eine Farbe und einen Schalter
        /// (Folienzugsettingitem); übernommen ist die Aufteilung, nicht der Code.
        /// </summary>
        public record class Folienzugeinstellung(
                bool Krieger = true, bool Reiter = true, bool Schiffe = true,
                bool Kreaturen = true, bool Zauberer = true, bool Charaktere = true,
                bool MitStartfeld = true) {

            public static readonly Folienzugeinstellung Alles = new();

            /// <summary>Gehört diese Gattung dazu?</summary>
            public bool Zeigt(FigurType typ) => typ switch {
                FigurType.Krieger or FigurType.LeichteArtillerie or FigurType.SchwereArtillerie
                    or FigurType.BeritteneLeichteArtillerie or FigurType.BeritteneSchwereArtillerie => Krieger,
                FigurType.Reiter => Reiter,
                FigurType.Schiff or FigurType.LeichtesKriegsschiff or FigurType.SchweresKriegsschiff
                    or FigurType.PiratenSchiff or FigurType.PiratenLeichtesKriegsschiff
                    or FigurType.PiratenSchweresKriegsschiff => Schiffe,
                FigurType.Kreatur => Kreaturen,
                FigurType.Zauberer => Zauberer,
                FigurType.Charakter or FigurType.CharakterZauberer => Charaktere,
                _ => false,
            };
        }

        /// <summary>
        /// Der Weg einer Figur in diesem Zug.
        /// </summary>
        /// <param name="Figur">wer gelaufen ist</param>
        /// <param name="Gattung">wonach sich die Farbe richtet</param>
        /// <param name="Felder">die Felder in der Reihenfolge des Weges, das Startfeld zuerst</param>
        public record class Zugspur(Spielfigur Figur, FigurType Gattung, List<KleinfeldPosition> Felder);

        /// <summary>
        /// Alle Wege, die die eigenen Figuren in diesem Zug gelaufen sind.
        ///
        /// Das ist der Folienzug: was sonst nur eine Figur nach der anderen zeigt, liegt hier auf
        /// einmal über der Karte - wie die Folie, die früher über den Spielplan gelegt wurde.
        /// </summary>
        public static List<Zugspur> FindeZugspuren(Folienzugeinstellung? einstellung = null) {
            einstellung ??= Folienzugeinstellung.Alles;
            List<Zugspur> spuren = [];

            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)) {
                if (Plausibilität.IsValid(figur) == false)
                    continue;
                if (einstellung.Zeigt(figur.Typ) == false)
                    continue;

                var spur = new Bewegungsspur(figur);
                if (spur.Count == 0)
                    continue;

                List<KleinfeldPosition> felder = [];
                if (einstellung.MitStartfeld && figur.gf_von > 0)
                    felder.Add(new KleinfeldPosition(figur.gf_von, figur.kf_von));
                felder.AddRange(spur.ToList());
                spuren.Add(new Zugspur(figur, figur.Typ, felder));
            }
            return spuren;
        }

        #endregion
    }
}
