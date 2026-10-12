using PhoenixModel.Database;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Ein Speicherdurchgang steht in einer Transaktion: er bleibt ganz oder gar nicht.
    ///
    /// <b>Worum es geht.</b> Der ACE-Treiber beendet den Prozess gelegentlich mitten im Schreiben,
    /// ohne eine Ausnahme zu werfen - kein <c>catch</c> greift, keine Meldung erscheint. Bis hierher
    /// stand danach die halbe Hälfte eines Durchgangs in der Datei: ein Zustand, den niemand
    /// gewollt hat und den hinterher niemand auseinandersortiert. Die ungeklärten 18.000 GS aus
    /// Zug 169 (Offene-Regelfragen.md, Punkt 9) sehen genau so aus.
    ///
    /// <b>Was hier geprüft wird und was nicht.</b> Der Abbruch selbst lässt sich nicht nachstellen -
    /// er beendet den Prozess und damit den Testlauf. Prüfbar ist die Voraussetzung, auf der alles
    /// beruht: dass Access überhaupt eine Transaktion annimmt und ein Zurückrollen wirklich
    /// zurückrollt. Hielte es das nicht ein, wäre die Klammer eine Beruhigung ohne Wirkung - und
    /// das wäre schlimmer als keine Klammer, weil sich niemand mehr darum kümmern würde.
    ///
    /// Geschrieben wird ausschliesslich in eine Kopie; die echten Spieldaten bleiben unberührt.
    /// </summary>
    public class SchreibdurchgangTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        private static string ErstelleKopie(out string spielwiese) {
            spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Durchgang_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(spielwiese);
            string ziel = Path.Combine(spielwiese, Path.GetFileName(TestSetup.ZugdatenPfad));
            File.Copy(TestSetup.ZugdatenPfad, ziel);
            return ziel;
        }

        /// <summary>
        /// Schreibt einen Vermerk in die Kopie und schliesst den Durchgang so ab, wie es der
        /// Parameter sagt.
        /// </summary>
        /// <returns>die Nummer des Zauberers, dessen Vermerk geschrieben wurde</returns>
        private static int SchreibeVermerk(string kopie, string vermerk, bool bestätigen) {
            var zauberer = SharedData.Zauberer!.First(z => Plausibilität.IsValid(z));
            int nummer = zauberer.Nummer;
            string vorher = zauberer.sonstiges;

            // Die Fabrik will das Passwort im Klartext; in den Einstellungen liegt es verschlüsselt.
            PasswordHolder holder = new(TestSetup.ZugdatenPasswort);
            using IDatenbank db = Datenbankfabrik.Erzeuge(kopie, holder.DecryptedPassword);
            Assert.True(db.Open(), "Die Kopie liess sich nicht öffnen");
            var transaktion = db.BeginneTransaktion();
            Assert.NotNull(transaktion);
            try {
                using var command = db.OpenDBCommand();
                command.Transaction = transaktion;
                zauberer.sonstiges = vermerk;
                zauberer.Save(command);
                if (bestätigen)
                    transaktion.Commit();
                else
                    transaktion.Rollback();
            }
            finally {
                transaktion.Dispose();
                zauberer.sonstiges = vorher;
                db.Close();
            }
            return nummer;
        }

        private static string LiesVermerk(string kopie, int nummer) {
            Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, ProgramView.SelectedMonth));
            var neu = SharedData.Zauberer!.FirstOrDefault(z => z.Nummer == nummer);
            Assert.True(neu != null, $"Zauberer {nummer} fehlt in der Kopie");
            return neu!.sonstiges;
        }

        /// <summary>
        /// Ein zurückgerollter Durchgang hinterlässt nichts. Das ist der Fall, auf den es ankommt:
        /// genauso endet ein Durchgang, in dessen Mitte der Treiber den Prozess abreisst.
        /// </summary>
        [StaFact]
        public void EinZurueckgerollterDurchgangHinterlaesstNichts() {
            LadeAlles();
            string kopie = ErstelleKopie(out string spielwiese);
            try {
                int nummer = SchreibeVermerk(kopie, "darf nicht stehenbleiben", bestätigen: false);
                Assert.NotEqual("darf nicht stehenbleiben", LiesVermerk(kopie, nummer));
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Die Gegenprobe. Ohne sie bewiese der Test oben nur, dass nichts geschrieben wird - und
        /// das wäre er auch, wenn die Transaktion gar nichts täte oder der Vermerk nie ankäme.
        /// </summary>
        [StaFact]
        public void EinBestaetigterDurchgangBleibtStehen() {
            LadeAlles();
            string kopie = ErstelleKopie(out string spielwiese);
            try {
                int nummer = SchreibeVermerk(kopie, "bleibt stehen", bestätigen: true);
                Assert.Equal("bleibt stehen", LiesVermerk(kopie, nummer));
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }
    }
}
