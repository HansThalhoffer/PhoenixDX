using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class Monat : IDatabaseTable
    {
        public const string TableName = "Monat";
        string IDatabaseTable.TableName => TableName;

        public string? zug { get; set; }

        public string Bezeichner => zug ?? string.Empty;

        public enum Felder
        {
            zug
        }
        public void Load(DbDataReader reader)
        {
            zug = reader.GetString((int)Felder.zug);          
        }
    }
}
