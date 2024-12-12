using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
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

        public BauwerkBasis(int? nummer, int? baupunkte, string? bauwerk)
        {
            Nummer = nummer;
            Baupunkte = baupunkte;
            Bauwerk = bauwerk;
            if (bauwerk != null)
            {
                Bauwerke[bauwerk] = this;
            }
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
        
        public string Bezeichner { get => Bauwerk ?? "Null"; }

    }

    public class Bauwerk : BauwerkBasis, IDatabaseTable, IPropertyHolder
    {
        Bauwerk(int? nummer, int? baupunkte, string? bauwerk): base (nummer, baupunkte, bauwerk)
        {
        
        }
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;
    }


    public class Rüstort : BauwerkBasis, IDatabaseTable, IPropertyHolder
    {
        Rüstort(int? nummer, int? baupunkte, string? bauwerk) : base(nummer, baupunkte, bauwerk)
        {
        }
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;
        public string? Ruestort { get; set; }
        public int? KapazitätTruppen { get; set; }
        public int? KapazitätHF { get; set; }
        public int? KapazitätZ { get; set; }
        public bool? canSieged { get; set; }
    }
}
