using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbPZE
{
    public class PzeTempsettings : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "tempsettings";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"{this.monat} {this.reichsnummer}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int monat { get; set; }
        public int reichsnummer { get; set; }
        public string? reichsname { get; set; }
        public string? dbname { get; set; }
        public string? dbpass { get; set; }

        public enum Felder
        {
            monat, reichsnummer, reichsname, dbname, dbpass,
        }

        public void Load(DbDataReader reader)
        {
            this.monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
            this.reichsnummer = DatabaseConverter.ToInt32(reader[(int)Felder.reichsnummer]);
            this.reichsname = DatabaseConverter.ToString(reader[(int)Felder.reichsname]);
            this.dbname = DatabaseConverter.ToString(reader[(int)Felder.dbname]);
            this.dbpass = DatabaseConverter.ToString(reader[(int)Felder.dbpass]);
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
