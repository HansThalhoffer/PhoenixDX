using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.CrossRef
{
    public class BauwerkBasis 
    {
        static Dictionary<string, BauwerkBasis> Bauwerke = [];
        static BauwerkBasis? GetBauwerk(string name)
        {
            if (Bauwerke.ContainsKey(name)) return Bauwerke[name];
            return null;
        }

        public int? Nummer { get; set; }
        public int? Baupunkte { get; set; }
        public string? Bauwerk { get; set; }

        private static readonly string[] PropertiestoIgnore = [];
        public virtual Dictionary<string, string> Properties
        {
            get
            {
                return PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
        
        public string Bezeichner { get => Bauwerk ?? "Bauwerk"; }
        public enum Felder
        {
            Nummer, Baupunkte, Bauwerk
        }
        public void Load(DbDataReader reader)
        {
            Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            Bauwerk = DatabaseConverter.ToString(reader[(int)Felder.Bauwerk]);
        }

    }

    public class Bauwerk : BauwerkBasis, IDatabaseTable, IPropertyHolder
    {
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;
       
        public new void Load(DbDataReader reader)
        {
            base.Load(reader);
        }
    }


    public class Rüstort : BauwerkBasis, IDatabaseTable, IPropertyHolder
    {
        public const string TableName = "Rüstort_crossref";
        string IDatabaseTable.TableName => TableName;

        public string? Ruestort { get; set; }
        public int? KapazitätTruppen { get; set; }
        public int? KapazitätHF { get; set; }
        public int? KapazitätZ { get; set; }
        public bool? canSieged { get; set; }

        public new enum Felder
        {
            Nummer, Baupunkte, Bauwerk, Ruestort, KapazitätTruppen, KapazitätHF, KapazitätZ, canSieged
        }
        public new void Load(DbDataReader reader)
        {
            base.Load(reader);
            Ruestort = DatabaseConverter.ToString(reader[(int)Felder.Ruestort]);
            KapazitätTruppen = DatabaseConverter.ToInt32(reader[(int)Felder.KapazitätTruppen]);
            KapazitätHF = DatabaseConverter.ToInt32(reader[(int)Felder.KapazitätHF]);
            KapazitätZ = DatabaseConverter.ToInt32(reader[(int)Felder.KapazitätZ]);
            canSieged = DatabaseConverter.ToBool(reader[(int)Felder.canSieged]);
        }
    }
}
