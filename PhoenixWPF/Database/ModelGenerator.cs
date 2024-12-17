using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PhoenixWPF.Database
{
    internal class ModelGenerator
    {
        public static void Start(string databasePath, string password, string tableName, string nspace)
        {
            string className = tableName.Substring(0,1).ToUpper() + tableName.Substring(1);
            string outputPath = className + ".cs";
            string connectionString = $"Provider = Microsoft.ACE.OLEDB.16.0; Data Source = E:\\PZE.NET\\_Data\\{databasePath}; Persist Security Info = False;";
            if (string.IsNullOrEmpty(password) == false)
            {
                connectionString += "Jet OLEDB:Database Password=" + password + ";";
            }
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand($"SELECT * FROM {tableName} WHERE 1=0", connection);
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    var schemaTable = reader.GetSchemaTable();

                    if (schemaTable != null)
                    {
                        GenerateCSharpClass(schemaTable, className, outputPath, nspace, tableName);
                    }
                }
            }
        }

        static void GenerateCSharpClass(System.Data.DataTable schemaTable, string className, string outputPath, string nspace, string tableName)
        {
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Data.Common;");
                writer.WriteLine("using PhoenixModel.Database;");
                writer.WriteLine("using PhoenixModel.Helper;");
                writer.WriteLine();
                writer.WriteLine($"namespace PhoenixModel.{nspace}");
                writer.WriteLine("{"); 
                writer.WriteLine($"public class {className}: IDatabaseTable, IEigenschaftler");
                writer.WriteLine("{");

                writer.WriteLine($"public const string TableName = \"{tableName}\"");
                writer.WriteLine("string IDatabaseTable.TableName => TableName;");
                writer.WriteLine("public string Bezeichner => id.ToString();");
                writer.WriteLine("// IEigenschaftler");
                writer.WriteLine("private static readonly string[] PropertiestoIgnore = [];");
                writer.WriteLine("public List<Eigenschaft> Eigenschaften {get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }");

                foreach (System.Data.DataRow row in schemaTable.Rows)
                {
                    string? columnName = row["ColumnName"].ToString();
                    if (columnName == null)
                        continue;
                    Type dataType = (Type)row["DataType"];
                    string propertyType = GetCSharpType(dataType);

                    writer.WriteLine($"    public {propertyType} {columnName} {{ get; set; }}");
                }
                writer.WriteLine();
                writer.WriteLine("  public enum Felder");
                writer.WriteLine("{");
                string line = string.Empty;
                foreach (System.Data.DataRow row in schemaTable.Rows)
                {
                    string? columnName = row["ColumnName"].ToString();
                    if (columnName == null)
                        continue;
                    line += $"{columnName}, ";
                    if (line.Length > 250)
                    {
                        writer.WriteLine(line);
                        line = string.Empty;
                    }
                }
                line = line.Substring(0, line.Length - 1);
                writer.WriteLine(line);

                writer.WriteLine("}");
                writer.WriteLine();
                writer.WriteLine("    public void Load(DbDataReader reader)");
                writer.WriteLine("    {");

                foreach (System.Data.DataRow row in schemaTable.Rows)
                {
                    string? columnName = row["ColumnName"].ToString();
                    if (columnName == null)
                        continue;
                    Type dataType = (Type)row["DataType"];
                    string methodName = GetConverterMethodName(dataType);

                    writer.WriteLine($"        this.{columnName} = DatabaseConverter.{methodName}(reader[(int) Felder.{columnName}]);");
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.WriteLine("}");
            }
        }

        static string GetCSharpType(Type dataType)
        {
            if (dataType == typeof(int)) return "int";
            if (dataType == typeof(short)) return "short";
            if (dataType == typeof(long)) return "long";
            if (dataType == typeof(decimal)) return "decimal";
            if (dataType == typeof(double)) return "int";
            if (dataType == typeof(float)) return "float";
            if (dataType == typeof(bool)) return "bool";
            if (dataType == typeof(string)) return "string?";
            if (dataType == typeof(DateTime)) return "DateTime";
            if (dataType == typeof(byte[])) return "byte[]";
            return "object"; // Fallback for unrecognized types
        }

        static string GetConverterMethodName(Type dataType)
        {
            if (dataType == typeof(int)) return "ToInt32";
            if (dataType == typeof(short)) return "ToInt16";
            if (dataType == typeof(long)) return "ToInt64";
            if (dataType == typeof(decimal)) return "ToDecimal";
            if (dataType == typeof(double)) return "ToInt32";
            if (dataType == typeof(float)) return "ToSingle";
            if (dataType == typeof(bool)) return "ToBool";
            if (dataType == typeof(string)) return "ToString";
            if (dataType == typeof(DateTime)) return "ToDateTime";
            if (dataType == typeof(byte[])) return "ToByteArray";
            return "ToObject"; // Fallback for unrecognized types
        }
    }
}
