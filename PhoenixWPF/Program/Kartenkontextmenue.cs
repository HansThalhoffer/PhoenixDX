using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Das Kontextmenü, das der Rechtsklick auf ein Kleinfeld öffnet.
    ///
    /// Der Rechtsklick schiebt auch die Karte; das Menü erscheint deshalb nur, wenn die Maus
    /// zwischen Drücken und Loslassen stehen geblieben ist. Diese Unterscheidung trifft die
    /// Kartendarstellung, hier kommt nur noch das fertige Ereignis an.
    /// </summary>
    internal static class Kartenkontextmenue {

        /// <summary>
        /// Die Deckkraft der Hervorhebung. Weniger als die Hälfte, damit das Gelände darunter
        /// noch zu erkennen ist.
        /// </summary>
        private const float Deckkraft = 0.45f;

        /// <summary>
        /// Baut das Menü zu einem Kleinfeld und zeigt es an der Mausposition.
        /// </summary>
        public static void Zeige(int gf, int kf) {
            var gemark = KleinfeldView.GetKleinfeld(new KleinfeldPosition(gf, kf));
            if (gemark == null)
                return;

            var menü = new ContextMenu { Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint };

            var info = new MenuItem { Header = $"Info zu {gemark.Bezeichner}" };
            info.Click += (s, e) => ZeigeInfo(gemark);
            menü.Items.Add(info);

            var eigene = EigeneFiguren(gemark);
            if (eigene.Count > 0)
                menü.Items.Add(BaueMöglicheZüge(eigene));

            // Eine Hervorhebung soll sich auch wieder loswerden lassen, ohne die Karte neu zu laden
            var löschen = new MenuItem { Header = "Hervorhebung aufheben" };
            löschen.Click += (s, e) => Main.Map?.LöscheHervorhebung();
            menü.Items.Add(new Separator());
            menü.Items.Add(löschen);

            menü.IsOpen = true;
        }

        /// <summary>
        /// Die Figuren des eigenen Reiches, die auf dieser Gemark stehen und sich bewegen können.
        /// </summary>
        private static List<Spielfigur> EigeneFiguren(KleinFeld gemark) {
            return SpielfigurenView.GetSpielfiguren(gemark)
                .Where(figur => figur.Nation != null && figur.Nation == ProgramView.SelectedNation)
                .OrderBy(figur => figur.Typ.ToString())
                .ThenBy(figur => figur.Nummer)
                .ToList();
        }

        /// <summary>
        /// Das Untermenü mit einem Eintrag je eigener Figur. Die Auswahl hebt die Felder hervor,
        /// die diese Figur in diesem Zug noch erreichen kann.
        /// </summary>
        private static MenuItem BaueMöglicheZüge(List<Spielfigur> figuren) {
            var züge = new MenuItem { Header = "Mögliche Züge" };
            foreach (var figur in figuren) {
                var eintrag = new MenuItem {
                    Header = $"{figur.Typ} {figur.Nummer} - {figur.Stärke}, {figur.bp} BP",
                };
                var gemerkt = figur;
                eintrag.Click += (s, e) => ZeigeMöglicheZüge(gemerkt);
                züge.Items.Add(eintrag);
            }
            return züge;
        }

        /// <summary>
        /// Zeigt die erreichbaren Felder einer Figur in der Farbe ihres Reiches.
        /// </summary>
        private static void ZeigeMöglicheZüge(Spielfigur figur) {
            try {
                var erreichbar = BewegungsRules.GetErreichbareFelder(figur);
                if (erreichbar.Count == 0) {
                    SpielWPF.LogInfo($"{figur.Bezeichner} kann sich nicht bewegen",
                        figur.bp <= 0
                            ? "Die Figur hat keine Bewegungspunkte mehr."
                            : "Von diesem Feld aus ist mit den verbleibenden Bewegungspunkten kein Nachbarfeld erreichbar.");
                    Main.Map?.LöscheHervorhebung();
                    return;
                }

                var farbe = GetReichsfarbe(figur);
                Main.Map?.HebeHervor(erreichbar.Select(k => new KleinfeldPosition(k.gf, k.kf)),
                    farbe.R, farbe.G, farbe.B, Deckkraft);

                SpielWPF.LogInfo($"{figur.Bezeichner} erreicht {erreichbar.Count} Felder",
                    $"Mit {figur.bp} Bewegungspunkten ab {figur.CreateBezeichner()}. "
                    + "Die Hervorhebung lässt sich über das Kontextmenü wieder aufheben.");
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die möglichen Züge von {figur.Bezeichner} liessen sich nicht ermitteln", ex.Message);
            }
        }

        /// <summary>
        /// Die Farbe des Reiches, zu dem die Figur gehört. Kennt das Reich keine, wird eine
        /// auffällige Ersatzfarbe genommen - die Hervorhebung soll ja gesehen werden.
        /// </summary>
        private static Color GetReichsfarbe(Spielfigur figur) {
            var farbe = figur.Nation?.Farbe;
            if (farbe == null)
                return Colors.Yellow;
            return Color.FromRgb(farbe.Value.R, farbe.Value.G, farbe.Value.B);
        }

        private static void ZeigeInfo(KleinFeld gemark) {
            try {
                new GemarkInfoDialog(gemark).ShowDialog();
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die Infos zu {gemark.Bezeichner} liessen sich nicht anzeigen", ex.Message);
            }
        }
    }
}
