using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Teleportiert einen Zauberer ohne Ladung.
    ///
    /// "Nur die Teleportation ist eine Grundfähigkeit aller Zauberer und kostet keine
    /// Zauberkraftpunkte. Sie ist eine Bewegungsfertigkeit!!!" (Regelwerk 1.3) Deshalb ist das
    /// kein <see cref="CastSpellCommando"/>: der Zauberer kann im selben Monat noch zaubern.
    ///
    /// Bezahlt wird mit Teleportpunkten - je nach Klasse 6 bis 9 Gemarken im Monat. Wer Rüstgüter
    /// mitnehmen will, braucht dagegen einen Zauberspruch und mindestens Klasse ZE; das steht im
    /// <see cref="CastSpellCommando"/>.
    ///
    /// - "Zauberer 501 teleportiert nach 755/22"
    /// </summary>
    public class TeleportCommand : BaseCommand, IPhoenixCommand {

        public TeleportCommand(string commandString) : base(commandString) {
        }

        /// <summary>Die Nummer des Zauberers, 0 wenn die Auswahl gilt</summary>
        public int UnitID { get; set; } = 0;

        /// <summary>Das Zielfeld</summary>
        public KleinfeldPosition? LocationTo { get; set; } = null;

        /// <summary>Der Zauberer, der teleportiert</summary>
        public Zauberer? Figur { get; set; } = null;

        /// <summary>Die zurückgelegte Entfernung in Gemarken</summary>
        public int Entfernung { get; private set; } = 0;

        private BewegungsZustand? _zustandVorher = null;
        private int _teleportpunkteVorher = 0;
        private string? _befehlTeleportVorher = null;
        private (int gf, int kf) _teleportVonVorher = (0, 0);
        private (int gf, int kf) _teleportNachVorher = (0, 0);

        public override bool CanAppliedTo(ISelectable selectable) => selectable is Zauberer;

        public override string ToString()
            => $"Zauberer {Figur?.Nummer ?? UnitID} teleportiert nach {LocationTo?.CreateBezeichner()}";

        private Zauberer? BestimmeZauberer() {
            if (Figur != null)
                return Figur;
            if (UnitID == 0)
                return _Selectable as Zauberer;
            return SharedData.Zauberer?.FirstOrDefault(z => z.Nummer == UnitID && Plausibilität.IsValid(z));
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var zauberer = BestimmeZauberer();
            if (zauberer == null)
                return new CommandResultError("Der Zauberer wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Zauberer bestimmen. Bitte den Zauberer auswählen "
                    + "oder mit seiner Nummer angeben.", this);

            var geprüft = ZaubereiRules.PrüfeTeleport(zauberer, LocationTo, out int entfernung);
            if (geprüft.HasErrors)
                return new CommandResultError(geprüft, this);
            Entfernung = entfernung;
            return new CommandResultSuccess(geprüft, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var zauberer = BestimmeZauberer();
            if (zauberer == null || LocationTo == null)
                return new CommandResultError("Der Zauberer wurde nicht gefunden", CommandString, this);

            Figur = zauberer;
            _zustandVorher = BewegungsZustand.Sichern(zauberer);
            _teleportpunkteVorher = zauberer.tp;
            _befehlTeleportVorher = zauberer.Befehl_Teleport ?? string.Empty;
            _teleportVonVorher = (zauberer.Teleport_gf_von, zauberer.Teleport_kf_von);
            _teleportNachVorher = (zauberer.Teleport_gf_nach, zauberer.Teleport_kf_nach);

            var start = ZaubereiRules.GetStandort(zauberer);
            zauberer.Teleport_gf_von = start.gf;
            zauberer.Teleport_kf_von = start.kf;
            zauberer.Teleport_gf_nach = LocationTo.gf;
            zauberer.Teleport_kf_nach = LocationTo.kf;
            zauberer.Befehl_Teleport = new Teleportbefehl { Von = start, Nach = LocationTo }.ToString();
            zauberer.tp += Entfernung;
            zauberer.gf_nach = LocationTo.gf;
            zauberer.kf_nach = LocationTo.kf;

            Update(zauberer, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneFelderNeu(start, LocationTo);
            IsExecuted = true;
            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Figur == null || _zustandVorher == null)
                return new CommandResultError("Der Teleport lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein ausgeführter Vorgang vermerkt.", this);

            var start = ZaubereiRules.GetStandort(Figur);
            _zustandVorher.Wiederherstellen();
            Figur.tp = _teleportpunkteVorher;
            Figur.Befehl_Teleport = _befehlTeleportVorher ?? string.Empty;
            Figur.Teleport_gf_von = _teleportVonVorher.gf;
            Figur.Teleport_kf_von = _teleportVonVorher.kf;
            Figur.Teleport_gf_nach = _teleportNachVorher.gf;
            Figur.Teleport_kf_nach = _teleportNachVorher.kf;

            Update(Figur, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneFelderNeu(start, ZaubereiRules.GetStandort(Figur));
            IsExecuted = false;
            return new CommandResultSuccess("Der Teleport wurde zurückgenommen", ToString(), this);
        }

        private static void ZeichneFelderNeu(params KleinfeldPosition?[] felder) {
            foreach (var position in felder) {
                if (position == null)
                    continue;
                var kleinfeld = KleinfeldView.GetKleinfeld(position);
                if (kleinfeld != null)
                    SharedData.UpdateQueue.Enqueue(kleinfeld);
            }
        }
    }

    public class TeleportCommandParser : SimpleParser {

        /// <summary>
        /// "Zauberer 501 teleportiert nach 755/22"
        ///
        /// Mit Ladung ist es ein Zauberspruch und gehört zum <see cref="CastSpellCommando"/> -
        /// deshalb endet der Ausdruck hinter dem Zielfeld.
        /// </summary>
        private static readonly Regex TeleportRegex = new(
            @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?teleportiert?\s+(?:von\s+\d+/\d+\s+)?nach\s+(?<feld>\d+/\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var treffer = TeleportRegex.Match(commandString);
            if (treffer.Success == false)
                return Fail(out command);

            try {
                command = new TeleportCommand(commandString) {
                    UnitID = treffer.Groups["unitId"].Success ? ParseInt(treffer.Groups["unitId"].Value) : 0,
                    LocationTo = ParseLocation(treffer.Groups["feld"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des Teleportbefehls gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
