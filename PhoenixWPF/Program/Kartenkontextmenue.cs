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

            // Ein Untermenü, nicht zwei: "Spielfiguren" und "Mögliche Züge" führten dieselbe
            // Liste und unterschieden sich nur darin, was der Klick auslöste. Jetzt tut er beides
            // - die Figur wird ausgewählt und ihre Züge werden hervorgehoben.
            //
            // Es erscheint immer, auch wenn nichts anzubieten ist: fehlte es wortlos, wüsste
            // niemand, ob die Funktion fehlt oder nur nichts auf dem Feld steht.
            menü.Items.Add(BaueMöglicheZüge(gemark));

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
        /// Das einzige Untermenü über das, was auf der Gemark steht.
        ///
        /// Angeboten wird alles - eigene Figuren aus den Zugdaten und fremde Einheiten aus der
        /// Feindaufklärung. Das sind zwei getrennte Quellen; wer nur die erste abfragt, meldet auf
        /// einem Feld voller fremder Heere "hier steht nichts".
        ///
        /// Fremde stehen mit ihrem Reich in der Liste, sind aber abgeblendet: auswählen lässt sich
        /// nur, was einem gehört. Sie wegzulassen hiesse zu verschweigen, dass dort etwas steht.
        ///
        /// Ein Klick auf eine eigene Figur wählt sie aus <em>und</em> hebt ihre möglichen Züge
        /// hervor. In der Bewegungsphase zieht ein Klick auf eines der hervorgehobenen Felder die
        /// Figur dann dorthin.
        /// </summary>
        private static MenuItem BaueMöglicheZüge(KleinFeld gemark) {
            var alle = SpielfigurenView.GetFeldbesetzung(gemark);
            if (alle.Count == 0)
                return new MenuItem { Header = "Mögliche Züge (hier steht nichts)", IsEnabled = false };

            int eigene = alle.Count(besetzung => besetzung.IstAuswählbar);
            // Die Phase steht in der Überschrift, nicht erst in einer Meldung hinterher. In der
            // Rüstphase bewegt sich nichts, und ohne diesen Hinweis sieht die leere Karte danach
            // wie ein Programmfehler aus.
            var züge = new MenuItem {
                Header = eigene == 0 ? $"Mögliche Züge (keine eigenen Einheiten, {alle.Count} fremde)"
                    : ZugView.Phase == Zugphase.Rüstphase ? "Mögliche Züge (erst die Rüstphase beenden)"
                    : "Mögliche Züge",
            };

            foreach (var besetzung in alle) {
                var eintrag = new MenuItem {
                    Header = Beschrifte(besetzung),
                    IsEnabled = besetzung.IstAuswählbar,
                };
                if (besetzung.IstAuswählbar) {
                    var gemerkt = besetzung.Figur!;
                    eintrag.Click += (s, e) => {
                        Wähle(gemerkt);
                        ZeigeMöglicheZüge(gemerkt);
                    };
                }
                züge.Items.Add(eintrag);
            }
            return züge;
        }

        /// <summary>
        /// Wie ein Eintrag heisst. Eigene Figuren tragen ihre Bewegungspunkte mit; von fremden
        /// weiss man sie nicht.
        ///
        /// Der Höchstwert steht mit dabei: "1 BP" sieht aus wie ein Programmfehler, sobald die
        /// Karte daraufhin nichts hervorhebt. "1 von 21 BP" erklärt sich selbst.
        /// </summary>
        private static string Beschrifte(SpielfigurenView.Feldeintrag besetzung) {
            if (besetzung.IstAuswählbar == false || besetzung.Figur == null)
                return besetzung.Beschriftung;
            return $"{besetzung.Beschriftung}, {besetzung.Figur.bp} von {besetzung.Figur.bp_max} BP";
        }

        /// <summary>
        /// Zeigt die erreichbaren Felder einer Figur in der Farbe ihres Reiches.
        ///
        /// Wird auch nach einem Zug wieder aufgerufen: die Figur steht dann woanders, und was sie
        /// von dort aus noch erreicht, ist neu zu rechnen.
        /// </summary>
        internal static void ZeigeMöglicheZüge(Spielfigur figur) {
            try {
                var erreichbar = BewegungsRules.GetErreichbareFelder(figur);

                // Die Rüstphase ist kein Fehler, sondern eine Entscheidung, die noch aussteht -
                // also wird sie hier gestellt und nicht in eine Meldung geschrieben, die im
                // Protokoll untergeht. Wer zusieht, warum die Karte nichts hervorhebt, will genau
                // das jetzt beantworten.
                if (erreichbar.Count == 0 && ZugView.Phase == Zugphase.Rüstphase) {
                    if (BeendeRüstphaseAufNachfrage() == false)
                        return;
                    erreichbar = BewegungsRules.GetErreichbareFelder(figur);
                }

                if (erreichbar.Count == 0) {
                    Bewegungshinweis.Beende();
                    // Den Grund kennen die Regeln - vorher stand hier geraten, es lae­ge an den
                    // Bewegungspunkten, und der haeufigste Grund ist die Phase.
                    var grund = BewegungsRules.ErkläreWarumNichtsErreichbar(figur);
                    SpielWPF.LogInfo(grund.Title, grund.Message);
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
                    + $"{hervorgehoben} in {farbe}. Ein Klick auf eines davon zieht die Figur dorthin; "
                    + "die Hervorhebung lässt sich über das Kontextmenü wieder aufheben.");
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die möglichen Züge von {figur.Bezeichner} liessen sich nicht ermitteln", ex.Message);
            }
        }

        /// <summary>
        /// Fragt, ob die Rüstphase jetzt beendet werden soll, und beendet sie gegebenenfalls.
        ///
        /// Dieselbe Frage stellt das Zug-Menü; sie hier zu wiederholen erspart den Umweg, wenn
        /// jemand gerade wissen wollte, wohin eine Figur ziehen kann. Die Warnung ist dieselbe:
        /// danach ist in diesem Zug nicht mehr zu rüsten und nicht mehr zu bauen.
        /// </summary>
        /// <returns>true, wenn die Rüstphase daraufhin beendet wurde</returns>
        private static bool BeendeRüstphaseAufNachfrage() {
            var antwort = MessageBox.Show(
                "Der Zug steht in der Rüstphase; darin wird nicht bewegt (Regelwerk Kapitel 3).\r\n\r\n"
                + "Die Rüstphase jetzt beenden? Danach kann in diesem Zug nicht mehr gerüstet und "
                + "nicht mehr gebaut werden. Zurück geht es nicht.",
                "Rüstphase beenden", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (antwort != MessageBoxResult.Yes)
                return false;

            var ergebnis = ZugView.NächstePhase();
            if (ergebnis.HasErrors) {
                SpielWPF.LogError(ergebnis.Title, ergebnis.Message);
                return false;
            }
            SpielWPF.LogInfo(ergebnis.Title, ergebnis.Message);
            return true;
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
