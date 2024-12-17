using System;
using System.Collections.Generic;
using static PhoenixModel.dbErkenfara.Defaults.Terrain;

namespace PhoenixModel.dbErkenfara.Defaults
{
    public class Terrain
    {
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
            AuftauchpunktUnbekannt // den gibt es nicht in der Tabelle der Datenbank
        }
        

        public TerrainType Typ { get; set; }
        public string Name { get; set; }
        public int Höhe { get; set; }
        public int Einwohner { get; set; }
        public int Einnahmen { get; set; }
        public string Farbe { get; set; }
        public string Art { get; set; }

        public Terrain(TerrainType terrainType, string name, int höhe, int einwohner, int einnahmen, string farbe, string art)
        {
            Typ = terrainType;
            Name = name;
            Höhe = höhe;
            Einwohner = einwohner;
            Einnahmen = einnahmen;
            Farbe = farbe;
            Art = art;
        }

        public override string ToString()
        {
            return $"{Typ}: {Name}, Höhe: {Höhe}, Einwohner: {Einwohner}, Einnahmen: {Einnahmen}, Farbe: {Farbe}, Art: {Art}";
        }

        public static Terrain[] Terrains =
        {
            new Terrain(TerrainType.Default, "Default", 0, 0, 0, "#cf216c", "Meer"),
            new Terrain(TerrainType.Wasser, "Wasser", 0, 0, 0, "#7eb8ec", "Meer"),
            new Terrain(TerrainType.Hochland, "Hochland", 2, 40000, 800, "#ccad72", "Land"),
            new Terrain(TerrainType.Wald, "Wald", 1, 40000, 800, "#22946c", "Land"),
            new Terrain(TerrainType.Wüste, "Wüste", 1, 15000, 300, "#ffff2c", "Land"),
            new Terrain(TerrainType.Sumpf, "Sumpf", 1, 15000, 300, "#f39ebd", "Land"),
            new Terrain(TerrainType.Bergland, "Bergland", 3, 25000, 500, "#94662b", "Land"),
            new Terrain(TerrainType.Gebirge, "Gebirge", 4, 10000, 200, "#6e6c6a", "Land"),
            new Terrain(TerrainType.Tiefsee, "Tiefsee", 0, 0, 0, "#2c7394", "Meer"),
            new Terrain(TerrainType.Tiefland, "Tiefland", 1, 50000, 1000, "#8ecc2b", "Land"),
            new Terrain(TerrainType.Auftauchpunkt, "Auftauchpunkt", 0, 0, 0, "#8616ab", "Meer"),
            new Terrain(TerrainType.Tiefseeeinbahnpunkt, "Tiefseeeinbahnpunkt (Tiefsee)", 0, 0, 0, "#19516b", "Meer"),
            new Terrain(TerrainType.AuftauchpunktUnbekannt, "Auftauchpunkt (unbekannt)", 0, 0, 0, "#e8178e", "Land")
        };
    }
}
