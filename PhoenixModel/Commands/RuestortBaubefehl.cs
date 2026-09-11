using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Extensions;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Gemeinsamer Unterbau für Ausbau und Reparatur eines Rüstortes.
    ///
    /// Beides läuft gleich ab: es entsteht ein Eintrag in der Tabelle ruestung_ruestorte für den
    /// laufenden Zug, der besagt, wieviele Baupunkte in diesem Monat gebaut werden. Unterschiedlich
    /// ist nur, in welche Spalte die Punkte gehen - BP_up beim Ausbau, BP_rep bei der Reparatur -
    /// und welche Regeln <see cref="RuestortRules"/> dafür anlegt.
    ///
    /// Ausgeführt wird das Vorhaben nicht sofort: Rüstorte "können generell nur am Ende eines
    /// Monats errichtet, ausgebaut oder repariert werden" (Regelwerk 1.5). Der Befehl hält also
    /// den Auftrag fest, nicht das Ergebnis.
    /// </summary>
    public abstract class RuestortBaubefehl : BaseCommand, IPhoenixCommand {

        public KleinfeldPosition? Location { get; set; } = null;
        public int Baupunkte { get; set; } = 0;

        /// <summary>Ausbau oder Reparatur - bestimmt Regeln und Zielspalte</summary>
        protected abstract Bauart Bauart { get; }

        /// <summary>Der angelegte oder ergänzte Auftrag, erst nach der Ausführung gesetzt</summary>
        protected RuestungRuestorte? Auftrag { get; private set; } = null;

        /// <summary>Der Auftrag gab es vorher noch nicht und muss beim Zurücknehmen ganz weg</summary>
        private bool _auftragIstNeu = false;
        private int _bpUpVorher, _bpRepVorher;

        protected RuestortBaubefehl(string commandString) : base(commandString) {
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable is KleinFeld kleinfeld && kleinfeld.Gebäude != null;
        }

        /// <summary>
        /// Das Kleinfeld, an dem gebaut wird
        /// </summary>
        protected KleinFeld? BestimmeKleinfeld() {
            if (Location != null)
                return KleinfeldView.GetKleinfeld(Location);
            return _Selectable as KleinFeld;
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var kleinfeld = BestimmeKleinfeld();
            if (kleinfeld == null)
                return new CommandResultError("Das Kleinfeld wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Feld bestimmen.", this);

            var geprüft = RuestortRules.Prüfe(kleinfeld, Bauart, Baupunkte);
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

            var kleinfeld = BestimmeKleinfeld();
            if (kleinfeld == null || SharedData.RuestungRuestorte == null)
                return new CommandResultError("Die Zugdaten sind nicht geladen", CommandString, this);

            // für ein Feld gibt es nur einen Auftrag je Zug; ein zweiter Befehl ergänzt ihn
            var auftrag = SharedData.RuestungRuestorte
                .FirstOrDefault(a => a.gf == kleinfeld.gf && a.kf == kleinfeld.kf);
            _auftragIstNeu = auftrag == null;

            if (auftrag == null) {
                auftrag = new RuestungRuestorte {
                    gf = kleinfeld.gf,
                    kf = kleinfeld.kf,
                    ZugMonat = ZugView.AktuellerZug.Zug,
                };
            }

            _bpUpVorher = auftrag.BP_up;
            _bpRepVorher = auftrag.BP_rep;

            if (Bauart == Bauart.Reparatur)
                auftrag.BP_rep += Baupunkte;
            else
                auftrag.BP_up += Baupunkte;

            if (_auftragIstNeu) {
                SharedData.RuestungRuestorte.ReopenSharedData().Add(auftrag);
                SharedData.StoreQueue.Insert(auftrag);
            }
            else {
                SharedData.StoreQueue.Enqueue(auftrag);
            }

            Auftrag = auftrag;
            SharedData.UpdateQueue.Enqueue(kleinfeld);
            ProgramView.Update(kleinfeld, ViewEventArgs.ViewEventType.UpdateKleinfeld);

            IsExecuted = true;
            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// War der Auftrag vorher schon da, werden nur die Baupunkte zurückgesetzt; war er neu,
        /// verschwindet er ganz - sonst bliebe ein Auftrag über null Baupunkte stehen.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Auftrag == null)
                return new CommandResultError("Das lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein Bauauftrag vermerkt.", this);

            if (_auftragIstNeu) {
                SharedData.RuestungRuestorte?.RemoveInstance(Auftrag);
                SharedData.StoreQueue.Delete(Auftrag);
            }
            else {
                Auftrag.BP_up = _bpUpVorher;
                Auftrag.BP_rep = _bpRepVorher;
                SharedData.StoreQueue.Enqueue(Auftrag);
            }

            var kleinfeld = BestimmeKleinfeld();
            if (kleinfeld != null) {
                SharedData.UpdateQueue.Enqueue(kleinfeld);
                ProgramView.Update(kleinfeld, ViewEventArgs.ViewEventType.UpdateKleinfeld);
            }

            Auftrag = null;
            IsExecuted = false;
            return new CommandResultSuccess(
                Bauart == Bauart.Reparatur ? "Die Reparatur wurde zurückgenommen" : "Der Ausbau wurde zurückgenommen",
                $"Auf {Location?.CreateBezeichner()} wird nicht mehr gebaut.", this);
        }
    }
}
