using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class Kosten : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Kosten";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Unittyp ?? "unbekannte Kosten";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Unittyp { get; set; }
        public int GS { get; set; }
        public int BauPunkte { get; set; }
        public int RP { get; set; }

        public enum Felder
        {
            Unittyp, GS, BauPunkte, RP,
        }

        public void Load(DbDataReader reader)
        {
            this.Unittyp = DatabaseConverter.ToString(reader[(int)Felder.Unittyp]);
            this.GS = DatabaseConverter.ToInt32(reader[(int)Felder.GS]);
            this.BauPunkte = DatabaseConverter.ToInt32(reader[(int)Felder.BauPunkte]);
            this.RP = DatabaseConverter.ToInt32(reader[(int)Felder.RP]);
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
