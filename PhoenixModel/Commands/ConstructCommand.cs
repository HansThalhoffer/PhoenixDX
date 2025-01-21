using PhoenixModel.Commands.Parser;
using PhoenixModel.ViewModel;
using PhoenixModel.dbZugdaten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PhoenixModel.dbCrossRef;

namespace PhoenixModel.Commands {

    /// <summary>
    /// erzeugt ein kleines Bauwerk, das eine Richtung braucht, wie eine Wand, Strasse oder eine Brücke
    ///    - "Errichte Wall im Nordosten von 202/33"
    ///    - "Errichte Brücke im Süden von 444/22"
    ///    - "Errichte Brücke im Norden von 47/11"
    /// </summary>
    public class ConstructCommand: DefaultCommand, ICommandParser, ICommand {
        public enum ConstructionElement {
            None,Bruecke,Strasse,Wall
        }       
        
        public ConstructionElement What { get; set; } = ConstructionElement.None;      // "Wand", "Brücke"
        public Direction? Direction { get; set; } = null; // "Nordosten", "Süden", "Norden", etc.
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;

        private static readonly Regex ConstructRegexErrichte = new Regex(
             @"^Errichte\s+(?<what>\w+)\s+im\s+(?<direction>\w+)\s+von\s+(?<loc>[^\s]+)$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
            );
        
        private static readonly Regex ConstructRegexBaue = new Regex(
             @"^Baue\s+(?<what>\w+)\s+auf\s+(?<loc>[^\s]+)\s+eine\s+(?<direction>\w+)$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
            );

        public ConstructCommand(string commandString, KleinfeldPosition? pos) :base(commandString) {
            Location = pos;
        }

        private ConstructionElement parseConstructionElement(string input) {
            return input.ToLower()
            switch {
                "wall" => ConstructionElement.Wall,
                "strasse" => ConstructionElement.Strasse,
                "straße" => ConstructionElement.Strasse,
                "brücke" => ConstructionElement.Bruecke,
                _ => ConstructionElement.None
            };
        }

        public bool ParseCommand(string commandString, out ICommand? command) {
            var match = ConstructRegexErrichte.Match(commandString);
            if (!match.Success) {
                match = ConstructRegexBaue.Match(commandString);
                if (!match.Success) 
                    return Fail(out command);
            }
            command = new ConstructCommand(commandString, CommandParser.ParseLocation(match.Groups["loc"].Value)) {
                What = parseConstructionElement(match.Groups["what"].Value),
                Direction = CommandParser.ParseDirection(match.Groups["direction"].Value),
            };
            return true;        
        }

        public static string GenerateCommand(KleinfeldPosition pos, ConstructionElement what, Direction direction) {
            return $"Errichte {what} im {(DirectionNames)direction} von {pos.CreateBezeichner()}";
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (What == ConstructionElement.None)
                return new CommandResultError("Es wurde kein zu errichtendes Bauwerk angegeben", $"In dem Befehl konnte das Bauwerk (Straße, Brücke, Wall) nicht gefunden werden \r\n {this.CommandString}");
            if (Direction == null)
                return new CommandResultError("Es wurde keine Richtung angegeben", $"In dem Befehl konnte die Richtung (Nordosten, Westen, etc) nicht gefunden werden \r\n {this.CommandString}");
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}");
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}");
            if (SharedData.RuestungBauwerke == null)
                return new CommandResultError("Die RuestungBauwerke wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die RuestungBauwerke aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}");

            var kosten = SharedData.Kosten.Where(kosten => kosten.Unittyp == What.ToString()).First();
            if (kosten == null)
                return new CommandResultError($"Die Kostentablle enthält keinen Wert für {What}", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle im Feld Unittyp das genannte Bauwerk nicht kennen \r\n {this.CommandString}");

            return new CommandResultSuccess("Das ConstructCommand kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}");
        }

        private RuestungBauwerke? CreateRuestungBauwerke() {
            if (Kosten != null && Location != null && CheckPreconditions() == true) {
                return new RuestungBauwerke() {
                    GF = Location.gf,
                    KF = Location.kf,
                    Art = $"{What.ToString()}_{Direction.ToString()}",
                    BP_rep = 0,
                    BP_neu = Kosten.BauPunkte,
                    Kosten = Kosten.GS,
                };
            }
            return null;
        }

        /// <summary>
        /// Versucht den Befehl rückgäng zu machen
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch gelöscht
        /// </summary>
        public override CommandResult UndoCommand() {
            RuestungBauwerke? bauwerk = CreateRuestungBauwerke();
            if (bauwerk != null && SharedData.RuestungBauwerke != null) { 
                var existing = SharedData.RuestungBauwerke.Where(bw => bw.Equals(bauwerk)).First();
                if (existing == null)
                    return new CommandResultError("Der Auftrag für dieses Bauwerk existiert nicht und kann daher nicht rückgänig gemacht werden", $"Der Befehl kann nicht rückgängig gemacht werden, da er nicht in den Zugdaten gespeichert wurde\r\n {this.CommandString}");
                SharedData.RuestungBauwerke.Remove(existing);
                SharedData.StoreQueue.Delete(existing);
            }

            return new CommandResultError("Fehler","Keine Ahnung warum");
        }

        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück. 
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch geschrieben
        /// </summary>
        public override CommandResult ExecuteCommand() {
            RuestungBauwerke? bauwerk = CreateRuestungBauwerke();
            if (bauwerk != null && SharedData.RuestungBauwerke != null) {
                SharedData.RuestungBauwerke.Add(bauwerk);
                SharedData.StoreQueue.Insert(bauwerk);
                return new CommandResultSuccess("Das ConstructCommand wurde ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}");
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum");
        }

    }
}
