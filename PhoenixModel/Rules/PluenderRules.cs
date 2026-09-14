using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Plünderung einer Gemark (Regelwerk 3.2.2.1).
    ///
    /// "Ein eroberungsfähiges Heer, welches sich in fremdem Gebiet befindet, hat neben dem Erobern
    /// die Möglichkeit, diese Gemark stattdessen zu plündern. Da die Plünderung einer Gemark 1
    /// Monat dauert, kann sich das plündernde Heer diesen Monat nicht bewegen. Für diese
    /// geplünderte Gemark erhält das Heer die doppelten Einnahmen dieser Gemark. Ebenso dürfen
    /// Rüstorte geplündert werden. Durch Plündern erzielte Einnahmen sind sonstige Einnahmen.
    /// Geplünderte Gemarken bringen 8 Monate keine Einnahmen mehr und es kann in geplünderten
    /// Rüstorten 8 Monate lang nicht gerüstet werden."
    ///
    /// Die Karte führt dazu die Spalte gepluendert. Was darin steht, sagt keine Quelle; im
    /// Datenbestand stehen kleine Zahlen (2) in einem Zug 170, ein Zugmonat kann es also nicht
    /// sein. Gelesen wird sie hier als **Zahl der Monate, die noch ohne Einnahmen bleiben** - das
    /// ist die einzige Lesart, die zu den Werten und zur Regel passt. Die Einnahmesicht wertet die
    /// Spalte schon lange so aus: was darin steht, bringt nichts ein.
    ///
    /// Heruntergezählt wird nicht beim Spieler, sondern bei der Spielleitung: die Karte ist für
    /// alle dieselbe, und wenn jeder Spieler seine Kopie herunterzählte, liefe sie auseinander.
    /// </summary>
    public static class PlünderRules {

        /// <summary>
        /// "Geplünderte Gemarken bringen 8 Monate keine Einnahmen mehr" (Regelwerk 3.2.2.1)
        /// </summary>
        public const int MonateOhneEinnahmen = 8;

        /// <summary>
        /// Der Faktor auf die Einnahmen: "erhält das Heer die doppelten Einnahmen dieser Gemark"
        /// </summary>
        public const int Beutefaktor = 2;

        /// <summary>
        /// Steht diese Gemark noch unter den Folgen einer Plünderung?
        /// </summary>
        public static bool IstGeplündert(KleinFeld? gemark) => gemark != null && gemark.gepluendert != 0;

        /// <summary>
        /// Wieviele Monate die Gemark noch nichts einbringt
        /// </summary>
        public static int GetRestmonate(KleinFeld? gemark) => Math.Max(0, gemark?.gepluendert ?? 0);

        /// <summary>
        /// Was eine Plünderung einbringt: die doppelten Einnahmen der Gemark.
        ///
        /// Gezählt wird, was die Gemark im Monat abwirft - Gelände und Bauwerk zusammen. Eine
        /// Gemark, die schon geplündert ist, bringt nichts mehr: sie wirft ja selbst nichts ab.
        ///
        /// Wohin das Geld gehört, lässt das Regelwerk offen. "erhält das Heer die doppelten
        /// Einnahmen", aber "Durch Plündern erzielte Einnahmen sind sonstige Einnahmen", und die
        /// "fließen dem Staatsschatz zu und werden wie gewöhnliche Einnahmen behandelt" (3.2.2).
        /// Deshalb wird hier nur gerechnet und nicht gebucht.
        /// </summary>
        public static int BerechneBeute(KleinFeld? gemark)
            => gemark == null ? 0 : Beutefaktor * EinnahmenView.GetGesamtEinnahmen(gemark);

        /// <summary>
        /// Prüft, ob dieses Heer diese Gemark plündern darf.
        ///
        /// Drei Bedingungen: das Heer ist eroberungsfähig (1.8), die Gemark gehört einem anderen
        /// Reich, und das Heer bewegt sich in diesem Monat nicht - "Da die Plünderung einer Gemark
        /// 1 Monat dauert, kann sich das plündernde Heer diesen Monat nicht bewegen."
        /// </summary>
        public static Result Prüfe(TruppenSpielfigur? heer, KleinFeld? gemark) {
            if (heer == null)
                return Result.Fail("Es ist kein Heer ausgewählt", "Ohne Heer wird nicht geplündert.");
            if (gemark == null)
                return Result.Fail("Es ist keine Gemark ausgewählt", "Ohne Gemark gibt es nichts zu plündern.");

            if (HeeresRules.IstEroberungsfähig(heer) == false)
                return Result.Fail($"{heer.Bezeichner} ist nicht eroberungsfähig",
                    $"Dafür braucht es ein Landheer mit {HeeresRules.RaumpunkteFürEroberung} Raumpunkten "
                    + "und einem Heerführer oder Adeligen (Regelwerk 1.8).");

            if (gemark.Nation == null || (heer.Nation != null && gemark.Nation.Equals(heer.Nation)))
                return Result.Fail($"{gemark.Bezeichner} ist kein fremdes Gebiet",
                    "Geplündert wird in fremdem Gebiet (Regelwerk 3.2.2.1).");

            if (heer.gf != gemark.gf || heer.kf != gemark.kf)
                return Result.Fail($"{heer.Bezeichner} steht nicht auf {gemark.Bezeichner}",
                    "Geplündert wird die Gemark, auf der das Heer steht.");

            if (HatSichBewegt(heer))
                return Result.Fail($"{heer.Bezeichner} hat sich in diesem Monat bewegt",
                    "Die Plünderung einer Gemark dauert einen Monat; das plündernde Heer kann sich "
                    + "in diesem Monat nicht bewegen (Regelwerk 3.2.2.1).");

            if (IstGeplündert(gemark))
                return Result.Fail($"{gemark.Bezeichner} ist bereits geplündert",
                    $"Die Gemark bringt noch {GetRestmonate(gemark)} Monate nichts ein.");

            return Result.Success($"{heer.Bezeichner} plündert {gemark.Bezeichner}",
                $"Das bringt {BerechneBeute(gemark)} GS als sonstige Einnahme. Die Gemark bringt danach "
                + $"{MonateOhneEinnahmen} Monate nichts mehr ein.");
        }

        /// <summary>
        /// Vermerkt die Plünderung auf der Gemark: ab jetzt acht Monate ohne Einnahmen.
        /// </summary>
        /// <returns>was die Plünderung eingebracht hat</returns>
        public static int Plündere(KleinFeld? gemark) {
            if (gemark == null)
                return 0;
            int beute = BerechneBeute(gemark);
            gemark.gepluendert = MonateOhneEinnahmen;
            return beute;
        }

        /// <summary>
        /// Zählt die Folgen einer Plünderung um einen Monat herunter.
        ///
        /// Gehört zum Zugwechsel der Spielleitung, nicht zu dem eines Spielers: die Karte ist für
        /// alle dieselbe.
        /// </summary>
        /// <returns>true, wenn sich etwas geändert hat</returns>
        public static bool ZähleHerunter(KleinFeld? gemark) {
            if (gemark == null || gemark.gepluendert <= 0)
                return false;
            gemark.gepluendert--;
            return true;
        }

        /// <summary>
        /// Zählt alle geplünderten Gemarken der Karte um einen Monat herunter
        /// </summary>
        /// <returns>wieviele Gemarken noch unter den Folgen stehen</returns>
        public static int ZähleAlleHerunter() {
            if (SharedData.Map == null)
                return 0;
            int übrig = 0;
            foreach (var gemark in SharedData.Map.Values) {
                ZähleHerunter(gemark);
                if (gemark.gepluendert > 0)
                    übrig++;
            }
            return übrig;
        }

        /// <summary>
        /// Hat sich das Heer in diesem Monat bewegt?
        ///
        /// Die Zielposition ist entweder nicht gesetzt oder dieselbe wie die Ausgangsposition -
        /// dann steht es noch, wo es stand.
        /// </summary>
        private static bool HatSichBewegt(TruppenSpielfigur heer)
            => heer.gf_nach != 0 && (heer.gf_nach != heer.gf_von || heer.kf_nach != heer.kf_von);
    }
}
