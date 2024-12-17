using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbErkenfara
{
    public class Weltbilanz : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Weltbilanz";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ReichId ?? "unbekanntes Reich";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Reichname { get; set; }
        public string? ReichId { get; set; }
        public int Einnahmen { get; set; }

        public enum Felder
        {
            Reichname, ReichId, Einnahmen,
        }

        public void Load(DbDataReader reader)
        {
            this.Reichname = DatabaseConverter.ToString(reader[(int)Felder.Reichname]);
            this.ReichId = DatabaseConverter.ToString(reader[(int)Felder.ReichId]);
            this.Einnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.Einnahmen]);
        }
    }
}
