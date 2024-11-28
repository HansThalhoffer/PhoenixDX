﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PhoenixModel.Karte;
using SharpDX.Direct3D9;
using static PhoenixModel.Karte.Terrain;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Net;

namespace PhoenixDX.Structures
{
    public class Gelaende : PhoenixModel.Karte.Terrain
    {
        Texture2D hexTexture;
       
        public Gelaende(Terrain source, string image, ContentManager contentManager):
            base(source.Typ, source.Name, source.Höhe, source.Einwohner, source.Einnahmen, source.Farbe, source.Art)
        {
            try
            {
                const string folder = "Images/";
                hexTexture = contentManager.Load<Texture2D>(folder + image);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Texture2D GetTexture() { return hexTexture; }

        static bool _isLoaded = false;
        public static bool IsLoaded { get { return _isLoaded; }}
        
        public static void LoadContent(ContentManager contentManager)
        {
            Terrains[(int)TerrainType.Default] = new Gelaende(Terrains[(int)TerrainType.Default], "defaultTerrain", contentManager);
            Terrains[(int)TerrainType.Wasser] = new Gelaende(Terrains[(int)TerrainType.Wasser], "ocean", contentManager);
            Terrains[(int)TerrainType.Hochland] = new Gelaende(Terrains[(int)TerrainType.Hochland], "highland", contentManager);
            Terrains[(int)TerrainType.Wald] = new Gelaende(Terrains[(int)TerrainType.Wald], "forest", contentManager);
            Terrains[(int)TerrainType.Wüste] = new Gelaende(Terrains[(int)TerrainType.Wüste], "desert", contentManager);
            Terrains[(int)TerrainType.Sumpf] = new Gelaende(Terrains[(int)TerrainType.Sumpf], "swamp", contentManager);
            Terrains[(int)TerrainType.Bergland] = new Gelaende(Terrains[(int)TerrainType.Bergland], "highland", contentManager);
            Terrains[(int)TerrainType.Gebirge] = new Gelaende(Terrains[(int)TerrainType.Gebirge], "mountain", contentManager);
            Terrains[(int)TerrainType.Tiefsee] = new Gelaende(Terrains[(int)TerrainType.Tiefsee], "deepsea", contentManager);
            Terrains[(int)TerrainType.Tiefland] = new Gelaende(Terrains[(int)TerrainType.Default], "tiefland", contentManager);
            Terrains[(int)TerrainType.Auftauchpunkt] = new Gelaende(Terrains[(int)TerrainType.Default], "auftauchpunkt_ocean", contentManager);
            Terrains[(int)TerrainType.Tiefseeeinbahnpunkt] = new Gelaende(Terrains[(int)TerrainType.Default], "deepseapoint", contentManager);
            Terrains[(int)TerrainType.AuftauchpunktUnbekannt] = new Gelaende(Terrains[(int)TerrainType.Default], "auftauchpunkt_unbekannt", contentManager);
            _isLoaded = true;
        }

    }
}