using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbPZE {
    public class PzeUsersetting :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "usersetting";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int ID { get; set; }
        public string? item { get; set; }
        public int fieldsize { get; set; }

        public enum Felder
        {
            id, item, fieldsize,
        }

        public void Load(DbDataReader reader)
        {
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            this.item = DatabaseConverter.ToString(reader[(int)Felder.item]);
            this.fieldsize = DatabaseConverter.ToInt32(reader[(int)Felder.fieldsize]);
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
