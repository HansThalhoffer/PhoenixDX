using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbZugdatan
{
    public class BilanzEinnahmen : IDatabaseTable
    {
        public const string TableName = "Bilanz_einnahmen";
        string IDatabaseTable.TableName => TableName;

        public int monat { get; set; } = 0;
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

        public string Bezeichner => monat.ToString();

        public enum Felder
        {
            monat,
            Tiefland,
            Tieflandwald,
            Tieflandwüste,
            Tieflandsumpf,
            Hochland,
            Bergland,
            Gebirge,
            Burgen,
            Städte,
            Festungen,
            Hauptstadt,
            Festungshauptstadt,
            Kampfeinnahmen,
            Pluenderungen,
            eroberte_burgen,
            eroberte_staedte,
            eroberte_festungen,
            eroberte_hauptstadt,
            eroberte_festungshauptstadt,
            Reichsschatzalt,
            gew_einnahmen,
            bes_einnahmen,
            gew_ausgaben,
            bes_ausgaben,
            Geldverleih,
            Geldschenkung,
            Sonstiges,
            Reichsschatzneu
        }

        public void Load(DbDataReader reader)
        {
            monat = reader.GetInt32((int)Felder.monat);
            Tiefland = reader.GetInt32((int)Felder.Tiefland);
            Tieflandwald = reader.GetInt32((int)Felder.Tieflandwald);
            Tieflandwüste = reader.GetInt32((int)Felder.Tieflandwüste);
            Tieflandsumpf = reader.GetInt32((int)Felder.Tieflandsumpf);
            Hochland = reader.GetInt32((int)Felder.Hochland);
            Bergland = reader.GetInt32((int)Felder.Bergland);
            Gebirge = reader.GetInt32((int)Felder.Gebirge);
            Burgen = reader.GetInt32((int)Felder.Burgen);
            Städte = reader.GetInt32((int)Felder.Städte);
            Festungen = reader.GetInt32((int)Felder.Festungen);
            Hauptstadt = reader.GetInt32((int)Felder.Hauptstadt);
            Festungshauptstadt = reader.GetInt32((int)Felder.Festungshauptstadt);
            Kampfeinnahmen = reader.GetInt32((int)Felder.Kampfeinnahmen);
            Pluenderungen = reader.GetInt32((int)Felder.Pluenderungen);
            eroberte_burgen = reader.GetInt32((int)Felder.eroberte_burgen);
            eroberte_staedte = reader.GetInt32((int)Felder.eroberte_staedte);
            eroberte_festungen = reader.GetInt32((int)Felder.eroberte_festungen);
            eroberte_hauptstadt = reader.GetInt32((int)Felder.eroberte_hauptstadt);
            eroberte_festungshauptstadt = reader.GetInt32((int)Felder.eroberte_festungshauptstadt);
            Reichsschatzalt = reader.GetInt32((int)Felder.Reichsschatzalt);
            gew_einnahmen = reader.GetInt32((int)Felder.gew_einnahmen);
            bes_einnahmen = reader.GetInt32((int)Felder.bes_einnahmen);
            gew_ausgaben = reader.GetInt32((int)Felder.gew_ausgaben);
            bes_ausgaben = reader.GetInt32((int)Felder.bes_ausgaben);
            Geldverleih = reader.GetInt32((int)Felder.Geldverleih);
            Geldschenkung = reader.GetInt32((int)Felder.Geldschenkung);
            Sonstiges = reader.GetInt32((int)Felder.Sonstiges);
            Reichsschatzneu = reader.GetInt32((int)Felder.Reichsschatzneu);
        }

    }



}
