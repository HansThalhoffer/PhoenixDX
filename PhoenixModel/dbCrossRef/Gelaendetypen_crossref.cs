using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbCrossRef {
    public class Gelaendetypen_crossref :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Gelaendetypen_crossref";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Feldtyp { get; set; }
        public int Nummer { get; set; }

        public enum Felder
        {
            Feldtyp, Nummer,
        }

        public void Load(DbDataReader reader)
        {
            this.Feldtyp = DatabaseConverter.ToString(reader[(int)Felder.Feldtyp]);
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Delete(DbCommand reader) {
            throw new NotImplementedException();
        }
    }
}
