using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Art eines Teleportfeldes, entspricht der Spalte Art der Tabelle Teleportpunkte
    /// in der crossref.mdb.
    /// </summary>
    public enum TeleportArt {
        /// <summary>Kein Teleportfeld</summary>
        Keins,
        /// <summary>Das Teleportfeld des Piratennestes (Art N)</summary>
        Piratennest,
        /// <summary>Eines der sechs Teleportfelder am Rand von Erkenfara (Art A)</summary>
        Erkenfara,
        /// <summary>Einer der beiden Tiefseepunkte, die Einbahnstraßen sind (Art T)</summary>
        Tiefsee,
    }

    /// <summary>
    /// Ein Teleportfeld der Karte
    /// </summary>
    public class Teleportfeld {
        public required KleinfeldPosition Position { get; init; }
        public required TeleportArt Art { get; init; }
        public override string ToString() => $"{Position.CreateBezeichner()} ({Art})";
    }

    /// <summary>
    /// Die Regeln rund um die Teleportfelder, Regelwerk Kapitel 6.6.3 bis 6.6.5.
    ///
    /// - Das Teleportfeld selbst ist nur zum Verlassen der Welt da. Aufgetaucht wird nie darauf,
    ///   sondern auf einem der sechs Kleinfelder ringsherum.
    /// - Von den acht Teleportpunkten ist pro Monat genau einer aktiv; nur über ihn kommt man
    ///   zurück. Er wird zusammen mit der Zugreihenfolge vier Züge im Voraus von der Spielleitung
    ///   ausgewürfelt (1w8: 1-6 die sechs Punkte am Kartenrand, 7-8 die beiden Tiefseepunkte) und
    ///   steht in der Spalte Auftauchpunkt_A der Tabelle Zugreihenfolge. Die Anwendung würfelt
    ///   also nicht selbst.
    /// - Auch das konkrete Auftauchfeld unter den sechs ist von der Spielleitung vorgegeben und den
    ///   Spielern bekannt. In den Daten steht es bisher nicht, deshalb wird es im Dialog ausgewählt.
    /// </summary>
    public static class TeleportRules {

        /// <summary>
        /// Die Teleportfelder werden aus der Karte gelesen: Geländetyp 10 ist ein Auftauchpunkt,
        /// Geländetyp 11 ein Tiefsee-Einbahnpunkt. Die Tabelle Teleportpunkte der crossref.mdb
        /// liefert nur die Einordnung in Nest, Erkenfara und Tiefsee.
        ///
        /// Die Karte ist dabei die verlässlichere Quelle: in der Tabelle steht als zweiter
        /// Tiefseepunkt 806/46, in der Karte und im Regelwerk ist es 806/47.
        /// </summary>
        private static List<Teleportfeld>? _teleportfelder = null;
        private static readonly object _lock = new();

        /// <summary>
        /// Muss aufgerufen werden, wenn Karte oder Crossreferenzen neu geladen wurden
        /// </summary>
        public static void ResetCache() {
            lock (_lock) {
                _teleportfelder = null;
            }
        }

        /// <summary>
        /// Alle Teleportfelder der Karte
        /// </summary>
        public static IReadOnlyList<Teleportfeld> GetTeleportfelder() {
            lock (_lock) {
                if (_teleportfelder != null)
                    return _teleportfelder;
                if (SharedData.Map == null || SharedData.Map.IsAddingCompleted == false)
                    return [];

                // die Einordnung aus der Crossreferenztabelle vorbereiten
                Dictionary<string, TeleportArt> artNachFeld = [];
                if (SharedData.Teleportpunkte != null) {
                    foreach (var eintrag in SharedData.Teleportpunkte) {
                        var position = SimpleParser.ParseLocation(eintrag.Feld ?? string.Empty);
                        if (position == null)
                            continue;
                        artNachFeld[position.CreateBezeichner()] = eintrag.Art?.ToUpperInvariant() switch {
                            "N" => TeleportArt.Piratennest,
                            "A" => TeleportArt.Erkenfara,
                            "T" => TeleportArt.Tiefsee,
                            _ => TeleportArt.Keins,
                        };
                    }
                }

                List<Teleportfeld> felder = [];
                foreach (var kleinfeld in SharedData.Map.Values) {
                    if (IstTeleportfeld(kleinfeld) == false)
                        continue;
                    string bezeichner = kleinfeld.CreateBezeichner();
                    if (artNachFeld.TryGetValue(bezeichner, out var art) == false || art == TeleportArt.Keins) {
                        // Die Tabelle kennt dieses Feld nicht - aus dem Gelände ableiten
                        art = kleinfeld.TerrainType == TerrainType.Tiefseeeinbahnpunkt ? TeleportArt.Tiefsee : TeleportArt.Erkenfara;
                        ProgramView.LogWarning($"Das Teleportfeld {bezeichner} fehlt in der Tabelle Teleportpunkte",
                            $"Die Karte weist {bezeichner} als Teleportfeld aus, die Tabelle Teleportpunkte der crossref.mdb kennt es aber nicht. Es wird als {art} behandelt.");
                    }
                    felder.Add(new Teleportfeld { Position = new KleinfeldPosition(kleinfeld.gf, kleinfeld.kf), Art = art });
                }

                // Einträge der Tabelle, zu denen es kein Teleportfeld auf der Karte gibt, sind veraltet
                foreach (var eintrag in artNachFeld) {
                    if (felder.Any(f => f.Position.CreateBezeichner() == eintrag.Key) == false)
                        ProgramView.LogWarning($"Der Teleportpunkt {eintrag.Key} existiert so nicht auf der Karte",
                            $"In der Tabelle Teleportpunkte der crossref.mdb steht {eintrag.Key}, die Karte führt dort aber kein Teleportfeld. Der Eintrag wird ignoriert.");
                }

                _teleportfelder = felder;
                return _teleportfelder;
            }
        }

        /// <summary>
        /// Ist dieses Kleinfeld ein Teleportfeld?
        /// </summary>
        public static bool IstTeleportfeld(KleinFeld? kleinfeld) {
            if (kleinfeld == null)
                return false;
            return kleinfeld.TerrainType == TerrainType.Auftauchpunkt
                || kleinfeld.TerrainType == TerrainType.AuftauchpunktUnbekannt
                || kleinfeld.TerrainType == TerrainType.Tiefseeeinbahnpunkt;
        }

        /// <summary>
        /// Die Art des Teleportfeldes an dieser Position
        /// </summary>
        public static TeleportArt GetArt(KleinfeldPosition? position) {
            if (position == null)
                return TeleportArt.Keins;
            string bezeichner = position.CreateBezeichner();
            foreach (var feld in GetTeleportfelder()) {
                if (feld.Position.CreateBezeichner() == bezeichner)
                    return feld.Art;
            }
            return TeleportArt.Keins;
        }

        /// <summary>
        /// Die Teleportpunkte, die von dem übergebenen Teleportfeld aus angesteuert werden können.
        ///
        /// Vom Piratennest aus ist das der von der Spielleitung vorgegebene Auftauchpunkt - die
        /// Anwendung kann ihn nicht kennen und bietet deshalb alle in Frage kommenden an.
        /// Von Erkenfara aus geht es immer zum Piratennest.
        /// Die Tiefseepunkte sind Einbahnstraßen und führen nur zum Nest.
        /// </summary>
        public static List<Teleportfeld> GetMöglicheZiele(KleinfeldPosition? teleportfeld) {
            var art = GetArt(teleportfeld);
            var alle = GetTeleportfelder();
            return art switch {
                // von der Insel nach Erkenfara: die sechs Randfelder und die beiden Tiefseepunkte
                TeleportArt.Piratennest => alle.Where(f => f.Art == TeleportArt.Erkenfara || f.Art == TeleportArt.Tiefsee).ToList(),
                // von Erkenfara zum Nest
                TeleportArt.Erkenfara or TeleportArt.Tiefsee => alle.Where(f => f.Art == TeleportArt.Piratennest).ToList(),
                _ => [],
            };
        }

        /// <summary>
        /// Die Teleportpunkte, die in diesem Zug tatsächlich angesteuert werden dürfen.
        ///
        /// Für die Reise von der Pirateninsel nach Erkenfara gibt die Spielleitung den Auftauchpunkt
        /// vier Züge im Voraus vor (Regelwerk 6.6.3); er steht in der Tabelle Zugreihenfolge. Ist er
        /// für den laufenden Monat bekannt, bleibt genau dieser Punkt übrig. Fehlt der Eintrag,
        /// werden alle regelkonformen Punkte angeboten.
        /// </summary>
        public static List<Teleportfeld> GetVorgegebeneZiele(KleinfeldPosition? teleportfeld) {
            var möglich = GetMöglicheZiele(teleportfeld);
            if (GetArt(teleportfeld) != TeleportArt.Piratennest)
                return möglich;

            var vorgabe = ZugView.GetAuftauchpunkt();
            if (vorgabe == null)
                return möglich;
            var gefiltert = möglich.Where(p => p.Position.gf == vorgabe.gf && p.Position.kf == vorgabe.kf).ToList();
            if (gefiltert.Count == 0) {
                ProgramView.LogWarning($"Der vorgegebene Auftauchpunkt {vorgabe.CreateBezeichner()} ist kein Teleportfeld",
                    $"In der Zugreihenfolge steht für diesen Monat {vorgabe.CreateBezeichner()} als Auftauchpunkt, auf der Karte ist das aber kein Teleportfeld. Es werden alle Punkte angeboten.");
                return möglich;
            }
            return gefiltert;
        }

        /// <summary>
        /// Kostet das Auftauchen selbst noch Bewegungspunkte?
        ///
        /// Von der Insel nach Erkenfara ja: der Auftauchpunkt "zählt als 2tes Wasserfeld"
        /// (Regelwerk 6.6.4, im dortigen Beispiel zieht sich das Reich eine weitere Seebewegung ab).
        /// Von Erkenfara zum Nest nein: dort werden "erst ab hier" weitere Bewegungspunkte fällig
        /// (Regelwerk 6.6.5), das Auftauchen ist also mit dem Betreten des Teleportfeldes abgegolten.
        /// </summary>
        public static bool KostetAuftauchen(TeleportArt artDesAbfahrtsfeldes) {
            return artDesAbfahrtsfeldes == TeleportArt.Piratennest;
        }

        /// <summary>
        /// Prüft, ob die Figur von dem Teleportfeld, auf dem sie steht, auf dem gewünschten Feld
        /// auftauchen darf, und ermittelt die Kosten dafür.
        /// </summary>
        public static SchrittErgebnis PrüfeAuftauchen(Spielfigur figur, KleinFeld? teleportfeld, KleinfeldPosition? auftauchfeld) {
            if (teleportfeld == null || IstTeleportfeld(teleportfeld) == false)
                return SchrittErgebnis.Fehler("Das ist kein Teleportfeld",
                    $"Ein Auftauchen ist nur von einem Teleportfeld aus möglich, {teleportfeld?.CreateBezeichner()} ist keines");

            var ziel = KleinfeldView.GetKleinfeld(auftauchfeld);
            if (ziel == null)
                return SchrittErgebnis.Fehler("Das Auftauchfeld liegt nicht auf der Karte",
                    $"{auftauchfeld?.CreateBezeichner()} wurde in den Kartendaten nicht gefunden");

            var art = GetArt(teleportfeld);
            var möglicheZiele = GetMöglicheZiele(teleportfeld);
            var punkt = möglicheZiele.FirstOrDefault(p => GetAuftauchfelder(p.Position).Any(f => f.gf == ziel.gf && f.kf == ziel.kf));
            if (punkt == null)
                return SchrittErgebnis.Fehler("Dort kann nicht aufgetaucht werden",
                    $"{ziel.CreateBezeichner()} grenzt an keinen der von {teleportfeld.CreateBezeichner()} aus erreichbaren Teleportpunkte: "
                    + string.Join(", ", möglicheZiele.Select(p => p.ToString())));

            var ergebnis = new SchrittErgebnis("Auftauchen möglich",
                $"{figur.Bezeichner} taucht bei {punkt.Position.CreateBezeichner()} auf und erscheint auf {ziel.CreateBezeichner()}", false) {
                Start = teleportfeld,
                Ziel = ziel,
                Teleportpunkt = true,
                Wegerecht = BewegungsRules.HatWegerecht(figur, ziel),
                HöhenstufenGesamt = figur.hoehenstufen,
            };

            if (KostetAuftauchen(art)) {
                var verbrauch = BewegungsRules.GetVerbrauch(figur, ziel.Gelaendetyp ?? 0, ergebnis.Wegerecht, false);
                if (verbrauch == null)
                    return SchrittErgebnis.Fehler("Keine Bewegungsdaten vorhanden",
                        $"Für {figur.Typ} gibt es keinen Eintrag für den Geländetyp {ziel.Gelaendetyp} des Auftauchfeldes {ziel.CreateBezeichner()}");
                ergebnis.BPKosten = verbrauch.BP;
            }

            if (ergebnis.BPKosten > figur.bp)
                return SchrittErgebnis.Fehler("Die Einheit hat nicht genug Bewegungspunkte",
                    $"Das Auftauchen auf {ziel.CreateBezeichner()} kostet {ergebnis.BPKosten} BP, {figur.Bezeichner} hat aber nur noch {figur.bp}");
            ergebnis.BPRest = figur.bp - ergebnis.BPKosten;
            return ergebnis;
        }

        /// <summary>
        /// Aufgetaucht wird nie auf dem Teleportpunkt selbst, sondern auf einem der sechs Kleinfelder
        /// ringsherum. Geliefert werden nur die Felder, die auf der Karte liegen und Wasser sind.
        ///
        /// Welches der sechs es ist, gibt die Spielleitung vor. Da diese Angabe in den Daten fehlt,
        /// liefert die Funktion alle in Frage kommenden und die Auswahl trifft der Benutzer.
        /// </summary>
        public static List<KleinFeld> GetAuftauchfelder(KleinfeldPosition? teleportpunkt) {
            List<KleinFeld> result = [];
            if (teleportpunkt == null)
                return result;
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KleinfeldView.GetKleinfeld(KartenKoordinaten.GetNachbar(teleportpunkt, richtung));
                if (nachbar != null && nachbar.IsWasser)
                    result.Add(nachbar);
            }
            return result;
        }
    }
}
