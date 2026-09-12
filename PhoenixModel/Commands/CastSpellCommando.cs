using PhoenixModel.Commands.Parser;
using PhoenixModel.Database;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text.RegularExpressions;

namespace PhoenixModel.Commands {

    /// <summary>
    /// Gibt für einen Zauberer einen Zauberspruch auf.
    ///
    /// Die Anwendung wertet nichts aus: sie prüft den Spruch, zieht die Zauberkraftpunkte ab und
    /// hält den Befehl in den Spalten Befehl_magie, Befehl_bannt und Befehl_Teleport fest. Die
    /// Spielleitung liest ihn von dort und würfelt.
    ///
    /// - "Zauberer 501 errichtet eine magische Wand im Nordosten von 701/33"
    /// - "Zauberer 501 reisst die magische Wand im Westen von 701/33 ein"
    /// - "Zauberer 501 bannt 2000 Raumpunkte auf 701/33"
    /// - "Zauberer 501 fordert 701/33 zum Zauberduell"
    /// - "Zauberer 501 teleportiert nach 755/22 mit Krieger 101, Reiter 203"
    ///
    /// Die Regeln stehen in <see cref="ZaubereiRules"/>, das Format der Befehle in
    /// <see cref="Zauberbefehl"/> und <see cref="Bannbefehl"/>.
    /// </summary>
    public class CastSpellCommando : BaseCommand, IPhoenixCommand {

        public CastSpellCommando(string commandString) : base(commandString) {
        }

        /// <summary>Die Nummer des Zauberers, 0 wenn die Auswahl gilt</summary>
        public int UnitID { get; set; } = 0;

        /// <summary>Der Spruch</summary>
        public Zauberspruch Spell { get; set; } = Zauberspruch.Keiner;

        /// <summary>Die betroffene Gemark</summary>
        public KleinfeldPosition? LocationTo { get; set; } = null;

        /// <summary>Die Gemarkseite, an der eine Wand liegt</summary>
        public Direction? Richtung { get; set; } = null;

        /// <summary>Die Raumpunkte, die gebannt werden sollen</summary>
        public int Raumpunkte { get; set; } = 0;

        /// <summary>Die Nummern der Figuren, die beim Teleport mitkommen</summary>
        public List<int> LadungIds { get; set; } = [];

        /// <summary>Der Zauberer, der den Spruch spricht</summary>
        public Zauberer? Wirker { get; set; } = null;

        /// <summary>Die Figuren, die beim Teleport mitkommen</summary>
        public List<Spielfigur> TeleportPayLoad { get; set; } = [];

        /// <summary>
        /// Der Zustand der Figur vor der Ausführung. Zurückgenommen wird auf diesen Stand, statt
        /// den eigenen Befehl wieder herauszuschneiden - so bleiben auch Altlasten in den Spalten
        /// unangetastet.
        /// </summary>
        private string? _befehlMagieVorher = null;
        private string? _befehlBanntVorher = null;
        private string? _befehlTeleportVorher = null;
        private int _zauberkraftVorher = 0;
        private int _teleportpunkteVorher = 0;
        private (int gf, int kf) _teleportVonVorher = (0, 0);
        private (int gf, int kf) _teleportNachVorher = (0, 0);

        /// <summary>Die Kosten des Spruchs in Zauberkraftpunkten</summary>
        public int Kosten { get; private set; } = 0;

        public override bool CanAppliedTo(ISelectable selectable) => selectable is Zauberer;

        public override string ToString() => Spell switch {
            Zauberspruch.WandErrichten
                => $"Zauberer {NummerFürAnzeige} errichtet eine magische Wand im {Richtung} von {LocationTo?.CreateBezeichner()}",
            Zauberspruch.WandEinreissen
                => $"Zauberer {NummerFürAnzeige} reisst die magische Wand im {Richtung} von {LocationTo?.CreateBezeichner()} ein",
            Zauberspruch.Bannen
                => $"Zauberer {NummerFürAnzeige} bannt {Raumpunkte} Raumpunkte auf {LocationTo?.CreateBezeichner()}",
            Zauberspruch.Zauberduell
                => $"Zauberer {NummerFürAnzeige} fordert {LocationTo?.CreateBezeichner()} zum Zauberduell",
            Zauberspruch.TeleportMitRüstgütern
                => $"Zauberer {NummerFürAnzeige} teleportiert nach {LocationTo?.CreateBezeichner()} mit "
                 + string.Join(", ", LadungIds),
            _ => CommandString,
        };

        private string NummerFürAnzeige => (Wirker?.Nummer ?? UnitID).ToString();

        /// <summary>
        /// Sucht den Zauberer, der den Spruch sprechen soll
        /// </summary>
        private Zauberer? BestimmeZauberer() {
            if (Wirker != null)
                return Wirker;
            if (UnitID == 0)
                return _Selectable as Zauberer;
            return SharedData.Zauberer?
                .FirstOrDefault(z => z.Nummer == UnitID && Plausibilität.IsValid(z));
        }

        /// <summary>
        /// Sucht die Figuren, die beim Teleport mitkommen sollen
        /// </summary>
        private List<Spielfigur> BestimmeLadung() {
            if (TeleportPayLoad.Count > 0)
                return TeleportPayLoad;
            List<Spielfigur> ergebnis = [];
            foreach (int nummer in LadungIds) {
                var figur = SpielfigurenView.GetSpielfigur(nummer);
                if (figur != null)
                    ergebnis.Add(figur);
            }
            return ergebnis;
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

            Result geprüft;
            int kosten = 0;
            switch (Spell) {
                case Zauberspruch.WandErrichten:
                case Zauberspruch.WandEinreissen:
                    geprüft = ZaubereiRules.PrüfeWand(zauberer, Spell, LocationTo, Richtung, out kosten);
                    break;
                case Zauberspruch.Bannen:
                    geprüft = ZaubereiRules.PrüfeBann(zauberer, LocationTo, Raumpunkte, out kosten);
                    break;
                case Zauberspruch.Zauberduell:
                    geprüft = ZaubereiRules.PrüfeDuell(zauberer, LocationTo);
                    break;
                case Zauberspruch.TeleportMitRüstgütern:
                    geprüft = ZaubereiRules.PrüfeTeleportMitRüstgütern(zauberer, LocationTo, BestimmeLadung(), out kosten);
                    break;
                default:
                    return new CommandResultError("Der Zauberspruch ist unbekannt", CommandString, this);
            }

            Kosten = kosten;
            return geprüft.HasErrors ? new CommandResultError(geprüft, this) : new CommandResultSuccess(geprüft, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        /// </summary>
        public override CommandResult ExecuteCommand() {
            CommandResult result = CheckPreconditions();
            if (result.HasErrors)
                return result;

            var zauberer = BestimmeZauberer();
            if (zauberer == null)
                return new CommandResultError("Der Zauberer wurde nicht gefunden", CommandString, this);

            Wirker = zauberer;
            MerkeZustand(zauberer);

            switch (Spell) {
                case Zauberspruch.WandErrichten:
                case Zauberspruch.WandEinreissen:
                case Zauberspruch.Zauberduell:
                    var befehle = Zauberbefehl.LiesAlle(zauberer.Befehl_magie);
                    befehle.Add(new Zauberbefehl {
                        Spruch = Spell,
                        Ziel = LocationTo ?? new KleinfeldPosition(),
                        Richtung = Richtung,
                    });
                    zauberer.Befehl_magie = Zauberbefehl.Schreibe(befehle);
                    break;

                case Zauberspruch.Bannen:
                    zauberer.Befehl_bannt = new Bannbefehl {
                        Ziel = LocationTo ?? new KleinfeldPosition(),
                        Raumpunkte = Raumpunkte,
                    }.ToString();
                    break;

                case Zauberspruch.TeleportMitRüstgütern:
                    var ladung = BestimmeLadung();
                    TeleportPayLoad = ladung;
                    var start = ZaubereiRules.GetStandort(zauberer);
                    int entfernung = ZaubereiRules.GetEntfernung(zauberer, LocationTo, ZaubereiRules.GetFreieTeleportpunkte(zauberer));
                    zauberer.Teleport_gf_von = start.gf;
                    zauberer.Teleport_kf_von = start.kf;
                    zauberer.Teleport_gf_nach = LocationTo?.gf ?? 0;
                    zauberer.Teleport_kf_nach = LocationTo?.kf ?? 0;
                    zauberer.Befehl_Teleport = new Teleportbefehl {
                        MitRüstgütern = true,
                        Von = start,
                        Nach = LocationTo ?? new KleinfeldPosition(),
                        Ladung = ladung.Select(figur => figur.Nummer).ToList(),
                    }.ToString();
                    // die zurückgelegten Gemarken zählen gegen die Teleportweite des Monats
                    zauberer.tp += Math.Max(0, entfernung);
                    VersetzeFiguren(zauberer, ladung, LocationTo);
                    break;
            }

            // "Die Zauberkraftpunkte werden vor dem Sprechen des Zauberspruchs von den
            // Zauberkraftpunkten abgezogen." (Regelwerk 1.4) - auch ein verlorenes Zauberduell
            // gibt sie nicht zurück.
            zauberer.GP_akt -= Kosten;

            Update(zauberer, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            foreach (var figur in TeleportPayLoad) {
                if (figur is IDatabaseTable tabelle)
                    SharedData.StoreQueue.Enqueue(tabelle);
            }
            ZeichneFelderNeu();
            IsExecuted = true;
            return new CommandResultSuccess(result.Title, result.Message, this);
        }

        /// <summary>
        /// <see cref="IPhoenixCommand"/>
        ///
        /// Zurückgenommen wird auf den Stand vor der Ausführung: Befehlsspalten, Zauberkraft und -
        /// beim Teleport - die Positionen von Zauberer und Ladung.
        /// </summary>
        public override CommandResult UndoCommand() {
            if (Wirker == null || _befehlMagieVorher == null)
                return new CommandResultError("Der Zauberspruch lässt sich nicht zurücknehmen",
                    "Zu diesem Befehl ist kein ausgeführter Vorgang vermerkt.", this);

            Wirker.Befehl_magie = _befehlMagieVorher;
            Wirker.Befehl_bannt = _befehlBanntVorher ?? string.Empty;
            Wirker.Befehl_Teleport = _befehlTeleportVorher ?? string.Empty;
            Wirker.GP_akt = _zauberkraftVorher;
            Wirker.tp = _teleportpunkteVorher;
            Wirker.Teleport_gf_von = _teleportVonVorher.gf;
            Wirker.Teleport_kf_von = _teleportVonVorher.kf;
            Wirker.Teleport_gf_nach = _teleportNachVorher.gf;
            Wirker.Teleport_kf_nach = _teleportNachVorher.kf;

            if (Spell == Zauberspruch.TeleportMitRüstgütern)
                VersetzeFiguren(Wirker, TeleportPayLoad,
                    new KleinfeldPosition(_teleportVonVorher.gf, _teleportVonVorher.kf));

            Update(Wirker, ViewEventArgs.ViewEventType.UpdateSpielfiguren);
            foreach (var figur in TeleportPayLoad) {
                if (figur is IDatabaseTable tabelle)
                    SharedData.StoreQueue.Enqueue(tabelle);
            }
            ZeichneFelderNeu();
            IsExecuted = false;
            return new CommandResultSuccess("Der Zauberspruch wurde zurückgenommen", ToString(), this);
        }

        private void MerkeZustand(Zauberer zauberer) {
            _befehlMagieVorher = zauberer.Befehl_magie ?? string.Empty;
            _befehlBanntVorher = zauberer.Befehl_bannt ?? string.Empty;
            _befehlTeleportVorher = zauberer.Befehl_Teleport ?? string.Empty;
            _zauberkraftVorher = zauberer.GP_akt;
            _teleportpunkteVorher = zauberer.tp;
            _teleportVonVorher = (zauberer.gf_nach > 0 ? zauberer.gf_nach : zauberer.gf_von,
                                  zauberer.gf_nach > 0 ? zauberer.kf_nach : zauberer.kf_von);
            _teleportNachVorher = (zauberer.Teleport_gf_nach, zauberer.Teleport_kf_nach);
        }

        /// <summary>
        /// Setzt Zauberer und Ladung auf das Zielfeld. Die Ladung reist mit, bleibt also nicht am
        /// Ausgangsfeld stehen.
        /// </summary>
        private static void VersetzeFiguren(Zauberer zauberer, IEnumerable<Spielfigur> ladung, KleinfeldPosition? ziel) {
            if (ziel == null)
                return;
            zauberer.gf_nach = ziel.gf;
            zauberer.kf_nach = ziel.kf;
            foreach (var figur in ladung) {
                figur.gf_nach = ziel.gf;
                figur.kf_nach = ziel.kf;
            }
        }

        /// <summary>
        /// Die betroffenen Felder werden markiert und müssen deshalb neu gezeichnet werden
        /// </summary>
        private void ZeichneFelderNeu() {
            foreach (var position in new[] { LocationTo, Wirker == null ? null : ZaubereiRules.GetStandort(Wirker) }) {
                if (position == null)
                    continue;
                var kleinfeld = KleinfeldView.GetKleinfeld(position);
                if (kleinfeld != null)
                    SharedData.UpdateQueue.Enqueue(kleinfeld);
            }
        }
    }

    public class CastSpellCommandoParser : SimpleParser {

        /// <summary>
        /// "Zauberer 501 errichtet eine magische Wand im Nordosten von 701/33"
        /// Die Angabe des Zauberers ist freiwillig, wenn einer ausgewählt ist.
        /// </summary>
        private static readonly Regex WandErrichtenRegex = new(
              @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?errichte?t?\s+eine\s+magische\s+Wand\s+"
            + @"(?:im|in\s+den|nach)\s+(?<richtung>[A-Za-zÄÖÜäöü]+)\s+von\s+(?<feld>\d+/\d+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// "Zauberer 501 reisst die magische Wand im Westen von 701/33 ein"
        /// </summary>
        private static readonly Regex WandEinreissenRegex = new(
              @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?(?:reisse?t?|reiße?t?)\s+die\s+magische\s+Wand\s+"
            + @"(?:im|in\s+den|nach)\s+(?<richtung>[A-Za-zÄÖÜäöü]+)\s+von\s+(?<feld>\d+/\d+)\s+ein$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// "Zauberer 501 bannt 2000 Raumpunkte auf 701/33"
        /// "Zauberer 501 bannt 701/33 mit 4 Zauberkraftpunkten"
        /// </summary>
        private static readonly Regex BannRegex = new(
              @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?bannt?\s+"
            + @"(?:(?<rp>\d+)\s+(?:Raumpunkte|RP)\s+auf\s+(?<feld>\d+/\d+)"
            + @"|(?<feld2>\d+/\d+)\s+mit\s+(?<zkp>\d+)\s+(?:Zauberkraftpunkten?|ZKP))$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// "Zauberer 501 fordert 701/33 zum Zauberduell"
        /// </summary>
        private static readonly Regex DuellRegex = new(
              @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?fordert?\s+(?:den\s+Zauberer\s+(?:auf\s+)?)?"
            + @"(?<feld>\d+/\d+)\s+(?:zum\s+(?:Zauber)?[Dd]uell|heraus)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// "Zauberer 501 teleportiert nach 755/22 mit Krieger 101, Reiter 203"
        /// Ohne Ladung ist es kein Zauberspruch, sondern gewöhnliche Bewegung - deshalb ist die
        /// Ladung hier Pflicht.
        /// </summary>
        private static readonly Regex TeleportRegex = new(
              @"^(?:Zauberer\s+(?<unitId>\d+)\s+)?teleportiert?\s+(?:von\s+\d+/\d+\s+)?nach\s+(?<feld>\d+/\d+)"
            + @"\s+mit\s+(?<ladung>.+)$",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Die Nummern in einer Ladungsangabe wie "Krieger 101, Reiter 203, 404"
        /// </summary>
        private static readonly Regex LadungsNummern = new(@"\d+", RegexOptions.Compiled);

        public override bool ParseCommand(string commandString, out IPhoenixCommand? command) {
            try {
                var treffer = WandErrichtenRegex.Match(commandString);
                if (treffer.Success)
                    return BaueWand(commandString, treffer, Zauberspruch.WandErrichten, out command);

                treffer = WandEinreissenRegex.Match(commandString);
                if (treffer.Success)
                    return BaueWand(commandString, treffer, Zauberspruch.WandEinreissen, out command);

                treffer = BannRegex.Match(commandString);
                if (treffer.Success) {
                    bool überZKP = treffer.Groups["zkp"].Success;
                    var feld = ParseLocation(überZKP ? treffer.Groups["feld2"].Value : treffer.Groups["feld"].Value);
                    command = new CastSpellCommando(commandString) {
                        Spell = Zauberspruch.Bannen,
                        UnitID = LiesNummer(treffer),
                        LocationTo = feld,
                        Raumpunkte = überZKP ? 0 : ParseInt(treffer.Groups["rp"].Value),
                    };
                    if (überZKP && command is CastSpellCommando bann)
                        bann.Raumpunkte = BerechneRaumpunkteAusZKP(bann, ParseInt(treffer.Groups["zkp"].Value));
                    return true;
                }

                treffer = DuellRegex.Match(commandString);
                if (treffer.Success) {
                    command = new CastSpellCommando(commandString) {
                        Spell = Zauberspruch.Zauberduell,
                        UnitID = LiesNummer(treffer),
                        LocationTo = ParseLocation(treffer.Groups["feld"].Value),
                    };
                    return true;
                }

                treffer = TeleportRegex.Match(commandString);
                if (treffer.Success) {
                    command = new CastSpellCommando(commandString) {
                        Spell = Zauberspruch.TeleportMitRüstgütern,
                        UnitID = LiesNummer(treffer),
                        LocationTo = ParseLocation(treffer.Groups["feld"].Value),
                        LadungIds = LadungsNummern.Matches(treffer.Groups["ladung"].Value)
                            .Select(m => int.Parse(m.Value)).ToList(),
                    };
                    return true;
                }
            }
            catch (Exception ex) {
                ProgramView.LogError("Beim Lesen des Zauberspruchs gab es einen Fehler", ex.Message);
                command = null;
                return false;
            }
            return Fail(out command);
        }

        private static bool BaueWand(string commandString, Match treffer, Zauberspruch spruch, out IPhoenixCommand? command) {
            var richtung = ParseRichtung(treffer.Groups["richtung"].Value);
            if (richtung == null)
                return Fail(out command);
            command = new CastSpellCommando(commandString) {
                Spell = spruch,
                UnitID = LiesNummer(treffer),
                LocationTo = ParseLocation(treffer.Groups["feld"].Value),
                Richtung = richtung,
            };
            return true;
        }

        private static int LiesNummer(Match treffer)
            => treffer.Groups["unitId"].Success ? ParseInt(treffer.Groups["unitId"].Value) : 0;

        /// <summary>
        /// Rechnet eine Angabe in Zauberkraftpunkten in die Raumpunkte um, die sich damit auf die
        /// tatsächliche Entfernung bannen lassen. Steht der Zauberer noch nicht fest, gilt die
        /// nahe Entfernung; die Prüfung korrigiert das später.
        /// </summary>
        private static int BerechneRaumpunkteAusZKP(CastSpellCommando befehl, int zauberkraftpunkte) {
            var zauberer = SharedData.Zauberer?.FirstOrDefault(z => z.Nummer == befehl.UnitID);
            int entfernung = zauberer == null
                ? ZaubereiRules.MinEntfernungBann
                : ZaubereiRules.GetEntfernung(zauberer, befehl.LocationTo, ZaubereiRules.MaxEntfernungBann);
            return ZaubereiRules.BerechneBannwirkung(Math.Max(entfernung, ZaubereiRules.MinEntfernungBann), zauberkraftpunkte);
        }

        /// <summary>
        /// Erkennt eine Windrichtung in Kurz- und Langform
        /// </summary>
        public static Direction? ParseRichtung(string eingabe) => eingabe.ToLower() switch {
            "nw" or "nordwesten" or "nordwest" => Direction.NW,
            "no" or "nordosten" or "nordost" => Direction.NO,
            "o" or "osten" or "ost" => Direction.O,
            "so" or "südosten" or "suedosten" or "südost" or "suedost" => Direction.SO,
            "sw" or "südwesten" or "suedwesten" or "südwest" or "suedwest" => Direction.SW,
            "w" or "westen" or "west" => Direction.W,
            _ => null,
        };
    }
}
