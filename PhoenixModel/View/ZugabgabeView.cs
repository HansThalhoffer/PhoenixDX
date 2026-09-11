using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;
using System.Text;

namespace PhoenixModel.View {

    /// <summary>
    /// Was die Zugabgabe mit einer einzelnen Figur vorhat
    /// </summary>
    public enum ZugabgabeSchicksal {
        /// <summary>Die Figur hat sich bewegt und wird mit ihrer neuen Position übernommen</summary>
        Bewegt,
        /// <summary>Die Figur ist stehen geblieben und behält ihre Position</summary>
        Stehengeblieben,
        /// <summary>Die Figur hat keinen Heerführer bzw. keine Gutpunkte mehr und verschwindet</summary>
        Aufgelöst,
    }

    /// <summary>
    /// Was die Zugabgabe mit einer Figur vorhat, für die Anzeige im Trockenlauf
    /// </summary>
    public class ZugabgabeEintrag {
        public required Spielfigur Figur { get; init; }
        public required ZugabgabeSchicksal Schicksal { get; init; }
        /// <summary>Bewegungspunkte, die die Figur in diesem Zug nicht verbraucht hat</summary>
        public int UngenutzteBewegungspunkte { get; init; }

        public override string ToString() {
            string text = $"{Figur.Bezeichner} auf {Figur.CreateBezeichner()}: {Schicksal}";
            if (UngenutzteBewegungspunkte > 0)
                text += $", {UngenutzteBewegungspunkte} BP ungenutzt";
            return text;
        }
    }

    /// <summary>
    /// Der Bericht eines Trockenlaufs der Zugabgabe
    /// </summary>
    public class ZugabgabeBericht {
        public required Zugmonat AktuellerZug { get; init; }
        public required Zugmonat NächsterZug { get; init; }
        public required Zugphase Phase { get; init; }
        public List<ZugabgabeEintrag> Figuren { get; } = [];
        /// <summary>Hinweise, die der Benutzer vor der Abgabe sehen sollte</summary>
        public List<string> Hinweise { get; } = [];
        /// <summary>Gründe, die eine Abgabe verhindern</summary>
        public List<string> Hindernisse { get; } = [];

        public bool KannAbgegebenWerden => Hindernisse.Count == 0;

        public int AnzahlBewegt => Figuren.Count(f => f.Schicksal == ZugabgabeSchicksal.Bewegt);
        public int AnzahlStehengeblieben => Figuren.Count(f => f.Schicksal == ZugabgabeSchicksal.Stehengeblieben);
        public int AnzahlAufgelöst => Figuren.Count(f => f.Schicksal == ZugabgabeSchicksal.Aufgelöst);

        /// <summary>
        /// Eine lesbare Zusammenfassung für den Dialog
        /// </summary>
        public string Zusammenfassung {
            get {
                var sb = new StringBuilder();
                sb.AppendLine($"Zugabgabe von {AktuellerZug} nach {NächsterZug.Beschreibung}");
                sb.AppendLine($"Aktuelle Phase: {Phase}");
                sb.AppendLine();
                sb.AppendLine($"{Figuren.Count} Figuren werden übernommen:");
                sb.AppendLine($"  {AnzahlBewegt} haben sich bewegt");
                sb.AppendLine($"  {AnzahlStehengeblieben} sind stehen geblieben");
                if (AnzahlAufgelöst > 0)
                    sb.AppendLine($"  {AnzahlAufgelöst} verschwinden von der Karte");
                if (Hinweise.Count > 0) {
                    sb.AppendLine();
                    sb.AppendLine("Hinweise:");
                    foreach (var hinweis in Hinweise)
                        sb.AppendLine($"  {hinweis}");
                }
                if (Hindernisse.Count > 0) {
                    sb.AppendLine();
                    sb.AppendLine("Die Abgabe ist nicht möglich:");
                    foreach (var hindernis in Hindernisse)
                        sb.AppendLine($"  {hindernis}");
                }
                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Die Zugabgabe schliesst den Zug eines Reiches ab und bereitet den Folgezug vor.
    ///
    /// Diese Klasse liefert den Trockenlauf: sie sagt, was passieren würde, ohne etwas zu
    /// verändern. Das eigentliche Kopieren der Datenbank gehört in die Anwendungsschicht.
    /// </summary>
    public static class ZugabgabeView {

        /// <summary>
        /// Prüft den aktuellen Stand und beschreibt, was eine Zugabgabe bewirken würde.
        /// Es wird nichts verändert.
        /// </summary>
        public static ZugabgabeBericht ErstelleBericht() {
            var aktuell = ZugView.AktuellerZug;
            var bericht = new ZugabgabeBericht {
                AktuellerZug = aktuell,
                NächsterZug = aktuell.Nächster,
                Phase = ZugView.Phase,
            };

            if (ProgramView.SelectedNation == null) {
                bericht.Hindernisse.Add("Es ist kein Reich ausgewählt.");
                return bericht;
            }
            if (aktuell.IstGültig == false) {
                bericht.Hindernisse.Add("Der aktuelle Zugmonat ist unbekannt.");
                return bericht;
            }
            if (ZugView.Settings == null) {
                bericht.Hindernisse.Add("Die Zugdaten sind nicht geladen.");
                return bericht;
            }
            if (ZugView.Phase == Zugphase.Abgeschlossen)
                bericht.Hindernisse.Add($"Zug {aktuell.Zug} wurde bereits abgegeben.");
            if (ZugView.Phase == Zugphase.Rüstphase)
                bericht.Hinweise.Add("Die Rüstphase ist noch nicht abgeschlossen - es wurde in diesem Zug noch nicht bewegt.");

            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            if (armee.Count == 0)
                bericht.Hindernisse.Add($"Für {ProgramView.SelectedNation.Name} sind keine Figuren geladen.");

            int mitRestpunkten = 0;
            foreach (var figur in armee) {
                var schicksal = ZugendeRules.IstAufgelöst(figur) ? ZugabgabeSchicksal.Aufgelöst
                    : figur.gf_nach > 0 ? ZugabgabeSchicksal.Bewegt
                    : ZugabgabeSchicksal.Stehengeblieben;
                int rest = Math.Max(0, figur.bp);
                if (schicksal == ZugabgabeSchicksal.Bewegt && rest > 0)
                    mitRestpunkten++;
                bericht.Figuren.Add(new ZugabgabeEintrag {
                    Figur = figur,
                    Schicksal = schicksal,
                    UngenutzteBewegungspunkte = rest,
                });
            }

            if (mitRestpunkten > 0)
                bericht.Hinweise.Add($"{mitRestpunkten} bewegte Figuren haben noch Bewegungspunkte übrig - unverbrauchte Punkte verfallen ersatzlos.");

            var aufgelöste = bericht.Figuren.Where(f => f.Schicksal == ZugabgabeSchicksal.Aufgelöst).ToList();
            foreach (var eintrag in aufgelöste)
                bericht.Hinweise.Add($"{eintrag.Figur.Bezeichner} verschwindet von der Karte, weil "
                    + (eintrag.Figur is TruppenSpielfigur ? "kein Heerführer mehr da ist" : "keine Gutpunkte mehr da sind") + ".");

            if (SharedData.Commands.Count > 0)
                bericht.Hinweise.Add($"{SharedData.Commands.Count} Befehle dieses Zuges werden mit abgegeben und lassen sich danach nicht mehr zurücknehmen.");

            return bericht;
        }

        /// <summary>
        /// Wendet die Zugendregeln auf alle Figuren des eigenen Reiches an.
        ///
        /// Die Reihenfolge ist wichtig: erst bleiben alle nicht bewegten Figuren stehen, dann erst
        /// werden alle in den nächsten Zug geschoben.
        /// </summary>
        /// <returns>die Anzahl der übernommenen Figuren</returns>
        public static int SchiebeAlleFigurenInDenNächstenZug() {
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            foreach (var figur in armee)
                ZugendeRules.SetzeAufAusgangsposition(figur);
            foreach (var figur in armee)
                ZugendeRules.SchiebeInNächstenZug(figur);
            return armee.Count;
        }
    }
}
