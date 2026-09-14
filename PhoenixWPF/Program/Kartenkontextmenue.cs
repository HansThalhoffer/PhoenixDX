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

            menü.Items.Add(BaueSpielfiguren(gemark));

            // Das Untermenü erscheint immer, auch wenn nichts anzubieten ist - sonst fehlt es
            // wortlos und man weiß nicht, ob die Funktion fehlt oder nur nichts da ist.
            var eigene = EigeneFiguren(gemark);
            if (eigene.Count > 0) {
                menü.Items.Add(BaueMöglicheZüge(eigene));
            }
            else {
                var fremde = SpielfigurenView.GetSpielfiguren(gemark).Count;
                menü.Items.Add(new MenuItem {
                    Header = fremde > 0
                        ? $"Mögliche Züge (keine eigenen Einheiten, {fremde} fremde)"
                        : "Mögliche Züge (hier steht nichts)",
                    IsEnabled = false,
                });
            }

            // Eine Hervorhebung soll sich auch wieder loswerden lassen, ohne die Karte neu zu laden
            var löschen = new MenuItem { Header = "Hervorhebung aufheben" };
            löschen.Click += (s, e) => {
                Main.Map?.LöscheHervorhebung();
                Bewegungshinweis.Beende();
            };
            menü.Items.Add(new Separator());
            menü.Items.Add(löschen);

            menü.IsOpen = true;
        }

        /// <summary>
        /// Das Untermenü mit den Spielfiguren der Gemark. Ein Klick wählt die Figur aus - damit
        /// zeigt die Eigenschaftsanzeige sie an, und Umschalt+Klick auf die Karte bewegt sie.
        ///
        /// Fremde Figuren stehen mit ihrem Reich in der Liste, sind aber abgeblendet: auswählen
        /// lässt sich nur, was einem gehört. Sie stattdessen wegzulassen hiesse zu verschweigen,
        /// dass dort etwas steht.
        /// </summary>
        private static MenuItem BaueSpielfiguren(KleinFeld gemark) {
            var alle = SpielfigurenView.GetSpielfigurenZurAuswahl(gemark);
            if (alle.Count == 0)
                return new MenuItem { Header = "Spielfiguren (hier steht nichts)", IsEnabled = false };

            var menü = new MenuItem { Header = $"Spielfiguren ({alle.Count})" };
            foreach (var figur in alle) {
                bool eigene = figur.Nation != null && figur.Nation == ProgramView.SelectedNation;
                var eintrag = new MenuItem {
                    Header = eigene
                        ? $"{figur.Typ} {figur.Nummer} - {figur.Stärke}"
                        : $"{figur.Nation?.Reich}: {figur.Typ} {figur.Nummer} - {figur.Stärke}",
                    IsEnabled = eigene,
                };
                if (eigene) {
                    var gemerkt = figur;
                    eintrag.Click += (s, e) => Wähle(gemerkt);
                }
                menü.Items.Add(eintrag);
            }
            return menü;
        }

        /// <summary>
        /// Wählt eine Figur aus - denselben Weg, den auch die Figurenliste nimmt: die Karte rückt
        /// auf das Feld und die Figur wird die aktuelle Auswahl.
        /// </summary>
        private static void Wähle(Spielfigur figur) {
            try {
                Main.Instance.Spiel?.SelectGemark(figur);
                // Auswählen kann scheitern, ohne dass jemand etwas tut: Spielfigur.Select lässt
                // nur eigene Figuren zu. Das wortlos zu übergehen wäre das Schlimmste.
                //
                // ReferenceEquals steht hier ausdrücklich, obwohl == dasselbe täte: Spielfigur
                // erbt von KleinfeldPosition ein Equals nach gf/kf, zwei verschiedene Figuren
                // desselben Feldes sind also Equals. Wer diese Zeile später auf Equals, Contains
                // oder Distinct umstellt, prüft damit nicht mehr, was hier gemeint ist.
                if (ReferenceEquals(Main.Instance.SelectionHistory.Current, figur) == false)
                    SpielWPF.LogWarning($"{figur.Bezeichner} liess sich nicht auswählen",
                        figur.Nation == ProgramView.SelectedNation
                            ? "Die Auswahl wurde nicht übernommen."
                            : $"Die Figur gehört {figur.Nation?.Reich ?? "einem anderen Reich"}; auswählen lässt sich nur, was einem selbst gehört.");
            }
            catch (Exception ex) {
                SpielWPF.LogError($"{figur.Bezeichner} liess sich nicht auswählen", ex.Message);
            }
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
                    Bewegungshinweis.Beende();
                    SpielWPF.LogInfo($"{figur.Bezeichner} kann sich nicht bewegen",
                        figur.bp <= 0
                            ? "Die Figur hat keine Bewegungspunkte mehr."
                            : "Von diesem Feld aus ist mit den verbleibenden Bewegungspunkten kein Nachbarfeld erreichbar.");
                    Main.Map?.LöscheHervorhebung();
                    return;
                }

                var karte = Main.Map;
                if (karte == null) {
                    SpielWPF.LogError("Die Karte ist nicht ansprechbar",
                        "Die Verbindung zur Kartendarstellung fehlt, deshalb lässt sich nichts hervorheben.");
                    return;
                }

                // Ab jetzt erklaert der Mauszeiger, warum ein Feld nicht dabei ist
                Bewegungshinweis.Aktiviere(figur, erreichbar);

                var farbe = GetReichsfarbe(figur);
                int hervorgehoben = karte.HebeHervor(erreichbar.Select(k => new KleinfeldPosition(k.gf, k.kf)),
                    farbe.R, farbe.G, farbe.B, Deckkraft);

                SpielWPF.LogInfo($"{figur.Bezeichner} erreicht {erreichbar.Count} Felder",
                    $"Mit {figur.bp} Bewegungspunkten ab {figur.CreateBezeichner()}, hervorgehoben sind "
                    + $"{hervorgehoben} in {farbe}. Die Hervorhebung lässt sich über das Kontextmenü wieder aufheben.");
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
