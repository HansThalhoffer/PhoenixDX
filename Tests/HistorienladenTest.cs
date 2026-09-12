using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;

namespace Tests {

    /// <summary>
    /// Ruft den Bericht der Handwerker genau einmal auf und haelt fest, was er hinterlaesst.
    ///
    /// Einmal, weil der Bericht die Ruestungstabellen von rund vierzig vergangenen Zuegen liest
    /// und dabei zwei Access-Verbindungen je Zug oeffnet - rund 80 Verbindungen je Aufruf. Jede
    /// davon traegt ein Absturzrisiko (siehe SpeichernIntegrationTest), drei Aufrufe haben den
    /// Testlauf messbar wieder an die Grenze gebracht.
    /// </summary>
    public class Historienumgebung {

        public string SpeicherortVorher { get; }
        public string SpeicherortNachher { get; }
        public string RuestorteVorher { get; }
        public string RuestorteNachher { get; }
        public long BaupunkteVorher { get; }
        public long BaupunkteNachher { get; }
        public int GebäudeVorher { get; }
        public int GebäudeNachher { get; }
        public int BefehleVorher { get; }
        public int BefehleNachher { get; }
        public int BauauftraegeVorher { get; }
        public int BauauftraegeNachher { get; }
        public int RuestorteAuftraegeVorher { get; }
        public int RuestorteAuftraegeNachher { get; }

        public Historienumgebung() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);

            SpeicherortVorher = RuestungBauwerke.DatabaseName;
            RuestorteVorher = RuestungRuestorte.DatabaseName;
            BaupunkteVorher = SharedData.Map!.Values.Sum(k => (long)k.Baupunkte);
            GebäudeVorher = SharedData.Gebäude!.Count;
            BefehleVorher = SharedData.Commands.Count;
            BauauftraegeVorher = SharedData.RuestungBauwerke!.Count;
            RuestorteAuftraegeVorher = SharedData.RuestungRuestorte!.Count;

            Zugdaten.LoadBaukostenHistory();

            SpeicherortNachher = RuestungBauwerke.DatabaseName;
            RuestorteNachher = RuestungRuestorte.DatabaseName;
            BaupunkteNachher = SharedData.Map!.Values.Sum(k => (long)k.Baupunkte);
            GebäudeNachher = SharedData.Gebäude!.Count;
            BefehleNachher = SharedData.Commands.Count;
            BauauftraegeNachher = SharedData.RuestungBauwerke!.Count;
            RuestorteAuftraegeNachher = SharedData.RuestungRuestorte!.Count;
        }
    }

    /// <summary>
    /// Der Bericht ueber vergangene Zuege darf den laufenden Zug nicht anfassen.
    ///
    /// Zwei Fehler steckten hier, beide ausgeloest vom selben Menuepunkt: der Bericht hat den
    /// Speicherort auf eine alte Zugdatenbank umgestellt, und er hat die Bauauftraege von
    /// vierzig vergangenen Zuegen auf die aktuelle Karte gelegt.
    /// </summary>
    public class HistorienladenTest : IClassFixture<Historienumgebung> {

        private readonly Historienumgebung _umgebung;

        public HistorienladenTest(Historienumgebung umgebung) {
            _umgebung = umgebung;
        }

        /// <summary>
        /// Der Speicherlauf entscheidet an der statischen Eigenschaft DatabaseName, in welche
        /// Datei ein Datensatz geht. Blieb sie nach dem Bericht auf einer alten Zugdatenbank
        /// stehen, fand der Speicherlauf die Datei nicht unter den bekannten und verwarf den
        /// Datensatz - ein danach erteilter Bauauftrag war damit still weg.
        /// </summary>
        [StaFact]
        public void DerBerichtVerstelltDenSpeicherortNicht() {
            Assert.False(string.IsNullOrEmpty(_umgebung.SpeicherortVorher), "Vor dem Test ist kein Speicherort gesetzt");
            Assert.Equal(_umgebung.SpeicherortVorher, _umgebung.SpeicherortNachher);
            Assert.Equal(_umgebung.RuestorteVorher, _umgebung.RuestorteNachher);
            Assert.Equal(TestSetup.ZugdatenPfad, _umgebung.SpeicherortNachher);
        }

        /// <summary>
        /// Jede gelesene Zeile hat frueher ihren Bauauftrag auf die aktuelle Karte gelegt und
        /// ihren Befehl in die Zughistorie geschrieben - ueber vierzig Zuege hinweg.
        /// </summary>
        [StaFact]
        public void DerBerichtVerfaelschtKarteUndZughistorieNicht() {
            Assert.Equal(_umgebung.BaupunkteVorher, _umgebung.BaupunkteNachher);
            Assert.Equal(_umgebung.GebäudeVorher, _umgebung.GebäudeNachher);
            Assert.Equal(_umgebung.BefehleVorher, _umgebung.BefehleNachher);
        }

        /// <summary>
        /// Und die geladenen Zugdaten des laufenden Zuges bleiben stehen.
        /// </summary>
        [StaFact]
        public void DerBerichtLaesstDieGeladenenZugdatenInRuhe() {
            Assert.Equal(_umgebung.BauauftraegeVorher, _umgebung.BauauftraegeNachher);
            Assert.Equal(_umgebung.RuestorteAuftraegeVorher, _umgebung.RuestorteAuftraegeNachher);
        }

        /// <summary>
        /// Die Bauauftraege des laufenden Zuges gehoeren dagegen sehr wohl auf die Karte: ein
        /// begonnener Bau erscheint dort als Baustelle. Das darf beim Herausziehen der
        /// Nebenwirkung aus dem Laden nicht verlorengegangen sein.
        /// </summary>
        [StaFact]
        public void DieBauauftraegeDesLaufendenZugesStehenAufDerKarte() {
            var aufträge = SharedData.RuestungBauwerke!.ToList();
            if (aufträge.Count == 0)
                return;   // in diesem Zug wurde nichts gebaut, dann ist hier nichts zu pruefen

            foreach (var auftrag in aufträge) {
                var feld = SharedData.Map![auftrag.CreateBezeichner()];
                Assert.True(feld.Baupunkte < 0 || SharedData.Gebäude!.ContainsKey(auftrag.CreateBezeichner()),
                    $"Der Bauauftrag auf {auftrag.CreateBezeichner()} steht nicht auf der Karte");
            }
        }
    }
}
