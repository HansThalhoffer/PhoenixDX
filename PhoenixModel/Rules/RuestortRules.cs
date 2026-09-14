using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Was an einem Rüstort gebaut wird
    /// </summary>
    public enum Bauart {
        /// <summary>Die nächste Ausbaustufe wird erreicht</summary>
        Ausbau,
        /// <summary>Verlorene Baupunkte werden wiederhergestellt</summary>
        Reparatur,
    }

    /// <summary>
    /// Die Regeln für Ausbau und Reparatur von Rüstorten (Regelwerk 1.5 und 1.5.12).
    ///
    /// "Die Summe der Baupunkte einer Baustelle eines Rüstortes darf pro Monat um maximal 250
    /// Baupunkte erhöht werden." und "Beschädigte Rüstorte können beginnend mit dem nächsten Monat
    /// durch Zahlung von 50 GS pro Baupunkt, um maximal 250 Baupunkte pro Monat, repariert werden."
    ///
    /// "Ein Baupunkt kostet 50 Goldstücke."
    ///
    /// Die Ausbaustufen stehen in der Tabelle ruestort_crossref und gehen in Schritten von 250
    /// Baupunkten: die Zwischenstufen Burg-I bis Burg-III sind der Baufortschritt einer Burg zur
    /// Stadt. Ein Monat Bauzeit entspricht damit genau einer Stufe.
    ///
    /// Wichtig für das Verständnis der Kartendaten: die Spalte Ruestort eines Kleinfeldes nennt die
    /// Stufe, die dort stehen sollte, die Spalte Baupunkte den tatsächlichen Zustand. Weichen sie
    /// voneinander ab, ist der Rüstort beschädigt.
    /// </summary>
    public static class RuestortRules {

        /// <summary>
        /// Um soviele Baupunkte darf eine Baustelle pro Monat wachsen (Regelwerk 1.5)
        /// </summary>
        public const int MaxBaupunkteProMonat = 250;

        /// <summary>
        /// "Ein Baupunkt kostet 50 Goldstücke" (Regelwerk 1.5)
        /// </summary>
        public const int KostenProBaupunkt = SchatzkammerRules.KostenProBaupunkt;

        /// <summary>
        /// Was ein Bauvorhaben kostet
        /// </summary>
        public static int BerechneKosten(int baupunkte) => Math.Max(0, baupunkte) * KostenProBaupunkt;

        /// <summary>
        /// Die fertige Stufe hinter einer Zwischenstufe: aus "Stadt-II" wird "Stadt".
        ///
        /// Die Referenztabelle führt zwischen zwei fertigen Rüstorten drei Zwischenstufen je 250
        /// Baupunkte - Burg-I bis Burg-III zwischen Burg und Stadt. Sie tragen die Werte der
        /// fertigen Burg (Regelwerk 1.5: "man kann bis zur erneuten Fertigstellung der Stadt nur
        /// die Rüstkapazität einer Burg nutzen"), heissen aber anders. Wer einen Rüstort an seinem
        /// Namen erkennt, fragt deshalb nach der Grundstufe.
        /// </summary>
        public static string? GetGrundstufe(string? stufenname) {
            if (string.IsNullOrWhiteSpace(stufenname))
                return null;
            int strich = stufenname.IndexOf('-');
            return strich < 0 ? stufenname : stufenname[..strich];
        }

        /// <summary>
        /// Dasselbe für einen Rüstort aus der Referenztabelle
        /// </summary>
        public static string? GetGrundstufe(Rüstort? rüstort) => GetGrundstufe(rüstort?.Ruestort);

        /// <summary>
        /// Die Ausbaustufe, die an diesem Kleinfeld stehen sollte
        /// </summary>
        public static Rüstort? GetSollstufe(KleinFeld? kleinfeld) {
            if (kleinfeld == null)
                return null;
            return BauwerkeView.GetRuestortReferenz(kleinfeld.Ruestort);
        }

        /// <summary>
        /// Wieviele Baupunkte dem Rüstort zu seiner Sollstufe fehlen
        /// </summary>
        public static int GetSchaden(KleinFeld? kleinfeld) {
            var soll = GetSollstufe(kleinfeld);
            if (soll == null || soll.Baupunkte == null || kleinfeld == null)
                return 0;
            return Math.Max(0, soll.Baupunkte.Value - kleinfeld.Baupunkte);
        }

        /// <summary>
        /// Die nächsthöhere Ausbaustufe über dem aktuellen Stand.
        ///
        /// Audvacar bleibt aussen vor: der Ort steht mit einem unerreichbaren Wert in der Tabelle
        /// und ist kein Ziel eines gewöhnlichen Ausbaus.
        /// </summary>
        public static Rüstort? GetNächsteStufe(KleinFeld? kleinfeld) {
            if (kleinfeld == null || SharedData.RüstortReferenz == null)
                return null;
            return SharedData.RüstortReferenz
                .Where(stufe => stufe.Baupunkte != null
                             && stufe.Baupunkte > kleinfeld.Baupunkte
                             && stufe.Baupunkte - kleinfeld.Baupunkte <= MaxBaupunkteProMonat)
                .OrderBy(stufe => stufe.Baupunkte)
                .FirstOrDefault();
        }

        /// <summary>
        /// Prüft ein Bauvorhaben an einem Rüstort, ohne etwas zu verändern.
        /// </summary>
        /// <param name="kleinfeld">das Kleinfeld mit dem Rüstort</param>
        /// <param name="art">Ausbau oder Reparatur</param>
        /// <param name="baupunkte">wieviele Baupunkte in diesem Monat gebaut werden</param>
        /// <summary>
        /// Wieviele Baupunkte in diesem Monat an dieser Baustelle dazukommen dürfen.
        ///
        /// Gewöhnlich <see cref="MaxBaupunkteProMonat"/>. Wird die Baustelle belagert, sinkt die
        /// Grenze je Gemark, von der aus belagert wird, um 15 Prozent: "die monatlich maximal
        /// mögliche Erhöhung der Baupunkte der Großbaustelle verringert sich pro Gemark von der
        /// aus belagert wird um 15%" (Regelwerk 1.7.2). Als Großbaustelle gilt der Neuaufbau und
        /// die Reparatur eines Rüstorts.
        /// </summary>
        public static int GetMaxBaupunkteProMonat(KleinFeld? kleinfeld)
            => GetMaxBaupunkteProMonat(kleinfeld, out _);

        /// <summary>
        /// Dasselbe, und dazu die Belagerung, die dahinter steckt
        /// </summary>
        public static int GetMaxBaupunkteProMonat(KleinFeld? kleinfeld, out BelagerungsRules.Belagerung belagerung) {
            belagerung = RuestRules.GetBelagerung(kleinfeld);
            return belagerung.Besteht
                ? BelagerungsRules.Mindere(MaxBaupunkteProMonat, belagerung)
                : MaxBaupunkteProMonat;
        }

        public static Result Prüfe(KleinFeld? kleinfeld, Bauart art, int baupunkte) {
            if (kleinfeld == null)
                return Result.Fail("Es ist kein Kleinfeld ausgewählt", "Ohne Feld lässt sich nicht bauen.");

            if (ConstructRules.IsNotAllowedOnWater(kleinfeld) is Result wasser && wasser.HasErrors)
                return wasser;
            if (ConstructRules.IsAllowedForOwner(kleinfeld) is Result besitzer && besitzer.HasErrors)
                return besitzer;
            if (ConstructRules.IsRüstPhase() is Result phase && phase.HasErrors)
                return phase;

            if (kleinfeld.Gebäude == null)
                return Result.Fail($"Auf {kleinfeld.Bezeichner} steht kein Rüstort",
                    "Ausgebaut und repariert wird nur, wo schon etwas steht.");

            if (baupunkte <= 0)
                return Result.Fail("Es wurden keine Baupunkte angegeben", $"Angegeben waren {baupunkte}.");
            int obergrenze = GetMaxBaupunkteProMonat(kleinfeld, out var belagerung);
            if (baupunkte > obergrenze) {
                string grund = belagerung.Besteht
                    ? $"Die Baustelle ist belagert: {belagerung.Beschreibung}. Statt {MaxBaupunkteProMonat} "
                      + $"Baupunkten sind in diesem Monat nur {obergrenze} möglich (Regelwerk 1.7.2)."
                    : $"Angegeben waren {baupunkte}. Die Summe der Baupunkte einer Baustelle darf pro Monat um "
                      + $"maximal {MaxBaupunkteProMonat} erhöht werden (Regelwerk 1.5).";
                return Result.Fail($"Mehr als {obergrenze} Baupunkte gehen nicht in einem Monat", grund);
            }

            int schaden = GetSchaden(kleinfeld);
            if (art == Bauart.Reparatur) {
                if (schaden == 0)
                    return Result.Fail($"Der Rüstort auf {kleinfeld.Bezeichner} ist unbeschädigt",
                        $"Er hat seine vollen {kleinfeld.Baupunkte} Baupunkte. Zum Vergrössern dient der Ausbau.");
                if (baupunkte > schaden)
                    return Result.Fail("Soviel ist gar nicht kaputt",
                        $"Dem Rüstort fehlen {schaden} Baupunkte, repariert werden sollen {baupunkte}.");
            }
            else {
                if (schaden > 0)
                    return Result.Fail($"Der Rüstort auf {kleinfeld.Bezeichner} ist beschädigt",
                        $"Ihm fehlen {schaden} Baupunkte zu seiner Stufe {GetSollstufe(kleinfeld)?.Bauwerk}. "
                        + "Er muss erst repariert werden, bevor er weiter wachsen kann.");

                var nächste = GetNächsteStufe(kleinfeld);
                if (nächste == null)
                    return Result.Fail($"Der Rüstort auf {kleinfeld.Bezeichner} lässt sich nicht weiter ausbauen",
                        $"Über {kleinfeld.Baupunkte} Baupunkten gibt es keine Stufe, die in einem Monat erreichbar wäre.");
                if (baupunkte > nächste.Baupunkte - kleinfeld.Baupunkte)
                    return Result.Fail("Soviele Baupunkte braucht die nächste Stufe nicht",
                        $"Bis {nächste.Bauwerk} fehlen {nächste.Baupunkte - kleinfeld.Baupunkte} Baupunkte, "
                        + $"gebaut werden sollen {baupunkte}.");
            }

            int kosten = BerechneKosten(baupunkte);
            if (ConstructRules.IsEnoughMoney(kleinfeld, kosten) is Result geld && geld.HasErrors)
                return geld;

            // "allerdings kann die Reparatur eines Bauwerks von einem nicht alliierten,
            // eroberungsfähigen Heer verhindert werden" (Regelwerk 1.5.10) - für den Ausbau
            // gilt dasselbe (1.5).
            if (ConstructRules.WirdGestört(kleinfeld) is Result störung && störung.HasErrors)
                return störung;

            string was = art == Bauart.Reparatur ? "Die Reparatur" : "Der Ausbau";
            return Result.Success($"{was} auf {kleinfeld.Bezeichner} ist möglich",
                $"{baupunkte} Baupunkte zu je {KostenProBaupunkt} GS kosten {kosten} GS.");
        }
    }
}
