using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
{
    public class Gebäude : GemarkPosition, IPropertyHolder, IDatabaseTable
    {
        public const string TableName = "bauwerksliste";
        string IDatabaseTable.TableName => TableName;
        public string? Reich { get; set; }
        public string? Bauwerknamem { get; set; }

        public string Bezeichner { get => CreateBezeichner(gf, kf); }       
        private static readonly string[] PropertiestoIgnore = [];
        public Dictionary<string, string> Properties
        {
            get
            {
                return PropertiesProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
    }
}
