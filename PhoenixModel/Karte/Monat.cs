using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Karte
{
    internal class Monat
: IDatabaseTable
    {
        public const string TableName = "Monat";
        string IDatabaseTable.TableName => TableName;
        
        public string? zug { get; set; }
    }
}
