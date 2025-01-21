using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Lehensvergabe :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Lehensvergabe";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int ID { get; set; } = 0;
        public int gf { get; set; } = 0;
        public int kf { get; set; } = 0;
        public string Ruestort { get; set; } = string.Empty;
        public string Ruestortname { get; set; } = string.Empty ;
        public string Charname { get; set; } = string.Empty;
        public string Charrang { get; set; } = string.Empty;
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;

        public enum Felder
        {
            id, gf, kf, Ruestort, Ruestortname, Charname, Charrang, x, y,
        }

        public void Load(DbDataReader reader)
        {
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            this.gf = DatabaseConverter.ToInt32(reader[(int)Felder.gf]);
            this.kf = DatabaseConverter.ToInt32(reader[(int)Felder.kf]);
            this.Ruestort = DatabaseConverter.ToString(reader[(int)Felder.Ruestort]);
            this.Ruestortname = DatabaseConverter.ToString(reader[(int)Felder.Ruestortname]);
            this.Charname = DatabaseConverter.ToString(reader[(int)Felder.Charname]);
            this.Charrang = DatabaseConverter.ToString(reader[(int)Felder.Charrang]);
            this.x = DatabaseConverter.ToInt32(reader[(int)Felder.x]);
            this.y = DatabaseConverter.ToInt32(reader[(int)Felder.y]);
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            gf = {this.gf},
            kf = {this.kf},
            Ruestort = '{DatabaseConverter.EscapeString(this.Ruestort)}',
            Ruestortname = '{DatabaseConverter.EscapeString(this.Ruestortname)}',
            Charname = '{DatabaseConverter.EscapeString(this.Charname)}',
            Charrang = '{DatabaseConverter.EscapeString(this.Charrang)}',
            x = {this.x},
            y = {this.y}
        WHERE ID = {this.ID}";

           if ( command.ExecuteNonQuery() == 0)
                Insert(command);
        }


        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            gf, kf, Ruestort, Ruestortname, Charname, Charrang, x, y
        ) VALUES (
            {this.gf}, {this.kf}, 
            '{DatabaseConverter.EscapeString(this.Ruestort)}', 
            '{DatabaseConverter.EscapeString(this.Ruestortname)}', 
            '{DatabaseConverter.EscapeString(this.Charname)}', 
            '{DatabaseConverter.EscapeString(this.Charrang)}', 
            {this.x}, {this.y}
        )";

            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {TableName}
        WHERE 
            gf = {this.gf} AND
            kf = {this.kf} AND
            Ruestort = '{DatabaseConverter.EscapeString(this.Ruestort)}' AND
            Ruestortname = '{DatabaseConverter.EscapeString(this.Ruestortname)}' AND
            Charname = '{DatabaseConverter.EscapeString(this.Charname)}' AND
            Charrang = '{DatabaseConverter.EscapeString(this.Charrang)}' AND
            x = {this.x} AND
            y = {this.y}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

    }
}
