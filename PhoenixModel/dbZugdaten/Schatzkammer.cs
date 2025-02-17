using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Schatzkammer : IDatabaseTable, IEigenschaftler {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Schatzkammer";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"Monat {monat} {schenkung_bekommen} {schenkung_getaetigt}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Reichschatz { get; set; }
        public int Einahmen_land { get; set; }
        public int schenkung_bekommen { get; set; }
        public int GS_bei_truppen { get; set; }
        public int schenkung_getaetigt { get; set; }
        public int Verruestet { get; set; }
        public int monat { get; set; }

        public enum Felder {
            monat, Reichschatz, Einahmen_land, schenkung_bekommen, GS_bei_truppen, schenkung_getaetigt, Verruestet
        }

        public void Load(DbDataReader reader) {
            this.Reichschatz = DatabaseConverter.ToInt32(reader[(int)Felder.Reichschatz]);
            this.Einahmen_land = DatabaseConverter.ToInt32(reader[(int)Felder.Einahmen_land]);
            this.schenkung_bekommen = DatabaseConverter.ToInt32(reader[(int)Felder.schenkung_bekommen]);
            this.GS_bei_truppen = DatabaseConverter.ToInt32(reader[(int)Felder.GS_bei_truppen]);
            this.schenkung_getaetigt = DatabaseConverter.ToInt32(reader[(int)Felder.schenkung_getaetigt]);
            this.Verruestet = DatabaseConverter.ToInt32(reader[(int)Felder.Verruestet]);
            this.monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Reichschatz = {this.Reichschatz},
            Einahmen_land = {this.Einahmen_land},
            schenkung_bekommen = {this.schenkung_bekommen},
            GS_bei_truppen = {this.GS_bei_truppen},
            schenkung_getaetigt = {this.schenkung_getaetigt},
            Verruestet = {this.Verruestet}
        WHERE  monat = {this.monat}";

            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Reichschatz, Einahmen_land, schenkung_bekommen, GS_bei_truppen, 
            schenkung_getaetigt, Verruestet, monat
        ) VALUES (
            {this.Reichschatz}, {this.Einahmen_land}, {this.schenkung_bekommen}, 
            {this.GS_bei_truppen}, {this.schenkung_getaetigt}, {this.Verruestet}, {this.monat}
        )";

            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {TableName}
        WHERE 
            Reichschatz = {this.Reichschatz} AND
            Einahmen_land = {this.Einahmen_land} AND
            schenkung_bekommen = {this.schenkung_bekommen} AND
            GS_bei_truppen = {this.GS_bei_truppen} AND
            schenkung_getaetigt = {this.schenkung_getaetigt} AND
            Verruestet = {this.Verruestet} AND
            monat = {this.monat}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

    }
}
