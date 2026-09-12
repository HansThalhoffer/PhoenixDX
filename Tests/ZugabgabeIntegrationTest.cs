using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Fährt eine komplette Zugabgabe gegen eine Kopie der echten Zugdaten.
    ///
    /// Der Test fasst die echten Spieldaten nicht an: er kopiert die Zugdatenbank in ein eigenes
    /// Verzeichnis unterhalb des Temp-Ordners und gibt dort ab. Am Ende wird wieder auf die echte
    /// Datenbank umgestellt, damit die folgenden Tests nicht auf der Kopie weiterarbeiten.
    /// </summary>
    public class ZugabgabeIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Legt eine Spielwiese an: ein eigenes Datenverzeichnis mit derselben Struktur wie die
        /// echten Spieldaten und einer Kopie der Datenbank des laufenden Zuges. Die Struktur muss
        /// stimmen, weil die Zugabgabe daraus den Ablageort des Folgezuges und den der Übergabe an
        /// die Spielleitung ableitet.
        /// </summary>
        private static string ErstelleKopie(int zug) {
            string quelle = TestSetup.ZugdatenPfad;
            string spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Zugabgabe_" + Guid.NewGuid().ToString("N"));
            string verzeichnis = Path.Combine(spielwiese, "_Data", "Zugdaten", zug.ToString());
            Directory.CreateDirectory(verzeichnis);
            string ziel = Path.Combine(verzeichnis, Path.GetFileName(quelle));
            File.Copy(quelle, ziel);
            return ziel;
        }

        /// <summary>
        /// Das Wurzelverzeichnis der Spielwiese zu einer Kopie, zum Aufräumen
        /// </summary>
        private static string GetSpielwiese(string kopie)
            => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(kopie)!, "..", "..", ".."));

        /// <summary>
        /// Packt das Archiv wieder aus und prüft, dass die erwartete Datenbank drin ist und sich
        /// mit dem vereinbarten Passwort auch entschlüsseln lässt. Ein Archiv, das die Spielleitung
        /// nicht öffnen kann, ist keine Zugabgabe.
        /// </summary>
        private static void PruefeArchiv(string archiv, string erwarteteDatei) {
            string ausgepackt = Path.Combine(Path.GetDirectoryName(archiv)!, "Probe");
            Directory.CreateDirectory(ausgepackt);
            using (var zip = Ionic.Zip.ZipFile.Read(archiv)) {
                var eintrag = zip.Entries.FirstOrDefault(e => e.FileName.EndsWith(erwarteteDatei, StringComparison.OrdinalIgnoreCase));
                Assert.True(eintrag != null, $"{erwarteteDatei} steckt nicht in {archiv}");
                Assert.True(eintrag!.UsesEncryption, "Das Archiv ist unverschlüsselt");
                eintrag.Password = PhoenixWPF.Database.PasswortProvider.ZipPasswort;
                eintrag.Extract(ausgepackt, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
            }
            string datei = Path.Combine(ausgepackt, erwarteteDatei);
            Assert.True(File.Exists(datei), $"{erwarteteDatei} liess sich nicht aus {archiv} auspacken");
            Assert.True(new FileInfo(datei).Length > 0, "Die ausgepackte Datenbank ist leer");
        }

        [StaFact]
        public void ZugabgabeLegtDenFolgezugAn() {
            LadeAlles();
            int zug = ZugView.AktuellerZug.Zug;
            int folgeZug = zug + 1;

            string kopie = ErstelleKopie(zug);
            string spielwiese = GetSpielwiese(kopie);
            try {
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug), $"Die Kopie {kopie} liess sich nicht laden");

                // den Stand vor der Abgabe festhalten
                var vorher = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                int anzahlVorher = vorher.Count;
                int aufgelösteVorher = vorher.Count(ZugendeRules.IstAufgelöst);
                var zielpositionen = vorher.Where(f => ZugendeRules.IstAufgelöst(f) == false)
                                           .ToDictionary(f => f.Nummer, f => (f.gf_nach, f.kf_nach));
                Assert.True(anzahlVorher > 0, "Für den Test braucht es Figuren");

                var ergebnis = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
                Assert.NotNull(ergebnis.NeueDatenbank);
                Assert.True(File.Exists(ergebnis.NeueDatenbank), $"{ergebnis.NeueDatenbank} wurde nicht angelegt");
                Assert.Equal(anzahlVorher - aufgelösteVorher, ergebnis.ÜbernommeneFiguren);

                // das Paket für die Spielleitung liegt bereit und enthält den abgegebenen Zug
                Assert.NotNull(ergebnis.ÜbergabeArchiv);
                Assert.True(File.Exists(ergebnis.ÜbergabeArchiv), $"{ergebnis.ÜbergabeArchiv} wurde nicht angelegt");
                Assert.EndsWith($"_{zug}.zip", ergebnis.ÜbergabeArchiv);
                PruefeArchiv(ergebnis.ÜbergabeArchiv!, Path.GetFileName(kopie));

                // die Quelle bleibt als Archiv erhalten und ist als abgegeben markiert
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);

                // und nun die frisch angelegte Datenbank des Folgezuges
                Assert.True(TestSetup.LadeZugdatenAusDatei(ergebnis.NeueDatenbank!, folgeZug));

                var settings = ZugView.Settings;
                Assert.NotNull(settings);
                Assert.Equal(folgeZug, settings!.Monat);
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);

                // die Rüstung des Vorzuges ist bezahlt und abgeräumt
                Assert.Empty(SharedData.Ruestung!);
                Assert.Empty(SharedData.RuestungBauwerke!);
                Assert.Empty(SharedData.RuestungRuestorte!);

                // der Reichsschatz des Folgemonats ist die Bilanz des abgegebenen Monats
                var abgerechnet = SchatzkammerRules.GetMonat(zug);
                var neu = SchatzkammerRules.GetMonat(folgeZug);
                Assert.NotNull(abgerechnet);
                Assert.NotNull(neu);
                Assert.Equal(SchatzkammerRules.BerechneBilanz(abgerechnet), neu!.Reichschatz);
                Assert.Equal(ergebnis.NeuerReichsschatz, neu.Reichschatz);
                Assert.Equal(0, neu.Verruestet);
                Assert.Equal(0, neu.schenkung_getaetigt);
                Assert.Equal(0, neu.schenkung_bekommen);

                // die Figuren stehen da, wo sie im Vorzug angekommen sind, und haben wieder
                // volle Bewegungspunkte
                var nachher = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                Assert.Equal(ergebnis.ÜbernommeneFiguren, nachher.Count);
                foreach (var figur in nachher) {
                    Assert.True(zielpositionen.TryGetValue(figur.Nummer, out var ziel),
                        $"Figur {figur.Nummer} taucht im Folgezug auf, war aber vorher nicht da");
                    Assert.True(figur.gf_von == ziel.gf_nach && figur.kf_von == ziel.kf_nach,
                        $"Figur {figur.Nummer} startet auf {figur.gf_von}/{figur.kf_von}, "
                        + $"angekommen war sie aber auf {ziel.gf_nach}/{ziel.kf_nach}");
                    Assert.True(figur.gf_von > 0, $"Figur {figur.Nummer} ist von der Karte gefallen");
                }
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Das Rüstungsarchiv muss dort landen, wo die Anwendung beim Aufbau der Rüstungshistorie
        /// danach sucht: im Zugverzeichnis, als Ruestung_&lt;Reich&gt;_&lt;Zug&gt;.zip.
        /// </summary>
        [StaFact]
        public void RuestungspaketLiegtImZugverzeichnis() {
            LadeAlles();
            int zug = ZugView.AktuellerZug.Zug;

            string kopie = ErstelleKopie(zug);
            string spielwiese = GetSpielwiese(kopie);
            try {
                var ergebnis = SpielleitungsUebergabe.ErstelleRüstungspaket(kopie, zug);
                Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
                Assert.NotNull(ergebnis.Archiv);

                Assert.Equal(Path.GetDirectoryName(kopie), Path.GetDirectoryName(ergebnis.Archiv));
                Assert.StartsWith("Ruestung_", Path.GetFileName(ergebnis.Archiv)!);
                Assert.EndsWith($"_{zug}.zip", ergebnis.Archiv);

                // die Anwendung sucht mit genau diesem Muster danach
                Assert.NotNull(Directory.EnumerateFiles(Path.GetDirectoryName(kopie)!, "Ruestung_*.zip").FirstOrDefault());
                PruefeArchiv(ergebnis.Archiv!, Path.GetFileName(kopie));
            }
            finally {
                TestSetup.RäumeAuf(spielwiese);
            }
        }

        /// <summary>
        /// Ein zweites Abgeben desselben Zuges darf die bereits begonnene Arbeit im Folgezug nicht
        /// stillschweigend wegwerfen.
        /// </summary>
        [StaFact]
        public void ZugabgabeUeberschreibtDenFolgezugNichtUngefragt() {
            LadeAlles();
            int zug = ZugView.AktuellerZug.Zug;

            string kopie = ErstelleKopie(zug);
            string spielwiese = GetSpielwiese(kopie);
            try {
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.True(Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug).Erfolgreich);

                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.True(Zugabgabe.ZielExistiert(kopie, zug + 1));

                // die erste Sperre ist die Phase: der Zug ist als abgegeben vermerkt
                var zweiterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.False(zweiterVersuch.Erfolgreich);
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);

                // die zweite Sperre ist die vorhandene Datei. Sie greift auch dann, wenn die
                // Spielleitung den Zug wieder geöffnet hat, weil im Folgezug schon gearbeitet
                // worden sein kann.
                ZugView.Settings!.Phase = (int)Zugphase.Bewegungsphase;
                var dritterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.False(dritterVersuch.Erfolgreich);
                Assert.Contains("gibt es bereits", dritterVersuch.Meldung);

                // mit ausdrücklicher Zustimmung geht es dann doch
                var vierterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug, zielÜberschreiben: true);
                Assert.True(vierterVersuch.Erfolgreich, $"{vierterVersuch.Meldung} {vierterVersuch.Details}");
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                TestSetup.RäumeAuf(spielwiese);
            }
        }
    }
}
