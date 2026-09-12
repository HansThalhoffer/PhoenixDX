using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Gibt eine geschlossene Datenbank ihre Datei wieder frei?
    ///
    /// Die Zugabgabe und die Rückgabe von der Spielleitung verlassen sich darauf: sie kopieren,
    /// überschreiben und verschieben .mdb-Dateien direkt nachdem die Anwendung mit ihnen fertig
    /// ist. Hält der Access-Treiber die Datei dann noch, schlägt das im besten Fall mit einer
    /// Freigabeverletzung fehl - und im schlechtesten stirbt der Prozess in der nativen Schicht,
    /// wie es der Kommentar zum Verbindungspooling in AccessDatabase beschreibt.
    /// </summary>
    public class DatenbankFreigabeTest {

        /// <summary>
        /// Legt eine Arbeitskopie der echten Zugdatenbank an. Angefasst wird nur die Kopie.
        /// </summary>
        private static string ErstelleKopie(out string spielwiese) {
            string quelle = TestSetup.ZugdatenPfad;
            spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Freigabe_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(spielwiese);
            string ziel = Path.Combine(spielwiese, Path.GetFileName(quelle));
            File.Copy(quelle, ziel);
            return ziel;
        }

        /// <summary>
        /// Hält irgendjemand die Datei noch? Exklusiv öffnen gelingt nur, wenn nicht.
        /// </summary>
        private static bool IstFrei(string datei) {
            try {
                using var probe = new FileStream(datei, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException) {
                return false;
            }
        }

        /// <summary>
        /// Öffnen, lesen, schliessen - und die Datei muss sofort danach frei sein. Mehrfach, weil
        /// die Abstürze dieser Sorte sporadisch auftreten.
        /// </summary>
        [StaFact]
        public void EineGeschlosseneDatenbankGibtIhreDateiSofortFrei() {
            TestSetup.Setup();
            string kopie = ErstelleKopie(out string spielwiese);
            string passwort = new PhoenixModel.Database.PasswordHolder(TestSetup.ZugdatenPasswort).DecryptedPassword ?? string.Empty;

            try {
                for (int durchgang = 1; durchgang <= 10; durchgang++) {
                    using (var db = new AccessDatabase(kopie, passwort)) {
                        Assert.True(db.Open(), $"Durchgang {durchgang}: die Kopie liess sich nicht öffnen");
                        using (var reader = db.OpenReader("SELECT nummer, Beschriftung FROM Zauberer ORDER BY nummer")) {
                            int gelesen = 0;
                            while (reader.Read() && gelesen < 5)
                                gelesen++;
                            Assert.True(gelesen > 0, $"Durchgang {durchgang}: nichts gelesen");
                        }
                    }
                    Assert.True(IstFrei(kopie),
                        $"Durchgang {durchgang}: die Datei ist nach dem Schliessen der Verbindung noch belegt. "
                        + "Darauf bauen Zugabgabe und Rückgabe auf.");
                }
            }
            finally {
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Derselbe Ablauf über den Lader, den die Anwendung wirklich benutzt: er öffnet die
        /// Datenbank, füllt SharedData und schliesst wieder.
        /// </summary>
        [StaFact]
        public void NachDemLadenDerZugdatenIstDieDateiFrei() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);

            string kopie = ErstelleKopie(out string spielwiese);

            try {
                for (int durchgang = 1; durchgang <= 3; durchgang++) {
                    Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, PhoenixModel.View.ProgramView.SelectedMonth),
                        $"Durchgang {durchgang}: die Kopie liess sich nicht laden");
                    Assert.True(IstFrei(kopie),
                        $"Durchgang {durchgang}: die Zugdatenbank ist nach dem Laden noch belegt");
                }

                // und das ist die Operation, auf die es ankommt: ersetzen, wie es die Rückgabe tut
                File.Copy(TestSetup.ZugdatenPfad, kopie, true);
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }
    }
}
