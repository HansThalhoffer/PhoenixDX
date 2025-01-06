using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
using static PhoenixModel.ExternalTables.GeländeTabelle;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Net;
using PhoenixModel.ExternalTables;

namespace PhoenixDX.Structures
{
    public class Gelaende : GeländeTabelle
    {
        Texture2D hexTexture;
        List<Texture2D>hexTextures = new List<Texture2D>();

        public Gelaende(GeländeTabelle source, string image, ContentManager contentManager):
            base(source.Typ, source.Name, source.Höhe, source.Einwohner, source.Einnahmen, source.Farbe, source.IsWasser)
        {
            try
            {
                string folder = "Images/TilesetV/";
                /*if (image == "mountain")
                {
                    hexTexture = contentManager.Load<Texture2D>("Images/TilesetN/" + image);
                    hexTextures.Add( contentManager.Load<Texture2D>("Images/TilesetN/mountain"));
                    hexTextures.Add(contentManager.Load<Texture2D>("Images/TilesetN/mountain1" ));
                    hexTextures.Add(contentManager.Load<Texture2D>("Images/TilesetN/mountain2"));
                }
                else
                */
                    hexTexture = contentManager.Load<Texture2D>(folder + image);
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, $"Die Textur für {source.Name} konnte nicht geladen werden", ex);
            }
        }

        public Texture2D GetTexture() { 
        
            //if (hexTextures.Count == 0)
                return hexTexture;
            /*Random rnd = new Random();
            int index = rnd.Next(0,hexTextures.Count);
            if (index >= hexTextures.Count)
                index = hexTextures.Count;
            return hexTextures[index];*/
        }

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
            Terrains[(int)TerrainType.Tiefland] = new Gelaende(Terrains[(int)TerrainType.Tiefland], "tiefland", contentManager);
            Terrains[(int)TerrainType.Auftauchpunkt] = new Gelaende(Terrains[(int)TerrainType.Auftauchpunkt], "auftauchpunkt_ocean", contentManager);
            Terrains[(int)TerrainType.Tiefseeeinbahnpunkt] = new Gelaende(Terrains[(int)TerrainType.Tiefseeeinbahnpunkt], "deepseapoint", contentManager);
            Terrains[(int)TerrainType.AuftauchpunktUnbekannt] = new Gelaende(Terrains[(int)TerrainType.AuftauchpunktUnbekannt], "auftauchpunkt_unbekannt", contentManager);
            _isLoaded = true;
        }

    }
}
