using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Karte
{
    public class Reich
    {
        public class DBhandle : IDatabaseTable
        {
            public const string TableName = "DBhandle";
            string IDatabaseTable.TableName => TableName;
        
            public int? Nummer { get; set; }
            public string? Reich { get; set; }
            public string? DBname { get; set; }
            public string? DBpass { get; set; }
        }
    }
}
