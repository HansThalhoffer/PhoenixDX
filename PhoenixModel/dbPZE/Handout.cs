using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbPZE
{
    public class Handout : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Handout";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        public int gf_von { get; set; }
        public int kf_von { get; set; }
        public int gf_nach { get; set; }
        public int kf_nach { get; set; }
        public string? fusmit { get; set; }

        public enum Felder
        {
            nummer, gf_von, kf_von, gf_nach, kf_nach, fusmit,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.gf_von = DatabaseConverter.ToInt32(reader[(int)Felder.gf_von]);
            this.kf_von = DatabaseConverter.ToInt32(reader[(int)Felder.kf_von]);
            this.gf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.gf_nach]);
            this.kf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.kf_nach]);
            this.fusmit = DatabaseConverter.ToString(reader[(int)Felder.fusmit]);
        }
    }
}
