using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für das Ein- und Ausschiffen (Regelwerk 0.3.2, 1.5.2 und 4).
    ///
    /// "Einschiffen eines Heeres kostet keine Bewegungspunkte (sondern nur Höhenstufenpunkte). Das
    /// Ausschiffen eines Heeres kosten Bewegungspunkte entsprechend des zu betretenden Kleinfeldes
    /// und Höhenstufenpunkte."
    ///
    /// "Die Transportkapazität eines Schiffes beträgt 100 Raumpunkte. ... Ein Schiff kann nur
    /// reichseigene Rüstgüter transportieren."
    ///
    /// Eine Kaianlage "ermöglicht einem Heer die Überquerung einer weiteren Höhenstufe und wirkt
    /// dadurch wie eine Straße", das Einschiffen kostet darüber also nur einen Höhenstufenpunkt.
    /// </summary>
    public static class SchifffahrtsRules {

        /// <summary>
        /// Was ein einzelnes Schiff an Rüstgütern fassen kann (Regelwerk 1.6).
        ///
        /// Kriegsschiffe tragen nichts: sie sind "speziell für den Fernkampf ausgerüstet" und haben
        /// laut Regelwerk eine Transportkapazität von 0.
        /// </summary>
        public const int TransportkapazitätProSchiff = 100;

        /// <summary>
        /// Was das Einschiffen an Höhenstufenpunkten kostet - über eine Kaianlage weniger
        /// </summary>
        public static int GetHöhenstufenKosten(bool überKai)
            => überKai ? BewegungsRules.HöhenstufenKostenMitStraße : BewegungsRules.HöhenstufenKostenOhneStraße;

        /// <summary>
        /// Wieviele Raumpunkte diese Flotte tragen kann.
        ///
        /// Gezählt werden nur die gewöhnlichen Schiffe; die Katapultplätze einer Flotte stehen für
        /// Kriegsschiffe, die keine Ladekapazität haben.
        /// </summary>
        public static int GetTransportkapazität(TruppenSpielfigur? flotte) {
            if (flotte == null || flotte.BaseTyp != FigurType.Schiff)
                return 0;
            return flotte.staerke * TransportkapazitätProSchiff;
        }

        /// <summary>
        /// Die Truppen, die auf dieser Flotte sind. Sie tragen deren Nummer in auf_Flotte.
        /// </summary>
        public static List<TruppenSpielfigur> GetLadung(TruppenSpielfigur? flotte) {
            if (flotte == null)
                return [];
            string nummer = flotte.Nummer.ToString();
            return SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>()
                .Where(truppe => truppe.BaseTyp != FigurType.Schiff && truppe.auf_Flotte == nummer)
                .ToList();
        }

        /// <summary>
        /// Wieviele Raumpunkte auf der Flotte noch frei sind
        /// </summary>
        public static int GetFreieKapazität(TruppenSpielfigur? flotte) {
            if (flotte == null)
                return 0;
            int belegt = GetLadung(flotte).Sum(truppe => SpielfigurRules.BerechneRaumpunkte(truppe));
            return GetTransportkapazität(flotte) - belegt;
        }

        /// <summary>
        /// Die Flottenreferenz des Reiches aus der Tabelle Reich_crossref.
        ///
        /// Eine eingeschiffte Truppe steht nicht mehr auf der Karte, sondern auf dem Feld
        /// (Flottenreferenz / Nummer der Flotte). Die Altanwendung baut ihre Flottenliste beim
        /// Laden genau daraus wieder auf, deshalb bleibt die Schreibweise unverändert.
        /// </summary>
        /// <returns>der Flottenkey, oder null wenn es dafür keinen Eintrag gibt</returns>
        public static int? GetFlottenreferenz(Nation? reich) {
            if (reich == null || SharedData.Diplomatie == null)
                return null;
            var eintrag = SharedData.Diplomatie.Values
                .FirstOrDefault(d => d.ReferenzNation == reich && d.Nation == ProgramView.SelectedNation);
            return eintrag?.Flottenkey;
        }

        /// <summary>
        /// Steht die Truppe auf einer Flotte?
        /// </summary>
        public static bool IstEingeschifft(TruppenSpielfigur? truppe)
            => truppe != null && truppe.BaseTyp != FigurType.Schiff && string.IsNullOrEmpty(truppe.auf_Flotte) == false;

        /// <summary>
        /// Prüft, ob eine Truppe auf die angegebene Flotte kann, ohne etwas zu verändern.
        /// </summary>
        /// <param name="höhenstufenKosten">was der Vorgang an Höhenstufenpunkten kostet</param>
        public static Result PrüfeEinschiffen(Spielfigur? figur, TruppenSpielfigur? flotte, out int höhenstufenKosten) {
            höhenstufenKosten = 0;

            if (figur is NamensSpielfigur)
                return Result.Fail($"{figur.Bezeichner} schifft nicht mit dem Heer ein",
                    "Charaktere und Zauberer verfügen über eigene Schiffe (Regelwerk 1.9).");
            if (figur is not TruppenSpielfigur truppe)
                return Result.Fail("Es ist keine Truppe ausgewählt", "Nur Heere und Flotten lassen sich einschiffen.");
            if (truppe.BaseTyp == FigurType.Schiff)
                return Result.Fail($"{truppe.Bezeichner} ist selbst eine Flotte", "Schiffe fahren nicht auf Schiffen.");
            if (flotte == null || flotte.BaseTyp != FigurType.Schiff)
                return Result.Fail("Es ist keine Flotte ausgewählt", "Zum Einschiffen braucht es ein Ziel.");

            if (IstEingeschifft(truppe))
                return Result.Fail($"{truppe.Bezeichner} ist schon eingeschifft",
                    $"Die Truppe befindet sich auf der Flotte {truppe.auf_Flotte} und muss erst ausgeschifft werden.");

            if (string.IsNullOrEmpty(truppe.Chars) == false)
                return Result.Fail($"{truppe.Bezeichner} ist mit einem Charakter verbunden",
                    "Solange ein Charakter bei dem Heer steht, lässt es sich nicht einschiffen.");

            if (flotte.Nation != truppe.Nation)
                return Result.Fail("Die Flotte gehört einem anderen Reich",
                    "Ein Schiff kann nur reichseigene Rüstgüter transportieren (Regelwerk 1.6).");

            var standort = KleinfeldView.GetKleinfeld(truppe);
            var wasser = KleinfeldView.GetKleinfeld(flotte);
            if (standort == null || wasser == null)
                return Result.Fail("Die Felder lassen sich nicht bestimmen",
                    $"{truppe.Bezeichner} oder {flotte.Bezeichner} steht nicht auf der Karte.");

            var richtung = BewegungsRules.GetRichtung(standort, wasser);
            if (richtung == null)
                return Result.Fail("Die Flotte liegt nicht nebenan",
                    $"{flotte.Bezeichner} liegt auf {wasser.Bezeichner} und damit nicht an {standort.Bezeichner} an.");

            int kosten = GetHöhenstufenKosten(BewegungsRules.HatKai(standort, wasser, richtung.Value));
            if (truppe.hoehenstufen + kosten > BewegungsRules.MaxHöhenstufenPunkte)
                return Result.Fail($"{truppe.Bezeichner} hat nicht genug Höhenstufenpunkte",
                    $"Das Einschiffen kostet {kosten} Punkte, verbraucht sind bereits {truppe.hoehenstufen} von "
                    + $"{BewegungsRules.MaxHöhenstufenPunkte}. Eine Kaianlage würde es auf "
                    + $"{BewegungsRules.HöhenstufenKostenMitStraße} verbilligen.");

            int platz = GetFreieKapazität(flotte);
            int braucht = SpielfigurRules.BerechneRaumpunkte(truppe);
            if (braucht > platz)
                return Result.Fail($"Auf {flotte.Bezeichner} ist nicht genug Platz",
                    $"{truppe.Bezeichner} braucht {braucht} Raumpunkte, frei sind noch {platz} von "
                    + $"{GetTransportkapazität(flotte)}. Es hilft nur, vorher abzuspalten.");

            höhenstufenKosten = kosten;
            return Result.Success($"{truppe.Bezeichner} kann auf {flotte.Bezeichner}",
                $"Das kostet {kosten} Höhenstufenpunkte und keine Bewegungspunkte.");
        }

        /// <summary>
        /// Prüft, ob eine eingeschiffte Truppe auf das angegebene Landfeld kann.
        ///
        /// Anders als das Einschiffen kostet das Ausschiffen auch Bewegungspunkte, und zwar die des
        /// betretenen Feldes - deshalb wird dafür die gewöhnliche Schrittprüfung verwendet.
        /// </summary>
        public static Result PrüfeAusschiffen(Spielfigur? figur, TruppenSpielfigur? flotte, KleinfeldPosition? ziel, out SchrittErgebnis? schritt) {
            schritt = null;

            if (figur is not TruppenSpielfigur truppe)
                return Result.Fail("Es ist keine Truppe ausgewählt", "Nur Heere lassen sich ausschiffen.");
            if (IstEingeschifft(truppe) == false)
                return Result.Fail($"{truppe.Bezeichner} ist gar nicht eingeschifft", "Es gibt nichts auszuschiffen.");
            if (flotte == null)
                return Result.Fail("Die Flotte wurde nicht gefunden",
                    $"{truppe.Bezeichner} verweist auf die Flotte {truppe.auf_Flotte}, die sich nicht bestimmen lässt.");

            var wasser = KleinfeldView.GetKleinfeld(flotte);
            var landfeld = KleinfeldView.GetKleinfeld(ziel);
            if (wasser == null || landfeld == null)
                return Result.Fail("Die Felder lassen sich nicht bestimmen", "Flotte oder Zielfeld stehen nicht auf der Karte.");
            if (landfeld.IsWasser)
                return Result.Fail($"{landfeld.Bezeichner} ist Wasser", "Ausgeschifft wird an Land.");

            var richtung = BewegungsRules.GetRichtung(wasser, landfeld);
            if (richtung == null)
                return Result.Fail("Das Zielfeld liegt nicht nebenan",
                    $"{landfeld.Bezeichner} grenzt nicht an {wasser.Bezeichner}, wo die Flotte liegt.");

            // die Truppe steht rechnerisch auf dem Wasserfeld der Flotte und macht von dort einen
            // gewöhnlichen Schritt an Land - mit allem, was dazugehört: Gelände, Wall, Wegerecht
            var geprüft = BewegungsRules.PrüfeSchritt(truppe, wasser, landfeld, richtung.Value,
                truppe.bp, truppe.hoehenstufen);
            if (geprüft.HasErrors)
                return Result.Fail($"{truppe.Bezeichner} kann nicht auf {landfeld.Bezeichner} ausschiffen", geprüft.Message);

            schritt = geprüft;
            return Result.Success($"{truppe.Bezeichner} kann auf {landfeld.Bezeichner} ausschiffen",
                $"Das kostet {truppe.bp - geprüft.BPRest} Bewegungspunkte und lässt {geprüft.BPRest} übrig.");
        }
    }
}
