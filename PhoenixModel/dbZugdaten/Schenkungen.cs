using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Schenkungen : IDatabaseTable, ISelectable {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Schenkungen";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        private static readonly string[] PropertiestoIgnore = ["ID","Bezeichner", "Database"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Schenkung_bekommen { get; set; }
        public string? Schenkung_bekommenID { get; set; }
        public int Schenkung_an { get; set; }
        public string? Schenkung_anID { get; set; }
        public int monat { get; set; }
        public int ID { get; set; } = 0;

        public enum Felder
        {
            Schenkung_bekommen, Schenkung_bekommenID, Schenkung_an, Schenkung_anID, monat, id,
        }

        public void Load(DbDataReader reader)
        {
            this.Schenkung_bekommen = DatabaseConverter.ToInt32(reader[(int)Felder.Schenkung_bekommen]);
            this.Schenkung_bekommenID = DatabaseConverter.ToString(reader[(int)Felder.Schenkung_bekommenID]);
            this.Schenkung_an = DatabaseConverter.ToInt32(reader[(int)Felder.Schenkung_an]);
            this.Schenkung_anID = DatabaseConverter.ToString(reader[(int)Felder.Schenkung_anID]);
            this.monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
        }

        public void Save(DbCommand command) {
            if (ID == 0)
                Insert(command);
            
            command.CommandText = $@"
        UPDATE {TableName} SET
            Schenkung_bekommen = {this.Schenkung_bekommen},
            Schenkung_bekommenID = '{DatabaseConverter.EscapeString(this.Schenkung_bekommenID)}',
            Schenkung_an = {this.Schenkung_an},
            Schenkung_anID = '{DatabaseConverter.EscapeString(this.Schenkung_anID)}',
            monat = {this.monat}
        WHERE ID = {this.ID}";

            // Wenn kein Update mögiich ist, dann Insert
            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Schenkung_bekommen, Schenkung_bekommenID, Schenkung_an, Schenkung_anID, monat
        ) VALUES (
            {this.Schenkung_bekommen}, 
            '{DatabaseConverter.EscapeString(this.Schenkung_bekommenID)}', 
            {this.Schenkung_an}, 
            '{DatabaseConverter.EscapeString(this.Schenkung_anID)}', 
            {this.monat}
           
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {TableName}
        WHERE 
            Schenkung_bekommen = {this.Schenkung_bekommen} AND
            Schenkung_bekommenID = '{DatabaseConverter.EscapeString(this.Schenkung_bekommenID)}' AND
            Schenkung_an = {this.Schenkung_an} AND
            Schenkung_anID = '{DatabaseConverter.EscapeString(this.Schenkung_anID)}' AND
            monat = {this.monat}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

        public bool Select() {
            return false;
        }

        public bool Edit() {
            return false;
        }
    }
}
