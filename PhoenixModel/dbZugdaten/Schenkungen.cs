using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbZugdaten
{
    public class Schenkungen : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Schenkungen";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();
        private static readonly string[] PropertiestoIgnore = ["ID","Bezeichner"];
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
