using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Was mit fremdem Gebiet geschieht, das im Zuge einer Bewegung betreten wird
    /// </summary>
    public enum EroberungsModus {
        /// <summary>Das Gebiet wird auferobert (Befehl E:)</summary>
        Erobern,
        /// <summary>Das Gebiet wird nur geplündert (Befehl P:)</summary>
        Plündern,
    }

    /// <summary>
    /// Repräsentiert einen Befehl zum Bewegen einer Figur.
    /// Bewege Reiter 220 [von 503/22] nach 504/07 [via 503/21, 503/16, 503/4]
    ///
    /// Der Befehl bewegt die Figur Schritt für Schritt über benachbarte Kleinfelder. Jeder Schritt
    /// wird durch <see cref="BewegungsRules.PrüfeSchritt(Spielfigur, Direction)"/> geprüft.
    /// Vor der Ausführung wird der komplette Bewegungszustand der Figur gesichert, damit ein Undo
    /// die Figur exakt zurücksetzen kann.
    /// </summary>
    public class MoveCommand : BaseCommand, IEquatable<MoveCommand> {
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
        /// Wird beim Betreten fremden Gebietes erobert oder nur geplündert?
        /// Die Altanwendung erobert grundsätzlich, daher ist das die Voreinstellung.
        /// </summary>
        public EroberungsModus Eroberung { get; set; } = EroberungsModus.Erobern;

        /// <summary>
        /// Der gesicherte Zustand der Figur vor der Bewegung. Wird für das Undo benötigt.
        /// </summary>
        private BewegungsZustand? _zustandVorBewegung = null;

        /// <summary>
        /// Alle Kleinfelder, die durch diese Bewegung neu gezeichnet werden müssen
        /// </summary>
        private readonly List<KleinFeld> _betroffeneKleinfelder = [];

        /// <summary>
        /// Erstellt eine neue Instanz des MoveCommand mit einem gegebenen Befehlsstring.
        /// </summary>
        /// <param name="commandString">Der Befehlsstring.</param>
        public MoveCommand(string commandString) : base(commandString) { }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable != null && selectable is Spielfigur;
        }

        public override bool HasEffectOn(ISelectable selectable) {
            if (base.HasEffectOn(selectable))
                return true;
            if (selectable is Spielfigur figur)
                return figur.Typ == this.Figur && figur.Nummer == this.UnitId;
            if (selectable is KleinfeldPosition position)
                return GetWegpunkte().Any(p => p.gf == position.gf && p.kf == position.kf);
            return false;
        }

        /// <summary>
        /// Ein ausgeführtes Kommando kann immer zurückgenommen werden, da der Zustand davor gesichert wurde.
        /// </summary>
        public override bool CanUndo => IsExecuted && _zustandVorBewegung != null;

        public override string ToString() {
            string result = $"Bewege {SimpleParser.UnitTypeToString(Figur)} {UnitId} von {FromLocation} nach {ToLocation}";
            if (ViaLocations != null && ViaLocations.Count > 0)
                result += " via " + string.Join(", ", ViaLocations.Select(v => v.CreateBezeichner()));
            return result;
        }

        /// <summary>
        /// Setzt den Zustand, auf den ein Undo die Figur zurücksetzt. Wird beim Rekonstruieren
        /// einer bereits in der Zugdatenbank gespeicherten Bewegung benötigt.
        /// <seealso cref="PhoenixModel.View.BewegungView.RekonstruiereBewegung"/>
        /// </summary>
        public void SetzeZustandVorBewegung(BewegungsZustand zustand) {
            _zustandVorBewegung = zustand;
            IsExecuted = true;
        }

        /// <summary>
        /// Die Spielfigur, die dieser Befehl bewegt
        /// </summary>
        public Spielfigur? GetSpielfigur() {
            if (_Selectable is Spielfigur bekannt)
                return bekannt;
            return SpielfigurenView.GetSpielfigur(Figur, UnitId);
        }

        /// <summary>
        /// Die Zielfelder aller Schritte in der Reihenfolge der Bewegung, ohne das Startfeld
        /// </summary>
        public List<KleinfeldPosition> GetWegpunkte() {
            List<KleinfeldPosition> wegpunkte = [];
            if (ViaLocations != null)
                wegpunkte.AddRange(ViaLocations);
            if (ToLocation != null)
                wegpunkte.Add(ToLocation);
            return wegpunkte;
        }

        /// <summary>
        /// Überprüft die Vorbedingungen für den Befehl, ohne etwas zu verändern.
        /// Dafür wird die komplette Bewegung probeweise durchgeführt und anschließend zurückgesetzt.
        /// </summary>
        /// <returns>Das Ergebnis der Vorbedingungsprüfung.</returns>
        public override CommandResult CheckPreconditions() {
            return Durchführen(true);
        }

        /// <summary>
        /// Führt den Befehl aus.
        /// </summary>
        /// <returns>Das Ergebnis der Befehlsausführung.</returns>
        public override CommandResult ExecuteCommand() {
            if (IsExecuted)
                return new CommandResultError("Die Bewegung wurde bereits ausgeführt",
                    $"Der Befehl wurde schon einmal ausgeführt und kann nicht erneut ausgeführt werden:\r\n{this.CommandString}", this);

            CommandResult result = Durchführen(false);
            if (result.HasErrors)
                return result;

            var figur = GetSpielfigur();
            if (figur == null)
                return new CommandResultError("Die Figur wurde nicht gefunden", $"Zu dem Befehl {this.CommandString} gibt es keine passende Spielfigur", this);

            ZeichneBetroffeneKleinfelderNeu();
            if (figur is Database.IDatabaseTable eintrag)
                Update(eintrag, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            return new CommandResultSuccess("Die Bewegung wurde ausgeführt", $"Der Befehl wurde ausgeführt:\r\n{this.ToString()}", this);
        }

        /// <summary>
        /// Macht den Befehl rückgängig, indem der gesicherte Zustand der Figur wiederhergestellt wird.
        /// </summary>
        /// <returns>Das Ergebnis des Rückgängigmachens.</returns>
        public override CommandResult UndoCommand() {
            if (_zustandVorBewegung == null)
                return new CommandResultError("Die Bewegung kann nicht zurückgenommen werden",
                    $"Zu dem Befehl {this.CommandString} wurde kein Zustand vor der Bewegung gesichert", this);

            var figur = _zustandVorBewegung.Figur;
            _zustandVorBewegung.Wiederherstellen();
            _zustandVorBewegung = null;
            IsExecuted = false;

            ZeichneBetroffeneKleinfelderNeu();
            _betroffeneKleinfelder.Clear();
            if (figur is Database.IDatabaseTable eintrag)
                Update(eintrag, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            return new CommandResultSuccess("Die Bewegung wurde zurückgenommen", $"Der Befehl wurde rückgängig gemacht:\r\n{this.ToString()}", this);
        }

        /// <summary>
        /// Führt die Bewegung durch. Im Testmodus wird der Zustand der Figur anschließend
        /// wieder auf den Ausgangswert gesetzt, es bleibt also nichts zurück.
        /// </summary>
        /// <param name="nurTesten">true, wenn nur geprüft und nichts verändert werden soll</param>
        private CommandResult Durchführen(bool nurTesten) {
            if (SharedData.Map == null || SharedData.Map.IsAddingCompleted == false)
                return new CommandResultError("Die Karte ist noch nicht geladen",
                    $"Ohne Karte kann keine Bewegung durchgeführt werden:\r\n{this.CommandString}", this);

            var figur = GetSpielfigur();
            if (figur == null)
                return new CommandResultError("Die Figur wurde nicht gefunden",
                    $"Es gibt keine Figur vom Typ {Figur} mit der Nummer {UnitId}:\r\n{this.CommandString}", this);

            if (ProgramView.BelongsToUser(figur) == false)
                return new CommandResultError("Die Figur gehört einem anderen Reich",
                    $"{figur.Bezeichner} kann nicht bewegt werden, da die Figur nicht zum eigenen Reich gehört", this);

            var wegpunkte = GetWegpunkte();
            if (wegpunkte.Count == 0)
                return new CommandResultError("Es wurde kein Ziel angegeben",
                    $"In dem Befehl konnte kein Zielfeld gefunden werden:\r\n{this.CommandString}", this);

            if (FromLocation != null && FromLocation.gf != 0 && (FromLocation.gf != figur.gf || FromLocation.kf != figur.kf))
                return new CommandResultError("Die Figur steht nicht auf dem angegebenen Startfeld",
                    $"{figur.Bezeichner} steht auf {figur.CreateBezeichner()}, der Befehl geht aber von {FromLocation.CreateBezeichner()} aus", this);

            var zustand = BewegungsZustand.Sichern(figur);
            List<KleinFeld> betroffen = [];

            try {
                KleinfeldPosition aktuell = new(figur.gf, figur.kf);
                for (int i = 0; i < wegpunkte.Count; i++) {
                    var wegpunkt = wegpunkte[i];
                    var richtung = BewegungsRules.GetRichtung(aktuell, wegpunkt);
                    SchrittErgebnis schritt;

                    if (richtung != null) {
                        // ein gewöhnlicher Schritt auf ein Nachbarfeld
                        schritt = BewegungsRules.PrüfeSchritt(figur, richtung.Value);
                        if (schritt.HasErrors == false && schritt.Teleportpunkt && i == wegpunkte.Count - 1) {
                            zustand.Wiederherstellen();
                            return new CommandResultError("Das Auftauchfeld fehlt",
                                $"{wegpunkt.CreateBezeichner()} ist ein Teleportfeld. Auf einem Teleportfeld bleibt niemand stehen - der Weg muss mit dem Feld weitergehen, auf dem die Figur auftaucht", this);
                        }
                    }
                    else if (TeleportRules.IstTeleportfeld(KleinfeldView.GetKleinfeld(aktuell))) {
                        // der Sprung von einem Teleportfeld auf das gewählte Auftauchfeld
                        schritt = TeleportRules.PrüfeAuftauchen(figur, KleinfeldView.GetKleinfeld(aktuell), wegpunkt);
                    }
                    else {
                        zustand.Wiederherstellen();
                        return new CommandResultError("Der Weg ist unterbrochen",
                            $"{wegpunkt.CreateBezeichner()} ist kein Nachbarfeld von {aktuell.CreateBezeichner()}. Bitte alle Zwischenfelder mit via angeben", this);
                    }

                    if (schritt.HasErrors) {
                        zustand.Wiederherstellen();
                        return new CommandResultError(schritt, this);
                    }

                    WendeSchrittAn(figur, schritt);
                    if (schritt.Start != null)
                        betroffen.Add(schritt.Start);
                    if (schritt.Ziel != null)
                        betroffen.Add(schritt.Ziel);
                    aktuell = wegpunkt;
                }
            }
            catch (Exception ex) {
                zustand.Wiederherstellen();
                ProgramView.LogError($"Fehler bei der Ausführung von {this.CommandString}", ex.Message);
                return new CommandResultError("Bei der Bewegung ist ein Fehler aufgetreten", ex.Message, this);
            }

            if (nurTesten) {
                zustand.Wiederherstellen();
            }
            else {
                _zustandVorBewegung = zustand;
                _betroffeneKleinfelder.Clear();
                _betroffeneKleinfelder.AddRange(betroffen.Distinct());
            }
            return new CommandResultSuccess("Die Bewegung ist möglich", $"Der Befehl kann ausgeführt werden:\r\n{this.ToString()}", this);
        }

        /// <summary>
        /// Überträgt das Ergebnis eines geprüften Schrittes auf die Figur
        /// </summary>
        private void WendeSchrittAn(Spielfigur figur, SchrittErgebnis schritt) {
            var ziel = schritt.Ziel;
            if (ziel == null)
                return;
            figur.gf_nach = ziel.gf;
            figur.kf_nach = ziel.kf;
            figur.ph_xy = ziel.ph_xy;
            figur.bp = schritt.BPRest;
            figur.hoehenstufen = schritt.HöhenstufenGesamt;
            new Bewegungsspur(figur).Add(ziel);

            // fremdes Gebiet wird auferobert oder geplündert
            if (schritt.Erobert && figur is TruppenSpielfigur truppe) {
                string marke = Eroberung == EroberungsModus.Erobern ? "E" : "P";
                truppe.Befehl_erobert += $"{marke}:{ziel.gf}/{ziel.kf};";
            }
        }

        /// <summary>
        /// Sorgt dafür, dass Start- und Zielfelder der Bewegung in der Karte aktualisiert werden
        /// </summary>
        private void ZeichneBetroffeneKleinfelderNeu() {
            foreach (var kleinfeld in _betroffeneKleinfelder)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
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
            @"^Bewege\s+(?<type>[\w ]+?)\s+(?<unitId>\d+)(?:\s+von\s+(?<from>[^\s]+))?\s+nach\s+(?<to>[^\s]+)(?:\s+via\s+(?<via>.*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
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
                    ViaLocations = via.Count > 0 ? via : null,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MoveCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Erzeugt den Befehlsstring für die Bewegung einer Figur auf ein benachbartes Kleinfeld
        /// </summary>
        public static string GenerateCommand(Spielfigur figur, KleinfeldPosition ziel) {
            return $"Bewege {UnitTypeToString(figur.Typ)} {figur.Nummer} von {figur.CreateBezeichner()} nach {ziel.CreateBezeichner()}";
        }

        /// <summary>
        /// Erzeugt den Befehlsstring für die Bewegung einer Figur über mehrere Kleinfelder
        /// </summary>
        public static string GenerateCommand(Spielfigur figur, IList<KleinfeldPosition> weg) {
            if (weg.Count == 0)
                return string.Empty;
            string befehl = $"Bewege {UnitTypeToString(figur.Typ)} {figur.Nummer} von {figur.CreateBezeichner()} nach {weg[weg.Count - 1].CreateBezeichner()}";
            if (weg.Count > 1)
                befehl += " via " + string.Join(", ", weg.Take(weg.Count - 1).Select(p => p.CreateBezeichner()));
            return befehl;
        }
    }
}
