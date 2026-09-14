using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Verlegt die Hauptstadt in eine bestehende Festung (Regelwerk 1.5.13).
    ///
    /// "Verlege die Hauptstadt nach 100/5"        - der erste der vier Monate
    /// "Vollende die Hauptstadtverlegung nach 100/5" - der vierte
    ///
    /// Beide Schritte stehen im Regelwerk und liegen drei Monate auseinander. Der erste macht aus
    /// der alten Hauptstadt eine Festung und kostet 50.000 GS, der vierte aus der bezeichneten
    /// Festung die neue Hauptstadt.
    ///
    /// Dass die drei Monate wirklich um sind, kann die Anwendung nicht nachhalten - eine laufende
    /// Verlegung hat keinen Platz in den Daten. Der Befehl nennt beim Beginn den Monat, in dem der
    /// Abschluss fällig wäre, und prüft beim Abschluss, was sich prüfen lässt: dass das Reich keine
    /// Hauptstadt mehr hat und auf dem Ziel eine bestehende Festung steht.
    ///
    /// Das Geld wird hier so wenig gebucht wie beim Ausbau eines Rüstortes - es wird geprüft.
    /// Gebucht wird bei der Auswertung.
    /// </summary>
    public class HauptstadtverlegungCommand : BaseCommand, IPhoenixCommand, IEquatable<HauptstadtverlegungCommand> {

        /// <summary>Die Festung, in die verlegt wird</summary>
        public KleinfeldPosition? Location { get; set; } = null;

        /// <summary>Der vierte Monat statt des ersten</summary>
        public bool IstAbschluss { get; set; } = false;

        /// <summary>Das Feld, dessen Stufe sich geändert hat - für das Zurücknehmen</summary>
        private KleinFeld? _geändert = null;
        private int? _ruestortVorher = null;
        private int _baupunkteVorher = 0;

        public HauptstadtverlegungCommand(string commandString) : base(commandString) { }

        public override string ToString()
            => IstAbschluss
            ? $"Vollende die Hauptstadtverlegung nach {Location}"
            : $"Verlege die Hauptstadt nach {Location}";

        public override bool CanAppliedTo(ISelectable selectable) => selectable is KleinFeld;

        private KleinFeld? BestimmeZiel()
            => Location != null ? KleinfeldView.GetKleinfeld(Location) : _Selectable as KleinFeld;

        public override CommandResult CheckPreconditions() {
            var ziel = BestimmeZiel();
            if (ziel == null)
                return new CommandResultError("Das Kleinfeld wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Feld bestimmen.", this);

            var geprüft = IstAbschluss
                ? HauptstadtRules.PrüfeAbschluss(ziel)
                : HauptstadtRules.PrüfeBeginn(ziel);
            return geprüft.HasErrors
                ? new CommandResultError(geprüft, this)
                : new CommandResultSuccess(geprüft, this);
        }

        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var ziel = BestimmeZiel();
            if (ziel == null)
                return new CommandResultError("Das Kleinfeld wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Feld bestimmen.", this);

            // Beim Beginn ändert sich die alte Hauptstadt, beim Abschluss das Ziel - gemerkt wird
            // das Feld, das sich tatsächlich geändert hat.
            KleinFeld? betroffen = IstAbschluss ? ziel : HauptstadtRules.FindeHauptstadt(ziel.Nation);
            if (betroffen == null)
                return new CommandResultError("Die Hauptstadt wurde nicht gefunden",
                    $"Der Befehl kann nicht ausgeführt werden:\r\n {CommandString}", this);

            _ruestortVorher = betroffen.Ruestort;
            _baupunkteVorher = betroffen.Baupunkte;

            bool geschafft = IstAbschluss
                ? HauptstadtRules.SchliesseAb(ziel)
                : HauptstadtRules.Beginne(ziel) != null;
            if (geschafft == false)
                return new CommandResultError("Die Ausbaustufe liess sich nicht setzen",
                    "In der Rüstortreferenz fehlt die Stufe, die dafür gebraucht wird.", this);

            _geändert = betroffen;
            SharedData.StoreQueue.Enqueue(betroffen);
            SharedData.UpdateQueue.Enqueue(betroffen);
            ProgramView.Update(betroffen, ViewEventArgs.ViewEventType.UpdateKleinfeld);

            IsExecuted = true;
            if (IstAbschluss == false)
                ProgramView.LogInfo(ziel, $"Die Hauptstadtverlegung nach {ziel.Bezeichner} hat begonnen",
                    $"{betroffen.Bezeichner} ist ab jetzt eine Festung und bringt entsprechend weniger "
                    + $"Einnahmen und Rüstkapazität. Der Abschluss ist im Monat "
                    + $"{HauptstadtRules.GetAbschlussmonat(ProgramView.SelectedMonth)} fällig und muss "
                    + "dann befohlen werden.");

            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        public override bool CanUndo => _geändert != null && _ruestortVorher != null;

        public override CommandResult UndoCommand() {
            if (CanUndo == false || _geändert == null)
                return new CommandResultError("Das lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist keine geänderte Ausbaustufe vermerkt.", this);

            _geändert.Ruestort = _ruestortVorher;
            _geändert.Baupunkte = _baupunkteVorher;

            SharedData.StoreQueue.Enqueue(_geändert);
            SharedData.UpdateQueue.Enqueue(_geändert);
            ProgramView.Update(_geändert, ViewEventArgs.ViewEventType.UpdateKleinfeld);

            IsExecuted = false;
            return new CommandResultSuccess($"{_geändert.Bezeichner} steht wieder wie vorher",
                $"Der Befehl wurde zurückgenommen:\r\n {CommandString}", this);
        }

        public bool Equals(HauptstadtverlegungCommand? other)
            => other != null && Nullable.Equals(Location, other.Location)
            && IstAbschluss == other.IstAbschluss;

        public override bool Equals(object? obj) => obj is HauptstadtverlegungCommand anderer && Equals(anderer);

        public override int GetHashCode() => HashCode.Combine(Location, IstAbschluss);
    }

    /// <summary>
    /// Liest die beiden Schritte der Hauptstadtverlegung.
    ///
    /// "Verlege die Hauptstadt nach 100/5"
    /// "Vollende die Hauptstadtverlegung nach 100/5"
    /// </summary>
    public class HauptstadtverlegungCommandParser : SimpleParser {

        private static readonly Regex BefehlRegex = new(
            @"^(?<art>Verlege|Vollende)\s+(?:die\s+)?(?<was>Hauptstadtverlegung|Hauptstadt)\s+"
            + @"(?:nach|in|zur|zu)\s+(?<gf>\d+)\s*/\s*(?<kf>\d+)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            command = null;
            var treffer = BefehlRegex.Match(commandString.Trim());
            if (treffer.Success == false)
                return false;

            try {
                bool abschluss = treffer.Groups["art"].Value.StartsWith("Vollende", StringComparison.OrdinalIgnoreCase);

                command = new HauptstadtverlegungCommand(commandString) {
                    Location = new KleinfeldPosition(
                        ParseInt(treffer.Groups["gf"].Value),
                        ParseInt(treffer.Groups["kf"].Value)),
                    IstAbschluss = abschluss,
                };
                return true;
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des Verlegungsbefehls gab es einen Fehler", ex.Message);
                return false;
            }
        }
    }
}
