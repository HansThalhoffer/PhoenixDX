using Microsoft.Win32;
using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Windows;

namespace PhoenixWPF.Database
{


    public class AccessDatabase : IDisposable
    {
        private readonly OleDbConnection _connection;

        public static string GetInstalledAceOleDbProvider()
        {
            // Registry path for OLEDB Providers
            // string registryPath32Bit = @"SOFTWARE\WOW6432Node\Classes\Microsoft.ACE.OLEDB.";
            string registryPath64Bit = @"SOFTWARE\Classes\Microsoft.ACE.OLEDB.";

            string? result = null;

            // Check for ACE.OLEDB providers by iterating over known versions
            string[] versions = { "18.0", "17.0", "16.0", "15.0", "14.0", "12.0", "11.0", "10.0", "8.0", "4.0" }; // Known versions of ACE OLEDB provider
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

            return "No Microsoft.ACE.OLEDB provider installed.";
        }

        public AccessDatabase(string databaseFilePath, string? pw)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
                throw new ArgumentException("Database file path must be provided.", nameof(databaseFilePath));

            string provider = GetInstalledAceOleDbProvider();
            if (provider == null)
            {
                MessageBox.Show("Es ist kein Microsoft.ACE.OLEDB Treiber installiert. Bitte einen entsprechenden Treiber installieren");
            }
            string connectionString = $@"Provider={provider};Data Source={databaseFilePath};Persist Security Info=False;";
            
            if (string.IsNullOrEmpty(pw) == false)
            {
                connectionString += "Jet OLEDB:Database Password=" + pw + ";";
            }
            _connection = new OleDbConnection(connectionString);

        }

        public bool IsConnected()
        {
            return _connection != null && _connection.State == ConnectionState.Open;
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
                    MessageBox.Show("Fehler beim Öffnen der PZE Datenbank: " + ex.Message);
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
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        /// <summary>
        /// Executes a query that returns a result set.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A DataTable containing the result set.</returns>
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
        public DbDataReader OpenReader(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query must be provided.", nameof(query));

            using var command = new OleDbCommand(query, _connection);
            return command.ExecuteReader();
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

        /// <summary>
        /// converts a database field object to Int
        /// </summary>
        static public int ToInt(object o)
        {
            try
            {
                if (o == null)
                    throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
                if (o is DBNull)
                    return 0;
                if (o.GetType() == typeof(int))
                    return Convert.ToInt32(o);
                if (o.GetType() == typeof(double))
                    return Convert.ToInt32(o);
                if (o.GetType() == typeof(float))
                    return Convert.ToInt32(o);

                string? s = o.ToString();
                if (String.IsNullOrEmpty(s))
                    return 0;
                return int.Parse(s);
            }
            catch { }
            return -1;
        }

        /// <summary>
        /// converts a database field object to Int
        /// </summary>
        static public string ToString(object o)
        {
            if (o == null)
                throw new ArgumentNullException("Unbekanntes Feld in der Tabelle"); 
            if (o is DBNull)
                return string.Empty;
            string? s = o.ToString();
            if (String.IsNullOrEmpty(s))
                return string.Empty;
            return s;
        }
    }
}