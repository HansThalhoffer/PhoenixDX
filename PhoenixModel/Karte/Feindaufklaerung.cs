using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Karte
{
    internal class Feindaufklaerung: IDatabaseTable
    {
        public const string TableName = "Feindaufklaerung";
        string IDatabaseTable.TableName => TableName;

        public int? id { get; set; }
        public int? Nummer { get; set; }
        public string? Reich { get; set; }
        public string? Art { get; set; }
        public int? GF { get; set; }
        public int? KF { get; set; }
        public string? Notiz { get; set; }
    }
}
