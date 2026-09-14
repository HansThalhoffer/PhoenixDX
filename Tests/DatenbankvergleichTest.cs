using PhoenixModel.Database;
using PhoenixWPF.Database;

namespace Tests {

    /// <summary>
    /// Der Vergleich zweier Datenbanken - Issue #11.
    ///
    /// Geprueft wird gegen Kopien: die Kartendatenbank wird zweimal ins Temp-Verzeichnis kopiert,
    /// eine davon gezielt veraendert. Die echten Daten des Spielers werden nur gelesen.
    /// </summary>
    public class DatenbankvergleichTest : IDisposable {

        private readonly List<string> _kopien = [];

        private string Kopiere() {
            string ziel = Path.Combine(Path.GetTempPath(), $"Vergleich_{Guid.NewGuid():N}.mdb");
            File.Copy(TestSetup.KartenPfad, ziel);
            _kopien.Add(ziel);
            return ziel;
        }

        public void Dispose() {
            foreach (var kopie in _kopien) {
                try { File.Delete(kopie); } catch { /* liegt im Temp */ }
            }
            GC.SuppressFinalize(this);
        }

        private static PasswordHolder.EncryptedString Passwort => TestSetup.KartenPasswort;

        /// <summary>
        /// Zwei gleiche Datenbanken haben keinen Unterschied. Faellt dieser Test, meldet der
        /// Vergleich Rauschen - und ein Bericht voller Rauschen ist wertlos.
        /// </summary>
        [StaFact]
        public void ZweiGleicheDatenbankenHabenKeinenUnterschied() {
            TestSetup.Setup();
            string a = Kopiere();
            string b = Kopiere();

            var ergebnis = Datenbankvergleich.Vergleiche(a, b, Passwort);

            Assert.Equal(0, ergebnis.Unterschiede);
            Assert.Contains("Kein Unterschied gefunden", ergebnis.Bericht);
        }

        /// <summary>
        /// Ein geaendertes Feld taucht im Bericht auf, mit Tabelle, Schluessel, altem und neuem Wert.
        /// </summary>
        [StaFact]
        public void EinGeaendertesFeldStehtImBericht() {
            TestSetup.Setup();
            string a = Kopiere();
            string b = Kopiere();

            // eine Tabelle mit Primaerschluessel und Zeilen suchen und dort ein Feld veraendern
            string? tabelle = null, schluesselspalte = null, textspalte = null;
            object? schluesselwert = null;
            string? klartext = new PasswordHolder(Passwort).DecryptedPassword;
            using (var db = new AccessDatabase(b, klartext)) {
                Assert.True(db.Open(), "Die Kopie liess sich nicht oeffnen");
                foreach (var name in db.GetTabellennamen()) {
                    var schluessel = db.GetPrimärschluessel(name);
                    if (schluessel.Count != 1)
                        continue;
                    var daten = db.ExecuteQuery($"SELECT * FROM [{name}]");
                    if (daten.Rows.Count == 0)
                        continue;
                    // eine Textspalte, die nicht zum Schluessel gehoert
                    foreach (System.Data.DataColumn spalte in daten.Columns) {
                        if (spalte.DataType == typeof(string) && spalte.ColumnName != schluessel[0]) {
                            textspalte = spalte.ColumnName;
                            break;
                        }
                    }
                    if (textspalte == null)
                        continue;
                    tabelle = name;
                    schluesselspalte = schluessel[0];
                    schluesselwert = daten.Rows[0][schluessel[0]];
                    break;
                }

                Assert.True(tabelle != null, "Keine Tabelle mit einfachem Primaerschluessel und Textspalte gefunden");
                db.ExecuteNonQuery($"UPDATE [{tabelle}] SET [{textspalte}] = 'VERGLEICHSPROBE' "
                    + $"WHERE [{schluesselspalte}] = {Wert(schluesselwert)}");
                db.Close();
            }

            var ergebnis = Datenbankvergleich.Vergleiche(a, b, Passwort);

            Assert.True(ergebnis.Unterschiede > 0, "Die Aenderung wurde nicht gefunden");
            Assert.Contains($"Tabelle {tabelle}", ergebnis.Bericht);
            Assert.Contains("VERGLEICHSPROBE", ergebnis.Bericht);
            Assert.Contains(textspalte!, ergebnis.Bericht);
        }

        /// <summary>
        /// Eine geloeschte Zeile steht als "nur in A" im Bericht.
        /// </summary>
        [StaFact]
        public void EineGeloeschteZeileStehtAlsNurInA() {
            TestSetup.Setup();
            string a = Kopiere();
            string b = Kopiere();

            string? klartext = new PasswordHolder(Passwort).DecryptedPassword;
            using (var db = new AccessDatabase(b, klartext)) {
                Assert.True(db.Open());
                // die Bauwerkliste hat immer Zeilen und laesst sich gefahrlos in der Kopie kuerzen
                int vorher = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM bauwerksliste"));
                Assert.True(vorher > 0);
                db.ExecuteNonQuery("DELETE FROM bauwerksliste WHERE gf = (SELECT MIN(gf) FROM bauwerksliste)");
                db.Close();
            }

            var ergebnis = Datenbankvergleich.Vergleiche(a, b, Passwort);

            Assert.True(ergebnis.Unterschiede > 0);
            Assert.Contains("bauwerksliste", ergebnis.Bericht);
            Assert.Contains("nur in A", ergebnis.Bericht);
        }

        /// <summary>
        /// Eine fehlende Datei ergibt einen Bericht, keine Ausnahme.
        /// </summary>
        [StaFact]
        public void EineFehlendeDateiErgibtEinenBericht() {
            TestSetup.Setup();
            string gibtesnicht = Path.Combine(Path.GetTempPath(), $"GibtEsNicht_{Guid.NewGuid():N}.mdb");

            var ergebnis = Datenbankvergleich.Vergleiche(TestSetup.KartenPfad, gibtesnicht, Passwort);

            Assert.Equal(0, ergebnis.Unterschiede);
            Assert.Contains("fehlt", ergebnis.Bericht);
            Assert.Contains(gibtesnicht, ergebnis.Bericht);
        }

        /// <summary>Ein Schluesselwert so, wie ihn eine SQL-Bedingung braucht</summary>
        private static string Wert(object? wert) {
            if (wert is string text)
                return $"'{text.Replace("'", "''")}'";
            return Convert.ToString(wert, System.Globalization.CultureInfo.InvariantCulture) ?? "0";
        }
    }
}
