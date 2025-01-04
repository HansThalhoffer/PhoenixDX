using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class Crossref_zauberer_teleport :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "crossref_zauberer_teleport";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => $"{GP} {ZX} {Teleport}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int GP { get; set; }
        public string? ZX { get; set; }
        public int Teleport { get; set; }
        public int Regeneration_GP { get; set; }

        public enum Felder
        {
            GP, ZX, Teleport, Regeneration_GP,
        }

        public void Load(DbDataReader reader)
        {
            this.GP = DatabaseConverter.ToInt32(reader[(int)Felder.GP]);
            this.ZX = DatabaseConverter.ToString(reader[(int)Felder.ZX]);
            this.Teleport = DatabaseConverter.ToInt32(reader[(int)Felder.Teleport]);
            this.Regeneration_GP = DatabaseConverter.ToInt32(reader[(int)Felder.Regeneration_GP]);
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
