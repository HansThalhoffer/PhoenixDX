namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Die vier Jahreszeiten auf Erkenfara
    /// </summary>
    public enum Jahreszeit {
        Winter,
        Frühling,
        Sommer,
        Herbst,
    }

    /// <summary>
    /// Ein Spielzug und der Monat, zu dem er gehört.
    ///
    /// Ein Phoenix-Jahr gliedert sich in acht Monate (Regelwerk Kapitel 3), innerhalb eines Monats
    /// ist jedes Reich einmal am Zug. Aus der fortlaufenden Zugnummer ergeben sich Jahr, Monatsname,
    /// Jahreszeit sowie die Frage, ob es ein Rüst- oder Einnahmemonat ist.
    /// </summary>
    public class Zugmonat : IEquatable<Zugmonat> {

        /// <summary>
        /// Ein Phoenix-Jahr hat acht Monate
        /// </summary>
        public const int MonateProJahr = 8;

        /// <summary>
        /// Die Namen der Monate in der Reihenfolge des Jahres
        /// </summary>
        private static readonly string[] _namen = [
            "Hawar", "Rim", "Naliv", "Larn", "Hel", "Jawan", "Lud", "Agul"
        ];

        private static readonly Jahreszeit[] _jahreszeiten = [
            Jahreszeit.Winter, Jahreszeit.Frühling, Jahreszeit.Frühling, Jahreszeit.Sommer,
            Jahreszeit.Sommer, Jahreszeit.Herbst, Jahreszeit.Herbst, Jahreszeit.Winter
        ];

        /// <summary>
        /// Die Monate, in denen gerüstet wird: Hawar und Hel.
        ///
        /// Regelwerk 3.2: "Jeweils am Ende des 4. Monats, Larn, und 8. Monats, Agul, werden die
        /// Steuern eingetrieben, welche dann ... im darauf folgenden Rüstmonat (Hawar und Hel)
        /// zur Verfügung stehen."
        ///
        /// Achtung: die Übersichtstabelle in Kapitel 3 des Regelwerks widerspricht dem - dort sind
        /// Naliv und Jawan als Einnahmemonat und Larn als Rüstmonat markiert. Die Spalte
        /// "Besonderes" ist dort offenbar verrutscht. Massgeblich ist der Text in 3.2, der sich
        /// auch mit der Altanwendung deckt.
        /// </summary>
        private static readonly int[] _rüstmonate = [0, 4];

        /// <summary>
        /// Die Monate, an deren Ende die Steuern eingetrieben werden: Larn und Agul (Regelwerk 3.2)
        /// </summary>
        private static readonly int[] _einnahmemonate = [3, 7];

        /// <summary>
        /// Die fortlaufende Nummer des Spielzuges. Der erste Zug hat die Nummer 1.
        /// </summary>
        public int Zug { get; }

        public Zugmonat(int zug) {
            Zug = zug;
        }

        /// <summary>
        /// Ist die Zugnummer überhaupt sinnvoll?
        /// </summary>
        public bool IstGültig => Zug > 0;

        /// <summary>
        /// Das Spieljahr, beginnend bei 0 für die ersten acht Züge
        /// </summary>
        public int Jahr => (Zug - 1) / MonateProJahr;

        /// <summary>
        /// Die Position des Monats innerhalb des Jahres, 0 für Hawar bis 7 für Agul
        /// </summary>
        public int MonatImJahr => (Zug - 1) % MonateProJahr;

        /// <summary>
        /// Der Name des Monats
        /// </summary>
        public string Name => IstGültig ? _namen[MonatImJahr] : "unbekannt";

        /// <summary>
        /// Die Jahreszeit, in der dieser Monat liegt
        /// </summary>
        public Jahreszeit Jahreszeit => _jahreszeiten[IstGültig ? MonatImJahr : 0];

        /// <summary>
        /// In den Rüstmonaten kann mit dem Geld des Reichsschatzes gerüstet werden (Regelwerk 3.3)
        /// </summary>
        public bool IstRüstmonat => IstGültig && _rüstmonate.Contains(MonatImJahr);

        /// <summary>
        /// Am Ende eines Einnahmemonats werden die Steuern eingetrieben (Regelwerk 3.2)
        /// </summary>
        public bool IstEinnahmemonat => IstGültig && _einnahmemonate.Contains(MonatImJahr);

        /// <summary>
        /// Der darauf folgende Spielzug
        /// </summary>
        public Zugmonat Nächster => new(Zug + 1);

        /// <summary>
        /// Der vorhergehende Spielzug
        /// </summary>
        public Zugmonat Vorheriger => new(Zug - 1);

        /// <summary>
        /// Eine kurze Beschreibung für die Anzeige, etwa "Larn (Sommer, Einnahmemonat)"
        /// </summary>
        public string Beschreibung {
            get {
                if (IstGültig == false)
                    return "unbekannter Zug";
                string zusatz = IstRüstmonat ? ", Rüstmonat" : IstEinnahmemonat ? ", Einnahmemonat" : string.Empty;
                return $"{Name} ({Jahreszeit}{zusatz})";
            }
        }

        public override string ToString() => IstGültig ? $"Zug {Zug} - {Beschreibung}, Jahr {Jahr}" : "unbekannter Zug";

        public bool Equals(Zugmonat? other) => other != null && other.Zug == Zug;
        public override bool Equals(object? obj) => Equals(obj as Zugmonat);
        public override int GetHashCode() => Zug.GetHashCode();
    }
}
