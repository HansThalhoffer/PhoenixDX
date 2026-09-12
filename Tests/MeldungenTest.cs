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
                Assert.Contains("fehlen in der Bauwerkliste", gebäude[0].Titel);
                Assert.Contains("/", gebäude[0].Message);
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
    }
}
