using PhoenixModel.Commands.Parser;
using PhoenixModel.ViewModel;
using PhoenixModel.dbZugdaten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        /// Führt den Standardbefehl aus und gibt eine Fehlermeldung zurück.
        /// </summary>
        public override CommandResult ExecuteCommand() {
            if (What == ConstructionElement.None) 
                return new CommandResultError("Es wurde kein zu errichtendes Bauwerk angegeben", $"In dem Befehl konnte das Bauwerk (Straße, Brücke, Wall) nicht gefunden werden \r\n {this.CommandString}");
            if (Direction == null)
                return new CommandResultError("Es wurde keine Richtung angegeben", $"In dem Befehl konnte die Richtung (Nordosten, Westen, etc) nicht gefunden werden \r\n {this.CommandString}");
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}");

            RuestungBauwerke bauwerk = new RuestungBauwerke() {
                GF = Location.gf,
                KF = Location.kf,
                Art = What.ToString(),
            };



            return new CommandResultError("Fehler","Keine Ahnung warum");
        }

    }
}
