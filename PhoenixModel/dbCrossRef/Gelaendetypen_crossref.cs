using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class Gelaendetypen_crossref :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
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
    }
}
