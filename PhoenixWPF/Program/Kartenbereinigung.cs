using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
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
        /// <param name="Berichtigt">Eintraege, deren Reich oder Bauwerkname aus der Karte uebernommen wurde</param>
        /// <param name="OhneAngabeInDerKarte">
        /// Eintraege, zu denen die Karte weder ein Reich noch einen Namen nennt. Dort wird nichts
        /// uebernommen: ein leerer Wert aus der Karte heisst "unbekannt", nicht "niemand", und
        /// wuerde eine vorhandene Angabe loeschen.
        /// </param>
        public record class Ergebnis(string? Sicherung, List<string> Geloescht, List<string> Behalten,
                                     List<string> Berichtigt, List<string> OhneAngabeInDerKarte);

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
            var (zuBerichtigen, ohneAngabe) = SucheAbweichungen(zuLoeschen);

            if (nurAnzeigen || (zuLoeschen.Count == 0 && zuBerichtigen.Count == 0))
                return new Ergebnis(null, zuLoeschen, abgleich.MitRüstortOhneBaupunkte,
                    [.. zuBerichtigen.Select(eintrag => eintrag.Gebäude.Bezeichner)], ohneAngabe);

            string sicherung = Sichere(datenbankpfad);

            List<string> geloescht = [];
            List<string> berichtigt = [];
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

                if (zuBerichtigen.Count > 0) {
                    // Erst uebernehmen, dann schreiben - und ueber eine einzige Verbindung. Je
                    // Datensatz eine zu oeffnen ist langsam und bringt den Access-Treiber
                    // sporadisch zum Absturz.
                    foreach (var eintrag in zuBerichtigen) {
                        if (string.IsNullOrEmpty(eintrag.Reich) == false)
                            eintrag.Gebäude.Reich = eintrag.Reich;
                        if (string.IsNullOrEmpty(eintrag.Name) == false)
                            eintrag.Gebäude.Bauwerknamen = eintrag.Name;
                    }
                    var vorgänge = zuBerichtigen
                        .Select(eintrag => new DatabaseQueue.DatabaseQueueItem(eintrag.Gebäude, DatabaseQueue.DatabaseQueueCommand.Save))
                        .ToList();
                    int geschrieben = db.SchreibeAlle(vorgänge);
                    // Gemeldet wird nur, was durchgelaufen ist. SchreibeAlle protokolliert jeden
                    // einzelnen Fehlschlag und macht mit den uebrigen weiter.
                    berichtigt.AddRange(zuBerichtigen.Take(geschrieben).Select(eintrag => eintrag.Gebäude.Bezeichner));
                }
            }
            return new Ergebnis(sicherung, geloescht, abgleich.MitRüstortOhneBaupunkte, berichtigt, ohneAngabe);
        }

        /// <summary>
        /// Ein Eintrag der Bauwerkliste, der nicht zu der Karte passt.
        /// </summary>
        private record class Abweichung(Gebäude Gebäude, string Reich, string Name);

        /// <summary>
        /// Sucht die Eintraege, deren Reich oder Bauwerkname nicht zu der Karte passt.
        ///
        /// Die Karte ist die gepflegte Tabelle: dort steht, wem eine Gemark gehoert und wie das
        /// Bauwerk heisst. In der Bauwerkliste steht beides noch einmal, und beides laeuft
        /// auseinander - 706/38 fuehrt die Liste als Theostelos, die Karte sagt Ohar.
        ///
        /// Uebernommen wird nur, was die Karte auch nennt. Ein leerer Wert dort heisst
        /// "unbekannt", nicht "niemand"; ihn zu uebernehmen wuerde eine vorhandene Angabe loeschen.
        /// </summary>
        /// <param name="zuLoeschen">Eintraege, die ohnehin verschwinden - die braucht niemand mehr zu berichtigen</param>
        private static (List<Abweichung> Abweichungen, List<string> OhneAngabe) SucheAbweichungen(List<string> zuLoeschen) {
            List<Abweichung> abweichungen = [];
            List<string> ohneAngabe = [];
            if (SharedData.Gebäude == null || SharedData.Map == null)
                return (abweichungen, ohneAngabe);

            var verschwindet = zuLoeschen.ToHashSet();
            foreach (var gebäude in SharedData.Gebäude.Values) {
                if (verschwindet.Contains(gebäude.Bezeichner))
                    continue;
                if (SharedData.Map.TryGetValue(gebäude.Bezeichner, out var gemark) == false)
                    continue;

                // Nummer 0 heisst "kein Reich" - die Karte sagt damit nichts aus, und nichts
                // auszusagen ist kein Grund, eine vorhandene Angabe zu ueberschreiben. 806/27
                // steht in der Liste als Spielleitung, in der Karte als keinReich.
                var kartenNation = gemark.Nation;
                if (kartenNation != null && kartenNation.Nummer == ReichTabelle.KeinReich)
                    kartenNation = null;

                string reich = kartenNation?.Reich ?? string.Empty;
                string name = gemark.Bauwerknamen ?? string.Empty;
                if (kartenNation == null && string.IsNullOrEmpty(name)) {
                    ohneAngabe.Add(gebäude.Bezeichner);
                    continue;
                }

                // Verglichen werden Reiche, nicht Schreibweisen: "Pirat" und "Choson" sind
                // dieselbe Nation (Nummer 9). Ueber die Zeichenkette verglichen waeren das
                // fuenf Aenderungen, die nichts aendern.
                var listenNation = string.IsNullOrEmpty(gebäude.Reich)
                    ? null
                    : NationenView.GetNationFromString(gebäude.Reich);
                bool reichWeichtAb = kartenNation != null && kartenNation != listenNation;
                bool nameWeichtAb = string.IsNullOrEmpty(name) == false && name != (gebäude.Bauwerknamen ?? string.Empty);
                if (reichWeichtAb || nameWeichtAb)
                    abweichungen.Add(new Abweichung(gebäude, reichWeichtAb ? reich : string.Empty, name));
            }
            return (abweichungen, ohneAngabe);
        }
    }
}
