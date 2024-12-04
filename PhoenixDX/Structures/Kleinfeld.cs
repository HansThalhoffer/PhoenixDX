using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using static PhoenixModel.Karte.Terrain;
using System.Numerics;

namespace PhoenixDX.Structures
{
    public class Kleinfeld : Hex
    {
        public int ReichKennzahl { get; set; }
        public KartenKoordinaten Koordinaten { get; private set; }

        public static readonly int TextureWidth = 138;
        public static readonly int TextureHeight = 160;


        TerrainType _terrainType  = TerrainType.Default;

        Dictionary<string, KleinfeldAdorner> _adorner = new Dictionary<string, KleinfeldAdorner>();
        public Dictionary<string, KleinfeldAdorner> Adorner { get { return _adorner; } }

        public Kleinfeld(int gf, int kf): base(Hex.RadiusGemark, true)
        {
            Koordinaten = new KartenKoordinaten(gf, kf,0,0);
        }

        public Vector2 Position { get
            {
                Vector2 v = new Vector2 ( Koordinaten.dbx * Kleinfeld.TextureWidth, Koordinaten.dby * Kleinfeld.TextureHeight);
                return v;
            } 
        }

        bool _isInitalized = false;
        public bool Initialize(Gemark gem)
        {
            if (_isInitalized == true)
                return false;
            _isInitalized = true;
            ReichKennzahl = (int)gem.Reich;
            if (gem.Gelaendetyp <= (int) TerrainType.AuftauchpunktUnbekannt)
                _terrainType = (TerrainType)gem.Gelaendetyp;

            Koordinaten =  new KartenKoordinaten(Koordinaten.gf, Koordinaten.kf, (int)gem.x, (int)gem.y);
            
            Adorner.Add("Fluss", new Fluss(gem));
            Adorner.Add("Wand", new Wall(gem));
            Adorner.Add("Brücke", new Bruecke(gem));
            Adorner.Add("Strasse", new Strasse(gem));
            Adorner.Add("Kai", new Kai(gem));
            return true;
        }

        #region Content

        public List<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            Gelaende gel = Terrain.Terrains[(int)_terrainType] as Gelaende;
            if (gel != null)
                textures.Add(gel.GetTexture());
            foreach(var adorner in Adorner.Values)
            {
                textures.AddRange(adorner.GetTextures());
            }
            
            return textures;
        }

        static bool _isLoaded = false;
        public static bool IsLoaded { get { return _isLoaded; } }
        
        public static void LoadContent(ContentManager contentManager)
        {
            // Get all types in the current assembly that derive from the base type
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(KleinfeldAdorner)))
                .ToList();

            var textureValues = new List<AdornerTexture>();
            foreach (var type in derivedTypes)
            {
                var textureField = type.GetField("Texture", BindingFlags.Public | BindingFlags.Static);
                if (textureField != null && textureField.FieldType == typeof(AdornerTexture))
                {
                    var fieldValue = textureField.GetValue(null) as AdornerTexture;
                    if (fieldValue != null)
                    {
                        textureValues.Add(fieldValue);
                    }
                }
            }
            const string folder = "Images/";
            List< Texture2D> textures = new List< Texture2D>();
            foreach (var adornerTexture in textureValues)
            {
                foreach (string name in Enum.GetNames(typeof(Direction)))
                {
                    string fileName = folder + adornerTexture.ImageStartsWith + name;
                    try
                    {
                        textures.Add(contentManager.Load<Texture2D>(fileName));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                adornerTexture.SetTextures(textures.ToArray());
            }

            _isLoaded = true;
        }
        #endregion

    }

}
