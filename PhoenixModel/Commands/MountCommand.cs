using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
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
    /// Lässt Krieger auf ihre Reittiere aufsitzen oder Reiter absitzen.
    ///
    /// - "Sitze auf mit 300 Kriegern von Krieger 102 und 2 Heerführern"
    /// - "Sitze ab mit 300 Reitern von Reiter 205 und 2 Heerführern"
    ///
    /// Dabei entsteht ein neues Heer der anderen Gattung; das alte behält, was nicht mitkommt.
    /// Die Regeln stehen in <see cref="ReitRules"/>.
    ///
    /// Vermerkt wird der Vorgang wie in der Altanwendung: das Reiterheer trägt #AGV mit der Nummer
    /// des Kriegerheeres, das Kriegerheer #AGZ mit der Nummer des Reiterheeres.
    /// </summary>
    public class MountCommand : BaseCommand, IPhoenixCommand {

        public enum Modus { aufsitzen, absitzen }

        public Modus Mode { get; set; }
        public int UnitId { get; set; }
        public int Anzahl { get; set; }
        public int Heerführer { get; set; }

        /// <summary>Das Heer, aus dem heraus auf- oder abgesessen wird</summary>
        public TruppenSpielfigur? Heer { get; set; } = null;

        /// <summary>Das dabei entstandene Heer, erst nach der Ausführung gesetzt</summary>
        public TruppenSpielfigur? Entstanden { get; private set; } = null;

        private int _stärkeVorher, _pferdeVorher, _hfVorher, _bpVorher, _bpMaxVorher, _rpVorher;
        private string? _befehlVorher = null;

        public MountCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            string was = Mode == Modus.absitzen ? "Reitern" : "Kriegern";
            string richtung = Mode == Modus.absitzen ? "ab" : "auf";
            return $"Sitze {richtung} mit {Anzahl} {was} von {UnitId} und {Heerführer} Heerführern";
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            if (selectable is not TruppenSpielfigur truppe)
                return false;
            if (SchifffahrtsRules.IstEingeschifft(truppe))
                return false;
            return Mode == Modus.absitzen
                ? truppe.BaseTyp == FigurType.Reiter && truppe.staerke > 0
                : truppe.BaseTyp == FigurType.Krieger && truppe.Pferde > 0;
        }

        private TruppenSpielfigur? BestimmeHeer() {
            if (Heer != null)
                return Heer;
            if (_Selectable is TruppenSpielfigur ausgewählt && (UnitId == 0 || ausgewählt.Nummer == UnitId))
                return ausgewählt;
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(truppe => truppe.Nummer == UnitId);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var heer = BestimmeHeer();
            if (heer == null)
                return new CommandResultError("Das Heer wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Heer bestimmen.", this);

            var geprüft = Mode == Modus.absitzen
                ? ReitRules.PrüfeAbsitzen(heer, Anzahl, Heerführer)
                : ReitRules.PrüfeAufsitzen(heer, Anzahl, Heerführer);

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

            var heer = BestimmeHeer();
            if (heer == null)
                return new CommandResultError("Das Heer wurde nicht gefunden", CommandString, this);

            bool aufsitzen = Mode == Modus.aufsitzen;
            var zielGattung = aufsitzen ? FigurType.Reiter : FigurType.Krieger;
            int? nummer = HeeresRules.FindeFreieNummer(zielGattung);
            if (nummer == null)
                return new CommandResultError("Es ist keine Nummer mehr frei", CommandString, this);

            TruppenSpielfigur neu = aufsitzen ? new Reiter() : new Krieger();

            Heer = heer;
            _stärkeVorher = heer.staerke;
            _pferdeVorher = heer.Pferde;
            _hfVorher = heer.hf;
            _bpVorher = heer.bp;
            _bpMaxVorher = heer.bp_max;
            _rpVorher = heer.rp;
            _befehlVorher = heer.Befehl_bew;

            var standort = HeeresRules.GetStandort(heer);
            neu.Nummer = nummer.Value;
            neu.gf_von = standort.gf;
            neu.kf_von = standort.kf;
            neu.gf_nach = 0;
            neu.kf_nach = 0;
            neu.ph_xy = heer.ph_xy;
            neu.hoehenstufen = heer.hoehenstufen;
            neu.Garde = heer.Garde;
            neu.staerke = Anzahl;
            neu.hf = Heerführer;
            // beim Absitzen führen die Krieger ihre Reittiere am Zügel mit (Regelwerk 1.1.3)
            neu.Pferde = aufsitzen ? 0 : Anzahl;

            neu.bp_max = BewegungsRules.BerechneBewegungspunkte(neu);
            // wer sich in diesem Zug schon bewegt hat, gewinnt durch den Wechsel keine Strecke dazu
            neu.bp = heer.gf_nach > 0 ? 0 : neu.bp_max;
            neu.rp = SpielfigurRules.BerechneRaumpunkte(neu);

            heer.staerke -= Anzahl;
            heer.hf -= Heerführer;
            if (aufsitzen)
                heer.Pferde -= Anzahl;
            heer.bp_max = BewegungsRules.BerechneBewegungspunkte(heer);
            heer.bp = Math.Min(heer.bp, heer.bp_max);
            heer.rp = SpielfigurRules.BerechneRaumpunkte(heer);

            // das Reiterheer trägt die Nummer des Kriegerheeres und umgekehrt
            var reiterheer = aufsitzen ? neu : heer;
            var kriegerheer = aufsitzen ? heer : neu;
            reiterheer.Befehl_bew += $"#AGV:{kriegerheer.Nummer}";
            kriegerheer.Befehl_bew += $"#AGZ:{reiterheer.Nummer}";

            if (FügeDenZugdatenHinzu(neu) == false) {
                MacheRückgängig(heer);
                return new CommandResultError($"{neu.Bezeichner} liess sich nicht anlegen",
                    "Die Zugdaten sind nicht geladen.", this);
            }

            Entstanden = neu;
            if (neu is Database.IDatabaseTable neueTabelle)
                SharedData.StoreQueue.Insert(neueTabelle);
            if (heer is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneNeu(standort);

            IsExecuted = true;
            return new CommandResultSuccess(
                aufsitzen ? $"{Anzahl} Krieger sind aufgesessen" : $"{Anzahl} Reiter sind abgesessen",
                $"Daraus wurde {neu.Bezeichner} mit {neu.bp} von {neu.bp_max} Bewegungspunkten.", this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Das entstandene Heer verschwindet wieder, auch aus den Zugdaten - bleibt es liegen,
        /// steht es beim nächsten Laden doppelt da.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Heer == null || Entstanden == null)
                return new CommandResultError("Das lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein entstandenes Heer vermerkt.", this);

            var standort = HeeresRules.GetStandort(Entstanden);
            EntferneAusDenZugdaten(Entstanden);
            if (Entstanden is Database.IDatabaseTable entfernteTabelle)
                SharedData.StoreQueue.Delete(entfernteTabelle);

            MacheRückgängig(Heer);
            if (Heer is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneNeu(standort);

            string bezeichner = Entstanden.Bezeichner;
            Entstanden = null;
            IsExecuted = false;
            return new CommandResultSuccess(
                Mode == Modus.absitzen ? "Das Absitzen wurde zurückgenommen" : "Das Aufsitzen wurde zurückgenommen",
                $"{bezeichner} gibt es nicht mehr, {Heer.Bezeichner} ist wieder vollständig.", this);
        }

        private void MacheRückgängig(TruppenSpielfigur heer) {
            heer.staerke = _stärkeVorher;
            heer.Pferde = _pferdeVorher;
            heer.hf = _hfVorher;
            heer.bp = _bpVorher;
            heer.bp_max = _bpMaxVorher;
            heer.rp = _rpVorher;
            heer.Befehl_bew = _befehlVorher;
        }

        private static bool FügeDenZugdatenHinzu(TruppenSpielfigur figur) {
            figur.AssignToSelectedReich();
            switch (figur) {
                case Krieger krieger when SharedData.Krieger != null:
                    SharedData.Krieger.ReopenSharedData().Add(krieger);
                    return true;
                case Reiter reiter when SharedData.Reiter != null:
                    SharedData.Reiter.ReopenSharedData().Add(reiter);
                    return true;
                default:
                    return false;
            }
        }

        private static void EntferneAusDenZugdaten(TruppenSpielfigur figur) {
            switch (figur) {
                case Krieger krieger: SharedData.Krieger?.RemoveInstance(krieger); break;
                case Reiter reiter: SharedData.Reiter?.RemoveInstance(reiter); break;
            }
        }

        private static void ZeichneNeu(KleinfeldPosition standort) {
            var kleinfeld = KleinfeldView.GetKleinfeld(standort);
            if (kleinfeld != null)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
        }
    }

    public class MountCommandParser : SimpleParser {

        /// <summary>
        /// - "Sitze auf mit 300 Kriegern von Krieger 102 und 2 Heerführern"
        /// - "Sitze ab mit 300 Reitern von Reiter 205 und 2 Heerführern"
        /// </summary>
        private static readonly Regex MountRegex = new(
              @"^Sitze\s+(?:(?<auf>auf)|(?<ab>ab))\s+mit\s+(?<anzahl>\d+)\s+[A-Za-zÄÖÜäöüß]+"
            + @"\s+von\s+(?:\w+\s+)?(?<unitId>\d+)"
            + @"\s+und\s+(?<hf>\d+)\s+Heerführer(?:n|s)?$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = MountRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                command = new MountCommand(commandString) {
                    Mode = match.Groups["ab"].Success ? MountCommand.Modus.absitzen : MountCommand.Modus.aufsitzen,
                    Anzahl = ParseInt(match.Groups["anzahl"].Value),
                    UnitId = ParseInt(match.Groups["unitId"].Value),
                    Heerführer = ParseInt(match.Groups["hf"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MountCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
