using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;

namespace Tests {

    /// <summary>
    /// Was passiert, wenn ein Schreibvorgang scheitert.
    ///
    /// Frueher: nichts Sichtbares. Die Ausnahme wurde protokolliert, die Methode lief durch, und
    /// der Aufrufer hielt den Vorgang fuer erledigt. Im Testlauf war selbst das Protokoll leer,
    /// weil die Meldung direkt in die Anzeige ging und LogPage.AddToLog ohne Oberflaeche sofort
    /// aussteigt. Ein fehlgeschlagenes Loeschen sah aus wie ein erfolgreiches.
    /// </summary>
    public class DatenbankfehlerTest {

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

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadKarte();
        }

        /// <summary>
        /// Laesst sich die Datenbank nicht oeffnen, meldet sich das - und sagt, dass nichts
        /// geschrieben wurde.
        /// </summary>
        [StaFact]
        public void EinFehlgeschlagenesLoeschenMeldetSichUndGibtFalseZurueck() {
            LadeAlles();
            var irgendeines = SharedData.Gebäude!.Values.First();

            string gibtesnicht = Path.Combine(Path.GetTempPath(), $"GibtEsNicht_{Guid.NewGuid():N}.mdb");
            var db = new ErkenfaraKarte(gibtesnicht, TestSetup.KartenPasswort);

            bool ergebnis = true;
            var meldungen = SammleBeim(() => ergebnis = db.Delete(irgendeines));

            Assert.False(ergebnis, "Ein Loeschen in einer nicht vorhandenen Datenbank darf nicht true liefern");
            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Error);
            Assert.Contains(meldungen, m => m.Message.Contains("nichts geschrieben"));
        }

        /// <summary>
        /// Die Bereinigung zaehlt nur, was wirklich verschwunden ist.
        ///
        /// Geprueft mit einer Datei, die zwar da ist, aber keine Access-Datenbank - die Sicherung
        /// gelingt, das Loeschen nicht. Genau die Konstellation, in der frueher ein "erledigt"
        /// gemeldet worden waere.
        /// </summary>
        [StaFact]
        public void WasSichNichtLoeschenLaesstGiltNichtAlsGeloescht() {
            LadeAlles();

            var leeresGemark = SharedData.Map!.Values.First(gemark =>
                gemark.Baupunkte == 0
                && (gemark.Ruestort == null || gemark.Ruestort == 0)
                && SharedData.Gebäude!.ContainsKey(gemark.Bezeichner) == false);
            var karteileiche = new Gebäude { gf = leeresGemark.gf, kf = leeresGemark.kf, Bauwerknamen = "Geisterburg" };
            Assert.True(SharedData.Gebäude!.TryAdd(karteileiche.Bezeichner, karteileiche));

            string keineDatenbank = Path.Combine(Path.GetTempPath(), $"KeineDatenbank_{Guid.NewGuid():N}.mdb");
            File.WriteAllText(keineDatenbank, "Das ist keine Access-Datenbank.");
            Kartenbereinigung.Ergebnis? ergebnis = null;
            try {
                var meldungen = SammleBeim(() =>
                    ergebnis = Kartenbereinigung.Bereinige(keineDatenbank, TestSetup.KartenPasswort));

                Assert.NotNull(ergebnis);
                Assert.Empty(ergebnis!.Geloescht);
                Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Error);
                // und der Eintrag bleibt im Speicher stehen, sonst faende ihn der naechste Start
                // wieder und meldete ihn erneut
                Assert.True(SharedData.Gebäude.ContainsKey(karteileiche.Bezeichner));
            }
            finally {
                SharedData.Gebäude.TryRemove(karteileiche.Bezeichner, out _);
                File.Delete(keineDatenbank);
                foreach (var reste in Directory.GetFiles(Path.GetTempPath(), "KeineDatenbank_*_vor_Bereinigung_*.bak"))
                    File.Delete(reste);
            }
        }

        /// <summary>
        /// Meldungen der Anwendungsseite laufen denselben Weg wie die des Modells. Sonst sieht sie
        /// im Testlauf niemand.
        /// </summary>
        [StaFact]
        public void MeldungenDerAnwendungsseiteSindBeobachtbar() {
            var meldungen = SammleBeim(() => {
                SpielWPF.LogInfo("Eine Probe", "zum Mitlesen");
                SpielWPF.LogWarning("Eine Warnung", "zum Mitlesen");
                SpielWPF.LogError("Ein Fehler", "zum Mitlesen");
            });

            Assert.Equal(3, meldungen.Count);
            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Info && m.Titel == "Eine Probe");
            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Warning && m.Titel == "Eine Warnung");
            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Error && m.Titel == "Ein Fehler");
        }
    }
}
