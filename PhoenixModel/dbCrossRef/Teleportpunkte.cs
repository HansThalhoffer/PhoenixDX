using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class Teleportpunkte : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Teleportpunkte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Feld ?? "unbekannter Teleportpunkt";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string? Feld { get; set; }
        public string? Art { get; set; }

        public enum Felder
        {
            Feld, Art,
        }

        public void Load(DbDataReader reader)
        {
            this.Feld = DatabaseConverter.ToString(reader[(int)Felder.Feld]);
            this.Art = DatabaseConverter.ToString(reader[(int)Felder.Art]);
        }
    }
}
