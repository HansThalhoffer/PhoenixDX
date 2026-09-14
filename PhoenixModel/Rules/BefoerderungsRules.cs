using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Beförderung und Degradierung von Charakteren (Regelwerk 1.9.1).
    ///
    /// "Wird ein Charakter befördert so rückt er Augenblicklich auf seine aktuelle Position auf,
    /// es ändert sich hier dann sofort sein Gutpunktmaxium, die aktuellen Gutpunkte bleiben. Die
    /// Sperrfrist ist somit aufgehoben, ergibt sich aber automatisch aus der monatlichen
    /// Gutpunktregeneration. Wird ein Charakter degradiert, so sinken seine maximalen und
    /// aktuellen Gutpunkte."
    ///
    /// Das ist der Kern: nach oben steigt nur das Maximum, nach unten fällt beides. Der Rest
    /// ergibt sich von selbst - ein beförderter Charakter füllt sein neues Maximum mit den fünf
    /// Gutpunkten auf, die er im Monat regeneriert, und ist trotzdem sofort im Amt.
    ///
    /// Zwei Bedingungen stehen davor: "Die Positionen Burgherr, Stadthalter, Festungsherr und
    /// Herrscher sind in jedem Reich nur einmalig vorhanden" und "können nur besetzt werden, wenn
    /// für jeden Rang der erforderliche Rüstort vorhanden ist".
    ///
    /// Das Amt steht in der Beschriftung der Figur - dort liest es auch
    /// <see cref="CharacterView.GetAssumedKlasse"/> ab. Eine Beförderung schreibt es deshalb um.
    /// </summary>
    public static class BeförderungsRules {

        /// <summary>
        /// Das Gutpunktmaximum eines Amtes.
        ///
        /// Burgherr 24, Stadthalter 36 und Herrscher 60 nennt das Beförderungsbeispiel in 1.9.1
        /// ausdrücklich. Für den Festungsherrn nennt das Regelwerk keinen Wert; er steht im Rang
        /// dazwischen und ist hier mit 48 eingeordnet. Die 12 des Heerführers stammen aus dem
        /// durchgerechneten Beispiel in 5.3, wo zwei Charakterheerführer mit je 12 Gutpunkten
        /// antreten. Beides steht in den offenen Regelfragen.
        /// </summary>
        public static int GetGutpunktmaximum(Characterklasse klasse) => klasse switch {
            Characterklasse.HF => 12,
            Characterklasse.BUH => 24,
            Characterklasse.STH => 36,
            Characterklasse.FSH => 48,
            Characterklasse.HER => 60,
            _ => 0,
        };

        /// <summary>
        /// Der Rüstort, den ein Amt braucht (Regelwerk 1.5.5 bis 1.5.8 und 1.9.1).
        ///
        /// Eine Burg "kann der Stammsitz des Burgherren sein", eine Stadt der des Burgherren oder
        /// Stadthalters, eine Festung der des Burgherren, Stadthalters oder Festungsherrn. Jedes
        /// Amt braucht also einen Rüstort mindestens seiner eigenen Klasse; ein grösserer tut es
        /// auch.
        /// </summary>
        public static string? GetErforderlichenRüstort(Characterklasse klasse) => klasse switch {
            Characterklasse.BUH => "Burg",
            Characterklasse.STH => "Stadt",
            Characterklasse.FSH => "Festung",
            Characterklasse.HER => "Hauptstadt",
            _ => null,
        };

        /// <summary>
        /// Hat dieses Reich einen Rüstort, der für dieses Amt reicht?
        ///
        /// Verglichen wird über die Baupunkte der Ausbaustufe: was mindestens soviel hat wie die
        /// geforderte Stufe, trägt das Amt.
        /// </summary>
        public static bool HatErforderlichenRüstort(Nation? reich, Characterklasse klasse) {
            string? gefordert = GetErforderlichenRüstort(klasse);
            if (gefordert == null)
                return true;
            if (reich == null || SharedData.Map == null || SharedData.RüstortReferenz == null)
                return false;

            var stufe = SharedData.RüstortReferenz.FirstOrDefault(r => r.Ruestort == gefordert);
            if (stufe == null)
                return false;

            foreach (var gemark in SharedData.Map.Values) {
                if (gemark.Nation == null || gemark.Nation.Equals(reich) == false)
                    continue;
                var rüstort = BauwerkeView.GetRüstortNachKarte(gemark);
                if (rüstort != null && rüstort.Baupunkte >= stufe.Baupunkte)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Ist dieses Amt im Reich schon besetzt?
        ///
        /// "Die Positionen Burgherr, Stadthalter, Festungsherr und Herrscher sind in jedem Reich
        /// nur einmalig vorhanden" - der Heerführer nicht, von denen gibt es viele.
        /// </summary>
        /// <param name="ausser">ein Charakter, der nicht zählt - gewöhnlich der, um den es geht</param>
        public static bool IstAmtBesetzt(Nation? reich, Characterklasse klasse, Character? ausser = null) {
            if (reich == null || klasse == Characterklasse.HF || klasse == Characterklasse.none)
                return false;

            foreach (var figur in SpielfigurenView.GetSpielfiguren(reich)) {
                if (figur is not Character charakter || ReferenceEquals(charakter, ausser))
                    continue;
                if (CharacterView.GetAssumedKlasse(charakter) == klasse)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Prüft eine Beförderung.
        /// </summary>
        public static Result PrüfeBeförderung(Character? charakter, Characterklasse ziel) {
            if (charakter == null)
                return Result.Fail("Es ist kein Charakter ausgewählt", "Befördert wird ein Charakter.");
            if (ziel == Characterklasse.none)
                return Result.Fail("Es ist kein Amt angegeben", "Befördert wird in ein Amt.");

            var jetzt = CharacterView.GetAssumedKlasse(charakter);
            if (GetGutpunktmaximum(ziel) <= GetGutpunktmaximum(jetzt))
                return Result.Fail($"{charakter.Bezeichner} ist schon {jetzt} oder höher",
                    $"Eine Beförderung führt nach oben; {ziel} liegt nicht über {jetzt}. Zum Absteigen "
                    + "gibt es die Degradierung.");

            if (HatErforderlichenRüstort(charakter.Nation, ziel) == false)
                return Result.Fail($"Für das Amt {ziel} fehlt der Rüstort",
                    $"Die Position kann nur besetzt werden, wenn eine {GetErforderlichenRüstort(ziel)} "
                    + "oder ein grösserer Rüstort im Reich steht (Regelwerk 1.9.1).");

            if (IstAmtBesetzt(charakter.Nation, ziel, charakter))
                return Result.Fail($"Das Amt {ziel} ist bereits besetzt",
                    "Burgherr, Stadthalter, Festungsherr und Herrscher gibt es in jedem Reich nur einmal "
                    + "(Regelwerk 1.9.1). Der bisherige Amtsträger muss erst degradiert werden.");

            return Result.Success($"{charakter.Bezeichner} wird {ziel}",
                $"Sein Gutpunktmaximum steigt auf {GetGutpunktmaximum(ziel)}; die aktuellen "
                + $"{charakter.GP_akt} Gutpunkte bleiben und wachsen monatlich nach.");
        }

        /// <summary>
        /// Prüft eine Degradierung.
        ///
        /// Nach unten braucht es keinen Rüstort und keine freie Stelle - wer absteigt, macht Platz.
        /// </summary>
        public static Result PrüfeDegradierung(Character? charakter, Characterklasse ziel) {
            if (charakter == null)
                return Result.Fail("Es ist kein Charakter ausgewählt", "Degradiert wird ein Charakter.");

            var jetzt = CharacterView.GetAssumedKlasse(charakter);
            if (GetGutpunktmaximum(ziel) >= GetGutpunktmaximum(jetzt))
                return Result.Fail($"{charakter.Bezeichner} ist schon {jetzt}",
                    $"Eine Degradierung führt nach unten; {ziel} liegt nicht unter {jetzt}.");

            return Result.Success($"{charakter.Bezeichner} wird {ziel}",
                $"Maximale und aktuelle Gutpunkte sinken auf {GetGutpunktmaximum(ziel)}.");
        }

        /// <summary>
        /// Befördert einen Charakter: "es ändert sich hier dann sofort sein Gutpunktmaxium, die
        /// aktuellen Gutpunkte bleiben."
        /// </summary>
        public static void Befördere(Character? charakter, Characterklasse ziel) {
            if (charakter == null)
                return;
            charakter.GP_ges = GetGutpunktmaximum(ziel);
            charakter.Beschriftung = SetzeAmt(charakter.Beschriftung, ziel);
        }

        /// <summary>
        /// Degradiert einen Charakter: "so sinken seine maximalen und aktuellen Gutpunkte".
        ///
        /// Das Beispiel des Regelwerks setzt den abgedankten Herrscher auf 24/24 und merkt an,
        /// dass "zusätzlich erworbene Gutpunkte natürlich auch berücksichtigt" werden. Welche
        /// Gutpunkte zusätzlich erworben wurden, steht nirgends - die Zugdaten führen nur den
        /// Stand. Gesetzt wird deshalb der Wert des neuen Amtes.
        /// </summary>
        public static void Degradiere(Character? charakter, Characterklasse ziel) {
            if (charakter == null)
                return;
            int maximum = GetGutpunktmaximum(ziel);
            charakter.GP_ges = maximum;
            charakter.GP_akt = Math.Min(charakter.GP_akt, maximum);
            charakter.Beschriftung = SetzeAmt(charakter.Beschriftung, ziel);
        }

        /// <summary>
        /// Der Rüstort eines Adligen wurde kleiner: "so sinken die aktuellen Gutpunkte des Adligen
        /// auf die der Rüstortklasse entsprechenden. Der Rang des Adligen bleibt dennoch erhalten!"
        /// (Regelwerk 1.9.1)
        ///
        /// Das Amt bleibt also stehen - nur die aktuellen Gutpunkte fallen auf das, was der
        /// kleinere Rüstort trägt. Das Maximum bleibt ebenfalls: der Rang ist geblieben.
        /// </summary>
        /// <returns>true, wenn etwas gesunken ist</returns>
        public static bool SenkeAufRüstortklasse(Character? charakter, KleinFeld? rüstort) {
            if (charakter == null)
                return false;
            var stufe = rüstort == null ? null : BauwerkeView.GetRüstortNachKarte(rüstort);
            var klasse = GetKlasseZuRüstort(stufe?.Ruestort);
            int maximum = GetGutpunktmaximum(klasse);
            if (charakter.GP_akt <= maximum)
                return false;
            charakter.GP_akt = maximum;
            return true;
        }

        /// <summary>
        /// Welches Amt ein Rüstort trägt - die Umkehrung von <see cref="GetErforderlichenRüstort"/>
        /// </summary>
        public static Characterklasse GetKlasseZuRüstort(string? rüstort) => rüstort switch {
            "Burg" => Characterklasse.BUH,
            "Stadt" => Characterklasse.STH,
            "Festung" => Characterklasse.FSH,
            "Hauptstadt" or "Festungshauptstadt" => Characterklasse.HER,
            _ => Characterklasse.HF,
        };

        /// <summary>
        /// Schreibt das Amt in die Beschriftung, ohne die Nummer dahinter zu verlieren.
        ///
        /// Aus "HF0815" wird "BUH0815", aus "STH" wird "HER". Was kein bekanntes Amt vorne trägt,
        /// bekommt das neue davorgesetzt.
        /// </summary>
        public static string SetzeAmt(string? beschriftung, Characterklasse ziel) {
            string kürzel = ziel == Characterklasse.none ? string.Empty : ziel.ToString();
            string rest = (beschriftung ?? string.Empty).Trim();

            foreach (var amt in new[] { Characterklasse.HER, Characterklasse.FSH, Characterklasse.STH,
                                        Characterklasse.BUH, Characterklasse.HF }) {
                // Der Datenbestand kennt auch die Schreibweise mit C davor - CHF für den
                // Charakterheerführer. Auch die gilt als Amt und wird ersetzt.
                foreach (string vorhanden in new[] { amt.ToString(), "C" + amt.ToString() }) {
                    if (rest.StartsWith(vorhanden, StringComparison.OrdinalIgnoreCase)) {
                        return kürzel + rest[vorhanden.Length..];
                    }
                }
            }
            return kürzel + rest;
        }
    }
}
