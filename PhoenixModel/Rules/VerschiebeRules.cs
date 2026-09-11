using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Was sich zwischen zwei Heeren verschieben lässt
    /// </summary>
    public enum Verschiebbar {
        Gold,
        Kampfeinnahmen,
        Heerführer,
        Pferde,
        LeichteKatapulte,
        SchwereKatapulte,
    }

    /// <summary>
    /// Die Regeln für das Verschieben von Rüstgütern und Geld (Regelwerk 1.8, 3.2.3 und 6.6).
    ///
    /// Zwischen zwei Heeren auf derselben Gemark lässt sich umladen. Für Heerführer gilt zusätzlich
    /// Errata 17: "Heerführer dürfen nach belieben auf derselben Gemark zwischen Heeren und Flotten
    /// (auch im Schiffsbauch) verschoben werden, wenn das abgebende Heer sich in diesem Monat noch
    /// nicht bewegt hat."
    ///
    /// Für Geld gilt: "Die Kampfeinnahmen, Einnahmen durch Plündern und Schenkungen können sofort
    /// delokalisiert werden und werden somit als 'sonstige Einnahmen' dem Reichsschatz zugeordnet.
    /// Soll Gold aus dem Reichsschatz wieder einer Einheit zugeordnet werden so kann dies nur über
    /// einen Rüstort oder Audvacar geschehen."
    ///
    /// Der Weg in den Reichsschatz ist damit überall möglich, der Weg heraus nur an einem eigenen
    /// Rüstort. Die Altanwendung verlangte für beide Richtungen einen Rüstort; das Regelwerk gibt
    /// das für die Einzahlung nicht her.
    /// </summary>
    public static class VerschiebeRules {

        /// <summary>
        /// Wieviel eine Truppe von der Sorte hat
        /// </summary>
        public static int GetBestand(TruppenSpielfigur? truppe, Verschiebbar was) {
            if (truppe == null)
                return 0;
            return was switch {
                Verschiebbar.Gold => truppe.GS,
                Verschiebbar.Kampfeinnahmen => truppe.Kampfeinnahmen,
                Verschiebbar.Heerführer => truppe.hf,
                Verschiebbar.Pferde => truppe.Pferde,
                Verschiebbar.LeichteKatapulte => truppe.LKP,
                Verschiebbar.SchwereKatapulte => truppe.SKP,
                _ => 0,
            };
        }

        /// <summary>
        /// Ändert den Bestand einer Truppe um den angegebenen Betrag
        /// </summary>
        public static void Ändere(TruppenSpielfigur truppe, Verschiebbar was, int betrag) {
            switch (was) {
                case Verschiebbar.Gold: truppe.GS += betrag; break;
                case Verschiebbar.Kampfeinnahmen: truppe.Kampfeinnahmen += betrag; break;
                case Verschiebbar.Heerführer: truppe.hf += betrag; break;
                case Verschiebbar.Pferde: truppe.Pferde += betrag; break;
                case Verschiebbar.LeichteKatapulte: truppe.LKP += betrag; break;
                case Verschiebbar.SchwereKatapulte: truppe.SKP += betrag; break;
            }
        }

        /// <summary>
        /// Die Bezeichnung für Meldungen
        /// </summary>
        public static string GetBezeichnung(Verschiebbar was) => was switch {
            Verschiebbar.Gold => "Gold",
            Verschiebbar.Kampfeinnahmen => "Kampfeinnahmen",
            Verschiebbar.Heerführer => "Heerführer",
            Verschiebbar.Pferde => "Pferde",
            Verschiebbar.LeichteKatapulte => "leichte Katapulte",
            Verschiebbar.SchwereKatapulte => "schwere Katapulte",
            _ => was.ToString(),
        };

        /// <summary>
        /// Hat sich die Truppe in diesem Monat bewegt?
        /// </summary>
        public static bool HatSichBewegt(TruppenSpielfigur truppe)
            => truppe.gf_nach > 0 && (truppe.gf_nach != truppe.gf_von || truppe.kf_nach != truppe.kf_von);

        /// <summary>
        /// Steht die Truppe auf einem eigenen Rüstort?
        /// </summary>
        public static KleinFeld? GetEigenenRüstort(TruppenSpielfigur? truppe) {
            if (truppe == null)
                return null;
            var feld = KleinfeldView.GetKleinfeld(HeeresRules.GetStandort(truppe));
            if (feld == null || feld.Baupunkte <= 0)
                return null;
            return feld.Nation == ProgramView.SelectedNation ? feld : null;
        }

        /// <summary>
        /// Prüft das Umladen zwischen zwei Heeren, ohne etwas zu verändern.
        /// </summary>
        public static Result PrüfeZwischenHeeren(TruppenSpielfigur? quelle, TruppenSpielfigur? ziel, Verschiebbar was, int menge) {
            if (quelle == null || ziel == null)
                return Result.Fail("Es wurden nicht beide Heere gefunden", "Zum Umladen braucht es ein abgebendes und ein aufnehmendes Heer.");
            if (ReferenceEquals(quelle, ziel) || quelle.Nummer == ziel.Nummer)
                return Result.Fail("Ein Heer lädt nicht bei sich selbst um", quelle.Bezeichner);

            // Der Ort steht vor der Menge: dass zwei Heere gar nicht beieinander stehen, ist der
            // grundsaetzlichere Einwand und haengt nicht davon ab, was sie dabei haben.
            var standortQuelle = HeeresRules.GetStandort(quelle);
            var standortZiel = HeeresRules.GetStandort(ziel);
            if (standortQuelle.Equals(standortZiel) == false)
                return Result.Fail("Die Heere stehen nicht auf derselben Gemark",
                    $"{quelle.Bezeichner} steht auf {standortQuelle.CreateBezeichner()}, {ziel.Bezeichner} auf "
                    + $"{standortZiel.CreateBezeichner()}. Umgeladen wird nur auf derselben Gemark (Regelwerk 1.8).");

            var vorprüfung = PrüfeMenge(quelle, was, menge);
            if (vorprüfung.HasErrors)
                return vorprüfung;

            if (was == Verschiebbar.Heerführer) {
                if (HatSichBewegt(quelle))
                    return Result.Fail($"{quelle.Bezeichner} hat sich in diesem Monat schon bewegt",
                        "Heerführer dürfen nur abgegeben werden, wenn das abgebende Heer sich in diesem Monat "
                        + "noch nicht bewegt hat (Errata 17 zu Regelwerk 1.8).");
                if (quelle.hf - menge < HeeresRules.MinHeerführer)
                    return Result.Fail($"{quelle.Bezeichner} bliebe ohne Heerführer zurück",
                        $"Von {quelle.hf} Heerführern sollen {menge} gehen. Jedes Heer braucht einen, der es "
                        + "befehligt (Regelwerk 1.8).");
            }

            return Result.Success($"{menge} {GetBezeichnung(was)} gehen von {quelle.Bezeichner} zu {ziel.Bezeichner}",
                $"Beide stehen auf {standortQuelle.CreateBezeichner()}.");
        }

        /// <summary>
        /// Prüft die Einzahlung in den Reichsschatz.
        ///
        /// Das geht überall: Kampfeinnahmen und Plündergut "können sofort delokalisiert werden"
        /// (Regelwerk 6.6).
        /// </summary>
        public static Result PrüfeZumReichsschatz(TruppenSpielfigur? truppe, Verschiebbar was, int menge) {
            if (truppe == null)
                return Result.Fail("Es ist keine Truppe ausgewählt", "Ohne Truppe gibt es nichts einzuzahlen.");
            if (was != Verschiebbar.Gold && was != Verschiebbar.Kampfeinnahmen)
                return Result.Fail($"{GetBezeichnung(was)} gehören nicht in den Reichsschatz",
                    "In den Reichsschatz wandern nur Gold und Kampfeinnahmen.");

            var vorprüfung = PrüfeMenge(truppe, was, menge);
            if (vorprüfung.HasErrors)
                return vorprüfung;

            return Result.Success($"{menge} {GetBezeichnung(was)} von {truppe.Bezeichner} gehen in den Reichsschatz",
                "Delokalisiertes Geld zählt danach als sonstige Einnahme.");
        }

        /// <summary>
        /// Prüft die Auszahlung aus dem Reichsschatz an eine Truppe.
        ///
        /// "Soll Gold aus dem Reichsschatz wieder einer Einheit zugeordnet werden so kann dies nur
        /// über einen Rüstort oder Audvacar geschehen." (Regelwerk 6.6)
        /// </summary>
        public static Result PrüfeVomReichsschatz(TruppenSpielfigur? truppe, int menge) {
            if (truppe == null)
                return Result.Fail("Es ist keine Truppe ausgewählt", "Ohne Truppe gibt es nichts auszuzahlen.");
            if (menge <= 0)
                return Result.Fail("Es wurde kein Betrag angegeben", $"Angegeben waren {menge}.");

            var rüstort = GetEigenenRüstort(truppe);
            if (rüstort == null)
                return Result.Fail($"{truppe.Bezeichner} steht auf keinem eigenen Rüstort",
                    "Gold aus dem Reichsschatz kann einer Einheit nur über einen Rüstort oder Audvacar "
                    + "zugeordnet werden (Regelwerk 6.6).");

            var schatz = SchatzkammerRules.GetMonat(ZugView.AktuellerZug.Zug);
            if (schatz == null)
                return Result.Fail("Die Schatzkammer hat keinen Stand für diesen Monat", string.Empty);
            if (menge > schatz.Reichschatz)
                return Result.Fail("Soviel liegt nicht im Reichsschatz",
                    $"Ausgezahlt werden sollen {menge} GS, vorhanden sind {schatz.Reichschatz}.");

            return Result.Success($"{menge} GS gehen an {truppe.Bezeichner}",
                $"Ausgezahlt über den Rüstort auf {rüstort.Bezeichner}.");
        }

        private static Result PrüfeMenge(TruppenSpielfigur truppe, Verschiebbar was, int menge) {
            if (menge <= 0)
                return Result.Fail("Es wurde nichts angegeben", $"Angegeben waren {menge} {GetBezeichnung(was)}.");
            int bestand = GetBestand(truppe, was);
            if (menge > bestand)
                return Result.Fail($"Soviel hat {truppe.Bezeichner} nicht",
                    $"Verschoben werden sollen {menge} {GetBezeichnung(was)}, vorhanden sind {bestand}.");
            return Result.Success();
        }
    }
}
