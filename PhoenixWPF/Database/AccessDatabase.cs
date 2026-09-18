using Microsoft.Win32;
using PhoenixModel.View;
using PhoenixWPF.Program;
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Windows;

namespace PhoenixWPF.Database
{
    /// <summary>
    /// Repräsentiert eine Verbindung zu einer Access-Datenbank via OLEDB
    /// </summary>
    public class AccessDatabase : IDisposable
    {
        private OleDbConnection _connection;

        /// <summary>
        /// Die Verbindungszeichenfolge, mit der diese Instanz arbeitet - zugleich der Schlüssel
        /// im Verbindungscache.
        /// </summary>
        private readonly string _verbindungszeichenfolge;

        /// <summary>
        /// Das Schloss der gemeinsam benutzten Verbindung, oder null bei einer eigenen.
        /// </summary>
        private object? _schloss;

        /// <summary>
        /// Wurde das Schloss von dieser Instanz betreten? Nur dann darf es wieder verlassen werden.
        /// </summary>
        private bool _betreten = false;

        #region Verbindungscache

        /// <summary>
        /// Die offen gehaltenen Verbindungen, je Verbindungszeichenfolge eine.
        ///
        /// <b>Warum:</b> der Access-Treiber stürzt nicht an der Menge der Abfragen, sondern an der
        /// Zahl der <em>Verbindungen</em>. Gemessen etwa eine Zugriffsverletzung in
        /// mso99Lwin32client.dll je 400 Verbindungen, die den Prozess ohne Ausnahme beendet. Ein
        /// voller Testlauf öffnete 758 Verbindungen und stürzte entsprechend etwa zweimal ab; die
        /// Anwendung sammelt sie über eine lange Sitzung ebenso an, weil jeder Speicherlauf mit
        /// Inhalt eine neue öffnet.
        ///
        /// Bleibt je Datei eine Verbindung offen, sinkt das auf eine Handvoll je Prozess.
        ///
        /// <b>Der Preis:</b> eine offene Verbindung hält die Datei. Wer sie kopieren, packen oder
        /// ersetzen will - die Zugabgabe etwa -, muss vorher <see cref="SchliesseAlle"/> rufen.
        /// Das ist der Grund, warum es diese Methode gibt, und nicht bloss Aufräumen.
        /// </summary>
        private static readonly Dictionary<string, OleDbConnection> _verbindungen = [];

        /// <summary>
        /// Je Verbindung ein Schloss: eine OleDbConnection verträgt keine zwei Benutzer zugleich,
        /// und die Anwendung greift aus mehreren Threads zu (Karte, Oberfläche, Laden im
        /// Hintergrund). Solange eine AccessDatabase offen ist, gehört ihr die Verbindung allein.
        /// </summary>
        private static readonly Dictionary<string, object> _schlösser = [];

        /// <summary>
        /// Schützt die beiden Verzeichnisse selbst - nicht die Verbindungen darin.
        /// </summary>
        private static readonly object _cacheSchloss = new();

        /// <summary>
        /// Wie lange auf eine belegte Verbindung gewartet wird, bevor eine eigene geöffnet wird.
        ///
        /// Ohne diese Grenze könnten sich zwei Threads, die zwei Dateien in verschiedener
        /// Reihenfolge belegen, gegenseitig blockieren - die Anwendung stünde still und niemand
        /// wüsste warum. Nach Ablauf gilt wieder das alte Verhalten: eine eigene Verbindung.
        /// Das ist langsamer und zählt gegen die 400, aber es hängt nichts.
        /// </summary>
        private static readonly TimeSpan Geduld = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Beim Ende des Prozesses werden die offen gehaltenen Verbindungen geschlossen.
        ///
        /// Sonst bleibt neben jeder Datenbank ihre Sperrdatei liegen - Access legt eine .ldb an,
        /// solange jemand verbunden ist, und räumt sie erst beim Schliessen der letzten
        /// Verbindung weg. Vorher fiel das nicht auf, weil jede Verbindung sofort wieder zuging.
        /// </summary>
        static AccessDatabase() {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => SchliesseAlle();
        }

        /// <summary>
        /// Schliesst alle offen gehaltenen Verbindungen und gibt damit die Dateien frei.
        ///
        /// Muss gerufen werden, bevor eine Datenbankdatei kopiert, gepackt, verschoben oder
        /// ersetzt wird. Danach arbeitet alles weiter wie zuvor - die nächste Benutzung öffnet
        /// eine neue Verbindung.
        /// </summary>
        public static void SchliesseAlle() {
            List<KeyValuePair<string, OleDbConnection>> offen;
            lock (_cacheSchloss) {
                offen = [.. _verbindungen];
                _verbindungen.Clear();
            }
            foreach (var eintrag in offen) {
                // Das Schloss der Verbindung wird mitgenommen: sonst schlösse man sie jemandem
                // unter den Händen weg, der gerade liest.
                object schloss;
                lock (_cacheSchloss) {
                    if (_schlösser.TryGetValue(eintrag.Key, out var vorhanden) == false)
                        continue;
                    schloss = vorhanden;
                }
                lock (schloss) {
                    try {
                        if (eintrag.Value.State != ConnectionState.Closed)
                            eintrag.Value.Close();
                        eintrag.Value.Dispose();
                    }
                    catch (Exception ex) {
                        ProgramView.LogWarning("Eine Datenbankverbindung liess sich nicht schliessen", ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Wieviele Verbindungen gerade offen gehalten werden - für die Prüfung.
        /// </summary>
        public static int AnzahlOffenerVerbindungen {
            get { lock (_cacheSchloss) { return _verbindungen.Count; } }
        }

        private static OleDbConnection HoleVerbindung(string zeichenfolge) {
            lock (_cacheSchloss) {
                if (_verbindungen.TryGetValue(zeichenfolge, out var vorhanden))
                    return vorhanden;
                var neu = new OleDbConnection(zeichenfolge);
                _verbindungen[zeichenfolge] = neu;
                return neu;
            }
        }

        private static object HoleSchloss(string zeichenfolge) {
            lock (_cacheSchloss) {
                if (_schlösser.TryGetValue(zeichenfolge, out var vorhanden))
                    return vorhanden;
                var neu = new object();
                _schlösser[zeichenfolge] = neu;
                return neu;
            }
        }

        #endregion

        /// <summary>
        /// Ermittelt die installierte Microsoft ACE OLEDB Provider-Version aus der Windows-Registrierung.
        /// </summary>
        /// <returns>Der Name des installierten OLEDB Providers oder null, wenn keiner gefunden wurde.</returns>
        public static string? GetInstalledAceOleDbProvider()
        {
            // Registrypfad für OLEDB Provider
              // string registryPath32Bit = @"SOFTWARE\WOW6432Node\Classes\Microsoft.ACE.OLEDB.";
            string registryPath64Bit = @"SOFTWARE\Classes\Microsoft.ACE.OLEDB.";

            string? result = null;

            // Überprüft bekannte Versionen des ACE OLEDB Providers
            string[] versions = { "18.0", "17.0", "16.0", "15.0", "14.0", "12.0", "11.0", "10.0", "8.0", "4.0" }; 
            foreach (var version in versions)
            {
                using (var key = Registry.LocalMachine.OpenSubKey($"{registryPath64Bit}{version}"))
                {
                    if (key != null)
                    {
                        result = $"Microsoft.ACE.OLEDB.{version}";
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Standard-SQL-Abfrage zur Rückgabe der letzten automatisch von der Datenbank eingefügten Identität.
        /// </summary>
        static string QueryIdentityAfterInsert =  "SELECT @@IDENTITY;";
        
        /// <summary>
        /// Erstellt eine Verbindung zur Access-Datenbank.
        /// </summary>
        /// <param name="databaseFilePath">Der Pfad zur Access-Datenbankdatei.</param>
        /// <param name="pw">Optionales Passwort für die Datenbank.</param>
        /// <exception cref="ArgumentException">Wird ausgelöst, wenn kein Dateipfad angegeben wurde.</exception>
        public AccessDatabase(string databaseFilePath, string? pw) : this(databaseFilePath, pw, gemeinsam: true) { }

        /// <summary>
        /// Erstellt eine Verbindung zur Access-Datenbank.
        /// </summary>
        /// <param name="databaseFilePath">Der Pfad zur Access-Datenbankdatei.</param>
        /// <param name="pw">Optionales Passwort für die Datenbank.</param>
        /// <param name="gemeinsam">
        /// true, wenn die offen gehaltene Verbindung aus dem Cache benutzt werden soll. false für
        /// Fälle, die ausdrücklich eine eigene brauchen - etwa die Passwortprobe, die auch mit
        /// einem falschen Passwort aufgerufen wird und nichts hinterlassen darf.
        /// </param>
        /// <exception cref="ArgumentException">Wird ausgelöst, wenn kein Dateipfad angegeben wurde.</exception>
        private AccessDatabase(string databaseFilePath, string? pw, bool gemeinsam)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
                throw new ArgumentException("Database file path must be provided.", nameof(databaseFilePath));

            string connectionString = string.Empty;
            string? provider = GetInstalledAceOleDbProvider();
            if (provider == null)
            {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, "Es ist kein Microsoft.ACE.OLEDB Treiber installiert. Bitte einen entsprechenden Treiber installieren", "Der 'Microsoft Access Database Engine 2016 Redistributable' Treiber für die Access Datenbank muss installiert sein. Normalerweise ist der automatisch mit dem Office installiert, hier anscheinend nicht. Die Installationsdateien befinden sich unter 'Redistribute' im Hauptverzeichnis. Sie könne auch bei Microsoft heruntergeladen werden."));
                connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + databaseFilePath + "\"; ";
            }
            else
            {
                connectionString = $@"Provider={provider};Data Source={databaseFilePath};Persist Security Info=False;";
            }
            
            if (string.IsNullOrEmpty(pw) == false)
            {
                connectionString += "Jet OLEDB:Database Password=" + pw + ";";
            }
            // Verbindungspooling abschalten.
            //
            // Der Access-Treiber haelt gepoolte Verbindungen nativ offen und wird ueber mehrere
            // Threads hinweg wiederverwendet. Bei dem schnellen Oeffnen und Schliessen, das diese
            // Anwendung betreibt - beim Speichern eine Verbindung je Datensatz - fuehrt das
            // reproduzierbar zu einer Zugriffsverletzung in mso99Lwin32client.dll und damit zum
            // sofortigen Absturz des Prozesses ohne verwertbare Ausnahme.
            // -4 bedeutet: weder Pooling noch automatische Transaktionsanmeldung.
            connectionString += "OLE DB Services=-4;";

            _verbindungszeichenfolge = connectionString;
            if (gemeinsam) {
                _connection = HoleVerbindung(connectionString);
                _schloss = HoleSchloss(connectionString);
            }
            else {
                _connection = new OleDbConnection(connectionString);
                _schloss = null;
            }
        }

        /// <summary>
        /// Versucht die Datenbank mit dem übergebenen Klartextpasswort zu öffnen, ohne etwas zu laden
        /// und ohne einen Fehler zu protokollieren. Damit lässt sich vor dem Laden feststellen, ob ein
        /// gespeichertes Passwort überhaupt noch passt.
        /// </summary>
        /// <param name="databaseFilePath">Pfad zur Access Datenbank</param>
        /// <param name="pw">das Passwort im Klartext</param>
        /// <param name="fehler">die Fehlermeldung, falls das Öffnen nicht geklappt hat</param>
        /// <returns>true, wenn die Datenbank geöffnet werden konnte</returns>
        public static bool TestConnection(string databaseFilePath, string? pw, out string fehler)
        {
            fehler = string.Empty;
            if (string.IsNullOrWhiteSpace(databaseFilePath) || System.IO.File.Exists(databaseFilePath) == false)
            {
                fehler = $"Die Datei {databaseFilePath} existiert nicht";
                return false;
            }
            try
            {
                // ausdrücklich eine eigene Verbindung: hier wird auch mit einem falschen Passwort
                // angeklopft, und das darf im Cache nichts hinterlassen
                using var test = new AccessDatabase(databaseFilePath, pw, gemeinsam: false);
                if (test._connection.State != ConnectionState.Open)
                    test._connection.Open();
                test._connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                fehler = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// Gibt an, ob die Datenbankverbindung aktuell geöffnet ist.
        /// </summary>
        public bool IsConnected
        {
            get { return _connection != null && _connection.State == ConnectionState.Open; }
        }

        /// <summary>
        /// Gibt den Namen der verbundenen Datenbank zurück.
        /// </summary>
        public string DatabaseName
        {
            get 
            {
                if (IsConnected == false)
                    return string.Empty;
                return _connection.DataSource;
            }
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public bool Open()
        {
            // Die gemeinsame Verbindung gehört ab jetzt dieser Instanz allein, bis sie
            // geschlossen wird. Wartet jemand zu lange, bekommt er eine eigene - lieber langsam
            // als hängend.
            if (_schloss != null && _betreten == false)
            {
                if (System.Threading.Monitor.TryEnter(_schloss, Geduld))
                {
                    _betreten = true;
                }
                else
                {
                    ProgramView.LogWarning("Eine Datenbankverbindung war zu lange belegt",
                        $"Für {_connection.DataSource} wird ersatzweise eine eigene Verbindung geöffnet. "
                        + "Das ist langsamer; häufen sich diese Meldungen, greifen zwei Abläufe gleichzeitig zu.");
                    _connection = new OleDbConnection(_verbindungszeichenfolge);
                    _schloss = null;
                }
            }

            if (_connection.State != ConnectionState.Open)
            {
                try
                {
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, "Fehler beim Öffnen der PZE Datenbank", ex.Message));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void Close()
        {
            // erst die Lesebefehle freigeben, dann die Verbindung - die umgekehrte Reihenfolge
            // lässt den Treiber auf freigegebenen Strukturen arbeiten
            foreach (var command in _offeneLeseBefehle)
            {
                try { command.Dispose(); }
                catch (Exception ex) { ProgramView.LogWarning("Ein Datenbankbefehl konnte nicht freigegeben werden", ex.Message); }
            }
            _offeneLeseBefehle.Clear();

            // Eine gemeinsame Verbindung bleibt offen - genau darum geht es. Geschlossen wird sie
            // erst von SchliesseAlle, wenn jemand die Datei braucht.
            if (_schloss == null && _connection.State != ConnectionState.Closed)
                _connection.Close();

            if (_betreten)
            {
                _betreten = false;
                System.Threading.Monitor.Exit(_schloss!);
            }
        }

        /// <summary>
        /// Executes a query that returns a result set.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A DataTable containing the result set.</returns>
        /// <summary>
        /// Die Namen aller Tabellen der Datenbank, ohne die Systemtabellen von Access.
        ///
        /// Gebraucht für den Vergleich zweier Zugdatenbanken: welche Tabellen es gibt, steht
        /// nirgends im Quelltext vollständig - die Anwendung lädt nur die, die sie kennt.
        /// </summary>
        public List<string> GetTabellennamen() {
            List<string> namen = [];
            var schema = _connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, [null, null, null, "TABLE"]);
            if (schema == null)
                return namen;
            foreach (DataRow zeile in schema.Rows) {
                string name = Convert.ToString(zeile["TABLE_NAME"]) ?? string.Empty;
                if (string.IsNullOrEmpty(name) || name.StartsWith("MSys", StringComparison.OrdinalIgnoreCase))
                    continue;
                namen.Add(name);
            }
            namen.Sort(StringComparer.OrdinalIgnoreCase);
            return namen;
        }

        /// <summary>
        /// Die Spalten des Primärschlüssels einer Tabelle, in der Reihenfolge des Schlüssels.
        /// Eine leere Liste heisst: die Tabelle hat keinen.
        /// </summary>
        public List<string> GetPrimärschluessel(string tabelle) {
            List<string> spalten = [];
            var schema = _connection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, [null, null, tabelle]);
            if (schema == null)
                return spalten;
            foreach (DataRow zeile in schema.Rows.Cast<DataRow>().OrderBy(zeile => Convert.ToInt32(zeile["ORDINAL"] is DBNull ? 0 : zeile["ORDINAL"])))
                spalten.Add(Convert.ToString(zeile["COLUMN_NAME"]) ?? string.Empty);
            return spalten;
        }

        public DataTable ExecuteQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query must be provided.", nameof(query));

            using var command = new OleDbCommand(query, _connection);
            using var adapter = new OleDbDataAdapter(command);
            var result = new DataTable();
            adapter.Fill(result);
            return result;
        }

        /// <summary>
        /// Executes a query that returns a reader.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A DataTable containing the result set.</returns>
        /// <summary>
        /// Befehle, die zu noch offenen Lesern gehören und erst danach freigegeben werden dürfen
        /// </summary>
        private readonly List<OleDbCommand> _offeneLeseBefehle = [];

        public DbDataReader OpenReader(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query must be provided.", nameof(query));

            // Der Befehl darf hier NICHT mit using freigegeben werden.
            //
            // Vorher stand hier "using var command" - damit wurde der Befehl beim Verlassen der
            // Methode freigegeben, während der zurückgegebene Leser noch daran hing und noch keine
            // Zeile gelesen hatte. Der Access-Treiber arbeitet dann auf bereits freigegebenen
            // nativen Strukturen weiter. Das ist die Ursache der sporadischen Zugriffsverletzung in
            // mso99Lwin32client.dll, die den ganzen Prozess ohne Ausnahme beendet hat.
            //
            // Der Befehl bleibt jetzt bis zum Schliessen der Verbindung am Leben. AccessDatabase
            // ist kurzlebig und wird von den Aufrufern in einem using gehalten.
            var command = new OleDbCommand(query, _connection);
            _offeneLeseBefehle.Add(command);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes a query that returns a reader.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DbCommand OpenDBCommand()
        {
            return new DbCommandFacade(_connection.CreateCommand(), QueryIdentityAfterInsert);
        }

        /// <summary>
        /// Executes a non-query command (e.g., INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="commandText">The SQL command to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentException("Command text must be provided.", nameof(commandText));

            using var command = new OleDbCommand(commandText, _connection);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a scalar query and returns a single value.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object? ExecuteScalar(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query must be provided.", nameof(query));

            using var command = new OleDbCommand(query, _connection);
            return command?.ExecuteScalar();
        }

        /// <summary>
        /// Disposes the database connection.
        /// </summary>
        public void Dispose()
        {
            bool eigene = _schloss == null;
            Close();
            // Nur eine eigene Verbindung gehört dieser Instanz und darf mit ihr sterben.
            if (eigene)
                _connection.Dispose();
        }

       
    }
}