using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
{
    internal class EinfuegeFehler : IDatabaseTable
    {
        public const string TableName = "Einfuegefehler";
        string IDatabaseTable.TableName => TableName;

        public string? Reich { get; set; }
        public string? Referenzreich { get; set; }
        public int? Wegerecht { get; set; }
        public int? Wegerecht_von { get; set; }
        public string? DBname { get; set; }
        public int? Kuestenrecht { get; set; }
        public int? Kuestenrecht_von { get; set; }
        public int? Flottenkey { get; set; }
    }
}
