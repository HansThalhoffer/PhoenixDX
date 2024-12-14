using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
{
    public class Gebäude : GemarkPosition, IPropertyHolder, IDatabaseTable
    {
        // IDatabaseTable
        public const string TableName = "bauwerksliste";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(gf, kf); }
        // IPropertyHolder
        private static readonly string[] PropertiestoIgnore = [];
        public Dictionary<string, string> Properties { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

      
        // Felder der Tabellen
        public string? Reich { get; set; }
        public string? Bauwerknamen { get; set; }

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
    }
}
