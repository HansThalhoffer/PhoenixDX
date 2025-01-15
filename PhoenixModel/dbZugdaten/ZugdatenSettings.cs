using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class ZugdatenSettings:  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "settings";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => id.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int id { get; set; }
        public int Monat { get; set; }
        public int Reich { get; set; }
        public string? Reichsname { get; set; }
        public string? dbversion { get; set; }
        public int Ruestmonat { get; set; }
        public int Invasorflag { get; set; }
        public int Audvacargeld { get; set; }
        public int Piratenflag { get; set; }
        public int Phase { get; set; }

        public enum Felder
        {
            id, Monat, Reich, Reichsname, dbversion, Ruestmonat, Invasorflag, Audvacargeld, Piratenflag, Phase,
        }

        public void Load(DbDataReader reader)
        {
            this.id = DatabaseConverter.ToInt32(reader[(int) Felder.id]);
            this.Monat = DatabaseConverter.ToInt32(reader[(int) Felder.Monat]);
            this.Reich = DatabaseConverter.ToInt32(reader[(int) Felder.Reich]);
            this.Reichsname = DatabaseConverter.ToString(reader[(int) Felder.Reichsname]);
            this.dbversion = DatabaseConverter.ToString(reader[(int) Felder.dbversion]);
            this.Ruestmonat = DatabaseConverter.ToInt32(reader[(int) Felder.Ruestmonat]);
            this.Invasorflag = DatabaseConverter.ToInt32(reader[(int) Felder.Invasorflag]);
            this.Audvacargeld = DatabaseConverter.ToInt32(reader[(int) Felder.Audvacargeld]);
            this.Piratenflag = DatabaseConverter.ToInt32(reader[(int) Felder.Piratenflag]);
            this.Phase = DatabaseConverter.ToInt32(reader[(int) Felder.Phase]);
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
