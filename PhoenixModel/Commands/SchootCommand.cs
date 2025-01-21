using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PhoenixModel.Commands.SchootCommand;

namespace PhoenixModel.Commands {

    public class SchootCommand : DefaultCommand, ICommand {
        /// <summary>
        /// die Namen entsprechen der Kostentabelle in crossref.mdb
        /// </summary>
        public enum ConstructionElement {
            None,
            LKP,// Leichte Katapulte
            SKP,// Schwere Katapulte
            LKS,// Leichte Kriegsschiffe 
            SKS,// Schwere Kriegsschiffe
        }

        public ConstructionElement With { get; set; } = ConstructionElement.None;
        public KleinfeldPosition? TargetLocation { get; set; } = null;
        public KleinfeldPosition? SourceLocation { get; set; } = null;
        public int? Nummer { get; set; }


        public SchootCommand(string commandString) : base(commandString) {
        }

        


        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult ExecuteCommand() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="ICommand"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }
    }

    public class SchootCommandParser : ICommandParser {
        private static readonly Regex AttackRegex = new Regex(
              // Explanation:
              // ^Beschieße\s+(?<targetLoc>[^\s]+)\s+      : "Beschieße <LOC>"
              // mit\s+                                   : "mit"
              // (?<weaponName>[^\d]+)                    : weapon name (consume non-digit chars, e.g. "Schweren Katapulten ")
              // (\s+(?<weaponId>\d+))?                   : optional space + numeric ID group (e.g. "105")
              // \s+von\s+(?<sourceLoc>[^\s]+)$           : "von <LOC>" at the end
              @"^Beschieße\s+(?<targetLoc>[^\s]+)\s+mit\s+(?<equipment>[^\d]+)(\s+(?<unitId>\d+))?\s+von\s+(?<sourceLoc>[^\s]+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }

        private ConstructionElement parseConstructionElement(string input) {
            return input.ToLower()
            switch {
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
                _ => ConstructionElement.None
            };
        }

        public bool ParseCommand(string commandString, out ICommand? command) {
            var match = AttackRegex.Match(commandString);
            if (!match.Success) {
                return Fail(out command);
            }

            int? nummer = null;
            try { nummer = int.Parse(match.Groups["unitId"].Value); } catch { };
            command = new SchootCommand(commandString) {
                With = parseConstructionElement(match.Groups["unitType"].Value),
                Nummer = nummer,
                TargetLocation = CommandParser.ParseLocation(match.Groups["targetLoc"].Value),
                SourceLocation = CommandParser.ParseLocation(match.Groups["sourceLoc"].Value),
            };
            return true;
        }
    }
}
