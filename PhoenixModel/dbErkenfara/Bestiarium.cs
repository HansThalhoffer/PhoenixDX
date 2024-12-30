using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbErkenfara
{
    public class Bestiarium : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "bestiarium";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Kreaturenname ?? "unbekannte Kreatur";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Kreaturenname { get; set; }
        public string? Beschreibung1 { get; set; }
        public string? Beschreibung2 { get; set; }
        public string? Beschreibung3 { get; set; }
        public string? Beschreibung4 { get; set; }
        public string? Waffengattung { get; set; }
        public int GP { get; set; }
        public int HF { get; set; }
        public int Stärke { get; set; }
        public string? IMG { get; set; }
        public int BP { get; set; }

        public enum Felder
        {
            Kreaturenname, Beschreibung1, Beschreibung2, Beschreibung3, Beschreibung4, Waffengattung, GP, HF, Stärke, IMG, BP,
        }

        public void Load(DbDataReader reader)
        {
            this.Kreaturenname = DatabaseConverter.ToString(reader[(int)Felder.Kreaturenname]);
            this.Beschreibung1 = DatabaseConverter.ToString(reader[(int)Felder.Beschreibung1]);
            this.Beschreibung2 = DatabaseConverter.ToString(reader[(int)Felder.Beschreibung2]);
            this.Beschreibung3 = DatabaseConverter.ToString(reader[(int)Felder.Beschreibung3]);
            this.Beschreibung4 = DatabaseConverter.ToString(reader[(int)Felder.Beschreibung4]);
            this.Waffengattung = DatabaseConverter.ToString(reader[(int)Felder.Waffengattung]);
            this.GP = DatabaseConverter.ToInt32(reader[(int)Felder.GP]);
            this.HF = DatabaseConverter.ToInt32(reader[(int)Felder.HF]);
            this.Stärke = DatabaseConverter.ToInt32(reader[(int)Felder.Stärke]);
            this.IMG = DatabaseConverter.ToString(reader[(int)Felder.IMG]);
            this.BP = DatabaseConverter.ToInt32(reader[(int)Felder.BP]);
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
