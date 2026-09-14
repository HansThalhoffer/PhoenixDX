using PhoenixModel.dbZugdaten;
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
    /// Was hier noch fehlt, weil es zum Zauberduell gehört und nicht zum Rückzugsgefecht: die
    /// verbrauchten Zauberkraftpunkte, die als Würfel mit dem festen Ergebnis 1 mitgeworfen
    /// werden, und die Regel, dass Treffer gegen so einen automatischen Würfel bei der
    /// Gutpunktrechnung aussen vor bleiben.
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
        public record class Würfelergebnis(int TrefferEines, int TrefferAnderes, int Parierte, int Verfallene) {
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
        public static Würfelergebnis WerteWürfelAus(IEnumerable<int>? eines, IEnumerable<int>? anderes) {
            var linke = (eines ?? []).Where(augen => augen > 0).OrderByDescending(augen => augen).ToList();
            var rechte = (anderes ?? []).Where(augen => augen > 0).OrderByDescending(augen => augen).ToList();

            int paare = Math.Min(linke.Count, rechte.Count);
            int trefferLinks = 0, trefferRechts = 0, pariert = 0;
            for (int i = 0; i < paare; i++) {
                if (linke[i] > rechte[i])
                    trefferLinks++;
                else if (rechte[i] > linke[i])
                    trefferRechts++;
                else
                    pariert++;
            }
            return new Würfelergebnis(trefferLinks, trefferRechts, pariert,
                Math.Abs(linke.Count - rechte.Count));
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
