using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixModel.Helper.SharedData;

namespace PhoenixWPF.Database
{
    public abstract class DatabaseLoader
    {
        public class PasswortProvider : PasswordHolder.IPasswordProvider
        {
            string _databaseName = string.Empty;
            public PasswortProvider(string databaseName)
            { _databaseName = databaseName; }
            public EncryptedString Password
            {
                get
                {
                    PasswordDialog dialog = new PasswordDialog($"Das Passwort für die Datenbank {_databaseName} bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
        }

        protected delegate T LoadObject<T>(DbDataReader reader);

        protected void Load<T> (AccessDatabase? connector, ref BlockingCollection<T>? collection, string[] felder) where T : IDatabaseTable, new()
        {
            if (connector == null)
                return;
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
                        typeof(T).GetMethod("Load")?.Invoke(obj,[reader]);
                        collection.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, ($"Fehler beim Öffnen der {collection.GetType()} Datenbank: " + ex.Message +"\n\r"+query)));
            }
            collection.CompleteAdding();
            total = collection.Count();
            SpielWPF.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen"));
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
                    typeof(T).GetMethod("Load")?.Invoke(obj, [reader]);
                    string key = PropertyProcessor.GetPropertyValue(obj, "Bezeichner");
                    collection.Add(key, obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            SpielWPF.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T).Name} geladen"));
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
                SpielWPF.LogError("Das Laden im Hintergrund war nicht erfolgreich");
            }
        }

        public void BackgroundLoad()
        {
            using (var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true})
            {
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
                SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, ($"Fehler beim Öffnen der Tabelle {tableName}: " + ex.Message + "\n\r" + query)));
            }
            return total;
        }


        protected int GetCount(AccessDatabase? connector, string tableName, string filter)
        {
            if (connector == null)
                return -1;
            int total = 0;
            string query = string.IsNullOrEmpty(filter) ? $"SELECT count(*) FROM {tableName}": $"SELECT count(*) FROM {tableName} WHERE {filter}";
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
                SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, ($"Fehler beim Öffnen der Tabelle {tableName}: " + ex.Message + "\n\r" + query)));
            }
            return total;
        }
    }
}
