using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.ExternalTables.GeländeTabelle;

namespace PhoenixModel.ExternalTables
{
    internal class EinwohnerUndEinnahmenTabelle
    {
        public struct Werte
        {
            public int Einwohner = 0;
            public int Einnahmen = 1;

            public Werte(int einwohner, int einnahmen)
            {
                this.Einwohner = einwohner;
                this.Einnahmen = einnahmen;
            }
        }


        public static Dictionary<string, Werte> EinwohnerUndEinnahmen = new Dictionary<string, Werte>
        {
            // name,              max einwohner, einnahmen pro event
            // aus den Terains
            { "Default", new Werte(0, 0) },
            { "Wasser", new Werte(0, 0) },
            { "Hochland", new Werte(40000, 800) },
            { "Wald", new Werte(40000, 800) },
            { "Wüste", new Werte(15000, 300) },
            { "Sumpf", new Werte(15000, 300) },
            { "Bergland", new Werte(25000, 500) },
            { "Gebirge", new Werte(10000, 200) },
            { "Tiefsee", new Werte(0, 0) },
            { "Tiefland", new Werte(50000, 1000) },
            { "Auftauchpunkt", new Werte(0, 0) },
            { "Tiefseeeinbahnpunkt (Tiefsee)", new Werte(0, 0) },
            { "Auftauchpunkt (unbekannt)", new Werte(0, 0) },
            // aus den Bauwerken
            { "Baustelle", new Werte ( 0, 0 ) },
            { "Dorf-I", new Werte ( 0, 0 ) },
            { "Dorf-II", new Werte ( 0, 0 ) },
            { "Dorf-III", new Werte ( 0, 0 ) },
            { "Burg", new Werte ( 10000, 1000 ) },
            { "Burg-I", new Werte ( 10000, 1000 ) },
            { "Burg-II", new Werte ( 10000, 1000 ) },
            { "Burg-III", new Werte ( 10000, 1000 ) },
            { "Stadt", new Werte ( 30000, 2000 ) },
            { "Stadt-I", new Werte ( 30000, 2000 ) },
            { "Stadt-II", new Werte ( 30000, 2000 ) },
            { "Stadt-III", new Werte ( 30000, 2000 ) },
            { "Festung", new Werte ( 70000, 3000 ) },
            { "Festung-I", new Werte ( 70000, 3000 ) },
            { "Festung-II", new Werte ( 70000, 3000 ) },
            { "Festung-III", new Werte ( 70000, 3000 ) },
            { "Hauptstadt", new Werte ( 100000, 5000 ) },
            { "Hauptstadt-I", new Werte ( 100000, 5000 ) },
            { "Hauptstadt-II", new Werte ( 100000, 5000 ) },
            { "Hauptstadt-III", new Werte ( 100000, 5000 ) },
            { "Festungshauptstadt", new Werte ( 100000, 6000 ) },
            { "Audvacar", new Werte ( 0, 0 ) },
            // sonstige EinnahmenView
            {"Kampfeinnahmen", new Werte( 0, 1) },
            {"Pluenderungen", new Werte( 0, 1) },
            {"eroberte_burgen", new Werte( 0, 1) },
            {"eroberte_staedte", new Werte( 0, 1) },
            {"eroberte_festungen", new Werte( 0, 1) },
            {"eroberte_hauptstadt", new Werte( 0, 1) },
            {"eroberte_festungshauptstadt", new Werte( 0, 1) },
        };

    }
}
