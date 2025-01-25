using System;
using System.Data.Common;
using System.Diagnostics;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbCrossRef {
    [DebuggerDisplay("{Bezeichner}")]
    public class Kosten :  IDatabaseTable, IEigenschaftler, IEquatable<Kosten> {
    
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Kosten";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Unittyp;
        public override string ToString()=> $"{Unittyp} GS {GS} BP {BauPunkte} RP {RP}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public string Unittyp { get; set; } = string.Empty;
        public int GS { get; set; }
        public int BauPunkte { get; set; }
        public int Raumpunkte => RP;
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

        public void Delete(DbCommand reader) => throw new NotImplementedException();

        public bool Equals(Kosten? other) {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            if (other == null) return false;
            if( Unittyp != other.Unittyp ) return false;
            if( GS != other.GS ) return false;
            if (BauPunkte != other.BauPunkte ) return false;
            if (RP != other.RP ) return false;
            return true;
        }
    }
}
