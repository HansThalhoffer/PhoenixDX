using PhoenixModel.Commands.Parser;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Extensions;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Schifft ein Heer ein oder aus.
    ///
    /// - "Schiffe Krieger 153 ein auf Schiff 308"
    /// - "Schiffe Krieger 153 aus von Schiff 308 nach 843/02"
    ///
    /// Einschiffen kostet keine Bewegungspunkte, nur Höhenstufenpunkte; Ausschiffen kostet beides
    /// (Regelwerk 4). Die Regeln stehen in <see cref="SchifffahrtsRules"/>.
    ///
    /// Eine eingeschiffte Truppe steht nicht mehr auf der Karte, sondern auf dem Feld
    /// (Flottenreferenz / Nummer der Flotte) - so liest es auch die Altanwendung wieder ein.
    /// </summary>
    public class EmbarkCommand : BaseCommand, IPhoenixCommand {

        public enum Modus { einschiffen, ausschiffen }

        public Modus Mode { get; set; }
        public FigurType Figur { get; set; }
        public int UnitId { get; set; }
        public int ShipId { get; set; }
        public KleinfeldPosition? LandLocation { get; set; }

        /// <summary>Die Truppe, die ein- oder aussteigt</summary>
        public TruppenSpielfigur? Truppe { get; set; } = null;

        /// <summary>Die Flotte</summary>
        public TruppenSpielfigur? Flotte { get; set; } = null;

        /// <summary>
        /// Der Stand der Truppe vor dem Vorgang. BewegungsZustand sichert Position,
        /// Bewegungspunkte, Hoehenstufen, Befehle und die ganze Bewegungsspur - genau das, was
        /// sich beim Ein- und Ausschiffen aendert.
        /// </summary>
        private BewegungsZustand? _zustandTruppe = null;

        /// <summary>Die Flottenzugehoerigkeit der Truppe, die der BewegungsZustand nicht kennt</summary>
        private string? _aufFlotteVorher = null;
        private string? _flottenladungVorher = null;
        private string? _flottenbefehlVorher = null;

        public EmbarkCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            return Mode == Modus.ausschiffen
                ? $"Schiffe {Figur} {UnitId} aus von Schiff {ShipId} nach {LandLocation?.CreateBezeichner()}"
                : $"Schiffe {Figur} {UnitId} ein auf Schiff {ShipId}";
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            if (selectable is not Spielfigur figur)
                return false;
            return Mode == Modus.ausschiffen ? figur.CanDisEmbark() : figur.CanEmbark();
        }

        private TruppenSpielfigur? BestimmeTruppe() {
            if (Truppe != null)
                return Truppe;
            if (_Selectable is TruppenSpielfigur ausgewählt && (UnitId == 0 || ausgewählt.Nummer == UnitId))
                return ausgewählt;
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(truppe => truppe.Nummer == UnitId);
        }

        private TruppenSpielfigur? BestimmeFlotte(TruppenSpielfigur? truppe) {
            if (Flotte != null)
                return Flotte;
            // beim Ausschiffen steht die Flotte in auf_Flotte, wenn sie nicht angegeben wurde
            int nummer = ShipId;
            if (nummer == 0 && truppe != null && int.TryParse(truppe.auf_Flotte, out int ausLadung))
                nummer = ausLadung;
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(schiff => schiff.BaseTyp == FigurType.Schiff && schiff.Nummer == nummer);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var truppe = BestimmeTruppe();
            if (truppe == null)
                return new CommandResultError("Die Truppe wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich keine Truppe bestimmen.", this);
            var flotte = BestimmeFlotte(truppe);

            var geprüft = Mode == Modus.ausschiffen
                ? SchifffahrtsRules.PrüfeAusschiffen(truppe, flotte, LandLocation, out _)
                : SchifffahrtsRules.PrüfeEinschiffen(truppe, flotte, out _);

            return geprüft.HasErrors
                ? new CommandResultError(geprüft, this)
                : new CommandResultSuccess(geprüft, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var truppe = BestimmeTruppe();
            var flotte = BestimmeFlotte(truppe);
            if (truppe == null || flotte == null)
                return new CommandResultError("Truppe oder Flotte wurden nicht gefunden", CommandString, this);

            Truppe = truppe;
            Flotte = flotte;
            _zustandTruppe = BewegungsZustand.Sichern(truppe);
            _aufFlotteVorher = truppe.auf_Flotte;
            _flottenladungVorher = flotte.auf_Flotte;
            _flottenbefehlVorher = flotte.Befehl_bew;

            return Mode == Modus.ausschiffen ? Ausschiffen(truppe, flotte) : Einschiffen(truppe, flotte);
        }

        private CommandResult Einschiffen(TruppenSpielfigur truppe, TruppenSpielfigur flotte) {
            var geprüft = SchifffahrtsRules.PrüfeEinschiffen(truppe, flotte, out int höhenstufen);
            if (geprüft.HasErrors)
                return new CommandResultError(geprüft, this);

            int? referenz = SchifffahrtsRules.GetFlottenreferenz(truppe.Nation);
            if (referenz == null || referenz == 0)
                return new CommandResultError("Die Flottenreferenz des Reiches fehlt",
                    $"In der Tabelle Reich_crossref gibt es keinen Flottenkey für {truppe.Nation?.Name}. "
                    + "Ohne ihn liesse sich die eingeschiffte Truppe später nicht wiederfinden.", this);

            // die Truppe verlässt die Karte und steht ab jetzt im Schiffsbauch
            new Bewegungsspur(truppe).Clear();
            truppe.gf_nach = referenz.Value;
            truppe.kf_nach = flotte.Nummer;
            truppe.auf_Flotte = flotte.Nummer.ToString();
            truppe.hoehenstufen += höhenstufen;
            truppe.Befehl_bew += $"#SCE:{flotte.Nummer}";

            flotte.auf_Flotte += $"#{truppe.Nummer}";
            flotte.Befehl_bew += $"#SCE:{truppe.Nummer}";

            Speichere(truppe);
            Speichere(flotte);
            ZeichneNeu(flotte);

            IsExecuted = true;
            return new CommandResultSuccess($"{truppe.Bezeichner} ist an Bord von {flotte.Bezeichner}",
                $"Das hat {höhenstufen} Höhenstufenpunkte gekostet und keine Bewegungspunkte.", this);
        }

        private CommandResult Ausschiffen(TruppenSpielfigur truppe, TruppenSpielfigur flotte) {
            var geprüft = SchifffahrtsRules.PrüfeAusschiffen(truppe, flotte, LandLocation, out var schritt);
            if (geprüft.HasErrors || schritt == null)
                return new CommandResultError(geprüft, this);

            var ziel = schritt.Ziel;
            if (ziel == null)
                return new CommandResultError("Das Zielfeld fehlt", CommandString, this);

            truppe.gf_nach = ziel.gf;
            truppe.kf_nach = ziel.kf;
            truppe.ph_xy = ziel.ph_xy;
            truppe.bp = schritt.BPRest;
            truppe.hoehenstufen = schritt.HöhenstufenGesamt;
            truppe.auf_Flotte = string.Empty;
            truppe.Befehl_bew += $"#SCA:{flotte.Nummer}";

            flotte.auf_Flotte = EntferneAusLadung(flotte.auf_Flotte, truppe.Nummer);
            flotte.Befehl_bew += $"#SCA:{truppe.Nummer}";

            Speichere(truppe);
            Speichere(flotte);
            ZeichneNeu(flotte);
            ZeichneNeu(ziel);

            IsExecuted = true;
            return new CommandResultSuccess($"{truppe.Bezeichner} ist auf {ziel.Bezeichner} an Land",
                geprüft.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Beide Seiten bekommen ihren Stand zurück. Die Truppe wird dabei nie angelegt oder
        /// gelöscht, nur verschoben - es kann also nichts doppelt entstehen.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Truppe == null || Flotte == null || _zustandTruppe == null)
                return new CommandResultError("Das lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein Zustand vor der Ausführung vermerkt.", this);

            _zustandTruppe.Wiederherstellen();
            Truppe.auf_Flotte = _aufFlotteVorher ?? string.Empty;
            Flotte.auf_Flotte = _flottenladungVorher;
            Flotte.Befehl_bew = _flottenbefehlVorher;

            Speichere(Truppe);
            Speichere(Flotte);
            ZeichneNeu(Flotte);
            if (LandLocation != null)
                ZeichneNeu(LandLocation);

            IsExecuted = false;
            return new CommandResultSuccess(
                Mode == Modus.ausschiffen ? "Das Ausschiffen wurde zurückgenommen" : "Das Einschiffen wurde zurückgenommen",
                $"{Truppe.Bezeichner} steht wieder wie vorher.", this);
        }

        /// <summary>
        /// Streicht eine Truppe aus der Ladungsliste einer Flotte. Die Liste sieht aus wie
        /// "#153#207"; gestrichen wird genau der eine Eintrag.
        /// </summary>
        public static string EntferneAusLadung(string? ladung, int nummer) {
            if (string.IsNullOrEmpty(ladung))
                return string.Empty;
            var übrige = ladung.Split('#', StringSplitOptions.RemoveEmptyEntries)
                .Where(eintrag => eintrag.Trim() != nummer.ToString());
            return string.Concat(übrige.Select(eintrag => $"#{eintrag}"));
        }

        private void Speichere(TruppenSpielfigur truppe) {
            if (truppe is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
        }

        private static void ZeichneNeu(KleinfeldPosition? position) {
            var kleinfeld = KleinfeldView.GetKleinfeld(position);
            if (kleinfeld != null)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
        }

    }

    public class EmbarkCommandParser : SimpleParser {

        /// <summary>
        /// - "Schiffe Krieger 153 ein auf Schiff 308"
        /// - "Schiffe Krieger 153 aus von Schiff 308 nach 843/02"
        ///
        /// Die Angabe der Flotte ist beim Ausschiffen freiwillig, sie steht ja bei der Truppe.
        /// </summary>
        private static readonly Regex EmbarkRegex = new(
              @"^Schiffe\s+(?<figur>\w+)\s+(?<unitId>\d+)\s+"
            + @"(?:(?<ein>ein)\s+auf|(?<aus>aus)\s+von)\s+(?:Schiff|Flotte)\s+(?<shipId>\d+)"
            + @"(?:\s+(?:auf|von)\s+\d+/\d+)?"
            + @"(?:\s+nach\s+(?<land>\d+/\d+))?$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = EmbarkRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                bool aus = match.Groups["aus"].Success;
                if (aus && match.Groups["land"].Success == false)
                    return Fail(out command);

                command = new EmbarkCommand(commandString) {
                    Mode = aus ? EmbarkCommand.Modus.ausschiffen : EmbarkCommand.Modus.einschiffen,
                    Figur = ParseUnitType(match.Groups["figur"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    ShipId = ParseInt(match.Groups["shipId"].Value),
                    LandLocation = match.Groups["land"].Success ? ParseLocation(match.Groups["land"].Value) : null,
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
