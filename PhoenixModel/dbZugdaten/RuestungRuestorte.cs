using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class RuestungRuestorte :  IDatabaseTable, IEigenschaftler, IEquatable<RuestungRuestorte> {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestung_ruestorte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int GF { get; set; }
        public int KF { get; set; }
        public int BP_rep { get; set; }
        public int BP_up { get; set; }
        public int ID { get; set; }

        public enum Felder
        {
            GF, KF, BP_rep, BP_up, id,
        }

        public void Load(DbDataReader reader)
        {
            this.GF = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            this.KF = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            this.BP_rep = DatabaseConverter.ToInt32(reader[(int)Felder.BP_rep]);
            this.BP_up = DatabaseConverter.ToInt32(reader[(int)Felder.BP_up]);
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
        }
        public void Save(DbCommand command)
        {
            command.CommandText = $@"
        UPDATE {TableName} SET
            GF = {this.GF},
            KF = {this.KF},
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
        INSERT INTO {TableName} (GF, KF, BP_rep, BP_up)
        VALUES ({this.GF}, {this.KF}, {this.BP_rep}, {this.BP_up})";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {TableName}
        WHERE 
            GF = {this.GF} AND
            KF = {this.KF} AND
            BP_rep = {this.BP_rep} AND
            BP_up = {this.BP_up}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

        public bool Equals(RuestungRuestorte? other) {
            if (other == null) return false;

            return GF == other.GF &&
                   KF == other.KF &&
                   BP_rep == other.BP_rep &&
                   BP_up == other.BP_up;
        }

        public override bool Equals(object? obj) {
            return Equals(obj as RuestungRuestorte);
        }

        public override int GetHashCode() {
            return HashCode.Combine(GF, KF, BP_rep, BP_up);
        }
    }
}
