using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class Infolog : IDatabaseTable
    {
        public const string TableName = "Infolog";
        string IDatabaseTable.TableName => TableName;

        public int? ID { get; set; }
        public string? InfoType { get; set; }
        public string? Infotext { get; set; }
        public int? ReichID { get; set; }
        public string? Monat { get; set; }
    }
}
