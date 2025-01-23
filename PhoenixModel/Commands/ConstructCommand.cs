using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// erzeugt ein kleines Bauwerk, das eine Richtung braucht, wie eine Wand, Strasse oder eine Brücke
    ///    - "Errichte Wall im Nordosten von 202/33"
    ///    - "Errichte Brücke im Süden von 444/22"
    ///    - "Errichte Brücke im Norden von 47/11"
    /// </summary>
    public class ConstructCommand : SimpleCommand, ICommand, IEquatable<ConstructCommand> {
       
        public ConstructionElementType What { get; set; } = ConstructionElementType.None;      // "Wand", "Brücke"
        public Direction? Direction { get; set; } = null; // "Nordosten", "Süden", "Norden", etc.
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;

        public ConstructCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            return (Direction != null)? $"Errichte {SimpleParser.ConstructionElementTypeToString(What)} im {Direction} von {Location}" : $"Errichte {SimpleParser.ConstructionElementTypeToString(What)} auf {Location}"; ;
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (What == ConstructionElementType.None)
                return new CommandResultError("Es wurde kein zu errichtendes Bauwerk angegeben", $"In dem Befehl konnte das Bauwerk (Straße, Brücke, Wall) nicht gefunden werden \r\n {this.CommandString}",this);
            if (Direction == null)
                return new CommandResultError("Es wurde keine Richtung angegeben", $"In dem Befehl konnte die Richtung (Nordosten, Westen, etc) nicht gefunden werden \r\n {this.CommandString}", this);
            if (Location == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}", this);
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}", this);
            if (SharedData.RuestungBauwerke == null)
                return new CommandResultError("Die RuestungBauwerke wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die RuestungBauwerke aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}", this);

            var kosten = SharedData.Kosten.Where(kosten => kosten.Unittyp == What.ToString()).First();
            if (kosten == null)
                return new CommandResultError($"Die Kostentablle enthält keinen Wert für {What}", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle im Feld Unittyp das genannte Bauwerk nicht kennen \r\n {this.CommandString}", this);

            return new CommandResultSuccess("Das ConstructCommand kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}", this);
        }

        private RuestungBauwerke? CreateRuestungBauwerke() {
            if (Kosten != null && Location != null) {
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
                    return new CommandResultError("Der Auftrag für dieses Bauwerk existiert nicht und kann daher nicht rückgänig gemacht werden", $"Der Befehl kann nicht rückgängig gemacht werden, da er nicht in den Zugdaten gespeichert wurde\r\n {this.CommandString}", this);
                SharedData.RuestungBauwerke.Remove(existing);
                SharedData.StoreQueue.Delete(existing);
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum", this);
        }

        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück. 
        /// Wenn in der Datenbank etwas geschrieben werden musste, wird es auch geschrieben
        /// </summary>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if ( result.HasErrors ) 
                return result;
            RuestungBauwerke? bauwerk = CreateRuestungBauwerke();
            if (bauwerk != null && SharedData.RuestungBauwerke != null) {
                SharedData.RuestungBauwerke.Add(bauwerk);
                SharedData.StoreQueue.Insert(bauwerk);
                return new CommandResultSuccess("Das ConstructCommand wurde ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);
            }

            return new CommandResultError("Fehler", "Keine Ahnung warum", this);
        }

        public bool Equals(ConstructCommand? other) {
            if (other == null)
                return false;
            if (What != other.What) 
                return false;
            if (Direction != other.Direction) 
                return false;
            if ((Location == null && other.Location == null) == false) {
                if (Location == null || other.Location == null)
                    return false;
                if (Location.Equals(other.Location) == false)
                    return false;
            }
            if ((Kosten == null && other.Kosten == null) == false) {
                if (Kosten == null || other.Kosten == null)
                    return false;
                if (Kosten.Equals(other.Kosten) == false)
                    return false;
            }
            return true;

        }

        /// <summary>
        /// Überprüft die Gleichheit mit einem anderen Objekt.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Objekt.</param>
        /// <returns>True, wenn die Objekte gleich sind, sonst false.</returns>
        public override bool Equals(object? obj) {
            return Equals(obj as ConstructCommand);
        }

        public override int GetHashCode() {
            return HashCode.Combine(What, Direction, Location, CommandString);
        }
    }

    public class ConstructCommandParser : SimpleParser {

        private static readonly Regex ConstructRegexErrichte = new Regex(
            @"^Errichte\s+(?<what>\w+)\s+im\s+(?<direction>\w+)\s+von\s+(?<loc>[^\s]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
           );

        private static readonly Regex ConstructRegexBaue = new Regex(
             @"^Errichte\s+(?<what>\w+)\s+auf\s+(?<loc>[^\s]+)$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled
            );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = ConstructRegexErrichte.Match(commandString);
            if (!match.Success) {
                match = ConstructRegexBaue.Match(commandString);
                if (!match.Success)
                    return Fail(out command);
            }
            try {
                var bauwerk = parseConstructionElement(match.Groups["what"].Value);

                command = new ConstructCommand(commandString) {
                    Location = ParseLocation(match.Groups["loc"].Value),
                    What = bauwerk,
                    Direction = ParseDirection(match.Groups["direction"].Value),
                    Kosten = KostenView.GetKosten(bauwerk.ToString()),
                };
            }
            catch (Exception ex) {
                Program.ProgramView.LogError("Beim Lesen des ConstructCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }

        public static string GenerateCommand(KleinfeldPosition pos, ConstructionElementType what, Direction direction) {
            return $"Errichte {what} im {(DirectionNames)direction} von {pos.CreateBezeichner()}";
        }

    }
}
