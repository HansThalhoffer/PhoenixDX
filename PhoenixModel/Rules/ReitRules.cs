using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für das Auf- und Absitzen (Regelwerk 1.1.2 und 1.1.3).
    ///
    /// "Ein Reittier macht aus einem Krieger einen Reiter, wenn der Krieger auf diesem Tier
    /// aufsitzt." und "Ein Reiter, der absitzt, wird zu einem Krieger mit einem Reittier am Zügel
    /// und transportiert in diesem Moment das Reittier."
    ///
    /// "Eingeschiffte Truppen können nicht auf und / oder absitzen."
    ///
    /// Ein Reittier trägt genau einen Krieger, Auf- und Absitzen gehen also immer eins zu eins.
    /// </summary>
    public static class ReitRules {

        /// <summary>
        /// Ein Reittier nimmt genau einen Krieger auf
        /// </summary>
        public const int KriegerProReittier = 1;

        /// <summary>
        /// Prüft, ob Krieger aufsitzen können, ohne etwas zu verändern.
        /// </summary>
        /// <param name="heer">das Kriegerheer, dessen Krieger aufsitzen</param>
        /// <param name="anzahl">wieviele Krieger aufsitzen - ebenso viele Reittiere werden gebraucht</param>
        /// <param name="heerführer">wieviele Heerführer mit aufsitzen</param>
        public static Result PrüfeAufsitzen(TruppenSpielfigur? heer, int anzahl, int heerführer) {
            if (heer == null)
                return Result.Fail("Es ist kein Heer ausgewählt", "Ohne Heer sitzt niemand auf.");
            if (heer.BaseTyp != FigurType.Krieger)
                return Result.Fail($"{heer.Bezeichner} ist kein Kriegerheer",
                    "Aufsitzen können nur Krieger, die Reittiere am Zügel führen (Regelwerk 1.1.2).");

            var vorprüfung = PrüfeGemeinsam(heer, anzahl, heerführer);
            if (vorprüfung.HasErrors)
                return vorprüfung;

            if (anzahl > heer.staerke)
                return Result.Fail($"Soviele Krieger hat {heer.Bezeichner} nicht",
                    $"Aufsitzen sollen {anzahl}, vorhanden sind {heer.staerke}.");
            if (anzahl > heer.Pferde)
                return Result.Fail($"Soviele Reittiere hat {heer.Bezeichner} nicht",
                    $"Für {anzahl} Krieger braucht es ebenso viele Reittiere, vorhanden sind {heer.Pferde}.");

            var rest = PrüfeVerbleibendesHeer(heer, heer.staerke - anzahl, heer.Pferde - anzahl, heer.hf - heerführer);
            if (rest.HasErrors)
                return rest;

            if (HeeresRules.FindeFreieNummer(FigurType.Reiter) == null)
                return Result.Fail("Es ist keine Reiternummer mehr frei",
                    $"Im Nummernkreis ab {HeeresRules.GetStartNummer(FigurType.Reiter)} sind alle Nummern vergeben.");

            return Result.Success($"{anzahl} Krieger von {heer.Bezeichner} können aufsitzen",
                $"Daraus wird ein Reiterheer mit {anzahl} Reitern und {heerführer} Heerführern.");
        }

        /// <summary>
        /// Prüft, ob Reiter absitzen können, ohne etwas zu verändern.
        /// </summary>
        /// <param name="heer">das Reiterheer, dessen Reiter absitzen</param>
        /// <param name="anzahl">wieviele Reiter absitzen</param>
        /// <param name="heerführer">wieviele Heerführer mit absitzen</param>
        public static Result PrüfeAbsitzen(TruppenSpielfigur? heer, int anzahl, int heerführer) {
            if (heer == null)
                return Result.Fail("Es ist kein Heer ausgewählt", "Ohne Heer sitzt niemand ab.");
            if (heer.BaseTyp != FigurType.Reiter)
                return Result.Fail($"{heer.Bezeichner} ist kein Reiterheer",
                    "Absitzen können nur Reiter (Regelwerk 1.1.3).");

            var vorprüfung = PrüfeGemeinsam(heer, anzahl, heerführer);
            if (vorprüfung.HasErrors)
                return vorprüfung;

            if (anzahl > heer.staerke)
                return Result.Fail($"Soviele Reiter hat {heer.Bezeichner} nicht",
                    $"Absitzen sollen {anzahl}, vorhanden sind {heer.staerke}.");

            var rest = PrüfeVerbleibendesHeer(heer, heer.staerke - anzahl, heer.Pferde, heer.hf - heerführer);
            if (rest.HasErrors)
                return rest;

            if (HeeresRules.FindeFreieNummer(FigurType.Krieger) == null)
                return Result.Fail("Es ist keine Kriegernummer mehr frei",
                    $"Im Nummernkreis ab {HeeresRules.GetStartNummer(FigurType.Krieger)} sind alle Nummern vergeben.");

            return Result.Success($"{anzahl} Reiter von {heer.Bezeichner} können absitzen",
                $"Daraus wird ein Kriegerheer mit {anzahl} Kriegern, die ebenso viele Reittiere am Zügel führen.");
        }

        /// <summary>
        /// Was für beide Richtungen gilt
        /// </summary>
        private static Result PrüfeGemeinsam(TruppenSpielfigur heer, int anzahl, int heerführer) {
            if (ZugView.KannBewegen == false)
                return Result.Fail($"In der {ZugView.PhasenBeschreibung} wird nicht auf- und abgesessen",
                    "Auf- und Absitzen gehört zum Spielzug; erst wenn die Rüstphase abgeschlossen ist, geht das.");

            if (SchifffahrtsRules.IstEingeschifft(heer))
                return Result.Fail($"{heer.Bezeichner} ist eingeschifft",
                    "Eingeschiffte Truppen können nicht auf und / oder absitzen (Regelwerk 1.1.3). "
                    + "Die Truppe muss erst an Land.");

            if (anzahl <= 0)
                return Result.Fail("Es wurde niemand angegeben", $"Die Anzahl war {anzahl}.");
            if (heerführer < HeeresRules.MinHeerführer)
                return Result.Fail("Das neue Heer braucht einen Heerführer",
                    "Jedes Heer muss einen Heerführer oder Adeligen haben, der es befehligt (Regelwerk 1.8).");
            if (heerführer > heer.hf)
                return Result.Fail($"Soviele Heerführer hat {heer.Bezeichner} nicht",
                    $"Mitkommen sollen {heerführer}, vorhanden sind {heer.hf}.");

            return Result.Success();
        }

        /// <summary>
        /// Das zurückbleibende Heer muss entweder ein gültiges Heer sein oder ganz verschwinden.
        ///
        /// Sitzt das ganze Heer auf, bleibt eine leere Hülle zurück. Die ist zulässig: sie hat
        /// keinen Heerführer mehr und wird beim Zugübergang aufgelöst, genau wie das aufgenommene
        /// Heer nach einer Fusion.
        /// </summary>
        private static Result PrüfeVerbleibendesHeer(TruppenSpielfigur heer, int stärke, int pferde, int heerführer) {
            bool bleibtEtwas = stärke > 0 || pferde > 0 || heer.LKP > 0 || heer.SKP > 0;
            if (bleibtEtwas == false)
                return Result.Success();

            if (heerführer < HeeresRules.MinHeerführer)
                return Result.Fail($"{heer.Bezeichner} bliebe ohne Heerführer zurück",
                    $"Zurück blieben {stärke} Stärke und {pferde} Reittiere, aber kein Heerführer, der sie befehligt. "
                    + "Entweder kommt ein Heerführer weniger mit, oder das ganze Heer sitzt auf.");

            return Result.Success();
        }
    }
}
