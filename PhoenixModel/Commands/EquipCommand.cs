using PhoenixModel.Commands.Parser;
using PhoenixModel.Extensions;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;
using static PhoenixModel.Commands.EmbarkCommand;
using static PhoenixModel.Commands.EquipCommand;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Das Rüsten von Rüstgütern als neue Armee und zu einer Armee hinzu oder einfach nur eine Armee mit und ohne Zeug rüsten
    /// </summary>
    public class EquipCommand : BaseCommand, IPhoenixCommand {        
        public struct ConstructionElement {
            public ConstructionElementType ConstructionElementType = ConstructionElementType.None;
            public int Count = 0;

            public ConstructionElement(ConstructionElementType constructionElementType, int count) {
                this.ConstructionElementType = constructionElementType;
                Count = count;
            }
        }

        public ConstructionElementType Target { get; set; } = ConstructionElementType.None;      // "Wand", "Brücke"
        public int? TargetID { get; set; } = null;
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;
        public List<ConstructionElement> Equipment = [];
    
        public override string ToString() {
            string rüste = $"Rüste {Equipment[0].Count} {SimpleParser.ConstructionElementTypeToString(Equipment[0].ConstructionElementType)}";
            if (Equipment.Count > 1) {
                for (int i = 1; i < Equipment.Count; i++)
                    rüste = $"{rüste} und {Equipment[i].Count} {SimpleParser.ConstructionElementTypeToString(Equipment[i].ConstructionElementType)}";
            }
            if (Target != ConstructionElementType.None)
                rüste = $"{rüste} zu {Target} {TargetID}";
           return $"{rüste} in Rüstort {Location}";
        }

        public EquipCommand(string commandString, KleinfeldPosition? pos) : base(commandString) {
            Location = pos;
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable != null && (selectable is Spielfigur || (selectable is KleinFeld kleinfeld && kleinfeld.Gebäude != null));
        }


        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (Equipment.Count == 0)
                return new CommandResultError("Es wurde kein zu rüstendes Element angegeben", $"In dem Befehl konnte das Element (Krieger, Reiter) nicht gefunden werden \r\n {this.CommandString}", this);
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}", this);
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}", this);
            if (SharedData.Ruestung == null)
                return new CommandResultError("Die Ruestung wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Ruestung aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}", this);

            if (Equipment.Any(teil => teil.ConstructionElementType == ConstructionElementType.None))
                return new CommandResultError("Ein Rüstgut wurde nicht erkannt",
                    $"In '{this.CommandString}' steht ein Wort, das kein Rüstgut benennt.", this);
            if (Equipment.Any(teil => teil.Count <= 0))
                return new CommandResultError("Eine Stückzahl fehlt oder ist null",
                    $"In '{this.CommandString}' ist nicht zu jedem Rüstgut eine Anzahl angegeben.", this);

            // die eigentlichen Regeln des Rüstens (Regelwerk 3.3): Ort, Phase, Kapazität, Mittel
            var gemark = KleinfeldView.GetKleinfeld(Location);
            var regeln = Rules.RuestRules.Prüfe(gemark, CreateRuestung());
            if (regeln.HasErrors)
                return new CommandResultError(regeln.Title, regeln.Message, this);

            return new CommandResultSuccess("Die Rüstung kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}", this);
        }

        /// <summary>
        /// Baut aus dem Befehl den Ruestungsauftrag, wie ihn die Zugdatenbank fuehrt.
        ///
        /// Die Tabelle hat je Ruestgut eine Spalte; der Befehl nennt sie als Liste. Frueher stand
        /// hier ein Auftrag aus lauter Nullen - er wurde gespeichert, aber er ruestete nichts.
        ///
        /// Nummer 0 heisst: ein neues Heer, dessen Nummer die Spielleitung vergibt. Wird zu einer
        /// vorhandenen Einheit geruestet, traegt der Auftrag deren Nummer.
        /// </summary>
        private Ruestung? CreateRuestung() {
            if (Location == null)
                return null;

            var rüstung = new Ruestung() {
                gf = Location.gf,
                kf = Location.kf,
                ZugMonat = ProgramView.SelectedMonth,
                Nummer = TargetID ?? 0,
                Name_x = string.Empty,
                Beschriftung = string.Empty,
            };

            foreach (var teil in Equipment) {
                if (teil.Count <= 0)
                    continue;
                switch (teil.ConstructionElementType) {
                    case ConstructionElementType.K: rüstung.K += teil.Count; break;
                    case ConstructionElementType.R: rüstung.R += teil.Count; break;
                    case ConstructionElementType.P: rüstung.P += teil.Count; break;
                    case ConstructionElementType.S: rüstung.S += teil.Count; break;
                    case ConstructionElementType.LKP: rüstung.LKP += teil.Count; break;
                    case ConstructionElementType.SKP: rüstung.SKP += teil.Count; break;
                    case ConstructionElementType.LKS: rüstung.LKS += teil.Count; break;
                    case ConstructionElementType.SKS: rüstung.SKS += teil.Count; break;
                    case ConstructionElementType.HF: rüstung.HF += teil.Count; break;
                    case ConstructionElementType.ZA: rüstung.Z += teil.Count; break;
                    case ConstructionElementType.ZB: rüstung.ZB += teil.Count; break;
                    default:
                        // Bauwerke werden nicht hier geruestet, sondern mit dem ConstructCommand
                        return null;
                }
            }
            return rüstung;
        }

        /// <summary>Der eingestellte Auftrag, damit sich das Ruesten zuruecknehmen laesst</summary>
        private Ruestung? _eingestellt = null;

        /// <summary>
        /// Versucht den Befehl rückgäng zu machen
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch gelöscht
        /// </summary>
        public override CommandResult UndoCommand() {
            if (_eingestellt == null || SharedData.Ruestung == null)
                return new CommandResultError("Diese Rüstung wurde nie eingestellt",
                    $"Der Befehl lässt sich nicht zurücknehmen, weil er nichts in die Rüstungstabelle "
                    + $"geschrieben hat:\r\n{this.CommandString}", this);

            // Aus der geteilten Liste nehmen und den Auftrag aus der Datenbank loeschen. Die
            // Sammlung ist nach dem Laden geschlossen und muss dafuer wieder geoeffnet werden.
            var verbleibend = SharedData.Ruestung.ReopenSharedData()
                .Where(eintrag => ReferenceEquals(eintrag, _eingestellt) == false)
                .ToList();
            SharedData.Ruestung = [];
            foreach (var eintrag in verbleibend)
                SharedData.Ruestung.Add(eintrag);

            SharedData.StoreQueue.Delete(_eingestellt);
            var zurückgenommen = _eingestellt;
            _eingestellt = null;
            return new CommandResultSuccess("Die Rüstung wurde zurückgenommen",
                $"Der Auftrag auf {zurückgenommen.gf}/{zurückgenommen.kf} wurde entfernt:\r\n{this.CommandString}", this);
        }

        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück. 
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch geschrieben
        /// </summary>
        public override CommandResult ExecuteCommand() {
            Ruestung? ruest = CreateRuestung();
            if (ruest != null && SharedData.Ruestung != null) {

                // die Sammlung wird nach dem Laden fuer Ergaenzungen geschlossen und muss vor dem
                // Hinzufuegen wieder geoeffnet werden - sonst wirft Add eine Ausnahme
                SharedData.Ruestung.ReopenSharedData().Add(ruest);
                SharedData.StoreQueue.Insert(ruest);
                _eingestellt = ruest;
                return new CommandResultSuccess("Die Rüstung wurde ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum", this);
        }
    }

    /// <summary>
    /// Der Parser zu dem Befehl des Rüstens
    /// - Rüste 6 Leichte Katapulte zu Krieger 115 in Rüstort 506/17
    /// - Rüste 3 Heeführer zu Krieger 115 in Rüstort 506/17
    /// - Rüste 1000 Krieger und 3 Heerführer und 2 Leichten Katapulten in Rüstort 506/17
    /// - Rüste 1000 Krieger und 200 Pferde und 1 Heerführer in Rüstort 506/17
    /// - Rüste 300 Reiter und 7 Heerführer in Rüstort 506/17
    /// - Rüste 500 Krieger und 10 Pferde und 3 Heerführern und 2 Schwere Katapulte in Rüstort 123/45

    /// </summary>
    public class EquipCommandParser : SimpleParser {
       
        private static readonly Regex EquipExistingRegex = new Regex(
                @"^Rüste\s+(?<gear>[\w\s]+)\s+in\s+Rüstort\s+(?<loc>[^\s]+)\s+zu\s+(?<unitType>\w+)\s+(?<unitId>\d+)$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex CreateRegex = new Regex(
           // Explanation:
           //   Rüste <strength> <unitType> mit <gear> in Rüstort <loc>
           //   where <gear> can be anything up to "in Rüstort", including
           //   multiple "und" parts, e.g. "3 Heerführern und 2 Leichten Katapulten"
           @"^Rüste\s+(?<gear>.*?)\s+in\s+Rüstort\s+(?<loc>[^\s]+)$",
           RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {

            var match = EquipExistingRegex.Match(commandString);
            if (!match.Success)
                match = CreateRegex.Match(commandString);
            if (!match.Success) 
                    return Fail(out command);
            try {
                List<ConstructionElement> eq = [];
           
                // e.g. "3 Heerführern und 2 Leichten Katapulten"
                string gearText = match.Groups["gear"].Value.Trim();
                
                // 3) Split on " und "
                var gearParts = gearText.Split(new[] { " und " }, StringSplitOptions.RemoveEmptyEntries);
                // gearParts[0] = "3 Heerführern"
                // gearParts[1] = "2 Leichten Katapulten"   (if it exists)                

                foreach (var part in gearParts) {
                    // Jeder Teil hat die Form "<Anzahl> <Rüstgut>", also "3 Heerführern" oder
                    // "2 Leichten Katapulten".
                    //
                    // Hier stand frueher ein Zugriff auf die Gruppen "equipment" und "strength" -
                    // die gibt es in keinem der beiden Ausdruecke. Jedes Rüstgut wurde damit zu
                    // (None, 0): der Befehl liess sich lesen und ausführen und rüstete nichts.
                    var itemMatch = Regex.Match(part.Trim(), @"^(?<count>\d+)\s+(?<desc>.+)$");
                    if (itemMatch.Success == false)
                        return Fail(out command);

                    int count = int.Parse(itemMatch.Groups["count"].Value);
                    string desc = itemMatch.Groups["desc"].Value.Trim();
                    eq.Add(new ConstructionElement(parseConstructionElement(desc), count));
                }
                if (eq.Count == 0)
                    return Fail(out command);

                var ziel = ConstructionElementType.None;
                int? zielId = null;
                if (match.Groups["unitType"].Success) {
                    ziel = parseConstructionElement(match.Groups["unitType"].Value);
                    zielId = ParseInt(match.Groups["unitId"].Value);
                }

                command = new EquipCommand(commandString, ParseLocation(match.Groups["loc"].Value)) {
                    Equipment = eq,
                    Location = ParseLocation(match.Groups["loc"].Value),
                    Target = ziel,
                    TargetID = zielId,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des EquipCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
