using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixModel.Database;
using PhoenixWPF.Database;
using PhoenixWPF.Program;

namespace Tests {

    /// <summary>
    /// Die Bereinigung der Bauwerkliste und ihr Schalter.
    ///
    /// Geloescht wird hier nie: alle Tests laufen als Probelauf. Die echte Kartendatenbank bleibt
    /// unberuehrt - genau das prueft einer der Tests auch nach.
    /// </summary>
    public class KartenbereinigungTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadKarte();
        }

        /// <summary>
        /// Der Schalter wird in allen Schreibweisen erkannt, die man auf einer Kommandozeile
        /// erwartet - und nur der Schalter.
        /// </summary>
        [Fact]
        public void DerSchalterWirdInAllenUeblichenSchreibweisenErkannt() {
            foreach (var schreibweise in new[] { "/cleanup", "-cleanup", "--cleanup", "/CleanUp", "  /cleanup  " })
                Assert.True(Kommandozeile.IstGesetzt([schreibweise], "cleanup"), schreibweise);

            foreach (var daneben in new[] { "cleanupx", "clean", "/cleanup2", "", "   " })
                Assert.False(Kommandozeile.IstGesetzt([daneben], "cleanup"), daneben);

            Assert.False(Kommandozeile.IstGesetzt(null, "cleanup"));
            Assert.False(Kommandozeile.IstGesetzt([], "cleanup"));
            // ohne Schalter passiert nichts - das ist der wichtigste Fall
            Assert.False(Kommandozeile.IstGesetzt([@"C:\irgendwo\datei.mdb"], "cleanup"));
        }

        /// <summary>
        /// Ein Probelauf findet die Karteileiche und ruehrt die Datenbank nicht an.
        /// </summary>
        [StaFact]
        public void DerProbelaufFindetKarteileichenUndSchreibtNichts() {
            LadeAlles();

            // Ein Gemark ohne Bauwerk, zu dem die Bauwerkliste keinen Eintrag fuehrt
            var leeresGemark = SharedData.Map!.Values.First(gemark =>
                gemark.Baupunkte == 0
                && (gemark.Ruestort == null || gemark.Ruestort == 0)
                && SharedData.Gebäude!.ContainsKey(gemark.Bezeichner) == false);

            // Genau so sieht eine Karteileiche aus: Eintrag in der Liste, nichts in der Karte
            var karteileiche = new Gebäude { gf = leeresGemark.gf, kf = leeresGemark.kf, Bauwerknamen = "Geisterburg" };
            Assert.True(SharedData.Gebäude!.TryAdd(karteileiche.Bezeichner, karteileiche));

            var datei = new FileInfo(TestSetup.KartenPfad);
            var geschriebenVorher = datei.LastWriteTimeUtc;
            var ordner = datei.Directory!;
            int sicherungenVorher = ordner.GetFiles("*_vor_Bereinigung_*.bak").Length;

            try {
                var ergebnis = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

                Assert.Contains(karteileiche.Bezeichner, ergebnis.Geloescht);
                // ein Probelauf sichert nicht und loescht nicht
                Assert.Null(ergebnis.Sicherung);
                Assert.True(SharedData.Gebäude.ContainsKey(karteileiche.Bezeichner),
                    "Der Probelauf hat den Eintrag tatsaechlich entfernt");

                datei.Refresh();
                Assert.Equal(geschriebenVorher, datei.LastWriteTimeUtc);
                Assert.Equal(sicherungenVorher, ordner.GetFiles("*_vor_Bereinigung_*.bak").Length);
            }
            finally {
                SharedData.Gebäude.TryRemove(karteileiche.Bezeichner, out _);
            }
        }

        /// <summary>
        /// Ein Ruestort ohne Baupunkte wird nicht geloescht, sondern ausdruecklich behalten. In den
        /// echten Kartendaten ist das 709/2 - dort fuehrt die Karte eine Burg.
        /// </summary>
        [StaFact]
        public void EinRuestortOhneBaupunkteWirdBehaltenUndNichtGeloescht() {
            LadeAlles();
            var ergebnis = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

            foreach (var bezeichner in ergebnis.Behalten) {
                Assert.DoesNotContain(bezeichner, ergebnis.Geloescht);
                var gemark = SharedData.Map![bezeichner];
                Assert.True(gemark.Ruestort > 0, $"{bezeichner} steht unter Behalten, fuehrt aber keinen Ruestort");
            }
            // und was geloescht wuerde, fuehrt die Karte wirklich nicht mehr
            foreach (var bezeichner in ergebnis.Geloescht) {
                var gemark = SharedData.Map![bezeichner];
                Assert.Equal(0, gemark.Baupunkte);
                Assert.True(gemark.Ruestort == null || gemark.Ruestort == 0);
            }
        }

        /// <summary>
        /// Die Sicherung liegt neben der Datenbank, traegt die Endung .bak - damit sie beim
        /// naechsten Start nicht als Kartendatenbank aufgegriffen wird - und ist Byte fuer Byte
        /// so gross wie das Original.
        /// </summary>
        [StaFact]
        public void DieSicherungLiegtNebenDerDatenbankUndIstVollstaendig() {
            string quelle = Path.Combine(Path.GetTempPath(), $"Bereinigungsprobe_{Guid.NewGuid():N}.mdb");
            File.WriteAllBytes(quelle, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            string? sicherung = null;
            try {
                sicherung = Kartenbereinigung.Sichere(quelle);

                Assert.True(File.Exists(sicherung));
                Assert.Equal(Path.GetDirectoryName(quelle), Path.GetDirectoryName(sicherung));
                Assert.EndsWith(".mdb.bak", sicherung);
                Assert.Contains("_vor_Bereinigung_", sicherung);
                Assert.Equal(File.ReadAllBytes(quelle), File.ReadAllBytes(sicherung));
            }
            finally {
                File.Delete(quelle);
                if (sicherung != null) File.Delete(sicherung);
            }
        }

        /// <summary>
        /// Zaehlt die Zeilen der Bauwerkliste zu einer Gemark - direkt in der Datei.
        ///
        /// Nicht ueber SharedData pruefen: ErkenfaraKarte.Load laesst die Reparatur mitlaufen, und
        /// die legt einen geloeschten Eintrag sofort wieder an, wenn die Karte dort ein Gebaeude
        /// fuehrt. Wer so prueft, misst durch die Reparatur hindurch und haelt ein erfolgreiches
        /// Loeschen fuer gescheitert.
        /// </summary>
        private static int ZaehleZeilen(string datei, int gf, int kf) {
            PasswordHolder.EncryptedString verschluesselt = TestSetup.KartenPasswort;
            string? klartext = new PasswordHolder(verschluesselt).DecryptedPassword;
            using var connector = new AccessDatabase(datei, klartext);
            Assert.True(connector.Open(), $"{datei} liess sich nicht oeffnen");
            try {
                using var befehl = connector.OpenDBCommand();
                befehl.CommandText = $"SELECT COUNT(*) FROM bauwerksliste WHERE gf = {gf} AND kf = {kf}";
                return Convert.ToInt32(befehl.ExecuteScalar());
            }
            finally {
                connector.Close();
            }
        }

        /// <summary>
        /// Der echte Loeschpfad - gegen eine Kopie, nie gegen die Kartendaten des Spielers.
        ///
        /// Damit es etwas zu loeschen gibt, wird ein vorhandener Eintrag kurzzeitig zur
        /// Karteileiche gemacht: sein Gemark fuehrt in der Karte weder Baupunkte noch Ruestort.
        /// Geprueft wird danach in der Datei selbst - die Zeile ist weg, und die Sicherung hat sie
        /// noch.
        /// </summary>
        [StaFact]
        public void GegenEineKopieWirdDieZeileWirklichGeloescht() {
            LadeAlles();

            var opfer = SharedData.Gebäude!.Values.First(gebäude =>
                SharedData.Map!.TryGetValue(gebäude.Bezeichner, out var gemark) && gemark.Baupunkte > 0);
            var gemarkDesOpfers = SharedData.Map![opfer.Bezeichner];
            int baupunkteVorher = gemarkDesOpfers.Baupunkte;
            int? ruestortVorher = gemarkDesOpfers.Ruestort;

            string kopie = Path.Combine(Path.GetTempPath(), $"Bereinigungskopie_{Guid.NewGuid():N}.mdb");
            File.Copy(TestSetup.KartenPfad, kopie);
            string? sicherung = null;
            try {
                Assert.Equal(1, ZaehleZeilen(kopie, opfer.gf, opfer.kf));

                gemarkDesOpfers.Baupunkte = 0;
                gemarkDesOpfers.Ruestort = 0;

                var ergebnis = Kartenbereinigung.Bereinige(kopie, TestSetup.KartenPasswort);
                sicherung = ergebnis.Sicherung;

                Assert.Contains(opfer.Bezeichner, ergebnis.Geloescht);
                Assert.NotNull(sicherung);
                Assert.True(File.Exists(sicherung));
                // die echten Kartendaten sind nicht angefasst worden
                Assert.NotEqual(Path.GetDirectoryName(TestSetup.KartenPfad), Path.GetDirectoryName(sicherung));

                // der Beweis in der Datei selbst
                Assert.Equal(0, ZaehleZeilen(kopie, opfer.gf, opfer.kf));
                // und in der Sicherung steht die Zeile noch - genau dafuer ist sie da
                Assert.Equal(1, ZaehleZeilen(sicherung!, opfer.gf, opfer.kf));
            }
            finally {
                gemarkDesOpfers.Baupunkte = baupunkteVorher;
                gemarkDesOpfers.Ruestort = ruestortVorher;
                File.Delete(kopie);
                if (sicherung != null && File.Exists(sicherung))
                    File.Delete(sicherung);
                // Bereinige hat den Eintrag aus der geteilten Liste genommen - wieder herstellen
                TestSetup.LoadKarte(erzwingen: true);
            }
        }
    }
}
