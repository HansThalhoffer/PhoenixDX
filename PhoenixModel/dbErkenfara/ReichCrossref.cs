using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbErkenfara
{
    public class ReichCrossref :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public virtual string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        
        public const string TableName = "Reich_crossref";
        string IDatabaseTable.TableName => ReichCrossref.TableName;
        public string Bezeichner => $"{Referenzreich}/{Reich}";
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DBname", "Nummer", "DatabaseName", "Flottenkey"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int Nummer { get; set; }
        /// <summary>
        ///  das eigene Reich bzw. das Reich, das dem anderen die Rechte gibt
        /// </summary>
        public string? Referenzreich { get; set; }
        /// <summary>
        /// Das Reich, das die Rechte empfängt
        /// </summary>
        public int Wegerecht { get; set; }
        public int Kuestenrecht { get; set; }
        public string? Reich { get; set; }
        public int Wegerecht_von { get; set; }
        public string? DBname { get; set; }
        public int Kuestenrecht_von { get; set; }
        public int Flottenkey { get; set; }

        public enum Felder
        {
            Nummer, Reich, Referenzreich, Wegerecht, Wegerecht_von, DBname, Kuestenrecht, Kuestenrecht_von, Flottenkey,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            this.Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            this.Referenzreich = DatabaseConverter.ToString(reader[(int)Felder.Referenzreich]);
            this.Wegerecht = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht]);
            this.Wegerecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Wegerecht_von]);
            this.DBname = DatabaseConverter.ToString(reader[(int)Felder.DBname]);
            this.Kuestenrecht = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht]);
            this.Kuestenrecht_von = DatabaseConverter.ToInt32(reader[(int)Felder.Kuestenrecht_von]);
            this.Flottenkey = DatabaseConverter.ToInt32(reader[(int)Felder.Flottenkey]);
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
