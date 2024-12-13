using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
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
    public class DatabaseLoader
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

        protected void Load<T> (AccessDatabase? connector, BlockingCollection<T>? collection, string[] felder) where T : IDatabaseTable, new()
        {
            if (connector == null || collection == null)
                return;
            int total = 0;
            SharedData.Nationen = new BlockingCollection<Nation>();
            string felderListe = string.Join(", ", felder);
            using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM ORDER BY {felder[0]}"))
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

        /// <summary>
        /// Finds the value of the "Bezeichner" property in the given object.
        /// </summary>
        /// <param name="obj">The object to search for the property.</param>
        /// <returns>The value of the "Bezeichner" property if found; otherwise, null.</returns>
        public static string FindBezeichnerPropertyValue(object? obj)
        {
            // Get the PropertyInfo for the "Bezeichner" property
            var propertyInfo = obj?.GetType().GetProperty("Bezeichner", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Get the value of the "Bezeichner" property
            return propertyInfo?.GetValue(obj)?.ToString() ?? string.Empty;
        }

        protected void Load<T>(LoadObject<T> objReader, AccessDatabase? connector, BlockingDictionary<T>? collection, string[] felder)
        {
            if (connector == null || collection == null)
                return;
            int total = 0;
            SharedData.Nationen = new BlockingCollection<Nation>();
            string felderListe = string.Join(", ", felder);
            using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM ORDER BY {felder[0]}"))
            {
                while (reader != null && reader.Read())
                {
                    T obj = objReader(reader);
                    string key = FindBezeichnerPropertyValue(obj);
                    collection.Add(key, obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T)} geladen"));
        }
    }
}
