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
                return PropertiesProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
        
        public string Bezeichner { get => Bauwerk ?? "Bauwerk"; }
        public enum Felder
        {
            Nummer, Baupunkte, Bauwerk
        }
        public void Load(DbDataReader reader)
        {
            Nummer = reader.GetInt32((int)Felder.Nummer);
            Baupunkte = reader.GetInt32((int)Felder.Baupunkte);
            Bauwerk = reader.GetString((int)Felder.Bauwerk);
        }

    }

    public class Bauwerk : BauwerkBasis, IDatabaseTable, IPropertyHolder
    {
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;
       
        public void Load(DbDataReader reader)
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
        public void Load(DbDataReader reader)
        {
            base.Load(reader);
            Ruestort = reader.GetString((int)Felder.Ruestort);
            KapazitätTruppen = reader.GetInt32((int)Felder.KapazitätTruppen);
            KapazitätHF = reader.GetInt32((int)Felder.KapazitätHF);
            KapazitätZ = reader.GetInt32((int)Felder.KapazitätZ);
            canSieged = reader.GetBoolean((int)Felder.canSieged);
        }
    }
}
