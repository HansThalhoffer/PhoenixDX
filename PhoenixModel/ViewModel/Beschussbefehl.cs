using System.Text.RegularExpressions;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Die beiden Bauarten von Fernkampfwaffen.
    ///
    /// In der Datenbank steht nur "leicht" oder "schwer" - ob daraus ein Katapult (LKP/SKP) oder
    /// ein Kriegsschiff (LKS/SKS) wird, ergibt sich aus der Figur, die den Befehl trägt. Schiffe
    /// führen ihre Geschütze in denselben Spalten LKP und SKP wie die Landeinheiten. Die
    /// Altanwendung macht es über die Aliase ihres KatapultArt-Enums genauso.
    /// </summary>
    public enum Fernkampfwaffe {
        Leicht,
        Schwer,
    }

    /// <summary>
    /// Ein einzelner Beschussbefehl, wie er in der Spalte Befehl_ang einer Figur steht.
    ///
    /// Das Format stammt aus der Altanwendung und bleibt unverändert, weil die Spielleitung die
    /// Befehle so auswertet: "&lt;Anzahl&gt;&lt;LK|SK&gt;#&lt;gf&gt;/&lt;kf&gt;" und optional noch
    /// "#&lt;Entfernung&gt;KF". Mehrere Befehle stehen hintereinander, jeder mit einem führenden #.
    ///
    /// Beispiele: "4LK#603/78", "3SK#1002/87#2KF"
    /// </summary>
    public class Beschussbefehl {

        /// <summary>
        /// Trennt die Bestandteile eines Befehls und leitet jeden Befehl ein
        /// </summary>
        public const char Trenner = '#';

        /// <summary>
        /// Die Entfernung, die gilt, wenn im Befehl keine angegeben ist
        /// </summary>
        public const int StandardEntfernung = 1;

        /// <summary>Wieviele Geschütze auf dieses Ziel schiessen</summary>
        public int Anzahl { get; set; }

        /// <summary>Leichte oder schwere Fernkampfwaffe</summary>
        public Fernkampfwaffe Waffe { get; set; }

        /// <summary>Das beschossene Kleinfeld</summary>
        public KleinfeldPosition Ziel { get; set; } = new();

        /// <summary>Die Entfernung zum Ziel in Kleinfeldern</summary>
        public int Entfernung { get; set; } = StandardEntfernung;

        /// <summary>
        /// Das Kürzel der Waffe, wie es im Befehl steht
        /// </summary>
        public string Kürzel => Waffe == Fernkampfwaffe.Schwer ? "SK" : "LK";

        /// <summary>
        /// Ein einzelner Befehl in seiner Schreibweise für die Datenbank
        /// </summary>
        public override string ToString() {
            string basis = $"{Anzahl}{Kürzel}{Trenner}{Ziel.gf}/{Ziel.kf}";
            return Entfernung > StandardEntfernung ? $"{basis}{Trenner}{Entfernung}KF" : basis;
        }

        /// <summary>
        /// Eine lesbare Fassung für die Anzeige
        /// </summary>
        public string Beschreibung {
            get {
                string waffe = Waffe == Fernkampfwaffe.Schwer ? "schwere" : "leichte";
                string entfernung = Entfernung > StandardEntfernung ? $" auf {Entfernung} Gemarken" : string.Empty;
                return $"{Anzahl} {waffe} Fernkampfwaffen auf {Ziel.CreateBezeichner()}{entfernung}";
            }
        }

        /// <summary>
        /// Zerlegt den Inhalt von Befehl_ang in die einzelnen Befehle.
        ///
        /// Der Ausdruck ist aus der Altanwendung übernommen: ein Befehl besteht aus Anzahl und
        /// Waffenkürzel, dem Zielfeld und optional der Entfernung. Die Vorausschau sorgt dafür,
        /// dass die optionale Entfernung nicht als Anfang des nächsten Befehls gelesen wird.
        /// </summary>
        /// <example>
        /// "#4LK#603/78#3SK#1002/87#2KF" wird zu "4LK#603/78" und "3SK#1002/87#2KF"
        /// </example>
        private static readonly Regex BefehlsTrennung = new(
            @"#\d+[A-Za-z]+#\d+/\d+(?:#\d+[A-Za-z]+)?(?=(#\d+[A-Za-z]+#\d+/\d+)|$)",
            RegexOptions.Compiled);

        public static string[] Zerlege(string? befehle) {
            if (string.IsNullOrEmpty(befehle))
                return [];
            return BefehlsTrennung.Matches(befehle)
                .Select(treffer => treffer.Value.TrimStart(Trenner))
                .ToArray();
        }

        private static readonly Regex AnzahlUndWaffe = new(@"^(\d+)([A-Za-z]+)$", RegexOptions.Compiled);

        /// <summary>
        /// Liest einen einzelnen Befehl, etwa "3SK#1002/87#2KF"
        /// </summary>
        /// <returns>der Befehl oder null, wenn er sich nicht lesen lässt</returns>
        public static Beschussbefehl? Lies(string? befehl) {
            if (string.IsNullOrEmpty(befehl))
                return null;

            string[] teile = befehl.Split(Trenner);
            if (teile.Length < 2)
                return null;

            var kopf = AnzahlUndWaffe.Match(teile[0]);
            if (kopf.Success == false || int.TryParse(kopf.Groups[1].Value, out int anzahl) == false)
                return null;

            var ziel = ParseFeld(teile[1]);
            if (ziel == null)
                return null;

            int entfernung = StandardEntfernung;
            if (teile.Length > 2) {
                var abstand = AnzahlUndWaffe.Match(teile[2]);
                if (abstand.Success == false || int.TryParse(abstand.Groups[1].Value, out entfernung) == false)
                    return null;
            }

            return new Beschussbefehl {
                Anzahl = anzahl,
                Waffe = kopf.Groups[2].Value.StartsWith("S", StringComparison.OrdinalIgnoreCase)
                    ? Fernkampfwaffe.Schwer : Fernkampfwaffe.Leicht,
                Ziel = ziel,
                Entfernung = entfernung,
            };
        }

        private static KleinfeldPosition? ParseFeld(string eingabe) {
            string[] teile = eingabe.Split('/');
            if (teile.Length != 2)
                return null;
            if (int.TryParse(teile[0], out int gf) == false || int.TryParse(teile[1], out int kf) == false)
                return null;
            return new KleinfeldPosition(gf, kf);
        }

        /// <summary>
        /// Liest alle Befehle aus dem Inhalt von Befehl_ang. Was sich nicht lesen lässt, wird
        /// übergangen - die Spalte kann Altlasten enthalten.
        /// </summary>
        public static List<Beschussbefehl> LiesAlle(string? befehlAng) {
            var ergebnis = new List<Beschussbefehl>();
            foreach (string teil in Zerlege(befehlAng)) {
                var befehl = Lies(teil);
                if (befehl != null)
                    ergebnis.Add(befehl);
            }
            return ergebnis;
        }

        /// <summary>
        /// Setzt aus mehreren Befehlen den Inhalt von Befehl_ang zusammen. Jeder Befehl bekommt ein
        /// führendes #, so wie es die Altanwendung beim Anhängen auch gemacht hat.
        /// </summary>
        public static string Schreibe(IEnumerable<Beschussbefehl> befehle) {
            return string.Concat(befehle.Select(befehl => $"{Trenner}{befehl}"));
        }
    }
}
