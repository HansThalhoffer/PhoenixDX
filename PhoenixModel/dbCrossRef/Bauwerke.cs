using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbCrossRef
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
        public virtual List<Eigenschaft> Eigenschaften
        {
            get
            {
                return PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
        
        public string Bezeichner { get => Bauwerk ?? "Gebäude"; }
    }

    // kosten von Wall etc
    public class Bauwerk : BauwerkBasis, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;

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

    // referenzliste
    public class Rüstort : BauwerkBasis, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "ruestort_crossref";
        string IDatabaseTable.TableName => TableName;
        public static Dictionary<int, Rüstort> NachBaupunkten = [];

        public string? Ruestort { get; set; }
        public int? KapazitätTruppen { get; set; }
        public int? KapazitätHF { get; set; }
        public int? KapazitätZ { get; set; }
        public bool? canSieged { get; set; }

        public enum Felder
        {
            nummer, ruestort, Baupunkte, Kapazitaet_truppen, Kapazitaet_HF, Kapazitaet_Z, canSieged
        }

        public void Load(DbDataReader reader)
        {
            Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            Bauwerk = DatabaseConverter.ToString(reader[(int)Felder.ruestort]);
            Ruestort = DatabaseConverter.ToString(reader[(int)Felder.ruestort]);
            KapazitätTruppen = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_truppen]);
            KapazitätHF = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_HF]);
            KapazitätZ = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_Z]);
            canSieged = DatabaseConverter.ToBool(reader[(int)Felder.canSieged]);
            if (Baupunkte > 0)
                NachBaupunkten.Add(Baupunkte.Value, this);
        }
    }
}
