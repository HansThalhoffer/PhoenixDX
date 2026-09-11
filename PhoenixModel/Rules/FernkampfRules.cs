using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für den Fernkampf (Regelwerk Kapitel 1.2 und 5.1).
    ///
    /// Die Anwendung wertet den Beschuss nicht aus - sie nimmt den Befehl auf, prüft ihn und legt
    /// ihn in der Spalte Befehl_ang ab. Gewürfelt und verrechnet wird bei der Spielleitung anhand
    /// von Tabelle 8, die nicht Teil des Regelwerks, sondern der Tabellensammlung ist.
    ///
    /// Die Altanwendung hat den Beschuss nie zu Ende gebaut: ihr Dialog Unit_katapultieren wirft
    /// eine NotImplementedException, der Rumpf ist auskommentiert. Gelesen und auf der Karte
    /// angezeigt hat sie vorhandene Befehle aber sehr wohl. Die Prüfungen hier stammen daher teils
    /// aus dem auskommentierten Rumpf, teils direkt aus dem Regelwerk.
    /// </summary>
    public static class FernkampfRules {

        /// <summary>
        /// "Sie können nicht auf 0 Felder Entfernung schießen." (Regelwerk 5.1)
        /// </summary>
        public const int MinEntfernung = 1;

        /// <summary>
        /// Leichte Fernkampfwaffen wirken auf eine Gemark.
        /// </summary>
        public const int ReichweiteLeicht = 1;

        /// <summary>
        /// Schwere Fernkampfwaffen "können weiter oder höher schießen" (Regelwerk 1.2.2) - laut 5.1
        /// wirken Fernkampfwaffen "auf ein bis zwei Gemarken Entfernung". Auf die grössere
        /// Entfernung verringert sich der Schaden; das ist Sache der Spielleitung und steht in
        /// Tabelle 8.
        /// </summary>
        public const int ReichweiteSchwer = 2;

        /// <summary>
        /// Die Reichweite einer Bauart in Kleinfeldern
        /// </summary>
        public static int GetReichweite(Fernkampfwaffe waffe)
            => waffe == Fernkampfwaffe.Schwer ? ReichweiteSchwer : ReichweiteLeicht;

        /// <summary>
        /// Wieviele Geschütze dieser Bauart die Figur überhaupt hat.
        ///
        /// Schiffe führen ihre Kriegsschiffgeschütze in denselben Spalten wie die Landeinheiten
        /// ihre Katapulte, siehe <see cref="Fernkampfwaffe"/>.
        /// </summary>
        public static int GetVorhanden(Spielfigur? figur, Fernkampfwaffe waffe) {
            if (figur is not TruppenSpielfigur truppe)
                return 0;
            return waffe == Fernkampfwaffe.Schwer ? truppe.SKP : truppe.LKP;
        }

        /// <summary>
        /// Wieviele Geschütze dieser Bauart schon auf ein Ziel angesetzt sind.
        ///
        /// "Katapulte und Kriegsschiffe können generell nur einmal pro Monat schießen"
        /// (Regelwerk 1.2), deshalb zählt die Summe über alle Befehle, nicht der einzelne Befehl.
        /// </summary>
        public static int GetVerplant(Spielfigur? figur, Fernkampfwaffe waffe) {
            if (figur is not TruppenSpielfigur truppe)
                return 0;
            return Beschussbefehl.LiesAlle(truppe.Befehl_ang)
                .Where(befehl => befehl.Waffe == waffe)
                .Sum(befehl => befehl.Anzahl);
        }

        /// <summary>
        /// Wieviele Geschütze dieser Bauart noch frei sind
        /// </summary>
        public static int GetVerfügbar(Spielfigur? figur, Fernkampfwaffe waffe)
            => Math.Max(0, GetVorhanden(figur, waffe) - GetVerplant(figur, waffe));

        /// <summary>
        /// Die Entfernung zwischen zwei Kleinfeldern, gemessen in Schritten von Feld zu Feld.
        ///
        /// Gesucht wird in die Breite über die Nachbarschaftsbeziehung der Karte, statt aus den
        /// Koordinaten zu rechnen: Grossfeld und Kleinfeld bilden kein gleichmässiges Gitter, aus
        /// dem sich ein Abstand direkt ergäbe. Da nur Entfernungen bis zwei gebraucht werden, ist
        /// die Suche winzig.
        /// </summary>
        /// <returns>die Entfernung, oder -1 wenn das Ziel innerhalb von maxEntfernung nicht liegt</returns>
        public static int GetEntfernung(KleinfeldPosition? von, KleinfeldPosition? nach, int maxEntfernung) {
            if (von == null || nach == null)
                return -1;
            if (von.Equals(nach))
                return 0;

            var besucht = new HashSet<int> { von.Key };
            var aktuelleEbene = new List<KleinfeldPosition> { von };

            for (int entfernung = 1; entfernung <= maxEntfernung; entfernung++) {
                var nächsteEbene = new List<KleinfeldPosition>();
                foreach (var feld in aktuelleEbene) {
                    var nachbarn = KartenKoordinaten.GetKleinfeldNachbarn(feld);
                    if (nachbarn == null)
                        continue;
                    foreach (var nachbar in nachbarn) {
                        if (besucht.Add(nachbar.Key) == false)
                            continue;
                        if (nachbar.Equals(nach))
                            return entfernung;
                        nächsteEbene.Add(nachbar);
                    }
                }
                aktuelleEbene = nächsteEbene;
            }
            return -1;
        }

        /// <summary>
        /// Prüft einen geplanten Beschuss, ohne etwas zu verändern.
        /// </summary>
        /// <param name="figur">die Figur, die schiesst</param>
        /// <param name="waffe">leichte oder schwere Fernkampfwaffe</param>
        /// <param name="anzahl">wieviele Geschütze auf dieses Ziel schiessen sollen</param>
        /// <param name="ziel">das beschossene Kleinfeld</param>
        /// <param name="entfernung">die ermittelte Entfernung, wenn die Prüfung durchgeht</param>
        public static Result PrüfeBeschuss(Spielfigur? figur, Fernkampfwaffe waffe, int anzahl, KleinfeldPosition? ziel, out int entfernung) {
            entfernung = -1;

            if (figur == null)
                return Result.Fail("Es ist keine Figur ausgewählt", "Ohne Figur lässt sich kein Beschuss planen.");

            if (ZugView.KannBewegen == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht geschossen",
                    "Der Fernkampf gehört zum Spielzug; erst wenn die Rüstphase abgeschlossen ist, können Befehle für den Beschuss gegeben werden.");

            if (anzahl <= 0)
                return Result.Fail("Es muss mindestens eine Fernkampfwaffe schiessen",
                    $"Für {figur.Bezeichner} wurde die Anzahl {anzahl} angegeben.");

            string bezeichnung = waffe == Fernkampfwaffe.Schwer ? "schwere" : "leichte";

            int vorhanden = GetVorhanden(figur, waffe);
            if (vorhanden <= 0)
                return Result.Fail($"{figur.Bezeichner} hat keine {bezeichnung}n Fernkampfwaffen",
                    "Nur Einheiten mit Katapulten oder Kriegsschiffgeschützen können beschiessen.");

            int verfügbar = GetVerfügbar(figur, waffe);
            if (anzahl > verfügbar) {
                int verplant = GetVerplant(figur, waffe);
                return Result.Fail($"Soviele {bezeichnung} Fernkampfwaffen hat {figur.Bezeichner} nicht",
                    verplant > 0
                        ? $"Von {vorhanden} sind {verplant} schon auf andere Ziele angesetzt, frei sind noch {verfügbar}. "
                          + "Jede Fernkampfwaffe kann nur einmal im Monat schiessen."
                        : $"Vorhanden sind {vorhanden}, gefordert {anzahl}.");
            }

            if (ziel == null)
                return Result.Fail("Es ist kein Zielfeld angegeben", "Für einen Beschuss muss das Zielfeld feststehen.");

            var standort = new KleinfeldPosition(figur.gf_nach > 0 ? figur.gf_nach : figur.gf_von,
                                                 figur.gf_nach > 0 ? figur.kf_nach : figur.kf_von);
            if (standort.Equals(ziel))
                return Result.Fail("Auf das eigene Feld wird nicht geschossen",
                    "Fernkampfwaffen können nicht auf 0 Felder Entfernung schiessen (Regelwerk 5.1).");

            int reichweite = GetReichweite(waffe);
            entfernung = GetEntfernung(standort, ziel, reichweite);
            if (entfernung < MinEntfernung)
                return Result.Fail($"{ziel.CreateBezeichner()} liegt ausserhalb der Reichweite",
                    $"{bezeichnung.ToUpperInvariant()[0]}{bezeichnung[1..]} Fernkampfwaffen wirken auf {reichweite} "
                    + $"Gemark{(reichweite > 1 ? "en" : string.Empty)} Entfernung. Gemessen wird vom Feld "
                    + $"{standort.CreateBezeichner()}, auf dem die Figur am Ende ihrer Bewegung steht.");

            return Result.Success($"{anzahl} {bezeichnung} Fernkampfwaffen beschiessen {ziel.CreateBezeichner()}",
                $"Die Entfernung beträgt {entfernung} Gemark{(entfernung > 1 ? "en" : string.Empty)}.");
        }
    }
}
