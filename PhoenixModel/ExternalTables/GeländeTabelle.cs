using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using static PhoenixModel.ExternalTables.GeländeTabelle;

namespace PhoenixModel.ExternalTables {

    public enum TerrainType // entnommmen aus der Tabelle [crossref.mdb][Geleandetypen_crossref]
       {
        Default = 0,
        Wasser = 1,
        Hochland = 2,
        Wald = 3,
        Wüste = 4,
        Sumpf = 5,
        Bergland = 6,
        Gebirge = 7,
        Tiefsee = 8,
        Tiefland = 9,
        Auftauchpunkt = 10,
        Tiefseeeinbahnpunkt = 11,
        AuftauchpunktUnbekannt = 12, // den gibt es nicht in der Tabelle der Datenbank
        Küste = 13
    }

    public class GeländeTabelle: IEigenschaftler
    {
       

        // IEigenschaftler
        public string Bezeichner => Name;
        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Farbe", "Name", "IsWasser"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public TerrainType Typ { get; set; }
        public string Name { get; set; }
        public int Höhe { get; set; }
        public int Einwohner { get; set; }
        public int Einnahmen { get; set; }
        public string Farbe { get; set; }
        public bool IsWasser { get; set; }

        public GeländeTabelle(TerrainType terrainType, string name, int höhe, int einwohner, int einnahmen, string farbe, bool isWasser)
        {
            Typ = terrainType;
            Name = name;
            Höhe = höhe;
            // je Feld
            Einwohner = einwohner;
            Einnahmen = einnahmen; 
            // wird vom Programm ausgewertet
            Farbe = farbe;
            IsWasser = isWasser;
        }

        public override string ToString()
        {
            return $"{Typ}: {Name}, Höhe: {Höhe}, Einwohner: {Einwohner}, EinnahmenView: {Einnahmen}, Farbe: {Farbe}, Ist Wasser: {IsWasser}";
        }

        public static readonly GeländeTabelle[] Terrains =
        {
            new GeländeTabelle(TerrainType.Default, "Default", 0, 0, 0, "#cf216c", true),
            new GeländeTabelle(TerrainType.Wasser, "Wasser", 0, 0, 0, "#7eb8ec", true),
            new GeländeTabelle(TerrainType.Hochland, "Hochland", 2, 40000, 800, "#ccad72", false),
            new GeländeTabelle(TerrainType.Wald, "Wald", 1, 40000, 800, "#22946c", false),
            new GeländeTabelle(TerrainType.Wüste, "Wüste", 1, 15000, 300, "#ffff2c", false),
            new GeländeTabelle(TerrainType.Sumpf, "Sumpf", 1, 15000, 300, "#f39ebd", false),
            new GeländeTabelle(TerrainType.Bergland, "Bergland", 3, 25000, 500, "#94662b", false),
            new GeländeTabelle(TerrainType.Gebirge, "Gebirge", 4, 10000, 200, "#6e6c6a", false),
            new GeländeTabelle(TerrainType.Tiefsee, "Tiefsee", 0, 0, 0, "#2c7394", true),
            new GeländeTabelle(TerrainType.Tiefland, "Tiefland", 1, 50000, 1000, "#8ecc2b", false),
            new GeländeTabelle(TerrainType.Auftauchpunkt, "Auftauchpunkt", 0, 0, 0, "#8616ab", true),
            new GeländeTabelle(TerrainType.Tiefseeeinbahnpunkt, "Tiefseeeinbahnpunkt (Tiefsee)", 0, 0, 0, "#19516b", true),
            new GeländeTabelle(TerrainType.AuftauchpunktUnbekannt, "Auftauchpunkt (unbekannt)", 0, 0, 0, "#e8178e", false),
            new GeländeTabelle(TerrainType.Küste, "Küste", 0, 0, 0, "#7eb8ec", true),
        };
    }
}
