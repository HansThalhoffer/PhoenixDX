using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbErkenfara {
    public class Zugreihenfolge :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Zugreihenfolge";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Monat { get; set; }
        public int Reihenfolge { get; set; }
        public string? Reich { get; set; }
        public string? Auftauchpunkt_A { get; set; }
        public int ID { get; set; }

        public enum Felder
        {
            Monat, Reihenfolge, Reich, Auftauchpunkt_A, id,
        }

        public void Load(DbDataReader reader)
        {
            this.Monat = DatabaseConverter.ToString(reader[(int)Felder.Monat]);
            this.Reihenfolge = DatabaseConverter.ToInt32(reader[(int)Felder.Reihenfolge]);
            this.Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            this.Auftauchpunkt_A = DatabaseConverter.ToString(reader[(int)Felder.Auftauchpunkt_A]);
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Monat = '{DatabaseConverter.EscapeString(this.Monat)}',
            Reihenfolge = {this.Reihenfolge},
            Reich = '{DatabaseConverter.EscapeString(this.Reich)}',
            Auftauchpunkt_A = '{DatabaseConverter.EscapeString(this.Auftauchpunkt_A)}'
        WHERE ID = {this.ID}";

            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Monat, Reihenfolge, Reich, Auftauchpunkt_A, ID
        ) VALUES (
            '{DatabaseConverter.EscapeString(this.Monat)}',
            {this.Reihenfolge},
            '{DatabaseConverter.EscapeString(this.Reich)}',
            '{DatabaseConverter.EscapeString(this.Auftauchpunkt_A)}',
            {this.ID}
        )";

            command.ExecuteNonQuery();
        }

    }
}
