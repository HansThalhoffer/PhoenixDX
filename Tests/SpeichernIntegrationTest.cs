using PhoenixModel.Database;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.Diagnostics;
using System.IO;

namespace Tests {

    /// <summary>
    /// Das Speichern schreibt über eine einzige Verbindung je Durchgang.
    ///
    /// Vorher bekam jeder Datensatz seine eigene Verbindung. Der Access-Treiber baut dabei jedes
    /// Mal seine Strukturen neu auf: rund 185 Millisekunden je Datensatz, und etwa jede
    /// vierhundertste Verbindung endete in einer Zugriffsverletzung in mso99Lwin32client.dll, die
    /// den Prozess ohne verwertbare Ausnahme beendet hat. Gemessen im Ereignisprotokoll, immer
    /// derselbe Fehleroffset.
    ///
    /// Geschrieben wird ausschliesslich in eine Kopie; die echten Spieldaten bleiben unberührt.
    /// </summary>
    public class SpeichernIntegrationTest {

        /// <summary>
        /// So viele Schreibvorgänge, dass der alte Weg mit hoher Wahrscheinlichkeit abgestürzt
        /// wäre - er brauchte dafür ausserdem über eine Minute.
        /// </summary>
        private const int Vorgänge = 400;

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        private static string ErstelleKopie(out string spielwiese) {
            spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Speichern_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(spielwiese);
            string ziel = Path.Combine(spielwiese, Path.GetFileName(TestSetup.ZugdatenPfad));
            File.Copy(TestSetup.ZugdatenPfad, ziel);
            return ziel;
        }

        /// <summary>
        /// Viele Schreibvorgänge über eine Verbindung: sie müssen alle ankommen, und der Prozess
        /// muss es überleben.
        /// </summary>
        [StaFact]
        public void VieleAenderungenGehenUeberEineVerbindung() {
            LadeAlles();
            string kopie = ErstelleKopie(out string spielwiese);
            try {
                var zauberer = SharedData.Zauberer!
                    .Where(z => Plausibilität.IsValid(z))
                    .Take(Vorgänge)
                    .ToList();
                Assert.True(zauberer.Count > 0, "In den Zugdaten steht kein Zauberer");

                // jeder Zauberer bekommt einen erkennbaren Vermerk
                List<DatabaseQueue.DatabaseQueueItem> vorgänge = [];
                for (int i = 0; i < Vorgänge; i++) {
                    var figur = zauberer[i % zauberer.Count];
                    figur.sonstiges = $"Probe {i}";
                    vorgänge.Add(new DatabaseQueue.DatabaseQueueItem(figur, DatabaseQueue.DatabaseQueueCommand.Save));
                }

                var uhr = Stopwatch.StartNew();
                using (var db = new Zugdaten(kopie, TestSetup.ZugdatenPasswort)) {
                    db.SchreibeAlle(vorgänge);
                }
                uhr.Stop();

                // Der alte Weg brauchte je Datensatz eine eigene Verbindung und damit rund
                // 185 Millisekunden. Bleibt der Durchgang deutlich darunter, lief er gebündelt.
                Assert.True(uhr.ElapsedMilliseconds < Vorgänge * 50,
                    $"{Vorgänge} Änderungen dauerten {uhr.ElapsedMilliseconds} ms - das sieht nach einer "
                    + "Verbindung je Datensatz aus.");

                // und die Änderungen stehen wirklich in der Kopie
                var geschrieben = zauberer.ToDictionary(z => z.Nummer, z => z.sonstiges);
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, ProgramView.SelectedMonth));
                foreach (var eintrag in geschrieben) {
                    var neu = SharedData.Zauberer!.FirstOrDefault(z => z.Nummer == eintrag.Key);
                    Assert.True(neu != null, $"Zauberer {eintrag.Key} fehlt in der Kopie");
                    Assert.Equal(eintrag.Value, neu!.sonstiges);
                }
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Ein Vorgang, der nicht durchgeht, darf die übrigen nicht mitreissen. Vorher hatte jeder
        /// Datensatz seine eigene Verbindung und damit seine eigene Fehlerbehandlung; das Bündeln
        /// darf daran nichts ändern.
        /// </summary>
        [StaFact]
        public void EinFehlschlagReisstDieUebrigenNichtMit() {
            LadeAlles();
            string kopie = ErstelleKopie(out string spielwiese);
            try {
                var gut = SharedData.Zauberer!.First(z => Plausibilität.IsValid(z));
                gut.sonstiges = "geht durch";

                // eine Figur mit einer Nummer, die es nicht gibt, und einem Tabellennamen, den es
                // gibt - das Update trifft keine Zeile und fügt dann ein, was scheitern darf
                var kaputt = new Zauberer { Nummer = -1, Beschriftung = new string('x', 500) };

                List<DatabaseQueue.DatabaseQueueItem> vorgänge = [
                    new(kaputt, DatabaseQueue.DatabaseQueueCommand.Save),
                    new(gut, DatabaseQueue.DatabaseQueueCommand.Save),
                ];

                using (var db = new Zugdaten(kopie, TestSetup.ZugdatenPasswort)) {
                    db.SchreibeAlle(vorgänge);
                }

                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, ProgramView.SelectedMonth));
                var nachher = SharedData.Zauberer!.FirstOrDefault(z => z.Nummer == gut.Nummer);
                Assert.True(nachher != null, "Der gute Vorgang ist nicht angekommen");
                Assert.Equal("geht durch", nachher!.sonstiges);
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Eine leere Warteschlange darf keine Verbindung öffnen - sonst zahlt jeder Takt des
        /// Speicher-Timers den Verbindungsaufbau, obwohl es nichts zu tun gibt.
        /// </summary>
        [StaFact]
        public void OhneVorgaengeWirdKeineVerbindungGeoeffnet() {
            TestSetup.Setup();
            var uhr = Stopwatch.StartNew();
            using (var db = new Zugdaten("C:\\gibt\\es\\nicht\\Reich.mdb", TestSetup.ZugdatenPasswort)) {
                db.SchreibeAlle([]);
            }
            uhr.Stop();
            Assert.True(uhr.ElapsedMilliseconds < 50,
                $"Ein leerer Durchgang dauerte {uhr.ElapsedMilliseconds} ms - da wurde eine Verbindung geöffnet.");
        }
    }
}
