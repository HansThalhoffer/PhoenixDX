using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbZugdatan
{
    internal class BilanzEinnahmen : IDatabaseTable
    {
        public const string TableName = "Bilanz_einnahmen";
        string IDatabaseTable.TableName => TableName;

        public int? monat { get; set; }
        public int? Tiefland { get; set; }
        public int? Tieflandwald { get; set; }
        public int? Tieflandwüste { get; set; }
        public int? Tieflandsumpf { get; set; }
        public int? Hochland { get; set; }
        public int? Bergland { get; set; }
        public int? Gebirge { get; set; }
        public int? Burgen { get; set; }
        public int? Städte { get; set; }
        public int? Festungen { get; set; }
        public int? Hauptstadt { get; set; }
        public int? Festungshauptstadt { get; set; }
        public int? Kampfeinnahmen { get; set; }
        public int? Pluenderungen { get; set; }
        public int? eroberte_burgen { get; set; }
        public int? eroberte_staedte { get; set; }
        public int? eroberte_festungen { get; set; }
        public int? eroberte_hauptstadt { get; set; }
        public int? eroberte_festungshauptstadt { get; set; }
        public int? Reichsschatzalt { get; set; }
        public int? gew_einnahmen { get; set; }
        public int? bes_einnahmen { get; set; }
        public int? gew_ausgaben { get; set; }
        public int? bes_ausgaben { get; set; }
        public int? Geldverleih { get; set; }
        public int? Geldschenkung { get; set; }
        public int? Sonstiges { get; set; }
        public int? Reichsschatzneu { get; set; }
    }



}
