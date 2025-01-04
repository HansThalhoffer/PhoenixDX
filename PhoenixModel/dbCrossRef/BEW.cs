using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbCrossRef
{
    public class BEW
    {
        public string Bezeichner => Gelaende.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int Gelaende { get; set; }
        public string? Gelaendetext { get; set; }
        public int standart { get; set; }
        public int strasse { get; set; }
        public int wegerecht { get; set; }
        public int strasse_und_wegerecht { get; set; }
        public int hoehenstufe { get; set; }

        public enum Felder
        {
            Gelaende, Gelaendetext, standart, strasse, wegerecht, strasse_und_wegerecht, hoehenstufe,
        }

        public void Load(DbDataReader reader)
        {
            this.Gelaende = DatabaseConverter.ToInt32(reader[(int)Felder.Gelaende]);
            this.Gelaendetext = DatabaseConverter.ToString(reader[(int)Felder.Gelaendetext]);
            this.standart = DatabaseConverter.ToInt32(reader[(int)Felder.standart]);
            this.strasse = DatabaseConverter.ToInt32(reader[(int)Felder.strasse]);
            this.wegerecht = DatabaseConverter.ToInt32(reader[(int)Felder.wegerecht]);
            this.strasse_und_wegerecht = DatabaseConverter.ToInt32(reader[(int)Felder.strasse_und_wegerecht]);
            this.hoehenstufe = DatabaseConverter.ToInt32(reader[(int)Felder.hoehenstufe]);
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
