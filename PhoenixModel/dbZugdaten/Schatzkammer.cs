using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbZugdaten
{
    public class Schatzkammer : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Schatzkammer";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"Monat {monat} {schenkung_bekommen} {schenkung_getaetigt}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Reichschatz { get; set; }
        public int Einahmen_land { get; set; }
        public int schenkung_bekommen { get; set; }
        public int GS_bei_truppen { get; set; }
        public int schenkung_getaetigt { get; set; }
        public int Verruestet { get; set; }
        public int monat { get; set; }

        public enum Felder
        {
            monat, Reichschatz, Einahmen_land, schenkung_bekommen, GS_bei_truppen, schenkung_getaetigt, Verruestet
        }

        public void Load(DbDataReader reader)
        {
            this.Reichschatz = DatabaseConverter.ToInt32(reader[(int)Felder.Reichschatz]);
            this.Einahmen_land = DatabaseConverter.ToInt32(reader[(int)Felder.Einahmen_land]);
            this.schenkung_bekommen = DatabaseConverter.ToInt32(reader[(int)Felder.schenkung_bekommen]);
            this.GS_bei_truppen = DatabaseConverter.ToInt32(reader[(int)Felder.GS_bei_truppen]);
            this.schenkung_getaetigt = DatabaseConverter.ToInt32(reader[(int)Felder.schenkung_getaetigt]);
            this.Verruestet = DatabaseConverter.ToInt32(reader[(int)Felder.Verruestet]);
            this.monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
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
