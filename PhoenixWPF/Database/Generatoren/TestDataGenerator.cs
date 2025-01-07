using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Database.Generatoren
{
    internal class TestDataGenerator
    {

        public static void Start(string databasePath, string password)
        {
            
            string connectionString = $"Provider = Microsoft.ACE.OLEDB.16.0; Data Source = E:\\PZE.NET\\_Data\\{databasePath}; Persist Security Info = False;";
            if (string.IsNullOrEmpty(password) == false)
            {
                connectionString += "Jet OLEDB:Database Password=" + password + ";";
            }
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand($"SELECT * FROM Krieger WHERE 1=0", connection);
                var result = command.ExecuteNonQuery();
                
            }
        }
    }
}
