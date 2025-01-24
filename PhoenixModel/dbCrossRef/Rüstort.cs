using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbCrossRef {
    // referenzliste
    public class Rüstort : BauwerkBasis, IDatabaseTable, IEigenschaftler {
        public static string DatabaseName { get; set; } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestort_crossref";
        string IDatabaseTable.TableName => TableName;
        public static Dictionary<int, Rüstort> NachBaupunkten = [];

        public string? Ruestort { get; set; }
        public int? KapazitätTruppen { get; set; }
        public int? KapazitätHF { get; set; }
        public int? KapazitätZ { get; set; }
        public bool? canSieged { get; set; }

        string IDatabaseTable.Bezeichner => throw new NotImplementedException();

        public enum Felder {
            nummer, ruestort, Baupunkte, Kapazitaet_truppen, Kapazitaet_HF, Kapazitaet_Z, canSieged
        }

        public void Load(DbDataReader reader) {
            Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            Bauwerk = DatabaseConverter.ToString(reader[(int)Felder.ruestort]);
            Ruestort = DatabaseConverter.ToString(reader[(int)Felder.ruestort]);
            KapazitätTruppen = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_truppen]);
            KapazitätHF = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_HF]);
            KapazitätZ = DatabaseConverter.ToInt32(reader[(int)Felder.Kapazitaet_Z]);
            canSieged = DatabaseConverter.ToBool(reader[(int)Felder.canSieged]);
            if (Baupunkte > 0)
                NachBaupunkten.Add(Baupunkte.Value, this);
        }

        void IDatabaseTable.Load(DbDataReader reader) {
            throw new NotImplementedException();
        }

        void IDatabaseTable.Save(DbCommand reader) {
            throw new NotImplementedException();
        }

        void IDatabaseTable.Insert(DbCommand reader) {
            throw new NotImplementedException();
        }
    }
}
