using PhoenixModel.Commands;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln des Rüstens (Regelwerk 3.3 und 3.3.1).
    ///
    /// "In den Rüstmonaten kann mit dem Geld des Reichsschatzes gerüstet werden. In jedem Monat
    /// kann mit Besonderen Einnahmen (Kampfeinnahmen) gerüstet werden."
    ///
    /// Bewegliche Rüstgüter dürfen nur in eigenen Rüstorten gerüstet werden, die zu Beginn des
    /// Monats schon standen. Der Umfang hängt an der Größe des Rüstorts; die Zahlen stehen in der
    /// Tabelle ruestort_crossref der crossref.mdb und stimmen mit dem Regelwerk überein:
    ///
    ///     Burg        10.000 GS,  2 HF, 0 Zauberer
    ///     Stadt       25.000 GS,  5 HF, 1 Zauberer
    ///     Festung     40.000 GS,  8 HF, 2 Zauberer
    ///     Hauptstadt  50.000 GS, 10 HF, 3 Zauberer
    ///
    /// Beim Verrüsten besonderer Einnahmen werden diese Grenzen halbiert und abgerundet (3.3.1).
    ///
    /// Nicht abgebildet ist die Belagerung: das Regelwerk nimmt belagerte Rüstorte von der Rüstung
    /// aus (3.3 mit Verweis auf 1.7), aber im Datenmodell gibt es keinen Belagerungszustand. Wer
    /// das ergänzt, muss hier nachziehen.
    /// </summary>
    public static class RuestRules {

        /// <summary>
        /// Was in einem Rüstort in einem Monat gerüstet werden darf.
        /// </summary>
        /// <param name="Goldstücke">die Summe, für die Rüstgüter gekauft werden dürfen</param>
        /// <param name="Heerführer">die Zahl der Heerführer; Charaktere zählen mit (Regelwerk 3.3)</param>
        /// <param name="Zauberer">die Zahl der Zauberer; Charakterzauberer zählen mit</param>
        public record class Rüstkapazität(int Goldstücke, int Heerführer, int Zauberer) {
            /// <summary>Die halbierte und abgerundete Kapazität für besondere Einnahmen (Regelwerk 3.3.1)</summary>
            public Rüstkapazität Halbiert() => new(Goldstücke / 2, Heerführer / 2, Zauberer / 2);

            /// <summary>Nichts davon - für ein Feld ohne Rüstort</summary>
            public static readonly Rüstkapazität Keine = new(0, 0, 0);
        }

        /// <summary>
        /// Die Rüstkapazität des Rüstorts auf dieser Gemark.
        ///
        /// Ein beschädigter Rüstort sinkt auf die nächstniedrigere Ausbaustufe und hat auch nur
        /// deren Rüstkapazität (Regelwerk 1.5); das erledigt bereits die Zuordnung über die
        /// Baupunkte.
        /// </summary>
        /// <param name="ausBesonderenEinnahmen">true, wenn aus Kampfeinnahmen gerüstet wird</param>
        /// <returns>die Kapazität, oder <see cref="Rüstkapazität.Keine"/> wenn dort nichts steht</returns>
        public static Rüstkapazität GetKapazität(KleinFeld? gemark, bool ausBesonderenEinnahmen = false) {
            Rüstort? rüstort = gemark == null ? null : BauwerkeView.GetRüstortNachKarte(gemark);
            if (rüstort == null)
                return Rüstkapazität.Keine;

            var kapazität = new Rüstkapazität(
                rüstort.KapazitätTruppen ?? 0,
                rüstort.KapazitätHF ?? 0,
                rüstort.KapazitätZ ?? 0);
            return ausBesonderenEinnahmen ? kapazität.Halbiert() : kapazität;
        }

        /// <summary>
        /// Was in diesem Zug an dieser Gemark bereits gerüstet wurde.
        ///
        /// Die Rüstungstabelle enthält immer nur den laufenden Zug, weil sie bei der Zugabgabe
        /// geleert wird. Besondere Rüstungen sind Zuteilungen der Spielleitung; sie kosten den
        /// Reichsschatz nichts und zählen deshalb auch nicht gegen die Kapazität des Rüstorts.
        /// </summary>
        public static Rüstkapazität BerechneBereitsGerüstet(KleinfeldPosition? ort) {
            if (ort == null || SharedData.Ruestung == null)
                return Rüstkapazität.Keine;

            int gold = 0, heerführer = 0, zauberer = 0;
            foreach (var rüstung in SharedData.Ruestung) {
                if (rüstung.besRuestung != 0)
                    continue;
                if (rüstung.gf != ort.gf || rüstung.kf != ort.kf)
                    continue;
                gold += SchatzkammerRules.BerechneEinheitenkosten(rüstung);
                heerführer += rüstung.HF;
                zauberer += rüstung.Z + rüstung.ZB;
            }
            return new Rüstkapazität(gold, heerführer, zauberer);
        }

        /// <summary>
        /// Was ein Rüstauftrag kostet und wie viele Heerführer und Zauberer er bindet.
        /// </summary>
        public static Rüstkapazität BerechneUmfang(Ruestung? auftrag) {
            if (auftrag == null)
                return Rüstkapazität.Keine;
            return new Rüstkapazität(
                SchatzkammerRules.BerechneEinheitenkosten(auftrag),
                auftrag.HF,
                auftrag.Z + auftrag.ZB);
        }

        /// <summary>
        /// Prüft, ob auf dieser Gemark überhaupt gerüstet werden darf.
        ///
        /// Bewegliche Rüstgüter dürfen nur in eigenen Rüstorten gerüstet werden (Regelwerk 3.3).
        /// Ein Dorf ist keiner: seine Rüstkapazität ist null.
        /// </summary>
        public static Result PrüfeRüstort(KleinFeld? gemark) {
            if (gemark == null)
                return Result.Fail("Es wurde kein Kleinfeld angegeben",
                    "Gerüstet wird in einem Rüstort; dafür muss feststehen, in welchem.");

            if (gemark.Nation == null || gemark.Nation != ProgramView.SelectedNation)
                return Result.Fail($"{gemark.Bezeichner} gehört nicht zum eigenen Reich",
                    "Bewegliche Rüstgüter dürfen nur in eigenen Rüstorten gerüstet werden (Regelwerk 3.3).");

            var kapazität = GetKapazität(gemark);
            if (kapazität.Goldstücke <= 0)
                return Result.Fail($"Auf {gemark.Bezeichner} steht kein Rüstort",
                    "Gerüstet wird in einer Burg, einer Stadt, einer Festung oder einer Hauptstadt. "
                    + "Ein Dorf hat keine Rüstkapazität.");

            return Result.Success();
        }

        /// <summary>
        /// Prüft einen Rüstauftrag vollständig: Ort, Zugphase, Kapazität und Mittel.
        /// </summary>
        /// <param name="gemark">der Rüstort</param>
        /// <param name="auftrag">was gerüstet werden soll</param>
        /// <param name="ausBesonderenEinnahmen">
        /// true, wenn aus Kampfeinnahmen gerüstet wird. Damit darf jeden Monat gerüstet werden,
        /// die Kapazität halbiert sich aber (Regelwerk 3.3.1). Die Rüstungstabelle vermerkt die
        /// Herkunft des Geldes nicht, deshalb muss der Aufrufer sie mitgeben.
        /// </param>
        public static Result Prüfe(KleinFeld? gemark, Ruestung? auftrag, bool ausBesonderenEinnahmen = false) {
            if (PrüfeRüstort(gemark) is Result ortprüfung && ortprüfung.HasErrors)
                return ortprüfung;
            if (auftrag == null)
                return Result.Fail("Es wurde nichts zum Rüsten angegeben",
                    "Ein Rüstauftrag ohne Rüstgüter ändert nichts.");

            // Mit dem Reichsschatz wird nur im Rüstmonat gerüstet; besondere Einnahmen gehen immer
            if (ausBesonderenEinnahmen == false && ZugView.KannRüsten == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht gerüstet",
                    "Mit dem Geld des Reichsschatzes kann nur in der Rüstphase gerüstet werden. "
                    + "Mit besonderen Einnahmen aus dem Kampf geht es in jedem Monat (Regelwerk 3.3.1).");

            var kapazität = GetKapazität(gemark, ausBesonderenEinnahmen);
            var bereits = BerechneBereitsGerüstet(gemark);
            var umfang = BerechneUmfang(auftrag);

            if (bereits.Goldstücke + umfang.Goldstücke > kapazität.Goldstücke)
                return Result.Fail($"Die Rüstkapazität von {gemark!.Bezeichner} ist erschöpft",
                    $"Dort dürfen {kapazität.Goldstücke:N0} GS verrüstet werden"
                    + (ausBesonderenEinnahmen ? " (halbiert, weil aus besonderen Einnahmen)" : string.Empty)
                    + $"; vergeben sind {bereits.Goldstücke:N0}, dieser Auftrag kostet "
                    + $"{umfang.Goldstücke:N0}.");

            if (bereits.Heerführer + umfang.Heerführer > kapazität.Heerführer)
                return Result.Fail($"Zu viele Heerführer für {gemark!.Bezeichner}",
                    $"Dort dürfen {kapazität.Heerführer} Heerführer gerüstet werden, vergeben sind "
                    + $"{bereits.Heerführer}, dieser Auftrag verlangt {umfang.Heerführer}. "
                    + "Charaktere zählen dabei mit (Regelwerk 3.3).");

            if (bereits.Zauberer + umfang.Zauberer > kapazität.Zauberer)
                return Result.Fail($"Zu viele Zauberer für {gemark!.Bezeichner}",
                    $"Dort dürfen {kapazität.Zauberer} Zauberer gerüstet werden, vergeben sind "
                    + $"{bereits.Zauberer}, dieser Auftrag verlangt {umfang.Zauberer}.");

            // "Gerüstet werden darf nur mit den tatsächlich vorhandenen Mitteln." (Regelwerk 3.3)
            if (ausBesonderenEinnahmen) {
                // Besondere Einnahmen liegen nicht im Reichsschatz, sondern bei den Truppen im
                // Rüstort - und zwar bei denen, die schon zu Monatsbeginn dort standen.
                int verfügbar = VerschiebeRules.BerechneBesondereEinnahmen(gemark);
                if (umfang.Goldstücke > verfügbar)
                    return Result.Fail($"In {gemark!.Bezeichner} liegen nicht genug besondere Einnahmen",
                        $"Der Auftrag kostet {umfang.Goldstücke:N0} GS, dort stehen aber nur "
                        + $"{verfügbar:N0} GS an Kampfeinnahmen bereit. Sie müssen zu Beginn des Monats "
                        + "schon im Rüstort gewesen sein (Regelwerk 6.6).");
            }
            else if (SchatzkammerView.HasEnoughMoney(gemark, umfang.Goldstücke) == false)
                return Result.Fail("Im Reichsschatz ist nicht genug Geld",
                    $"Der Auftrag kostet {umfang.Goldstücke:N0} GS, verfügbar sind noch "
                    + $"{SchatzkammerView.MoneyToSpendThisTurn():N0}.");

            return Result.Success();
        }

        /// <summary>
        /// Prüft, ob in diese Wassergemark ein Schiff gerüstet werden darf.
        ///
        /// "Schiffe werden immer in eine Wassergemark neben einem eigenen Rüstort in der
        /// Höhenstufe 1 gerüstet." (Regelwerk 3.3)
        /// </summary>
        public static Result PrüfeSchiffsplatz(KleinFeld? wasser) {
            if (wasser == null)
                return Result.Fail("Es wurde kein Kleinfeld angegeben",
                    "Ein Schiff braucht eine Wassergemark, in die es gerüstet wird.");
            if (wasser.IsWasser == false)
                return Result.Fail($"{wasser.Bezeichner} ist kein Wasser",
                    "Schiffe werden in eine Wassergemark gerüstet (Regelwerk 3.3).");

            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(wasser, richtung));
                if (nachbar == null)
                    continue;
                if (PrüfeRüstort(nachbar).HasErrors)
                    continue;
                if (BewegungsRules.GetHöhenstufe(nachbar) != 1)
                    continue;
                return Result.Success();
            }

            return Result.Fail($"Neben {wasser.Bezeichner} liegt kein passender Rüstort",
                "Schiffe werden in eine Wassergemark neben einem eigenen Rüstort in der Höhenstufe 1 "
                + "gerüstet (Regelwerk 3.3).");
        }
    }
}
