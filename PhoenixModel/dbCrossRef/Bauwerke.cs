using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbCrossRef {
    public class BauwerkBasis
    {
        static Dictionary<string, BauwerkBasis> Bauwerke = [];
        public static BauwerkBasis? GetBauwerk(string name)
        {
            if (Bauwerke.ContainsKey(name)) 
                return Bauwerke[name];
            return null;
        }

        public int? Nummer { get; set; }
        public int? Baupunkte { get; set; }
        public string Bauwerk { get; set; } = string.Empty; 

        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public virtual List<Eigenschaft> Eigenschaften
        {
            get
            {
                return PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
        
        public string Bezeichner { get => Bauwerk ?? "Gebäude"; }

        public virtual void Save(DbCommand reader) {
            throw new NotImplementedException();
        }

        public virtual void Insert(DbCommand reader) {
            throw new NotImplementedException();
        }

        public virtual void Delete(DbCommand reader) {
            throw new NotImplementedException();
        }
    }

    // kosten von Wall etc
    /*public class Bauwerk : BauwerkBasis, IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Bauwerke_crossref";
        string IDatabaseTable.TableName => TableName;

        public enum Felder
        {
            Nummer, Baupunkte, Bauwerk
        }
        public void Load(DbDataReader reader)
        {
            Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            Bauwerk = DatabaseConverter.ToString(reader[(int)Felder.Bauwerk]);
        }

        
    }*/

    
}
