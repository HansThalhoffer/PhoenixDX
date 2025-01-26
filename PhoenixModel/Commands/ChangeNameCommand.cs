using PhoenixModel.Commands.Parser;
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
    public class ChangeNameCommand : BaseCommand, ICommand {

        public FigurType Figur = FigurType.None;
        public int UnitId { get; set; } = 0; 
        public string NewName{ get; set; } = string.Empty;
        public string NewSpielerName{ get; set; } = string.Empty;
        public string NewBeschriftung { get; set; } = string.Empty;
        public KleinfeldPosition? Location { get; set; }
        

        public override string ToString() {
            if (Figur != FigurType.None) {
                if (string.IsNullOrEmpty(NewName)) 
                    return $"Nenne {Figur} {UnitId} {NewName}";
                if (string.IsNullOrEmpty(NewSpielerName))
                    return $"{Figur} {UnitId} wird gespielt von {NewSpielerName}";
                if (string.IsNullOrEmpty(NewBeschriftung))
                    return $"Bezeichne {Figur} {UnitId} {NewSpielerName}";
            }
            return $"Nenne Gebäude auf {Location} {NewName}";
        }

        public ChangeNameCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        public override CommandResult ExecuteCommand() {
            throw new NotImplementedException();
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }

    }

    public class ChangeNameCommandParser : SimpleParser {
        private static readonly Regex FigurUmbennenRegex = new Regex(
            @"^Nenne\s+(?<Figur>\w+)\s+(?<UnitId>\d+)\s+(?<NewName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        private static readonly Regex NeuerSpielerRegex = new Regex(
            @"^(?<Figur>\w+)\s+(?<UnitId>\d+)\s+wird\s+gespielt\s+von\s+(?<NewSpielerName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex NeueBeschriftungRegex = new Regex(
            @"^Bezeichne\s+(?<Figur>\w+)\s+(?<UnitId>\d+)\s+(?<NewBeschriftung>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex BauwerkNameRegex = new Regex(
            @"^Nenne\s+Gebäude\s+auf\s+(?<Location>[^\s]+)\s+(?<NewName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out ICommand? command) {
            var match = FigurUmbennenRegex.Match(commandString);
            if (match.Success) {
                try {
                    command = new ChangeNameCommand(commandString) {
                        UnitId = ParseInt(match.Groups["UnitId"].Value),
                        Figur = ParseUnitType(match.Groups["Figur"].Value),
                        NewName = match.Groups["NewName"].Value,
                    };
                    return true;
                }
                catch (Exception ex) {
                    ProgramView.LogError("Beim Lesen des ChangeNameCommands gab es einen Fehler", ex.Message);
                    command = null;
                    return false;
                }
            }

            match = NeuerSpielerRegex.Match(commandString);
            if (match.Success) {
                try {
                    command = new ChangeNameCommand(commandString) {
                        UnitId = ParseInt(match.Groups["UnitId"].Value),
                        Figur = ParseUnitType(match.Groups["Figur"].Value),
                        NewSpielerName = match.Groups["NewSpielerName"].Value,
                    };
                    return true;
                }
                catch (Exception ex) {
                    ProgramView.LogError("Beim Lesen des ChangeNameCommands gab es einen Fehler", ex.Message);
                    command = null;
                    return false;
                }
            }
            match = NeueBeschriftungRegex.Match(commandString);
            if (match.Success) {
                try {
                    command = new ChangeNameCommand(commandString) {
                        UnitId = ParseInt(match.Groups["UnitId"].Value),
                        Figur = ParseUnitType(match.Groups["Figur"].Value),
                        NewBeschriftung = match.Groups["NewBeschriftung"].Value,
                    };
                    return true;
                }
                catch (Exception ex) {
                    ProgramView.LogError("Beim Lesen des ChangeNameCommands gab es einen Fehler", ex.Message);
                    command = null;
                    return false;
                }
            }
            match = BauwerkNameRegex.Match(commandString);
            if (match.Success) {
                try {
                    command = new ChangeNameCommand(commandString) {
                        Location = ParseLocation(match.Groups["Location"].Value),
                        NewName = match.Groups["NewName"].Value
                    };
                    return true;
                }
                catch (Exception ex) {
                    ProgramView.LogError("Beim Lesen des ChangeNameCommands gab es einen Fehler", ex.Message);
                    command = null;
                    return false;
                }
            }

            return Fail(out command);

        }
    }
}
