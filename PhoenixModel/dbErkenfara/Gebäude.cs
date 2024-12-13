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
        public Dictionary<string, string> Properties { get => PropertiesProcessor.CreateProperties(this, PropertiestoIgnore); }

        // Felder der Tabellen
        public string? Reich { get; set; }
        public string? Bauwerknamem { get; set; }

        public enum Felder
        {
            gf, kf, Reich, Bauwerknamem
        }
        public void Load(DbDataReader reader)
        {
            gf = reader.GetInt32((int)Felder.gf);
            kf = reader.GetInt32((int)Felder.kf);
            Reich = reader.GetString((int)Felder.Reich);
            Bauwerknamem = reader.GetString((int)Felder.Bauwerknamem);
        }
    }
}
