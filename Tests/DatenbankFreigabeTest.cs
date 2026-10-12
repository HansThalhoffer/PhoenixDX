using PhoenixModel.Database;
using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Gibt die Anwendung eine Datenbankdatei wieder frei - und wann?
    ///
    /// Die Zugabgabe und die Rueckgabe von der Spielleitung verlassen sich darauf: sie kopieren,
    /// ueberschreiben und verschieben .mdb-Dateien. Haelt der Access-Treiber die Datei dann noch,
    /// schlaegt das im besten Fall mit einer Freigabeverletzung fehl - und im schlechtesten
    /// stirbt der Prozess in der nativen Schicht.
    ///
    /// <b>Der Vertrag hat sich geaendert.</b> Frueher galt: geschlossen heisst frei. Seit
    /// AccessDatabase je Datei eine Verbindung offen haelt, gilt: frei ist die Datei nach
    /// <see cref="Datenbankwaechter.SchliesseAlle"/>. Das Offenhalten ist der Grund, warum der
    /// Treiber nicht mehr an der Zahl der Verbindungen stirbt - und SchliesseAlle ist der Preis
    /// dafuer, den die dateianfassenden Stellen zahlen muessen.
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
        /// Zehn Durchgaenge oeffnen, lesen, schliessen - und es bleibt bei <em>einer</em> Verbindung.
        ///
        /// Das ist der ganze Zweck der Uebung: der Access-Treiber stuerzt an der Zahl der
        /// Verbindungen, nicht an der Zahl der Abfragen. Gemessen etwa eine Zugriffsverletzung je
        /// 400 Verbindungen; ein voller Testlauf oeffnete 758 und stuerzte entsprechend etwa
        /// zweimal ab.
        /// </summary>
        [StaFact]
        public void ZehnDurchgaengeBrauchenNurEineVerbindung() {
            TestSetup.Setup();
            string kopie = ErstelleKopie(out string spielwiese);
            string passwort = new PhoenixModel.Database.PasswordHolder(TestSetup.ZugdatenPasswort).DecryptedPassword ?? string.Empty;

            Datenbankwaechter.SchliesseAlle();
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
                }
                Assert.Equal(1, Datenbankwaechter.AnzahlOffenerVerbindungen);

                // solange sie offen ist, gehoert die Datei der Anwendung
                Assert.False(IstFrei(kopie),
                    "Die Datei ist frei, obwohl eine Verbindung offen gehalten werden sollte");

                // und danach gehoert sie wieder niemandem
                Datenbankwaechter.SchliesseAlle();
                Assert.Equal(0, Datenbankwaechter.AnzahlOffenerVerbindungen);
                Assert.True(IstFrei(kopie),
                    "Nach SchliesseAlle ist die Datei noch belegt. Darauf bauen Zugabgabe und Rückgabe auf.");
            }
            finally {
                Datenbankwaechter.SchliesseAlle();
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Derselbe Ablauf ueber den Lader, den die Anwendung wirklich benutzt: er oeffnet die
        /// Datenbank, fuellt SharedData und schliesst wieder. Auch dreimaliges Laden kostet nur
        /// eine Verbindung, und ersetzen laesst sich die Datei nach SchliesseAlle.
        /// </summary>
        [StaFact]
        public void DreimalLadenKostetEineVerbindungUndDanachIstDieDateiFrei() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);

            string kopie = ErstelleKopie(out string spielwiese);

            try {
                Datenbankwaechter.SchliesseAlle();
                for (int durchgang = 1; durchgang <= 3; durchgang++) {
                    Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, PhoenixModel.View.ProgramView.SelectedMonth),
                        $"Durchgang {durchgang}: die Kopie liess sich nicht laden");
                }
                Assert.Equal(1, Datenbankwaechter.AnzahlOffenerVerbindungen);

                // und das ist die Operation, auf die es ankommt: ersetzen, wie es die Rückgabe tut
                Datenbankwaechter.SchliesseAlle();
                Assert.True(IstFrei(kopie), "Nach SchliesseAlle ist die Zugdatenbank noch belegt");
                File.Copy(TestSetup.ZugdatenPfad, kopie, true);
            }
            finally {
                Datenbankwaechter.SchliesseAlle();
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Die Passwortprobe hinterlaesst nichts im Cache - sie wird auch mit einem falschen
        /// Passwort gerufen, und eine Verbindung dazu will niemand offen halten.
        /// </summary>
        [StaFact]
        public void DiePasswortprobeHinterlaesstNichts() {
            TestSetup.Setup();
            string kopie = ErstelleKopie(out string spielwiese);
            string passwort = new PhoenixModel.Database.PasswordHolder(TestSetup.ZugdatenPasswort).DecryptedPassword ?? string.Empty;

            try {
                Datenbankwaechter.SchliesseAlle();
                Assert.True(Datenbankfabrik.PrüfeZugang(kopie, passwort, out string fehler), fehler);
                Assert.False(Datenbankfabrik.PrüfeZugang(kopie, "bestimmt-nicht-das-passwort", out _));

                Assert.Equal(0, Datenbankwaechter.AnzahlOffenerVerbindungen);
                Assert.True(IstFrei(kopie), "Die Passwortprobe hat die Datei nicht wieder freigegeben");
            }
            finally {
                Datenbankwaechter.SchliesseAlle();
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Eine benutzte Spielwiese muss sich wieder abräumen lassen.
        ///
        /// Das klingt nach Hausordnung und ist es nicht: in jeder Spielwiese liegt eine
        /// <b>Kopie echter Spieldaten</b>. Bleibt sie liegen, verteilt sich der Spielstand übers
        /// Temp-Verzeichnis. Gefunden an 169 Verzeichnissen und 496 MB, die sich über die Monate
        /// angesammelt hatten, weil <see cref="TestSetup.RäumeAuf"/> fünf Anläufe nahm, an der
        /// offen gehaltenen Verbindung scheiterte und dann stillschweigend aufgab.
        ///
        /// Der Vertrag steht oben: frei ist die Datei nach
        /// <see cref="Datenbankwaechter.SchliesseAlle"/>. Wer eine Datenbankdatei anfasst, muss
        /// ihn einhalten - auch ein Testhelfer.
        /// </summary>
        [StaFact]
        public void EineBenutzteSpielwieseLaesstSichAbraeumen() {
            TestSetup.Setup();
            string kopie = ErstelleKopie(out string spielwiese);
            string passwort = new PhoenixModel.Database.PasswordHolder(TestSetup.ZugdatenPasswort).DecryptedPassword ?? string.Empty;

            using (var db = new AccessDatabase(kopie, passwort)) {
                Assert.True(db.Open(), "Die Kopie liess sich nicht öffnen");
                db.Close();
            }
            // Geschlossen, aber nicht freigegeben - genau der Zustand, in dem aufgeräumt wird.
            Assert.False(IstFrei(kopie), "Die Verbindung wird nicht offen gehalten - dann prüft dieser Test nichts");

            TestSetup.RäumeAuf(spielwiese);

            Assert.False(Directory.Exists(spielwiese),
                "Die Spielwiese liegt noch da, und mit ihr eine Kopie der Zugdaten");
        }
    }
}
