using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {
    public class ChangeNameCommand : BaseCommand, IPhoenixCommand, IEquatable<ChangeNameCommand> {

        public FigurType Figur = FigurType.None;
        public int UnitId { get; set; } = 0;
        public string NewName { get; set; } = string.Empty;
        public string NewSpielerName { get; set; } = string.Empty;
        public string NewBeschriftung { get; set; } = string.Empty;
        public KleinfeldPosition? Location { get; set; }
        public string OldValue { get; set; } = string.Empty;

        public override string ToString() {
            if (Figur != FigurType.None) {
                if (string.IsNullOrEmpty(NewName) == false)
                    return $"Nenne {Figur} {UnitId} {NewName} ({OldValue})";
                if (string.IsNullOrEmpty(NewSpielerName) == false)
                    return $"{Figur} {UnitId} wird gespielt von {NewSpielerName} ({OldValue})";
                if (string.IsNullOrEmpty(NewBeschriftung) == false)
                    return $"Bezeichne {Figur} {UnitId} {NewBeschriftung} ({OldValue})";
            }
            return $"Nenne Gebäude auf {Location} {NewName} ({OldValue})";
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
                var figur = SpielfigurenView.GetSpielfigur(this.Figur, this.UnitId);
                if (figur == null)
                    return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein {Figur}", $"Der Befehl kann nicht ausgeführt werden, da die Figur nicht gefunden wurde:\r\n {CommandString}", this);
                if (figur is NamensSpielfigur spielfigur == false)
                    return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein {Figur}, die man umbenennen kann", $"Der Befehl kann nicht ausgeführt werden, da die Figur {figur.BaseTyp} keine Namen trägt :\r\n {CommandString}", this);

                if (string.IsNullOrEmpty(NewName) == false) {
                    OldValue = NewName;
                    spielfigur.charname = NewName;
                }
                if (string.IsNullOrEmpty(NewSpielerName) == false) {
                    OldValue = NewSpielerName;
                    spielfigur.SpielerName = NewSpielerName;
                }
                if (string.IsNullOrEmpty(NewBeschriftung) == false) { 
                    OldValue = NewBeschriftung; 
                    spielfigur.Beschriftung = NewBeschriftung; 
                }

                if (spielfigur is Zauberer wiz)
                    Update(wiz, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
                if (spielfigur is Character car)
                    Update(car, ViewEventArgs.ViewEventType.UpdateSpielfiguren);

                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
            else { // ein Gebäude erhält einen Namen
                if (CommonCommandErrors.MissingPosition(Location, CommandString, this, out CommandResult? r1) && r1 != null)
                    return r1;
                var kf = KleinfeldView.GetKleinfeld(Location);
                if (kf != null) {
                    if (kf.Gebäude == null)
                        return new CommandResultError($"Auf der angegebenen Kleinfeldposition {Location} befindet sich kein Gebäude", $"Der Befehl kann nicht ausgeführt werden, da die Kleinfeldposition zwingend ein Gebäude besitzen muss:\r\n {CommandString}", this);
                    // setze Namen in beiden Tabellen und speichere
                    kf.Bauwerknamen = this.NewName;
                    SharedData.StoreQueue.Enqueue(kf);
                    kf.Gebäude.Bauwerknamen = this.NewName;
                    Update(kf.Gebäude, ViewEventArgs.ViewEventType.UpdateGebäude);
                }
                //Update(item, ViewEventArgs.ViewEventType.UpdateGebäude);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
        }

        /// <summary>
        /// Das Ändern des Namens kann nur rückgängig gemacht werden, wenn der alte Namen nicht unbekannt ist
        /// </summary>
        public override bool CanUndo => string.IsNullOrEmpty(OldValue) == false;

        /// <summary>
        /// Das Ändern des Namens kann nur rückgängig gemacht werden, wenn der alte Namen bekannt ist
        /// </summary>
        public override CommandResult UndoCommand() {
            if (CanUndo == false) {
                return new CommandResultError($"Die Umbennenung von {Figur} {UnitId} kann nicht rückgängig gemacht werden", "Da der alte Namen/Bezeichner nicht bekannt ist, kann das Kommando nicht rückgängig gemacht werden.", this);
            }
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;
            if (Figur != FigurType.None) {
                var figur = SpielfigurenView.GetSpielfigur(this.Figur, this.UnitId);
                if (figur == null)
                    return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein {Figur}", $"Der Befehl kann nicht ausgeführt werden, da die Figur nicht gefunden wurde:\r\n {CommandString}", this);
                if (figur is NamensSpielfigur spielfigur == false)
                    return new CommandResultError($"Für die angegebene Nummer {UnitId} findet sich kein {Figur}, die man umbenennen kann", $"Der Befehl kann nicht ausgeführt werden, da die Figur {figur.BaseTyp} keine Namen trägt :\r\n {CommandString}", this);

                if (string.IsNullOrEmpty(NewName) == false) {
                    spielfigur.charname = OldValue;
                }
                if (string.IsNullOrEmpty(NewSpielerName) == false) {
                    spielfigur.SpielerName = OldValue;
                }
                if (string.IsNullOrEmpty(NewBeschriftung) == false) {
                    spielfigur.Beschriftung = OldValue;
                }

                if (spielfigur is Zauberer wiz)
                    Update(wiz, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
                if (spielfigur is Character car)
                    Update(car, ViewEventArgs.ViewEventType.UpdateSpielfiguren);

                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
            else { // ein Gebäude erhält einen Namen
                if (CommonCommandErrors.MissingPosition(Location, CommandString, this, out CommandResult? r1) && r1 != null)
                    return r1;
                var kf = KleinfeldView.GetKleinfeld(Location);
                if (kf != null) {
                    if (kf.Gebäude == null)
                        return new CommandResultError($"Auf der angegebenen Kleinfeldposition {Location} befindet sich kein Gebäude", $"Der Befehl kann nicht ausgeführt werden, da die Kleinfeldposition zwingend ein Gebäude besitzen muss:\r\n {CommandString}", this);
                    // setze alten Namen in beiden Tabellen und speichere
                    kf.Bauwerknamen = OldValue;
                    SharedData.StoreQueue.Enqueue(kf);
                    kf.Gebäude.Bauwerknamen = OldValue;
                    Update(kf.Gebäude, ViewEventArgs.ViewEventType.UpdateGebäude);
                }
                //Update(item, ViewEventArgs.ViewEventType.UpdateGebäude);
                return new CommandResultSuccess($"Das {this.GetType()} wurde erfolgreich ausgeführt", $"Der Befehl wurde ausgeführt:\r\n {this.CommandString}", this);

            }
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
            if (Location == null && other.Location != null)
                return false;

            // Vergleiche die relevanten Felder
            return Figur == other.Figur &&
                   UnitId == other.UnitId &&
                   NewName == other.NewName &&
                   NewSpielerName == other.NewSpielerName &&
                   NewBeschriftung == other.NewBeschriftung &&
                   OldValue == other.OldValue;
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

    /// <summary>
    /// Die ChangeName Kommandos enthalten den alten Wert in Klammern dahinter. Für das Umbenennen ist der alte Wert unwichtig, für das Undo aber sehr
    /// Nenne Gebäude auf 701/30 Bad Innozent (Dorf-III)
    /// Nenne Zauberer 503 Otto Waalkes ()
    /// Bezeichne Zaubeer 511 CZ4
    /// 503 wird gespielt von Hans Dampf (Karl Napp)
    /// </summary>
    public class ChangeNameCommandParser : SimpleParser {

        static string RemoveAndExtractBracketsContent(string input, out string extractedContent) {
            Match match = Regex.Match(input, @"\((.*?)\)");
            extractedContent = match.Success ? match.Groups[1].Value : "";
            return Regex.Replace(input, @"\s*\(.*?\)", "").Trim();
        }

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
            commandString = RemoveAndExtractBracketsContent(commandString, out string oldValue);
            var match = FigurUmbennenRegex.Match(commandString);
            if (match.Success) {
                try {
                    command = new ChangeNameCommand(commandString) {
                        UnitId = ParseInt(match.Groups["UnitId"].Value),
                        Figur = ParseUnitType(match.Groups["Figur"].Value),
                        NewName = match.Groups["NewName"].Value,
                        OldValue = oldValue,
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
                        OldValue = oldValue,
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
                        OldValue = oldValue,
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
                        NewName = match.Groups["NewName"].Value,
                        OldValue = oldValue,
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
