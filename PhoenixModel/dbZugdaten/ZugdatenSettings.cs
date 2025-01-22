using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    /// <summary>
    /// Die Klasse wurde genutzt, um die Altanwendung zu steuern und abzusichern, dass der Benutzer auch nicht die falsche Datei in das faLsche Zugdatenverzeichnis kopiert hat
    /// </summary>
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
        /// fortlaufende TargetID
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

        public override void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Monat = {this.Monat},
            Reich = {this.Reich},
            Reichsname = '{DatabaseConverter.EscapeString(this.Reichsname)}',
            dbversion = '{DatabaseConverter.EscapeString(this.dbversion)}',
            Ruestmonat = {this.Ruestmonat},
            Invasorflag = {this.Invasorflag},
            Audvacargeld = {this.Audvacargeld},
            Piratenflag = {this.Piratenflag},
            Phase = {this.Phase}
        WHERE id = {this.id}";
            command.ExecuteNonQuery();
        }


        public override void Insert(DbCommand command)
        {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            id, Monat, Reich, Reichsname, dbversion, Ruestmonat, Invasorflag, 
            Audvacargeld, Piratenflag, Phase
        ) VALUES (
            {this.id},
            {this.Monat},
            {this.Reich},
            '{DatabaseConverter.EscapeString(this.Reichsname)}',
            '{DatabaseConverter.EscapeString(this.dbversion)}',
            {this.Ruestmonat},
            {this.Invasorflag},
            {this.Audvacargeld},
            {this.Piratenflag},
            {this.Phase}
        )";

            command.ExecuteNonQuery();
        }
    }
}
