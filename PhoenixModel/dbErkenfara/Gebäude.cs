using PhoenixModel.dbCrossRef;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixModel.Program;

namespace PhoenixModel.dbErkenfara
{
    public class Gebäude : GemarkPosition, IEigenschaftler, IDatabaseTable, ISelectable
    {
        // IDatabaseTable
        public const string TableName = "bauwerksliste";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(); }
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

      
        // Felder der Tabellen
        public string? Reich { get; set; }
        public string? Bauwerknamen { get; set; }
        public Rüstort? Rüstort { get; set; } = null;
        // falls kaputt oder noch nicht fertig aufgebaut
        public bool InBau { get; set; } = false;
        public bool Zerstört { get; set; } = false;

        public enum Felder
        {
            gf, kf, Reich, Bauwerknamen
        }
        public void Load(DbDataReader reader)
        {
            gf = DatabaseConverter.ToInt32(reader[(int)Felder.gf]);
            kf = DatabaseConverter.ToInt32(reader[(int)Felder.kf]);
            Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            Bauwerknamen = DatabaseConverter.ToString(reader[(int)Felder.Bauwerknamen]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public bool Select()
        {
            return ViewModel.BelongsToUser(this);
        }
        public bool Edit()
        {
            return Select();
        }
    }
}
