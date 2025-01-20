using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbErkenfara {
    public class Weltbilanz :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Weltbilanz";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ReichId ?? "unbekanntes Nation";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
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
