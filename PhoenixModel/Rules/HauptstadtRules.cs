using PhoenixModel.dbPZE;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Hauptstadtverlegung (Regelwerk 1.5.13).
    ///
    /// "Von allen Rüstorten darf nur die Hauptstadt verlegt werden. Die Verlegung muß in eine
    /// bestehende Festung erfolgen und dauert 4 Monate. Im ersten Monat der Verlegung wird aus der
    /// alten Hauptstadt eine Festung ( dies ist bei der Einnahmeberechnung und bei der Rüstung zu
    /// beachten! ). Im vierten Monat wird aus der zuvor bezeichneten Festung die neue Hauptstadt.
    /// Für die Hauptstadtverlegung sind im ersten Monat 50.000 GS zu bezahlen und die Verlegung muß
    /// vorher offiziell an der Karte angekündigt werden."
    ///
    /// Zwei Schritte also, drei Monate auseinander: <see cref="Beginne"/> und
    /// <see cref="SchliesseAb"/>. Dazwischen hat das Reich keine Hauptstadt - das ist keine Lücke
    /// in der Umsetzung, sondern steht so im Regelwerk, und es hat Folgen: die alte Hauptstadt
    /// bringt in diesen Monaten nur noch die Einnahmen und die Rüstkapazität einer Festung, und der
    /// Herrscher sitzt auf keinem Rüstort seines Ranges mehr (1.9.1, siehe
    /// <see cref="BeförderungsRules.SenkeAufRüstortklasse"/>).
    ///
    /// Was die Anwendung nicht wissen kann: dass die drei Monate um sind. Eine laufende Verlegung
    /// steht nirgends in den Daten - es gibt keine Spalte dafür -, und ein Reich ohne Hauptstadt
    /// sieht genauso aus wie eines, das seine Hauptstadt im Krieg verloren hat. Der Abschluss wird
    /// deshalb befohlen und nicht errechnet; die Anwendung prüft, was sie prüfen kann, und nennt
    /// den Monat, in dem es soweit wäre (<see cref="GetAbschlussmonat"/>).
    ///
    /// Und was wird aus den Baupunkten? Das Regelwerk sagt es nicht. Eine Festung hat 3.000, eine
    /// Hauptstadt 5.000 (1.5.7, 1.5.8) - die Verlegung kostet aber nur 50.000 GS, während der
    /// Ausbau einer Festung zur Hauptstadt 100.000 GS kostet (1.5.8). Würde die bezeichnete Festung
    /// im vierten Monat einfach auf 5.000 Baupunkte springen, wäre die Verlegung der halbe Preis
    /// für dasselbe Bauwerk.
    ///
    /// Die Anwendung erfindet deshalb keine Baupunkte: verlegt wird die Bezeichnung. Die alte
    /// Hauptstadt wird zur Festung und behält höchstens deren 3.000 Baupunkte; die neue Hauptstadt
    /// behält die Baupunkte, die ihre Festung hatte, und ist damit eine Hauptstadt, der noch 2.000
    /// Baupunkte fehlen. Dass es diesen Zustand gibt, steht nicht nur in der Reparaturregel (1.5.10),
    /// sondern auch in den Spieldaten: die Hauptstadt von Theostelos steht mit 3.000 Baupunkten in
    /// der Karte. Die Frage liegt in Offene-Regelfragen.md.
    /// </summary>
    public static class HauptstadtRules {

        /// <summary>
        /// "Für die Hauptstadtverlegung sind im ersten Monat 50.000 GS zu bezahlen"
        /// </summary>
        public const int Kosten = 50_000;

        /// <summary>
        /// "Die Verlegung ... dauert 4 Monate."
        /// </summary>
        public const int Dauer = 4;

        public const string Festung = "Festung";
        public const string Hauptstadt = "Hauptstadt";
        public const string Festungshauptstadt = "Festungshauptstadt";

        /// <summary>
        /// Der Monat, in dem die Verlegung fertig wird: "Im vierten Monat wird aus der zuvor
        /// bezeichneten Festung die neue Hauptstadt."
        ///
        /// Der Beginn ist der erste der vier Monate, der Abschluss der vierte - dazwischen liegen
        /// drei Monate.
        /// </summary>
        public static int GetAbschlussmonat(int beginnmonat) => beginnmonat + Dauer - 1;

        /// <summary>
        /// Ist dieser Rüstort die Hauptstadt eines Reiches?
        ///
        /// Die Festungshauptstadt zählt mit: "Jedes Reich darf nur eine Hauptstadt ODER eine
        /// Festungshauptstadt besitzen" (Regelwerk 1.5.9).
        /// </summary>
        public static bool IstHauptstadt(Rüstort? rüstort)
            => RuestortRules.GetGrundstufe(rüstort) is Hauptstadt or Festungshauptstadt;

        /// <summary>
        /// Ist dieser Rüstort eine Festung - und keine Festungshauptstadt?
        /// </summary>
        public static bool IstFestung(Rüstort? rüstort) => RuestortRules.GetGrundstufe(rüstort) == Festung;

        /// <summary>
        /// Die Ausbaustufe aus der Referenztabelle, über ihren Namen
        /// </summary>
        public static Rüstort? GetStufe(string name)
            => SharedData.RüstortReferenz?.FirstOrDefault(stufe => stufe.Ruestort == name);

        /// <summary>
        /// Die Hauptstadt eines Reiches, oder null, wenn es keine hat.
        ///
        /// Keine zu haben ist ein gültiger Zustand: während einer Verlegung ist das so vorgesehen,
        /// und ein Reich kann seine Hauptstadt auch verlieren (Regelwerk 1.5.8).
        ///
        /// Gefragt ist hier die Bezeichnung, nicht der Zustand: eine zusammengeschossene Hauptstadt
        /// bleibt die Hauptstadt des Reiches, auch wenn sie in diesem Monat nur die Einnahmen und
        /// den Kampfvorteil einer Festung bringt. Deshalb die Sollstufe.
        /// </summary>
        public static KleinFeld? FindeHauptstadt(Nation? reich) {
            if (reich == null || SharedData.Map == null)
                return null;
            foreach (var gemark in SharedData.Map.Values) {
                if (gemark.Nation == null || gemark.Nation.Equals(reich) == false)
                    continue;
                if (IstHauptstadt(RuestortRules.GetSollstufe(gemark)))
                    return gemark;
            }
            return null;
        }

        /// <summary>
        /// Alle bestehenden Festungen eines Reiches - die möglichen Ziele einer Verlegung
        /// </summary>
        public static List<KleinFeld> FindeFestungen(Nation? reich) {
            List<KleinFeld> festungen = [];
            if (reich == null || SharedData.Map == null)
                return festungen;
            foreach (var gemark in SharedData.Map.Values) {
                if (gemark.Nation == null || gemark.Nation.Equals(reich) == false)
                    continue;
                if (IstBestehendeFestung(gemark))
                    festungen.Add(gemark);
            }
            return festungen;
        }

        /// <summary>
        /// Steht auf diesem Feld eine bestehende Festung?
        ///
        /// "bestehend" heisst fertig und als Festung ausgewiesen: die Karte führt in der Spalte
        /// Ruestort die Stufe, die dort stehen soll, und in den Baupunkten den tatsächlichen
        /// Zustand. Eine Festung, die auf 2.500 Baupunkte zusammengeschossen ist, soll zwar eine
        /// Festung sein, ist aber keine, in die man eine Hauptstadt verlegt - und eine beschädigte
        /// Hauptstadt, die gerade nur eine Festung trägt, ist die Hauptstadt des Reiches und kein
        /// Ziel.
        /// </summary>
        public static bool IstBestehendeFestung(KleinFeld? gemark) {
            if (gemark == null)
                return false;
            return IstFestung(RuestortRules.GetSollstufe(gemark))
                && IstFestung(BauwerkeView.GetRüstortNachKarte(gemark));
        }

        /// <summary>
        /// Prüft den Beginn einer Verlegung in die angegebene Festung.
        /// </summary>
        /// <param name="ziel">die Festung, in die verlegt werden soll</param>
        public static Result PrüfeBeginn(KleinFeld? ziel) {
            if (ziel == null)
                return Result.Fail("Es ist kein Ziel angegeben",
                    "Die Verlegung muss in eine bestehende Festung erfolgen (Regelwerk 1.5.13).");

            if (ConstructRules.IsAllowedForOwner(ziel) is Result besitzer && besitzer.HasErrors)
                return besitzer;
            if (ConstructRules.IsRüstPhase() is Result phase && phase.HasErrors)
                return phase;

            var reich = ziel.Nation;
            var alte = FindeHauptstadt(reich);
            if (alte == null)
                return Result.Fail($"{reich?.Reich} hat keine Hauptstadt, die sich verlegen liesse",
                    "Von allen Rüstorten darf nur die Hauptstadt verlegt werden (Regelwerk 1.5.13). "
                    + "Eine neue Hauptstadt entsteht durch Ausbau einer Festung für 100.000 GS (1.5.8).");

            if (alte.CreateBezeichner() == ziel.CreateBezeichner())
                return Result.Fail($"Auf {ziel.Bezeichner} steht die Hauptstadt schon",
                    "Verlegt wird an einen anderen Ort.");

            if (IstBestehendeFestung(ziel) == false) {
                var stand = BauwerkeView.GetRüstortNachKarte(ziel);
                return Result.Fail($"Auf {ziel.Bezeichner} steht keine bestehende Festung",
                    $"Dort steht {stand?.Ruestort ?? "nichts"}. Die Verlegung muss in eine bestehende "
                    + "Festung erfolgen (Regelwerk 1.5.13).");
            }

            if (ConstructRules.IsEnoughMoney(ziel, Kosten) is Result geld && geld.HasErrors)
                return geld;

            int abschluss = GetAbschlussmonat(ProgramView.SelectedMonth);
            return Result.Success($"Die Hauptstadt wird von {alte.Bezeichner} nach {ziel.Bezeichner} verlegt",
                $"Das kostet in diesem Monat {Kosten} GS und dauert {Dauer} Monate: {alte.Bezeichner} "
                + $"wird jetzt zur Festung, {ziel.Bezeichner} im Monat {abschluss} zur Hauptstadt. "
                + "Die Verlegung muss vorher offiziell an der Karte angekündigt werden.");
        }

        /// <summary>
        /// Prüft den Abschluss einer Verlegung - den vierten Monat.
        ///
        /// Ob die drei Monate wirklich um sind, lässt sich hier nicht feststellen (siehe oben);
        /// geprüft wird, dass das Reich keine Hauptstadt hat und auf dem Ziel eine bestehende
        /// Festung steht.
        /// </summary>
        public static Result PrüfeAbschluss(KleinFeld? ziel) {
            if (ziel == null)
                return Result.Fail("Es ist kein Ziel angegeben",
                    "Zum Abschluss gehört die Festung, die zur Hauptstadt wird.");

            if (ConstructRules.IsAllowedForOwner(ziel) is Result besitzer && besitzer.HasErrors)
                return besitzer;

            var alte = FindeHauptstadt(ziel.Nation);
            if (alte != null)
                return Result.Fail($"{ziel.Nation?.Reich} hat schon eine Hauptstadt auf {alte.Bezeichner}",
                    "Jedes Reich darf nur eine Hauptstadt haben (Regelwerk 1.5.8). Im ersten Monat der "
                    + "Verlegung wird aus der alten Hauptstadt eine Festung - dieser Schritt fehlt noch.");

            if (IstBestehendeFestung(ziel) == false) {
                var stand = BauwerkeView.GetRüstortNachKarte(ziel);
                return Result.Fail($"Auf {ziel.Bezeichner} steht keine bestehende Festung",
                    $"Dort steht {stand?.Ruestort ?? "nichts"}. Zur Hauptstadt wird die Festung, die "
                    + "bei Beginn der Verlegung bezeichnet wurde (Regelwerk 1.5.13).");
            }

            return Result.Success($"{ziel.Bezeichner} wird die neue Hauptstadt",
                "Im vierten Monat wird aus der zuvor bezeichneten Festung die neue Hauptstadt.");
        }

        /// <summary>
        /// Der erste Monat: "wird aus der alten Hauptstadt eine Festung ( dies ist bei der
        /// Einnahmeberechnung und bei der Rüstung zu beachten! )".
        ///
        /// Um beides kümmert sich die Anwendung von selbst, sobald die Stufe am Kleinfeld steht:
        /// <see cref="EinnahmenView"/> und die Rüstkapazität lesen beide den Rüstort der Karte.
        /// </summary>
        /// <returns>die alte Hauptstadt, die jetzt eine Festung ist, oder null</returns>
        public static KleinFeld? Beginne(KleinFeld? ziel) {
            var alte = FindeHauptstadt(ziel?.Nation);
            if (alte == null)
                return null;
            return SenkeAufStufe(alte, Festung) ? alte : null;
        }

        /// <summary>
        /// Der vierte Monat: "wird aus der zuvor bezeichneten Festung die neue Hauptstadt".
        ///
        /// Eine gewöhnliche Hauptstadt, auch wenn die alte eine Festungshauptstadt war - das
        /// Regelwerk kennt für die Verlegung nur die Hauptstadt.
        ///
        /// Die Baupunkte bleiben stehen (siehe oben): aus der Festung mit 3.000 Baupunkten wird
        /// eine Hauptstadt, der noch 2.000 fehlen.
        /// </summary>
        public static bool SchliesseAb(KleinFeld? ziel) => SetzeBezeichnung(ziel, Hauptstadt);

        /// <summary>
        /// Setzt die Ausbaustufe eines Kleinfeldes, ohne die Baupunkte anzufassen.
        ///
        /// Die Spalte Ruestort nennt die Stufe, die dort stehen soll, die Spalte Baupunkte den
        /// tatsächlichen Zustand (siehe <see cref="RuestortRules"/>). Hier ändert sich nur die
        /// erste.
        /// </summary>
        public static bool SetzeBezeichnung(KleinFeld? gemark, string stufenname) {
            var stufe = GetStufe(stufenname);
            if (gemark == null || stufe == null || stufe.Nummer == null)
                return false;
            gemark.Ruestort = stufe.Nummer;
            return true;
        }

        /// <summary>
        /// Setzt die Ausbaustufe und deckelt die Baupunkte darauf.
        ///
        /// Wer absteigt, behält nicht die Steine der höheren Stufe: aus einer Hauptstadt mit 5.000
        /// Baupunkten wird eine Festung mit 3.000. Was schon darunter lag, bleibt liegen - eine
        /// beschädigte Hauptstadt wird zu einer ebenso beschädigten Festung.
        /// </summary>
        public static bool SenkeAufStufe(KleinFeld? gemark, string stufenname) {
            var stufe = GetStufe(stufenname);
            if (gemark == null || stufe == null || stufe.Nummer == null || stufe.Baupunkte == null)
                return false;
            gemark.Ruestort = stufe.Nummer;
            gemark.Baupunkte = Math.Min(gemark.Baupunkte, stufe.Baupunkte.Value);
            return true;
        }
    }
}
