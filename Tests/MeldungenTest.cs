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
        /// und deshalb eine Meldung wert.
        /// </summary>
        [StaFact]
        public void DerEinzelfallBeimZugriffMeldetSichWeiterhin() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();

            var gemark = SharedData.Map!.Values.First(k => k.Baupunkte > 0);
            Assert.True(SharedData.Gebäude!.TryRemove(gemark.Bezeichner, out var entfernt));
            try {
                var meldungen = SammleBeim(() => BauwerkeView.ErgänzeFehlendesGebäude(gemark));
                Assert.Single(meldungen);
                Assert.Contains("Fehlendes Gebäude", meldungen[0].Titel);
                // und die Meldung verspricht nichts, was sie nicht haelt
                Assert.DoesNotContain("automatisch korrigiert", meldungen[0].Message);
                Assert.Contains("für diese Sitzung", meldungen[0].Message);
            }
            finally {
                if (entfernt != null)
                    SharedData.Gebäude![gemark.Bezeichner] = entfernt;
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
    }
}
