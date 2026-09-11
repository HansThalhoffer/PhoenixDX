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
    /// Verschiebt Rüstgüter und Geld zwischen zwei Heeren auf derselben Gemark oder zwischen einem
    /// Heer und dem Reichsschatz.
    ///
    /// - "Verschiebe 100 Pferde von Krieger 101 zu Krieger 103"
    /// - "Verschiebe 2 Heerführer von Krieger 101 zu Krieger 103"
    /// - "Verschiebe 2000 Gold von Krieger 101 in den Reichsschatz"
    /// - "Verschiebe 2000 Gold aus dem Reichsschatz zu Krieger 101"
    ///
    /// Die Regeln stehen in <see cref="VerschiebeRules"/>.
    /// </summary>
    public class ShareCommand : BaseCommand, IPhoenixCommand {

        public enum Modus {
            zwischenHeeren,
            inDenReichsschatz,
            ausDemReichsschatz,
        }

        public Modus Mode { get; set; } = Modus.zwischenHeeren;
        public Verschiebbar Was { get; set; } = Verschiebbar.Gold;
        public int Menge { get; set; }
        public int QuelleUnitId { get; set; }
        public int ZielUnitId { get; set; }

        public TruppenSpielfigur? Quelle { get; set; } = null;
        public TruppenSpielfigur? Ziel { get; set; } = null;

        /// <summary>Der Stand des Reichsschatzes vor der Buchung, für das Zurücknehmen</summary>
        private int _reichsschatzVorher;
        private Schatzkammer? _schatz = null;

        public ShareCommand(string commandString) : base(commandString) {
        }

        public override string ToString() {
            string was = $"{Menge} {VerschiebeRules.GetBezeichnung(Was)}";
            return Mode switch {
                Modus.inDenReichsschatz => $"Verschiebe {was} von {QuelleUnitId} in den Reichsschatz",
                Modus.ausDemReichsschatz => $"Verschiebe {was} aus dem Reichsschatz zu {ZielUnitId}",
                _ => $"Verschiebe {was} von {QuelleUnitId} zu {ZielUnitId}",
            };
        }

        public override bool CanAppliedTo(ISelectable selectable) => selectable is TruppenSpielfigur;

        private TruppenSpielfigur? Finde(int nummer) {
            if (nummer == 0)
                return _Selectable as TruppenSpielfigur;
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .FirstOrDefault(truppe => truppe.Nummer == nummer);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult CheckPreconditions() {
            Quelle ??= Finde(QuelleUnitId);
            Ziel ??= Finde(ZielUnitId);

            var geprüft = Mode switch {
                Modus.inDenReichsschatz => VerschiebeRules.PrüfeZumReichsschatz(Quelle, Was, Menge),
                Modus.ausDemReichsschatz => VerschiebeRules.PrüfeVomReichsschatz(Ziel, Menge),
                _ => VerschiebeRules.PrüfeZwischenHeeren(Quelle, Ziel, Was, Menge),
            };

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

            switch (Mode) {
                case Modus.inDenReichsschatz:
                    _schatz = SchatzkammerRules.GetMonat(ZugView.AktuellerZug.Zug);
                    if (_schatz == null || Quelle == null)
                        return new CommandResultError("Die Schatzkammer hat keinen Stand für diesen Monat", CommandString, this);
                    _reichsschatzVorher = _schatz.Reichschatz;
                    VerschiebeRules.Ändere(Quelle, Was, -Menge);
                    _schatz.Reichschatz += Menge;
                    Speichere(Quelle);
                    SharedData.StoreQueue.Enqueue(_schatz);
                    break;

                case Modus.ausDemReichsschatz:
                    _schatz = SchatzkammerRules.GetMonat(ZugView.AktuellerZug.Zug);
                    if (_schatz == null || Ziel == null)
                        return new CommandResultError("Die Schatzkammer hat keinen Stand für diesen Monat", CommandString, this);
                    _reichsschatzVorher = _schatz.Reichschatz;
                    _schatz.Reichschatz -= Menge;
                    VerschiebeRules.Ändere(Ziel, Verschiebbar.Gold, Menge);
                    Speichere(Ziel);
                    SharedData.StoreQueue.Enqueue(_schatz);
                    break;

                default:
                    if (Quelle == null || Ziel == null)
                        return new CommandResultError("Es wurden nicht beide Heere gefunden", CommandString, this);
                    VerschiebeRules.Ändere(Quelle, Was, -Menge);
                    VerschiebeRules.Ändere(Ziel, Was, Menge);
                    if (Was == Verschiebbar.Heerführer || Was == Verschiebbar.Pferde
                        || Was == Verschiebbar.LeichteKatapulte || Was == Verschiebbar.SchwereKatapulte) {
                        // die Zusammensetzung hat sich geändert, damit auch der Raumbedarf
                        Quelle.rp = SpielfigurRules.BerechneRaumpunkte(Quelle);
                        Ziel.rp = SpielfigurRules.BerechneRaumpunkte(Ziel);
                    }
                    Speichere(Quelle);
                    Speichere(Ziel);
                    break;
            }

            IsExecuted = true;
            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Verschoben wird nur, nie angelegt oder gelöscht - das Zurücknehmen bucht also einfach
        /// in die andere Richtung.
        /// </summary>
        public override CommandResult UndoCommand() {
            switch (Mode) {
                case Modus.inDenReichsschatz:
                    if (_schatz == null || Quelle == null)
                        return NichtsZumZurücknehmen();
                    VerschiebeRules.Ändere(Quelle, Was, Menge);
                    _schatz.Reichschatz = _reichsschatzVorher;
                    Speichere(Quelle);
                    SharedData.StoreQueue.Enqueue(_schatz);
                    break;

                case Modus.ausDemReichsschatz:
                    if (_schatz == null || Ziel == null)
                        return NichtsZumZurücknehmen();
                    VerschiebeRules.Ändere(Ziel, Verschiebbar.Gold, -Menge);
                    _schatz.Reichschatz = _reichsschatzVorher;
                    Speichere(Ziel);
                    SharedData.StoreQueue.Enqueue(_schatz);
                    break;

                default:
                    if (Quelle == null || Ziel == null)
                        return NichtsZumZurücknehmen();
                    VerschiebeRules.Ändere(Ziel, Was, -Menge);
                    VerschiebeRules.Ändere(Quelle, Was, Menge);
                    Quelle.rp = SpielfigurRules.BerechneRaumpunkte(Quelle);
                    Ziel.rp = SpielfigurRules.BerechneRaumpunkte(Ziel);
                    Speichere(Quelle);
                    Speichere(Ziel);
                    break;
            }

            IsExecuted = false;
            return new CommandResultSuccess("Das Verschieben wurde zurückgenommen", ToString(), this);
        }

        private CommandResult NichtsZumZurücknehmen()
            => new CommandResultError("Das lässt sich nicht zurücknehmen",
                "Zu diesem Befehl ist kein ausgeführter Vorgang vermerkt.", this);

        private void Speichere(TruppenSpielfigur truppe) {
            if (truppe is Database.IDatabaseTable tabelle)
                Update(tabelle, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
        }
    }

    public class ShareCommandParser : SimpleParser {

        /// <summary>
        /// - "Verschiebe 100 Pferde von Krieger 101 zu Krieger 103"
        /// - "Verschiebe 2000 Gold von Krieger 101 in den Reichsschatz"
        /// - "Verschiebe 2000 Gold aus dem Reichsschatz zu Krieger 101"
        /// </summary>
        private static readonly Regex ShareRegex = new(
              @"^Verschiebe\s+(?<menge>\d+)\s+(?<was>Gold|Goldstücke|Kampfeinnahmen|Heerführer(?:n)?|Pferde(?:n)?"
            + @"|leichte[n]?\s+Katapulte[n]?|schwere[n]?\s+Katapulte[n]?)\s+"
            + @"(?:von\s+(?:\w+\s+)?(?<quelle>\d+)\s+(?:zu\s+(?:\w+\s+)?(?<ziel>\d+)|in\s+den\s+Reichsschatz)"
            + @"|aus\s+dem\s+Reichsschatz\s+zu\s+(?:\w+\s+)?(?<ausZiel>\d+))$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            var match = ShareRegex.Match(commandString);
            if (match.Success == false)
                return Fail(out command);

            try {
                var was = ParseWas(match.Groups["was"].Value);
                if (was == null)
                    return Fail(out command);

                var modus = match.Groups["ausZiel"].Success ? ShareCommand.Modus.ausDemReichsschatz
                          : match.Groups["ziel"].Success ? ShareCommand.Modus.zwischenHeeren
                          : ShareCommand.Modus.inDenReichsschatz;

                command = new ShareCommand(commandString) {
                    Mode = modus,
                    Was = was.Value,
                    Menge = ParseInt(match.Groups["menge"].Value),
                    QuelleUnitId = match.Groups["quelle"].Success ? ParseInt(match.Groups["quelle"].Value) : 0,
                    ZielUnitId = match.Groups["ausZiel"].Success ? ParseInt(match.Groups["ausZiel"].Value)
                               : match.Groups["ziel"].Success ? ParseInt(match.Groups["ziel"].Value) : 0,
                };
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des ShareCommand gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return true;
        }

        public static Verschiebbar? ParseWas(string eingabe) {
            string normiert = Regex.Replace(eingabe.ToLower(), @"\s+", " ").Trim();
            return normiert switch {
                "gold" or "goldstücke" => Verschiebbar.Gold,
                "kampfeinnahmen" => Verschiebbar.Kampfeinnahmen,
                "heerführer" or "heerführern" => Verschiebbar.Heerführer,
                "pferde" or "pferden" => Verschiebbar.Pferde,
                "leichte katapulte" or "leichten katapulten" or "leichte katapulten" => Verschiebbar.LeichteKatapulte,
                "schwere katapulte" or "schweren katapulten" or "schwere katapulten" => Verschiebbar.SchwereKatapulte,
                _ => null,
            };
        }
    }
}
