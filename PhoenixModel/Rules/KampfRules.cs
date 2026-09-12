using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Vorteile, die im Kampf auf die Gutpunkte einer Seite aufgeschlagen werden.
    ///
    /// Die Werte stammen aus der Kampftabelle der Spielleitung (k_tabelle_2010.xlsm, Blatt
    /// "Kampfauswertung", Spalte H). Sie sind dort je Heer dreimal untereinander aufgeführt -
    /// einmal für jedes der bis zu drei Heere einer Seite - und tragen überall dieselben Zahlen.
    ///
    /// Die Altanwendung hat diese Tabelle nur geöffnet und drei Zellen vorbelegt; gerechnet hat
    /// die Spielleitung von Hand. Deshalb ist die Tabelle und nicht die Altanwendung die Vorlage.
    /// </summary>
    public enum Kampfvorteil {
        /// <summary>Geländevorteil für Reiter im Tiefland, Hochland und in der Wüste</summary>
        GeländeReiter,
        /// <summary>Geländevorteil für Krieger im Wald und im Sumpf</summary>
        GeländeKrieger,
        /// <summary>Ein Gardeheer</summary>
        Gardeheer,
        /// <summary>Kampf im eigenen Standardgelände, in dem die Hauptstadt anfangs steht</summary>
        Standardgelände,
        /// <summary>Angriff auf eine Straße hinauf</summary>
        AngriffStraßeHinauf,
        /// <summary>Angriff aus einem Wald heraus</summary>
        AngriffAusWald,
        /// <summary>Angriff in einen Sumpf hinein</summary>
        AngriffInSumpf,
        /// <summary>Angriff in eine Wüste hinein</summary>
        AngriffInWüste,
        /// <summary>Kampf bergab</summary>
        KampfBergab,
        /// <summary>Verteidigung hinter einer Brücke anstatt eines Flusses</summary>
        HinterBrücke,
        /// <summary>Verteidigung hinter einem Fluß</summary>
        HinterFluß,
        /// <summary>Verteidigung hinter einem Wall</summary>
        HinterWall,
        /// <summary>Verteidigung aus einer eigenen Burg heraus</summary>
        AusBurg,
        /// <summary>Verteidigung aus einer eigenen Stadt heraus</summary>
        AusStadt,
        /// <summary>Verteidigung aus einer eigenen Festung heraus</summary>
        AusFestung,
        /// <summary>Verteidigung aus einer eigenen Hauptstadt heraus</summary>
        AusHauptstadt,
        /// <summary>Verteidigung aus einer eigenen Festungshauptstadt heraus</summary>
        AusFestungshauptstadt,
        /// <summary>Verteidigung aus einem fremden Rüstort heraus</summary>
        AusFremdemRüstort,
        /// <summary>Unterstützung durch nicht kämpfende, benachbarte eigene Heere, je Kleinfeld</summary>
        NachbarUnterstützung,
    }

    /// <summary>
    /// Die Regeln für den Kampf (Regelwerk Kapitel 5).
    ///
    /// Die Anwendung rechnet hier, was bisher von Hand in einer Excel-Tabelle gerechnet wurde. Die
    /// Formeln sind aus dem Blatt "Kampfauswertung" der Kampftabelle übernommen; wo das Regelwerk
    /// dasselbe anders beschreibt, ist es im jeweiligen Kommentar vermerkt.
    ///
    /// Der Kern ist eine Umrechnung: alles, was kämpft, wird in Baupunkte umgerechnet, der Schaden
    /// wird in Baupunkten verteilt und am Ende wieder in Einheiten zurückgerechnet. Baupunkte sind
    /// also die gemeinsame Währung des Kampfes.
    /// </summary>
    public static class KampfRules {

        /// <summary>
        /// Die Gutpunkte, die ein Vorteil einbringt (Kampftabelle, Spalte H).
        /// </summary>
        public static int GetGutpunkte(Kampfvorteil vorteil) => vorteil switch {
            Kampfvorteil.GeländeReiter => 140,
            Kampfvorteil.GeländeKrieger => 140,
            Kampfvorteil.Gardeheer => 300,
            Kampfvorteil.Standardgelände => 50,
            Kampfvorteil.AngriffStraßeHinauf => 20,
            Kampfvorteil.AngriffAusWald => 20,
            Kampfvorteil.AngriffInSumpf => 20,
            Kampfvorteil.AngriffInWüste => 20,
            Kampfvorteil.KampfBergab => 20,
            Kampfvorteil.HinterBrücke => 30,
            Kampfvorteil.HinterFluß => 50,
            Kampfvorteil.HinterWall => 50,
            Kampfvorteil.AusBurg => 100,
            Kampfvorteil.AusStadt => 200,
            Kampfvorteil.AusFestung => 300,
            Kampfvorteil.AusHauptstadt => 400,
            Kampfvorteil.AusFestungshauptstadt => 500,
            Kampfvorteil.AusFremdemRüstort => 50,
            Kampfvorteil.NachbarUnterstützung => 10,
            _ => 0,
        };

        /// <summary>
        /// Die Vorteile, die einen Rüstort ausmachen. Steht einer davon, verteilt sich der
        /// Fernkampfschaden anders: 67 Prozent auf den Rüstort, 33 Prozent auf die Truppen
        /// (Kampftabelle C83, D86, D87; Errata 52 zu Regelwerk 5.1).
        /// </summary>
        public static readonly Kampfvorteil[] Rüstortvorteile = [
            Kampfvorteil.AusBurg, Kampfvorteil.AusStadt, Kampfvorteil.AusFestung,
            Kampfvorteil.AusHauptstadt, Kampfvorteil.AusFestungshauptstadt,
        ];

        /// <summary>
        /// Der Anteil des Fernkampfschadens, der auf den Rüstort geht, wenn einer verteidigt wird
        /// </summary>
        public const double AnteilAufRüstort = 0.67;

        /// <summary>
        /// Was eine Einheit in der gemeinsamen Währung des Kampfes wiegt (Kampftabelle B99 bis B107).
        ///
        /// Krieger 0,1 - Reiter 0,2 - Schiffe 10 - Heerführer anteilig - LKS 200 - SKS 200 -
        /// LKP 200 - SKP 400 - Pferde 0,1
        /// </summary>
        public const double BaupunkteProKrieger = 0.1;
        public const double BaupunkteProReiter = 0.2;
        public const double BaupunkteProSchiff = 10;
        public const double BaupunkteProLKS = 200;
        public const double BaupunkteProSKS = 200;
        public const double BaupunkteProLKP = 200;
        public const double BaupunkteProSKP = 400;
        public const double BaupunkteProPferd = 0.1;

        /// <summary>
        /// Ein Gardeheer nimmt nur ein Drittel der Verluste (Kampftabelle D99 und folgende:
        /// bei Garde wird durch 3 geteilt).
        /// </summary>
        public const double GardeTeiler = 3;

        /// <summary>
        /// Rechnet eine Truppe in Baupunkte um.
        ///
        /// Ob die Stärke Krieger oder Reiter sind, sagt die Gattung der Figur - beide stehen in
        /// derselben Spalte. Schiffe zählen als Schiffe, ihre Ladung wird getrennt gerechnet.
        /// </summary>
        public static double BerechneBaupunkte(TruppenSpielfigur? truppe) {
            if (truppe == null)
                return 0;
            double summe = truppe.LKP * BaupunkteProLKP
                         + truppe.SKP * BaupunkteProSKP
                         + truppe.Pferde * BaupunkteProPferd;
            switch (truppe.BaseTyp) {
                case FigurType.Reiter:
                    summe += truppe.staerke * BaupunkteProReiter;
                    break;
                case FigurType.Schiff:
                    // Schiffe führen ihre Geschütze in denselben Spalten wie die Landeinheiten,
                    // dort zählen sie aber als Kriegsschiffe (siehe Fernkampfwaffe)
                    summe = truppe.staerke * BaupunkteProSchiff
                          + truppe.LKP * BaupunkteProLKS
                          + truppe.SKP * BaupunkteProSKS;
                    break;
                default:
                    summe += truppe.staerke * BaupunkteProKrieger;
                    break;
            }
            return summe;
        }

        /// <summary>
        /// Der Schutzfaktor, mit dem Gutpunkte den Schaden mindern: Gutpunkte durch 100, plus eins
        /// (Kampftabelle C85 und E91; Errata 52 zu Regelwerk 5.1: "Jedes Heer wird danach von
        /// seinen eigenen GP nochmals geschützt. Gutpunkte Einheit /100+1").
        ///
        /// 0 Gutpunkte ergeben Faktor 1, der Schaden bleibt also unverändert.
        /// </summary>
        public static double BerechneSchutzfaktor(double gutpunkte) => gutpunkte / 100 + 1;

        /// <summary>
        /// Wieviel Schaden nach dem Schutz durch Gutpunkte übrig bleibt
        /// </summary>
        public static double MindereSchaden(double schaden, double gutpunkte)
            => schaden / BerechneSchutzfaktor(gutpunkte);

        /// <summary>
        /// Verteilt Schaden auf die Heere einer Seite.
        ///
        /// Jedes Heer bekommt den Anteil, den es an den Baupunkten der Seite hält, und wird
        /// anschliessend von seinen eigenen Gutpunkten geschützt (Kampftabelle E91 bis E93).
        /// Heere ohne Baupunkte bekommen nichts ab.
        /// </summary>
        /// <param name="schaden">der Schaden in Baupunkten, den die Seite insgesamt erleidet</param>
        /// <param name="heere">je Heer die Baupunkte und die Gutpunkte</param>
        /// <returns>je Heer der Schaden in Baupunkten, in derselben Reihenfolge</returns>
        public static double[] VerteileSchaden(double schaden, IReadOnlyList<(double Baupunkte, double Gutpunkte)> heere) {
            var ergebnis = new double[heere.Count];
            double gesamt = heere.Sum(heer => heer.Baupunkte);
            if (gesamt <= 0 || schaden <= 0)
                return ergebnis;

            for (int i = 0; i < heere.Count; i++) {
                if (heere[i].Baupunkte <= 0)
                    continue;
                double anteil = schaden * heere[i].Baupunkte / gesamt;
                ergebnis[i] = MindereSchaden(anteil, heere[i].Gutpunkte);
            }
            return ergebnis;
        }

        /// <summary>
        /// Die Verluste eines Heeres, aufgeschlüsselt nach Gattung.
        /// </summary>
        public class Verluste {
            public int Krieger { get; set; }
            public int Reiter { get; set; }
            public int Schiffe { get; set; }
            public int Heerführer { get; set; }
            public int LKS { get; set; }
            public int SKS { get; set; }
            public int LKP { get; set; }
            public int SKP { get; set; }
            public int Pferde { get; set; }

            /// <summary>Die Verluste in Baupunkten, wie sie in die Umrechnung gegangen sind</summary>
            public double Baupunkte { get; set; }

            public override string ToString()
                => $"{Baupunkte:n0} BP: {Krieger} Krieger, {Reiter} Reiter, {Schiffe} Schiffe, "
                 + $"{Heerführer} HF, {LKP} LKP, {SKP} SKP, {LKS} LKS, {SKS} SKS, {Pferde} Pferde";
        }

        /// <summary>
        /// Rechnet den Schaden eines Heeres in verlorene Einheiten zurück.
        ///
        /// Der Schaden verteilt sich innerhalb des Heeres nach dem Baupunktanteil der Gattung, und
        /// jede Gattung wird dann mit ihrem eigenen Faktor in Stück zurückgerechnet
        /// (Kampftabelle C99 bis D107). Ein Gardeheer verliert nur ein Drittel.
        ///
        /// Verloren wird höchstens, was da ist.
        /// </summary>
        /// <param name="truppe">das betroffene Heer</param>
        /// <param name="schadenInBaupunkten">der Schaden, den dieses Heer abbekommt</param>
        public static Verluste BerechneVerluste(TruppenSpielfigur? truppe, double schadenInBaupunkten) {
            var verluste = new Verluste { Baupunkte = Math.Max(0, schadenInBaupunkten) };
            if (truppe == null || schadenInBaupunkten <= 0)
                return verluste;

            double gesamt = BerechneBaupunkte(truppe);
            if (gesamt <= 0)
                return verluste;

            double teiler = truppe.Garde ? GardeTeiler : 1;
            double anteil = Math.Min(1, schadenInBaupunkten / gesamt) / teiler;

            bool istSchiff = truppe.BaseTyp == FigurType.Schiff;
            if (istSchiff) {
                verluste.Schiffe = Anteilig(truppe.staerke, anteil);
                verluste.LKS = Anteilig(truppe.LKP, anteil);
                verluste.SKS = Anteilig(truppe.SKP, anteil);
            }
            else {
                if (truppe.BaseTyp == FigurType.Reiter)
                    verluste.Reiter = Anteilig(truppe.staerke, anteil);
                else
                    verluste.Krieger = Anteilig(truppe.staerke, anteil);
                verluste.LKP = Anteilig(truppe.LKP, anteil);
                verluste.SKP = Anteilig(truppe.SKP, anteil);
                verluste.Pferde = Anteilig(truppe.Pferde, anteil);
            }
            verluste.Heerführer = Anteilig(truppe.hf, anteil);
            return verluste;
        }

        /// <summary>
        /// Der Anteil einer Gattung, kaufmännisch gerundet und nie mehr als vorhanden.
        ///
        /// Die Kampftabelle rundet die Nachkommastelle über einen Prozentwurf auf oder ab
        /// (Spalten F bis I, "% Wurf"). Solange nicht gewürfelt wird, ist kaufmännisches Runden
        /// die Entsprechung ohne Zufall; <see cref="RundeMitWurf"/> bildet den Wurf nach.
        /// </summary>
        private static int Anteilig(int vorhanden, double anteil)
            => vorhanden <= 0 ? 0 : Math.Min(vorhanden, (int)Math.Round(vorhanden * anteil, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Der Prozentwurf der Kampftabelle: die Nachkommastelle eines Verlustes entscheidet per
        /// Wurf, ob aufgerundet wird (Kampftabelle G103 und I103).
        ///
        /// "TRUNC(rest*100) - (100-wurf) > 0" heisst: liegt der gewürfelte Prozentwert unter dem
        /// Nachkommaanteil, kostet es ein weiteres Stück.
        /// </summary>
        /// <param name="verlust">der ungerundete Verlust</param>
        /// <param name="wurf">das Ergebnis eines Prozentwurfs, 1 bis 100</param>
        public static int RundeMitWurf(double verlust, int wurf) {
            if (verlust <= 0)
                return 0;
            int ganz = (int)Math.Truncate(verlust);
            // Abweichung von der Tabelle, bewusst: 3,40 minus 3 ergibt in Binärarithmetik
            // 0,3999999999999999, mal 100 abgeschnitten also 39 statt 40. Excel rechnet genauso
            // falsch, nur hängt daran ein ganzer Krieger. Die Winzigkeit hebt das Rauschen auf,
            // ohne an der Regel etwas zu ändern - sie wirkt nur dort, wo der Rest rechnerisch
            // ohnehin auf einem vollen Prozent liegt.
            int prozent = (int)Math.Truncate((verlust - ganz) * 100 + 1e-9);
            return prozent - (100 - wurf) > 0 ? ganz + 1 : ganz;
        }

        /// <summary>
        /// Bringt das Gelände einem Heer einen Vorteil?
        ///
        /// Reiter sind im Tiefland, Hochland und in der Wüste im Vorteil, Krieger im Wald und im
        /// Sumpf (Kampftabelle Zeilen 26 und 27).
        /// </summary>
        public static Kampfvorteil? GetGeländevorteil(FigurType gattung, TerrainType gelände) {
            bool reiterGelände = gelände is TerrainType.Tiefland or TerrainType.Hochland or TerrainType.Wüste;
            bool kriegerGelände = gelände is TerrainType.Wald or TerrainType.Sumpf;

            if (gattung == FigurType.Reiter && reiterGelände)
                return Kampfvorteil.GeländeReiter;
            if (gattung == FigurType.Krieger && kriegerGelände)
                return Kampfvorteil.GeländeKrieger;
            return null;
        }

        /// <summary>
        /// Die Summe der Gutpunkte aus einer Liste von Vorteilen.
        ///
        /// Die Nachbarunterstützung zählt je Kleinfeld, deshalb die Anzahl (Kampftabelle J80:
        /// der Wert wird mit der Zahl der unterstützenden Felder multipliziert).
        /// </summary>
        public static int BerechneVorteile(IEnumerable<Kampfvorteil> vorteile, int unterstützendeFelder = 0) {
            int summe = 0;
            foreach (var vorteil in vorteile) {
                summe += vorteil == Kampfvorteil.NachbarUnterstützung
                    ? GetGutpunkte(vorteil) * unterstützendeFelder
                    : GetGutpunkte(vorteil);
            }
            return summe;
        }

        /// <summary>
        /// Steht die Seite in einem Rüstort? Dann trägt der Rüstort den grössten Teil des
        /// Fernkampfschadens.
        /// </summary>
        public static bool VerteidigtRüstort(IEnumerable<Kampfvorteil> vorteile)
            => vorteile.Any(v => Rüstortvorteile.Contains(v));

        /// <summary>
        /// Teilt den Fernkampfschaden zwischen Rüstort und Truppen auf.
        /// </summary>
        /// <returns>der Anteil auf den Rüstort und der Anteil auf die Truppen</returns>
        public static (double AufRüstort, double AufTruppen) TeileFernkampfschaden(double schaden, bool rüstort) {
            if (rüstort == false)
                return (0, schaden);
            double aufOrt = schaden * AnteilAufRüstort;
            return (aufOrt, schaden - aufOrt);
        }
    }
}
