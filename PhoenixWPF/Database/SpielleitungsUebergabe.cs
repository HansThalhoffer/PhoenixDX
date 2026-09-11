using Ionic.Zip;
using PhoenixWPF.Program;
using System.IO;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Das Ergebnis einer Paketerstellung für die Spielleitung
    /// </summary>
    public class UebergabeErgebnis {
        public bool Erfolgreich { get; init; }
        public string Meldung { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
        /// <summary>Das erzeugte Archiv, sofern es angelegt wurde</summary>
        public string? Archiv { get; init; }
        /// <summary>Die Grösse des Archivs in Bytes</summary>
        public long Grösse { get; init; }

        public static UebergabeErgebnis Fehler(string meldung, string details = "")
            => new() { Erfolgreich = false, Meldung = meldung, Details = details };
    }

    /// <summary>
    /// Packt die Zugdatenbank für die Spielleitung in ein Archiv.
    ///
    /// Vorlage sind ServerConnection.UebergabeAnSlViaZip und ServerConnection.ZipRuestung der
    /// Altanwendung. Die Dateinamen sind bewusst dieselben geblieben, damit die Spielleitung
    /// weiterhin mit ihrem gewohnten Ablauf arbeiten kann und die Archive beider Anwendungen
    /// nebeneinander liegen dürfen.
    /// </summary>
    public static class SpielleitungsUebergabe {

        /// <summary>
        /// Das Verzeichnis, in dem die Archive für die Spielleitung landen. In der Altanwendung war
        /// das ein fest verdrahteter Pfad ausserhalb der Spieldaten; hier liegt es neben den
        /// übrigen Daten, damit ein USB-Stick alles beisammen hat.
        ///
        /// Die Zugdatenbank liegt als &lt;Daten&gt;/Zugdaten/&lt;Zug&gt;/&lt;Reich&gt;.mdb, zwei
        /// Ebenen höher beginnt also das Datenverzeichnis. Das wird bewusst relativ bestimmt und
        /// nicht am Namen "_Data" festgemacht, damit es auch für eine Kopie ausserhalb der
        /// Spieldaten funktioniert.
        /// </summary>
        public static string? BestimmeÜbergabeverzeichnis(string zugdatenPfad) {
            string? zugverzeichnis = Path.GetDirectoryName(zugdatenPfad);
            string? zugdatenWurzel = zugverzeichnis == null ? null : Path.GetDirectoryName(zugverzeichnis);
            string? daten = zugdatenWurzel == null ? null : Path.GetDirectoryName(zugdatenWurzel);
            if (daten == null)
                return null;
            return Path.Combine(daten, "Spielleitung");
        }

        /// <summary>
        /// Der Name des Reiches, wie er im Dateinamen der Zugdatenbank steht
        /// </summary>
        private static string GetReichsname(string zugdatenPfad)
            => Path.GetFileNameWithoutExtension(zugdatenPfad);

        /// <summary>
        /// Packt die Zugdatenbank als abgegebenen Zug: &lt;Reich&gt;_&lt;Zug&gt;.zip im Übergabeordner
        /// </summary>
        public static UebergabeErgebnis ErstelleZugpaket(string zugdatenPfad, int zug, bool überschreiben = true)
            => Packe(zugdatenPfad, BestimmeÜbergabeverzeichnis(zugdatenPfad), $"{GetReichsname(zugdatenPfad)}_{zug}.zip", überschreiben);

        /// <summary>
        /// Packt die Zugdatenbank als Rüstungsabgabe: Ruestung_&lt;Reich&gt;_&lt;Zug&gt;.zip
        ///
        /// Die Rüstung wird abgegeben, bevor bewegt wird, damit die Spielleitung die neuen
        /// Einheiten schon kennt. Das Archiv landet im Zugverzeichnis neben der Datenbank, denn
        /// genau dort sucht die Anwendung beim Aufbau der Rüstungshistorie danach.
        /// </summary>
        public static UebergabeErgebnis ErstelleRüstungspaket(string zugdatenPfad, int zug, bool überschreiben = true)
            => Packe(zugdatenPfad, Path.GetDirectoryName(zugdatenPfad), $"Ruestung_{GetReichsname(zugdatenPfad)}_{zug}.zip", überschreiben);

        private static UebergabeErgebnis Packe(string zugdatenPfad, string? verzeichnis, string archivname, bool überschreiben) {
            if (File.Exists(zugdatenPfad) == false)
                return UebergabeErgebnis.Fehler("Die Zugdatenbank existiert nicht",
                    $"Unter {zugdatenPfad} liegt keine Datei, es gibt nichts zu packen.");

            if (verzeichnis == null)
                return UebergabeErgebnis.Fehler("Der Ablageort für die Übergabe lässt sich nicht bestimmen",
                    $"Aus {zugdatenPfad} ergibt sich kein Datenverzeichnis.");

            string archiv = Path.Combine(verzeichnis, archivname);
            if (File.Exists(archiv) && überschreiben == false)
                return UebergabeErgebnis.Fehler($"Das Archiv {archivname} gibt es bereits",
                    $"{archiv} existiert schon und soll nicht überschrieben werden.");

            try {
                Directory.CreateDirectory(verzeichnis);
                if (File.Exists(archiv))
                    File.Delete(archiv);

                using (var zip = new ZipFile()) {
                    zip.Password = PasswortProvider.ZipPasswort;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    zip.AddFile(zugdatenPfad, string.Empty);
                    zip.Save(archiv);
                }
            }
            catch (Exception ex) {
                return UebergabeErgebnis.Fehler($"Das Archiv {archivname} liess sich nicht erstellen", ex.Message);
            }

            var info = new FileInfo(archiv);
            SpielWPF.Log(new PhoenixModel.Program.LogEntry($"Das Archiv {archivname} liegt bereit",
                $"{archiv} ist {info.Length / 1024} KB gross und kann an die Spielleitung gegeben werden."));

            return new UebergabeErgebnis {
                Erfolgreich = true,
                Meldung = $"Das Archiv {archivname} liegt bereit",
                Details = $"Es liegt unter {archiv}.",
                Archiv = archiv,
                Grösse = info.Length,
            };
        }
    }
}
