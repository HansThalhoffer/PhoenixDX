using Microsoft.Win32;
using PhoenixModel.Database;
using PhoenixModel.View;
using PhoenixWPF.Program;
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace PhoenixWPF.Database
{
    /// <summary>
    /// Eine Verbindung zu einer Access-Datenbank über OLE DB.
    ///
    /// <b>Was hier drin steht und nirgends sonst:</b> die Erkennung des installierten
    /// ACE-Treibers, der Bau der Verbindungszeichenfolge samt Passwort, die Abfrage des Schemas
    /// über <c>GetOleDbSchemaTable</c> und <c>SELECT @@IDENTITY</c>. Das ist der
    /// Access-eigene Teil; alles andere steht in <see cref="IDatenbank"/> und im
    /// <see cref="Datenbankwaechter"/> und gilt für jeden Unterbau.
    ///
    /// <b>Die Verbindung gehört dieser Klasse nicht.</b> Sie gehört dem Wächter der Datei und
    /// bleibt offen; <see cref="Open"/> leiht sie aus und sperrt sie dabei für andere,
    /// <see cref="Close"/> gibt sie zurück. Eine Instanz ist deshalb kurzlebig und gehört in ein
    /// <c>using</c>. Warum überhaupt offen gehalten wird, steht beim Wächter - kurz: der Treiber
    /// stürzt an der Zahl der Verbindungen, nicht an der Menge der Abfragen.
    /// </summary>
    public class AccessDatabase : IDatenbank
    {
        /// <summary>
        /// Die ausgeliehene Verbindung, oder null, solange nicht geöffnet ist.
        /// </summary>
        private OleDbConnection? _connection;

        /// <summary>
        /// Der Wächter, von dem die Verbindung geliehen wird - null bei einer eigenen.
        /// </summary>
        private readonly Datenbankwaechter? _wächter;

        /// <summary>
        /// Eine eigene Verbindung für die Fälle, die ausdrücklich keine gemeinsame wollen.
        /// Es gibt genau einen: die Passwortprobe (siehe <see cref="TestConnection"/>).
        /// </summary>
        private readonly OleDbConnection? _eigeneVerbindung;

        /// <summary>
        /// Die Verbindungszeichenfolge - zugleich der Schlüssel des Wächters. <b>Enthält das
        /// Passwort</b> und darf deshalb in keine Meldung geraten.
        /// </summary>
        private readonly string _verbindungszeichenfolge;

        /// <summary>
        /// Der Pfad, wie der Aufrufer ihn übergeben hat. Nicht aus der Verbindung zurückgelesen:
        /// an diesem Namen erkennt der Speicherlauf wieder, in welche Datei ein Datensatz gehört,
        /// und er muss dazu buchstabengleich mit dem Pfad aus den Einstellungen sein.
        /// </summary>
        private readonly string _datenbankdatei;

        /// <summary>
        /// Wurde die gemeinsame Verbindung von dieser Instanz ausgeliehen? Nur dann darf sie
        /// zurückgegeben werden.
        /// </summary>
        private bool _geliehen = false;

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
        /// Access kennt dafür @@IDENTITY; ein anderer Unterbau setzt hier etwas anderes ein, und
        /// genau deshalb nimmt <see cref="DbCommandFacade"/> die Abfrage als Parameter.
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
        /// true, wenn die offen gehaltene Verbindung des Wächters benutzt werden soll. false für
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
            // Anwendung betrieben hat - beim Speichern eine Verbindung je Datensatz - fuehrt das
            // reproduzierbar zu einer Zugriffsverletzung in mso99Lwin32client.dll und damit zum
            // sofortigen Absturz des Prozesses ohne verwertbare Ausnahme.
            // -4 bedeutet: weder Pooling noch automatische Transaktionsanmeldung.
            connectionString += "OLE DB Services=-4;";

            _verbindungszeichenfolge = connectionString;
            _datenbankdatei = databaseFilePath;
            if (gemeinsam) {
                _wächter = Datenbankwaechter.Hole(connectionString, () => new OleDbConnection(connectionString));
            }
            else {
                _eigeneVerbindung = new OleDbConnection(connectionString);
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
                // angeklopft, und das darf beim Wächter nichts hinterlassen
                using var test = new AccessDatabase(databaseFilePath, pw, gemeinsam: false);
                var verbindung = test._eigeneVerbindung!;
                if (verbindung.State != ConnectionState.Open)
                    verbindung.Open();
                verbindung.Close();
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
        /// Gibt den Namen der verbundenen Datenbank zurück - leer, solange nicht geöffnet ist.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                if (IsConnected == false)
                    return string.Empty;
                return _datenbankdatei;
            }
        }

        /// <summary>
        /// Leiht die Verbindung aus und öffnet sie.
        ///
        /// Ist sie von einem anderen Ablauf belegt, wird gewartet; nach der
        /// <see cref="Datenbankwaechter.Geduld"/> wird aufgegeben und false geliefert. <b>Früher
        /// wurde an dieser Stelle eine eigene Verbindung geöffnet.</b> Das war falsch: unter Last
        /// wich die Anwendung genau in die Betriebsart aus, an der der Treiber stirbt. Lieber ein
        /// gemeldeter Fehlschlag als ein Absturz des Prozesses.
        /// </summary>
        /// <returns>false, wenn die Verbindung nicht zu bekommen war; der Grund ist protokolliert</returns>
        public bool Open()
        {
            if (_eigeneVerbindung != null) {
                try {
                    if (_eigeneVerbindung.State != ConnectionState.Open)
                        _eigeneVerbindung.Open();
                    _connection = _eigeneVerbindung;
                    return true;
                }
                catch (Exception ex) {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,
                        $"Die Datenbank {System.IO.Path.GetFileName(_datenbankdatei)} liess sich nicht öffnen", ex.Message));
                    return false;
                }
            }

            if (_geliehen)
                return _connection != null;

            var verbindung = _wächter!.Betrete(out string fehler);
            if (verbindung == null) {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,
                    $"Die Datenbank {System.IO.Path.GetFileName(_datenbankdatei)} liess sich nicht öffnen", fehler));
                return false;
            }

            // Der Wächter kennt nur DbConnection; hier unten ist sie immer eine OleDbConnection,
            // weil der Erzeuger dieser Klasse gehört. Der Schemazugriff und die OleDbCommand
            // brauchen den konkreten Typ.
            _connection = (OleDbConnection)verbindung;
            _geliehen = true;
            return true;
        }

        /// <summary>
        /// Gibt die Verbindung zurück. Eine gemeinsame bleibt offen - genau darum geht es.
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

            if (_eigeneVerbindung != null) {
                if (_eigeneVerbindung.State != ConnectionState.Closed)
                    _eigeneVerbindung.Close();
                _connection = null;
                return;
            }

            if (_geliehen) {
                _geliehen = false;
                _connection = null;
                _wächter!.Verlasse();
            }
        }

        /// <summary>
        /// Die Namen aller Tabellen der Datenbank, ohne die Systemtabellen von Access.
        ///
        /// Gebraucht für den Vergleich zweier Zugdatenbanken: welche Tabellen es gibt, steht
        /// nirgends im Quelltext vollständig - die Anwendung lädt nur die, die sie kennt.
        /// </summary>
        public List<string> GetTabellennamen() {
            List<string> namen = [];
            var schema = Verbindung.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, [null, null, null, "TABLE"]);
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
            var schema = Verbindung.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, [null, null, tabelle]);
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

            using var command = new OleDbCommand(query, Verbindung);
            using var adapter = new OleDbDataAdapter(command);
            var result = new DataTable();
            adapter.Fill(result);
            return result;
        }

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
            // Der Befehl bleibt jetzt bis zum Close am Leben. AccessDatabase ist kurzlebig und
            // wird von den Aufrufern in einem using gehalten.
            var command = new OleDbCommand(query, Verbindung);
            _offeneLeseBefehle.Add(command);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Ein Befehlsobjekt für Schreibvorgänge. Es muss vor dem <see cref="Close"/> freigegeben werden.
        /// </summary>
        public DbCommand OpenDBCommand()
        {
            return new DbCommandFacade(Verbindung.CreateCommand(), QueryIdentityAfterInsert);
        }

        /// <summary>
        /// Beginnt eine Transaktion über die ausgeliehene Verbindung.
        ///
        /// Access kann das; ein Speicherdurchgang bleibt damit ganz oder gar nicht stehen. Der
        /// Aufrufer muss sie in einem <c>finally</c> abschliessen - eine offen gelassene
        /// Transaktion trifft als nächstes einen fremden Ablauf auf derselben Verbindung.
        /// </summary>
        public DbTransaction? BeginneTransaktion()
        {
            return Verbindung.BeginTransaction();
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

            using var command = new OleDbCommand(commandText, Verbindung);
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

            using var command = new OleDbCommand(query, Verbindung);
            return command?.ExecuteScalar();
        }

        /// <summary>
        /// Die ausgeliehene Verbindung, für alles, was ohne sie nicht geht.
        ///
        /// Wer hierauf zugreift, ohne vorher <see cref="Open"/> gerufen zu haben, bekommt eine
        /// Ausnahme mit einem Satz, der sagt, was fehlt - und nicht eine NullReferenceException
        /// irgendwo im Treiber.
        /// </summary>
        private OleDbConnection Verbindung =>
            _connection ?? throw new InvalidOperationException(
                $"Die Datenbank {System.IO.Path.GetFileName(_datenbankdatei)} ist nicht geöffnet. "
                + "Vor jedem Zugriff muss Open() gerufen und sein Ergebnis geprüft werden.");

        /// <summary>
        /// Gibt die Verbindung zurück. Eine eigene stirbt mit dieser Instanz, eine gemeinsame
        /// gehört dem Wächter und bleibt offen.
        /// </summary>
        public void Dispose()
        {
            Close();
            _eigeneVerbindung?.Dispose();
        }


    }
}
