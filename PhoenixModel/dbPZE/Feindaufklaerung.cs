using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE {
    public class Feindaufklaerung: KleinfeldPosition, IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Feindaufklaerung";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => id.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int id { get; set; } = 0;
        public int Nummer { get; set; } = 0;
        public string Reich { get; set; } = string.Empty;
        public string Art { get; set; } = string.Empty;
        public string Notiz { get; set; } = string.Empty;

        public enum Felder
        {
            id, Nummer, Reich, Art, GF, KF, Notiz
        }
        public void Load(DbDataReader reader)
        {
            id = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            Art = DatabaseConverter.ToString(reader[(int)Felder.Art]);
            gf = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            kf = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            Notiz = DatabaseConverter.ToString(reader[(int)Felder.Notiz]);
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
