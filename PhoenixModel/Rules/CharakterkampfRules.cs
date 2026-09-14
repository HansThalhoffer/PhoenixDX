using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Kampfrunde der Charaktere (Regelwerk 5.3).
    ///
    /// "Im Charakterkampf und im Zauberduell gibt es nur eine einzige Kampfrunde!"
    ///
    /// Gerechnet wird hier ab den Würfeln. Geworfen wird bei der Spielleitung - so wie beim
    /// Katapultbeschuss die Trefferpunkte und beim Nahkampf der W20. Was jeder Kämpfer wirft, sagt
    /// <see cref="GetWürfelzahl"/>: "jeder Kämpfer erhält pro Gutpunkt seines Charakters 1 W6 zum
    /// Würfeln. Bei Zauberern wird der aktuelle Zauberkraftwert für die Anzahl der W6 genommen."
    ///
    /// Diese Runde wird zweimal gebraucht: einmal als Charakterkampf vor dem Nahkampf, und einmal
    /// als Rückzugsgefecht danach (5.4, siehe <see cref="RückzugsRules"/>) - "Es gibt nach dem
    /// Nahkampf eine neue Kampfrunde des Charakterkampfes unter den zuvor beschriebenen Regeln."
    ///
    /// Das Zauberduell läuft nach denselben Regeln, mit drei Unterschieden (Regelwerk 5.3): die
    /// Grenze von vier Angreifern je Charakter gilt dort nicht, die verbrauchten
    /// Zauberkraftpunkte werden als Würfel mit dem festen Ergebnis 1 mitgeworfen, und Treffer
    /// gegen so einen automatischen Würfel bleiben bei der Gutpunktrechnung aussen vor.
    ///
    /// Nicht hier: wann es überhaupt zum Kampf kommt. Den Charakterkampf meldet
    /// <see cref="KonfliktRules"/> als Kampfart, die Zauberduelle sucht
    /// <see cref="KonfliktRules.FindeZauberduelle()"/>. Und der Zauberspruch, der ein Duell
    /// auslösen kann, gehört zur Zauberei.
    /// </summary>
    public static class CharakterkampfRules {

        /// <summary>
        /// "Es dürfen bis zu maximal 4 Personen auf einen Charakter schlagen." (Regelwerk 5.3)
        ///
        /// Für Zauberduelle gilt diese Grenze ausdrücklich nicht.
        /// </summary>
        public const int MaxAngreiferProCharakter = 4;

        /// <summary>
        /// Die Klassenstufen aus der Tabelle in Regelwerk 5.3.
        ///
        /// Weltlicher Charakter / Charakterzauberer / Klasse:
        /// Zivilist ZA 1, Heerführer ZB 2, Burgherr ZC 3, Stadthalter ZD 4, Festungsherr ZE 5,
        /// Herrscher ZF 6.
        /// </summary>
        public const int KlasseZivilist = 1;
        public const int KlasseHerrscher = 6;

        /// <summary>
        /// Die Klassenstufe eines weltlichen Charakters.
        ///
        /// Ohne Amt ist er Zivilist - Stufe 1.
        /// </summary>
        public static int GetKlassenstufe(Characterklasse klasse) => klasse switch {
            Characterklasse.HF => 2,
            Characterklasse.BUH => 3,
            Characterklasse.STH => 4,
            Characterklasse.FSH => 5,
            Characterklasse.HER => 6,
            _ => KlasseZivilist,
        };

        /// <summary>
        /// Die Klassenstufe eines Zauberers - ZA bis ZF sind die Stufen 1 bis 6.
        /// </summary>
        public static int GetKlassenstufe(Zaubererklasse klasse) => klasse switch {
            Zaubererklasse.ZA => 1,
            Zaubererklasse.ZB => 2,
            Zaubererklasse.ZC => 3,
            Zaubererklasse.ZD => 4,
            Zaubererklasse.ZE => 5,
            Zaubererklasse.ZF => 6,
            _ => KlasseZivilist,
        };

        /// <summary>
        /// Die Klassenstufe einer Figur, soweit sie sich aus ihr ablesen lässt.
        /// </summary>
        public static int GetKlassenstufe(NamensSpielfigur? figur) {
            return figur switch {
                Zauberer zauberer => GetKlassenstufe(zauberer.Klasse),
                Character charakter => GetKlassenstufe(View.CharacterView.GetAssumedKlasse(charakter)),
                _ => KlasseZivilist,
            };
        }

        /// <summary>
        /// Wieviele Würfel ein Kämpfer wirft: einen je aktuellem Gutpunkt, bei Zauberern je
        /// Zauberkraftpunkt (Regelwerk 5.3).
        /// </summary>
        public static int GetWürfelzahl(NamensSpielfigur? figur) => Math.Max(0, figur?.GP_akt ?? 0);

        /// <summary>
        /// Das Ergebnis einer Würfelauswertung.
        /// </summary>
        /// <param name="TrefferEines">die Treffer, die die erste Seite gelandet hat</param>
        /// <param name="TrefferAnderes">die Treffer der zweiten Seite</param>
        /// <param name="Parierte">Würfelpaare mit gleicher Augenzahl</param>
        /// <param name="Verfallene">Würfel, für die es kein Gegenstück gab</param>
        public record class Würfelergebnis(int TrefferEines, int TrefferAnderes, int Parierte, int Verfallene,
                int AnrechenbareTrefferEines = 0, int AnrechenbareTrefferAnderes = 0) {
            public static readonly Würfelergebnis Nichts = new(0, 0, 0, 0);

            /// <summary>
            /// "Sieger ist, wer in dieser Kampfrunde mehr Treffer gegen einen einzelnen Gegner
            /// gelandet als erhalten hat." Bei Gleichstand gewinnt niemand.
            /// </summary>
            public int Vorsprung => TrefferEines - TrefferAnderes;
        }

        /// <summary>
        /// Stellt die Würfel zweier Kämpfer gegeneinander (Regelwerk 5.3).
        ///
        /// "Zuerst alle 6er gegeneinander, dann die 5er, die 4er usw.. Hat ein Kämpfer die
        /// geforderte Zahl nicht mehr zum parieren, so muß er die nächst höhere Zahl seines Wurfes
        /// dagegen einsetzen! Würfel, die nicht mehr zu Würfelpaaren zusammengefügt werden können,
        /// entfallen. Würfelpaarungen mit gleicher Augenzahl sind parierte Angriffe. Ist ein
        /// Ergebnis bei einem Würfel eines Paares höher, so ist der höhere Wurf ein Treffer beim
        /// Gegner."
        ///
        /// Beides zusammen heisst: beide Seiten legen ihre Würfel der Grösse nach ab und paaren
        /// sie von oben herab. Wer die geforderte Zahl nicht hat, muss eine höhere opfern - das
        /// ist genau die Paarung, die sich ergibt, wenn man den jeweils höchsten verbliebenen
        /// Würfel gegen den höchsten des Gegners stellt. Was übrig bleibt, weil eine Seite mehr
        /// Würfel hat, entfällt.
        /// </summary>
        public static Würfelergebnis WerteWürfelAus(IEnumerable<int>? eines, IEnumerable<int>? anderes)
            => WerteWürfelAus((eines ?? []).Select(augen => new Würfel(augen)),
                              (anderes ?? []).Select(augen => new Würfel(augen)));

        /// <summary>
        /// Ein einzelner Würfel.
        /// </summary>
        /// <param name="Augen">das Ergebnis</param>
        /// <param name="Automatisch">
        /// ein Würfel, der nicht geworfen wurde, sondern fest auf 1 steht - im Zauberduell stehen
        /// die verbrauchten Zauberkraftpunkte dafür (Regelwerk 5.3)
        /// </param>
        public readonly record struct Würfel(int Augen, bool Automatisch = false);

        /// <summary>
        /// Dieselbe Auswertung, aber mit dem Wissen, welche Würfel automatische Einser sind.
        ///
        /// "von der Berechnung sind Treffer gegen eine "automatische 1" ausgenommen"
        /// (Regelwerk 5.3): solche Treffer zählen für den Ausgang, aber nicht für die Gutpunkte.
        /// Deshalb gibt es beide Zahlen.
        /// </summary>
        public static Würfelergebnis WerteWürfelAus(IEnumerable<Würfel>? eines, IEnumerable<Würfel>? anderes) {
            var linke = (eines ?? []).Where(w => w.Augen > 0).OrderByDescending(w => w.Augen).ToList();
            var rechte = (anderes ?? []).Where(w => w.Augen > 0).OrderByDescending(w => w.Augen).ToList();

            int paare = Math.Min(linke.Count, rechte.Count);
            int trefferLinks = 0, trefferRechts = 0, pariert = 0;
            int anrechenbarLinks = 0, anrechenbarRechts = 0;
            for (int i = 0; i < paare; i++) {
                if (linke[i].Augen > rechte[i].Augen) {
                    trefferLinks++;
                    if (rechte[i].Automatisch == false)
                        anrechenbarLinks++;
                }
                else if (rechte[i].Augen > linke[i].Augen) {
                    trefferRechts++;
                    if (linke[i].Automatisch == false)
                        anrechenbarRechts++;
                }
                else {
                    pariert++;
                }
            }
            return new Würfelergebnis(trefferLinks, trefferRechts, pariert,
                Math.Abs(linke.Count - rechte.Count), anrechenbarLinks, anrechenbarRechts);
        }

        /// <summary>
        /// Die Würfel eines Zauberers im Zauberduell (Regelwerk 5.3).
        ///
        /// "Bei einem Zauberduell bestimmt der aktuelle Zauberkraftwert die Anzahl der W6.
        /// Zusätzlich werden die verbrauchten Zauberkraftwerte (also die Differenz zwischen den
        /// Gutpunkten und dem aktuellen Zauberkraftwert) als W1 (als ein Würfel mit dem
        /// automatischen Ergebnis 1) berücksichtigt."
        ///
        /// "Bei neutralisierten Zauberern (siehe 1.3) werden die neutralisierten
        /// Zauberkraftpunkte wie verbrauchte Zauberkraftpunkte behandelt." - neutralisierte Punkte
        /// gehören deshalb hier als verbraucht hinein.
        /// </summary>
        /// <param name="zauberer">der Zauberer</param>
        /// <param name="würfe">was die Spielleitung für ihn geworfen hat</param>
        /// <param name="neutralisiert">wieviele seiner Zauberkraftpunkte neutralisiert sind</param>
        public static List<Würfel> BaueZauberduellwürfel(NamensSpielfigur? zauberer, IEnumerable<int>? würfe, int neutralisiert = 0) {
            List<Würfel> ergebnis = [.. (würfe ?? []).Where(augen => augen > 0).Select(augen => new Würfel(augen))];
            if (zauberer == null)
                return ergebnis;

            int verbraucht = Math.Max(0, zauberer.GP_ges - zauberer.GP_akt) + Math.Max(0, neutralisiert);
            for (int i = 0; i < verbraucht; i++)
                ergebnis.Add(new Würfel(1, Automatisch: true));
            return ergebnis;
        }

        /// <summary>
        /// Die Gutpunkte, die ein Kämpfer aus der Runde mitnimmt (Regelwerk 5.3).
        ///
        /// "[Treffer beim Gegner - Treffer vom Gegner] / [Klassenstufe(GP gesamt Siegerseite) :
        /// Klassenstufe(GP gesamt Verliererseite)]"
        ///
        /// Der Teiler ist für beide Beteiligten derselbe: wer verliert, bekommt denselben Betrag
        /// abgezogen, den der Sieger gutgeschrieben bekommt. Wer sich einem Höherrangigen stellt,
        /// gewinnt mehr - "da er sich mutig einem höheren gestellt hat".
        ///
        /// Gerundet wird auf ganze Gutpunkte, kaufmännisch: das Regelwerk rechnet in seinen vier
        /// Beispielen so (2,67 wird zu 3 und 1,5 zu 2), die Spalte GP_akt der Zugdaten kennt
        /// ohnehin nur ganze Zahlen. "Bei NPC Figuren wird der Gutpunktzuwachs abgerundet."
        /// </summary>
        /// <param name="treffer">die eigenen Treffer</param>
        /// <param name="gegentreffer">die Treffer des Gegners</param>
        /// <param name="klasseSieger">die Klassenstufe der siegreichen Seite</param>
        /// <param name="klasseVerlierer">die Klassenstufe der unterlegenen Seite</param>
        /// <param name="npc">eine Figur der Spielleitung? Dann wird abgerundet.</param>
        public static int BerechneGutpunktänderung(int treffer, int gegentreffer,
                int klasseSieger, int klasseVerlierer, bool npc = false) {
            double roh = BerechneGutpunktänderungGenau(treffer, gegentreffer, klasseSieger, klasseVerlierer);
            return npc
                ? (int)Math.Floor(roh)
                : (int)Math.Round(roh, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Nimmt diese Figur am Charakterkampf teil?
        ///
        /// "Charakterzauberer nehmen nicht am Charakterkampf teil." (Regelwerk 5.3) Sie haben
        /// stattdessen das Zauberduell und die Notteleportation.
        /// </summary>
        public static bool NimmtAmCharakterkampfTeil(NamensSpielfigur? figur)
            => figur != null && figur.Typ != ExternalTables.FigurType.CharakterZauberer;

        /// <summary>
        /// Eine Kampfpaarung: auf wen schlägt wer.
        /// </summary>
        /// <param name="Ziel">der Angegriffene</param>
        /// <param name="Angreifer">die Charaktere, die auf ihn schlagen</param>
        public record class Paarung(NamensSpielfigur Ziel, IReadOnlyList<NamensSpielfigur> Angreifer);

        /// <summary>
        /// Prüft eine Aufstellung von Kampfpaarungen (Regelwerk 5.3).
        ///
        /// "Jeder Verteidiger muß erst durch einen Angreifer in einen Kampf verwickelt werden,
        /// bevor zusätzliche Angreifer zu dieser Kampfpaarung hinzukommen können. Es dürfen bis zu
        /// maximal 4 Personen auf einen Charakter schlagen."
        ///
        /// Die Vier gilt im Zauberduell nicht: "Charakterkampf: Es dürfen bis zu maximal 4
        /// Personen auf einen Charakter schlagen. Dies gilt für Zauberduelle nicht."
        /// </summary>
        /// <param name="paarungen">die aufgestellten Paarungen</param>
        /// <param name="verteidiger">alle Charaktere, die angegriffen werden könnten</param>
        /// <param name="zauberduell">gilt die Viererregel nicht?</param>
        public static Result PrüfePaarungen(IReadOnlyList<Paarung>? paarungen,
                IEnumerable<NamensSpielfigur>? verteidiger, bool zauberduell = false) {
            var aufstellung = paarungen ?? [];
            if (aufstellung.Count == 0)
                return Result.Fail("Es ist keine Kampfpaarung aufgestellt",
                    "Ohne Paarung gibt es keinen Charakterkampf.");

            List<NamensSpielfigur> schonAmZug = [];
            foreach (var paarung in aufstellung) {
                if (paarung.Angreifer.Count == 0)
                    return Result.Fail($"Auf {paarung.Ziel.Bezeichner} schlägt niemand",
                        "Eine Paarung ohne Angreifer ist keine.");

                if (zauberduell == false && paarung.Angreifer.Count > MaxAngreiferProCharakter)
                    return Result.Fail($"Zu viele Angreifer auf {paarung.Ziel.Bezeichner}",
                        $"Es dürfen bis zu {MaxAngreiferProCharakter} Personen auf einen Charakter schlagen, "
                        + $"hier sind es {paarung.Angreifer.Count}.");

                foreach (var angreifer in paarung.Angreifer) {
                    if (schonAmZug.Any(bekannt => ReferenceEquals(bekannt, angreifer)))
                        return Result.Fail($"{angreifer.Bezeichner} schlägt auf zwei Gegner",
                            "Jeder Charakter schlägt sich mit einem Gegner.");
                    schonAmZug.Add(angreifer);
                }
            }

            // Erst müssen alle Verteidiger verwickelt sein, bevor ein zweiter Angreifer
            // dazukommt.
            var offene = (verteidiger ?? [])
                .Where(kandidat => aufstellung.Any(p => ReferenceEquals(p.Ziel, kandidat)) == false)
                .ToList();
            if (offene.Count > 0) {
                var gedoppelt = aufstellung.FirstOrDefault(p => p.Angreifer.Count > 1);
                if (gedoppelt != null)
                    return Result.Fail($"{offene[0].Bezeichner} ist noch nicht verwickelt",
                        $"Auf {gedoppelt.Ziel.Bezeichner} schlagen schon {gedoppelt.Angreifer.Count} Charaktere. "
                        + "Jeder Verteidiger muss erst in einen Kampf verwickelt werden, bevor zusätzliche "
                        + "Angreifer hinzukommen.");
            }

            return Result.Success($"{aufstellung.Count} Kampfpaarungen stehen",
                "Es wird gleichzeitig gewürfelt.");
        }

        /// <summary>
        /// "Jeder Zauberer stirbt im Zauberduell, sobald sein Gutpunktwert unter die Grenze von 1
        /// sinkt!" (Regelwerk 5.3)
        /// </summary>
        public const int ZaubererLebtAb = 1;

        /// <summary>
        /// Was aus einem Zauberer nach dem Duell wird.
        /// </summary>
        /// <param name="Gestorben">sein Gutpunktwert ist unter 1 gesunken</param>
        /// <param name="Zurückgeschleudert">
        /// er hat verloren oder unentschieden beendet und steht wieder in seiner Hauptstadt
        /// </param>
        /// <param name="Zauberkraftpunkte">was ihm davon bleibt</param>
        public record class Zauberduellfolge(bool Gestorben, bool Zurückgeschleudert, int Zauberkraftpunkte);

        /// <summary>
        /// Die Folgen eines Zauberduells (Regelwerk 5.3).
        ///
        /// "Zauberer die ein Zauberduell verloren oder unentschieden beendet haben werden in die
        /// Hauptstadt ihres Reiches zurück geschleudert und verlieren dabei alle noch vorhandenen
        /// Zauberkraftpunkte."
        ///
        /// Wer gewinnt, bleibt stehen und behält, was er hat.
        /// </summary>
        /// <param name="gutpunkteNachher">sein Gutpunktwert nach der Runde</param>
        /// <param name="vorsprung">seine Treffer minus die des Gegners</param>
        public static Zauberduellfolge BestimmeZauberduellfolge(int gutpunkteNachher, int vorsprung) {
            if (gutpunkteNachher < ZaubererLebtAb)
                return new Zauberduellfolge(Gestorben: true, Zurückgeschleudert: false, Zauberkraftpunkte: 0);
            if (vorsprung > 0)
                return new Zauberduellfolge(false, false, Math.Max(0, gutpunkteNachher));
            return new Zauberduellfolge(false, Zurückgeschleudert: true, Zauberkraftpunkte: 0);
        }

        /// <summary>
        /// Dieselbe Rechnung ohne Rundung - "Nur das Ansparen der Gutpunkte erfolgt mit
        /// Nachkommastellen." (Regelwerk 1.9.1)
        /// </summary>
        public static double BerechneGutpunktänderungGenau(int treffer, int gegentreffer,
                int klasseSieger, int klasseVerlierer) {
            if (klasseSieger <= 0 || klasseVerlierer <= 0)
                return 0;
            double teiler = (double)klasseSieger / klasseVerlierer;
            return (treffer - gegentreffer) / teiler;
        }
    }
}
