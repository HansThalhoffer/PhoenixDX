using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbPZE {
    public class PzeBilanzEinnahmen :  IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "Bilanz_einnahmen";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => monat.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        public int monat { get; set; }
        public int Tiefland { get; set; }
        public int Tieflandwald { get; set; }
        public int Tieflandwüste { get; set; }
        public int Tieflandsumpf { get; set; }
        public int Hochland { get; set; }
        public int Bergland { get; set; }
        public int Gebirge { get; set; }
        public int Burgen { get; set; }
        public int Städte { get; set; }
        public int Festungen { get; set; }
        public int Hauptstadt { get; set; }
        public int Festungshauptstadt { get; set; }
        public int Kampfeinnahmen { get; set; }
        public int Pluenderungen { get; set; }
        public int eroberte_burgen { get; set; }
        public int eroberte_staedte { get; set; }
        public int eroberte_festungen { get; set; }
        public int eroberte_hauptstadt { get; set; }
        public int eroberte_festungshauptstadt { get; set; }
        public int Reichsschatzalt { get; set; }
        public int gew_einnahmen { get; set; }
        public int bes_einnahmen { get; set; }
        public int gew_ausgaben { get; set; }
        public int bes_ausgaben { get; set; }
        public int Geldverleih { get; set; }
        public int Geldschenkung { get; set; }
        public int Sonstiges { get; set; }
        public int Reichsschatzneu { get; set; }

        public enum Felder
        {
            monat, Tiefland, Tieflandwald, Tieflandwüste, Tieflandsumpf, Hochland, Bergland, Gebirge, Burgen, Städte, Festungen, Hauptstadt, Festungshauptstadt, Kampfeinnahmen, Pluenderungen, eroberte_burgen, eroberte_staedte, eroberte_festungen, eroberte_hauptstadt,
            eroberte_festungshauptstadt, Reichsschatzalt, gew_einnahmen, bes_einnahmen, gew_ausgaben, bes_ausgaben, Geldverleih, Geldschenkung, Sonstiges, Reichsschatzneu,
        }

        public void Load(DbDataReader reader)
        {
            this.monat = DatabaseConverter.ToInt32(reader[(int)Felder.monat]);
            this.Tiefland = DatabaseConverter.ToInt32(reader[(int)Felder.Tiefland]);
            this.Tieflandwald = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandwald]);
            this.Tieflandwüste = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandwüste]);
            this.Tieflandsumpf = DatabaseConverter.ToInt32(reader[(int)Felder.Tieflandsumpf]);
            this.Hochland = DatabaseConverter.ToInt32(reader[(int)Felder.Hochland]);
            this.Bergland = DatabaseConverter.ToInt32(reader[(int)Felder.Bergland]);
            this.Gebirge = DatabaseConverter.ToInt32(reader[(int)Felder.Gebirge]);
            this.Burgen = DatabaseConverter.ToInt32(reader[(int)Felder.Burgen]);
            this.Städte = DatabaseConverter.ToInt32(reader[(int)Felder.Städte]);
            this.Festungen = DatabaseConverter.ToInt32(reader[(int)Felder.Festungen]);
            this.Hauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.Hauptstadt]);
            this.Festungshauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.Festungshauptstadt]);
            this.Kampfeinnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.Kampfeinnahmen]);
            this.Pluenderungen = DatabaseConverter.ToInt32(reader[(int)Felder.Pluenderungen]);
            this.eroberte_burgen = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_burgen]);
            this.eroberte_staedte = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_staedte]);
            this.eroberte_festungen = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_festungen]);
            this.eroberte_hauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_hauptstadt]);
            this.eroberte_festungshauptstadt = DatabaseConverter.ToInt32(reader[(int)Felder.eroberte_festungshauptstadt]);
            this.Reichsschatzalt = DatabaseConverter.ToInt32(reader[(int)Felder.Reichsschatzalt]);
            this.gew_einnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.gew_einnahmen]);
            this.bes_einnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.bes_einnahmen]);
            this.gew_ausgaben = DatabaseConverter.ToInt32(reader[(int)Felder.gew_ausgaben]);
            this.bes_ausgaben = DatabaseConverter.ToInt32(reader[(int)Felder.bes_ausgaben]);
            this.Geldverleih = DatabaseConverter.ToInt32(reader[(int)Felder.Geldverleih]);
            this.Geldschenkung = DatabaseConverter.ToInt32(reader[(int)Felder.Geldschenkung]);
            this.Sonstiges = DatabaseConverter.ToInt32(reader[(int)Felder.Sonstiges]);
            this.Reichsschatzneu = DatabaseConverter.ToInt32(reader[(int)Felder.Reichsschatzneu]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
