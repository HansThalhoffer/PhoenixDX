using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixWPF.Program {

    /// <summary>
    /// Raeumt in der Bauwerkliste der Erkenfarakarte.mdb auf.
    ///
    /// Die Karte ist die gepflegte Tabelle. Fuehrt sie zu einem Eintrag der Bauwerkliste weder
    /// Baupunkte noch einen Ruestort, steht dort nichts mehr - der Eintrag ist eine Karteileiche.
    /// Die Anwendung meldet solche Eintraege bei jedem Start, entfernt sie aber von sich aus nicht:
    /// das Loeschen ist nicht rueckgaengig zu machen, und die Datei kommt von der Spielleitung.
    ///
    /// Deshalb laeuft die Bereinigung nur, wenn sie ausdruecklich angefordert wird - beim Start mit
    /// dem Schalter /cleanup. Vorher wird die Datenbank gesichert.
    ///
    /// Nicht angetastet wird ein Gemark, das zwar keine Baupunkte, aber noch einen Ruestort fuehrt.
    /// Dort steht laut Karte weiter ein Bauwerk; das ist ein Widerspruch in der Karte und nichts,
    /// was sich durch Loeschen aufloesen liesse.
    /// </summary>
    public static class Kartenbereinigung {

        /// <summary>
        /// Was die Bereinigung getan hat.
        /// </summary>
        /// <param name="Sicherung">der Pfad der Sicherung, oder null, wenn nichts zu tun war</param>
        /// <param name="Geloescht">die entfernten Eintraege - bei einem Probelauf die, die entfernt wuerden</param>
        /// <param name="Behalten">Eintraege mit Ruestort ohne Baupunkte, die absichtlich stehen bleiben</param>
        public record class Ergebnis(string? Sicherung, List<string> Geloescht, List<string> Behalten);

        /// <summary>
        /// Legt eine Sicherung der Datenbank neben die Datei.
        ///
        /// Eine Sicherung, die man nicht aufmachen kann, ist keine - deshalb wird die Groesse
        /// verglichen. Die Endung .bak verhindert, dass die Kopie beim naechsten Start als
        /// Kartendatenbank aufgegriffen wird.
        /// </summary>
        /// <returns>der Pfad der Sicherung</returns>
        public static string Sichere(string datenbankpfad) {
            string ordner = System.IO.Path.GetDirectoryName(datenbankpfad) ?? ".";
            string name = System.IO.Path.GetFileNameWithoutExtension(datenbankpfad);
            string ziel = System.IO.Path.Combine(ordner,
                $"{name}_vor_Bereinigung_{DateTime.Now:yyyyMMdd_HHmmss}.mdb.bak");

            System.IO.File.Copy(datenbankpfad, ziel, overwrite: false);
            long quelle = new System.IO.FileInfo(datenbankpfad).Length;
            long kopie = new System.IO.FileInfo(ziel).Length;
            if (quelle != kopie)
                throw new System.IO.IOException(
                    $"Die Sicherung {ziel} hat {kopie} Bytes, die Datenbank {quelle}. Es wurde nichts geloescht.");
            return ziel;
        }

        /// <summary>
        /// Entfernt aus der Bauwerkliste, was die Karte nicht mehr fuehrt.
        ///
        /// Karte und Bauwerkliste muessen geladen sein. Geloescht wird erst, wenn die Sicherung
        /// steht; scheitert sie, scheitert die ganze Bereinigung und die Datenbank bleibt, wie sie
        /// war.
        /// </summary>
        /// <param name="nurAnzeigen">
        /// true fuer einen Probelauf: es wird nichts gesichert und nichts geloescht, das Ergebnis
        /// sagt trotzdem, was betroffen waere.
        /// </param>
        public static Ergebnis Bereinige(string datenbankpfad, string verschluesseltesPasswort, bool nurAnzeigen = false) {
            var abgleich = BauwerkeView.MarkiereZerstörteBauwerke();
            var zuLoeschen = abgleich.Zerstört;

            if (zuLoeschen.Count == 0 || nurAnzeigen)
                return new Ergebnis(null, zuLoeschen, abgleich.MitRüstortOhneBaupunkte);

            string sicherung = Sichere(datenbankpfad);

            List<string> geloescht = [];
            using (var db = new ErkenfaraKarte(datenbankpfad, verschluesseltesPasswort)) {
                foreach (var bezeichner in zuLoeschen) {
                    if (SharedData.Gebäude == null || SharedData.Gebäude.TryGetValue(bezeichner, out var gebäude) == false)
                        continue;
                    // Nur was wirklich aus der Datenbank verschwunden ist, gilt als geloescht.
                    // Schlaegt es fehl, bleibt der Eintrag auch im Speicher stehen - sonst faende
                    // ihn der naechste Start wieder und meldete ihn erneut.
                    if (db.Delete(gebäude) == false)
                        continue;
                    SharedData.Gebäude.TryRemove(bezeichner, out _);
                    geloescht.Add(bezeichner);
                }
            }
            return new Ergebnis(sicherung, geloescht, abgleich.MitRüstortOhneBaupunkte);
        }
    }
}
