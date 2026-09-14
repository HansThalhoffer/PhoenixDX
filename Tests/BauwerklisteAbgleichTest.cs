using PhoenixModel.Database;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using PhoenixWPF.Program;

namespace Tests {

    /// <summary>
    /// Der Abgleich der Bauwerkliste gegen die Karte - Issue #16, Teil 2 und 3.
    ///
    /// Die Karte ist die gepflegte Tabelle: dort steht, wem eine Gemark gehoert und wie das
    /// Bauwerk heisst. Die Bauwerkliste fuehrt beides noch einmal, und beides laeuft auseinander,
    /// sobald eine Gemark den Besitzer wechselt.
    ///
    /// Geschrieben wird hier nie in die echten Kartendaten: alle Tests laufen als Probelauf, der
    /// eine Test, der wirklich schreibt, gegen eine Kopie.
    /// </summary>
    public class BauwerklisteAbgleichTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadKarte(erzwingen: true);
        }

        /// <summary>Liest einen Wert der Bauwerkliste direkt aus der Datei</summary>
        private static string? LiesReich(string datei, int gf, int kf) {
            PasswordHolder.EncryptedString verschluesselt = TestSetup.KartenPasswort;
            string? klartext = new PasswordHolder(verschluesselt).DecryptedPassword;
            using var connector = new AccessDatabase(datei, klartext);
            Assert.True(connector.Open(), $"{datei} liess sich nicht oeffnen");
            try {
                using var befehl = connector.OpenDBCommand();
                befehl.CommandText = $"SELECT Reich FROM bauwerksliste WHERE gf = {gf} AND kf = {kf}";
                return befehl.ExecuteScalar()?.ToString();
            }
            finally {
                connector.Close();
            }
        }

        /// <summary>
        /// Was berichtigt wuerde, weicht wirklich ab - und zwar im Reich, nicht in der
        /// Schreibweise.
        /// </summary>
        [StaFact]
        public void WasBerichtigtWuerdeWeichtWirklichAb() {
            LadeAlles();
            var ergebnis = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

            foreach (var bezeichner in ergebnis.Berichtigt) {
                var gebäude = SharedData.Gebäude![bezeichner];
                var gemark = SharedData.Map![bezeichner];

                // die Karte muss ein echtes Reich oder einen Namen nennen
                bool kartenReich = gemark.Nation != null && gemark.Nation.Nummer != ReichTabelle.KeinReich;
                bool kartenName = string.IsNullOrEmpty(gemark.Bauwerknamen) == false;
                Assert.True(kartenReich || kartenName, $"{bezeichner} steht unter Berichtigt, die Karte sagt aber nichts");

                // und es muss ein Unterschied bestehen, der ueber die Schreibweise hinausgeht
                var listenNation = string.IsNullOrEmpty(gebäude.Reich)
                    ? null
                    : NationenView.GetNationFromString(gebäude.Reich);
                bool reichAnders = kartenReich && gemark.Nation != listenNation;
                bool nameAnders = kartenName && gemark.Bauwerknamen != (gebäude.Bauwerknamen ?? string.Empty);
                Assert.True(reichAnders || nameAnders, $"{bezeichner} weicht gar nicht ab");
            }
        }

        /// <summary>
        /// Dieselbe Nation in anderer Schreibweise ist keine Abweichung.
        ///
        /// In den echten Daten steht in der Bauwerkliste "Pirat", in der Karte "Choson" - beides
        /// ist Nummer 9. Ueber die Zeichenkette verglichen waeren das fuenf Aenderungen, die
        /// nichts aendern, aber die Schreibweise der Spielleitung ueberschreiben.
        /// </summary>
        [StaFact]
        public void EineAndereSchreibweiseDesselbenReichesIstKeineAbweichung() {
            LadeAlles();
            var vorher = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

            // ein Eintrag, der bisher nicht abweicht und dessen Gemark ein echtes Reich hat
            var unauffällig = SharedData.Gebäude!.Values.First(gebäude =>
                vorher.Berichtigt.Contains(gebäude.Bezeichner) == false
                && SharedData.Map!.TryGetValue(gebäude.Bezeichner, out var gemark)
                && gemark.Nation != null && gemark.Nation.Nummer != ReichTabelle.KeinReich);

            string? reichVorher = unauffällig.Reich;
            try {
                // andere Schreibweise, gleiches Reich
                unauffällig.Reich = reichVorher!.ToLowerInvariant();
                var nachher = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);
                Assert.DoesNotContain(unauffällig.Bezeichner, nachher.Berichtigt);

                // ein wirklich anderes Reich dagegen schon
                var anderes = SharedData.Nationen!.First(nation =>
                    nation.Nummer != ReichTabelle.KeinReich
                    && nation != SharedData.Map![unauffällig.Bezeichner].Nation);
                unauffällig.Reich = anderes.Reich;
                var mitFehler = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);
                Assert.Contains(unauffällig.Bezeichner, mitFehler.Berichtigt);
            }
            finally {
                unauffällig.Reich = reichVorher;
            }
        }

        /// <summary>
        /// Sagt die Karte "keinReich", wird nichts uebernommen.
        ///
        /// Nummer 0 ist kein Reich, sondern die Abwesenheit eines Reiches. In den echten Daten
        /// steht 806/27 in der Liste als Spielleitung und in der Karte als keinReich; das zu
        /// uebernehmen hiesse, eine Angabe durch eine Nichtangabe zu ersetzen.
        /// </summary>
        [StaFact]
        public void EinLeeresReichInDerKarteUeberschreibtNichts() {
            LadeAlles();
            var ergebnis = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

            foreach (var bezeichner in ergebnis.Berichtigt) {
                var gemark = SharedData.Map![bezeichner];
                if (gemark.Nation == null || gemark.Nation.Nummer == ReichTabelle.KeinReich)
                    // dann darf nur der Name der Grund sein, nicht das Reich
                    Assert.False(string.IsNullOrEmpty(gemark.Bauwerknamen),
                        $"{bezeichner} wird berichtigt, obwohl die Karte weder Reich noch Namen nennt");
            }

            // und was gar nichts sagt, steht unter OhneAngabeInDerKarte statt unter Berichtigt
            foreach (var bezeichner in ergebnis.OhneAngabeInDerKarte) {
                var gemark = SharedData.Map![bezeichner];
                Assert.True(gemark.Nation == null || gemark.Nation.Nummer == ReichTabelle.KeinReich);
                Assert.True(string.IsNullOrEmpty(gemark.Bauwerknamen));
                Assert.DoesNotContain(bezeichner, ergebnis.Berichtigt);
            }
        }

        /// <summary>
        /// Der Probelauf findet die Abweichungen und ruehrt die Datenbank nicht an.
        /// </summary>
        [StaFact]
        public void DerProbelaufBerichtigtNichts() {
            LadeAlles();
            var datei = new FileInfo(TestSetup.KartenPfad);
            var geschriebenVorher = datei.LastWriteTimeUtc;

            var ergebnis = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);

            Assert.Null(ergebnis.Sicherung);
            datei.Refresh();
            Assert.Equal(geschriebenVorher, datei.LastWriteTimeUtc);
        }

        /// <summary>
        /// Und der echte Lauf schreibt es auch wirklich - geprueft in der Datei, gegen eine Kopie.
        /// </summary>
        [StaFact]
        public void GegenEineKopieStehtDanachDasReichDerKarteInDerDatei() {
            LadeAlles();
            var vorschau = Kartenbereinigung.Bereinige(TestSetup.KartenPfad, "egal", nurAnzeigen: true);
            if (vorschau.Berichtigt.Count == 0)
                return; // die Daten stimmen ueberein, dann ist hier nichts zu zeigen

            string bezeichner = vorschau.Berichtigt[0];
            var gemark = SharedData.Map![bezeichner];
            string erwartet = gemark.Nation!.Reich;
            int gf = gemark.gf, kf = gemark.kf;

            string kopie = Path.Combine(Path.GetTempPath(), $"Abgleichkopie_{Guid.NewGuid():N}.mdb");
            File.Copy(TestSetup.KartenPfad, kopie);
            string? sicherung = null;
            try {
                Assert.NotEqual(erwartet, LiesReich(kopie, gf, kf));

                var ergebnis = Kartenbereinigung.Bereinige(kopie, TestSetup.KartenPasswort);
                sicherung = ergebnis.Sicherung;

                Assert.Contains(bezeichner, ergebnis.Berichtigt);
                Assert.Equal(erwartet, LiesReich(kopie, gf, kf));
                // in der Sicherung steht noch der alte Stand
                Assert.NotEqual(erwartet, LiesReich(sicherung!, gf, kf));
            }
            finally {
                File.Delete(kopie);
                if (sicherung != null && File.Exists(sicherung))
                    File.Delete(sicherung);
                // Bereinige hat die Eintraege im Speicher angepasst - wieder herstellen
                TestSetup.LoadKarte(erzwingen: true);
            }
        }
    }
}
