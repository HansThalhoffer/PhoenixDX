using PhoenixModel.dbErkenfara;
using PhoenixModel.Extensions;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Das Rückzugsgefecht (Regelwerk 5.4).
    ///
    /// "Sollten Charaktere eines unterlegenen Heeres den Charakterkampf und den Nahkampf der
    /// Truppen überlebt haben, so können sie noch ein Rückzugsgefecht versuchen."
    ///
    /// Es ist die letzte Gelegenheit des Verlierers: der Nahkampf reibt ihn auf (siehe
    /// <see cref="NahkampfRules"/>), seine Charaktere aber können sich noch heraushalten. Der
    /// Ablauf:
    ///
    /// 1. Wer darf es überhaupt versuchen? Nur wer vor dem Nahkampf einen Charakterkampf oder ein
    ///    Zauberduell bestritten hat. Ritter, die einen Ritterkampf geführt haben, nicht mehr.
    /// 2. Die Charaktere der Sieger dürfen die Teilnahme ablehnen. "Lehnen alle Charaktere der
    ///    siegreichen Truppen die Teilnahme ab, so ist der Rückzug erfolgreich."
    /// 3. Sonst gibt es eine neue Kampfrunde des Charakterkampfes mit den verbliebenen Gutpunkten
    ///    (<see cref="CharakterkampfRules"/>). Jetzt haben die Charaktere der Sieger die
    ///    Initiative - "Bei dieser Regel ist es unerheblich, wer bisher Angreifer und Verteidiger
    ///    war."
    /// 4. Wer überlebt, zieht sich in eine angrenzende Gemark seiner Wahl zurück, die nicht von
    ///    gegnerischen Truppen besetzt ist.
    ///
    /// Wer dabei stirbt, ist nicht tot: "Charaktere, die im Rückzugsgefecht sterben, sind gefangen
    /// genommen worden."
    ///
    /// Die Notteleportation der Charakterzauberer läuft analog; sie gehört zur Zauberei und steht
    /// nicht hier.
    /// </summary>
    public static class RückzugsRules {

        /// <summary>
        /// Wie lange ein zurückgezogener Charakter nicht angreifen darf: "Zurückgezogene
        /// Charaktere dürfen im laufenden und im kommenden Monat keine eigenen Angriffe mehr
        /// starten." (Regelwerk 5.4)
        ///
        /// Dasselbe gilt nach einem freien Rückzug für Ritter: "Nach einem freien Rückzug können
        /// Ritter im kommenden Zug ebenfalls nicht mehr angreifen."
        /// </summary>
        public const int Angriffssperre = 1;

        /// <summary>
        /// Darf dieser Charakter im laufenden Zug noch angreifen?
        /// </summary>
        /// <param name="zugDesRückzugs">der Zug, in dem er sich zurückgezogen hat</param>
        /// <param name="aktuellerZug">der laufende Zug</param>
        public static bool DarfAngreifen(int zugDesRückzugs, int aktuellerZug)
            => aktuellerZug > zugDesRückzugs + Angriffssperre;

        /// <summary>
        /// Was ein Charakter vor dem Nahkampf getan hat - davon hängt ab, ob er sich zurückziehen
        /// darf.
        /// </summary>
        /// <param name="HatCharakterkampfGeführt">
        /// hat er vor dem Nahkampf einen Charakterkampf oder ein Zauberduell bestritten?
        /// </param>
        /// <param name="HatRitterkampfGeführt">hat er einen Ritterkampf geführt?</param>
        /// <param name="HatNahkampfÜberlebt">steht er nach dem Nahkampf noch?</param>
        public record class Vorgeschichte(bool HatCharakterkampfGeführt, bool HatRitterkampfGeführt,
                bool HatNahkampfÜberlebt);

        /// <summary>
        /// Prüft, ob ein Charakter ein Rückzugsgefecht führen darf.
        ///
        /// "die Einleitung eines Rückzugefechts, bzw. die Notteleportation bei Charakterzauberern,
        /// kann nur erfolgen wenn vor dem Nahkampf der entsprechende Charakter einen
        /// Charakterkampf oder ein Zauberduell bestritten hat" - und: "Ritter, die einen
        /// Ritterkampf durchgeführt haben, können am Rückzugsgefecht nicht mehr teilnehmen."
        /// </summary>
        public static Result Prüfe(NamensSpielfigur? charakter, Vorgeschichte vorgeschichte) {
            if (charakter == null)
                return Result.Fail("Es ist kein Charakter ausgewählt",
                    "Ohne Charakter gibt es kein Rückzugsgefecht.");

            if (vorgeschichte.HatNahkampfÜberlebt == false)
                return Result.Fail($"{charakter.Bezeichner} hat den Nahkampf nicht überlebt",
                    "Das Rückzugsgefecht führen nur Charaktere, die den Nahkampf der Truppen überstanden haben.");

            if (vorgeschichte.HatRitterkampfGeführt)
                return Result.Fail($"{charakter.Bezeichner} hat einen Ritterkampf geführt",
                    "Ritter, die einen Ritterkampf durchgeführt haben, können am Rückzugsgefecht nicht mehr teilnehmen (Regelwerk 5.4).");

            if (vorgeschichte.HatCharakterkampfGeführt == false)
                return Result.Fail($"{charakter.Bezeichner} hat vor dem Nahkampf nicht gekämpft",
                    "Ein Rückzugsgefecht kann nur einleiten, wer vor dem Nahkampf einen Charakterkampf "
                    + "oder ein Zauberduell bestritten hat (Regelwerk 5.4).");

            return Result.Success($"{charakter.Bezeichner} kann ein Rückzugsgefecht versuchen",
                "Die Charaktere der siegreichen Truppen entscheiden, ob sie sich darauf einlassen.");
        }

        /// <summary>
        /// "Lehnen alle Charaktere der siegreichen Truppen die Teilnahme ab, so ist der Rückzug
        /// erfolgreich." (Regelwerk 5.4)
        ///
        /// Ohne einen einzigen Charakter auf der Siegerseite gibt es niemanden, der das Gefecht
        /// führen könnte - dann zieht der Verlierer ebenfalls unbehelligt ab.
        /// </summary>
        /// <param name="teilnehmendeSieger">die Charaktere der Sieger, die sich einlassen</param>
        public static bool RückzugOhneGefecht(IEnumerable<NamensSpielfigur>? teilnehmendeSieger)
            => teilnehmendeSieger == null || teilnehmendeSieger.Any() == false;

        /// <summary>
        /// Die Gemarken, in die sich ein Charakter zurückziehen darf.
        ///
        /// "so dürfen sie sich in ein nicht von gegnerischen Truppen besetztes, angrenzende Gemark
        /// ihrer Wahl zurückziehen." (Regelwerk 5.4)
        ///
        /// Zwei Bedingungen also: angrenzend, und frei von gegnerischen Truppen. Gegnerisch heisst
        /// hier, was <see cref="DiplomatieRules"/> dazu sagt - auf der Gemark, um die es geht.
        /// Von einem Wegerecht ist nicht die Rede; ein Rückzug ist kein gewöhnlicher Zug, deshalb
        /// wird nur geprüft, ob das Gelände die Figur überhaupt trägt.
        ///
        /// Fremde Charaktere allein halten eine Gemark nicht besetzt: das Regelwerk spricht von
        /// Truppen.
        /// </summary>
        /// <param name="charakter">der Charakter, der sich zurückzieht</param>
        /// <param name="fremdeFiguren">
        /// die Figuren der anderen Reiche, gewöhnlich aus <see cref="Spielleitungsdaten"/>
        /// </param>
        public static List<KleinFeld> FindeRückzugsfelder(NamensSpielfigur? charakter,
                IEnumerable<Spielfigur>? fremdeFiguren = null) {
            List<KleinFeld> ergebnis = [];
            if (charakter == null || Plausibilität.IsValid(charakter) == false)
                return ergebnis;

            var nachbarn = KleinfeldView.GetNachbarn(charakter, 1, includeSelf: false);
            if (nachbarn == null)
                return ergebnis;

            var besatzung = (fremdeFiguren ?? Spielleitungsdaten.GetAlleFiguren()).ToList();
            foreach (var nachbar in nachbarn) {
                if (IstPassierbar(charakter, nachbar) == false)
                    continue;
                if (IstVonGegnernBesetzt(charakter.Nation, nachbar, besatzung))
                    continue;
                ergebnis.Add(nachbar);
            }
            return ergebnis;
        }

        /// <summary>
        /// Trägt das Gelände diese Figur überhaupt? Ein Charakter zieht sich nicht ins Meer zurück.
        ///
        /// Die Bewegungsdaten allein reichen als Prüfung nicht: sie geben für Charaktere auch auf
        /// Wasser Kosten an (Wasser 5, Tiefsee 8), weil Charaktere mit einer Flotte reisen. Wessen
        /// Heer gerade aufgerieben wurde, hat aber keine Flotte unter sich - ohne Schiff bleibt
        /// das Wasser zu.
        /// </summary>
        private static bool IstPassierbar(Spielfigur figur, KleinFeld ziel) {
            if (ziel.IsWasser && figur.IsOnShip() == false)
                return false;
            var verbrauch = BewegungsRules.GetVerbrauch(figur, ziel.Gelaendetyp ?? 0, wegerecht: true, straße: false);
            return verbrauch != null && verbrauch.BP < BewegungsRules.BPUnpassierbar;
        }

        /// <summary>
        /// Steht auf dieser Gemark ein Heer eines verfeindeten Reiches?
        /// </summary>
        private static bool IstVonGegnernBesetzt(Nation? eigenes, KleinFeld gemark, IReadOnlyList<Spielfigur> figuren) {
            foreach (var figur in figuren) {
                if (figur is not TruppenSpielfigur)
                    continue;
                if (figur.gf != gemark.gf || figur.kf != gemark.kf)
                    continue;
                if (DiplomatieRules.SindVerfeindet(eigenes, figur.Nation, gemark))
                    return true;
            }
            return false;
        }
    }
}
