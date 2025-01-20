using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbCrossRef {
    public class Teleportpunkte :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Teleportpunkte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Feld ?? "unbekannter Teleportpunkt";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Feld { get; set; }
        public string? Art { get; set; }

        public enum Felder
        {
            Feld, Art,
        }

        public void Load(DbDataReader reader)
        {
            this.Feld = DatabaseConverter.ToString(reader[(int)Felder.Feld]);
            this.Art = DatabaseConverter.ToString(reader[(int)Felder.Art]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
