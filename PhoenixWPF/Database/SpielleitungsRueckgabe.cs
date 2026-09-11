using Ionic.Zip;
using PhoenixWPF.Program;
using System.IO;
using System.Net.NetworkInformation;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Woher die Zugdatenbank stammt, die zurückgekommen ist
    /// </summary>
    public enum Rückgabequelle {
        Unbekannt,
        /// <summary>Direkt als Datenbank aus dem Verzeichnis der Spielleitung</summary>
        Datenbank,
        /// <summary>Aus einem verschlüsselten Archiv ausgepackt</summary>
        Archiv,
    }

    /// <summary>
    /// Das Ergebnis einer Rückgabe
    /// </summary>
    public class RückgabeErgebnis {
        public bool Erfolgreich { get; init; }
        public string Meldung { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
        /// <summary>Die Datenbank, die jetzt lokal liegt</summary>
        public string? Datenbank { get; init; }
        public Rückgabequelle Quelle { get; init; } = Rückgabequelle.Unbekannt;

        public static RückgabeErgebnis Fehler(string meldung, string details = "")
            => new() { Erfolgreich = false, Meldung = meldung, Details = details };
    }

    /// <summary>
    /// Holt die von der Spielleitung ausgewertete Zugdatenbank zurück.
    ///
    /// Vorlage ist ServerConnection.CopyReichDBFromServer der Altanwendung. Der Normalfall ist die
    /// Netzwerkfreigabe der Spielleitung; ist sie nicht erreichbar, kommt der Zug über einen
    /// Datenträger. Beides ist hier derselbe Weg: es wird ein Verzeichnis durchsucht, und ob das
    /// über das Netz oder über einen Stick erreichbar ist, spielt keine Rolle.
    ///
    /// In dem Verzeichnis wird zuerst die Datenbank selbst gesucht und erst danach das Archiv - so
    /// hält es die Altanwendung auch.
    /// </summary>
    public static class SpielleitungsRückgabe {

        /// <summary>
        /// Wie lange auf eine Antwort des Servers gewartet wird. Auf dem Gelände ist das Netz
        /// entweder da oder nicht; langes Warten hilft niemandem.
        /// </summary>
        private const int AntwortzeitMillisekunden = 1000;

        /// <summary>
        /// Ist der Server der Spielleitung erreichbar?
        /// </summary>
        /// <param name="freigabe">die Freigabe, etwa \\Server\PZEData</param>
        public static bool IstErreichbar(string? freigabe, out string fehler) {
            fehler = string.Empty;
            string? rechner = GetRechnername(freigabe);
            if (rechner == null) {
                fehler = $"Aus '{freigabe}' lässt sich kein Rechnername ablesen. Erwartet wird eine Freigabe der Form \\\\Rechner\\Freigabe.";
                return false;
            }
            try {
                using var ping = new Ping();
                var antwort = ping.Send(rechner, AntwortzeitMillisekunden);
                if (antwort.Status == IPStatus.Success)
                    return true;
                fehler = $"Der Server {rechner} antwortet nicht ({antwort.Status}).";
                return false;
            }
            catch (Exception ex) {
                fehler = $"Der Server {rechner} ist nicht erreichbar: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Liest den Rechnernamen aus einer UNC-Freigabe
        /// </summary>
        public static string? GetRechnername(string? freigabe) {
            if (string.IsNullOrWhiteSpace(freigabe) || freigabe.StartsWith(@"\\") == false)
                return null;
            var teile = freigabe.TrimStart('\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
            return teile.Length > 0 ? teile[0] : null;
        }

        /// <summary>
        /// Das Verzeichnis, in dem die Spielleitung den Zug eines Reiches ablegt.
        ///
        /// Die Altanwendung legt je Reich eine eigene Freigabe an und darunter je Zug ein
        /// Verzeichnis: \\Server\&lt;Reich&gt;\&lt;Zug&gt;.
        /// </summary>
        public static string BestimmeSerververzeichnis(string freigabe, string reich, int zug) {
            string? rechner = GetRechnername(freigabe);
            if (rechner == null)
                return Path.Combine(freigabe, reich, zug.ToString());
            return Path.Combine($@"\\{rechner}", reich, zug.ToString());
        }

        /// <summary>
        /// Holt den Zug aus einem Verzeichnis - egal ob Netzwerkfreigabe oder Datenträger.
        /// </summary>
        /// <param name="quellverzeichnis">wo gesucht wird</param>
        /// <param name="reich">der Dateiname des Reiches ohne Endung, etwa "Theostelos"</param>
        /// <param name="zug">der Zugmonat, für den Namen des Archivs</param>
        /// <param name="zielDatenbank">wohin die Datenbank gelegt wird</param>
        /// <param name="überschreiben">
        /// eine bereits vorhandene lokale Datenbank ersetzen. Das verwirft alles, was lokal in
        /// diesem Zug schon gemacht wurde, und muss vom Benutzer bestätigt werden.
        /// </param>
        public static RückgabeErgebnis HoleZug(string quellverzeichnis, string reich, int zug, string zielDatenbank, bool überschreiben = false) {
            if (Directory.Exists(quellverzeichnis) == false)
                return RückgabeErgebnis.Fehler($"Das Verzeichnis {quellverzeichnis} gibt es nicht",
                    "Ist der Server erreichbar und der Zug dort schon abgelegt? Sonst hilft das Archiv von der Spielleitung.");

            if (File.Exists(zielDatenbank) && überschreiben == false)
                return RückgabeErgebnis.Fehler($"Die Zugdaten für Zug {zug} gibt es lokal bereits",
                    $"{zielDatenbank} existiert schon. Wird sie ersetzt, geht alles verloren, was in diesem Zug lokal schon gemacht wurde.");

            // erst die Datenbank selbst
            string datenbank = Path.Combine(quellverzeichnis, $"{reich}.mdb");
            if (File.Exists(datenbank)) {
                try {
                    LegeAb(datenbank, zielDatenbank);
                }
                catch (Exception ex) {
                    return RückgabeErgebnis.Fehler($"Die Zugdatenbank liess sich nicht von {datenbank} holen", ex.Message);
                }
                return Erfolg(zielDatenbank, Rückgabequelle.Datenbank, zug, datenbank);
            }

            // dann das Archiv
            string archiv = Path.Combine(quellverzeichnis, $"{reich}_{zug}.zip");
            if (File.Exists(archiv) == false) {
                // die Spielleitung benennt Archive nicht immer gleich - was passt, wird genommen
                archiv = Directory.EnumerateFiles(quellverzeichnis, $"{reich}*.zip").FirstOrDefault() ?? string.Empty;
                if (string.IsNullOrEmpty(archiv))
                    return RückgabeErgebnis.Fehler($"In {quellverzeichnis} liegt nichts für {reich}",
                        $"Weder {reich}.mdb noch ein passendes Archiv wurden gefunden.");
            }

            return PackeArchivAus(archiv, reich, zielDatenbank, überschreiben);
        }

        /// <summary>
        /// Packt die Zugdatenbank aus einem Archiv der Spielleitung aus.
        ///
        /// Das Archiv enthält die Datenbank je nach Herkunft in einem Unterverzeichnis, deshalb
        /// wird nach dem Dateinamen gesucht und nicht nach dem vollen Pfad.
        /// </summary>
        public static RückgabeErgebnis PackeArchivAus(string archiv, string reich, string zielDatenbank, bool überschreiben = false) {
            if (File.Exists(archiv) == false)
                return RückgabeErgebnis.Fehler($"Das Archiv {archiv} gibt es nicht", string.Empty);

            if (File.Exists(zielDatenbank) && überschreiben == false)
                return RückgabeErgebnis.Fehler("Die Zugdaten gibt es lokal bereits",
                    $"{zielDatenbank} existiert schon. Wird sie ersetzt, geht alles verloren, was in diesem Zug lokal schon gemacht wurde.");

            string zwischenlager = Path.Combine(Path.GetTempPath(), "PhoenixDX_Rueckgabe_" + Guid.NewGuid().ToString("N"));
            try {
                Directory.CreateDirectory(zwischenlager);
                string gesucht = $"{reich}.mdb";
                string? ausgepackt = null;

                using (var zip = ZipFile.Read(archiv)) {
                    var eintrag = zip.Entries.FirstOrDefault(e => Path.GetFileName(e.FileName)
                        .Equals(gesucht, StringComparison.OrdinalIgnoreCase));
                    if (eintrag == null)
                        return RückgabeErgebnis.Fehler($"In {Path.GetFileName(archiv)} steckt keine {gesucht}",
                            "Enthalten ist: " + string.Join(", ", zip.Entries.Select(e => Path.GetFileName(e.FileName)).Take(10)));

                    eintrag.Password = PasswortProvider.ZipPasswort;
                    eintrag.Extract(zwischenlager, ExtractExistingFileAction.OverwriteSilently);
                    ausgepackt = Path.Combine(zwischenlager, eintrag.FileName.Replace('/', Path.DirectorySeparatorChar));
                }

                if (ausgepackt == null || File.Exists(ausgepackt) == false)
                    return RückgabeErgebnis.Fehler($"{gesucht} liess sich nicht aus {Path.GetFileName(archiv)} auspacken", string.Empty);

                LegeAb(ausgepackt, zielDatenbank);
            }
            catch (Exception ex) {
                return RückgabeErgebnis.Fehler($"Das Archiv {Path.GetFileName(archiv)} liess sich nicht auspacken", ex.Message);
            }
            finally {
                try { Directory.Delete(zwischenlager, true); } catch { /* das Zwischenlager liegt im Temp */ }
            }

            return Erfolg(zielDatenbank, Rückgabequelle.Archiv, 0, archiv);
        }

        /// <summary>
        /// Legt die geholte Datenbank an ihren Platz. Das Zielverzeichnis wird bei Bedarf angelegt.
        /// </summary>
        private static void LegeAb(string quelle, string zielDatenbank) {
            string? verzeichnis = Path.GetDirectoryName(zielDatenbank);
            if (string.IsNullOrEmpty(verzeichnis) == false)
                Directory.CreateDirectory(verzeichnis);
            File.Copy(quelle, zielDatenbank, true);
        }

        private static RückgabeErgebnis Erfolg(string zielDatenbank, Rückgabequelle quelle, int zug, string herkunft) {
            string woher = quelle == Rückgabequelle.Archiv ? "dem Archiv" : "dem Verzeichnis der Spielleitung";
            SpielWPF.Log(new PhoenixModel.Program.LogEntry("Der Zug ist zurück",
                $"Die Zugdatenbank wurde aus {herkunft} geholt und liegt unter {zielDatenbank}."));
            return new RückgabeErgebnis {
                Erfolgreich = true,
                Meldung = zug > 0 ? $"Zug {zug} ist zurück" : "Der Zug ist zurück",
                Details = $"Die Zugdatenbank wurde aus {woher} geholt und liegt unter {zielDatenbank}.",
                Datenbank = zielDatenbank,
                Quelle = quelle,
            };
        }
    }
}
