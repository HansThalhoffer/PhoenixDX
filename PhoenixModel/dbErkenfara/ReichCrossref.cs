using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbErkenfara {
    public class ReichCrossref : IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public virtual string Database { get { return DatabaseName; } set { DatabaseName = value; } }

        public const string TableName = "Reich_crossref";
        string IDatabaseTable.TableName => ReichCrossref.TableName;
        public string Bezeichner => $"{ReferenzNation.Bezeichner}=>{Nation.Bezeichner}";

        protected virtual string GetTableName () { return TableName; }

        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DBname", "TargetID", "DatabaseName", "Database", "Flottenkey", "ReferenzNation", "Nation"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        public string Geber { get => ReferenzNation.Reich; }
        public int Wegerecht { get; set; }
        public int Kuestenrecht { get; set; }
        public string Empfänger { get => Nation.Reich; }
        public int Wegerecht_von { get; set; }
        public string? DBname { get; set; }
        public int Kuestenrecht_von { get; set; }
        public int Flottenkey { get; set; }


        /// <summary>
        ///  das eigene Reich bzw. das Reich, das dem anderen die Rechte gibt
        ///  da hier viele Namensirrungen existieren, sollte ReferenzNation verwendet werden
        /// </summary>
        protected string Referenzreich { get; set; } = string.Empty;
        /// <summary>
        /// Das Reich, das die Rechte empfängt
        ///  da hier viele Namensirrungen existieren, sollte Nation verwendet werden
        /// </summary>
        protected string Reich { get; set; } = string.Empty;

        Nation? _ReferenzNation = null;
        public Nation ReferenzNation { get => _ReferenzNation ??= NationenView.GetNationFromString(Referenzreich); }
        Nation? _Nation = null;
        public Nation Nation { get => _Nation ??= NationenView.GetNationFromString(Reich); }

        public enum Felder
        {
            Nummer, Reich, Referenzreich, Wegerecht, Wegerecht_von, DBname, Kuestenrecht, Kuestenrecht_von, Flottenkey,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            this.Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            this.Referenzreich = DatabaseConverter.ToString(reader[(int)Felder.Referenzreich]);
            this.Wegerecht = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht]);
            this.Wegerecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht_von]);
            this.DBname = DatabaseConverter.ToString(reader[(int)Felder.DBname]);
            this.Kuestenrecht = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht]);
            this.Kuestenrecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht_von]);
            this.Flottenkey = DatabaseConverter.ToInt32(reader[(int)Felder.Flottenkey]);
        }

        public void Save(DbCommand command)
        {
            command.CommandText = $@"
            UPDATE {GetTableName()} SET
                Reich = '{DatabaseConverter.EscapeString(this.Reich)}',
                Referenzreich = '{DatabaseConverter.EscapeString(this.Referenzreich)}',
                Wegerecht = {this.Wegerecht},
                Wegerecht_von = {this.Wegerecht_von},
                DBname = '{DatabaseConverter.EscapeString(this.DBname)}',
                Kuestenrecht = {this.Kuestenrecht},
                Kuestenrecht_von = {this.Kuestenrecht_von},
                Flottenkey = {this.Flottenkey}
            WHERE TargetID = {this.Nummer}";

            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command)
        {
            command.CommandText = $@"
            INSERT INTO {GetTableName()} (
                TargetID, Reich, Referenzreich, Wegerecht, Wegerecht_von, DBname, 
                Kuestenrecht, Kuestenrecht_von, Flottenkey
            ) VALUES (
                {this.Nummer}, 
                '{DatabaseConverter.EscapeString(this.Reich)}', 
                '{DatabaseConverter.EscapeString(this.Referenzreich)}', 
                {this.Wegerecht}, 
                {this.Wegerecht_von}, 
                '{DatabaseConverter.EscapeString(this.DBname)}', 
                {this.Kuestenrecht}, 
                {this.Kuestenrecht_von}, 
                {this.Flottenkey}
            )";

            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand reader) => throw new NotImplementedException();
    }
}
