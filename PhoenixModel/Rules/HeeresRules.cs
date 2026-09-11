using PhoenixModel.ExternalTables;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Der Anteil, der beim Teilen eines Heeres abgegeben wird
    /// </summary>
    public class Heeresanteil {
        public int Stärke { get; set; }
        public int Heerführer { get; set; }
        public int LKP { get; set; }
        public int SKP { get; set; }
        public int Pferde { get; set; }
        public int GS { get; set; }
        public int Kampfeinnahmen { get; set; }

        public bool IstLeer => Stärke <= 0 && Heerführer <= 0 && LKP <= 0 && SKP <= 0 && Pferde <= 0;
        public bool HatNegativeWerte => Stärke < 0 || Heerführer < 0 || LKP < 0 || SKP < 0 || Pferde < 0 || GS < 0 || Kampfeinnahmen < 0;

        public override string ToString() {
            var teile = new List<string>();
            if (Stärke > 0) teile.Add($"{Stärke} Stärke");
            if (Heerführer > 0) teile.Add($"{Heerführer} Heerführer");
            if (Pferde > 0) teile.Add($"{Pferde} Pferde");
            if (LKP > 0) teile.Add($"{LKP} leichte Fernkampfwaffen");
            if (SKP > 0) teile.Add($"{SKP} schwere Fernkampfwaffen");
            if (GS > 0) teile.Add($"{GS} GS");
            if (Kampfeinnahmen > 0) teile.Add($"{Kampfeinnahmen} Kampfeinnahmen");
            return teile.Count == 0 ? "nichts" : string.Join(", ", teile);
        }
    }

    /// <summary>
    /// Die Regeln für die Zusammensetzung von Heeren (Regelwerk Kapitel 1.8).
    ///
    /// "Die Heere können innerhalb ihrer Gattung beliebig geteilt und fusioniert werden, wenn sie
    /// sich im selben Monat auf derselben Gemark aufhalten." und "Jedes Heer muss mindestens 100
    /// Raumpunkte plus einen Heerführer oder Adeligen haben, der das Heer befehligt."
    ///
    /// Gardeheere sind ausgenommen: "Sie können mit keinen anderen Heeren fusionieren."
    /// (Regelwerk 6.8)
    /// </summary>
    public static class HeeresRules {

        /// <summary>
        /// Die Mindestgrösse eines Heeres in Raumpunkten (Regelwerk 1.8)
        /// </summary>
        public const int MinRaumpunkte = 100;

        /// <summary>
        /// Jedes Heer braucht einen Heerführer oder Adeligen, der es befehligt (Regelwerk 1.8)
        /// </summary>
        public const int MinHeerführer = 1;

        /// <summary>
        /// Wieviele Heere es je Gattung geben kann: die Nummernkreise gehen von 101 bis 199,
        /// von 201 bis 299 und so weiter (Regelwerk 1.8).
        /// </summary>
        public const int HeereProGattung = 99;

        /// <summary>
        /// Die erste Nummer des Nummernkreises, in dem eine Truppe dieser Gattung geführt wird
        /// </summary>
        public static int GetStartNummer(FigurType baseTyp) => baseTyp switch {
            FigurType.Krieger => Krieger.StartNummer,
            FigurType.Reiter => Reiter.StartNummer,
            FigurType.Schiff => Schiffe.StartNummer,
            FigurType.Kreatur => Kreaturen.StartNummer,
            _ => 0,
        };

        /// <summary>
        /// Gehören beide Truppen derselben Gattung an? "Die Rüstgüter Krieger, Reiter und Schiffe
        /// dürfen nicht gemischt werden." (Regelwerk 1.8)
        /// </summary>
        public static bool IstGleicheGattung(TruppenSpielfigur? eine, TruppenSpielfigur? andere)
            => eine != null && andere != null && eine.BaseTyp == andere.BaseTyp;

        /// <summary>
        /// Das Feld, auf dem die Truppe am Ende ihrer Bewegung steht
        /// </summary>
        public static KleinfeldPosition GetStandort(TruppenSpielfigur truppe)
            => new(truppe.gf_nach > 0 ? truppe.gf_nach : truppe.gf_von,
                   truppe.gf_nach > 0 ? truppe.kf_nach : truppe.kf_von);

        /// <summary>
        /// Sucht die nächste freie Nummer im Nummernkreis der Gattung.
        /// </summary>
        /// <returns>die freie Nummer, oder null wenn der Nummernkreis voll ist</returns>
        public static int? FindeFreieNummer(TruppenSpielfigur vorbild) => FindeFreieNummer(vorbild.BaseTyp);

        /// <summary>
        /// Sucht die nächste freie Nummer im Nummernkreis einer Gattung.
        ///
        /// Beim Aufsitzen entsteht aus einem Kriegerheer ein Reiterheer, die Nummer muss also im
        /// Nummernkreis der Zielgattung gesucht werden und nicht in dem der Ausgangsgattung.
        /// </summary>
        /// <returns>die freie Nummer, oder null wenn der Nummernkreis voll ist</returns>
        public static int? FindeFreieNummer(FigurType baseTyp) {
            int start = GetStartNummer(baseTyp);
            if (start == 0)
                return null;

            var vergeben = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(truppe => truppe.BaseTyp == baseTyp)
                .Select(truppe => truppe.Nummer)
                .ToHashSet();

            for (int nummer = start + 1; nummer <= start + HeereProGattung; nummer++)
                if (vergeben.Contains(nummer) == false)
                    return nummer;
            return null;
        }

        /// <summary>
        /// Prüft, ob ein Heer so geteilt werden kann, ohne etwas zu verändern.
        ///
        /// Beide Hälften müssen für sich genommen ein gültiges Heer ergeben: mindestens ein
        /// Heerführer und mindestens 100 Raumpunkte.
        /// </summary>
        /// <remarks>
        /// Das Regelwerk erlaubt das Teilen "beliebig", solange die Heere im selben Monat auf
        /// derselben Gemark stehen - eine bereits erfolgte Bewegung steht dem also nicht entgegen.
        /// Die Errata 17 knüpfen nur das Verschieben von Heerführern zwischen zwei bestehenden
        /// Heeren daran, dass das abgebende Heer sich noch nicht bewegt hat.
        /// </remarks>
        public static Result PrüfeTeilung(TruppenSpielfigur? heer, Heeresanteil? anteil) {
            if (heer == null)
                return Result.Fail("Es ist kein Heer ausgewählt", "Ohne Heer lässt sich nichts abspalten.");
            if (anteil == null || anteil.IstLeer)
                return Result.Fail("Es wurde nichts zum Abspalten angegeben",
                    "Das neue Heer braucht wenigstens einen Heerführer und Truppen.");
            if (anteil.HatNegativeWerte)
                return Result.Fail("Negative Mengen lassen sich nicht abspalten", anteil.ToString());

            if (anteil.Stärke > heer.staerke || anteil.Heerführer > heer.hf || anteil.LKP > heer.LKP
                || anteil.SKP > heer.SKP || anteil.Pferde > heer.Pferde
                || anteil.GS > heer.GS || anteil.Kampfeinnahmen > heer.Kampfeinnahmen)
                return Result.Fail($"Soviel hat {heer.Bezeichner} nicht",
                    $"Abgespalten werden sollen {anteil}. Vorhanden sind {heer.staerke} Stärke, {heer.hf} Heerführer, "
                    + $"{heer.Pferde} Pferde, {heer.LKP} leichte und {heer.SKP} schwere Fernkampfwaffen, "
                    + $"{heer.GS} GS und {heer.Kampfeinnahmen} Kampfeinnahmen.");

            if (anteil.Heerführer < MinHeerführer)
                return Result.Fail("Das neue Heer braucht einen Heerführer",
                    "Jedes Heer muss einen Heerführer oder Adeligen haben, der es befehligt (Regelwerk 1.8).");
            if (heer.hf - anteil.Heerführer < MinHeerführer)
                return Result.Fail($"{heer.Bezeichner} bliebe ohne Heerführer zurück",
                    $"Von {heer.hf} Heerführern sollen {anteil.Heerführer} mitgehen. Auch das zurückbleibende Heer "
                    + "braucht einen, der es befehligt (Regelwerk 1.8).");

            int raumpunkteAnteil = SpielfigurRules.BerechneRaumpunkte(heer,
                anteil.Stärke, anteil.Pferde, anteil.Heerführer, anteil.LKP, anteil.SKP);
            if (raumpunkteAnteil < MinRaumpunkte)
                return Result.Fail("Das neue Heer wäre zu klein",
                    $"Es käme auf {raumpunkteAnteil} Raumpunkte; ein Heer braucht mindestens {MinRaumpunkte} (Regelwerk 1.8).");

            int raumpunkteRest = SpielfigurRules.BerechneRaumpunkte(heer,
                heer.staerke - anteil.Stärke, heer.Pferde - anteil.Pferde, heer.hf - anteil.Heerführer,
                heer.LKP - anteil.LKP, heer.SKP - anteil.SKP);
            if (raumpunkteRest < MinRaumpunkte)
                return Result.Fail($"{heer.Bezeichner} bliebe zu klein zurück",
                    $"Es behielte {raumpunkteRest} Raumpunkte; ein Heer braucht mindestens {MinRaumpunkte} (Regelwerk 1.8).");

            if (FindeFreieNummer(heer) == null)
                return Result.Fail("Es ist keine Nummer mehr frei",
                    $"Im Nummernkreis ab {GetStartNummer(heer.BaseTyp)} sind alle {HeereProGattung} Nummern vergeben.");

            return Result.Success($"{heer.Bezeichner} kann geteilt werden",
                $"Abgespalten werden {anteil} mit {raumpunkteAnteil} Raumpunkten; zurück bleiben {raumpunkteRest}.");
        }

        /// <summary>
        /// Prüft, ob zwei Heere fusioniert werden können, ohne etwas zu verändern.
        /// </summary>
        /// <param name="quelle">das Heer, das bestehen bleibt</param>
        /// <param name="ziel">das Heer, das darin aufgeht</param>
        public static Result PrüfeFusion(TruppenSpielfigur? quelle, TruppenSpielfigur? ziel) {
            if (quelle == null || ziel == null)
                return Result.Fail("Für eine Fusion braucht es zwei Heere", "Es wurde nicht beides ausgewählt.");
            if (ReferenceEquals(quelle, ziel) || quelle.Nummer == ziel.Nummer)
                return Result.Fail("Ein Heer kann nicht mit sich selbst fusionieren", quelle.Bezeichner);

            if (IstGleicheGattung(quelle, ziel) == false)
                return Result.Fail("Die Gattungen passen nicht zusammen",
                    $"{quelle.Bezeichner} und {ziel.Bezeichner} gehören verschiedenen Gattungen an. "
                    + "Krieger, Reiter und Schiffe dürfen nicht gemischt werden (Regelwerk 1.8).");

            if (quelle.Garde || ziel.Garde)
                return Result.Fail("Gardeheere fusionieren nicht",
                    "Gardeheere und Gardeflotten können mit keinen anderen Heeren fusionieren (Regelwerk 6.8).");

            var standortQuelle = GetStandort(quelle);
            var standortZiel = GetStandort(ziel);
            if (standortQuelle.Equals(standortZiel) == false)
                return Result.Fail("Die Heere stehen nicht auf derselben Gemark",
                    $"{quelle.Bezeichner} steht auf {standortQuelle.CreateBezeichner()}, {ziel.Bezeichner} auf "
                    + $"{standortZiel.CreateBezeichner()}. Fusioniert wird nur auf derselben Gemark (Regelwerk 1.8).");

            return Result.Success($"{ziel.Bezeichner} geht in {quelle.Bezeichner} auf",
                $"Zusammen ergibt das {quelle.staerke + ziel.staerke} Stärke und {quelle.hf + ziel.hf} Heerführer "
                + $"auf {standortQuelle.CreateBezeichner()}.");
        }
    }
}
