using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class DBTablesExtern : IDatabaseTable
    {
        public const string TableName = "DBTablesextern";
        string IDatabaseTable.TableName => TableName;

        public int? id { get; set; }
        public string? tablename { get; set; }
        public string? usedbflag { get; set; }
    }
}
