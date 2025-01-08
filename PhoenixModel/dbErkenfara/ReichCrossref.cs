using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbErkenfara
{
    public class ReichCrossref : IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public virtual string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }

        public const string TableName = "Reich_crossref";
        string IDatabaseTable.TableName => ReichCrossref.TableName;
        public string Bezeichner => $"{Referenzreich}/{Reich}";

        protected virtual string GetTableName () { return TableName; }

        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DBname", "Nummer", "DatabaseName", "Flottenkey"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        /// <summary>
        ///  das eigene Reich bzw. das Reich, das dem anderen die Rechte gibt
        /// </summary>
        public string? Referenzreich { get; set; }
        /// <summary>
        /// Das Reich, das die Rechte empfängt
        /// </summary>
        public int Wegerecht { get; set; }
        public int Kuestenrecht { get; set; }
        public string? Reich { get; set; }
        public int Wegerecht_von { get; set; }
        public string? DBname { get; set; }
        public int Kuestenrecht_von { get; set; }
        public int Flottenkey { get; set; }

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
            WHERE Nummer = {this.Nummer}";

            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command)
        {
            command.CommandText = $@"
            INSERT INTO {GetTableName()} (
                Nummer, Reich, Referenzreich, Wegerecht, Wegerecht_von, DBname, 
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
    }
}
