using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class RuestungRuestorte : KleinfeldPosition,  IDatabaseTable, IEigenschaftler, IEquatable<RuestungRuestorte> {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestung_ruestorte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Database", "Bezeichner", "ID", "Key"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int ZugMonat { get; set; }
        public int BP_rep { get; set; }
        public int BP_up { get; set; }
        public int ID { get; set; }

        public enum Felder
        {
            GF, KF, BP_rep, BP_up, id,
        }

        public void Load(DbDataReader reader)
        {
            this.gf = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            this.kf = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            this.BP_rep = DatabaseConverter.ToInt32(reader[(int)Felder.BP_rep]);
            this.BP_up = DatabaseConverter.ToInt32(reader[(int)Felder.BP_up]);
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            this.ZugMonat = ProgramView.SelectedMonth;
        }
        public void Save(DbCommand command)
        {
            command.CommandText = $@"
        UPDATE {TableName} SET
            gf = {this.gf},
            kf = {this.kf},
            BP_rep = {this.BP_rep},
            BP_up = {this.BP_up}
        WHERE ID = {this.ID}";

            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Insert(DbCommand command)
        {
            command.CommandText = $@"
        INSERT INTO {TableName} (gf, kf, BP_rep, BP_up)
        VALUES ({this.gf}, {this.kf}, {this.BP_rep}, {this.BP_up})";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {TableName}
        WHERE 
            gf = {this.gf} AND
            kf = {this.kf} AND
            BP_rep = {this.BP_rep} AND
            BP_up = {this.BP_up}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

        public bool Equals(RuestungRuestorte? other) {
            if (other == null) return false;

            return gf == other.gf &&
                   kf == other.kf &&
                   BP_rep == other.BP_rep &&
                   BP_up == other.BP_up;
        }

        public override bool Equals(object? obj) {
            return Equals(obj as RuestungRuestorte);
        }

        public override int GetHashCode() {
            return HashCode.Combine(gf, kf, BP_rep, BP_up);
        }
    }
}
