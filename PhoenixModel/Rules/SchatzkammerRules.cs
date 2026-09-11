using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln rund um den Reichsschatz.
    ///
    /// Die Tabelle Schatzkammer der Zugdatenbank ist eine Historie mit einer Zeile je Monat. Der
    /// Reichsschatz eines Monats ist das Ergebnis des Vormonats: was am Anfang da war, plus
    /// Geländeeinnahmen und erhaltene Schenkungen, minus getätigte Schenkungen und alles Verrüstete.
    ///
    /// Die Formel ist aus SchatzkammerHelper.Item.Bilanz der Altanwendung übernommen und gegen die
    /// echte Historie geprüft, siehe SchatzkammerIntegrationTest.
    /// </summary>
    public static class SchatzkammerRules {

        /// <summary>
        /// Ein Baupunkt an einem Rüstort kostet 50 GS, egal ob repariert oder aufgewertet wird.
        /// Für Bauwerke steht der Betrag dagegen als Spalte in der Tabelle.
        /// </summary>
        public const int KostenProBaupunkt = 50;

        /// <summary>
        /// Das Ergebnis eines Monats und damit der Reichsschatz zu Beginn des Folgemonats.
        ///
        /// Übernommen aus SchatzkammerHelper.Item.Bilanz der Altanwendung:
        /// Reichschatz + einahmen_land + schenkung_bekommen - schenkung_getaetigt - verruestet
        /// </summary>
        public static int BerechneBilanz(Schatzkammer? monat) {
            if (monat == null)
                return 0;
            return monat.Reichschatz
                 + monat.Einahmen_land
                 + monat.schenkung_bekommen
                 - monat.schenkung_getaetigt
                 - monat.Verruestet;
        }

        /// <summary>
        /// Der Stand der Schatzkammer für den angegebenen Monat, oder null wenn es dafür keine
        /// Zeile gibt.
        /// </summary>
        public static Schatzkammer? GetMonat(int zug) {
            if (SharedData.Schatzkammer == null)
                return null;
            return SharedData.Schatzkammer.FirstOrDefault(k => k.monat == zug);
        }

        /// <summary>
        /// Alle Monate der Historie, aufsteigend sortiert
        /// </summary>
        public static List<Schatzkammer> GetHistorie() {
            if (SharedData.Schatzkammer == null)
                return [];
            return SharedData.Schatzkammer.OrderBy(k => k.monat).ToList();
        }

        /// <summary>
        /// Was eine Zeile der Rüstungstabelle den Reichsschatz gekostet hat.
        ///
        /// Die Kosten stehen nicht in der Rüstungstabelle, sondern ergeben sich aus der Stückzahl
        /// und der Kostentabelle - genau wie in RuestungsItem.kosten der Altanwendung.
        /// </summary>
        public static int BerechneEinheitenkosten(Ruestung rüstung) {
            return rüstung.K * KostenView.GetGSKosten(ConstructionElementType.K)
                 + rüstung.R * KostenView.GetGSKosten(ConstructionElementType.R)
                 + rüstung.P * KostenView.GetGSKosten(ConstructionElementType.P)
                 + rüstung.S * KostenView.GetGSKosten(ConstructionElementType.S)
                 + rüstung.LKP * KostenView.GetGSKosten(ConstructionElementType.LKP)
                 + rüstung.SKP * KostenView.GetGSKosten(ConstructionElementType.SKP)
                 + rüstung.LKS * KostenView.GetGSKosten(ConstructionElementType.LKS)
                 + rüstung.SKS * KostenView.GetGSKosten(ConstructionElementType.SKS)
                 + rüstung.Z * KostenView.GetGSKosten(ConstructionElementType.ZA)
                 + rüstung.ZB * KostenView.GetGSKosten(ConstructionElementType.ZB)
                 + rüstung.HF * KostenView.GetGSKosten(ConstructionElementType.HF);
        }

        /// <summary>
        /// Alles, was in diesem Zug gerüstet wurde: Einheiten, Bauwerke und Rüstorte.
        ///
        /// Besondere Rüstungen (besRuestung ungleich 0) sind Zuteilungen der Spielleitung und
        /// kosten den Reichsschatz nichts.
        ///
        /// Die drei Rüstungstabellen enthalten immer nur den laufenden Zug, weil sie bei der
        /// Zugabgabe geleert werden.
        /// </summary>
        public static int BerechneVerrüstet() {
            int summe = 0;

            if (SharedData.Ruestung != null)
                foreach (var rüstung in SharedData.Ruestung)
                    if (rüstung.besRuestung == 0)
                        summe += BerechneEinheitenkosten(rüstung);

            if (SharedData.RuestungBauwerke != null)
                foreach (var bauwerk in SharedData.RuestungBauwerke)
                    summe += bauwerk.Kosten > 0
                        ? bauwerk.Kosten
                        : (bauwerk.BP_neu + bauwerk.BP_rep) * KostenProBaupunkt;

            if (SharedData.RuestungRuestorte != null)
                foreach (var rüstort in SharedData.RuestungRuestorte)
                    summe += (rüstort.BP_up + rüstort.BP_rep) * KostenProBaupunkt;

            return summe;
        }

        /// <summary>
        /// Was das Reich in dem angegebenen Monat geschenkt bekommen hat
        /// </summary>
        public static int BerechneSchenkungenBekommen(int zug) {
            if (SharedData.Schenkungen == null)
                return 0;
            return SharedData.Schenkungen.Where(s => s.monat == zug).Sum(s => s.Schenkung_bekommen);
        }

        /// <summary>
        /// Was das Reich in dem angegebenen Monat verschenkt hat
        /// </summary>
        public static int BerechneSchenkungenGetätigt(int zug) {
            if (SharedData.Schenkungen == null)
                return 0;
            return SharedData.Schenkungen.Where(s => s.monat == zug).Sum(s => s.Schenkung_an);
        }

        /// <summary>
        /// Das Gold, das die eigenen Truppen mit sich führen. Es liegt nicht im Reichsschatz,
        /// wird aber mitgeführt, damit die Summe nachvollziehbar bleibt.
        /// </summary>
        public static int BerechneGoldBeiTruppen() {
            int summe = 0;
            foreach (var figur in SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation))
                if (figur is TruppenSpielfigur truppe)
                    summe += truppe.GS;
            return summe;
        }

        /// <summary>
        /// Rechnet die Zeile des laufenden Monats aus dem aktuellen Spielstand neu aus.
        ///
        /// Der Reichsschatz selbst bleibt unangetastet - der ist das Ergebnis des Vormonats und
        /// steht fest. Neu berechnet werden nur die Bewegungen innerhalb des Monats.
        /// </summary>
        /// <returns>die aktualisierte Zeile oder null, wenn es für den Monat keine gibt</returns>
        public static Schatzkammer? Zwischenbilanz(int zug) {
            var monat = GetMonat(zug);
            if (monat == null)
                return null;

            monat.Einahmen_land = new Zugmonat(zug).IstEinnahmemonat && ProgramView.SelectedNation != null
                ? EinnahmenView.GetReichEinnahmen(ProgramView.SelectedNation)
                : 0;
            monat.schenkung_bekommen = BerechneSchenkungenBekommen(zug);
            monat.schenkung_getaetigt = BerechneSchenkungenGetätigt(zug);
            monat.Verruestet = BerechneVerrüstet();
            monat.GS_bei_truppen = BerechneGoldBeiTruppen();
            return monat;
        }

        /// <summary>
        /// Legt die Zeile für den Folgemonat an beziehungsweise füllt sie neu.
        ///
        /// Der Reichsschatz des Folgemonats ist die Bilanz des laufenden Monats. Alle Bewegungen
        /// starten bei null, denn im Folgemonat ist noch nichts passiert. Die Landeinnahmen werden
        /// erst am Ende des Folgemonats eingetrieben und deshalb hier noch nicht gesetzt.
        /// </summary>
        /// <returns>die Zeile des Folgemonats, oder null wenn der laufende Monat fehlt</returns>
        /// <remarks>
        /// Eine neu angelegte Zeile wird nicht in <see cref="SharedData.Schatzkammer"/> aufgenommen -
        /// die Sammlung ist nach dem Laden für Ergänzungen geschlossen. Der Aufrufer schreibt die
        /// Zeile in die Datenbank des Folgezuges; beim nächsten Laden ist sie dann dabei.
        /// </remarks>
        public static Schatzkammer? BereiteFolgemonatVor(int zug) {
            var laufend = Zwischenbilanz(zug);
            if (laufend == null)
                return null;

            var folge = GetMonat(zug + 1) ?? new Schatzkammer { monat = zug + 1 };

            folge.Reichschatz = BerechneBilanz(laufend);
            folge.Einahmen_land = 0;
            folge.schenkung_bekommen = 0;
            folge.schenkung_getaetigt = 0;
            folge.Verruestet = 0;
            folge.GS_bei_truppen = laufend.GS_bei_truppen;

            return folge;
        }
    }
}
