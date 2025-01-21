using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbCrossRef {
    public class Wall_crossref :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "wall_crossref";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        public string? wall { get; set; }

        public enum Felder
        {
            nummer, wall,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.wall = DatabaseConverter.ToString(reader[(int)Felder.wall]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Delete(DbCommand reader) => throw new NotImplementedException();

    }
}
