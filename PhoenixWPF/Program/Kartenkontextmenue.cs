using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Das Kontextmenü, das der Rechtsklick auf ein Kleinfeld öffnet.
    ///
    /// Die rechte Taste tut nur noch dies; geschoben wird die Karte mit der mittleren oder mit
    /// Strg und der linken (<see cref="PhoenixDX.Helper.Kartensteuerung"/>). Hier kommt nur das
    /// fertige Ereignis an.
    /// </summary>
    internal static class Kartenkontextmenue {

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
            // Liste und unterschieden sich nur darin, was der Klick auslöste. Jetzt wählt er
            // aus - und an der Auswahl hängt die Anzeige der Bewegung.
            //
            // Es erscheint immer, auch wenn nichts anzubieten ist: fehlte es wortlos, wüsste
            // niemand, ob die Funktion fehlt oder nur nichts auf dem Feld steht.
            menü.Items.Add(BaueMöglicheZüge(gemark));

            // Die Bewegungsanzeige soll sich auch wieder loswerden lassen, ohne die Karte neu zu
            // laden. Eigene Markierungen des Benutzers bleiben dabei stehen - entfernt wird nur,
            // was die Bewegungsanzeige selbst gesetzt hat.
            var löschen = new MenuItem { Header = "Bewegungsanzeige aufheben" };
            löschen.Click += (s, e) => {
                BewegungView.VersteckeBewegung();
                Bewegungshinweis.Beende();
            };
            menü.Items.Add(new Separator());
            menü.Items.Add(löschen);

            menü.IsOpen = true;
        }

        /// <summary>
        /// Wählt eine Figur aus - denselben Weg, den auch die Figurenliste nimmt: die Karte rückt
        /// auf das Feld und die Figur wird die aktuelle Auswahl.
        ///
        /// Mehr ist hier nicht zu tun. An der Auswahl hängt die Anzeige der Bewegung: hellgrün die
        /// erreichbaren Felder, kornblumenblau der schon gelaufene Weg, dazu die Begründung am
        /// Mauszeiger und die Bewegung per Klick. Das Menü ist die <em>Auswahl</em> - und dafür
        /// gebraucht, weil auf einem Feld mehrere Einheiten stehen können und die Karte allein
        /// nicht weiss, welche gemeint ist.
        ///
        /// Steht der Zug noch in der Rüstphase, ist nichts erreichbar; dann wird gefragt, ob sie
        /// beendet werden soll, und die Anzeige danach aufgefrischt.
        ///
        /// <b>Die Karte bleibt dabei stehen.</b> Das Menü gehört zu einem Feld, auf das der
        /// Benutzer gerade mit der rechten Taste gezeigt hat - er schaut schon hin. Ein Schwenk
        /// auf die Bildmitte reisst ihm das Bild unter den Augen weg, und genau das ist gemeldet
        /// worden: "dann springt die Karte an eine andere Stelle". Vor dem Zusammenlegen der
        /// beiden Untermenüs rief der Eintrag "Mögliche Züge" nur das Hervorheben und bewegte die
        /// Karte nicht; das Nachführen kam aus dem Untermenü "Spielfiguren" mit.
        /// </summary>
        private static void Wähle(Spielfigur figur) {
            try {
                Main.Instance.Spiel?.SelectGemark(figur, karteNachführen: false);
                BieteDasEndeDerRüstphaseAn(figur);
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
        /// Ein Klick auf eine eigene Figur wählt sie aus; an der Auswahl hängt die Anzeige der
        /// Bewegung. Genau dafür wird das Menü gebraucht: auf einem Feld können mehrere Einheiten
        /// stehen, und die Karte allein weiss nicht, welche gemeint ist.
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
                    eintrag.Click += (s, e) => Wähle(gemerkt);
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
        /// Erreicht die Figur nichts, weil der Zug noch in der Rüstphase steht, wird das hier
        /// gefragt statt in eine Protokollzeile geschrieben, die niemand sucht.
        ///
        /// Die Phase ist kein Fehler, sondern eine Entscheidung, die noch aussteht. Wer gerade
        /// wissen wollte, wohin eine Figur ziehen kann, will sie jetzt beantworten.
        /// </summary>
        private static void BieteDasEndeDerRüstphaseAn(Spielfigur figur) {
            if (ZugView.Phase != Zugphase.Rüstphase)
                return;
            if (BewegungsRules.GetErreichbareFelder(figur).Count > 0)
                return;
            if (BeendeRüstphaseAufNachfrage() == false) {
                var grund = BewegungsRules.ErkläreWarumNichtsErreichbar(figur);
                SpielWPF.LogInfo(grund.Title, grund.Message);
                return;
            }
            // Die Anzeige hing an der Auswahl und lief vorhin ins Leere - jetzt noch einmal.
            Main.Instance.SelectionHistory.Refresh();
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
