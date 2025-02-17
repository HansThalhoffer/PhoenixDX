using System;
using System.ComponentModel.Design;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class RuestungBauwerke :  KleinfeldPosition, IDatabaseTable, IEigenschaftler, IEquatable<RuestungBauwerke>
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestung_bauwerke";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName","Database","Bezeichner","ID","Key"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int ZugMonat { get; set; }
        public string Art { get; set; } = string.Empty ;
        public int BP_rep { get; set; }
        public int BP_neu { get; set; }
        public int Kosten { get; set; }
        public int ID { get; set; }

        public enum Felder
        {
            GF, KF, BP_rep, BP_neu, Art, Kosten, id,
        }

        public void Load(DbDataReader reader)
        {
            this.gf = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            this.kf = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            this.BP_rep = DatabaseConverter.ToInt32(reader[(int)Felder.BP_rep]);
            this.BP_neu = DatabaseConverter.ToInt32(reader[(int)Felder.BP_neu]);
            this.Art = DatabaseConverter.ToString(reader[(int)Felder.Art]);
            this.Kosten = DatabaseConverter.ToInt32(reader[(int)Felder.Kosten]);
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
                gf = {this.gf} AND
                kf = {this.kf} AND
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
            gf, kf, BP_rep, BP_neu, Art, Kosten
        ) VALUES (
            {this.gf},
            {this.kf},
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

            return gf == other.gf &&
                   kf == other.kf &&
                   BP_rep == other.BP_rep &&
                   BP_neu == other.BP_neu &&
                   Kosten == other.Kosten &&
                   Art == other.Art; // String comparison (null-safe)
        }

        public override bool Equals(object? obj) {
            return Equals(obj as RuestungBauwerke);
        }

        public override int GetHashCode() {
            return HashCode.Combine(gf, kf, BP_rep, BP_neu, Art, Kosten);
        }
    }
}
