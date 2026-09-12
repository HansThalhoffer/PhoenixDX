using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.Data.Common;
using System.IO;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Das Ergebnis eines Ladelaufs über mehrere Reiche
    /// </summary>
    public class ReichsdatenErgebnis {
        public bool Erfolgreich { get; init; }
        public string Meldung { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
        /// <summary>Die Reiche, deren Zugdaten gelesen wurden</summary>
        public List<string> GeladeneReiche { get; init; } = [];
        /// <summary>Reiche, zu denen im Zugverzeichnis keine Datenbank liegt</summary>
        public List<string> FehlendeReiche { get; init; } = [];
        public int Figuren { get; init; }

        public static ReichsdatenErgebnis Fehler(string meldung, string details = "")
            => new() { Erfolgreich = false, Meldung = meldung, Details = details };
    }

    /// <summary>
    /// Liest die Figuren fremder Reiche für die Spielleitung.
    ///
    /// Nur lesend, und bewusst nicht über <see cref="Zugdaten"/>: der gewöhnliche Ladeweg füllt
    /// <see cref="SharedData"/> und setzt dabei die statische Eigenschaft DatabaseName jeder
    /// Tabellenklasse. Der Speicherlauf entscheidet an genau diesem Wert, wohin ein Datensatz
    /// geschrieben wird - ein zweites Reich über den gewöhnlichen Weg zu laden würde die Züge des
    /// Spielers in die fremde Datenbank umleiten. Deshalb stellt dieser Leser den vorgefundenen
    /// Zustand hinterher wieder her.
    ///
    /// Achtung: dieser Weg ist nur gegen Kopien einer einzigen Reichsdatenbank geprüft. Echte
    /// Daten mehrerer Reiche liegen nur einer Spielleitungsinstallation vor; bis das jemand
    /// ausprobiert hat, ist alles darüber hinaus unbelegt.
    /// </summary>
    public static class Reichsdaten {

        /// <summary>
        /// Liest die Figuren aller Reiche eines Zuges in <see cref="Spielleitungsdaten"/>.
        ///
        /// Erwartet wird die Ablage der Anwendung: unterhalb des Zugdatenverzeichnisses je Zug ein
        /// Verzeichnis, darin je Reich eine Datenbank, die nach dem DBname des Reiches heisst.
        /// Fehlt die Datenbank eines Reiches, wird das vermerkt und weitergemacht - eine
        /// Spielleitung arbeitet auch dann, wenn noch nicht alle Reiche abgegeben haben.
        /// </summary>
        /// <param name="zugverzeichnis">das Verzeichnis des Zuges</param>
        /// <param name="zug">die Nummer des Zuges</param>
        /// <param name="passwort">
        /// das Passwort, mit dem alle Datenbanken geöffnet werden. Ist es leer, wird je Reich das
        /// in der PZE hinterlegte Passwort genommen.
        /// </param>
        public static ReichsdatenErgebnis LadeAlleReiche(string zugverzeichnis, int zug, string? passwort = null) {
            if (Directory.Exists(zugverzeichnis) == false)
                return ReichsdatenErgebnis.Fehler($"Das Zugverzeichnis {zugverzeichnis} gibt es nicht",
                    "Ohne die Zugdaten der Reiche lässt sich nichts auswerten.");

            var nationen = SharedData.Nationen?.ToList();
            if (nationen == null || nationen.Count == 0)
                return ReichsdatenErgebnis.Fehler("Es sind keine Reiche bekannt",
                    "Die PZE-Datenbank muss geladen sein, bevor die Zugdaten der Reiche gelesen werden können.");

            List<string> geladen = [];
            List<string> fehlend = [];
            int figuren = 0;

            // Das ausgewählte Reich wird beim Lesen umgestellt, weil jede Figur in Spielfigur.Load
            // das gerade ausgewählte Reich zugewiesen bekommt. Am Ende steht wieder, was vorher da
            // war - sonst arbeitet die Spielerseite danach mit dem falschen Reich weiter.
            var vorherigeNation = ProgramView.SelectedNation;
            var vorherigeNamen = MerkeDatenbanknamen();
            try {
                foreach (var reich in nationen) {
                    if (string.IsNullOrWhiteSpace(reich.DBname))
                        continue;
                    string datei = Path.Combine(zugverzeichnis, $"{reich.DBname}.mdb");
                    if (File.Exists(datei) == false) {
                        fehlend.Add(reich.Reich);
                        continue;
                    }

                    string klartext = string.IsNullOrEmpty(passwort) ? reich.DBpass ?? string.Empty : passwort;
                    var armee = LiesFiguren(datei, klartext, reich);
                    if (armee == null) {
                        fehlend.Add(reich.Reich);
                        continue;
                    }
                    Spielleitungsdaten.Lege(reich, armee, zug);
                    geladen.Add(reich.Reich);
                    figuren += armee.Count;
                }
            }
            finally {
                // Sicherheitsnetz, heute ein Leerlauf: der Leser unten fasst weder das gewählte
                // Reich dauerhaft an noch die statischen Datenbanknamen, weil er nicht über
                // DatabaseLoader geht. Greift jemand später doch dorthin, faengt das hier es auf -
                // und ReichsdatenIntegrationTest zeigt, warum das noetig waere.
                ProgramView.SelectedNation = vorherigeNation;
                StelleDatenbanknamenWiederHer(vorherigeNamen);
            }

            if (geladen.Count == 0)
                return ReichsdatenErgebnis.Fehler($"In {zugverzeichnis} liegt keine Reichsdatenbank",
                    "Erwartet wird je Reich eine Datei, die nach dem Datenbanknamen des Reiches heisst.");

            return new ReichsdatenErgebnis {
                Erfolgreich = true,
                Meldung = $"{geladen.Count} Reiche für Zug {zug} gelesen",
                Details = $"{figuren} Figuren."
                        + (fehlend.Count > 0 ? $" Ohne Zugdaten: {string.Join(", ", fehlend)}." : string.Empty),
                GeladeneReiche = geladen,
                FehlendeReiche = fehlend,
                Figuren = figuren,
            };
        }

        /// <summary>
        /// Liest die Figuren einer einzelnen Reichsdatenbank.
        /// </summary>
        /// <returns>die Armee, oder null wenn sich die Datenbank nicht öffnen liess</returns>
        public static Armee? LiesFiguren(string datei, string passwort, Nation reich) {
            var vorherigeNation = ProgramView.SelectedNation;
            ProgramView.SelectedNation = reich;
            try {
                using var db = new AccessDatabase(datei, passwort);
                if (db.Open() == false) {
                    SpielWPF.LogError($"Die Zugdaten von {reich.Reich} liessen sich nicht öffnen", datei);
                    return null;
                }

                Armee armee = [];
                Lies<Krieger>(db, armee, Enum.GetNames(typeof(Krieger.Felder)), Krieger.TableName, reich);
                Lies<Reiter>(db, armee, Enum.GetNames(typeof(Reiter.Felder)), Reiter.TableName, reich);
                Lies<Schiffe>(db, armee, Enum.GetNames(typeof(Schiffe.Felder)), Schiffe.TableName, reich);
                Lies<Kreaturen>(db, armee, Enum.GetNames(typeof(Kreaturen.Felder)), Kreaturen.TableName, reich);
                Lies<Character>(db, armee, Enum.GetNames(typeof(Character.Felder)), Character.TableName, reich);
                Lies<Zauberer>(db, armee, Enum.GetNames(typeof(Zauberer.Felder)), Zauberer.TableName, reich);
                db.Close();
                return armee;
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Beim Lesen der Zugdaten von {reich.Reich} gab es einen Fehler", ex.Message);
                return null;
            }
            finally {
                ProgramView.SelectedNation = vorherigeNation;
            }
        }

        /// <summary>
        /// Liest eine Figurentabelle. Nur gültige Figuren kommen in die Armee: die Tabellen führen
        /// reihenweise leere Plätze mit der Position 0/0.
        /// </summary>
        private static void Lies<T>(AccessDatabase db, Armee armee, string[] felder, string tabelle, Nation reich)
            where T : Spielfigur, IDatabaseTable, new() {
            string abfrage = $"SELECT {string.Join(", ", felder)} FROM {tabelle} ORDER BY {felder[0]}";
            try {
                using DbDataReader reader = db.OpenReader(abfrage);
                while (reader.Read()) {
                    var figur = new T();
                    figur.Load(reader);
                    // auf Nummer sicher: Load weist das ausgewählte Reich zu, und das kann von
                    // aussen verstellt worden sein
                    figur.Nation = reich;
                    if (Plausibilität.IsValid(figur))
                        armee.Add(figur);
                }
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die Tabelle {tabelle} von {reich.Reich} liess sich nicht lesen", ex.Message);
            }
        }

        /// <summary>
        /// Die statischen Datenbanknamen der Figurentabellen, wie sie vor dem Lesen waren.
        ///
        /// Sie entscheiden, in welche Datei der Speicherlauf schreibt. Das Lesen fremder Reiche
        /// darf sie nicht verstellen.
        /// </summary>
        private static Dictionary<Type, string> MerkeDatenbanknamen() {
            Dictionary<Type, string> namen = [];
            foreach (var typ in Figurentypen)
                namen[typ] = PropertyProcessor.GetStaticValue(typ, "DatabaseName");
            return namen;
        }

        private static void StelleDatenbanknamenWiederHer(Dictionary<Type, string> namen) {
            foreach (var eintrag in namen)
                PropertyProcessor.SetStaticValue(eintrag.Key, "DatabaseName", eintrag.Value);
        }

        private static readonly Type[] Figurentypen = [
            typeof(Krieger), typeof(Reiter), typeof(Schiffe),
            typeof(Kreaturen), typeof(Character), typeof(Zauberer),
        ];
    }
}
