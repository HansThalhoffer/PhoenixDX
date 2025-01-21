using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class ZugdatenSettings: PzeSettingsMaster, IDatabaseTable, IEigenschaftler
    {
        public static new string DatabaseName { get; set;  } = string.Empty;
        public new string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public new const string TableName = "settings";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public override List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        /// <summary>
        /// fortlaufende Nummer
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Der aktuelle Zugmonat, für die diese Datenbank gilt
        /// </summary>
        public int Piratenflag { get; set; }
        /// <summary>
        /// In welcher Phase befindet sich der Zug
        /// 0 - Rüstphase
        /// 1 - Bewegungsphase
        /// </summary>
        public int Phase { get; set; }

        public new enum Felder
        {
            id, Monat, Reich, Reichsname, dbversion, Ruestmonat, Invasorflag, Audvacargeld, Piratenflag, Phase,
        }

        public override void Load(DbDataReader reader)
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

        public override void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public override void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
