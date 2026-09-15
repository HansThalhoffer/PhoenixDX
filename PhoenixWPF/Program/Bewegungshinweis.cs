using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Der Hinweis, warum ein Feld nicht erreichbar ist.
    ///
    /// Solange die moeglichen Zuege einer Figur hervorgehoben sind, zeigt die Karte nur, wohin es
    /// geht. Die haeufigere Frage ist, warum es anderswohin nicht geht - und die Antwort steht
    /// sonst nirgends. Faehrt der Mauszeiger ueber ein nicht hervorgehobenes Feld, stehen hier alle
    /// Gruende.
    ///
    /// Der Hinweis lebt nur, solange eine Hervorhebung aktiv ist. Ohne sie gibt es keinen Bezug,
    /// auf den sich ein "nicht erreichbar" beziehen koennte.
    /// </summary>
    internal static class Bewegungshinweis {

        private static ToolTip? _hinweis = null;
        private static Spielfigur? _figur = null;
        private static HashSet<string> _erreichbar = [];
        private static string _gezeigtesFeld = string.Empty;

        /// <summary>Die Figur, deren Zuege gerade hervorgehoben sind - oder null</summary>
        internal static Spielfigur? Figur => _figur;

        /// <summary>
        /// Gehoert dieses Feld zu den gerade hervorgehobenen?
        ///
        /// Massgeblich ist, was tatsaechlich leuchtet, und nicht eine zweite Rechnung: sonst
        /// koennte ein Feld anklickbar sein, das gar nicht hervorgehoben ist - oder umgekehrt.
        /// </summary>
        internal static bool IstHervorgehoben(KleinFeld? feld)
            => feld != null && _erreichbar.Contains(feld.Bezeichner);

        /// <summary>
        /// Merkt sich, wessen Zuege hervorgehoben sind. Ab jetzt gibt es Hinweise.
        /// </summary>
        internal static void Aktiviere(Spielfigur figur, IEnumerable<KleinFeld> erreichbar) {
            _figur = figur;
            _erreichbar = erreichbar.Select(feld => feld.Bezeichner).ToHashSet();
            Verbirg();
        }

        /// <summary>
        /// Beendet die Hinweise - die Hervorhebung ist weg, der Bezug also auch.
        /// </summary>
        internal static void Beende() {
            _figur = null;
            _erreichbar = [];
            Verbirg();
        }

        /// <summary>
        /// Zeigt den Hinweis zu dem Feld unter dem Mauszeiger, oder nimmt ihn zurueck.
        /// </summary>
        /// <param name="gf">Grossfeld, 0 wenn der Zeiger ueber keinem Feld steht</param>
        internal static void Zeige(int gf, int kf) {
            if (_figur == null || gf == 0) {
                Verbirg();
                return;
            }

            var gemark = KleinfeldView.GetKleinfeld(new KleinfeldPosition(gf, kf));
            if (gemark == null) {
                Verbirg();
                return;
            }
            // Was hervorgehoben ist, braucht keine Erklaerung
            if (_erreichbar.Contains(gemark.Bezeichner)) {
                Verbirg();
                return;
            }
            if (gemark.Bezeichner == _gezeigtesFeld)
                return;

            List<string> gründe;
            try {
                gründe = BewegungsRules.ErkläreUnerreichbarkeit(_figur, gemark);
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die Gründe für {gemark.Bezeichner} liessen sich nicht ermitteln", ex.Message);
                Verbirg();
                return;
            }
            if (gründe.Count == 0) {
                Verbirg();
                return;
            }

            _gezeigtesFeld = gemark.Bezeichner;
            ZeigeText($"{_figur.Bezeichner} erreicht {gemark.Bezeichner} nicht", gründe);
        }

        /// <summary>
        /// Baut den Hinweis auf und oeffnet ihn an der Mausposition.
        ///
        /// Bei jedem Feldwechsel neu geoeffnet: ein ToolTip bleibt sonst dort stehen, wo er
        /// aufgegangen ist, und zeigte dann am falschen Feld den falschen Text.
        /// </summary>
        private static void ZeigeText(string kopf, List<string> gründe) {
            var inhalt = new StackPanel();
            inhalt.Children.Add(new TextBlock {
                Text = kopf,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4),
            });
            foreach (var grund in gründe)
                inhalt.Children.Add(new TextBlock {
                    Text = "• " + grund,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 420,
                });

            if (_hinweis == null) {
                _hinweis = new ToolTip {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    StaysOpen = true,
                    HasDropShadow = true,
                };
                // Der Mauszeiger steht über dem eingebetteten Kartenfenster, nicht über einem
                // WPF-Element. Ohne Anker im Fenster hat der Hinweis keinen Platz, an dem er
                // aufgehen könnte.
                if (Application.Current?.MainWindow != null)
                    _hinweis.PlacementTarget = Application.Current.MainWindow;
            }
            _hinweis.Content = inhalt;
            _hinweis.IsOpen = false;
            _hinweis.IsOpen = true;
        }

        /// <summary>Nimmt den Hinweis zurueck</summary>
        internal static void Verbirg() {
            _gezeigtesFeld = string.Empty;
            if (_hinweis != null)
                _hinweis.IsOpen = false;
        }
    }
}
