using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Personal :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "personal";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID?? "unbekanntes Personal";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public string? Vorname { get; set; }
        public string? Nachname { get; set; }
        public string? Spielername { get; set; }
        public string? Pos { get; set; }
        public string? Anschrift { get; set; }
        public string? Tel { get; set; }
        public string? Email { get; set; }
        public string? ID { get; set; }

        public enum Felder
        {
            Vorname, Nachname, Spielername, Pos, Anschrift, Tel, Email, id,
        }

        public void Load(DbDataReader reader)
        {
            this.Vorname = DatabaseConverter.ToString(reader[(int)Felder.Vorname]);
            this.Nachname = DatabaseConverter.ToString(reader[(int)Felder.Nachname]);
            this.Spielername = DatabaseConverter.ToString(reader[(int)Felder.Spielername]);
            this.Pos = DatabaseConverter.ToString(reader[(int)Felder.Pos]);
            this.Anschrift = DatabaseConverter.ToString(reader[(int)Felder.Anschrift]);
            this.Tel = DatabaseConverter.ToString(reader[(int)Felder.Tel]);
            this.Email = DatabaseConverter.ToString(reader[(int)Felder.Email]);
            this.ID = DatabaseConverter.ToString(reader[(int)Felder.id]);
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Vorname = '{DatabaseConverter.EscapeString(this.Vorname)}',
            Nachname = '{DatabaseConverter.EscapeString(this.Nachname)}',
            Spielername = '{DatabaseConverter.EscapeString(this.Spielername)}',
            Pos = '{DatabaseConverter.EscapeString(this.Pos)}',
            Anschrift = '{DatabaseConverter.EscapeString(this.Anschrift)}',
            Tel = '{DatabaseConverter.EscapeString(this.Tel)}',
            Email = '{DatabaseConverter.EscapeString(this.Email)}'
        WHERE ID = '{DatabaseConverter.EscapeString(this.ID)}'";

            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Vorname, Nachname, Spielername, Pos, Anschrift, Tel, Email, ID
        ) VALUES (
            '{DatabaseConverter.EscapeString(this.Vorname)}',
            '{DatabaseConverter.EscapeString(this.Nachname)}',
            '{DatabaseConverter.EscapeString(this.Spielername)}',
            '{DatabaseConverter.EscapeString(this.Pos)}',
            '{DatabaseConverter.EscapeString(this.Anschrift)}',
            '{DatabaseConverter.EscapeString(this.Tel)}',
            '{DatabaseConverter.EscapeString(this.Email)}',
            '{DatabaseConverter.EscapeString(this.ID)}'
        )";

            command.ExecuteNonQuery();
        }

    }
}
