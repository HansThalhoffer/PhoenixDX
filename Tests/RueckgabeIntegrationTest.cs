using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Der Rueckweg von der Spielleitung.
    ///
    /// Normalfall ist die Netzwerkfreigabe, Notnagel ein Archiv vom Datentraeger. Beides ist
    /// derselbe Weg - es wird ein Verzeichnis durchsucht - und laesst sich deshalb vollstaendig
    /// ohne Netz pruefen.
    ///
    /// Die echten Spieldaten werden nicht angefasst: gearbeitet wird auf einer Kopie im
    /// Temp-Verzeichnis.
    /// </summary>
    public class RueckgabeIntegrationTest {

        private const string Reich = "Theostelos";

        /// <summary>
        /// Legt eine Spielwiese an und liefert ihr Wurzelverzeichnis
        /// </summary>
        private static string ErstelleSpielwiese()
            => Path.Combine(Path.GetTempPath(), "PhoenixDX_Rueckgabe_Test_" + Guid.NewGuid().ToString("N"));

        /// <summary>
        /// Eine Kopie der echten Zugdatenbank, damit etwas Echtes durch den Weg geschickt wird
        /// </summary>
        private static string KopiereZugdatenbank(string verzeichnis, string dateiname) {
            Directory.CreateDirectory(verzeichnis);
            string ziel = Path.Combine(verzeichnis, dateiname);
            File.Copy(TestSetup.ZugdatenPfad, ziel);
            return ziel;
        }

        [Fact]
        public void EineFreigabeLiefertIhrenRechnernamen() {
            Assert.Equal("192.168.1.66", SpielleitungsRückgabe.GetRechnername(@"\\192.168.1.66\PZEData"));
            Assert.Equal("spielleitung", SpielleitungsRückgabe.GetRechnername(@"\\spielleitung\PZEData\Unterordner"));
            Assert.Null(SpielleitungsRückgabe.GetRechnername(@"C:\lokal\kein\unc"));
            Assert.Null(SpielleitungsRückgabe.GetRechnername(null));
            Assert.Null(SpielleitungsRückgabe.GetRechnername(string.Empty));
        }

        /// <summary>
        /// Was kein UNC-Pfad ist, gilt sofort als nicht erreichbar - ohne erst ins Netz zu greifen.
        /// </summary>
        [Fact]
        public void EinUnbrauchbarerServerpfadGiltAlsNichtErreichbar() {
            Assert.False(SpielleitungsRückgabe.IstErreichbar(@"C:\kein\server", out string fehler));
            Assert.Contains("Rechnername", fehler);
        }

        [Fact]
        public void DasSerververzeichnisFolgtDemAufbauDerAltanwendung() {
            string verzeichnis = SpielleitungsRückgabe.BestimmeSerververzeichnis(@"\\192.168.1.66\PZEData", Reich, 171);
            Assert.Equal(@"\\192.168.1.66\Theostelos\171", verzeichnis);
        }

        /// <summary>
        /// Der Normalfall: die Spielleitung hat die Datenbank abgelegt.
        /// </summary>
        [StaFact]
        public void DerZugWirdAlsDatenbankGeholt() {
            TestSetup.Setup();
            string spielwiese = ErstelleSpielwiese();
            try {
                string quelle = Path.Combine(spielwiese, "server", "171");
                KopiereZugdatenbank(quelle, $"{Reich}.mdb");
                string ziel = Path.Combine(spielwiese, "lokal", "171", $"{Reich}.mdb");

                var ergebnis = SpielleitungsRückgabe.HoleZug(quelle, Reich, 171, ziel);

                Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
                Assert.Equal(Rückgabequelle.Datenbank, ergebnis.Quelle);
                Assert.True(File.Exists(ziel), $"{ziel} wurde nicht angelegt");
                Assert.Equal(new FileInfo(TestSetup.ZugdatenPfad).Length, new FileInfo(ziel).Length);
            }
            finally {
                try { Directory.Delete(spielwiese, true); } catch { /* liegt im Temp */ }
            }
        }

        /// <summary>
        /// Der Notnagel: kein Netz, die Spielleitung gibt ein Archiv heraus. Das Archiv wird hier
        /// mit derselben Uebergabe erzeugt, die auch die Zugabgabe benutzt - damit ist zugleich
        /// geprueft, dass Hin- und Rueckweg zueinander passen.
        /// </summary>
        [StaFact]
        public void DerZugWirdAusDemArchivGeholtWennKeineDatenbankDaLiegt() {
            TestSetup.Setup();
            string spielwiese = ErstelleSpielwiese();
            try {
                // ein Archiv erzeugen, so wie es die Spielleitung bekommt
                string zugverzeichnis = Path.Combine(spielwiese, "_Data", "Zugdaten", "171");
                string datenbank = KopiereZugdatenbank(zugverzeichnis, $"{Reich}.mdb");
                var paket = SpielleitungsUebergabe.ErstelleZugpaket(datenbank, 171);
                Assert.True(paket.Erfolgreich, $"{paket.Meldung} {paket.Details}");

                // nur das Archiv liegt im Uebergabeverzeichnis, keine Datenbank
                string quelle = Path.GetDirectoryName(paket.Archiv)!;
                Assert.Empty(Directory.EnumerateFiles(quelle, "*.mdb"));

                string ziel = Path.Combine(spielwiese, "lokal", "171", $"{Reich}.mdb");
                var ergebnis = SpielleitungsRückgabe.HoleZug(quelle, Reich, 171, ziel);

                Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
                Assert.Equal(Rückgabequelle.Archiv, ergebnis.Quelle);
                Assert.True(File.Exists(ziel), $"{ziel} wurde nicht angelegt");
                Assert.Equal(new FileInfo(datenbank).Length, new FileInfo(ziel).Length);
            }
            finally {
                try { Directory.Delete(spielwiese, true); } catch { /* liegt im Temp */ }
            }
        }

        /// <summary>
        /// Lokal begonnene Arbeit darf nicht stillschweigend ueberschrieben werden.
        /// </summary>
        [StaFact]
        public void VorhandeneZugdatenWerdenNichtUngefragtErsetzt() {
            TestSetup.Setup();
            string spielwiese = ErstelleSpielwiese();
            try {
                string quelle = Path.Combine(spielwiese, "server", "171");
                KopiereZugdatenbank(quelle, $"{Reich}.mdb");

                string lokal = Path.Combine(spielwiese, "lokal", "171");
                Directory.CreateDirectory(lokal);
                string ziel = Path.Combine(lokal, $"{Reich}.mdb");
                File.WriteAllText(ziel, "hier steht schon etwas");

                var abgelehnt = SpielleitungsRückgabe.HoleZug(quelle, Reich, 171, ziel);
                Assert.False(abgelehnt.Erfolgreich);
                Assert.Contains("bereits", abgelehnt.Meldung);
                Assert.Equal("hier steht schon etwas", File.ReadAllText(ziel));

                var erlaubt = SpielleitungsRückgabe.HoleZug(quelle, Reich, 171, ziel, überschreiben: true);
                Assert.True(erlaubt.Erfolgreich, $"{erlaubt.Meldung} {erlaubt.Details}");
                Assert.Equal(new FileInfo(TestSetup.ZugdatenPfad).Length, new FileInfo(ziel).Length);
            }
            finally {
                try { Directory.Delete(spielwiese, true); } catch { /* liegt im Temp */ }
            }
        }

        [StaFact]
        public void EinLeeresVerzeichnisWirdVerstaendlichGemeldet() {
            TestSetup.Setup();
            string spielwiese = ErstelleSpielwiese();
            try {
                string quelle = Path.Combine(spielwiese, "server", "171");
                Directory.CreateDirectory(quelle);
                string ziel = Path.Combine(spielwiese, "lokal", "171", $"{Reich}.mdb");

                var ergebnis = SpielleitungsRückgabe.HoleZug(quelle, Reich, 171, ziel);
                Assert.False(ergebnis.Erfolgreich);
                Assert.Contains(Reich, ergebnis.Meldung);
                Assert.False(File.Exists(ziel));
            }
            finally {
                try { Directory.Delete(spielwiese, true); } catch { /* liegt im Temp */ }
            }
        }
    }
}
