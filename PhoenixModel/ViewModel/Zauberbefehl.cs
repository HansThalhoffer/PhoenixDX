using System.Text.RegularExpressions;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Die Zaubersprüche, die ein Zauberer im Spielzug aufgeben kann.
    ///
    /// Die Teleportation eines Zauberers ohne Rüstgüter ist kein Zauberspruch, sondern eine
    /// Bewegungsfertigkeit (Regelwerk 1.3) und steht deshalb nicht in dieser Liste.
    /// </summary>
    public enum Zauberspruch {
        /// <summary>Kein Zauberspruch</summary>
        Keiner,
        /// <summary>Eine magische Wand errichten (Regelwerk 1.4.2)</summary>
        WandErrichten,
        /// <summary>Eine gegnerische magische Wand einreissen (Regelwerk 1.4.4)</summary>
        WandEinreissen,
        /// <summary>Ein Zauberduell auf einer Gemark fordern (Regelwerk 5.3)</summary>
        Zauberduell,
        /// <summary>Gegnerische Rüstgüter bannen (Regelwerk 1.4.3)</summary>
        Bannen,
        /// <summary>Teleportation mit Rüstgütern (Regelwerk 1.4.1)</summary>
        TeleportMitRüstgütern,
    }

    /// <summary>
    /// Ein einzelner Eintrag in der Spalte Befehl_magie eines Zauberers.
    ///
    /// Das Format stammt aus der Altanwendung und bleibt unverändert, weil die Spielleitung die
    /// Befehle so ausliest: jeder Befehl beginnt mit einem #, dann folgt das Kürzel, ein
    /// Doppelpunkt, das Zielfeld und - ausser beim Duell - die Gemarkseite.
    ///
    /// "#EW:603/45,NO" eine magische Wand im Nordosten von 603/45 errichten
    /// "#ZW:1002/87,W" die magische Wand im Westen von 1002/87 einreissen
    /// "#ZD:1002/87"   auf 1002/87 zum Zauberduell fordern
    ///
    /// Die Kürzel sind die der Altanwendung (AktionsTypConverter): EW, ZW, ZD. Gelesen und auf der
    /// Karte dargestellt hat die Altanwendung diese Befehle sehr wohl, geschrieben hat sie nie
    /// einen - die vier Dialoge Zauber_Wanderrichten, Zauber_Wandeinreissen, Zauber_Bannen und
    /// Zauber_zauberduell werfen alle eine NotImplementedException und ihr Rumpf ist
    /// auskommentiert. Das Format ist deshalb aus diesem Rumpf übernommen.
    /// </summary>
    public class Zauberbefehl {

        /// <summary>Leitet jeden Befehl ein</summary>
        public const char Trenner = '#';

        /// <summary>Steht zwischen Kürzel und Zielfeld</summary>
        public const char Doppelpunkt = ':';

        /// <summary>Trennt Zielfeld und Gemarkseite</summary>
        public const char Komma = ',';

        /// <summary>Der Zauberspruch</summary>
        public Zauberspruch Spruch { get; set; } = Zauberspruch.Keiner;

        /// <summary>Die betroffene Gemark</summary>
        public KleinfeldPosition Ziel { get; set; } = new();

        /// <summary>Die Gemarkseite, an der die Wand steht. Beim Duell gibt es keine.</summary>
        public Direction? Richtung { get; set; } = null;

        /// <summary>
        /// Das Kürzel, unter dem der Spruch in der Datenbank steht
        /// </summary>
        public static string? GetKürzel(Zauberspruch spruch) => spruch switch {
            Zauberspruch.WandErrichten => "EW",
            Zauberspruch.WandEinreissen => "ZW",
            Zauberspruch.Zauberduell => "ZD",
            _ => null,
        };

        /// <summary>
        /// Der Spruch zu einem Kürzel
        /// </summary>
        public static Zauberspruch GetSpruch(string? kürzel) => kürzel?.ToUpperInvariant() switch {
            "EW" => Zauberspruch.WandErrichten,
            "ZW" => Zauberspruch.WandEinreissen,
            "ZD" => Zauberspruch.Zauberduell,
            _ => Zauberspruch.Keiner,
        };

        /// <summary>
        /// Ein einzelner Befehl in seiner Schreibweise für die Datenbank, ohne führendes #
        /// </summary>
        public override string ToString() {
            string basis = $"{GetKürzel(Spruch)}{Doppelpunkt}{Ziel.gf}/{Ziel.kf}";
            return Richtung == null ? basis : $"{basis}{Komma}{Richtung}";
        }

        /// <summary>
        /// Eine lesbare Fassung für die Anzeige
        /// </summary>
        public string Beschreibung => Spruch switch {
            Zauberspruch.WandErrichten => $"magische Wand im {Richtung} von {Ziel.CreateBezeichner()} errichten",
            Zauberspruch.WandEinreissen => $"magische Wand im {Richtung} von {Ziel.CreateBezeichner()} einreissen",
            Zauberspruch.Zauberduell => $"Zauberduell auf {Ziel.CreateBezeichner()}",
            _ => ToString(),
        };

        private static readonly Regex BefehlsMuster = new(
            @"^(?<kuerzel>[A-Za-z]{2}):(?<gf>\d+)/(?<kf>\d+)(?:,(?<richtung>[A-Za-z]{1,2}))?$",
            RegexOptions.Compiled);

        /// <summary>
        /// Liest einen einzelnen Befehl, etwa "EW:603/45,NO"
        /// </summary>
        /// <returns>der Befehl oder null, wenn er sich nicht lesen lässt</returns>
        public static Zauberbefehl? Lies(string? befehl) {
            if (string.IsNullOrWhiteSpace(befehl))
                return null;
            var treffer = BefehlsMuster.Match(befehl.Trim());
            if (treffer.Success == false)
                return null;

            var spruch = GetSpruch(treffer.Groups["kuerzel"].Value);
            if (spruch == Zauberspruch.Keiner)
                return null;

            Direction? richtung = null;
            if (treffer.Groups["richtung"].Success) {
                if (Enum.TryParse<Direction>(treffer.Groups["richtung"].Value, true, out var gelesen) == false)
                    return null;
                richtung = gelesen;
            }

            // eine Wand liegt immer an einer Gemarkseite, ein Duell nie
            bool istDuell = spruch == Zauberspruch.Zauberduell;
            if (istDuell == (richtung != null))
                return null;

            return new Zauberbefehl {
                Spruch = spruch,
                Ziel = new KleinfeldPosition(int.Parse(treffer.Groups["gf"].Value), int.Parse(treffer.Groups["kf"].Value)),
                Richtung = richtung,
            };
        }

        /// <summary>
        /// Liest alle Befehle aus dem Inhalt von Befehl_magie. Was sich nicht lesen lässt, wird
        /// übergangen - die Spalte kann Altlasten enthalten.
        /// </summary>
        public static List<Zauberbefehl> LiesAlle(string? befehlMagie) {
            List<Zauberbefehl> ergebnis = [];
            if (string.IsNullOrWhiteSpace(befehlMagie))
                return ergebnis;
            foreach (string teil in befehlMagie.Split(Trenner, StringSplitOptions.RemoveEmptyEntries)) {
                var befehl = Lies(teil);
                if (befehl != null)
                    ergebnis.Add(befehl);
            }
            return ergebnis;
        }

        /// <summary>
        /// Setzt aus mehreren Befehlen den Inhalt von Befehl_magie zusammen. Jeder Befehl bekommt
        /// ein führendes #, so wie es die Altanwendung beim Anhängen auch gemacht hat.
        /// </summary>
        public static string Schreibe(IEnumerable<Zauberbefehl> befehle)
            => string.Concat(befehle.Select(befehl => $"{Trenner}{befehl}"));
    }

    /// <summary>
    /// Der Inhalt der Spalte Befehl_Teleport eines Zauberers.
    ///
    /// In dieser Spalte stehen zwei verschiedene Dinge, die das Regelwerk klar trennt: die
    /// Teleportation des Zauberers allein ist eine Bewegungsfertigkeit und kostet keine
    /// Zauberkraft (Regelwerk 1.3), die Teleportation mit Rüstgütern dagegen ist ein Zauberspruch
    /// (Regelwerk 1.4.1). Der Unterschied entscheidet darüber, ob der Zauberer in diesem Monat
    /// noch zaubern darf - deshalb steht er im Kürzel und nicht nur in der Ladung.
    ///
    /// "ZT:507/31-507/32"          der Zauberer teleportiert allein
    /// "ZTR:507/31-507/32,101,203" der Zauberer nimmt Krieger 101 und Reiter 203 mit
    ///
    /// Die Altanwendung hat diese Spalte nie beschrieben - ihr Teleportdialog ist ein Rumpf - und
    /// gibt damit kein Format vor. Geschrieben wird deshalb dieselbe Schreibweise wie bei den
    /// übrigen Zauberbefehlen, damit die Spielleitung nicht umlernen muss.
    /// </summary>
    public class Teleportbefehl {

        /// <summary>Kürzel der Teleportation ohne Ladung - eine Bewegungsfertigkeit</summary>
        public const string KürzelBewegung = "ZT";

        /// <summary>Kürzel der Teleportation mit Rüstgütern - ein Zauberspruch</summary>
        public const string KürzelZauberspruch = "ZTR";

        /// <summary>Werden Rüstgüter mitgenommen? Dann ist es ein Zauberspruch.</summary>
        public bool MitRüstgütern { get; set; }

        /// <summary>Das Feld, von dem aus teleportiert wird</summary>
        public KleinfeldPosition Von { get; set; } = new();

        /// <summary>Das Feld, auf dem der Zauberer erscheint</summary>
        public KleinfeldPosition Nach { get; set; } = new();

        /// <summary>Die Nummern der mitgenommenen Figuren</summary>
        public List<int> Ladung { get; set; } = [];

        public override string ToString() {
            string kürzel = MitRüstgütern ? KürzelZauberspruch : KürzelBewegung;
            string nummern = string.Join(",", Ladung);
            return $"{kürzel}:{Von.gf}/{Von.kf}-{Nach.gf}/{Nach.kf}"
                 + (string.IsNullOrEmpty(nummern) ? string.Empty : $",{nummern}");
        }

        public string Beschreibung {
            get {
                string was = MitRüstgütern
                    ? $" mit {(Ladung.Count == 0 ? "Rüstgütern" : string.Join(", ", Ladung))}"
                    : " allein";
                return $"teleportiert von {Von.CreateBezeichner()} nach {Nach.CreateBezeichner()}{was}";
            }
        }

        private static readonly Regex BefehlsMuster = new(
            @"^(?<kuerzel>ZTR|ZT):(?<vongf>\d+)/(?<vonkf>\d+)-(?<nachgf>\d+)/(?<nachkf>\d+)(?<ladung>(?:,\d+)*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Liest den Inhalt von Befehl_Teleport
        /// </summary>
        /// <returns>der Befehl oder null, wenn nichts Lesbares darin steht</returns>
        public static Teleportbefehl? Lies(string? befehlTeleport) {
            if (string.IsNullOrWhiteSpace(befehlTeleport))
                return null;
            var treffer = BefehlsMuster.Match(befehlTeleport.Trim());
            if (treffer.Success == false)
                return null;
            return new Teleportbefehl {
                MitRüstgütern = string.Equals(treffer.Groups["kuerzel"].Value, KürzelZauberspruch,
                    StringComparison.OrdinalIgnoreCase),
                Von = new KleinfeldPosition(int.Parse(treffer.Groups["vongf"].Value), int.Parse(treffer.Groups["vonkf"].Value)),
                Nach = new KleinfeldPosition(int.Parse(treffer.Groups["nachgf"].Value), int.Parse(treffer.Groups["nachkf"].Value)),
                Ladung = treffer.Groups["ladung"].Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToList(),
            };
        }
    }

    /// <summary>
    /// Der Inhalt der Spalte Befehl_bannt eines Zauberers.
    ///
    /// Anders als Befehl_magie nimmt diese Spalte nur einen Befehl auf; die Altanwendung hat sie
    /// zugewiesen statt angehängt: "ZB:&lt;gf&gt;/&lt;kf&gt;,&lt;Raumpunkte&gt;". Das passt zur
    /// Regel, denn gebannt wird einmal im Monat auf eine Gemark.
    ///
    /// Beispiel: "ZB:603/45,2000"
    /// </summary>
    public class Bannbefehl {

        /// <summary>Das Kürzel, mit dem der Befehl beginnt</summary>
        public const string Kürzel = "ZB";

        /// <summary>Die gebannte Gemark</summary>
        public KleinfeldPosition Ziel { get; set; } = new();

        /// <summary>Wieviele Raumpunkte an gegnerischen Rüstgütern gebannt werden</summary>
        public int Raumpunkte { get; set; }

        public override string ToString() => $"{Kürzel}:{Ziel.gf}/{Ziel.kf},{Raumpunkte}";

        public string Beschreibung => $"{Raumpunkte} Raumpunkte auf {Ziel.CreateBezeichner()} bannen";

        private static readonly Regex BefehlsMuster = new(
            @"^ZB:(?<gf>\d+)/(?<kf>\d+),(?<rp>\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Liest den Inhalt von Befehl_bannt
        /// </summary>
        /// <returns>der Befehl oder null, wenn nichts Lesbares darin steht</returns>
        public static Bannbefehl? Lies(string? befehlBannt) {
            if (string.IsNullOrWhiteSpace(befehlBannt))
                return null;
            var treffer = BefehlsMuster.Match(befehlBannt.Trim());
            if (treffer.Success == false)
                return null;
            return new Bannbefehl {
                Ziel = new KleinfeldPosition(int.Parse(treffer.Groups["gf"].Value), int.Parse(treffer.Groups["kf"].Value)),
                Raumpunkte = int.Parse(treffer.Groups["rp"].Value),
            };
        }
    }
}
