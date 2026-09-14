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
        private readonly OleDbConnection _connection;
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
        public AccessDatabase(string databaseFilePath, string? pw)
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

            _connection = new OleDbConnection(connectionString);

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
                using var test = new AccessDatabase(databaseFilePath, pw);
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

            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
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
            Close();
            _connection.Dispose();
        }

       
    }
}