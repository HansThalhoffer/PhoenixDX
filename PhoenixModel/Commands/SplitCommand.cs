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
    /// Spaltet ein neues Heer von einem bestehenden ab.
    ///
    /// Beispiel: "Spalte von Reiter 220 ab 400 Reiter mit 4 Heerführern"
    ///
    /// Die Regeln stehen in <see cref="HeeresRules"/>: beide Hälften brauchen einen Heerführer und
    /// mindestens 100 Raumpunkte, und gemischt wird nichts. Das neue Heer bekommt die nächste freie
    /// Nummer seiner Gattung und steht dort, wo das alte am Ende seiner Bewegung steht.
    /// </summary>
    public class SplitCommand : BaseCommand, IPhoenixCommand {

        public FigurType Figur { get; set; }
        public int OriginalUnitId { get; set; } = 0;

        /// <summary>
        /// Was abgegeben wird. Der Textbefehl füllt Stärke und Heerführer; über die Oberfläche
        /// lässt sich alles angeben.
        /// </summary>
        public Heeresanteil Anteil { get; set; } = new();

        /// <summary>Das Heer, von dem abgespalten wird</summary>
        public TruppenSpielfigur? Heer { get; set; } = null;

        /// <summary>Das abgespaltene Heer, erst nach der Ausführung gesetzt</summary>
        public TruppenSpielfigur? Abgespalten { get; private set; } = null;

        /// <summary>Der Inhalt von spaltetab vor der Teilung, für das Zurücknehmen</summary>
        private string? _spaltetabVorher = null;

        public SplitCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            string nummer = Abgespalten == null ? string.Empty : $" als {Abgespalten.Nummer}";
            return $"Spalte von {Figur} {OriginalUnitId} ab {Anteil}{nummer}";
        }

        public override bool CanAppliedTo(ISelectable selectable) {
            return selectable is Spielfigur figur && figur.CanSplit();
        }

        /// <summary>
        /// Sucht das Heer, von dem abgespalten werden soll
        /// </summary>
        private TruppenSpielfigur? BestimmeHeer() {
            if (Heer != null)
                return Heer;
            if (_Selectable is TruppenSpielfigur ausgewählt && (OriginalUnitId == 0 || ausgewählt.Nummer == OriginalUnitId))
                return ausgewählt;
            if (OriginalUnitId == 0)
                return null;
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(truppe => truppe.Nummer == OriginalUnitId);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            var heer = BestimmeHeer();
            if (heer == null)
                return new CommandResultError("Das zu teilende Heer wurde nicht gefunden",
                    $"Zu '{CommandString}' liess sich kein Heer bestimmen.", this);

            var geprüft = HeeresRules.PrüfeTeilung(heer, Anteil);
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
                return new CommandResultError("Das zu teilende Heer wurde nicht gefunden", CommandString, this);

            int? nummer = HeeresRules.FindeFreieNummer(heer);
            if (nummer == null)
                return new CommandResultError("Es ist keine Nummer mehr frei", CommandString, this);

            var neu = ErzeugeGleichartig(heer);
            if (neu == null)
                return new CommandResultError($"Aus {heer.Bezeichner} lässt sich kein neues Heer bilden",
                    "Nur Krieger, Reiter, Schiffe und Kreaturen können geteilt werden.", this);

            Heer = heer;
            _spaltetabVorher = heer.spaltetab;

            var standort = HeeresRules.GetStandort(heer);
            neu.Nummer = nummer.Value;
            neu.gf_von = standort.gf;
            neu.kf_von = standort.kf;
            // das neue Heer hat sich noch nicht bewegt
            neu.gf_nach = 0;
            neu.kf_nach = 0;
            neu.ph_xy = heer.ph_xy;
            neu.bp = heer.bp;
            neu.bp_max = heer.bp_max;
            neu.hoehenstufen = heer.hoehenstufen;
            neu.auf_Flotte = heer.auf_Flotte;
            neu.Garde = heer.Garde;

            neu.staerke = Anteil.Stärke;
            neu.hf = Anteil.Heerführer;
            neu.LKP = Anteil.LKP;
            neu.SKP = Anteil.SKP;
            neu.Pferde = Anteil.Pferde;
            neu.GS = Anteil.GS;
            neu.Kampfeinnahmen = Anteil.Kampfeinnahmen;
            neu.rp = SpielfigurRules.BerechneRaumpunkte(neu);
            neu.spaltetab = $"#ASV:{heer.Nummer}";

            ZieheAb(heer, Anteil);
            heer.spaltetab += $"#SA:{neu.Nummer}";
            heer.rp = SpielfigurRules.BerechneRaumpunkte(heer);

            if (FügeDenZugdatenHinzu(neu) == false) {
                // nichts ist passiert ausser an der Vorlage - das wird zurückgedreht
                GibZurück(heer, Anteil);
                heer.spaltetab = _spaltetabVorher;
                heer.rp = SpielfigurRules.BerechneRaumpunkte(heer);
                return new CommandResultError($"{neu.Bezeichner} liess sich nicht anlegen",
                    "Die Zugdaten sind nicht geladen.", this);
            }

            Abgespalten = neu;
            if (neu is Database.IDatabaseTable neueTabelle)
                SharedData.StoreQueue.Insert(neueTabelle);
            if (heer is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneStandortNeu(standort);

            IsExecuted = true;
            return new CommandResultSuccess($"{heer.Bezeichner} wurde geteilt",
                $"Abgespalten wurde {neu.Bezeichner} mit {Anteil}.", this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Das abgespaltene Heer verschwindet wieder, und das ursprüngliche bekommt genau zurück,
        /// was es abgegeben hat. Wichtig ist, dass das neue Heer dabei auch aus den Zugdaten
        /// herausfliegt - bleibt es liegen, steht es beim nächsten Laden doppelt da.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Heer == null || Abgespalten == null)
                return new CommandResultError("Die Teilung lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein abgespaltenes Heer vermerkt.", this);

            var standort = HeeresRules.GetStandort(Abgespalten);

            EntferneAusDenZugdaten(Abgespalten);
            if (Abgespalten is Database.IDatabaseTable entfernteTabelle)
                SharedData.StoreQueue.Delete(entfernteTabelle);

            GibZurück(Heer, Anteil);
            Heer.spaltetab = _spaltetabVorher;
            Heer.rp = SpielfigurRules.BerechneRaumpunkte(Heer);

            if (Heer is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            ZeichneStandortNeu(standort);

            string bezeichner = Abgespalten.Bezeichner;
            Abgespalten = null;
            IsExecuted = false;
            return new CommandResultSuccess("Die Teilung wurde zurückgenommen",
                $"{bezeichner} gibt es nicht mehr, {Heer.Bezeichner} ist wieder vollständig.", this);
        }

        private static void ZieheAb(TruppenSpielfigur heer, Heeresanteil anteil) {
            heer.staerke -= anteil.Stärke;
            heer.hf -= anteil.Heerführer;
            heer.LKP -= anteil.LKP;
            heer.SKP -= anteil.SKP;
            heer.Pferde -= anteil.Pferde;
            heer.GS -= anteil.GS;
            heer.Kampfeinnahmen -= anteil.Kampfeinnahmen;
        }

        private static void GibZurück(TruppenSpielfigur heer, Heeresanteil anteil) {
            heer.staerke += anteil.Stärke;
            heer.hf += anteil.Heerführer;
            heer.LKP += anteil.LKP;
            heer.SKP += anteil.SKP;
            heer.Pferde += anteil.Pferde;
            heer.GS += anteil.GS;
            heer.Kampfeinnahmen += anteil.Kampfeinnahmen;
        }

        /// <summary>
        /// Legt eine leere Figur derselben Gattung an
        /// </summary>
        private static TruppenSpielfigur? ErzeugeGleichartig(TruppenSpielfigur vorbild) => vorbild switch {
            Krieger => new Krieger(),
            Reiter => new Reiter(),
            Schiffe => new Schiffe(),
            Kreaturen => new Kreaturen(),
            _ => null,
        };

        /// <summary>
        /// Nimmt das neue Heer in die Zugdaten auf.
        ///
        /// Die Sammlungen werden nach dem Laden für Ergänzungen geschlossen, deshalb müssen sie
        /// vorher wieder geöffnet werden - siehe <see cref="BlockingCollectionExtension"/>.
        /// </summary>
        private static bool FügeDenZugdatenHinzu(TruppenSpielfigur figur) {
            figur.AssignToSelectedReich();
            switch (figur) {
                case Krieger krieger when SharedData.Krieger != null:
                    SharedData.Krieger.ReopenSharedData().Add(krieger);
                    return true;
                case Reiter reiter when SharedData.Reiter != null:
                    SharedData.Reiter.ReopenSharedData().Add(reiter);
                    return true;
                case Schiffe schiff when SharedData.Schiffe != null:
                    SharedData.Schiffe.ReopenSharedData().Add(schiff);
                    return true;
                case Kreaturen kreatur when SharedData.Kreaturen != null:
                    SharedData.Kreaturen.ReopenSharedData().Add(kreatur);
                    return true;
                default:
                    return false;
            }
        }

        private static void EntferneAusDenZugdaten(TruppenSpielfigur figur) {
            switch (figur) {
                case Krieger krieger: SharedData.Krieger?.RemoveInstance(krieger); break;
                case Reiter reiter: SharedData.Reiter?.RemoveInstance(reiter); break;
                case Schiffe schiff: SharedData.Schiffe?.RemoveInstance(schiff); break;
                case Kreaturen kreatur: SharedData.Kreaturen?.RemoveInstance(kreatur); break;
            }
        }

        private static void ZeichneStandortNeu(KleinfeldPosition standort) {
            var kleinfeld = KleinfeldView.GetKleinfeld(standort);
            if (kleinfeld != null)
                SharedData.UpdateQueue.Enqueue(kleinfeld);
        }
    }

    public class SplitCommandParser : SimpleParser {

        /// <summary>
        /// "Spalte von Reiter 220 ab 400 Reiter mit 4 Heerführern"
        ///
        /// Stärke und Heerführer sind Pflicht. Pferde und Fernkampfwaffen lassen sich anhängen:
        /// "... und 100 Pferden und 2 leichten Fernkampfwaffen"
        /// </summary>
        private static readonly Regex SplitRegex = new(
              @"^Spalte\s+von\s+(?<figur>\w+)\s+(?<originalId>\d+)\s+ab\s+(?<staerke>\d+)(\s+[A-Za-zÄÖÜäöüß]+)?"
            + @"\s+mit\s+(?<hf>\d+)\s+Heerführer(?:n|s)?"
            + @"(?:\s+und\s+(?<pferde>\d+)\s+Pferde[n]?)?"
            + @"(?:\s+und\s+(?<lkp>\d+)\s+leichten?\s+(?:Fernkampfwaffen?|Katapulten?))?"
            + @"(?:\s+und\s+(?<skp>\d+)\s+schweren?\s+(?:Fernkampfwaffen?|Katapulten?))?$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = SplitRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                command = new SplitCommand(commandString) {
                    Figur = ParseUnitType(match.Groups["figur"].Value),
                    OriginalUnitId = ParseInt(match.Groups["originalId"].Value),
                    Anteil = new Heeresanteil {
                        Stärke = ParseInt(match.Groups["staerke"].Value),
                        Heerführer = ParseInt(match.Groups["hf"].Value),
                        Pferde = match.Groups["pferde"].Success ? ParseInt(match.Groups["pferde"].Value) : 0,
                        LKP = match.Groups["lkp"].Success ? ParseInt(match.Groups["lkp"].Value) : 0,
                        SKP = match.Groups["skp"].Success ? ParseInt(match.Groups["skp"].Value) : 0,
                    },
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des SplitCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }
    }
}
