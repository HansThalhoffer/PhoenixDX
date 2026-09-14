using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für den Fernkampf (Regelwerk Kapitel 1.2 und 5.1).
    ///
    /// Zwei Dinge stehen hier nebeneinander: der Befehl und seine Auswertung.
    ///
    /// Der Befehl wird aufgenommen, geprüft und in der Spalte Befehl_ang abgelegt. Die Auswertung
    /// rechnet aus, was der Beschuss anrichtet - allerdings erst ab den Trefferpunkten. Wieviele
    /// Trefferpunkte ein Schuss bringt, steht in Tabelle 8 und wird mit einem W10 erwürfelt;
    /// Tabelle 8 gehört zur Tabellensammlung, nicht zum Regelwerk, und liegt im Datenbestand
    /// nicht vor. Der Würfel bleibt damit bei der Spielleitung, die Rechnerei nicht: "Um der SL
    /// die Arbeit zu erleichtern wird die Chance und das Ergebnis durch die IT, in der Auswertung
    /// ermittelt" (Regelwerk 1.5.11).
    ///
    /// Der Beschuss ist ein eigener Schritt vor dem Nahkampf: "Fernkampfwaffen, die in einem Monat
    /// angegriffen werden, in dem sie noch nicht geschossen haben, können vor dem Nahkampf auf den
    /// Angreifer schießen" (Regelwerk 1.2). Was danach übrig ist, geht in den Nahkampf - die
    /// Kampftabelle nennt diese Zwischenwerte "Werte nach Katabeschuss".
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

        /// <summary>
        /// Die Baupunkte einer Fernkampfwaffe (Regelwerk 1.2.1 bis 1.2.4).
        ///
        /// Leichte Waffen haben 200 Baupunkte, schwere 400 - das gilt für Katapulte wie für
        /// Kriegsschiffe. Zerstört ist eine Waffe, wenn sie die Hälfte davon verloren hat:
        /// "Ein LKP hat 200 Baupunkte und gilt mit nur noch 100 Baupunkten als zerstört."
        ///
        /// Achtung: die Kampftabelle rechnet im Nahkampf anders zurück. Dort kostet ein volles
        /// Geschütz einen vollen Satz Baupunkte (D103: Verlust mal 0,005 ergibt Stück je 200 BP),
        /// hier reicht die Hälfte. Deshalb steht der Unterschied in den offenen Regelfragen.
        /// </summary>
        public static int GetBaupunkte(Fernkampfwaffe waffe)
            => waffe == Fernkampfwaffe.Schwer ? 400 : 200;

        /// <summary>
        /// Der Schaden, ab dem eine Fernkampfwaffe zerstört ist - die Hälfte ihrer Baupunkte
        /// </summary>
        public static int GetZerstörungsschwelle(Fernkampfwaffe waffe) => GetBaupunkte(waffe) / 2;

        /// <summary>
        /// Die Wahrscheinlichkeit, mit der eine beschädigte Fernkampfwaffe zerstört ist
        /// (Regelwerk 1.5.11).
        ///
        /// "Die Beschädigung einer Einheit wird in % umgerechnet und dies ergibt die Chance mit
        /// welcher die Einheit zerstört wird, bzw. wieder repariert werden kann."
        ///
        /// Das Beispiel des Regelwerks: ein LKP mit 170 von 200 Baupunkten hat 30 von 100 möglichen
        /// Schadenspunkten, also 30 Prozent Zerstörungschance. Gewürfelt wird bei der Spielleitung;
        /// gerechnet wird hier.
        /// </summary>
        /// <returns>eine Wahrscheinlichkeit zwischen 0 und 1</returns>
        public static double BerechneZerstörungschance(Fernkampfwaffe waffe, double schadenInBaupunkten) {
            if (schadenInBaupunkten <= 0)
                return 0;
            return Math.Clamp(schadenInBaupunkten / GetZerstörungsschwelle(waffe), 0, 1);
        }

        /// <summary>
        /// Was ein Beschuss gegen einen Wall ausrichtet (Regelwerk 5.1).
        ///
        /// "Wälle - dabei wird im dahinter liegenden Gemark kein Schaden verursacht. Es muß jeder
        /// Wall einzeln anvisiert werden; überschüssige Trefferpunkte verfallen."
        ///
        /// Wer einen Wall beschiesst, trifft also nur den Wall, und was über dessen Baupunkte
        /// hinausgeht, ist verloren - es schlägt nicht auf das Feld dahinter durch.
        /// </summary>
        /// <returns>der Schaden am Wall in Baupunkten</returns>
        public static double BerechneWallschaden(double trefferpunkte, double baupunkteDesWalls) {
            if (trefferpunkte <= 0 || baupunkteDesWalls <= 0)
                return 0;
            return Math.Min(trefferpunkte, baupunkteDesWalls);
        }

        /// <summary>
        /// Ein beschossenes Heer mit dem, was es schützt.
        /// </summary>
        /// <param name="Heer">das Heer auf dem Zielfeld</param>
        /// <param name="Gutpunkte">
        /// die eigenen Gutpunkte des Heeres: Heerführer, Charaktere, Zauberer und ein Gardebonus
        /// (Kampftabelle C91: GP Heerführer plus GP Charakter plus GP Zauberer). Die Vorteile aus
        /// Gelände und Rüstort gehören nicht hierher - die schützen das Feld, nicht das Heer, und
        /// gehen als vorteileDesZiels in die Auswertung ein.
        /// </param>
        /// <param name="Gebannt">wieviele Truppen des Heeres gebannt sind (Spalte isbanned)</param>
        public record class Beschossen(TruppenSpielfigur Heer, double Gutpunkte = 0, int Gebannt = 0);

        /// <summary>
        /// Was ein Beschuss auf einem Zielfeld angerichtet hat.
        /// </summary>
        /// <param name="Trefferpunkte">die erwürfelten Trefferpunkte, wie sie hereinkamen</param>
        /// <param name="Schaden">was davon nach dem Schutz durch das Bauwerk übrig blieb</param>
        /// <param name="AufRüstort">der Anteil, der auf den Rüstort geht</param>
        /// <param name="AufTruppen">der Anteil, der auf die Heere geht</param>
        /// <param name="Verluste">je beschossenem Heer die Verluste, in der Reihenfolge der Übergabe</param>
        public record class Beschussergebnis(double Trefferpunkte, double Schaden, double AufRüstort,
                double AufTruppen, IReadOnlyList<KampfRules.Verluste> Verluste) {
            public static readonly Beschussergebnis Nichts = new(0, 0, 0, 0, []);
        }

        /// <summary>
        /// Wertet den Beschuss eines Zielfeldes aus (Regelwerk 5.1, Kampftabelle C2 bis E93).
        ///
        /// Drei Schritte, in dieser Reihenfolge:
        ///
        /// 1. "Alle Trefferpunkte in diesem Zielfeld werden addiert und durch die beim Verteidiger
        ///    vorhandenen Gutpunkte/100+1 dividiert" - das sind die Gutpunkte des Bauwerks und des
        ///    Geländes, die das ganze Feld schützen (Kampftabelle C85).
        /// 2. Steht ein Rüstort auf dem Feld, gehen 67 Prozent auf ihn und 33 Prozent auf die
        ///    Truppen; sonst tragen die Truppen alles.
        /// 3. "Jedes Heer wird danach von seinen eigenen GP nochmals geschützt", und der Schaden
        ///    verteilt sich nach dem Baupunktanteil der Heere (Kampftabelle E91 bis E93).
        ///
        /// Der Rest ist derselbe Weg wie im Nahkampf: innerhalb eines Heeres verteilt sich der
        /// Schaden nach der Zusammensetzung, und ein Gardeheer verliert nur ein Drittel.
        /// </summary>
        /// <param name="trefferpunkte">die Summe der erwürfelten Trefferpunkte auf dieses Feld</param>
        /// <param name="beschossene">die Heere auf dem Zielfeld</param>
        /// <param name="vorteileDesZiels">
        /// die Vorteile, die das Feld schützen - Rüstort, Wall, Fluss. Aus ihnen ergibt sich auch,
        /// ob überhaupt ein Rüstort verteidigt wird.
        /// </param>
        public static Beschussergebnis WerteBeschussAus(double trefferpunkte,
                IReadOnlyList<Beschossen>? beschossene, IEnumerable<Kampfvorteil>? vorteileDesZiels = null) {
            var ziele = beschossene ?? [];
            var vorteile = vorteileDesZiels?.ToList() ?? [];
            if (trefferpunkte <= 0 || ziele.Count == 0)
                return Beschussergebnis.Nichts with {
                    Verluste = [.. ziele.Select(_ => new KampfRules.Verluste())],
                };

            double schaden = KampfRules.MindereSchaden(trefferpunkte, KampfRules.BerechneVorteile(vorteile));
            var (aufRüstort, aufTruppen) = KampfRules.TeileFernkampfschaden(schaden, KampfRules.VerteidigtRüstort(vorteile));

            var basis = ziele
                .Select(ziel => (Baupunkte: KampfRules.BerechneWirksameBaupunkte(ziel.Heer, ziel.Gebannt),
                                 Gutpunkte: ziel.Gutpunkte))
                .ToList();
            double[] jeHeer = KampfRules.VerteileSchaden(aufTruppen, basis);

            var verluste = new List<KampfRules.Verluste>(ziele.Count);
            for (int i = 0; i < ziele.Count; i++)
                verluste.Add(KampfRules.BerechneVerluste(ziele[i].Heer, jeHeer[i]));

            return new Beschussergebnis(trefferpunkte, schaden, aufRüstort, aufTruppen, verluste);
        }
    }
}
