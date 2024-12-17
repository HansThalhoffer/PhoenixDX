using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;

namespace PhoenixModel.dbPZE
{
    public class PzeSettingsMaster : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Settings_Master";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"{this.Monat} {this.Reich}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Monat { get; set; }
        public int Reich { get; set; }
        public string? Reichsname { get; set; }
        public string? dbversion { get; set; }
        public int Ruestmonat { get; set; }
        public int Invasorflag { get; set; }
        public int Audvacargeld { get; set; }

        public enum Felder
        {
            Monat, Reich, Reichsname, dbversion, Ruestmonat, Invasorflag, Audvacargeld,
        }

        public void Load(DbDataReader reader)
        {
            this.Monat = DatabaseConverter.ToInt32(reader[(int)Felder.Monat]);
            this.Reich = DatabaseConverter.ToInt32(reader[(int)Felder.Reich]);
            this.Reichsname = DatabaseConverter.ToString(reader[(int)Felder.Reichsname]);
            this.dbversion = DatabaseConverter.ToString(reader[(int)Felder.dbversion]);
            this.Ruestmonat = DatabaseConverter.ToInt32(reader[(int)Felder.Ruestmonat]);
            this.Invasorflag = DatabaseConverter.ToInt32(reader[(int)Felder.Invasorflag]);
            this.Audvacargeld = DatabaseConverter.ToInt32(reader[(int)Felder.Audvacargeld]);
        }
    }
}
