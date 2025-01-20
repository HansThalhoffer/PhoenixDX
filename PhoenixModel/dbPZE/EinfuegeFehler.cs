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
    internal class EinfuegeFehler : IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Einfuegefehler";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public string? Reich { get; set; }
        public string? Referenzreich { get; set; }
        public int? Wegerecht { get; set; }
        public int? Wegerecht_von { get; set; }
        public string? DBname { get; set; }
        public int? Kuestenrecht { get; set; }
        public int? Kuestenrecht_von { get; set; }
        public int? Flottenkey { get; set; }

        public string Bezeichner => throw new NotImplementedException();
        enum Felder
        {
            Reich,
            Referenzreich,
            Wegerecht,
            Wegerecht_von,
            DBname,
            Kuestenrecht,
            Kuestenrecht_von,
            Flottenkey
        }
        public void Load(DbDataReader reader)
        {
            Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            Referenzreich = DatabaseConverter.ToString(reader[(int)Felder.Referenzreich]);
            Wegerecht = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht]);
            Wegerecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht_von]);
            DBname = DatabaseConverter.ToString(reader[(int)Felder.DBname]);
            Kuestenrecht = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht]);
            Kuestenrecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht_von]);
            Flottenkey = DatabaseConverter.ToInt32(reader[(int)Felder.Flottenkey]);
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
