using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PhoenixModel.Commands.EquipCommand;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Das Rüsten von Rüstgütern als neue Armee und zu einer Armee hinzu
    /// - Rüste Krieger 115 in Rüstort 506/17 mit der Stärke 1000 und 3 Heerführern
    /// - Rüste 6 Leichte Katapulte zu den Kriegern 115 in Rüstort 506/17
    /// - Rüste 3 Heeführer zu den Kriegern 115 in Rüstort 506/17
    /// </summary>
    public class EquipCommand : DefaultCommand, ICommand {

        /// <summary>
        /// die Namen entsprechen der Kostentabelle in crossref.mdb
        /// </summary>
        public enum ConstructionElement {
            None,
            K, // Krieger
            S, // Schiffe
            R, // Reiter
            LKP,// Leichte Katapulte
            SKP,// Schwere Katapulte
            LKS,// Leichte Kriegsschiffe 
            SKS,// Schwere Kriegsschiffe
            HF, // HF 
            ZA, // Zauberer Klasse A
            ZB, // Zauberer Klasse A
        }

        public ConstructionElement What { get; set; } = ConstructionElement.None;      // "Wand", "Brücke"
        public KleinfeldPosition? Location { get; set; } = null;
        public int? Nummer { get; set; } = null;
        public Kosten? Kosten = null;
        public int Stärke { get; set; } = 0;
        public ConstructionElement? Equipment { get; set; }
        public int? Heerführer { get; set; }

        public EquipCommand(string commandString, KleinfeldPosition? pos) : base(commandString) {
            Location = pos;
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (What == ConstructionElement.None)
                return new CommandResultError("Es wurde kein zu rüstendes Element angegeben", $"In dem Befehl konnte das Element (Krieger, Reiter, Heerführer, Leichte Katapulte etc.) nicht gefunden werden \r\n {this.CommandString}");
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}");
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}");
            if (SharedData.Ruestung == null)
                return new CommandResultError("Die Ruestung wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Ruestung aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}");

            var kosten = SharedData.Kosten.Where(kosten => kosten.Unittyp == What.ToString()).First();
            if (kosten == null)
                return new CommandResultError($"Die Kostentablle enthält keinen Wert für {What}", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle im Feld Unittyp das genannte Bauwerk nicht kennen \r\n {this.CommandString}");

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

    public class EquipCommandParser :ICommandParser {
        private static readonly Regex EquipArmyRegex = new Regex(
             @"^Rüste\s+(?<equipment>\w+)\s+(?<unitId>\d+)\s+in\s+Rüstort\s+(?<loc>[^\s]+)\s+mit\s+der\s+Stärke\s+(?<strength>\d+)\s+und\s+(?<hf>\d+)\s+Heerführern$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex EquipSiegeRegex = new Regex(
                @"^Rüste\s+(?<strength>\d+)\s+(?<equipment>[\w\s]+)\s+zu\s+den\s+(?<unitType>\w+)\s+(?<unitId>\d+)\s+in\s+Rüstort\s+(?<loc>[^\s]+)$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }

        private ConstructionElement parseConstructionElement(string input) {
            return input.ToLower()
            switch {
                "k" => ConstructionElement.K,
                "krieger" => ConstructionElement.K,
                "r" => ConstructionElement.R,
                "reiter" => ConstructionElement.R,
                "s" => ConstructionElement.S,
                "schiff" => ConstructionElement.S,
                "schiffe" => ConstructionElement.S,
                "lkp" => ConstructionElement.LKP,
                "leichte katapulte" => ConstructionElement.LKP,
                "leichte kp" => ConstructionElement.LKP,
                "skp" => ConstructionElement.SKP,
                "schwere katapulte" => ConstructionElement.SKP,
                "schwere kp" => ConstructionElement.SKP,
                "lks" => ConstructionElement.LKS,
                "leichte kriegsschiffe" => ConstructionElement.LKS,
                "leichte ks" => ConstructionElement.LKS,
                "sks" => ConstructionElement.SKS,
                "schwere kriegsschiffe" => ConstructionElement.SKS,
                "schwere ks" => ConstructionElement.SKS,
                "heerführer" => ConstructionElement.HF,
                "hf" => ConstructionElement.HF,
                "za" => ConstructionElement.ZA,
                "zauberer klasse a" => ConstructionElement.ZA,
                "zb" => ConstructionElement.ZB,
                "zauberer klasse b" => ConstructionElement.ZB,
                _ => ConstructionElement.None
            };
        }

        public bool ParseCommand(string commandString, out ICommand? command) {
            var match = EquipArmyRegex.Match(commandString);
            if (!match.Success) {
                match = EquipSiegeRegex.Match(commandString);
                if (!match.Success)
                    return Fail(out command);
            }

            int? nummer = null;
            try { nummer = int.Parse(match.Groups["unitId"].Value); } catch { };
            int? hf = null;
            try { hf = int.Parse(match.Groups["hf"].Value); } catch { };
            command = new EquipCommand(commandString, CommandParser.ParseLocation(match.Groups["loc"].Value)) {
                What = parseConstructionElement(match.Groups["unitType"].Value),
                Nummer = nummer,
                Equipment = match.Groups.ContainsKey("equipment") ? parseConstructionElement(match.Groups["equipment"].Value) : null,
                Heerführer = hf,
            };
            return true;
        }
    }
}
