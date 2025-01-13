using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixWPF.Program;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Common;
using static PhoenixModel.Program.SharedData;

namespace PhoenixWPF.Database
{
    public abstract class DatabaseLoader
    {
      
        protected delegate T LoadObject<T>(DbDataReader reader);

        private void SetDatabaseName<T>(T obj, AccessDatabase? connector)
        {
            if (obj is IDatabaseTable table)
            {
                if (connector != null)
                    table.DatabaseName = connector.DatabaseName;
            }
        }

        /// <summary>
        /// Lädt die objekte in eine Collection
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
                
                string query = $"SELECT {felderListe} FROM {tableName} ORDER BY {felder[0]}";
                try
                {
                    using (DbDataReader? reader = connector?.OpenReader(query))
                    {
                        while (reader != null && reader.Read())
                        {
                            T obj = new T();
                            SetDatabaseName(obj, connector);
                            typeof(T).GetMethod("Load")?.Invoke(obj, [reader]);
                            collection.Add(obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der {collection.GetType()} Datenbank:{tableName}", $"{query} führte zu folgendem Fehler \n\r{ex.Message}"));
                }
                collection.CompleteAdding();
                total = collection.Count();
                SpielWPF.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen", $"Das Laden der Datenbanktabelle {tableName} war erfolgreich"));
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
            using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM {tableName} ORDER BY {felder[0]}"))
            {
                while (reader != null && reader.Read())
                {
                    obj = new T();
                    SetDatabaseName(obj, connector);
                    typeof(T).GetMethod("Load")?.Invoke(obj, [reader]);
                    string key = PropertyProcessor.GetPropertyValue(obj, "Bezeichner");
                    collection.Add(key, obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            SpielWPF.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen", $"Das Laden der Datenbanktabelle {tableName} war erfolgreich"));
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
                SpielWPF.LogError("Das Laden im Hintergrund war nicht erfolgreich", "Das Laden im Hintergrund wurde durch den Nutzer abgebrochen");
            }
            if (_loadCompletedDelegate != null && (this as ILoadableDatabase) != null)
                _loadCompletedDelegate((ILoadableDatabase)this);
        }

        LoadCompleted? _loadCompletedDelegate = null;
        public void BackgroundLoad(LoadCompleted loadCompletedDelegate)
        {
            using (var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true })
            {
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
                SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Tabelle {tableName}: ", $"{query} erzeugte den Fehler: /n/r{ex.Message}"));
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
                SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Tabelle {tableName}: ", $"{query} erzeugte den Fehler: /n/r{ex.Message}"));
            }
            return total;
        }
    }
}
