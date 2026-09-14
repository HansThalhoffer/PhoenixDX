using PhoenixModel.Database;
using System.IO;
using System.Data;
using System.Globalization;
using System.Text;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Vergleicht zwei Zugdatenbanken Tabelle fuer Tabelle und schreibt das Ergebnis als Text.
    ///
    /// Gedacht ist das zum Verstehen der Altanwendung: legt man den Zug vor und nach der
    /// Auswertung der Spielleitung nebeneinander, steht im Bericht genau, was sie geaendert hat -
    /// und damit, was die neue Anwendung genauso tun muss.
    ///
    /// Verglichen wird ueber den Primaerschluessel der Tabelle. Hat eine Tabelle keinen, wird die
    /// ganze Zeile zum Schluessel; dann kann der Bericht nur sagen, dass eine Zeile hinzugekommen
    /// oder weggefallen ist, nicht welches Feld sich geaendert hat. Das steht dann auch so da.
    /// </summary>
    public static class Datenbankvergleich {

        /// <summary>
        /// Wieviele Einzelunterschiede je Tabelle ausgeschrieben werden. Der Rest wird gezaehlt.
        /// Ein Bericht, den niemand zu Ende liest, hilft nicht.
        /// </summary>
        public const int MaxZeilenJeTabelle = 40;

        /// <summary>Das Ergebnis eines Vergleichs</summary>
        /// <param name="Bericht">der Text, so wie er ausgegeben wird</param>
        /// <param name="Unterschiede">die Gesamtzahl der gefundenen Unterschiede</param>
        public record class Ergebnis(string Bericht, int Unterschiede);

        /// <summary>
        /// Vergleicht zwei Datenbanken. Es wird nur gelesen.
        /// </summary>
        /// <param name="dateiA">die aeltere Datenbank</param>
        /// <param name="dateiB">die neuere Datenbank</param>
        /// <param name="verschluesseltesPasswort">das Passwort, das fuer beide gilt</param>
        public static Ergebnis Vergleiche(string dateiA, string dateiB, PasswordHolder.EncryptedString verschluesseltesPasswort) {
            var bericht = new StringBuilder();
            int unterschiede = 0;

            bericht.AppendLine($"Vergleich zweier Datenbanken, erstellt am {DateTime.Now:yyyy-MM-dd HH:mm}");
            bericht.AppendLine($"  A: {dateiA}");
            bericht.AppendLine($"  B: {dateiB}");
            bericht.AppendLine();

            if (File.Exists(dateiA) == false || File.Exists(dateiB) == false) {
                bericht.AppendLine(File.Exists(dateiA) ? $"B fehlt: {dateiB}" : $"A fehlt: {dateiA}");
                return new Ergebnis(bericht.ToString(), 0);
            }

            string? klartext = new PasswordHolder(verschluesseltesPasswort).DecryptedPassword;
            using var a = new AccessDatabase(dateiA, klartext);
            using var b = new AccessDatabase(dateiB, klartext);
            if (a.Open() == false || b.Open() == false) {
                bericht.AppendLine("Mindestens eine der beiden Datenbanken liess sich nicht oeffnen.");
                return new Ergebnis(bericht.ToString(), 0);
            }

            try {
                var tabellenA = a.GetTabellennamen();
                var tabellenB = b.GetTabellennamen();

                foreach (var nurA in tabellenA.Except(tabellenB, StringComparer.OrdinalIgnoreCase)) {
                    bericht.AppendLine($"Tabelle {nurA}: nur in A vorhanden");
                    unterschiede++;
                }
                foreach (var nurB in tabellenB.Except(tabellenA, StringComparer.OrdinalIgnoreCase)) {
                    bericht.AppendLine($"Tabelle {nurB}: nur in B vorhanden");
                    unterschiede++;
                }
                if (unterschiede > 0)
                    bericht.AppendLine();

                foreach (var tabelle in tabellenA.Intersect(tabellenB, StringComparer.OrdinalIgnoreCase)) {
                    unterschiede += VergleicheTabelle(a, b, tabelle, bericht);
                }
            }
            catch (Exception ex) {
                bericht.AppendLine();
                bericht.AppendLine($"Der Vergleich wurde abgebrochen: {ex.Message}");
            }
            finally {
                a.Close();
                b.Close();
            }

            bericht.AppendLine();
            bericht.AppendLine(unterschiede == 0
                ? "Kein Unterschied gefunden."
                : $"{unterschiede} Unterschiede gefunden.");
            return new Ergebnis(bericht.ToString(), unterschiede);
        }

        /// <summary>
        /// Vergleicht eine einzelne Tabelle und haengt das Ergebnis an den Bericht an.
        /// </summary>
        /// <returns>die Zahl der Unterschiede in dieser Tabelle</returns>
        private static int VergleicheTabelle(AccessDatabase a, AccessDatabase b, string tabelle, StringBuilder bericht) {
            DataTable zeilenA = a.ExecuteQuery($"SELECT * FROM [{tabelle}]");
            DataTable zeilenB = b.ExecuteQuery($"SELECT * FROM [{tabelle}]");

            var schluessel = a.GetPrimärschluessel(tabelle);
            bool ohneSchluessel = schluessel.Count == 0;
            if (ohneSchluessel)
                schluessel = [.. zeilenA.Columns.Cast<DataColumn>().Select(spalte => spalte.ColumnName)];

            var spaltenA = zeilenA.Columns.Cast<DataColumn>().Select(spalte => spalte.ColumnName).ToList();
            var spaltenB = zeilenB.Columns.Cast<DataColumn>().Select(spalte => spalte.ColumnName).ToList();
            var gemeinsam = spaltenA.Intersect(spaltenB, StringComparer.OrdinalIgnoreCase).ToList();

            var indexA = Indiziere(zeilenA, schluessel);
            var indexB = Indiziere(zeilenB, schluessel);

            List<string> meldungen = [];
            foreach (var nurA in indexA.Keys.Except(indexB.Keys))
                meldungen.Add($"    nur in A: {nurA}");
            foreach (var nurB in indexB.Keys.Except(indexA.Keys))
                meldungen.Add($"    nur in B: {nurB}");

            if (ohneSchluessel == false) {
                foreach (var schluesselwert in indexA.Keys.Intersect(indexB.Keys)) {
                    var zeileA = indexA[schluesselwert];
                    var zeileB = indexB[schluesselwert];
                    List<string> felder = [];
                    foreach (var spalte in gemeinsam) {
                        string wertA = AlsText(zeileA[spalte]);
                        string wertB = AlsText(zeileB[spalte]);
                        if (wertA != wertB)
                            felder.Add($"{spalte}: '{wertA}' -> '{wertB}'");
                    }
                    if (felder.Count > 0)
                        meldungen.Add($"    {schluesselwert}: {string.Join(", ", felder)}");
                }
            }

            var nurInA = spaltenA.Except(spaltenB, StringComparer.OrdinalIgnoreCase).ToList();
            var nurInB = spaltenB.Except(spaltenA, StringComparer.OrdinalIgnoreCase).ToList();

            if (meldungen.Count == 0 && nurInA.Count == 0 && nurInB.Count == 0)
                return 0;

            bericht.AppendLine($"Tabelle {tabelle}  (A: {zeilenA.Rows.Count} Zeilen, B: {zeilenB.Rows.Count})");
            if (nurInA.Count > 0)
                bericht.AppendLine($"    Spalten nur in A: {string.Join(", ", nurInA)}");
            if (nurInB.Count > 0)
                bericht.AppendLine($"    Spalten nur in B: {string.Join(", ", nurInB)}");
            if (ohneSchluessel && meldungen.Count > 0)
                bericht.AppendLine("    (ohne Primaerschluessel - es laesst sich nur sagen, welche Zeilen fehlen oder dazugekommen sind)");

            foreach (var meldung in meldungen.Take(MaxZeilenJeTabelle))
                bericht.AppendLine(meldung);
            if (meldungen.Count > MaxZeilenJeTabelle)
                bericht.AppendLine($"    ... und {meldungen.Count - MaxZeilenJeTabelle} weitere");
            bericht.AppendLine();

            return meldungen.Count + nurInA.Count + nurInB.Count;
        }

        /// <summary>
        /// Legt die Zeilen einer Tabelle unter ihrem Schluesselwert ab. Doppelte Schluessel gibt es
        /// bei einem Primaerschluessel nicht; kommen sie doch vor, gewinnt die erste Zeile und der
        /// Rest taucht als Unterschied auf - das ist besser als eine Ausnahme.
        /// </summary>
        private static Dictionary<string, DataRow> Indiziere(DataTable tabelle, List<string> schluessel) {
            var index = new Dictionary<string, DataRow>();
            var vorhanden = schluessel
                .Where(spalte => tabelle.Columns.Contains(spalte))
                .ToList();
            foreach (DataRow zeile in tabelle.Rows) {
                string wert = string.Join(" | ", vorhanden.Select(spalte => $"{spalte}={AlsText(zeile[spalte])}"));
                index.TryAdd(wert, zeile);
            }
            return index;
        }

        /// <summary>
        /// Ein Feldwert als Text, unabhaengig von der Sprache des Rechners.
        ///
        /// Nachlaufende Leerzeichen fallen weg: Access liefert Textfelder oft aufgefuellt, und
        /// ein Bericht voller '  ' -> ' ' verdeckt die echten Unterschiede.
        /// </summary>
        private static string AlsText(object? wert) {
            if (wert == null || wert is DBNull)
                return string.Empty;
            if (wert is DateTime zeit)
                return zeit.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (wert is double zahl)
                return zahl.ToString("R", CultureInfo.InvariantCulture);
            return (Convert.ToString(wert, CultureInfo.InvariantCulture) ?? string.Empty).TrimEnd();
        }
    }
}
