using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Was sich auf einer Gemark bauen lässt - und wenn nichts, warum nicht.
    ///
    /// Die Bauregeln stehen in <see cref="ConstructRules"/> und geben zu jeder Frage eine Antwort
    /// mit Begründung. Die Oberfläche hat diese Begründungen bisher weggeworfen und nur die
    /// Schaltfläche versteckt: wer eine Gemark auswählte, auf der gerade nichts ging, sah den
    /// Baustern leer werden und erfuhr nicht, woran es lag. Diese Klasse hält beides zusammen -
    /// die Liste der Möglichkeiten und den Satz, der die Lage erklärt.
    ///
    /// Sie entscheidet nichts selbst. Jede Antwort kommt aus <see cref="ConstructRules"/>.
    /// </summary>
    public static class BauoptionenView {

        /// <summary>
        /// Die Bauwerke, die an einer Kante einer Gemark entstehen (Regelwerk 1.5.1 bis 1.5.4)
        /// </summary>
        public static readonly ConstructionElementType[] Kantenbauwerke =
            [ConstructionElementType.Strasse, ConstructionElementType.Wall,
             ConstructionElementType.Bruecke, ConstructionElementType.Kai];

        /// <summary>
        /// Eine einzelne Baumöglichkeit: was, wohin, geht es, und was sagt die Regel dazu.
        /// </summary>
        /// <param name="Art">Straße, Wall, Brücke, Kai oder Burg</param>
        /// <param name="Richtung">die Kante, an der gebaut wird; null bei der Burg</param>
        /// <param name="Möglich">ob die Regeln es zulassen</param>
        /// <param name="Titel">die Kurzfassung der Regelantwort</param>
        /// <param name="Begründung">die ausführliche Regelantwort</param>
        public record class Bauoption(ConstructionElementType Art, Direction? Richtung, bool Möglich,
                string Titel, string Begründung) {

            /// <summary>
            /// Wie die Möglichkeit heisst, etwa "Straße nach Nordosten" oder "Burg"
            /// </summary>
            public string Bezeichnung => Richtung == null
                ? Benenne(Art)
                : $"{Benenne(Art)} nach {(DirectionNames)Richtung.Value}";

            /// <summary>
            /// Ein Satz für die Kurzhilfe an der Schaltfläche: was es kostet, oder woran es scheitert.
            /// </summary>
            public string Hinweis => Möglich
                ? $"{Bezeichnung} - {KostenView.GetGSKosten(Art):n0} GS"
                : $"{Bezeichnung}: {Titel}. {Begründung}";
        }

        /// <summary>
        /// Der Name eines Bauwerks, wie ihn das Regelwerk schreibt
        /// </summary>
        public static string Benenne(ConstructionElementType art) => art switch {
            ConstructionElementType.Strasse => "Straße",
            ConstructionElementType.Bruecke => "Brücke",
            ConstructionElementType.Kai => "Kaianlage",
            ConstructionElementType.Wall => "Wall",
            ConstructionElementType.Burg => "Burg",
            _ => art.ToString(),
        };

        /// <summary>
        /// Fragt die Bauregeln zu einer einzelnen Möglichkeit.
        /// </summary>
        public static Result Prüfe(KleinFeld? feld, ConstructionElementType art, Direction richtung) {
            if (feld == null)
                return Result.Fail("Keine Gemark ausgewählt", "Gebaut wird auf einer Gemark.");
            return art switch {
                ConstructionElementType.Strasse => ConstructRules.CanConstructRoad(feld, richtung),
                ConstructionElementType.Wall => ConstructRules.CanConstructWall(feld, richtung),
                ConstructionElementType.Bruecke => ConstructRules.CanConstructBridge(feld, richtung),
                ConstructionElementType.Kai => ConstructRules.CanConstructKai(feld, richtung),
                ConstructionElementType.Burg => ConstructRules.CanConstructCastle(feld),
                _ => Result.Fail($"{Benenne(art)} lässt sich so nicht errichten",
                        "Für dieses Bauwerk gibt es hier keine Regel."),
            };
        }

        /// <summary>
        /// Alle Möglichkeiten einer Gemark: vier Kantenbauwerke in sechs Richtungen und die Burg.
        ///
        /// Die Liste ist immer vollständig - auch das, was gerade nicht geht, steht mit seiner
        /// Begründung darin. Die Oberfläche entscheidet, was sie davon zeigt.
        /// </summary>
        public static List<Bauoption> Bestimme(KleinFeld? feld) {
            List<Bauoption> optionen = [];
            foreach (var art in Kantenbauwerke) {
                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var geprüft = Prüfe(feld, art, richtung);
                    optionen.Add(new Bauoption(art, richtung, geprüft.HasErrors == false,
                        geprüft.Title, geprüft.Message));
                }
            }
            var burg = Prüfe(feld, ConstructionElementType.Burg, Direction.W);
            optionen.Add(new Bauoption(ConstructionElementType.Burg, null, burg.HasErrors == false,
                burg.Title, burg.Message));
            return optionen;
        }

        /// <summary>
        /// Geht auf dieser Gemark überhaupt etwas?
        /// </summary>
        public static bool IstEtwasMöglich(KleinFeld? feld) => Bestimme(feld).Any(option => option.Möglich);

        /// <summary>
        /// Der Satz über dem Baustern: was hier geht, oder woran es liegt, dass nichts geht.
        ///
        /// Die Reihenfolge der Gründe ist die der Bauregeln - erst muss eine Gemark ausgewählt
        /// sein, dann muss sie einem gehören, dann muss die Zeit stimmen. Was zuerst schiefgeht,
        /// steht oben; alles Weitere wäre nur Rauschen.
        /// </summary>
        /// <returns>ein Ergebnis, dessen Titel über den Baustern gehört; HasErrors heisst: hier geht nichts</returns>
        public static Result BeschreibeLage(KleinFeld? feld) {
            if (feld == null)
                return Result.Fail("Keine Gemark ausgewählt",
                    "Wähle auf der Karte eine Gemark deines Reiches aus, dann steht hier, was sich dort bauen lässt.");

            string wo = feld.CreateBezeichner();

            if (ConstructRules.IsAllowedForOwner(feld) is Result besitzer && besitzer.HasErrors)
                return Result.Fail($"{wo} gehört nicht zu deinem Reich",
                    "Gebaut wird nur auf eigenem Gebiet.");

            if (ConstructRules.IsNotAllowedOnWater(feld) is Result wasser && wasser.HasErrors)
                return Result.Fail($"{wo} liegt im Wasser", wasser.Message);

            if (ConstructRules.IsRüstPhase() is Result phase && phase.HasErrors)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht gebaut",
                    "Straßen, Wälle, Brücken, Kaianlagen und Burgen entstehen in der Rüstphase. "
                    + "Solange der Zug schon in der Bewegungsphase ist, geht das erst im nächsten Monat wieder.");

            var möglich = Bestimme(feld).Where(option => option.Möglich).ToList();
            if (möglich.Count == 0)
                return Result.Fail($"Auf {wo} lässt sich gerade nichts bauen",
                    "Entweder steht an jeder Kante schon etwas, das Gelände gibt es nicht her, "
                    + "oder das Geld reicht nicht. Die einzelnen Gründe stehen an den Schaltflächen.");

            return Result.Success($"{wo}: {möglich.Count} Möglichkeiten",
                string.Join(", ", möglich.Select(option => option.Bezeichnung)));
        }
    }
}
