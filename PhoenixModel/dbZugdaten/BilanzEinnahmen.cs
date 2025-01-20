using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbZugdaten {
    public class BilanzEinnahmen :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Bilanz_einnahmen";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Database"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


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
            monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
            Tiefland = DatabaseConverter.ToInt32(reader[(int)Felder.Tiefland]);
            Tieflandwald = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandwald]);
            Tieflandwüste = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandwüste]);
            Tieflandsumpf = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandsumpf]);
            Hochland = DatabaseConverter.ToInt32(reader[(int)Felder.Hochland]);
            Bergland = DatabaseConverter.ToInt32(reader[(int)Felder.Bergland]);
            Gebirge = DatabaseConverter.ToInt32(reader[(int)Felder.Gebirge]);
            Burgen = DatabaseConverter.ToInt32(reader[(int)Felder.Burgen]);
            Städte = DatabaseConverter.ToInt32(reader[(int)Felder.Städte]);
            Festungen = DatabaseConverter.ToInt32(reader[(int)Felder.Festungen]);
            Hauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.Hauptstadt]);
            Festungshauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.Festungshauptstadt]);
            Kampfeinnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.Kampfeinnahmen]);
            Pluenderungen = DatabaseConverter.ToInt32(reader[(int)Felder.Pluenderungen]);
            eroberte_burgen = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_burgen]);
            eroberte_staedte = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_staedte]);
            eroberte_festungen = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_festungen]);
            eroberte_hauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_hauptstadt]);
            eroberte_festungshauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_festungshauptstadt]);
            Reichsschatzalt = DatabaseConverter.ToInt32(reader[(int)Felder.Reichsschatzalt]);
            gew_einnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.gew_einnahmen]);
            bes_einnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.bes_einnahmen]);
            gew_ausgaben = DatabaseConverter.ToInt32(reader[(int)Felder.gew_ausgaben]);
            bes_ausgaben = DatabaseConverter.ToInt32(reader[(int)Felder.bes_ausgaben]);
            Geldverleih = DatabaseConverter.ToInt32(reader[(int)Felder.Geldverleih]);
            Geldschenkung = DatabaseConverter.ToInt32(reader[(int)Felder.Geldschenkung]);
            Sonstiges = DatabaseConverter.ToInt32(reader[(int)Felder.Sonstiges]);
            Reichsschatzneu = DatabaseConverter.ToInt32(reader[(int)Felder.Reichsschatzneu]);
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Tiefland = {this.Tiefland},
            Tieflandwald = {this.Tieflandwald},
            Tieflandwüste = {this.Tieflandwüste},
            Tieflandsumpf = {this.Tieflandsumpf},
            Hochland = {this.Hochland},
            Bergland = {this.Bergland},
            Gebirge = {this.Gebirge},
            Burgen = {this.Burgen},
            Städte = {this.Städte},
            Festungen = {this.Festungen},
            Hauptstadt = {this.Hauptstadt},
            Festungshauptstadt = {this.Festungshauptstadt},
            Kampfeinnahmen = {this.Kampfeinnahmen},
            Pluenderungen = {this.Pluenderungen},
            eroberte_burgen = {this.eroberte_burgen},
            eroberte_staedte = {this.eroberte_staedte},
            eroberte_festungen = {this.eroberte_festungen},
            eroberte_hauptstadt = {this.eroberte_hauptstadt},
            eroberte_festungshauptstadt = {this.eroberte_festungshauptstadt},
            Reichsschatzalt = {this.Reichsschatzalt},
            gew_einnahmen = {this.gew_einnahmen},
            bes_einnahmen = {this.bes_einnahmen},
            gew_ausgaben = {this.gew_ausgaben},
            bes_ausgaben = {this.bes_ausgaben},
            Geldverleih = {this.Geldverleih},
            Geldschenkung = {this.Geldschenkung},
            Sonstiges = {this.Sonstiges},
            Reichsschatzneu = {this.Reichsschatzneu}
        WHERE monat = {this.monat}";

            // Execute the command
            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            monat, Tiefland, Tieflandwald, Tieflandwüste, Tieflandsumpf, Hochland, Bergland, Gebirge, 
            Burgen, Städte, Festungen, Hauptstadt, Festungshauptstadt, Kampfeinnahmen, Pluenderungen, 
            eroberte_burgen, eroberte_staedte, eroberte_festungen, eroberte_hauptstadt, eroberte_festungshauptstadt, 
            Reichsschatzalt, gew_einnahmen, bes_einnahmen, gew_ausgaben, bes_ausgaben, Geldverleih, Geldschenkung, 
            Sonstiges, Reichsschatzneu
        ) VALUES (
            {this.monat}, {this.Tiefland}, {this.Tieflandwald}, {this.Tieflandwüste}, {this.Tieflandsumpf}, 
            {this.Hochland}, {this.Bergland}, {this.Gebirge}, {this.Burgen}, {this.Städte}, {this.Festungen}, 
            {this.Hauptstadt}, {this.Festungshauptstadt}, {this.Kampfeinnahmen}, {this.Pluenderungen}, 
            {this.eroberte_burgen}, {this.eroberte_staedte}, {this.eroberte_festungen}, {this.eroberte_hauptstadt}, 
            {this.eroberte_festungshauptstadt}, {this.Reichsschatzalt}, {this.gew_einnahmen}, {this.bes_einnahmen}, 
            {this.gew_ausgaben}, {this.bes_ausgaben}, {this.Geldverleih}, {this.Geldschenkung}, {this.Sonstiges}, 
            {this.Reichsschatzneu}
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }

    }



}
