using System;
using System.ComponentModel.Design;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class RuestungBauwerke :  IDatabaseTable, IEigenschaftler, IEquatable<RuestungBauwerke>
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestung_bauwerke";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int GF { get; set; }
        public int KF { get; set; }
        public int BP_rep { get; set; }
        public int BP_neu { get; set; }
        public string? Art { get; set; }
        public int Kosten { get; set; }
        public int ID { get; set; }

        public enum Felder
        {
            GF, KF, BP_rep, BP_neu, Art, Kosten, id,
        }

        public void Load(DbDataReader reader)
        {
            this.GF = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            this.KF = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            this.BP_rep = DatabaseConverter.ToInt32(reader[(int)Felder.BP_rep]);
            this.BP_neu = DatabaseConverter.ToInt32(reader[(int)Felder.BP_neu]);
            this.Art = DatabaseConverter.ToString(reader[(int)Felder.Art]);
            this.Kosten = DatabaseConverter.ToInt32(reader[(int)Felder.Kosten]);
            this.ID = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
        }

        public void Save(DbCommand command)
        {
            command.CommandText = $@"
        UPDATE {TableName} SET
            GF = {this.GF},
            KF = {this.KF},
            BP_rep = {this.BP_rep},
            BP_neu = {this.BP_neu},
            Art = '{DatabaseConverter.EscapeString(this.Art)}',
            Kosten = {this.Kosten}
        WHERE ID = {this.ID}";

            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Delete(DbCommand command) {
            if (ID == 0) {
            command.CommandText = $@"
            DELETE FROM {TableName}
            WHERE 
                GF = {this.GF} AND
                KF = {this.KF} AND
                BP_rep = {this.BP_rep} AND
                BP_neu = {this.BP_neu} AND
                Art = '{DatabaseConverter.EscapeString(this.Art)}' AND
                Kosten = {this.Kosten}";
                }
            else {
                command.CommandText = $@"DELETE FROM {TableName} WHERE ID = {this.ID}";
            }
            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Insert(DbCommand command)
        {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            GF, KF, BP_rep, BP_neu, Art, Kosten
        ) VALUES (
            {this.GF},
            {this.KF},
            {this.BP_rep},
            {this.BP_neu},
            '{DatabaseConverter.EscapeString(this.Art)}',
            {this.Kosten}
        )";

            // Execute the command
            command.ExecuteNonQuery();
            // hole die ID
            if (command is DbCommandFacade facade)
            this.ID = facade.GetLastInsertedId();
        }


        public bool Equals(RuestungBauwerke? other) {
            if (other == null) return false;

            return GF == other.GF &&
                   KF == other.KF &&
                   BP_rep == other.BP_rep &&
                   BP_neu == other.BP_neu &&
                   Kosten == other.Kosten &&
                   Art == other.Art; // String comparison (null-safe)
        }

        public override bool Equals(object? obj) {
            return Equals(obj as RuestungBauwerke);
        }

        public override int GetHashCode() {
            return HashCode.Combine(GF, KF, BP_rep, BP_neu, Art, Kosten);
        }
    }
}
