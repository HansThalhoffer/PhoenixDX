using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Schenkungen :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "Schenkungen";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        private static readonly string[] PropertiestoIgnore = ["ID","Bezeichner", "DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Schenkung_bekommen { get; set; }
        public string? Schenkung_bekommenID { get; set; }
        public int Schenkung_an { get; set; }
        public string? Schenkung_anID { get; set; }
        public int monat { get; set; }
        public int ID { get; set; }

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
            command.CommandText = $@"
        UPDATE {TableName} SET
            Schenkung_bekommen = {this.Schenkung_bekommen},
            Schenkung_bekommenID = '{DatabaseConverter.EscapeString(this.Schenkung_bekommenID)}',
            Schenkung_an = {this.Schenkung_an},
            Schenkung_anID = '{DatabaseConverter.EscapeString(this.Schenkung_anID)}',
            monat = {this.monat}
        WHERE ID = {this.ID}";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Schenkung_bekommen, Schenkung_bekommenID, Schenkung_an, Schenkung_anID, monat, ID
        ) VALUES (
            {this.Schenkung_bekommen}, 
            '{DatabaseConverter.EscapeString(this.Schenkung_bekommenID)}', 
            {this.Schenkung_an}, 
            '{DatabaseConverter.EscapeString(this.Schenkung_anID)}', 
            {this.monat}, 
            {this.ID}
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }

    }
}
