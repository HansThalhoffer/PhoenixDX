using PhoenixModel.Commands.Parser;
using PhoenixModel.Extensions;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Vereinigt zwei Heere derselben Gattung auf derselben Gemark.
    ///
    /// Beispiel: "Vereinige Reiter 221 mit 244" - 221 bleibt bestehen und nimmt alles von 244 auf.
    ///
    /// Das aufgenommene Heer wird nicht gelöscht, sondern geleert: es behält seine Nummer und einen
    /// Vermerk in fusmit, damit die Spielleitung den Vorgang nachvollziehen kann. Beim Zugübergang
    /// verschwindet es dann von selbst, weil es keinen Heerführer mehr hat - siehe
    /// <see cref="ZugendeRules.IstAufgelöst"/>.
    /// </summary>
    public class MergeCommand : BaseCommand, IPhoenixCommand {

        public FigurType Figur { get; set; }

        /// <summary>Die Nummer des Heeres, das bestehen bleibt</summary>
        public int BleibtUnitId { get; set; }

        /// <summary>Die Nummer des Heeres, das darin aufgeht</summary>
        public int GehtAufUnitId { get; set; }

        /// <summary>Das Heer, das bestehen bleibt</summary>
        public TruppenSpielfigur? Bleibt { get; set; } = null;

        /// <summary>Das Heer, das aufgenommen wird</summary>
        public TruppenSpielfigur? GehtAuf { get; set; } = null;

        private Fusionszustand? _zustandBleibt = null;
        private Fusionszustand? _zustandGehtAuf = null;

        public MergeCommand(string commandString) : base(commandString) {
        }

        public override string ToString() => $"Vereinige {Figur} {BleibtUnitId} mit {GehtAufUnitId}";

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable is TruppenSpielfigur truppe && truppe.CanFusion();
        }

        /// <summary>
        /// Sucht die beiden Heere. Das aufzunehmende ergibt sich aus der Nummer, das bleibende aus
        /// der Auswahl oder ebenfalls aus seiner Nummer.
        /// </summary>
        private (TruppenSpielfigur? bleibt, TruppenSpielfigur? gehtAuf) BestimmeHeere() {
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().ToList();

            var bleibt = Bleibt;
            if (bleibt == null && _Selectable is TruppenSpielfigur ausgewählt
                && (BleibtUnitId == 0 || ausgewählt.Nummer == BleibtUnitId))
                bleibt = ausgewählt;
            bleibt ??= armee.FirstOrDefault(truppe => truppe.Nummer == BleibtUnitId);

            var gehtAuf = GehtAuf ?? armee.FirstOrDefault(truppe => truppe.Nummer == GehtAufUnitId);
            return (bleibt, gehtAuf);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var (bleibt, gehtAuf) = BestimmeHeere();
            if (bleibt == null || gehtAuf == null)
                return new CommandResultError("Die Heere für die Fusion wurden nicht gefunden",
                    $"Zu '{CommandString}' liessen sich nicht beide Heere bestimmen.", this);

            var geprüft = HeeresRules.PrüfeFusion(bleibt, gehtAuf);
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

            var (bleibt, gehtAuf) = BestimmeHeere();
            if (bleibt == null || gehtAuf == null)
                return new CommandResultError("Die Heere für die Fusion wurden nicht gefunden", CommandString, this);

            Bleibt = bleibt;
            GehtAuf = gehtAuf;
            _zustandBleibt = new Fusionszustand(bleibt);
            _zustandGehtAuf = new Fusionszustand(gehtAuf);

            bleibt.staerke += gehtAuf.staerke;
            bleibt.hf += gehtAuf.hf;
            bleibt.LKP += gehtAuf.LKP;
            bleibt.SKP += gehtAuf.SKP;
            bleibt.Pferde += gehtAuf.Pferde;
            bleibt.GS += gehtAuf.GS;
            bleibt.Kampfeinnahmen += gehtAuf.Kampfeinnahmen;

            // das vereinigte Heer kommt nicht weiter als der langsamere Teil und schleppt die
            // bereits überwundenen Höhenstufen des anstrengenderen Weges mit
            bleibt.bp = Math.Min(bleibt.bp, gehtAuf.bp);
            bleibt.hoehenstufen = Math.Max(bleibt.hoehenstufen, gehtAuf.hoehenstufen);
            bleibt.rp = SpielfigurRules.BerechneRaumpunkte(bleibt);
            bleibt.fusmit += $"#FM:{gehtAuf.Nummer}";

            Leere(gehtAuf);
            gehtAuf.fusmit = $"#FZ:{bleibt.Nummer}{_zustandGehtAuf.Fusmit}";
            gehtAuf.rp = SpielfigurRules.BerechneRaumpunkte(gehtAuf);

            Speichere(bleibt);
            Speichere(gehtAuf);
            ZeichneStandortNeu(HeeresRules.GetStandort(bleibt));

            IsExecuted = true;
            return new CommandResultSuccess($"{gehtAuf.Bezeichner} ist in {bleibt.Bezeichner} aufgegangen",
                $"{bleibt.Bezeichner} hat jetzt {bleibt.staerke} Stärke und {bleibt.hf} Heerführer.", this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Beide Heere bekommen genau den Stand zurück, den sie vor der Fusion hatten. Das
        /// aufgenommene Heer wurde nur geleert und nie gelöscht, deshalb genügt das Zurückschreiben
        /// der Werte - es kann dabei nichts doppelt entstehen.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Bleibt == null || GehtAuf == null || _zustandBleibt == null || _zustandGehtAuf == null)
                return new CommandResultError("Die Fusion lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein Zustand vor der Fusion vermerkt.", this);

            _zustandBleibt.Wiederherstellen(Bleibt);
            _zustandGehtAuf.Wiederherstellen(GehtAuf);

            Speichere(Bleibt);
            Speichere(GehtAuf);
            ZeichneStandortNeu(HeeresRules.GetStandort(Bleibt));

            IsExecuted = false;
            return new CommandResultSuccess("Die Fusion wurde zurückgenommen",
                $"{GehtAuf.Bezeichner} steht wieder für sich.", this);
        }

        private static void Leere(TruppenSpielfigur truppe) {
            truppe.staerke = 0;
            truppe.hf = 0;
            truppe.LKP = 0;
            truppe.SKP = 0;
            truppe.Pferde = 0;
            truppe.GS = 0;
            truppe.Kampfeinnahmen = 0;
            truppe.Chars = string.Empty;
        }

        private void Speichere(TruppenSpielfigur truppe) {
            if (truppe is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
        }

        private static void ZeichneStandortNeu(KleinfeldPosition standort) {
            var kleinfeld = KleinfeldView.GetKleinfeld(standort);
            if (kleinfeld != null)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
        }

        /// <summary>
        /// Der Stand eines Heeres vor der Fusion, damit sich der Befehl sauber zurücknehmen lässt
        /// </summary>
        private class Fusionszustand {
            private readonly int _staerke, _hf, _lkp, _skp, _pferde, _gs, _kampfeinnahmen, _bp, _hoehenstufen, _rp;
            private readonly string? _fusmit;
            private readonly string? _chars;

            public string? Fusmit => _fusmit;

            public Fusionszustand(TruppenSpielfigur truppe) {
                _staerke = truppe.staerke;
                _hf = truppe.hf;
                _lkp = truppe.LKP;
                _skp = truppe.SKP;
                _pferde = truppe.Pferde;
                _gs = truppe.GS;
                _kampfeinnahmen = truppe.Kampfeinnahmen;
                _bp = truppe.bp;
                _hoehenstufen = truppe.hoehenstufen;
                _rp = truppe.rp;
                _fusmit = truppe.fusmit;
                _chars = truppe.Chars;
            }

            public void Wiederherstellen(TruppenSpielfigur truppe) {
                truppe.staerke = _staerke;
                truppe.hf = _hf;
                truppe.LKP = _lkp;
                truppe.SKP = _skp;
                truppe.Pferde = _pferde;
                truppe.GS = _gs;
                truppe.Kampfeinnahmen = _kampfeinnahmen;
                truppe.bp = _bp;
                truppe.hoehenstufen = _hoehenstufen;
                truppe.rp = _rp;
                truppe.fusmit = _fusmit;
                truppe.Chars = _chars;
            }
        }
    }

    public class MergeCommandParser : SimpleParser {

        /// <summary>
        /// "Vereinige Reiter 221 mit 244" - das erstgenannte Heer bleibt bestehen.
        /// </summary>
        private static readonly Regex MergeRegex = new(
              @"^Vereinige\s+(?<figur>\w+)\s+(?<bleibt>\d+)\s+(?:mit|und)\s+(?:\w+\s+)?(?<gehtAuf>\d+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = MergeRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                command = new MergeCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["figur"].Value),
                    BleibtUnitId = ParseInt(match.Groups["bleibt"].Value),
                    GehtAufUnitId = ParseInt(match.Groups["gehtAuf"].Value),
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des MergeCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
