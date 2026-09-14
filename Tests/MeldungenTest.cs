using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was beim Laden im Infotab landet.
    ///
    /// Der Infotab ist die Stelle, an der die Anwendung sagt, dass mit den Daten etwas nicht
    /// stimmt. Das funktioniert nur, solange dort wenig steht: zwei Dutzend gleichlautende
    /// Warnungen bei jedem Start bringen niemanden mehr dazu hinzusehen.
    /// </summary>
    public class MeldungenTest {

        /// <summary>
        /// Sammelt die Meldungen, die waehrend einer Aktion anfallen
        /// </summary>
        private static List<LogEntry> SammleBeim(Action aktion) {
            List<LogEntry> gesammelt = [];
            void Horcher(object? sender, ViewEventArgs e) {
                if (e.LogEntry != null)
                    lock (gesammelt) gesammelt.Add(e.LogEntry);
            }
            ProgramView.OnViewEvent += Horcher;
            try { aktion(); }
            finally { ProgramView.OnViewEvent -= Horcher; }
            return gesammelt;
        }

        /// <summary>
        /// In der Karte stehen Gebaeude, zu denen die Bauwerkliste keinen Eintrag fuehrt - in den
        /// echten Daten rund zwanzig. Dazu gehoert eine Meldung, nicht zwanzig.
        /// </summary>
        [StaFact]
        public void FehlendeGebaeudeErgebenEineMeldungUndNichtZwanzig() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);

            var meldungen = SammleBeim(() => TestSetup.LoadKarte(erzwingen: true));

            var gebäude = meldungen
                .Where(m => m.Titel.Contains("Bauwerkliste") || m.Titel.Contains("Bauwerktabelle"))
                .ToList();
            Assert.True(gebäude.Count <= 1,
                $"Das Laden der Karte meldet {gebäude.Count} mal fehlende Gebaeude: "
                + string.Join(" | ", gebäude.Select(m => m.Titel)));

            // und wenn welche fehlen, sagt die eine Meldung, wieviele und welche
            if (gebäude.Count == 1) {
                Assert.Contains("Bauwerkliste", gebäude[0].Titel);
                Assert.Contains("/", gebäude[0].Message);
                // Nachgetragene Eintraege sind kein Missstand mehr, sondern eine Mitteilung.
                // Nur was sich nicht nachtragen laesst, bleibt eine Warnung.
                if (gebäude[0].Titel.Contains("nachgetragen"))
                    Assert.Equal(LogEntry.LogType.Info, gebäude[0].Type);
                else
                    Assert.Equal(LogEntry.LogType.Warning, gebäude[0].Type);
            }
        }

        /// <summary>
        /// Die fehlenden Eintraege muessen in der Bauwerkliste landen, sonst fehlen sie beim
        /// naechsten Start wieder.
        ///
        /// Der Haken sitzt in der Reihenfolge: die Anwendung laedt die Karte VOR der PZE
        /// (Main.Load: LoadCrossRef, LoadKarte, LoadPZE). Waehrend die Bauwerkliste repariert wird,
        /// gibt es also noch keine Nationen, KleinFeld.Nation liefert null und der ergaenzte
        /// Eintrag bleibt ohne Reich. Ohne Reich ist er unvollstaendig - die Tabelle fuehrt genau
        /// vier Spalten - und wurde deshalb nie geschrieben.
        ///
        /// Die fruehere Fassung dieses Tests hat das nicht gefunden, weil sie die PZE vor der Karte
        /// geladen hat. Sie pruefte damit eine Reihenfolge, die es in der Anwendung nicht gibt.
        ///
        /// Geprueft wird die Speicherwarteschlange, nicht die Datenbank: im Testlauf leert sie
        /// niemand, die echten Kartendaten bleiben also unberuehrt.
        /// </summary>
        [StaFact]
        public void DasReichWirdNachgereichtSonstLandetDerNachtragNieInDerDatenbank() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            // die Reihenfolge der Anwendung: erst die Karte, dann die Reiche
            TestSetup.LoadKarte(erzwingen: true);
            TestSetup.LoadPZE(false, false);

            // Was die Reparatur erfunden hat, traegt IsNew; alles aus der Datenbank Geladene nicht.
            var nachgetragen = SharedData.Gebäude!.Values.Where(gebäude => gebäude.IsNew).ToList();
            if (nachgetragen.Count == 0)
                return; // in diesen Kartendaten fehlt nichts

            // So sehen die Eintraege aus, wie Phase 1 sie hinterlaesst: Position und Name aus der
            // Karte, kein Reich. Kopien, damit der Test die geteilten Daten nicht anfasst.
            var offen = nachgetragen
                .Select(gebäude => new PhoenixModel.dbErkenfara.Gebäude {
                    gf = gebäude.gf,
                    kf = gebäude.kf,
                    Bauwerknamen = gebäude.Bauwerknamen,
                })
                .ToList();
            Assert.All(offen, haus => Assert.True(string.IsNullOrEmpty(haus.Reich)));

            var (vollständig, ohneReich) = BauwerkeView.VervollständigeReiche(offen);

            Assert.True(ohneReich.Count == 0,
                $"Zu diesen Gemarken nennt die Karte kein Reich: {string.Join(", ", ohneReich)}");
            Assert.Equal(offen.Count, vollständig.Count);

            foreach (var haus in vollständig) {
                Assert.False(string.IsNullOrEmpty(haus.Reich), $"{haus.Bezeichner} hat kein Reich");
                Assert.True(haus.gf > 0 && haus.kf > 0, $"{haus.Bezeichner} hat keine Position");
                // und zwar das Reich, das die Karte fuer diese Gemark nennt
                Assert.Equal(SharedData.Map![haus.Bezeichner].Nation?.Reich, haus.Reich);

                // Der entscheidende Punkt: der Speicherlauf ordnet einen Datensatz ueber den Pfad
                // einer der vier bekannten Datenbanken zu. Stimmt er nicht genau mit dem ueberein,
                // was in den Einstellungen steht, verwirft er den Datensatz mit einem
                // Protokolleintrag - der Nachtrag liefe dann ins Leere.
                Assert.Equal(TestSetup.KartenPfad, haus.Database);
            }
        }

        /// <summary>
        /// Der ergaenzte Eintrag muss vollstaendig sein. Die Tabelle fuehrt vier Spalten, und das
        /// Reich stand frueher nicht darin - fuer die Anzeige genuegte das, fuer einen Eintrag in
        /// der Datenbank nicht.
        /// </summary>
        [StaFact]
        public void EinErgaenzterEintragKenntSeinReich() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);

            var mitReich = SharedData.Map!.Values.FirstOrDefault(k => k.Baupunkte > 0 && k.Nation != null);
            Assert.True(mitReich != null, "In der Karte steht kein Gebaeude mit Reich");

            // den Eintrag entfernen und ueber den Zugriff neu erzeugen lassen
            Assert.True(SharedData.Gebäude!.TryRemove(mitReich!.Bezeichner, out var entfernt));
            try {
                var ergänzt = BauwerkeView.ErgänzeFehlendesGebäude(mitReich, stillschweigend: true);
                Assert.True(ergänzt != null, "Es wurde kein Eintrag ergaenzt");
                Assert.Equal(mitReich.Nation!.DBname, ergänzt!.Reich);
                Assert.Equal(mitReich.gf, ergänzt.gf);
                Assert.Equal(mitReich.kf, ergänzt.kf);
            }
            finally {
                if (entfernt != null)
                    SharedData.Gebäude![mitReich.Bezeichner] = entfernt;
            }
        }

        /// <summary>
        /// Der Einzelfall, der erst beim Zugriff auffaellt, meldet sich weiterhin - der ist selten
        /// und deshalb eine Meldung wert. Aber er wird jetzt auch zum Schreiben vorgemerkt.
        ///
        /// Bisher hat nur die Sammelreparatur den Eintrag vorgemerkt. Der Einzelfall ergaenzte ihn
        /// im Speicher und sagte dazu, in der Datenbank fehle er weiterhin - beim naechsten Start
        /// stand dieselbe Meldung wieder da.
        /// </summary>
        [StaFact]
        public void DerEinzelfallBeimZugriffWirdAuchVorgemerkt() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();

            var gemark = SharedData.Map!.Values.First(k => k.Baupunkte > 0);
            Assert.True(SharedData.Gebäude!.TryRemove(gemark.Bezeichner, out var entfernt));
            SharedData.StoreQueue.Clear();
            int wartendVorher = BauwerkeView.AnzahlWartenderBauwerke;
            try {
                var meldungen = SammleBeim(() => BauwerkeView.ErgänzeFehlendesGebäude(gemark));
                Assert.Single(meldungen);
                Assert.Contains("Fehlendes Gebäude", meldungen[0].Titel);

                // Die Meldung verspricht nichts, was sie nicht haelt - behauptet aber auch nicht
                // mehr, der Eintrag bleibe in der Datenbank aus.
                Assert.DoesNotContain("automatisch korrigiert", meldungen[0].Message);
                Assert.DoesNotContain("fehlt er weiterhin", meldungen[0].Message);
                Assert.Contains("geschrieben", meldungen[0].Message);

                // Und das stimmt auch: der Eintrag ist entweder schon eingestellt oder wartet auf
                // sein Reich. Was von beidem, haengt daran, ob die Nationen schon geladen sind.
                bool eingestellt = SharedData.StoreQueue.ToList().Any(eintrag =>
                    eintrag.Table is PhoenixModel.dbErkenfara.Gebäude haus && haus.Bezeichner == gemark.Bezeichner);
                bool wartet = BauwerkeView.AnzahlWartenderBauwerke > wartendVorher;
                Assert.True(eingestellt || wartet,
                    "Der ergaenzte Eintrag ist weder eingestellt noch zum Schreiben vorgemerkt");
            }
            finally {
                if (entfernt != null)
                    SharedData.Gebäude![gemark.Bezeichner] = entfernt;
                SharedData.StoreQueue.Clear();
            }
        }

        /// <summary>
        /// Was das Kontextmenue "In Zwischenablage kopieren" aus einem Eintrag macht: Kopfzeile,
        /// darunter die Erklaerung. Ohne Erklaerung bleibt es bei der Kopfzeile - eine leere
        /// Folgezeile waere beim Einfuegen nur im Weg.
        /// </summary>
        [Fact]
        public void EinEintragWirdMitSeinerErklaerungKopiert() {
            var mitText = new LogEntry(LogEntry.LogType.Warning, "Fehlendes Gebäude", "Auf 305/4 steht ein Haus");
            Assert.Equal($"Fehlendes Gebäude{Environment.NewLine}Auf 305/4 steht ein Haus", mitText.AlsText());

            var ohneText = new LogEntry(LogEntry.LogType.Info, "Alles in Ordnung", string.Empty);
            Assert.Equal("Alles in Ordnung", ohneText.AlsText());
            Assert.False(ohneText.AlsText().EndsWith(Environment.NewLine));
        }

        /// <summary>
        /// Mehrere Eintraege kommen durch eine Leerzeile getrennt in die Zwischenablage. Eintraege
        /// ohne Titel zeigt die Liste nicht an und duerfen auch im kopierten Text keine Luecke
        /// hinterlassen.
        /// </summary>
        [Fact]
        public void AlleAngezeigtenEintraegeKommenDurchLeerzeilenGetrenntZusammen() {
            List<LogEntry> eintraege = [
                new LogEntry(LogEntry.LogType.Warning, "Erste Warnung", "dazu die Erklärung"),
                new LogEntry(LogEntry.LogType.Info, string.Empty, "ohne Titel, faellt weg"),
                new LogEntry(LogEntry.LogType.Error, "Zweiter Fehler", string.Empty),
            ];

            string text = LogEntry.AlsText(eintraege);
            string nl = Environment.NewLine;
            Assert.Equal($"Erste Warnung{nl}dazu die Erklärung{nl}{nl}Zweiter Fehler", text);
            Assert.DoesNotContain("faellt weg", text);

            // und nichts anzuzeigen ergibt nichts zu kopieren, keine Ausnahme
            Assert.Equal(string.Empty, LogEntry.AlsText([]));
            Assert.Equal(string.Empty, LogEntry.AlsText(null));
        }

        /// <summary>
        /// Alles, was die Reparatur in der Bauwerkliste ergaenzt, muss auf dem Weg in die Datenbank
        /// sein - sonst steht dieselbe Meldung beim naechsten Start wieder da.
        ///
        /// Es gibt zwei Wege dorthin, je nachdem, ob das Reich schon zu ermitteln ist: sofort in die
        /// Speicherwarteschlange, oder erst auf die Warteliste und von dort, sobald die Nationen
        /// geladen sind. Welcher Weg greift, haengt an der Ladereihenfolge - die Anwendung laedt die
        /// Karte vor der PZE, ein Testlauf hat die Nationen oft schon. Der Test prueft deshalb nicht
        /// den Weg, sondern das Ergebnis: am Ende ist jeder ergaenzte Eintrag eingestellt.
        ///
        /// Geprueft wird die Speicherwarteschlange, nicht die Datenbank: im Testlauf leert sie
        /// niemand, die echten Kartendaten bleiben also unberuehrt.
        /// </summary>
        [StaFact]
        public void JedesErgaenzteBauwerkLandetInDerSpeicherwarteschlange() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadPZE(false, false);

            SharedData.StoreQueue.Clear();
            try {
                TestSetup.LoadKarte(erzwingen: true);

                // Was die Reparatur erfunden hat, traegt IsNew; alles aus der Datenbank Geladene nicht.
                var ergänzt = SharedData.Gebäude!.Values.Where(gebäude => gebäude.IsNew).ToList();
                if (ergänzt.Count == 0)
                    return; // in diesen Kartendaten fehlt nichts

                // was noch wartet, jetzt nachziehen - die Anwendung tut das, sobald alles geladen ist
                var (_, ohneReich) = BauwerkeView.SchreibeWartendeBauwerke();
                Assert.True(ohneReich.Count == 0,
                    $"Zu diesen Gemarken nennt die Karte kein Reich: {string.Join(", ", ohneReich)}");
                Assert.Equal(0, BauwerkeView.AnzahlWartenderBauwerke);

                var vorgemerkt = SharedData.StoreQueue.ToList()
                    .Where(eintrag => eintrag.Table is PhoenixModel.dbErkenfara.Gebäude)
                    .ToDictionary(eintrag => ((PhoenixModel.dbErkenfara.Gebäude)eintrag.Table).Bezeichner);

                foreach (var haus in ergänzt) {
                    Assert.True(vorgemerkt.ContainsKey(haus.Bezeichner),
                        $"{haus.Bezeichner} wurde ergaenzt, aber nicht zum Schreiben vorgemerkt");

                    var eintrag = vorgemerkt[haus.Bezeichner];
                    // eingefuegt, nicht aktualisiert - die Zeile gibt es ja noch nicht
                    Assert.Equal(PhoenixModel.Database.DatabaseQueue.DatabaseQueueCommand.Insert, eintrag.Command);
                    Assert.False(string.IsNullOrEmpty(haus.Reich), $"{haus.Bezeichner} hat kein Reich");
                    Assert.Equal(SharedData.Map![haus.Bezeichner].Nation?.Reich, haus.Reich);

                    // Der entscheidende Punkt: der Speicherlauf ordnet einen Datensatz ueber den
                    // Pfad einer der vier bekannten Datenbanken zu. Stimmt er nicht genau mit dem
                    // ueberein, was in den Einstellungen steht, verwirft er den Datensatz mit einem
                    // Protokolleintrag - der Nachtrag liefe dann ins Leere.
                    Assert.Equal(TestSetup.KartenPfad, haus.Database);
                }
            }
            finally {
                // nichts davon darf in die echten Kartendaten laufen
                SharedData.StoreQueue.Clear();
            }
        }

        /// <summary>
        /// Ein Ruestort in der Karte ohne Baupunkte ist kein zerstoertes Gebaeude.
        ///
        /// Bisher galt jeder Eintrag der Bauwerkliste als zerstoert, sobald die Karte dort null
        /// Baupunkte fuehrte - und jeder ergab eine eigene Warnung mit dem Versprechen, der Fehler
        /// sei "automatisch korrigiert". Beides stimmte nicht: fuehrt die Karte weiter einen
        /// Ruestort, steht dort sehr wohl ein Bauwerk, und korrigiert wurde nie etwas - die Tabelle
        /// hat gar kein Feld dafuer.
        /// </summary>
        [StaFact]
        public void EinRuestortOhneBaupunkteGiltNichtAlsZerstoert() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadKarte(erzwingen: true);

            var abgleich = BauwerkeView.MarkiereZerstörteBauwerke();

            // Was als zerstoert gilt, fuehrt die Karte tatsaechlich nicht mehr
            foreach (var bezeichner in abgleich.Zerstört) {
                var gemark = SharedData.Map![bezeichner];
                Assert.Equal(0, gemark.Baupunkte);
                Assert.True(gemark.Ruestort == null || gemark.Ruestort == 0,
                    $"{bezeichner} gilt als zerstoert, die Karte fuehrt aber Ruestort {gemark.Ruestort}");
                Assert.True(SharedData.Gebäude![bezeichner].Zerstört);
            }

            // Und was noch einen Ruestort hat, bleibt unangetastet
            foreach (var bezeichner in abgleich.MitRüstortOhneBaupunkte) {
                var gemark = SharedData.Map![bezeichner];
                Assert.Equal(0, gemark.Baupunkte);
                Assert.True(gemark.Ruestort > 0);
                Assert.False(SharedData.Gebäude![bezeichner].Zerstört,
                    $"{bezeichner} fuehrt einen Ruestort und darf nicht als zerstoert gelten");
            }

            // Die beiden Gruppen ueberschneiden sich nicht und decken alles ab
            Assert.Empty(abgleich.Zerstört.Intersect(abgleich.MitRüstortOhneBaupunkte));
            int betroffen = SharedData.Gebäude!.Values.Count(gebäude =>
                SharedData.Map!.TryGetValue(gebäude.Bezeichner, out var gemark) && gemark.Baupunkte == 0);
            Assert.Equal(betroffen, abgleich.Zerstört.Count + abgleich.MitRüstortOhneBaupunkte.Count);
        }
    }
}
