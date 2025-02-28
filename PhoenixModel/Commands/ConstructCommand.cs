using PhoenixModel.Commands.Parser;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Extensions;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// erzeugt ein kleines Bauwerk, das eine Richtung braucht, wie eine Wand, Strasse oder eine Brücke
    ///    - "Errichte Wall im Nordosten von 202/33"
    ///    - "Errichte Brücke im Süden von 444/22"
    ///    - "Errichte Brücke im Norden von 47/11"
    /// </summary>
    public class ConstructCommand : BaseCommand, IPhoenixCommand, IEquatable<ConstructCommand> {
       
        public ConstructionElementType What { get; set; } = ConstructionElementType.None;      // "Wand", "Brücke"
        public Direction? Direction { get; set; } = null; // "Nordosten", "Süden", "Norden", etc.
        public KleinfeldPosition? Location { get; set; } = null;
        public Kosten? Kosten = null;

        public ConstructCommand(string commandString) : base(commandString) {
        }

        public override bool CanUndo => IsExecuted == true;

        public override bool HasEffectOn(ISelectable selectable) {
            if (base.HasEffectOn(selectable)) 
                return true;
            if (selectable is KleinfeldPosition position && position.Equals(this.Location))
                return true;
            return false;
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return (selectable != null && selectable is KleinFeld);
        }


        public override string ToString() {
            return (Direction != null)? $"Errichte {What} im {Direction} von {Location}" : $"Errichte {What} auf {Location}"; ;
        }

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            if (What == ConstructionElementType.None)
                return new CommandResultError("Es wurde kein zu errichtendes Bauwerk angegeben", $"In dem Befehl konnte das Bauwerk (Straße, Brücke, Wall) nicht gefunden werden \r\n {this.CommandString}",this);
            if (Direction == null && What != ConstructionElementType.Burg)
                return new CommandResultError("Es wurde keine Richtung angegeben", $"In dem Befehl konnte die Richtung (Nordosten, Westen, etc) nicht gefunden werden \r\n {this.CommandString}", this);
            if (Location == null || SharedData.Map == null)
                return new CommandResultError("Es wurde kein Kleinfeld angegeben", $"In dem Befehl konnte das Kleinfeld zB '701/22' nicht gefunden werden \r\n {this.CommandString}", this);
            if (SharedData.Kosten == null)
                return new CommandResultError("Die Kostentabelle wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle aus der crossref.mdb nicht geladen wurden \r\n {this.CommandString}", this);
            if (SharedData.RuestungBauwerke == null)
                return new CommandResultError("Die RuestungBauwerke wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die RuestungBauwerke aus der Zugdaten Datenbank nicht geladen wurden \r\n {this.CommandString}", this);

            var kosten = KostenView.GetKosten(What.ToString());
            if (kosten == null)
                return new CommandResultError($"Die Kostentablle enthält keinen Wert für {What}", $"Der Befehl kann nicht ausgeführt werden, da die Kostentabelle im Feld Unittyp das genannte Bauwerk nicht kennen \r\n {this.CommandString}", this);
            var kf = SharedData.Map[Location.CreateBezeichner()];
            // leider unvermeidbar, obwohl es überprüft wird
            Direction dir = this.Direction != null ? Direction.Value : ViewModel.Direction.NW;
            switch (What) {
                case ConstructionElementType.Bruecke:
                    return new CommandResult(ConstructRules.CanConstructBridge(kf, dir), this);
                case ConstructionElementType.Burg:
                    return new CommandResult(ConstructRules.CanConstructCastle(kf), this);
                case ConstructionElementType.Kai:
                    return new CommandResult(ConstructRules.CanConstructKai(kf, dir), this);
                case ConstructionElementType.Strasse:
                    return new CommandResult(ConstructRules.CanConstructRoad(kf, dir),this);
                case ConstructionElementType.Wall:
                    return new CommandResult(ConstructRules.CanConstructWall(kf, dir), this);

            }
            return new CommandResultSuccess("Das ConstructCommand kann ausgeführt werden", $"Der Befehl kann ausgeführt werden:\r\n {this.CommandString}", this);
        }

        /// <summary>
        /// In der Datenbank stehen bei Neuanlagen die folgenden Werte, dabei ist id ein autoinc wert
        /// gf	kf	BP_rep  BP_neu  Art	    Kosten	id
        /// 305	4	0	    100	    Wall_NW	5000	40
        /// 305	4	0	    100	    Wall_O	5000	41
        /// 305	4	0	    60	    Kai_NO	3000	42
        /// </summary>
        /// <returns></returns>
        private RuestungBauwerke? CreateRuestungBauwerke() {
            if (Kosten != null && Location != null) {
                return new RuestungBauwerke() {
                    gf = Location.gf,
                    kf = Location.kf,
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

                // Todo das Bauwerk noch aus der Karte löschen

                if (SharedData.Map != null)
                    SharedData.UpdateQueue.Enqueue(SharedData.Map[bauwerk.CreateBezeichner()]);
                ProgramView.Update(bauwerk, EventsAndArgs.ViewEventArgs.ViewEventType.UpdateKleinfeld);
                return new CommandResultSuccess("Undo von ConstructCommand wurde ausgeführt", $"Der Befehl wurde rückgängig gemacht:\r\n {this.CommandString}", this);
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
            if (bauwerk != null && SharedData.RuestungBauwerke != null && SharedData.Map != null) {
                try {
                    SharedData.RuestungBauwerke.ReopenSharedData();
                    SharedData.RuestungBauwerke.Add(bauwerk);

                    // wenn es eine Burg ist, dann braucht es ein Bauwerk
                    if (this.What == ConstructionElementType.Burg) {
                        BauwerkeView.AddBaustelle(SharedData.Map[bauwerk.CreateBezeichner()]);
                    }

                    RuestungBauwerkeView.UpdateKleinFeld(bauwerk);
                    IsExecuted = true;
                    Update(bauwerk, EventsAndArgs.ViewEventArgs.ViewEventType.UpdateKleinfeld);
                }
                catch (Exception ex) {
                    ProgramView.LogError($"Fehler bei der Ausführung von {this.CommandString}",ex.Message);
                }
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

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
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
                ProgramView.LogError("Beim Lesen des ConstructCommand gab es einen Fehler", ex.Message);
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
