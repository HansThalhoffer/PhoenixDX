﻿using PhoenixModel.Commands.Parser;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PhoenixModel.Commands.DiplomacyCommand;

namespace PhoenixModel.Commands {
    public class ChangeNameCommand : BaseCommand, IPhoenixCommand, IEquatable<ChangeNameCommand> {

        public FigurType Figur = FigurType.None;
        public int UnitId { get; set; } = 0;
        public string NewName { get; set; } = string.Empty;
        public string NewSpielerName { get; set; } = string.Empty;
        public string NewBeschriftung { get; set; } = string.Empty;
        public KleinfeldPosition? Location { get; set; }

        public override string ToString() {
            if (Figur != FigurType.None) {
                if (string.IsNullOrEmpty(NewName) == false)
                    return $"Nenne {Figur} {UnitId} {NewName}";
                if (string.IsNullOrEmpty(NewSpielerName) == false)
                    return $"{Figur} {UnitId} wird gespielt von {NewSpielerName}";
                if (string.IsNullOrEmpty(NewBeschriftung) == false)
                    return $"Bezeichne {Figur} {UnitId} {NewSpielerName}";
            }
            return $"Nenne Gebäude auf {Location} {NewName}";
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return (selectable != null && selectable is NamensSpielfigur);
        }

        public ChangeNameCommand(string commandString) : base(commandString) {
        }

        public override CommandResult CheckPreconditions() {
            if (Figur != FigurType.None) {
                if (CommonCommandErrors.MissingUnitID(UnitId, CommandString, this, out CommandResult? r1) && r1 != null)
                    return r1;

                if (string.IsNullOrEmpty(NewBeschriftung) && string.IsNullOrEmpty(NewSpielerName) && string.IsNullOrEmpty(NewBeschriftung))
                    return new CommandResultError("Es wurde kein neuer Name angegeben", $"Es muss je nach Befehl ein neuer Name für Spieler, Charakter oder Gebäude angegeben werden:\r\n {this.CommandString}", this);
                switch (Figur) {
                    case FigurType.Krieger:
                    case FigurType.Kreatur:
                    case FigurType.Reiter:
                        return new CommandResultError("Reiter, Krieger, Kreaturen können nicht umbenannt werden", "Nur Zauberer und Charaktere können eine eigene Beschriftung oder Namen erhalten", this);
                    case FigurType.Charakter:
                        if (CommonCommandErrors.NotLoaded(SharedData.Character, "Charakter", CommandString, this, out CommandResult? r2) && r2 != null)
                            return r2;
                        break;
                    case FigurType.Zauberer:
                        if (CommonCommandErrors.NotLoaded(SharedData.Zauberer, "Zauberer", CommandString, this, out CommandResult? r3) && r3 != null)
                            return r3;
                        break;
                }
            }
            else {
                if (CommonCommandErrors.MissingPosition(Location, CommandString, this, out CommandResult? r1) && r1 != null)
                    return r1;
            }

            return new CommandResultSuccess($"Das {this.GetType()} kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}", this);
        }

        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            if (Figur != FigurType.None) {
                switch (Figur) {
                    case FigurType.Charakter:
                        if (CommonCommandErrors.NotLoaded(SharedData.Character, "Charakter", CommandString, this, out CommandResult? r2) && r2 != null)
                            return r2;
                        break;
                    case FigurType.Zauberer:
                        if (CommonCommandErrors.NotLoaded(SharedData.Zauberer, "Zauberer", CommandString, this, out CommandResult? r3) && r3 != null)
                            return r3;
                        break;
                }
                //Update(item, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
            else {
                if (CommonCommandErrors.MissingPosition(Location, CommandString, this, out CommandResult? r1) && r1 != null)
                    return r1;
                //Update(item, ViewEventArgs.ViewEventType.UpdateGebäude);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
        }

        /// <summary>
        /// Das Ändern des Namens kann nicht rückgängig gemacht werden, da der alte Namen unbekannt ist
        /// </summary>
        public override bool CanUndo => false;

        /// <summary>
        /// Das Ändern des Namens kann nicht rückgängig gemacht werden, da der alte Namen unbekannt ist
        /// </summary>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }


        // Implementing IEquatable<ChangeNameCommand>

        /// <summary>
        /// Vergleicht zwei Instanzen von <see cref="ChangeNameCommand"/> auf Gleichheit.
        /// </summary>
        /// <param name="other">Die andere Instanz von <see cref="ChangeNameCommand"/>, mit der verglichen wird.</param>
        /// <returns>True, wenn die Instanzen gleich sind, andernfalls False.</returns>
        public bool Equals(ChangeNameCommand? other) {
            // Überprüft, ob das andere Objekt null ist oder nicht vom gleichen Typ ist
            if (other == null)
                return false;

            if (Location != null && Location.Equals(other.Location) == false)
                return false;
            if (Location == null &&  other.Location != null) 
                return false;

            // Vergleiche die relevanten Felder
            return Figur == other.Figur &&
                   UnitId == other.UnitId &&
                   NewName == other.NewName &&
                   NewSpielerName == other.NewSpielerName &&
                   NewBeschriftung == other.NewBeschriftung;
        }

        /// <summary>
        /// Überschreibt die Equals-Methode, um die Instanzen zu vergleichen.
        /// </summary>
        /// <param name="obj">Das Objekt, mit dem verglichen wird.</param>
        /// <returns>True, wenn das Objekt gleich ist, andernfalls False.</returns>
        public override bool Equals(object? obj) {
            if (obj is ChangeNameCommand otherCommand) {
                return Equals(otherCommand);
            }

            return false;
        }

        /// <summary>
        /// Überschreibt die Methode GetHashCode, um einen Hashwert zu berechnen, der die Gleichheit der Instanzen widerspiegelt.
        /// </summary>
        /// <returns>Ein Hashwert, der die Instanz repräsentiert.</returns>
        public override int GetHashCode() {
            // Verwendet HashCode.Combine für eine vereinfachte und effiziente Hashcode-Berechnung
            return HashCode.Combine(Figur, UnitId, NewName, NewSpielerName, NewBeschriftung, Location);
        }
    }

    public class ChangeNameCommandParser : SimpleParser {
        /// <summary>
        /// Regex zur Erkennung von Befehlen zum Umbenennen einer Figur.
        /// </summary>
        private static readonly Regex FigurUmbennenRegex = new Regex(
            @"^Nenne\s+(?<Figur>\w+)\s+(?<UnitId>\d+)\s+(?<NewName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Regex zur Erkennung von Befehlen zur Änderung des Spielers einer Figur.
        /// </summary>
        private static readonly Regex NeuerSpielerRegex = new Regex(
            @"^(?<Figur>\w+)\s+(?<UnitId>\d+)\s+wird\s+gespielt\s+von\s+(?<NewSpielerName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Regex zur Erkennung von Befehlen zur Änderung der Beschriftung einer Figur.
        /// </summary>
        private static readonly Regex NeueBeschriftungRegex = new Regex(
            @"^Bezeichne\s+(?<Figur>\w+)\s+(?<UnitId>\d+)\s+(?<NewBeschriftung>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Regex zur Erkennung von Befehlen zur Umbenennung eines Bauwerks.
        /// </summary>
        private static readonly Regex BauwerkNameRegex = new Regex(
            @"^Nenne\s+Gebäude\s+auf\s+(?<Location>[^\s]+)\s+(?<NewName>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Analysiert einen Befehl und erzeugt das entsprechende Command-Objekt.
        /// </summary>
        /// <param name="commandString">Der zu analysierende Befehl.</param>
        /// <param name="command">Das erstellte IPhoenixCommand-Objekt oder null im Fehlerfall.</param>
        /// <returns>True, wenn der Befehl erfolgreich analysiert wurde, andernfalls false.</returns>
        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
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
