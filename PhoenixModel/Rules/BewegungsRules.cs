using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.Extensions;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Bewegungsart bestimmt, welche der BEW_* Tabellen aus der crossref.mdb für den
    /// Bewegungspunkteverbrauch herangezogen wird. Die Namen entsprechen 1:1 den Tabellennamen.
    /// </summary>
    public enum BewegungsArt {
        Unbekannt,
        Chars,
        Kreaturen,
        Krieger,
        LKP,
        LKS,
        PiratenChars,
        PiratenLKS,
        PiratenSchiffe,
        PiratenSKS,
        Reiter,
        Schiffe,
        SKP,
        SKS,
    }

    /// <summary>
    /// Der Verbrauch eines einzelnen Bewegungsschrittes
    /// </summary>
    public class Bewegungskosten {
        /// <summary>
        /// Die verbrauchten Bewegungspunkte
        /// </summary>
        public int BP { get; set; }
        /// <summary>
        /// Die Höhenstufe des Zielgeländes (nicht der Verbrauch, siehe <see cref="SchrittErgebnis.HöhenstufenVerbrauch"/>)
        /// </summary>
        public int Höhenstufe { get; set; }
    }

    /// <summary>
    /// Das Ergebnis der Prüfung eines einzelnen Bewegungsschrittes von einem Kleinfeld auf ein benachbartes.
    /// Enthält im Erfolgsfall alle Werte, die für die Durchführung des Schrittes benötigt werden.
    /// </summary>
    public class SchrittErgebnis : Result {
        public SchrittErgebnis(string title, string message, bool hasErrors) : base(title, message, hasErrors) { }

        public static SchrittErgebnis Fehler(string titel, string nachricht) => new(titel, nachricht, true);

        /// <summary>Das Kleinfeld, auf dem die Figur vor dem Schritt steht</summary>
        public KleinFeld? Start { get; set; } = null;
        /// <summary>Das Kleinfeld, auf das die Figur ziehen möchte</summary>
        public KleinFeld? Ziel { get; set; } = null;
        /// <summary>Die Richtung von <see cref="Start"/> nach <see cref="Ziel"/></summary>
        public Direction Richtung { get; set; } = Direction.NW;
        /// <summary>Die Bewegungspunkte, die dieser Schritt kostet</summary>
        public int BPKosten { get; set; } = 0;
        /// <summary>Die Bewegungspunkte, die der Figur nach dem Schritt verbleiben</summary>
        public int BPRest { get; set; } = 0;
        /// <summary>Die Höhenstufenpunkte, die dieser Schritt kostet (0, 1 oder 2)</summary>
        public int HöhenstufenVerbrauch { get; set; } = 0;
        /// <summary>Die Summe der Höhenstufenpunkte nach dem Schritt</summary>
        public int HöhenstufenGesamt { get; set; } = 0;
        /// <summary>Die Figur bewegt sich auf Gebiet, für das sie Wegerecht bzw. Küstenrecht besitzt</summary>
        public bool Wegerecht { get; set; } = false;
        /// <summary>Der Schritt erfolgt über eine Straße - bestimmt die Spalte in der BEW_* Tabelle</summary>
        public bool Straße { get; set; } = false;
        /// <summary>Der Schritt erfolgt über eine Kaianlage - vermindert wie eine Straße die Höhenstufenkosten</summary>
        public bool Kai { get; set; } = false;
        /// <summary>Das Zielfeld wird betreten, ohne dass Wegerecht besteht - es wird also erobert oder geplündert</summary>
        public bool Erobert { get; set; } = false;
        /// <summary>Der Schritt führt über einen feindlichen Wall - danach sind alle Bewegungspunkte aufgebraucht</summary>
        public bool ÜberFeindlichenWall { get; set; } = false;
        /// <summary>Das Zielfeld ist ein Teleportfeld, der eigentliche Zielpunkt muss noch ausgewählt werden</summary>
        public bool Teleportpunkt { get; set; } = false;
    }

    /// <summary>
    /// Alle Regeln rund um die Bewegung von Spielfiguren.
    ///
    /// Regelwerk Kapitel 4:
    /// - Jedes Heer bewegt sich mit der für seine langsamsten Heeresteile typischen Geschwindigkeit.
    /// - Alle Heere, die sich nicht auf Straßen bewegen oder Kaianlagen benutzen, dürfen in einem Monat
    ///   nur eine Höhenstufe überwinden.
    /// - Jede Einheit verfügt über 2 Höhenstufenpunkte, das Überwinden einer Höhenstufe kostet ohne
    ///   Hilfsmittel 2 Punkte. Eine Straße oder eine Kaianlage vermindern die Kosten auf 1.
    /// - Unverbrauchte Bewegungspunkte entfallen ersatzlos.
    ///
    /// Die eigentlichen Verbrauchswerte stehen in den Tabellen BEW_* der crossref.mdb (Tabelle 5 des Regelwerks).
    /// </summary>
    public static class BewegungsRules {

        /// <summary>
        /// Jede Einheit verfügt pro Zug über 2 Höhenstufenpunkte (Regelwerk Kapitel 4)
        /// </summary>
        public const int MaxHöhenstufenPunkte = 2;

        /// <summary>
        /// Das Überwinden einer Höhenstufe ohne Hilfsmittel kostet 2 Punkte
        /// </summary>
        public const int HöhenstufenKostenOhneStraße = 2;

        /// <summary>
        /// Eine Straße oder eine Kaianlage vermindert die Kosten einer Höhenstufe auf 1
        /// </summary>
        public const int HöhenstufenKostenMitStraße = 1;

        /// <summary>
        /// Gelände, das eine Figur überhaupt nicht betreten kann, steht in den BEW_* Tabellen mit
        /// 99 Bewegungspunkten - mehr, als eine Figur je hat. Das ist eine Schreibweise der
        /// Tabelle, kein Preis: der Unterschied zwischen "zu teuer" und "unmöglich" geht sonst
        /// verloren.
        /// </summary>
        public const int BPUnpassierbar = 99;

        /// <summary>
        /// Die Benutzung eines Teleportfeldes kostet 20 Bewegungspunkte (Regelwerk 6.6.5). Dieser Wert
        /// steht bereits in den BEW_* Tabellen beim Geländetyp 10 (auftauchpunkt) und wird von dort
        /// gelesen; die Konstante dient nur der Dokumentation und als Prüfwert.
        /// </summary>
        public const int BPKostenTeleportfeld = 20;

        /// <summary>
        /// Ein Bauauftrag markiert Straßen, Wälle, Brücken und Kaianlagen auf dem Kleinfeld mit -1.
        /// Solche Baustellen sind noch nicht benutzbar, daher zählt für die Bewegung nur ein Wert > 0.
        /// <seealso cref="PhoenixModel.View.RuestungBauwerkeView.UpdateKleinFeld"/>
        /// </summary>
        private static bool IstFertig(int? wert) => wert != null && wert > 0;

        #region Richtungen

        /// <summary>
        /// Liefert die Gegenrichtung. Die Richtungen sind im Uhrzeigersinn angeordnet,
        /// die Gegenrichtung liegt daher immer 3 Schritte weiter.
        /// </summary>
        public static Direction Gegenrichtung(Direction richtung) {
            return (Direction)(((int)richtung + 3) % 6);
        }

        /// <summary>
        /// Ermittelt, in welcher Richtung das Zielfeld vom Startfeld aus liegt.
        /// Liefert null, wenn die beiden Kleinfelder keine direkten Nachbarn sind.
        /// </summary>
        public static Direction? GetRichtung(KleinfeldPosition von, KleinfeldPosition nach) {
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KartenKoordinaten.GetNachbar(von, richtung);
                if (nachbar != null && nachbar.gf == nach.gf && nachbar.kf == nach.kf)
                    return richtung;
            }
            return null;
        }

        #endregion

        #region Zugriff auf die BEW_* Tabellen

        private static readonly Dictionary<BewegungsArt, Dictionary<int, BEW>> _bewCache = [];
        private static readonly object _bewCacheLock = new();

        /// <summary>
        /// Muss aufgerufen werden, wenn die Crossreferenzen neu geladen wurden.
        /// </summary>
        public static void ResetCache() {
            lock (_bewCacheLock) {
                _bewCache.Clear();
            }
        }

        /// <summary>
        /// Liefert die Rohdaten der zur Bewegungsart gehörenden BEW_* Tabelle
        /// </summary>
        private static IEnumerable<BEW>? GetTabelle(BewegungsArt art) {
            return art switch {
                BewegungsArt.Chars => SharedData.BEW_chars,
                BewegungsArt.Kreaturen => SharedData.BEW_Kreaturen,
                BewegungsArt.Krieger => SharedData.BEW_Krieger,
                BewegungsArt.LKP => SharedData.BEW_LKP,
                BewegungsArt.LKS => SharedData.BEW_LKS,
                BewegungsArt.PiratenChars => SharedData.BEW_PiratenChars,
                BewegungsArt.PiratenLKS => SharedData.BEW_PiratenLKS,
                BewegungsArt.PiratenSchiffe => SharedData.BEW_PiratenSchiffe,
                BewegungsArt.PiratenSKS => SharedData.BEW_PiratenSKS,
                BewegungsArt.Reiter => SharedData.BEW_Reiter,
                BewegungsArt.Schiffe => SharedData.BEW_Schiffe,
                BewegungsArt.SKP => SharedData.BEW_SKP,
                BewegungsArt.SKS => SharedData.BEW_SKS,
                _ => null,
            };
        }

        /// <summary>
        /// Ermittelt, ob die Nation der Figur das Piratenreich ist. Piraten haben eigene
        /// Bewegungstabellen für Schiffe und Charaktere (Regelwerk Kapitel Piraten).
        /// </summary>
        public static bool IstPirat(Nation? nation) {
            return nation != null && nation.Nummer == ReichTabelle.Piraten;
        }

        /// <summary>
        /// Ermittelt, welche BEW_* Tabelle für diese Spielfigur gilt.
        /// Der Typ einer Spielfigur berücksichtigt bereits mitgeführte Katapulte bzw. Kriegsschiffe,
        /// wobei die schweren Varianten Vorrang vor den leichten haben.
        /// </summary>
        public static BewegungsArt GetBewegungsArt(Spielfigur? figur) {
            if (figur == null)
                return BewegungsArt.Unbekannt;
            bool pirat = IstPirat(figur.Nation);
            return figur.Typ switch {
                FigurType.Krieger => BewegungsArt.Krieger,
                FigurType.Reiter => BewegungsArt.Reiter,
                FigurType.Kreatur => BewegungsArt.Kreaturen,
                FigurType.Charakter or FigurType.Zauberer or FigurType.CharakterZauberer
                    => pirat ? BewegungsArt.PiratenChars : BewegungsArt.Chars,
                FigurType.LeichteArtillerie or FigurType.BeritteneLeichteArtillerie => BewegungsArt.LKP,
                FigurType.SchwereArtillerie or FigurType.BeritteneSchwereArtillerie => BewegungsArt.SKP,
                FigurType.Schiff => pirat ? BewegungsArt.PiratenSchiffe : BewegungsArt.Schiffe,
                FigurType.LeichtesKriegsschiff => pirat ? BewegungsArt.PiratenLKS : BewegungsArt.LKS,
                FigurType.SchweresKriegsschiff => pirat ? BewegungsArt.PiratenSKS : BewegungsArt.SKS,
                FigurType.PiratenSchiff => BewegungsArt.PiratenSchiffe,
                FigurType.PiratenLeichtesKriegsschiff => BewegungsArt.PiratenLKS,
                FigurType.PiratenSchweresKriegsschiff => BewegungsArt.PiratenSKS,
                _ => BewegungsArt.Unbekannt,
            };
        }

        /// <summary>
        /// Liefert die Zeile der BEW_* Tabelle für den übergebenen Geländetyp
        /// </summary>
        public static BEW? GetBewegungsdaten(BewegungsArt art, int geländetyp) {
            if (art == BewegungsArt.Unbekannt)
                return null;
            lock (_bewCacheLock) {
                if (_bewCache.TryGetValue(art, out var tabelle) == false) {
                    var quelle = GetTabelle(art);
                    if (quelle == null)
                        return null;
                    tabelle = [];
                    foreach (var zeile in quelle)
                        tabelle[zeile.Gelaende] = zeile;
                    // solange nichts geladen ist, wird auch nichts gecached
                    if (tabelle.Count == 0)
                        return null;
                    _bewCache[art] = tabelle;
                }
                return tabelle.TryGetValue(geländetyp, out var bew) ? bew : null;
            }
        }

        /// <summary>
        /// Liefert die Zeile der BEW_* Tabelle, die für die Bewegung dieser Figur in dieses Gelände gilt
        /// </summary>
        public static BEW? GetBewegungsdaten(Spielfigur? figur, int geländetyp) {
            return GetBewegungsdaten(GetBewegungsArt(figur), geländetyp);
        }

        /// <summary>
        /// Die Höhenstufe eines Geländes ist unabhängig von der Figur, die es betritt.
        /// Sie wird daher - wie in der Altanwendung - immer aus der Tabelle der Krieger gelesen.
        /// </summary>
        public static int GetHöhenstufe(int geländetyp) {
            var bew = GetBewegungsdaten(BewegungsArt.Krieger, geländetyp);
            return bew?.hoehenstufe ?? 0;
        }

        /// <summary>
        /// Die Höhenstufe des Kleinfeldes
        /// </summary>
        public static int GetHöhenstufe(KleinFeld? kf) {
            return kf?.Gelaendetyp == null ? 0 : GetHöhenstufe(kf.Gelaendetyp.Value);
        }

        /// <summary>
        /// Ermittelt den Bewegungspunkteverbrauch für das Betreten eines Geländes
        /// </summary>
        /// <param name="figur">die Figur, die sich bewegt</param>
        /// <param name="geländetyp">der Geländetyp des Zielfeldes</param>
        /// <param name="wegerecht">besteht auf dem Zielfeld Wege- bzw. Küstenrecht?</param>
        /// <param name="straße">führt eine benutzbare Straße oder Kaianlage auf das Zielfeld?</param>
        public static Bewegungskosten? GetVerbrauch(Spielfigur? figur, int geländetyp, bool wegerecht, bool straße) {
            var bew = GetBewegungsdaten(figur, geländetyp);
            if (bew == null)
                return null;
            int bp;
            if (wegerecht && straße)
                bp = bew.strasse_und_wegerecht;
            else if (wegerecht)
                bp = bew.wegerecht;
            else if (straße)
                bp = bew.strasse;
            else
                bp = bew.standart;
            return new Bewegungskosten { BP = bp, Höhenstufe = bew.hoehenstufe };
        }

        #endregion

        #region Rechte auf dem Zielfeld

        /// <summary>
        /// Auf Wasser gilt die Küstengewässerregel, auf Land die Wegeregel. Das eigene Reichsgebiet
        /// ist immer erlaubt.
        /// </summary>
        public static bool HatWegerecht(Spielfigur? figur, KleinFeld? ziel) {
            if (ziel == null)
                return false;
            // eigenes Gebiet ist immer erlaubt
            if (figur?.Nation != null && ziel.Nation != null && figur.Nation == ziel.Nation)
                return true;
            if (ziel.IsWasser)
                return ziel.HasKüstenRecht;
            return ziel.HasWegeRecht;
        }

        #endregion

        #region Prüfung eines einzelnen Schrittes

        /// <summary>
        /// Prüft, ob die Figur einen Schritt in die angegebene Richtung ziehen darf und ermittelt
        /// dabei alle Werte, die für die Durchführung benötigt werden. Es wird nichts verändert.
        /// </summary>
        public static SchrittErgebnis PrüfeSchritt(Spielfigur? figur, Direction richtung) {
            if (figur == null)
                return SchrittErgebnis.Fehler("Keine Figur ausgewählt", "Für eine Bewegung muss zuerst eine Spielfigur ausgewählt werden");
            var start = KleinfeldView.GetKleinfeld(figur);
            if (start == null)
                return SchrittErgebnis.Fehler("Die Figur steht auf keinem bekannten Kleinfeld",
                    $"Die Figur {figur.Bezeichner} steht auf {figur.CreateBezeichner()}, das nicht auf der Karte gefunden wurde");
            var zielPosition = KartenKoordinaten.GetNachbar(start, richtung);
            var ziel = KleinfeldView.GetKleinfeld(zielPosition);
            if (ziel == null)
                return SchrittErgebnis.Fehler("Dort geht es nicht weiter",
                    $"Im {(DirectionNames)richtung} von {start.CreateBezeichner()} endet die bekannte Welt");
            return PrüfeSchritt(figur, start, ziel, richtung);
        }

        /// <summary>
        /// Prüft einen Schritt von einem Kleinfeld auf ein direkt benachbartes Kleinfeld.
        /// Die Reihenfolge der Prüfungen entspricht der Altanwendung (PZE_Main.bewegeUnit).
        /// </summary>
        public static SchrittErgebnis PrüfeSchritt(Spielfigur figur, KleinFeld start, KleinFeld ziel, Direction richtung) {
            return PrüfeSchritt(figur, start, ziel, richtung, figur.bp, figur.hoehenstufen);
        }

        /// <summary>
        /// Prüft einen Schritt für einen frei vorgegebenen Bewegungszustand, ohne die Figur zu lesen
        /// oder zu verändern. Damit lässt sich ein ganzer Weg durchrechnen, bevor die Figur bewegt wird.
        /// </summary>
        /// <param name="bpVerfügbar">die an dieser Stelle des Weges noch vorhandenen Bewegungspunkte</param>
        /// <param name="höhenstufenVerbraucht">die an dieser Stelle des Weges bereits verbrauchten Höhenstufenpunkte</param>
        public static SchrittErgebnis PrüfeSchritt(Spielfigur figur, KleinFeld start, KleinFeld ziel, Direction richtung,
                                                   int bpVerfügbar, int höhenstufenVerbraucht) {
            Direction gegenrichtung = Gegenrichtung(richtung);
            // Flüsse und fremde Wälle lassen sich nur als allererster Schritt des Zuges überwinden
            bool istErsterSchrittDesZuges = bpVerfügbar == figur.bp_max;

            var ergebnis = new SchrittErgebnis("Zug OK", $"{figur.Bezeichner} zieht von {start.CreateBezeichner()} nach {ziel.CreateBezeichner()}", false) {
                Start = start,
                Ziel = ziel,
                Richtung = richtung,
            };

            // Bewegt wird erst, wenn die Rüstphase abgeschlossen ist (Regelwerk Kapitel 3)
            if (ZugView.KannBewegen == false)
                return SchrittErgebnis.Fehler($"In der {ZugView.PhasenBeschreibung} wird nicht bewegt",
                    "Erst wenn die Rüstphase abgeschlossen ist, können Figuren bewegt werden.");

            // Eingeschiffte Truppen bewegen sich nicht selbst, sondern werden von der Flotte getragen.
            // Was als eingeschifft gilt, sagt IsOnShip - ein Leerzeichen in auf_Flotte ist keine Flotte.
            if (figur.IsOnShip())
                return SchrittErgebnis.Fehler("Diese Einheit ist eingeschifft",
                    $"{figur.Bezeichner} befindet sich auf der Flotte {((TruppenSpielfigur)figur).auf_Flotte} und muss erst ausgeschifft werden");

            // Ein Teleportfeld führt nicht auf das Nachbarfeld, sondern auf den zugehörigen Auftauchpunkt
            ergebnis.Teleportpunkt = ziel.TerrainType == TerrainType.Auftauchpunkt
                                  || ziel.TerrainType == TerrainType.AuftauchpunktUnbekannt
                                  || ziel.TerrainType == TerrainType.Tiefseeeinbahnpunkt;

            // Für den Bewegungspunkteverbrauch zählt nur eine Straße. Eine Kaianlage wirkt zwar
            // "wie eine Straße" (Regelwerk 1.5.2), das bezieht sich aber auf die Höhenstufen -
            // in den BEW_* Tabellen steht in der Spalte strasse für Wasserfelder 99, also unpassierbar.
            ergebnis.Straße = HatStraße(start, ziel, richtung, gegenrichtung);
            ergebnis.Kai = HatKai(start, ziel, richtung, gegenrichtung);
            ergebnis.Wegerecht = HatWegerecht(figur, ziel);

            // 1. Bewegungspunkte. Die Tabelle liefert 99 für Gelände, das diese Figur nicht betreten
            //    kann - das schlägt dann unten als "nicht genug Bewegungspunkte" durch.
            var verbrauch = GetVerbrauch(figur, ziel.Gelaendetyp ?? 0, ergebnis.Wegerecht, ergebnis.Straße);
            if (verbrauch == null)
                return SchrittErgebnis.Fehler("Keine Bewegungsdaten vorhanden",
                    $"Für {figur.Typ} gibt es in der Tabelle {GetBewegungsArt(figur)} der crossref.mdb keinen Eintrag für den Geländetyp {ziel.Gelaendetyp}");

            ergebnis.BPKosten = verbrauch.BP;
            if (ergebnis.BPKosten > bpVerfügbar)
                return SchrittErgebnis.Fehler("Die Einheit hat nicht genug Bewegungspunkte",
                    $"Der Schritt nach {ziel.CreateBezeichner()} ({ziel.Terrain.Name}) kostet {ergebnis.BPKosten} BP, {figur.Bezeichner} hat aber nur noch {bpVerfügbar} von {figur.bp_max} BP");
            ergebnis.BPRest = bpVerfügbar - ergebnis.BPKosten;

            // 2. Höhenstufen
            int höhendifferenz = GetHöhenstufe(start) - GetHöhenstufe(ziel);
            ergebnis.HöhenstufenGesamt = höhenstufenVerbraucht;
            if (höhendifferenz != 0) {
                if (höhendifferenz < -1 || höhendifferenz > 1)
                    return SchrittErgebnis.Fehler("Mehr als eine Höhenstufe",
                        $"Zwischen {start.Terrain.Name} und {ziel.Terrain.Name} liegen {Math.Abs(höhendifferenz)} Höhenstufen. In einem Zug kann nur eine Höhenstufe überwunden werden");
                // Eine Straße oder eine Kaianlage vermindert die Kosten einer Höhenstufe auf 1
                ergebnis.HöhenstufenVerbrauch = (ergebnis.Straße || ergebnis.Kai) ? HöhenstufenKostenMitStraße : HöhenstufenKostenOhneStraße;
                ergebnis.HöhenstufenGesamt += ergebnis.HöhenstufenVerbrauch;
            }

            // 3. Ein Fluss ohne Brücke kostet eine weitere Höhenstufe und kann nur als erster Schritt
            //    des Zuges überwunden werden
            bool flussImWeg = KleinfeldView.HasRiver(ziel, gegenrichtung) || KleinfeldView.HasRiver(start, richtung);
            bool brückeVorhanden = IstFertig(GetBrücke(start, richtung)) || IstFertig(GetBrücke(ziel, gegenrichtung));
            if (flussImWeg && brückeVorhanden == false) {
                if (istErsterSchrittDesZuges == false)
                    return SchrittErgebnis.Fehler("Fluss im Weg",
                        $"Der Fluss zwischen {start.CreateBezeichner()} und {ziel.CreateBezeichner()} hat keine Brücke und kann nur zu Beginn des Zuges überwunden werden");
                ergebnis.HöhenstufenVerbrauch += HöhenstufenKostenOhneStraße;
                ergebnis.HöhenstufenGesamt += HöhenstufenKostenOhneStraße;
            }

            // 4. Ein fremder Wall kostet den Rest des Zuges und kann nur zu Beginn überwunden werden
            bool wallImWeg = IstFertig(GetWall(ziel, gegenrichtung)) || IstFertig(GetWall(start, richtung));
            if (wallImWeg && ergebnis.Wegerecht == false) {
                if (istErsterSchrittDesZuges == false)
                    return SchrittErgebnis.Fehler("Wall im Weg",
                        $"Der Wall von {ziel.CreateBezeichner()} kann nur zu Beginn des Zuges überwunden werden");
                ergebnis.ÜberFeindlichenWall = true;
                ergebnis.BPRest = 0;
            }

            // 5. Die Höhenstufenpunkte des Zuges sind begrenzt
            if (ergebnis.HöhenstufenGesamt > MaxHöhenstufenPunkte)
                return SchrittErgebnis.Fehler("Zu viele Höhenstufen in diesem Zug",
                    $"{figur.Bezeichner} hat in diesem Zug bereits {höhenstufenVerbraucht} von {MaxHöhenstufenPunkte} Höhenstufenpunkten verbraucht, dieser Schritt würde {ergebnis.HöhenstufenVerbrauch} weitere kosten");

            // 6. Wird fremdes Gebiet betreten, so wird es erobert oder geplündert.
            //    Auf Wasser, in der Tiefsee und auf Teleportfeldern gibt es nichts zu erobern.
            ergebnis.Erobert = ergebnis.Wegerecht == false && IstEroberbar(ziel);

            return ergebnis;
        }

        /// <summary>
        /// Auf Wasser, Tiefsee und Teleportfeldern gibt es nichts zu erobern oder zu plündern
        /// </summary>
        public static bool IstEroberbar(KleinFeld ziel) {
            return ziel.TerrainType != TerrainType.Wasser
                && ziel.TerrainType != TerrainType.Tiefsee
                && ziel.TerrainType != TerrainType.Auftauchpunkt
                && ziel.TerrainType != TerrainType.AuftauchpunktUnbekannt
                && ziel.TerrainType != TerrainType.Tiefseeeinbahnpunkt;
        }

        /// <summary>
        /// Eine Straße zwischen zwei Kleinfeldern. Beide Seiten werden geprüft, da die Karte das
        /// Element nicht immer auf beiden Feldern führt.
        /// </summary>
        private static bool HatStraße(KleinFeld start, KleinFeld ziel, Direction richtung, Direction gegenrichtung) {
            return IstFertig(GetStraße(ziel, gegenrichtung)) || IstFertig(GetStraße(start, richtung));
        }

        /// <summary>
        /// Eine Kaianlage zwischen zwei Kleinfeldern. Beide Seiten werden geprüft, da die Karte das
        /// Element nicht immer auf beiden Feldern führt.
        /// </summary>
        private static bool HatKai(KleinFeld start, KleinFeld ziel, Direction richtung, Direction gegenrichtung) {
            return IstFertig(GetKai(ziel, gegenrichtung)) || IstFertig(GetKai(start, richtung));
        }

        /// <summary>
        /// Eine Kaianlage zwischen zwei Kleinfeldern, ohne dass der Aufrufer die Gegenrichtung
        /// kennen muss. Wird auch beim Einschiffen gebraucht, wo die Kaianlage die Höhenstufe
        /// verbilligt (Regelwerk 1.5.2).
        /// </summary>
        public static bool HatKai(KleinFeld? start, KleinFeld? ziel, Direction richtung) {
            if (start == null || ziel == null)
                return false;
            return HatKai(start, ziel, richtung, Gegenrichtung(richtung));
        }

        private static int? GetStraße(KleinFeld kf, Direction direction) => direction switch {
            Direction.NW => kf.Strasse_NW,
            Direction.NO => kf.Strasse_NO,
            Direction.O => kf.Strasse_O,
            Direction.SO => kf.Strasse_SO,
            Direction.SW => kf.Strasse_SW,
            _ => kf.Strasse_W,
        };

        private static int? GetKai(KleinFeld kf, Direction direction) => direction switch {
            Direction.NW => kf.Kai_NW,
            Direction.NO => kf.Kai_NO,
            Direction.O => kf.Kai_O,
            Direction.SO => kf.Kai_SO,
            Direction.SW => kf.Kai_SW,
            _ => kf.Kai_W,
        };

        private static int? GetWall(KleinFeld kf, Direction direction) => direction switch {
            Direction.NW => kf.Wall_NW,
            Direction.NO => kf.Wall_NO,
            Direction.O => kf.Wall_O,
            Direction.SO => kf.Wall_SO,
            Direction.SW => kf.Wall_SW,
            _ => kf.Wall_W,
        };

        private static int? GetBrücke(KleinFeld kf, Direction direction) => direction switch {
            Direction.NW => kf.Bruecke_NW,
            Direction.NO => kf.Bruecke_NO,
            Direction.O => kf.Bruecke_O,
            Direction.SO => kf.Bruecke_SO,
            Direction.SW => kf.Bruecke_SW,
            _ => kf.Bruecke_W,
        };

        #endregion

        #region Wegsuche

        /// <summary>
        /// Das Ergebnis einer Wegsuche: die Folge der zu betretenden Kleinfelder und was sie kostet.
        /// </summary>
        public class Weg {
            /// <summary>Die Zielfelder aller Schritte in der Reihenfolge der Bewegung, ohne das Startfeld</summary>
            public List<KleinfeldPosition> Wegpunkte { get; } = [];
            /// <summary>Die insgesamt verbrauchten Bewegungspunkte</summary>
            public int BPKosten { get; set; }
            /// <summary>Die nach dem Weg insgesamt verbrauchten Höhenstufenpunkte</summary>
            public int HöhenstufenGesamt { get; set; }
        }

        /// <summary>
        /// Ein Suchzustand der Wegsuche: das Kleinfeld und die dort bereits verbrauchten Höhenstufenpunkte.
        /// Die Höhenstufen gehören zum Zustand, weil ein billigerer Weg mehr Höhenstufen kosten kann
        /// und die Figur dann später nicht mehr weiterkommt.
        /// </summary>
        private readonly record struct Suchzustand(string Feld, int Höhenstufen);

        /// <summary>
        /// Durchsucht mit Dijkstra alle Felder, die die Figur mit ihren verbleibenden Bewegungspunkten
        /// erreichen kann. Es wird nichts verändert.
        /// </summary>
        /// <param name="erlaubtesTeleportfeld">
        /// Teleportfelder sind normalerweise von der Suche ausgeschlossen, weil dort der Auftauchpunkt
        /// ausgewählt werden muss. Wird eines ausdrücklich angesteuert, darf die Suche dorthin führen.
        /// </param>
        private static (Dictionary<Suchzustand, int> kosten, Dictionary<Suchzustand, (Suchzustand vorher, KleinFeld feld)> herkunft)?
            DurchsucheErreichbareFelder(Spielfigur figur, string? erlaubtesTeleportfeld = null) {
            var start = KleinfeldView.GetKleinfeld(figur);
            if (start == null)
                return null;

            Dictionary<Suchzustand, int> kosten = [];
            Dictionary<Suchzustand, (Suchzustand, KleinFeld)> herkunft = [];
            PriorityQueue<(KleinFeld feld, int verbraucht, int höhenstufen), (int, int)> offen = new();

            var startZustand = new Suchzustand(start.CreateBezeichner(), figur.hoehenstufen);
            kosten[startZustand] = 0;
            offen.Enqueue((start, 0, figur.hoehenstufen), (0, figur.hoehenstufen));

            while (offen.Count > 0) {
                var (feld, verbraucht, höhenstufen) = offen.Dequeue();
                var zustand = new Suchzustand(feld.CreateBezeichner(), höhenstufen);
                // ein besserer Weg zu diesem Zustand wurde bereits gefunden
                if (kosten.TryGetValue(zustand, out int bekannt) && bekannt < verbraucht)
                    continue;

                foreach (Direction richtung in Enum.GetValues<Direction>()) {
                    var nachbarPosition = KartenKoordinaten.GetNachbar(feld, richtung);
                    var nachbar = KleinfeldView.GetKleinfeld(nachbarPosition);
                    if (nachbar == null)
                        continue;

                    var schritt = PrüfeSchritt(figur, feld, nachbar, richtung, figur.bp - verbraucht, höhenstufen);
                    if (schritt.HasErrors)
                        continue;
                    // Teleportfelder brauchen die Auswahl eines Auftauchpunktes und gehören daher
                    // nicht in einen automatisch gesuchten Weg - es sei denn, genau dieses wurde angesteuert
                    string nachbarKey = nachbar.CreateBezeichner();
                    if (schritt.Teleportpunkt && nachbarKey != erlaubtesTeleportfeld)
                        continue;

                    // über BPRest gerechnet, damit der feindliche Wall - der alles verbraucht - richtig zählt
                    int neueKosten = figur.bp - schritt.BPRest;
                    var neuerZustand = new Suchzustand(nachbarKey, schritt.HöhenstufenGesamt);
                    if (kosten.TryGetValue(neuerZustand, out int alt) && alt <= neueKosten)
                        continue;

                    kosten[neuerZustand] = neueKosten;
                    herkunft[neuerZustand] = (zustand, nachbar);
                    offen.Enqueue((nachbar, neueKosten, schritt.HöhenstufenGesamt), (neueKosten, schritt.HöhenstufenGesamt));
                }
            }
            return (kosten, herkunft);
        }

        /// <summary>
        /// Sucht den günstigsten Weg der Figur zu dem angegebenen Kleinfeld.
        /// </summary>
        /// <param name="fehler">die Begründung, falls kein Weg gefunden wurde</param>
        /// <returns>der Weg oder null, wenn das Ziel nicht erreichbar ist</returns>
        public static Weg? FindeWeg(Spielfigur? figur, KleinfeldPosition? zielPosition, out string fehler) {
            fehler = string.Empty;
            if (figur == null) {
                fehler = "Es ist keine Spielfigur ausgewählt";
                return null;
            }
            var start = KleinfeldView.GetKleinfeld(figur);
            var ziel = KleinfeldView.GetKleinfeld(zielPosition);
            if (start == null || ziel == null) {
                fehler = "Start- oder Zielfeld liegt nicht auf der Karte";
                return null;
            }
            if (start.gf == ziel.gf && start.kf == ziel.kf) {
                fehler = $"{figur.Bezeichner} steht bereits auf {ziel.CreateBezeichner()}";
                return null;
            }

            string zielKey = ziel.CreateBezeichner();
            var suche = DurchsucheErreichbareFelder(figur, TeleportRules.IstTeleportfeld(ziel) ? zielKey : null);
            if (suche == null) {
                fehler = "Die Figur steht auf keinem bekannten Kleinfeld";
                return null;
            }
            var (kosten, herkunft) = suche.Value;

            // unter allen Zuständen auf dem Zielfeld den günstigsten wählen
            Suchzustand? bester = null;
            int besteKosten = int.MaxValue;
            int besteHöhenstufen = int.MaxValue;
            foreach (var eintrag in kosten) {
                if (eintrag.Key.Feld != zielKey)
                    continue;
                if (eintrag.Value < besteKosten || (eintrag.Value == besteKosten && eintrag.Key.Höhenstufen < besteHöhenstufen)) {
                    besteKosten = eintrag.Value;
                    besteHöhenstufen = eintrag.Key.Höhenstufen;
                    bester = eintrag.Key;
                }
            }
            if (bester == null) {
                fehler = $"{ziel.CreateBezeichner()} ist für {figur.Bezeichner} mit {figur.bp} Bewegungspunkten nicht erreichbar";
                return null;
            }

            var weg = new Weg { BPKosten = besteKosten, HöhenstufenGesamt = bester.Value.Höhenstufen };
            var aktuell = bester.Value;
            List<KleinfeldPosition> rückwärts = [];
            while (herkunft.TryGetValue(aktuell, out var schritt)) {
                rückwärts.Add(new KleinfeldPosition(schritt.feld.gf, schritt.feld.kf));
                aktuell = schritt.vorher;
            }
            rückwärts.Reverse();
            weg.Wegpunkte.AddRange(rückwärts);

            // Wie viele Wegpunkte schon belegt sind, sagt die Spur selbst - nicht der Zähler schritt.
            // In den echten Zugdaten gibt es Figuren, deren x1/y1 gefüllt ist, während schritt auf 0
            // steht (Theostelos Reiter 201 in Zug 40, von der Altanwendung so hinterlassen). Nach dem
            // Zähler wäre dann ein Wegpunkt zu viel frei, und der letzte Schritt ginge beim Speichern
            // verloren.
            var spur = new Bewegungsspur(figur);
            int maxWegpunkte = spur.MaxWegpunkte;
            if (spur.Count + weg.Wegpunkte.Count > maxWegpunkte) {
                fehler = $"Der Weg hat {weg.Wegpunkte.Count} Schritte, in der Zugdatenbank ist aber nur Platz für insgesamt {maxWegpunkte} Wegpunkte je Figur";
                return null;
            }
            return weg;
        }

        /// <summary>
        /// Alle Kleinfelder, die die Figur in diesem Zug noch erreichen kann.
        ///
        /// Das Feld, auf dem sie steht, ist dabei, wenn sie es verlassen und wieder betreten kann.
        /// Das ist kein Versehen: Hin- und Herbewegen ist erlaubt und kostet Bewegungspunkte. Die
        /// Suche laeuft ueber Zustaende aus Feld und ueberwundenen Hoehenstufen, deshalb kann
        /// dasselbe Feld ueber einen Rundweg erneut auftauchen.
        ///
        /// Dass eine Figur Bewegungspunkte hat, heisst uebrigens nicht, dass sie ein Feld
        /// erreicht: schwere Artillerie ist so langsam, dass einstellige Restpunkte fuer keinen
        /// Schritt reichen. Dann kommt eine leere Liste zurueck.
        /// </summary>
        public static List<KleinFeld> GetErreichbareFelder(Spielfigur? figur) {
            List<KleinFeld> result = [];
            if (figur == null)
                return result;
            var suche = DurchsucheErreichbareFelder(figur);
            if (suche == null)
                return result;
            HashSet<string> gesehen = [];
            foreach (var eintrag in suche.Value.herkunft) {
                if (gesehen.Add(eintrag.Key.Feld))
                    result.Add(eintrag.Value.feld);
            }
            return result;
        }

        /// <summary>
        /// Sammelt alle Gruende, warum die Figur dieses Feld nicht erreicht.
        ///
        /// Die Karte zeigt nur, wohin es geht. Warum es anderswohin nicht geht, ist die haeufigere
        /// Frage - und die Antwort steht sonst nirgends: die Wegsuche verwirft einen Schritt und
        /// behaelt den Grund fuer sich.
        ///
        /// Gesucht wird in zwei Schichten. Zuerst das, was am Zielfeld selbst liegt und von keinem
        /// Weg abhaengt: Gelaende, das diese Figur gar nicht betreten kann, ein Teleportfeld, die
        /// falsche Zugphase. Danach jeder der sechs Nachbarn: steht die Figur dort ueberhaupt
        /// jemals, und wenn ja, woran scheitert der letzte Schritt?
        ///
        /// Eine leere Liste heisst, dass das Feld erreichbar ist.
        /// </summary>
        public static List<string> ErkläreUnerreichbarkeit(Spielfigur? figur, KleinFeld? ziel) {
            List<string> gründe = [];
            if (figur == null) {
                gründe.Add("Es ist keine Spielfigur ausgewählt.");
                return gründe;
            }
            if (ziel == null) {
                gründe.Add("Dieses Feld liegt nicht auf der Karte.");
                return gründe;
            }

            // 1. Was die Figur ueberhaupt am Ziehen hindert - dann eruebrigt sich der Rest
            if (ZugView.KannBewegen == false) {
                gründe.Add($"In der {ZugView.PhasenBeschreibung} wird nicht bewegt.");
                return gründe;
            }
            if (figur.IsOnShip()) {
                gründe.Add($"{figur.Bezeichner} ist auf der Flotte {((TruppenSpielfigur)figur).auf_Flotte} eingeschifft.");
                return gründe;
            }

            var start = KleinfeldView.GetKleinfeld(figur);
            if (start == null) {
                gründe.Add($"{figur.Bezeichner} steht auf keinem bekannten Kleinfeld.");
                return gründe;
            }
            if (figur.bp <= 0) {
                gründe.Add($"{figur.Bezeichner} hat keine Bewegungspunkte mehr.");
                return gründe;
            }
            // Auf dem eigenen Feld steht die Figur bereits - da ist nichts zu erklären. Es taucht
            // in der Liste der erreichbaren Felder nur auf, wenn ein Rundweg dorthin zurückführt.
            if (start.gf == ziel.gf && start.kf == ziel.kf)
                return gründe;

            // 2. Was am Zielfeld selbst liegt, unabhaengig vom Weg dorthin
            bool wegerecht = HatWegerecht(figur, ziel);
            var verbrauch = GetVerbrauch(figur, ziel.Gelaendetyp ?? 0, wegerecht, false);
            if (verbrauch == null) {
                gründe.Add($"Für {figur.Typ} gibt es keine Bewegungsdaten zum Geländetyp {ziel.Gelaendetyp}.");
                return gründe;
            }
            if (verbrauch.BP >= BPUnpassierbar) {
                // Damit ist alles gesagt. Die Nachbarfelder durchzugehen brächte nur noch
                // "nicht genug Bewegungspunkte" - dieselbe Sache ein zweites Mal.
                gründe.Add($"{ziel.Terrain.Name} ist für {figur.Typ} nicht passierbar.");
                return gründe;
            }
            if (verbrauch.BP > figur.bp_max) {
                // Nicht dasselbe wie unpassierbar: mit Wegerecht oder über eine Strasse kann
                // genau dieses Gelände bezahlbar sein. Hier reicht es auch mit vollen Punkten nicht.
                gründe.Add($"Ein Schritt auf {ziel.Terrain.Name} kostet {verbrauch.BP} BP"
                    + $"{(wegerecht ? string.Empty : " (ohne Wegerecht)")}; {figur.Typ} hat höchstens {figur.bp_max}.");
                return gründe;
            }
            if (verbrauch.BP > figur.bp)
                gründe.Add($"Das Feld zu betreten kostet {verbrauch.BP} BP, {figur.Bezeichner} hat noch {figur.bp}.");

            if (TeleportRules.IstTeleportfeld(ziel))
                gründe.Add("Auf einem Teleportfeld muss der Auftauchpunkt ausgewählt werden; die Wegsuche führt nicht von selbst dorthin.");

            // 3. Der letzte Schritt, von jedem Nachbarn aus
            var suche = DurchsucheErreichbareFelder(figur);
            if (suche == null)
                return gründe;
            var kosten = suche.Value.kosten;

            List<string> unerreichbareNachbarn = [];
            List<string> letzterSchritt = [];
            bool irgendeinSchrittMöglich = false;

            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbarPosition = KartenKoordinaten.GetNachbar(ziel, Gegenrichtung(richtung));
                var nachbar = KleinfeldView.GetKleinfeld(nachbarPosition);
                if (nachbar == null)
                    continue;

                string nachbarKey = nachbar.CreateBezeichner();
                // alle Zustände, in denen die Figur auf diesem Nachbarn stehen kann
                var zustände = kosten.Where(eintrag => eintrag.Key.Feld == nachbarKey).ToList();
                if (zustände.Count == 0) {
                    unerreichbareNachbarn.Add(nachbar.Bezeichner);
                    continue;
                }

                // der aussagekräftigste Fehlschlag: der mit den wenigsten verbrauchten Punkten
                SchrittErgebnis? bester = null;
                foreach (var zustand in zustände.OrderBy(eintrag => eintrag.Value)) {
                    var schritt = PrüfeSchritt(figur, nachbar, ziel, richtung,
                        figur.bp - zustand.Value, zustand.Key.Höhenstufen);
                    if (schritt.HasErrors == false) {
                        irgendeinSchrittMöglich = true;
                        break;
                    }
                    bester ??= schritt;
                }
                if (irgendeinSchrittMöglich == false && bester != null)
                    letzterSchritt.Add($"Von {nachbar.Bezeichner} aus: {bester.Title}.");
            }

            // Der Schritt gelingt von irgendwoher: dann liegt es nicht am Weg. Übrig bleibt, was
            // am Zielfeld selbst hängt - beim Teleportfeld etwa die fehlende Auswahl des
            // Auftauchpunktes. Ist auch das leer, ist das Feld schlicht erreichbar.
            if (irgendeinSchrittMöglich)
                return gründe;

            gründe.AddRange(letzterSchritt);
            if (unerreichbareNachbarn.Count > 0)
                gründe.Add(unerreichbareNachbarn.Count == 1
                    ? $"{unerreichbareNachbarn[0]} ist selbst nicht erreichbar."
                    : $"Diese Nachbarfelder sind selbst nicht erreichbar: {string.Join(", ", unerreichbareNachbarn)}.");

            if (gründe.Count == 0)
                gründe.Add($"{ziel.Bezeichner} ist mit {figur.bp} Bewegungspunkten nicht zu erreichen.");
            return gründe;
        }

        #endregion

        #region Erreichbare Nachbarfelder

        /// <summary>
        /// Ermittelt alle Nachbarfelder, die die Figur mit ihren verbleibenden Bewegungspunkten
        /// in einem Schritt betreten darf. Nützlich für die Anzeige in der Karte.
        /// </summary>
        public static Dictionary<Direction, SchrittErgebnis> GetMöglicheSchritte(Spielfigur? figur) {
            Dictionary<Direction, SchrittErgebnis> result = [];
            if (figur == null)
                return result;
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var schritt = PrüfeSchritt(figur, richtung);
                if (schritt.HasErrors == false)
                    result.Add(richtung, schritt);
            }
            return result;
        }

        #endregion

        #region Bewegungspunkte einer Figur

        /// <summary>
        /// Die maximalen Bewegungspunkte einer Spielfigur nach Regelwerk Kapitel 1.
        /// Für bereits bestehende Figuren steht der Wert in der Zugdatenbank (bp_max); diese
        /// Berechnung wird benötigt, wenn eine Figur neu gerüstet wird oder ihren Typ wechselt
        /// (Aufsitzen, Absitzen, Katapulte aufnehmen oder abgeben).
        /// </summary>
        public static int BerechneBewegungspunkte(Spielfigur? figur) {
            if (figur == null)
                return 0;
            // Ein Landheer, das transportiert, hat nur noch 9 BP (Regelwerk 4.3)
            if (figur is TruppenSpielfigur truppe && IstTransportierendesLandheer(truppe))
                return 9;
            return BerechneBewegungspunkte(figur.Typ);
        }

        /// <summary>
        /// Die maximalen Bewegungspunkte eines Figurtyps nach Regelwerk Kapitel 1 (Tabelle 5).
        /// </summary>
        public static int BerechneBewegungspunkte(FigurType typ) {
            return typ switch {
                FigurType.Krieger => 9,
                FigurType.Reiter => 21,
                FigurType.Kreatur => 21,
                FigurType.LeichteArtillerie or FigurType.SchwereArtillerie => 9,
                FigurType.BeritteneLeichteArtillerie or FigurType.BeritteneSchwereArtillerie => 9,
                FigurType.Schiff or FigurType.PiratenSchiff => 42,
                FigurType.LeichtesKriegsschiff or FigurType.PiratenLeichtesKriegsschiff => 42,
                FigurType.SchweresKriegsschiff or FigurType.PiratenSchweresKriegsschiff => 42,
                // Der Heerführercharakter hat 21 Bewegungspunkte (Regelwerk 1.1). Zauberer
                // bewegen sich zwar "einzeln wie Reiter", das Regelwerk nennt bei ihnen aber
                // ausdrücklich 42 - und ein Charakterzauberer ist auch ein Zauberer.
                FigurType.Charakter => 21,
                FigurType.Zauberer or FigurType.CharakterZauberer => 42,
                _ => 0,
            };
        }

        /// <summary>
        /// Ein Landheer, welches Ladung mit sich führt, muss diese transportieren und hat somit
        /// nur noch 9 BP zur Verfügung. Ausgenommen ist eine Ladung von weniger als 50.000 GS
        /// (Regelwerk 4.3).
        /// </summary>
        public static bool IstTransportierendesLandheer(TruppenSpielfigur truppe) {
            if (truppe.BaseTyp == FigurType.Schiff)
                return false;
            return truppe.GS + truppe.Kampfeinnahmen >= 50000;
        }

        #endregion
    }
}
