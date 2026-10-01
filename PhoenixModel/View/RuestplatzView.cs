using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Alles, was eine Rüstoberfläche anzeigen und ausrechnen muss.
    ///
    /// Die Regeln des Rüstens stehen in <see cref="RuestRules"/>; hier steht nur, was daraus auf
    /// den Bildschirm gehört und wie aus Stückzahlen ein Befehl wird. Gerüstet wird dann über den
    /// Befehlsweg - <see cref="BaueBefehl"/> liefert den Text, den auch die Befehlseingabe
    /// versteht. Das ist Absicht: so landet die Rüstung unter "Aktueller Zug" und lässt sich
    /// zurücknehmen, statt an den Befehlen vorbei in die Tabelle geschrieben zu werden.
    ///
    /// Das Vorbild ist die "Rüstungskammer" der Altanwendung: links die eigenen Rüstorte, oben die
    /// Kapazität mit ihrem Verbrauch, daneben der Reichsschatz vorher und nachher, darunter die
    /// Stückzahlen je Rüstgut.
    /// </summary>
    public static class RuestplatzView {

        /// <summary>
        /// Ein Rüstgut, wie es die Oberfläche anbietet.
        /// </summary>
        /// <param name="Art">der Typ, wie ihn Befehl und Rüstungstabelle führen</param>
        /// <param name="Name">die Beschriftung, zugleich das Wort im Befehl</param>
        /// <param name="Preis">die Kosten je Stück in Goldstücken</param>
        /// <param name="BindetHeerführer">zählt gegen die Heerführerkapazität des Rüstorts</param>
        /// <param name="BindetZauberer">zählt gegen die Zaubererkapazität</param>
        public record class Rüstgut(ConstructionElementType Art, string Name, int Preis,
                                    bool BindetHeerführer = false, bool BindetZauberer = false);

        /// <summary>
        /// Die beweglichen Rüstgüter in der Reihenfolge, in der sie die Altanwendung zeigt.
        ///
        /// Bauwerke stehen nicht dabei - die werden nicht gerüstet, sondern gebaut
        /// (<see cref="ConstructCommand"/>), und dafür gibt es das Baumenü auf der Karte.
        /// </summary>
        public static readonly IReadOnlyList<ConstructionElementType> Rüstgutarten = [
            ConstructionElementType.K,
            ConstructionElementType.R,
            ConstructionElementType.P,
            ConstructionElementType.HF,
            ConstructionElementType.LKP,
            ConstructionElementType.SKP,
            ConstructionElementType.S,
            ConstructionElementType.LKS,
            ConstructionElementType.SKS,
            ConstructionElementType.ZA,
            ConstructionElementType.ZB,
        ];

        /// <summary>
        /// Die Rüstgüter mit ihren aktuellen Preisen aus der Kostentabelle.
        /// </summary>
        public static List<Rüstgut> GetRüstgüter() {
            List<Rüstgut> güter = [];
            foreach (var art in Rüstgutarten) {
                güter.Add(new Rüstgut(
                    art,
                    SimpleParser.ConstructionElementTypeToString(art).Trim(),
                    KostenView.GetGSKosten(art),
                    art == ConstructionElementType.HF,
                    art == ConstructionElementType.ZA || art == ConstructionElementType.ZB));
            }
            return güter;
        }

        /// <summary>
        /// Die eigenen Gemarken, auf denen ein Rüstort steht, nach ihrer Bezeichnung sortiert.
        ///
        /// Ein Dorf ist keiner: seine Rüstkapazität ist null, und genau daran wird es erkannt -
        /// dieselbe Prüfung, die auch <see cref="RuestRules.PrüfeRüstort"/> anstellt.
        /// </summary>
        public static List<KleinFeld> FindeRüstorte() {
            List<KleinFeld> orte = [];
            if (SharedData.Map == null)
                return orte;

            foreach (var gemark in SharedData.Map.Values) {
                if (gemark.Nation == null || gemark.Nation != ProgramView.SelectedNation)
                    continue;
                if (RuestRules.GetKapazität(gemark).Goldstücke <= 0)
                    continue;
                orte.Add(gemark);
            }
            orte.Sort((eines, anderes) => string.Compare(eines.Bezeichner, anderes.Bezeichner,
                StringComparison.OrdinalIgnoreCase));
            return orte;
        }

        /// <summary>
        /// Der Kopf der Rüstoberfläche zu einem Rüstort.
        /// </summary>
        /// <param name="Gemark">die Gemark, oder null wenn keine gewählt ist</param>
        /// <param name="Rüstortname">Burg, Stadt, Festung, Hauptstadt - oder leer</param>
        /// <param name="Kapazität">was dieser Rüstort in diesem Monat hergibt</param>
        /// <param name="Vergeben">was davon in diesem Zug schon vergeben ist</param>
        /// <param name="ReichsschatzAlt">der Stand zu Zugbeginn</param>
        /// <param name="VerrüstetGesamt">was im ganzen Reich in diesem Zug schon ausgegeben wurde</param>
        /// <param name="BesondereEinnahmenHier">Kampfeinnahmen der Heere auf dieser Gemark</param>
        /// <param name="Belagerung">die Belagerung, die auf dem Ort liegt</param>
        public record class Rüstübersicht(
                KleinFeld? Gemark,
                string Rüstortname,
                RuestRules.Rüstkapazität Kapazität,
                RuestRules.Rüstkapazität Vergeben,
                int ReichsschatzAlt,
                int VerrüstetGesamt,
                int BesondereEinnahmenHier,
                BelagerungsRules.Belagerung? Belagerung) {

            /// <summary>Was an diesem Ort noch frei ist - nie unter null</summary>
            public RuestRules.Rüstkapazität Frei => new(
                Math.Max(0, Kapazität.Goldstücke - Vergeben.Goldstücke),
                Math.Max(0, Kapazität.Heerführer - Vergeben.Heerführer),
                Math.Max(0, Kapazität.Zauberer - Vergeben.Zauberer));

            /// <summary>Was vom Reichsschatz nach allem bisher Gerüsteten übrig bleibt</summary>
            public int ReichsschatzNeu => ReichsschatzAlt - VerrüstetGesamt;

            /// <summary>Steht hier überhaupt ein Rüstort?</summary>
            public bool IstRüstort => Kapazität.Goldstücke > 0;

            public static readonly Rüstübersicht Keine = new(null, string.Empty,
                RuestRules.Rüstkapazität.Keine, RuestRules.Rüstkapazität.Keine, 0, 0, 0, null);
        }

        /// <summary>
        /// Stellt zusammen, was über einem Rüstort steht.
        /// </summary>
        /// <param name="ausBesonderenEinnahmen">
        /// true, wenn aus Kampfeinnahmen gerüstet wird: dann halbiert sich die Kapazität, dafür
        /// geht es in jedem Monat (Regelwerk 3.3.1).
        /// </param>
        public static Rüstübersicht GetÜbersicht(KleinFeld? gemark, bool ausBesonderenEinnahmen = false) {
            if (gemark == null)
                return Rüstübersicht.Keine;

            var rüstort = BauwerkeView.GetRüstortNachKarte(gemark);
            return new Rüstübersicht(
                gemark,
                rüstort?.Ruestort ?? string.Empty,
                RuestRules.GetKapazität(gemark, ausBesonderenEinnahmen),
                RuestRules.BerechneBereitsGerüstet(gemark),
                SchatzkammerView.MoneyToSpendThisTurn(),
                SchatzkammerRules.BerechneVerrüstet(),
                SchatzkammerView.GetMoneyOnKleinfeld(gemark),
                RuestRules.GetBelagerung(gemark));
        }

        /// <summary>
        /// Ein Posten des Rüstkorbes: ein Rüstgut und wieviel davon.
        /// </summary>
        public record class Posten(ConstructionElementType Art, int Anzahl);

        /// <summary>
        /// Was ein Korb kostet und wieviel er an Heerführern und Zauberern bindet.
        ///
        /// Gerechnet wird über denselben Weg wie beim Prüfen: aus den Posten wird ein
        /// Rüstungsauftrag, und den bewertet <see cref="RuestRules.BerechneUmfang"/>. Zwei
        /// getrennte Rechnungen für dieselbe Frage laufen sonst irgendwann auseinander.
        /// </summary>
        public static RuestRules.Rüstkapazität BerechneKorb(IEnumerable<Posten>? korb)
            => RuestRules.BerechneUmfang(BaueAuftrag(korb, null));

        /// <summary>
        /// Baut aus den Posten einen Rüstungsauftrag, wie ihn die Zugdatenbank führt.
        /// Posten ohne Stückzahl fallen weg.
        /// </summary>
        public static Ruestung? BaueAuftrag(IEnumerable<Posten>? korb, KleinfeldPosition? ort) {
            if (korb == null)
                return null;
            var auftrag = new Ruestung {
                gf = ort?.gf ?? 0,
                kf = ort?.kf ?? 0,
                ZugMonat = ProgramView.SelectedMonth,
                Name_x = string.Empty,
                Beschriftung = string.Empty,
            };
            foreach (var posten in korb) {
                if (posten.Anzahl <= 0)
                    continue;
                switch (posten.Art) {
                    case ConstructionElementType.K: auftrag.K += posten.Anzahl; break;
                    case ConstructionElementType.R: auftrag.R += posten.Anzahl; break;
                    case ConstructionElementType.P: auftrag.P += posten.Anzahl; break;
                    case ConstructionElementType.S: auftrag.S += posten.Anzahl; break;
                    case ConstructionElementType.LKP: auftrag.LKP += posten.Anzahl; break;
                    case ConstructionElementType.SKP: auftrag.SKP += posten.Anzahl; break;
                    case ConstructionElementType.LKS: auftrag.LKS += posten.Anzahl; break;
                    case ConstructionElementType.SKS: auftrag.SKS += posten.Anzahl; break;
                    case ConstructionElementType.HF: auftrag.HF += posten.Anzahl; break;
                    case ConstructionElementType.ZA: auftrag.Z += posten.Anzahl; break;
                    case ConstructionElementType.ZB: auftrag.ZB += posten.Anzahl; break;
                    default: continue;
                }
            }
            return auftrag;
        }

        /// <summary>
        /// Der Befehl zu einem Korb - derselbe Text, den auch die Befehlseingabe versteht.
        ///
        /// "Rüste 1000 Krieger und 3 Heerführer in Rüstort 506/17"
        /// "Rüste 6 Leichte Katapulte in Rüstort 506/17 zu Krieger 115"
        /// </summary>
        /// <param name="zielart">die Einheit, zu der gerüstet wird, oder None für ein neues Heer</param>
        /// <param name="zielnummer">deren Nummer</param>
        /// <returns>der Befehl, oder ein leerer Text wenn nichts zu rüsten ist</returns>
        public static string BaueBefehl(IEnumerable<Posten>? korb, KleinfeldPosition? ort,
                ConstructionElementType zielart = ConstructionElementType.None, int? zielnummer = null) {
            if (korb == null || ort == null)
                return string.Empty;

            var teile = korb
                .Where(posten => posten.Anzahl > 0 && posten.Art != ConstructionElementType.None)
                .Select(posten => $"{posten.Anzahl} {SimpleParser.ConstructionElementTypeToString(posten.Art).Trim()}")
                .ToList();
            if (teile.Count == 0)
                return string.Empty;

            string befehl = $"Rüste {string.Join(" und ", teile)} in Rüstort {ort.CreateBezeichner()}";
            if (zielart != ConstructionElementType.None && zielnummer != null)
                befehl = $"Rüste {string.Join(" und ", teile)} in Rüstort {ort.CreateBezeichner()} "
                       + $"zu {SimpleParser.ConstructionElementTypeToString(zielart).Trim()} {zielnummer}";
            return befehl;
        }

        /// <summary>
        /// Prüft einen Korb, ohne etwas zu verändern - für die laufende Rückmeldung in der
        /// Oberfläche, bevor jemand auf "Rüsten" drückt.
        /// </summary>
        public static Result Prüfe(KleinFeld? gemark, IEnumerable<Posten>? korb,
                bool ausBesonderenEinnahmen = false) {
            var auftrag = BaueAuftrag(korb, gemark);
            if (auftrag == null || BerechneKorb(korb).Goldstücke <= 0 && AnzahlStücke(korb) <= 0)
                return Result.Fail("Es ist nichts zum Rüsten angegeben",
                    "Trage ein, wieviel von welchem Rüstgut gerüstet werden soll.");
            return RuestRules.Prüfe(gemark, auftrag, ausBesonderenEinnahmen);
        }

        /// <summary>
        /// Wieviele Stücke insgesamt im Korb liegen - auch Güter, die nichts kosten, sind etwas.
        /// </summary>
        public static int AnzahlStücke(IEnumerable<Posten>? korb)
            => korb == null ? 0 : korb.Where(posten => posten.Anzahl > 0).Sum(posten => posten.Anzahl);

        /// <summary>
        /// Die Rüstungen, die in diesem Zug auf diesem Ort schon eingestellt sind.
        ///
        /// Besondere Rüstungen der Spielleitung sind dabei; sie kosten nichts und zählen nicht
        /// gegen die Kapazität, stehen dem Spieler aber zu Recht vor Augen.
        /// </summary>
        public static List<Ruestung> GetEingestellte(KleinfeldPosition? ort) {
            List<Ruestung> eingestellt = [];
            if (ort == null || SharedData.Ruestung == null)
                return eingestellt;
            foreach (var rüstung in SharedData.Ruestung)
                if (rüstung.gf == ort.gf && rüstung.kf == ort.kf)
                    eingestellt.Add(rüstung);
            return eingestellt;
        }

        /// <summary>
        /// Eine Zeile, wie sie unter "In diesem Rüstort gerüstet" steht.
        /// </summary>
        public static string Beschreibe(Ruestung? rüstung) {
            if (rüstung == null)
                return string.Empty;
            List<string> teile = [];
            void Nimm(int anzahl, ConstructionElementType art) {
                if (anzahl > 0)
                    teile.Add($"{anzahl} {SimpleParser.ConstructionElementTypeToString(art).Trim()}");
            }
            Nimm(rüstung.K, ConstructionElementType.K);
            Nimm(rüstung.R, ConstructionElementType.R);
            Nimm(rüstung.P, ConstructionElementType.P);
            Nimm(rüstung.HF, ConstructionElementType.HF);
            Nimm(rüstung.LKP, ConstructionElementType.LKP);
            Nimm(rüstung.SKP, ConstructionElementType.SKP);
            Nimm(rüstung.S, ConstructionElementType.S);
            Nimm(rüstung.LKS, ConstructionElementType.LKS);
            Nimm(rüstung.SKS, ConstructionElementType.SKS);
            Nimm(rüstung.Z, ConstructionElementType.ZA);
            Nimm(rüstung.ZB, ConstructionElementType.ZB);

            if (teile.Count == 0)
                return "(leer)";
            string text = string.Join(", ", teile);
            if (rüstung.Nummer > 0)
                text += $" zu Einheit {rüstung.Nummer}";
            if (rüstung.besRuestung != 0)
                text += " (besondere Rüstung der Spielleitung)";
            return text;
        }
    }
}
