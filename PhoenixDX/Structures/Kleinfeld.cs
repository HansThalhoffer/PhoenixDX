using PhoenixModel.dbErkenfara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using static PhoenixModel.ExternalTables.GeländeTabelle;
using SharpDX.Direct2D1.Effects;
using System.Drawing;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using PhoenixModel.dbCrossRef;
using Vektor = Microsoft.Xna.Framework.Vector2;
using PhoenixModel.ExternalTables;
using SharpDX.XAudio2;

namespace PhoenixDX.Structures
{
    public class Kleinfeld : Hex
    {

        static Microsoft.Xna.Framework.Vector2 _mapCoords = new();
        static Microsoft.Xna.Framework.Vector2 _mapSize = new();
        static Vektor _scale = new(0,0);
        
        public static readonly int TextureWidth = 138;
        public static readonly int TextureHeight = 160;
        
        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;
        public int ReichID { get; private set; }

        private Reich _reich = null;
        public Reich Reich { get => _reich; 
            set {  
                _reich = value;
                //Adorner.Add("Reich", value);
                Adorner.Insert(0,value);
            }
        }
        public bool IsSelected { get; set; } = false;
        public string Bezeichner { get; private set; }
        public int ReichKennzahl { get; set; }
        public KartenKoordinaten Koordinaten { get; private set; }

        TerrainType _terrainType  = TerrainType.Default;

        // Dictionary<string, KleinfeldAdorner> _adorner = [];
        // public Dictionary<string, KleinfeldAdorner> Adorner { get { return _adorner; } }
        List<KleinfeldAdorner> _adorner = [];
        public List<KleinfeldAdorner> Adorner { get { return _adorner; } }

        public Kleinfeld(int gf, int kf): base(Hex.RadiusGemark, true)
        {
            Koordinaten = new KartenKoordinaten(gf, kf,0,0);
            _terrainType = TerrainType.Default;
            var pos = Koordinaten.GetPositionInProvinz();
            X = pos.X;
            Y = pos.Y;
            Bezeichner = kf.ToString();
        }

        public static Vektor GetMapSize()
        {
            return _mapSize;
        }

        public Vektor GetMapPosition(Microsoft.Xna.Framework.Vector2 provinzCoords, Vektor scale )
        {
            if (scale.X != _scale.X || scale.X != _scale.Y)
            {
                _mapSize = new Microsoft.Xna.Framework.Vector2(Height * scale.X, Width * scale.Y);
                _scale.X = scale.X;
                _scale.Y = scale.Y;
                float x = 0;
                if (Y < 4)
                    x = ((X - 1) * ColumnWidth - (6-Y-x)*ColumnWidth/2 )* scale.X;
                if (Y == 4)
                    x = (X - 1) * ColumnWidth * scale.X;
                if (Y == 5)
                    x = ((X - 1) * ColumnWidth - ColumnWidth/2) * scale.X;
                if (Y == 6)
                    x = (X - 1) * ColumnWidth * scale.X;
                if (Y == 7)
                    x = ((X - 2) * ColumnWidth - ColumnWidth / 2) * scale.X;
                if (Y == 8)
                    x = ((X - 4) * ColumnWidth) * scale.X;

                float y = (Y - 1) * RowHeight * scale.Y;
                _mapCoords = new Microsoft.Xna.Framework.Vector2(provinzCoords.X+x, provinzCoords.Y+y);
            }
            return _mapCoords;
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
            ReichID = gem.Reich ?? -1;
            
            Adorner.Add(new Fluss(gem));
            Adorner.Add(new Kai(gem));
            Adorner.Add(new Bruecke(gem));
            Adorner.Add(new Strasse(gem));
            Adorner.Add(new Wall(gem));

            if (gem.Gebäude != null )
            {
                if (gem.Gebäude.InBau)
                    MappaMundi.Log(this.Koordinaten.gf, this.Koordinaten.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Warning, $"Gebäude in Bau {gem.Gebäude.Bauwerknamen} von {gem.Gebäude.Reich}"));
                if (gem.Gebäude.Zerstört)
                    MappaMundi.Log(this.Koordinaten.gf, this.Koordinaten.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Warning, $"Gebäude zerstört {gem.Gebäude.Bauwerknamen} von {gem.Gebäude.Reich}"));

                string name = gem.Gebäude.Rüstort.Bauwerk;
                if (RuestortSymbol.Ruestorte.ContainsKey(name))
                {
                    // Adorner.Add("Rüstort", RuestortSymbol.Ruestorte[name]);
                    Adorner.Add(RuestortSymbol.Ruestorte[name]);
                }
                else
                {
                    MappaMundi.Log(this.Koordinaten.gf, this.Koordinaten.kf, new PhoenixModel.Program.LogEntry($"Unbekanntes Gebäude {name}"));
                }
            }

            return true;
        }

        #region Selection

        public bool InKleinfeld(Microsoft.Xna.Framework.Vector2 mousePos)
        {
            if (mousePos == Vector2.Zero) 
                return false;
            if (_mapCoords.X > mousePos.X ||  _mapCoords.Y > mousePos.Y)
                return false;
            
            if (mousePos.X > _mapCoords.X + Width *_scale.X * 0.85f || mousePos.Y > _mapCoords.Y + Height *_scale.Y * 0.85f)
               return false;
            
           /* PointF[] hexVertices = {
                new PointF(_mapCoords.X, _mapCoords.Y - Height / 2f), // top
                new PointF(_mapCoords.X + Width / 2f, _mapCoords.Y - Height / 4f), // top-right
                new PointF(_mapCoords.X + Width / 2f, _mapCoords.Y + Height / 4f), // bottom-right
                new PointF(_mapCoords.X, _mapCoords.Y + Height / 2f), // bottom
                new PointF(_mapCoords.X - Width / 2f, _mapCoords.Y + Height / 4f), // bottom-left
                new PointF(_mapCoords.X - Width / 2f, _mapCoords.Y - Height / 4f)  // top-left
            };*/

            return true;

        }

        #endregion

        #region Content

        public List<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            Gelaende gel = GeländeTabelle.Terrains[(int)_terrainType] as Gelaende;
            if (gel != null)
                textures.Add(gel.GetTexture());
            foreach(KleinfeldAdorner adorner in Adorner)
            {
                if (adorner.HasDirections)
                    textures.AddRange(adorner.GetTextures());
                else
                    textures.Add(adorner.GetTexture());
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

            const string folder = "Images/TilesetV/";
            foreach (var adornerTexture in textureValues)
            {
                List<Texture2D> textures = new List<Texture2D>();
                foreach (string name in Enum.GetNames(typeof(Direction)))
                {
                    string fileName = folder + adornerTexture.ImageStartsWith + name;
                    try
                    {
                        textures.Add(contentManager.Load<Texture2D>(fileName));
                    }
                    catch (Exception ex)
                    {
                        MappaMundi.Log(0, 0, ex);
                    }
                }
                adornerTexture.SetTextures(textures.ToArray());
            }

            _isLoaded = true;
        }
        #endregion

    }

}
