using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace PhoenixWPF.Database
{


    public class AccessDatabase : IDisposable
    {
        private readonly OleDbConnection _connection;

        public AccessDatabase(string databaseFilePath, string? pw)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
                throw new ArgumentException("Database file path must be provided.", nameof(databaseFilePath));

            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databaseFilePath};Persist Security Info=False;";
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
                    Console.WriteLine("Fehler beim Öffnen der PZE Datenbank: " + ex.Message);
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