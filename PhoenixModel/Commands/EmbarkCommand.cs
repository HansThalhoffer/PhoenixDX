using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// - Schiffe Krieger 153 ein auf Schiff 308 auf 707/45
    /// - Schiffe Krieger 153 aus von Schiff 308 auf 843/01 nach 843/02
    /// </summary>
    public class EmbarkCommand : BaseCommand, IPhoenixCommand {
        public enum Modus { einschiffen, ausschiffen }

        public Modus Mode { get; set; }
        public FigurType Figur { get; set; }
        public int UnitId { get; set; }
        public int ShipId { get; set; }
        public KleinfeldPosition? ShipLocation { get; set; }
        public KleinfeldPosition? LandLocation { get; set; }

        public override string ToString() {
            string mode = Mode == Modus.ausschiffen ? "aus von" : "ein auf";
            string wasser  = Mode == Modus.ausschiffen ? $" nach {LandLocation}" : string.Empty;
            return $"Schiffe {Figur} {UnitId} {mode} Schiff {ShipId} auf {ShipLocation}{wasser}";
        }

        public EmbarkCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            throw new NotImplementedException();
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }
    }

    public class EmbarkCommandParser : SimpleParser {
        private static readonly Regex EmbarkCommandRegex = new Regex(
            // Explanation:
            // ^Schiffe\s+(?<cargoType>\w+)\s+(?<cargoId>\d+)\s+(?<mode>ein|aus)\s+
            // (?:auf|von)\s+Schiff\s+(?<shipId>\d+)\s+auf\s+(?<shipLoc>[^\s]+)
            // (?:\s+nach\s+(?<targetLoc>[^\s]+))?$
            @"^Schiffe\s+(?<Figur>\w+)\s+(?<UnitId>\d+)\s+(?<Mode>ein|aus)\s+(?:auf|von)\s+Schiff\s+(?<ShipId>\d+)\s+auf\s+(?<ShipLocation>[^\s]+)(?:\s+nach\s+(?<LandLocation>[^\s]+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );


        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = EmbarkCommandRegex.Match(commandString);
            if (!match.Success)
                return Fail(out command);

            try {
                command = new EmbarkCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["Figur"].Value),
                    UnitId = ParseInt(match.Groups["UnitId"].Value),
                    Mode = match.Groups["UnitId"].Value == "ein" ? EmbarkCommand.Modus.einschiffen : EmbarkCommand.Modus.ausschiffen,
                    ShipId = ParseInt(match.Groups["ShipId"].Value),
                    ShipLocation = ParseLocation(match.Groups["ShipLocation"].Value),
                    LandLocation = match.Groups.ContainsKey("LandLocation") ? ParseLocation(match.Groups["LandLocation"].Value): null,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des EmbarkCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
