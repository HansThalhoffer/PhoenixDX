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
        /// <summary>
        /// Verteidigung hinter einer Kaianlage, wenn auf dem Feld kein Rüstort steht
        /// (Regelwerk Anhang 10.1)
        /// </summary>
        HinterKaianlage,
        /// <summary>
        /// Verteidigung, wenn der Angreifer direkt auf das Feld ausschifft
        /// (Regelwerk Anhang 10.1)
        /// </summary>
        GegenAusschiffendenAngreifer,
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
            // Diese beiden stehen nur im Regelwerk (Anhang 10.1), nicht in der Kampftabelle:
            // deren Vorteilsliste endet bei der Nachbarunterstützung. Sie werden deshalb nur
            // gerechnet, wenn der Aufrufer sie ausdrücklich mitgibt - siehe ErgänzeKaianlage.
            Kampfvorteil.HinterKaianlage => 30,
            Kampfvorteil.GegenAusschiffendenAngreifer => 20,
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
        /// Die Baupunkte eines Heeres, soweit sie im Fernkampf getroffen werden können.
        ///
        /// "Werden gebannte und ungebannte Truppen von Fernkampfwaffen beschossen, so werden nur
        /// die ungebannten Truppen getroffen. Die ungebannten Truppen ziehen den gesamten Schaden
        /// auf sich." (Regelwerk 5.1)
        ///
        /// Abgezogen wird deshalb, was die gebannten Köpfe an Baupunkten ausmachen. Gebannt sind
        /// immer Krieger oder Reiter, nicht die Fernkampfwaffen - die bleiben im Ziel.
        ///
        /// Die Kampftabelle kennt die Zahl der gebannten Truppen zwar (Zeile 97), zieht sie aber
        /// nur von der Heeresstärke ab. Hier gilt das Regelwerk: wer gebannt ist, zieht keinen
        /// Beschuss auf sich.
        /// </summary>
        public static double BerechneWirksameBaupunkte(TruppenSpielfigur? truppe, int gebannt) {
            double gesamt = BerechneBaupunkte(truppe);
            if (truppe == null || gebannt <= 0 || gesamt <= 0)
                return gesamt;

            double proKopf = truppe.BaseTyp switch {
                FigurType.Reiter => BaupunkteProReiter,
                FigurType.Schiff => BaupunkteProSchiff,
                _ => BaupunkteProKrieger,
            };
            return Math.Max(0, gesamt - Math.Min(gebannt, truppe.staerke) * proKopf);
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
        /// Wieviel ein Kriegsschiff zur Heeresstärke beiträgt (Kampftabelle D150:
        /// leichte Kriegsschiffe zählen fünffach, schwere zehnfach).
        /// </summary>
        public const int HeeresstärkeProLKS = 5;
        public const int HeeresstärkeProSKS = 10;

        /// <summary>
        /// Ab dem wievielten Heerführer es nur noch einen halben Gutpunkt gibt, und ab wann gar
        /// keinen mehr (Errata 26 zu Regelwerk 1.1.5: "Bei einem Heer können bis zu 200 Heerführer
        /// stehen. Jeder dieser HF bringt 1 GP. Ab dem 101 HF nur noch 0,5 GP. Ab dem 201 HF
        /// bringen diese einem Heer keine weiteren GP.")
        /// </summary>
        public const int HeerführerVollerWert = 100;
        public const int HeerführerObergrenze = 200;

        /// <summary>
        /// Die Gutpunkte, die Heerführer einem Heer bringen.
        ///
        /// Die Kampftabelle rechnet hier schlicht mit der Anzahl (C146). Sie stammt von 2010, die
        /// Staffelung aus Errata 26 von 2014 - deshalb gilt hier die Errata. Bei bis zu 100
        /// Heerführern ist das dasselbe.
        /// </summary>
        public static double BerechneGutpunkteAusHeerführern(int heerführer) {
            if (heerführer <= 0)
                return 0;
            if (heerführer <= HeerführerVollerWert)
                return heerführer;
            int halbe = Math.Min(heerführer, HeerführerObergrenze) - HeerführerVollerWert;
            return HeerführerVollerWert + halbe * 0.5;
        }

        /// <summary>
        /// Die Heeresstärke eines Heeres (Kampftabelle D150 und C150).
        ///
        /// Gezählt werden Krieger, Reiter und Schiffe nach Köpfen, Kriegsschiffe mit ihrem
        /// Vielfachen. Landkatapulte gehen nicht ein. Gebannte Truppen werden abgezogen: sie sind
        /// wehrlos und kämpfen nicht mit (Regelwerk 1.4.3).
        /// </summary>
        public static double BerechneHeeresstärke(TruppenSpielfigur? truppe, int gebannt = 0) {
            if (truppe == null)
                return 0;
            double stärke = truppe.BaseTyp == FigurType.Schiff
                ? truppe.staerke + truppe.LKP * HeeresstärkeProLKS + truppe.SKP * HeeresstärkeProSKS
                : truppe.staerke;
            return Math.Max(0, stärke - Math.Max(0, gebannt));
        }

        /// <summary>
        /// Die Kampfstärke eines Heeres: "Kampfstärke(n) = Heeresstärke(n) * (1+ (GP(a) / 200)"
        /// (Regelwerk 5.5, Kampftabelle C174).
        ///
        /// In die Gutpunkte gehen die Vorteile, die Heerführer, Zauberer und Charaktere ein - und
        /// der Wurf mit dem W20: "Anschliessend würfeln beide Spielparteien mit einem W-20. Die
        /// gewürfelte Zahl wird zu den Gutpunkten hinzu addiert." (Regelwerk 5.5)
        /// </summary>
        public const int Gutpunktteiler = 200;

        public static double BerechneKampfstärke(double heeresstärke, double gutpunkte)
            => heeresstärke * (1 + gutpunkte / Gutpunktteiler);

        /// <summary>
        /// Eine 10:1-Übermacht liegt vor, wenn die eigene Heeresstärke die gegnerische um mehr als
        /// das Zehnfache übersteigt (Kampftabelle C171). Dann wird überrannt, statt zu kämpfen
        /// (Regelwerk 5.4).
        /// </summary>
        public const int ÜbermachtVerhältnis = 10;

        public static bool IstÜbermacht(double eigeneHeeresstärke, double fremdeHeeresstärke)
            => eigeneHeeresstärke / ÜbermachtVerhältnis > fremdeHeeresstärke;

        /// <summary>
        /// Der Faktor, mit dem die Grössenverhältnisse die Verluste verschieben (Regelwerk 5.5,
        /// Kampftabelle C187).
        ///
        /// Hat der Gewinner mehr Truppen, sinken seine Verluste; hat er weniger, steigen sie.
        /// Das Regelwerk gibt die Eckwerte an: -0,2 bei doppelter, -0,3 bei dreifacher und -0,4
        /// bei vierfacher Heeresstärke des Gewinners.
        /// </summary>
        public static double BerechneVerlustfaktor(double heeresstärkeVerlierer, double heeresstärkeGewinner) {
            if (heeresstärkeVerlierer <= 0 || heeresstärkeGewinner <= 0)
                return 0;
            return heeresstärkeVerlierer < heeresstärkeGewinner
                ? (heeresstärkeVerlierer - heeresstärkeGewinner) / 10 / heeresstärkeVerlierer - 0.1
                : (heeresstärkeVerlierer - heeresstärkeGewinner) / 10 / heeresstärkeGewinner + 0.1;
        }

        /// <summary>
        /// Die Verluste des Gewinners vor der Verrechnung der Gutpunkte:
        /// "Verluste(gesamt) = Basisverluste * ( 1 + Faktor)", wobei die Heeresstärke des
        /// Verlierers der Basisverlust ist (Regelwerk 5.5, Kampftabelle C189).
        ///
        /// Negative Verluste gibt es nicht (Regelwerk 5.5).
        /// </summary>
        public static double BerechneGesamtverluste(double heeresstärkeVerlierer, double heeresstärkeGewinner) {
            double faktor = BerechneVerlustfaktor(heeresstärkeVerlierer, heeresstärkeGewinner);
            return Math.Max(0, Math.Truncate(heeresstärkeVerlierer * (1 + faktor)));
        }

        /// <summary>
        /// Der Gutpunktschnitt einer Seite: "GP(schnitt) = (KS(gesamt) / HS(gesamt) - 1) * 100"
        /// (Regelwerk 5.5, Kampftabelle C191).
        /// </summary>
        public static double BerechneGutpunktschnitt(double kampfstärkeGesamt, double heeresstärkeGesamt)
            => heeresstärkeGesamt <= 0 ? 0 : (kampfstärkeGesamt / heeresstärkeGesamt - 1) * 100;

        /// <summary>
        /// Die Gutpunktdifferenz, die die Verluste eines Siegerheeres mindert oder erhöht:
        /// "GPF(diff) = (GP(Sieger)/2 - GP(schnitt)) / 100" (Regelwerk 5.5, Kampftabelle C192).
        ///
        /// Die Halbierung ist kein Versehen: "Gutpunkte wirken zu 50 % auf den Sieg und zu 50 %
        /// auf die Verluste des eigenen Heeres" (Regelwerk 5.5). In die Kampfstärke gehen sie
        /// voll ein, in die Verluste zur Hälfte.
        /// </summary>
        public static double BerechneGutpunktdifferenz(double gutpunkteSieger, double gutpunktschnittVerlierer)
            => (gutpunkteSieger / 2 - gutpunktschnittVerlierer) / 100;

        /// <summary>
        /// Wendet die Gutpunktdifferenz auf die Verluste eines Siegerheeres an (Regelwerk 5.5,
        /// Kampftabelle E200).
        ///
        /// Hat das Heer mehr Gutpunkte als der Schnitt des Verlierers, sinken seine Verluste,
        /// sonst steigen sie.
        /// </summary>
        public static double WendeGutpunktdifferenzAn(double verluste, double gutpunktdifferenz)
            => gutpunktdifferenz >= 0
                ? verluste / (1 + gutpunktdifferenz)
                : verluste * (1 + -1 * gutpunktdifferenz);

        /// <summary>
        /// Verteilt die Gesamtverluste auf die Heere des Gewinners:
        /// "Verluste Einheit(m) = (HS(m) / HS(gesamt)) * Verluste(gesamt)" (Regelwerk 5.5,
        /// Kampftabelle D200), und mindert sie anschliessend je Heer mit dessen Gutpunkten.
        /// </summary>
        /// <param name="gesamtverluste">die Verluste der Seite nach dem Grössenfaktor</param>
        /// <param name="heere">je Heer die Heeresstärke und die Gutpunkte</param>
        /// <param name="gutpunktschnittVerlierer">der Gutpunktschnitt der unterlegenen Seite</param>
        /// <returns>je Heer die Verluste in Heeresstärke, in derselben Reihenfolge</returns>
        public static double[] VerteileVerluste(double gesamtverluste,
                                                IReadOnlyList<(double Heeresstärke, double Gutpunkte)> heere,
                                                double gutpunktschnittVerlierer) {
            var ergebnis = new double[heere.Count];
            double gesamt = heere.Sum(heer => heer.Heeresstärke);
            if (gesamt <= 0 || gesamtverluste <= 0)
                return ergebnis;

            for (int i = 0; i < heere.Count; i++) {
                if (heere[i].Heeresstärke <= 0)
                    continue;
                double anteil = gesamtverluste * heere[i].Heeresstärke / gesamt;
                double differenz = BerechneGutpunktdifferenz(heere[i].Gutpunkte, gutpunktschnittVerlierer);
                ergebnis[i] = Math.Max(0, WendeGutpunktdifferenzAn(anteil, differenz));
            }
            return ergebnis;
        }

        /// <summary>
        /// Steht die Seite in einem Rüstort? Dann trägt der Rüstort den grössten Teil des
        /// Fernkampfschadens.
        /// </summary>
        public static bool VerteidigtRüstort(IEnumerable<Kampfvorteil> vorteile)
            => vorteile.Any(v => Rüstortvorteile.Contains(v));

        /// <summary>
        /// Nimmt den Vorteil der Kaianlage in die Liste auf, wenn er gilt.
        ///
        /// Das Regelwerk (Anhang 10.1) gibt der Verteidigung hinter einer Kaianlage 30 Gutpunkte,
        /// "wenn kein Rüstort auf dem Feld steht". Steht einer, zählt dessen Vorteil - die beiden
        /// addieren sich nicht.
        ///
        /// Achtung: in der Kampftabelle der Spielleitung gibt es diese Zeile nicht; deren
        /// Vorteilsliste endet bei der Nachbarunterstützung. Solange das nicht geklärt ist, rechnet
        /// die Anwendung den Vorteil nicht von sich aus, sondern nur, wo er ausdrücklich
        /// angefordert wird.
        /// </summary>
        /// <param name="vorteile">die bisher ermittelten Vorteile des Verteidigers</param>
        /// <param name="hinterKaianlage">verteidigt er hinter einer Kaianlage?</param>
        public static List<Kampfvorteil> ErgänzeKaianlage(IEnumerable<Kampfvorteil> vorteile, bool hinterKaianlage) {
            List<Kampfvorteil> ergebnis = [.. vorteile];
            if (hinterKaianlage == false || VerteidigtRüstort(ergebnis))
                return ergebnis;
            if (ergebnis.Contains(Kampfvorteil.HinterKaianlage) == false)
                ergebnis.Add(Kampfvorteil.HinterKaianlage);
            return ergebnis;
        }

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
