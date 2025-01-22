using PhoenixModel.Commands.Parser;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Repräsentiert einen Befehl zum Bewegen einer Figur.
    /// Bewege Reiter 220 [von 503/22] nach 504/07 [via 503/21, 503/16, 503/4]
    /// </summary>
    public class MoveCommand : SimpleCommand, IEquatable<MoveCommand> {
        /// <summary>
        /// Die Art der Figur, die bewegt wird.
        /// </summary>
        public FigurType Figur { get; set; }

        /// <summary>
        /// Die ID der Einheit.
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// Die Startposition der Bewegung.
        /// </summary>
        public KleinfeldPosition? FromLocation { get; set; }

        /// <summary>
        /// Die Zielposition der Bewegung.
        /// </summary>
        public KleinfeldPosition? ToLocation { get; set; }

        /// <summary>
        /// Zwischenstationen der Bewegung.
        /// </summary>
        public List<KleinfeldPosition>? ViaLocations { get; set; } = null;

        /// <summary>
        /// Erstellt eine neue Instanz des MoveCommand mit einem gegebenen Befehlsstring.
        /// </summary>
        /// <param name="commandString">Der Befehlsstring.</param>
        public MoveCommand(string commandString) : base(commandString) { }

        /// <summary>
        /// Überprüft die Vorbedingungen für den Befehl.
        /// </summary>
        /// <returns>Das Ergebnis der Vorbedingungsprüfung.</returns>
        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Führt den Befehl aus.
        /// </summary>
        /// <returns>Das Ergebnis der Befehlsausführung.</returns>
        public override CommandResult ExecuteCommand() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Macht den Befehl rückgängig.
        /// </summary>
        /// <returns>Das Ergebnis des Rückgängigmachens.</returns>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Überprüft, ob zwei MoveCommand-Instanzen gleich sind.
        /// </summary>
        /// <param name="other">Das zu vergleichende MoveCommand-Objekt.</param>
        /// <returns>True, wenn beide Objekte gleich sind, sonst false.</returns>
        // Implementation of IEquatable<MoveCommand>
        public bool Equals(MoveCommand? other) {
            if (other == null)
                return false;

            if (Figur != other.Figur)
                return false;

            if (UnitId != other.UnitId)
                return false;

            if (!Nullable.Equals(FromLocation, other.FromLocation))
                return false;

            if (!Nullable.Equals(ToLocation, other.ToLocation))
                return false;

            if (ViaLocations == null && other.ViaLocations == null)
                return true;

            if (ViaLocations == null || other.ViaLocations == null)
                return false;

            if (!ViaLocations.SequenceEqual(other.ViaLocations))
                return false;

            return true;
        }

        /// <summary>
        /// Überprüft die Gleichheit mit einem anderen Objekt.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Objekt.</param>
        /// <returns>True, wenn die Objekte gleich sind, sonst false.</returns>
        public override bool Equals(object? obj) {
            return Equals(obj as MoveCommand);
        }

        /// <summary>
        /// Gibt den Hashcode für das Objekt zurück.
        /// </summary>
        /// <returns>Der Hashcode.</returns>
        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + Figur.GetHashCode();
            hash = hash * 31 + UnitId.GetHashCode();
            hash = hash * 31 + (FromLocation?.GetHashCode() ?? 0);
            hash = hash * 31 + (ToLocation?.GetHashCode() ?? 0);
            hash = hash * 31 + (ViaLocations != null ? ViaLocations.Aggregate(0, (acc, item) => acc ^ item.GetHashCode()) : 0);
            return hash;
        }       
    }


    public class MoveCommandParser : SimpleParser {
        // Regex pattern explanation:
        //
        // ^Bewege\s+               : must start with "Bewege "
        // (?<type>\w+)\s+          : capture the unit type (e.g. "Reiter", "Krieger", etc.)
        // (?<id>\d+)               : capture the numeric unit ID
        // (?:\s+von\s+(?<from>[^\s]+))? : optionally capture something after "von " as "from" (e.g. "503/22")
        // \s+nach\s+(?<to>[^\s]+)  : capture the "nach" location
        // (?:\s+via\s+(?<via>.*))?$: optionally capture everything after "via" as a single string
        //
        private static readonly Regex MoveCommandRegex = new Regex(
            @"^Bewege\s+(?<type>\w+)\s+(?<unitId>\d+)(?:\s+von\s+(?<from>[^\s]+))?\s+nach\s+(?<to>[^\s]+)(?:\s+via\s+(?<via>.*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = MoveCommandRegex.Match(commandString);
            if (!match.Success) 
                return Fail(out command);
           
            try {
                List<KleinfeldPosition> via = [];
                if (match.Groups["via"].Success) {
                    string viaPart = match.Groups["via"].Value;
                    // split by comma
                    var viaLocations = viaPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in viaLocations) {
                        var loc = ParseLocation(v.Trim());
                        if (loc != null) via.Add(loc);
                    }
                }

                command = new MoveCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["type"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    FromLocation = ParseLocation(match.Groups["from"].Value),
                    ToLocation = ParseLocation(match.Groups["to"].Value),
                    ViaLocations = via.Count > 0?via:null,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MoveCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
