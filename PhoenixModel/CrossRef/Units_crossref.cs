using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class Units_crossref : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Units_crossref";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int ID { get; set; }
        public string? Unittype { get; set; }
        public int BP_max { get; set; }

        public enum Felder
        {
            ID, Unittype, BP_max,
        }

        public void Load(DbDataReader reader)
        {
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.ID]);
            this.Unittype = DatabaseConverter.ToString(reader[(int)Felder.Unittype]);
            this.BP_max = DatabaseConverter.ToInt32(reader[(int)Felder.BP_max]);
        }
    }
}
