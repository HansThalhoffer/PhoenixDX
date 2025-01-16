using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class RuestungBauwerke :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
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
        }       
    }
}
