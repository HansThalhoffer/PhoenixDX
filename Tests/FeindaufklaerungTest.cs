using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Das Laden der Feindaufklaerung - Issue #48.
    ///
    /// Die Datei fuehrt die Einheiten, die das eigene Reich aufgeklaert hat, und je nach Herkunft
    /// auch die eigenen. Eigene gehoeren nicht hinein: die stehen vollstaendig in den Zugdaten,
    /// waeren hier nur eine zweite, aeltere Wahrheit und erschienen auf der Karte als Fremde.
    ///
    /// Gelesen wird die echte Datei des Spielers; geschrieben wird nichts.
    /// </summary>
    public class FeindaufklaerungTest {

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
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Keine einzige eigene Einheit landet in der Feindaufklaerung - auch dann nicht, wenn die
        /// Datei sie fuehrt. In der Datei des Spielers stehen 45 Eintraege des eigenen Reiches.
        /// </summary>
        [StaFact]
        public void EigeneEinheitenLandenNichtInDerFeindaufklaerung() {
            LadeAlles();
            TestSetup.LoadFeinderkennung();
            Assert.True(SharedData.Feinde != null && SharedData.Feinde.Count > 0,
                $"Die Feindaufklaerung wurde nicht geladen: {TestSetup.Feindaufklärungsdatei}");

            Assert.DoesNotContain(SharedData.Feinde!, feind => feind.Nation == ProgramView.SelectedNation);

            // Gegenprobe an der Datei selbst: dort stehen eigene Eintraege sehr wohl
            int inDerDatei = File.ReadAllLines(TestSetup.Feindaufklärungsdatei)
                .Count(zeile => zeile.Split(';').Length > 1
                    && NationenView.GetNationFromString(zeile.Split(';')[1]) == ProgramView.SelectedNation);
            Assert.True(inDerDatei > 0,
                "In dieser Datei stehen keine eigenen Eintraege - dann prueft der Test den Filter nicht");

            // Die Bilanz muss aufgehen: jede Zeile mit Inhalt ist entweder geladen, eigen, ohne
            // brauchbare Position oder der Zeitstempeleintrag. Bleibt etwas uebrig, verschwindet
            // es unbemerkt.
            int ohnePosition = 0, zeitstempel = 0;
            foreach (var zeile in File.ReadAllLines(TestSetup.Feindaufklärungsdatei)) {
                if (string.IsNullOrWhiteSpace(zeile) || zeile.TrimStart().StartsWith('#'))
                    continue;
                var feind = new Feinde(zeile);
                if (feind.Nummer == Feinde.Zeitstempeleintrag || string.Equals(feind.Reich, "Update", StringComparison.OrdinalIgnoreCase))
                    zeitstempel++;
                else if (feind.Nation == ProgramView.SelectedNation)
                    continue;   // schon in inDerDatei gezaehlt
                else if (feind.gf <= 0 || feind.kf <= 0 || feind.kf > 48)
                    ohnePosition++;
            }
            Assert.Equal(ZeilenMitInhalt(), SharedData.Feinde!.Count + inDerDatei + ohnePosition + zeitstempel);
        }

        private static int ZeilenMitInhalt() {
            return File.ReadAllLines(TestSetup.Feindaufklärungsdatei)
                .Count(zeile => string.IsNullOrWhiteSpace(zeile) == false && zeile.TrimStart().StartsWith('#') == false);
        }

        /// <summary>
        /// Jede geladene Einheit hat eine Position auf der Karte und einen erkannten Typ. Eine
        /// Zeile, die das nicht hergibt, gehoert nicht auf die Karte.
        /// </summary>
        [StaFact]
        public void JedeGeladeneEinheitStehtAufDerKarteUndHatEinenTyp() {
            LadeAlles();
            TestSetup.LoadFeinderkennung();

            Assert.All(SharedData.Feinde!, feind => {
                Assert.True(feind.gf > 0 && feind.kf > 0 && feind.kf <= 48,
                    $"{feind.Reich} {feind.Nummer} steht auf {feind.gf}/{feind.kf}");
                Assert.True(SharedData.Map!.ContainsKey(feind.CreateBezeichner()),
                    $"{feind.CreateBezeichner()} liegt nicht auf der Karte");
                Assert.NotEqual(FigurType.None, feind.Typ);
            });
        }

        /// <summary>
        /// Das Laden sagt, was es geladen hat und woher.
        ///
        /// Es gibt mehrere Dateien dieses Namens im Datenbestand, und sie unterscheiden sich.
        /// Ohne Meldung faellt eine veraltete nicht auf - man sieht nur eine Karte, auf der
        /// irgendwelche fremden Heere stehen.
        /// </summary>
        [StaFact]
        public void DasLadenSagtWasEsGeladenHat() {
            LadeAlles();
            var meldungen = SammleBeim(() => TestSetup.LoadFeinderkennung(erzwingen: true));

            var meldung = meldungen.FirstOrDefault(m => m.Titel.Contains("Feindaufklärung"));
            Assert.True(meldung != null, "Das Laden der Feindaufklärung meldet sich nicht");
            Assert.Contains(TestSetup.Feindaufklärungsdatei, meldung!.Message);
            Assert.Contains("eigene", meldung.Message);
            Assert.Contains(SharedData.Feinde!.Count.ToString(), meldung.Titel);
        }

        /// <summary>
        /// Der Zeitstempeleintrag der Altanwendung ist keine Einheit und gehoert nicht auf die
        /// Karte. In den Dateien des Spielers steht er nicht, in aelteren aber schon.
        /// </summary>
        [StaFact]
        public void DerZeitstempeleintragIstKeineEinheit() {
            LadeAlles();
            TestSetup.LoadFeinderkennung();

            Assert.DoesNotContain(SharedData.Feinde!, feind => feind.Nummer == Feinde.Zeitstempeleintrag);
            Assert.DoesNotContain(SharedData.Feinde!, feind => string.Equals(feind.Reich, "Update", StringComparison.OrdinalIgnoreCase));
        }
    }
}
