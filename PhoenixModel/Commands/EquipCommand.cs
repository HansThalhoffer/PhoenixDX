using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;
using static PhoenixModel.Commands.EquipCommand;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Das Rüsten von Rüstgütern als neue Armee und zu einer Armee hinzu oder einfach nur eine Armee mit und ohne Zeug rüsten
    /// - Rüste Krieger 115 in Rüstort 506/17 mit der Stärke 1000 und 3 Heerführern
    /// - Rüste 6 Leichte Katapulte zu den Kriegern 115 in Rüstort 506/17
    /// - Rüste 3 Heeführer zu den Kriegern 115 in Rüstort 506/17
    /// - Rüste 1000 Krieger mit 3 Heerführern und 2 Leichten Katapulten in Rüstort 506/17
    /// - Rüste 1000 Krieger mit 200 Pferden und 1 Heerführer in Rüstort 506/17
    /// - Rüste 300 Reiter mit 7 Heerführern in Rüstort 506/17
    /// - Rüste 500 Krieger mit 10 Pferden und 3 Heerführern und 2 Schweren Katapulten in Rüstort 123/45
    /// </summary>
    public class EquipCommand : SimpleCommand, ICommand {

        /// <summary>
        /// die Namen entsprechen der Kostentabelle in crossref.mdb
        /// </summary>
        public enum ConstructionElementType {
            None,
            K, // Krieger
            S, // Schiffe
            R, // Reiter
            P, // PFerde
            LKP,// Leichte Katapulte
            SKP,// Schwere Katapulte
            LKS,// Leichte Kriegsschiffe 
            SKS,// Schwere Kriegsschiffe
            HF, // HF 
            ZA, // Zauberer Klasse A
            ZB, // Zauberer Klasse A
        }

        public struct ConstructionElement {
            ConstructionElementType constructionElementType = ConstructionElementType.None;
            public int Count = 0;

            public ConstructionElement(ConstructionElementType constructionElementType, int count) {
                this.constructionElementType = constructionElementType;
                Count = count;
            }
        }

        public ConstructionElementType Target { get; set; } = ConstructionElementType.None;      // "Wand", "Brücke"
        public int? TargetID { get; set; } = null;
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;
        public List<ConstructionElement> Equipment = [];
        public int? Heerführer { get; set; }

        public EquipCommand(string commandString, KleinfeldPosition? pos) : base(commandString) {
            Location = pos;
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (Target == ConstructionElementType.None)
                return new CommandResultError("Es wurde kein zu rüstendes Element angegeben", $"In dem Befehl konnte das Element (Krieger, Reiter) nicht gefunden werden \r\n {this.CommandString}");
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}");
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}");
            if (SharedData.Ruestung == null)
                return new CommandResultError("Die Ruestung wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Ruestung aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}");

            /*var kosten = SharedData.Kosten.Where(kosten => kosten.Unittyp == What.ToString()).First();
            if (kosten == null)
                return new CommandResultError($"Die Kostentablle enthält keinen Wert für {What}", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle im Feld Unittyp das genannte Bauwerk nicht kennen \r\n {this.CommandString}");
            */
            return new CommandResultSuccess("Das ConstructCommand kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}");
        }

        private Ruestung? CreateRuestung() {
            if (Kosten != null && Location != null && CheckPreconditions() == true) {
                return new Ruestung() {
                    Nummer = 0,
                    HF = Heerführer != null ? (int)Heerführer : 0,
                    Z = 0,
                    K = 0,
                    R = 0,
                    P = 0,
                    LKS = 0,
                    SKS = 0,
                    LKP = 0,
                    SKP = 0,
                    GP_akt = 0,
                    GP_ges = 0,
                    Garde = 0,
                    ZB = 0,
                    S = 0,
                    Neuruestung = 0,
                    KF_Flotte = 0,
                    GF_Flotte = 0,
                    Name_x = string.Empty,
                    Beschriftung = string.Empty,
                    besRuestung = 0
                };
            }
            return null;
        }


        /// <summary>
        /// Versucht den Befehl rückgäng zu machen
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch gelöscht
        /// </summary>
        public override CommandResult UndoCommand() {
            Ruestung? ruest = CreateRuestung();
            if (ruest != null && SharedData.Ruestung != null) {
                var existing = SharedData.Ruestung.Where(r => r.Equals(ruest)).First();
                if (existing == null)
                    return new CommandResultError("Der Auftrag für diese Rüstung existiert nicht und kann daher nicht rückgänig gemacht werden", $"Der Befehl kann nicht rückgängig gemacht werden, da er nicht in den Zugdaten gespeichert wurde\r\n {this.CommandString}");
                SharedData.Ruestung.Remove(existing);
                SharedData.StoreQueue.Delete(existing);
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum");
        }


        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück. 
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch geschrieben
        /// </summary>
        public override CommandResult ExecuteCommand() {
            Ruestung? ruest = CreateRuestung();
            if (ruest != null && SharedData.Ruestung != null) {

                SharedData.Ruestung.Add(ruest);
                SharedData.StoreQueue.Insert(ruest);
                return new CommandResultSuccess("Die Rüstung wurde ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}");
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum");
        }
    }

    /// <summary>
    /// Der Parser zu dem Befehl des Dazu-Rüstens
    /// </summary>
    public class EquipCommandParser : SimpleParser {
       

        private ConstructionElementType parseConstructionElement(string input) {
            return input.ToLower()
            switch {
                "k" => ConstructionElementType.K,
                "krieger" => ConstructionElementType.K,
                "kriegern" => ConstructionElementType.K,
                "r" => ConstructionElementType.R,
                "reiter" => ConstructionElementType.R,
                "reitern" => ConstructionElementType.R,
                "s" => ConstructionElementType.S,
                "schiff" => ConstructionElementType.S,
                "schiffe" => ConstructionElementType.S,
                "schiffen" => ConstructionElementType.S,
                "pferde" => ConstructionElementType.P,
                "lkp" => ConstructionElementType.LKP,
                "leichte katapulte" => ConstructionElementType.LKP,
                "leichte kp" => ConstructionElementType.LKP,
                "skp" => ConstructionElementType.SKP,
                "schwere katapulte" => ConstructionElementType.SKP,
                "schwere kp" => ConstructionElementType.SKP,
                "lks" => ConstructionElementType.LKS,
                "leichte kriegsschiffe" => ConstructionElementType.LKS,
                "leichte ks" => ConstructionElementType.LKS,
                "sks" => ConstructionElementType.SKS,
                "schwere kriegsschiffe" => ConstructionElementType.SKS,
                "schwere ks" => ConstructionElementType.SKS,
                "heerführer" => ConstructionElementType.HF,
                "hf" => ConstructionElementType.HF,
                "za" => ConstructionElementType.ZA,
                "zauberer klasse a" => ConstructionElementType.ZA,
                "zb" => ConstructionElementType.ZB,
                "zauberer klasse b" => ConstructionElementType.ZB,
                _ => ConstructionElementType.None
            };
        }

        private static readonly Regex EquipSiegeRegex = new Regex(
                @"^Rüste\s+(?<strength>\d+)\s+(?<equipment>[\w\s]+)\s+zu\s+den\s+(?<unitType>\w+)\s+(?<unitId>\d+)\s+in\s+Rüstort\s+(?<loc>[^\s]+)$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private bool ParseEquipCommand(string commandString, out ICommand? command) {
            var match = EquipSiegeRegex.Match(commandString);
                if (!match.Success)
                    return Fail(out command);
        
            try {
                command = new EquipCommand(commandString, ParseLocation(match.Groups["loc"].Value)) {
                    Target = parseConstructionElement(match.Groups["unitType"].Value),
                    TargetID = ParseInt(match.Groups["unitId"].Value),
                    Heerführer = ParseInt(match.Groups["hf"].Value),
                    Location = ParseLocation(match.Groups["loc"].Value),

                };
                if (command is EquipCommand eq && match.Groups.ContainsKey("equipment")) {
                    eq.Equipment.Add( new ConstructionElement(parseConstructionElement(match.Groups["equipment"].Value), ParseInt(match.Groups["strength"].Value)));
                }
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des EquipCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }

        private static readonly Regex FlexibleEquipRegex = new Regex(
           // Explanation:
           //   Rüste <strength> <unitType> mit <gear> in Rüstort <loc>
           //   where <gear> can be anything up to "in Rüstort", including
           //   multiple "und" parts, e.g. "3 Heerführern und 2 Leichten Katapulten"
           @"^Rüste\s+(?<strength>\d+)\s+(?<unitType>\w+)\s+mit\s+(?<gear>.*?)\s+in\s+Rüstort\s+(?<loc>[^\s]+)$",
           RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            if (ParseEquipCommand(commandString, out command))
                return true;

            var match = FlexibleEquipRegex.Match(commandString);
            if (!match.Success) 
                    return Fail(out command);
            try {
                List<ConstructionElement> eq = [];
                eq.Add(new ConstructionElement(parseConstructionElement(match.Groups["unitType"].Value), ParseInt(match.Groups["strength"].Value)));

                // e.g. "3 Heerführern und 2 Leichten Katapulten"
                string gearText = match.Groups["gear"].Value.Trim();
                
                // 3) Split on " und "
                var gearParts = gearText.Split(new[] { " und " }, StringSplitOptions.RemoveEmptyEntries);
                // gearParts[0] = "3 Heerführern"
                // gearParts[1] = "2 Leichten Katapulten"   (if it exists)                

                foreach (var part in gearParts) {
                    // parse each item as: "<count> <whatever>"
                    // e.g. "3 Heerführern", "2 Leichten Katapulten", etc.
                    // A simple pattern: ^(?<count>\d+)\s+(?<desc>.*)$
                    // to get the numeric part and the textual part:
                    var itemMatch = Regex.Match(part.Trim(), @"^(?<count>\d+)\s+(?<desc>.+)$");
                    if (itemMatch.Success) {
                        int count = int.Parse(itemMatch.Groups["count"].Value);
                        string desc = itemMatch.Groups["desc"].Value.Trim(); // "Heerführern", "Leichten Katapulten" etc.

                        eq.Add(new ConstructionElement(parseConstructionElement(match.Groups["equipment"].Value), ParseInt(match.Groups["strength"].Value)));
                    }
                    else {
                        // If it doesn't match e.g. "<count> <desc>", handle error or log
                    }
                }

                command = new EquipCommand(commandString, ParseLocation(match.Groups["loc"].Value)) {
                    Equipment = eq,
                    Heerführer = ParseInt(match.Groups["hf"].Value),
                    Location = ParseLocation(match.Groups["loc"].Value),
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
