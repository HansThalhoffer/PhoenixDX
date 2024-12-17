using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class Ruestort_crossref : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "ruestort_crossref";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        public string? ruestort { get; set; }
        public int Baupunkte { get; set; }
        public int Kapazitaet_truppen { get; set; }
        public int Kapazitaet_HF { get; set; }
        public int Kapazitaet_Z { get; set; }
        public bool canSieged { get; set; }

        public enum Felder
        {
            nummer, ruestort, Baupunkte, Kapazitaet_truppen, Kapazitaet_HF, Kapazitaet_Z, canSieged,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.ruestort = DatabaseConverter.ToString(reader[(int)Felder.ruestort]);
            this.Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            this.Kapazitaet_truppen = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_truppen]);
            this.Kapazitaet_HF = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_HF]);
            this.Kapazitaet_Z = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_Z]);
            this.canSieged = DatabaseConverter.ToBool(reader[(int)Felder.canSieged]);
        }
    }
}
