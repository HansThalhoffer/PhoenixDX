using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbZugdaten
{
    public class Personal :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "personal";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID?? "unbekanntes Personal";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public string? Vorname { get; set; }
        public string? Nachname { get; set; }
        public string? Spielername { get; set; }
        public string? Pos { get; set; }
        public string? Anschrift { get; set; }
        public string? Tel { get; set; }
        public string? Email { get; set; }
        public string? ID { get; set; }

        public enum Felder
        {
            Vorname, Nachname, Spielername, Pos, Anschrift, Tel, Email, id,
        }

        public void Load(DbDataReader reader)
        {
            this.Vorname = DatabaseConverter.ToString(reader[(int)Felder.Vorname]);
            this.Nachname = DatabaseConverter.ToString(reader[(int)Felder.Nachname]);
            this.Spielername = DatabaseConverter.ToString(reader[(int)Felder.Spielername]);
            this.Pos = DatabaseConverter.ToString(reader[(int)Felder.Pos]);
            this.Anschrift = DatabaseConverter.ToString(reader[(int)Felder.Anschrift]);
            this.Tel = DatabaseConverter.ToString(reader[(int)Felder.Tel]);
            this.Email = DatabaseConverter.ToString(reader[(int)Felder.Email]);
            this.ID = DatabaseConverter.ToString(reader[(int)Felder.id]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
