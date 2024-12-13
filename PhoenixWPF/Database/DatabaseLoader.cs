using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
using SharpDX.DirectWrite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var propertyInfo = typeof(T).GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
            string tabeName = propertyInfo?.GetValue(null,null)?.ToString() ?? string.Empty;
using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM {tabeName} ORDER BY {felder[0]}"))
            {
                while (reader != null && reader.Read())
                {
                    T obj = new T();
                    typeof(T).GetMethod("Load")?.Invoke(obj,[reader]);
                    collection.Add(obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T)} geladen"));
        }


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
            Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T)} geladen"));
        }

        
    }
}
