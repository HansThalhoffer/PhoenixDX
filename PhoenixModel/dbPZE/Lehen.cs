using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbPZE {
    public class Lehen :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Lehen";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int ID { get; set; }
        public int reichID { get; set; }
        public int personalID { get; set; }
        public string? gemarkID { get; set; }

        public enum Felder
        {
            ID, reichID, personalID, gemarkID,
        }

        public void Load(DbDataReader reader)
        {
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.ID]);
            this.reichID = DatabaseConverter.ToInt32(reader[(int)Felder.reichID]);
            this.personalID = DatabaseConverter.ToInt32(reader[(int)Felder.personalID]);
            this.gemarkID = DatabaseConverter.ToString(reader[(int)Felder.gemarkID]);
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
