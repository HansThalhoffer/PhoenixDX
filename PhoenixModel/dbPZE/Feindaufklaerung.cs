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
    public class Feindaufklaerung :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "Feindaufklaerung";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => id.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int id { get; set; } = 0;
        public int? Nummer { get; set; }
        public string? Reich { get; set; }
        public string? Art { get; set; }
        public int? GF { get; set; }
        public int? KF { get; set; }
        public string? Notiz { get; set; }


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
            GF = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            KF = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
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
