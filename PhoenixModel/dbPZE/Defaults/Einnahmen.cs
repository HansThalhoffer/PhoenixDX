using PhoenixModel.dbErkenfara.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE.Defaults
{
    public class Einnahmen
    {
        public static List<Einnahmen> Einnahmens = new List<Einnahmen>
        {
            new Einnahmen("Wasser", "w", 0, 0),
            new Einnahmen("Tiefland", "t", 50000, 1000),
            new Einnahmen("Tieflandwald", "twa", 40000, 800),
            new Einnahmen("Tieflandwüste", "twü", 15000, 300),
            new Einnahmen("Tieflandsumpf", "tsm", 15000, 300),
            new Einnahmen("Hochland", "h", 40000, 800),
            new Einnahmen("Bergland", "b", 25000, 500),
            new Einnahmen("Gebirge", "g", 10000, 200),
            new Einnahmen("Burg", "BUR", 10000, 1000),
            new Einnahmen("Stadt", "STD", 30000, 2000),
            new Einnahmen("Festung", "FST", 70000, 3000),
            new Einnahmen("Hauptstadt", "HPST", 100000, 5000),
            new Einnahmen("Festungshauptstadt", "FHPST", 100000, 6000)
        };


        public string Name { get; set; }
        public string ShortCode { get; set; }
        public int Inhabitants { get; set; }
        public int Income { get; set; }

        public Einnahmen(string name, string shortCode, int inhabitants, int income)
        {
            Name = name;
            ShortCode = shortCode;
            Inhabitants = inhabitants;
            Income = income;
        }

        public override string ToString()
        {
            return $"Terrain: {Name}, ShortCode: {ShortCode}, Inhabitants: {Inhabitants}, Income: {Income}";
        }
    }
}
