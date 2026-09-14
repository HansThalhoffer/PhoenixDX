using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixWPF.Program;
using PhoenixModel.ViewModel;
using PhoenixModel.EventsAndArgs;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Common;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database
{
    public abstract class DatabaseLoader
    {
      
        protected delegate T LoadObject<T>(DbDataReader reader);

        /// <summary>
        /// Lädt die objekte in eine Collection
        /// Wichtig: das erst genannte Feld im String Array bestimmt die Sortierung
        /// leider zwingt C# durch die ref parameterübergabe hier zwei identische Load Funktionen zu haben, eventuell wäre es eine
        /// Lösung die Colletion außerhalb zu erzeugen und das Laden in eine Funktion zu packen, wäre was zum Aufräumen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connector"></param>
        /// <param name="collection"></param>
        /// <param name="felder"></param>
        protected void Load<T>(AccessDatabase? connector, ref BlockingCollection<T>? collection, string[] felder) where T : IDatabaseTable, new()
        {
            if (connector != null)
            {
                int total = 0;
                collection = new BlockingCollection<T>();
                string felderListe = string.Join(", ", felder);
                string tableName = PropertyProcessor.GetConstValue<T>("TableName");
                PropertyProcessor.SetStaticValue<T>("DatabaseName", connector.DatabaseName);
                string query = $"SELECT {felderListe} FROM {tableName} ORDER BY {felder[0]}";
                try
                {
                    using (DbDataReader? reader = connector?.OpenReader(query))
                    {
                        while (reader != null && reader.Read())
                        {
                            T obj = new T();
                            typeof(T).GetMethod("Load")?.Invoke(obj, [reader]);
                            collection.Add(obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der {collection.GetType()} Datenbank:{tableName}", $"{query} führte zu folgendem Fehler \n\r{ex.Message}"));
                }
                collection.CompleteAdding();
                total = collection.Count();
                ProgramView.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen", $"Das Laden der Datenbanktabelle {tableName} war erfolgreich"));
            }
        }

        // andere Collection - hier Dictionary
        protected void Load<T>(AccessDatabase? connector, ref BlockingDictionary<T>? collection, string[] felder) where T : IDatabaseTable, new()
        {
            if (connector == null)
                return;
            int total = 0;
            collection = new BlockingDictionary<T>();
            string felderListe = string.Join(", ", felder);
            T obj = new T();
            string tableName = PropertyProcessor.GetConstValue<T>("TableName");
            PropertyProcessor.SetStaticValue<T>("DatabaseName", connector.DatabaseName);
            using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM {tableName} ORDER BY {felder[0]}"))
            {
                while (reader != null && reader.Read())
                {
                    obj = new T();
                    typeof(T).GetMethod("Load")?.Invoke(obj, [reader]);
                    string key = PropertyProcessor.GetPropertyValue(obj, "Bezeichner");
                    collection.Add(key, obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            ProgramView.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen", $"Das Laden der Datenbanktabelle {tableName} war erfolgreich"));
        }

        protected abstract void LoadInBackground();

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            // Do the actual work here
            LoadInBackground();
        }

        public int Percentage = 0;
        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Percentage = e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                ProgramView.LogError("Das Laden im Hintergrund war nicht erfolgreich", "Das Laden im Hintergrund wurde durch den Nutzer abgebrochen");
            }
            if (_loadCompletedDelegate != null && (this as ILoadableDatabase) != null)
                _loadCompletedDelegate((ILoadableDatabase)this);
        }

        LoadCompleted? _loadCompletedDelegate = null;
        public void BackgroundLoad(LoadCompleted? loadCompletedDelegate)
        {
            using (var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true })
            {
                if (loadCompletedDelegate != null)
                    _loadCompletedDelegate = loadCompletedDelegate;
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
        }

        protected int GetSum(AccessDatabase? connector, string tableName, string fieldName, string? filter = null)
        {
            if (connector == null)
                return -1;
            int total = 0;
            string query = string.IsNullOrEmpty(filter) ? $"SELECT SUM({fieldName}) AS Total FROM {tableName}" : $"SELECT SUM({fieldName}) AS Total FROM {tableName} WHERE {filter}";
            try
            {
                using (DbDataReader? reader = connector?.OpenReader(query))
                {
                    while (reader != null && reader.Read())
                    {
                        total = DatabaseConverter.ToInt32(reader[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Tabelle {tableName}: ", $"{query} erzeugte den Fehler: /n/r{ex.Message}"));
            }
            return total;
        }


        protected int GetCount(AccessDatabase? connector, string tableName, string filter)
        {
            if (connector == null)
                return -1;
            int total = 0;
            string query = string.IsNullOrEmpty(filter) ? $"SELECT count(*) FROM {tableName}" : $"SELECT count(*) FROM {tableName} WHERE {filter}";
            try
            {
                using (DbDataReader? reader = connector?.OpenReader(query))
                {
                    while (reader != null && reader.Read())
                    {
                        total = DatabaseConverter.ToInt32(reader[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Tabelle {tableName}: ", $"{query} erzeugte den Fehler: /n/r{ex.Message}"));
            }
            return total;
        }


        /// <summary>
        /// Schreibt einen einzelnen Datensatz.
        /// </summary>
        /// <returns>
        /// false, wenn nichts geschrieben wurde. Das muss der Aufrufer wissen koennen: frueher
        /// wurde jede Ausnahme nur protokolliert, und ein fehlgeschlagener Vorgang sah von aussen
        /// aus wie ein erfolgreicher.
        /// </returns>
        protected bool Save(IDatabaseTable table, EncryptedString encryptedpassword, string databaseFileName) {
            PasswordHolder holder = new(encryptedpassword);
            using (AccessDatabase connector = new(databaseFileName, holder.DecryptedPassword)) {
                if (connector?.Open() == false) {
                    ProgramView.LogError($"Die Datenbank {databaseFileName} liess sich nicht oeffnen",
                        "Es wurde nichts geschrieben.");
                    return false;
                }
                bool geschrieben = false;
                try {
                    if (connector != null) {
                        // der Befehl muss vor dem Schliessen der Verbindung freigegeben werden
                        using var command = connector.OpenDBCommand();
                        table.Save(command);
                        geschrieben = true;
                    }
                }
                catch (Exception ex) {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {databaseFileName}", ex.Message));
                }
                connector?.Close();
                return geschrieben;
            }
        }

        /// <summary>
        /// Schreibt einen einzelnen Datensatz.
        /// </summary>
        /// <returns>
        /// false, wenn nichts geschrieben wurde. Das muss der Aufrufer wissen koennen: frueher
        /// wurde jede Ausnahme nur protokolliert, und ein fehlgeschlagener Vorgang sah von aussen
        /// aus wie ein erfolgreicher.
        /// </returns>
        protected bool Insert(IDatabaseTable table, EncryptedString encryptedpassword, string databaseFileName) {
            PasswordHolder holder = new(encryptedpassword);
            using (AccessDatabase connector = new(databaseFileName, holder.DecryptedPassword)) {
                if (connector?.Open() == false) {
                    ProgramView.LogError($"Die Datenbank {databaseFileName} liess sich nicht oeffnen",
                        "Es wurde nichts geschrieben.");
                    return false;
                }
                bool geschrieben = false;
                try {
                    if (connector != null) {
                        // der Befehl muss vor dem Schliessen der Verbindung freigegeben werden
                        using var command = connector.OpenDBCommand();
                        table.Insert(command);
                        geschrieben = true;
                    }
                }
                catch (Exception ex) {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Speichern in der Datenbank {databaseFileName}", ex.Message));
                }
                connector?.Close();
                return geschrieben;
            }
        }

        /// <summary>
        /// Schreibt mehrere Vorgänge über eine einzige Verbindung.
        ///
        /// Siehe <see cref="ILoadableDatabase.SchreibeAlle"/>: je Datensatz eine Verbindung zu
        /// öffnen ist langsam und bringt den Access-Treiber sporadisch zum Absturz.
        ///
        /// Scheitert ein einzelner Vorgang, wird er protokolliert und die übrigen laufen weiter -
        /// genauso wie zuvor, als jeder Vorgang seine eigene Verbindung hatte. Ein halb
        /// geschriebener Speicherdurchgang ist immer noch besser als ein verworfener.
        /// </summary>
        /// <returns>die Anzahl der Vorgänge, die tatsächlich durchgelaufen sind</returns>
        protected int SchreibeAlle(IEnumerable<DatabaseQueue.DatabaseQueueItem> vorgänge, EncryptedString encryptedpassword, string databaseFileName) {
            var liste = vorgänge.ToList();
            if (liste.Count == 0)
                return 0;

            PasswordHolder holder = new(encryptedpassword);
            using AccessDatabase connector = new(databaseFileName, holder.DecryptedPassword);
            if (connector.Open() == false) {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,
                    $"Die Datenbank {databaseFileName} liess sich nicht öffnen",
                    $"{liste.Count} Änderungen konnten nicht gespeichert werden."));
                return 0;
            }

            int geschrieben = 0;
            try {
                // der Befehl muss vor dem Schliessen der Verbindung freigegeben werden
                using var command = connector.OpenDBCommand();
                foreach (var vorgang in liste) {
                    try {
                        switch (vorgang.Command) {
                            case DatabaseQueue.DatabaseQueueCommand.Insert:
                                vorgang.Table.Insert(command);
                                break;
                            case DatabaseQueue.DatabaseQueueCommand.Delete:
                                vorgang.Table.Delete(command);
                                break;
                            default:
                                vorgang.Table.Save(command);
                                break;
                        }
                        geschrieben++;
                    }
                    catch (Exception ex) {
                        ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,
                            $"Fehler beim Schreiben in die Tabelle {vorgang.Table.TableName}", ex.Message));
                    }
                }
            }
            catch (Exception ex) {
                ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,
                    $"Fehler beim Speichern in der Datenbank {databaseFileName}", ex.Message));
            }
            connector.Close();
            return geschrieben;
        }

        /// <summary>
        /// Schreibt einen einzelnen Datensatz.
        /// </summary>
        /// <returns>
        /// false, wenn nichts geschrieben wurde. Das muss der Aufrufer wissen koennen: frueher
        /// wurde jede Ausnahme nur protokolliert, und ein fehlgeschlagener Vorgang sah von aussen
        /// aus wie ein erfolgreicher.
        /// </returns>
        protected bool Delete(IDatabaseTable table, EncryptedString encryptedpassword, string databaseFileName) {
            PasswordHolder holder = new(encryptedpassword);
            using (AccessDatabase connector = new(databaseFileName, holder.DecryptedPassword)) {
                if (connector?.Open() == false) {
                    ProgramView.LogError($"Die Datenbank {databaseFileName} liess sich nicht oeffnen",
                        "Es wurde nichts geschrieben.");
                    return false;
                }
                bool geschrieben = false;
                try {
                    if (connector != null) {
                        // der Befehl muss vor dem Schliessen der Verbindung freigegeben werden
                        using var command = connector.OpenDBCommand();
                        table.Delete(command);
                        geschrieben = true;
                    }
                }
                catch (Exception ex) {
                    ProgramView.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Löschen in der Datenbank {databaseFileName}", ex.Message));
                }
                connector?.Close();
                return geschrieben;
            }
        }
    }
}
