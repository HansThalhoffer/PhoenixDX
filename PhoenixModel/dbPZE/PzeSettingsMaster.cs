using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbPZE {
    public class PzeSettingsMaster :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Settings_Master";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"{this.Monat} {this.Reich}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public virtual List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        /// <summary>
        /// Der aktuelle Zugmonat, für die diese Datenbank gilt
        /// </summary>
        public int Monat { get; set; }
        /// <summary>
        /// Das Reich, für die diese Datenbank gilt
        /// </summary>
        public int Reich { get; set; }
        /// <summary>
        /// Name des Reiches 
        /// </summary>
        public string? Reichsname { get; set; }
        /// <summary>
        /// Versionsnummer für die Altanwendung = 3.1
        /// </summary>
        public string? dbversion { get; set; }
        /// <summary>
        /// ist der aktuelle Monat ein Rüstmonat = 1 ansonsten 0
        /// </summary>
        public int Ruestmonat { get; set; }
        /// <summary>
        /// Ist das Reich ein Invasor = 1 ansonsten 0
        /// </summary>
        public int Invasorflag { get; set; }
        /// <summary>
        /// Erhält das Reich Geld aus der Audvarcar Regel
        /// </summary>
        public int Audvacargeld { get; set; }

        public enum Felder
        {
            Monat, Reich, Reichsname, dbversion, Ruestmonat, Invasorflag, Audvacargeld,
        }

        public virtual void Load(DbDataReader reader)
        {
            this.Monat = DatabaseConverter.ToInt32(reader[(int)Felder.Monat]);
            this.Reich = DatabaseConverter.ToInt32(reader[(int)Felder.Reich]);
            this.Reichsname = DatabaseConverter.ToString(reader[(int)Felder.Reichsname]);
            this.dbversion = DatabaseConverter.ToString(reader[(int)Felder.dbversion]);
            this.Ruestmonat = DatabaseConverter.ToInt32(reader[(int)Felder.Ruestmonat]);
            this.Invasorflag = DatabaseConverter.ToInt32(reader[(int)Felder.Invasorflag]);
            this.Audvacargeld = DatabaseConverter.ToInt32(reader[(int)Felder.Audvacargeld]);
        }

        public virtual void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public virtual void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(DbCommand reader) => throw new NotImplementedException();
    }
}
