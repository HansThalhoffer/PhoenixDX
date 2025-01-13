using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Helper;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Generic;
using PhoenixModel.dbErkenfara;

namespace PhoenixDX.Structures
{
    public class Marker : GemarkAdorner
    {
      
        static Dictionary<MarkerType, Texture2D> textures = [];
        Texture2D hexTexture = null;

        public Marker(MarkerType mark)
        {
            HasDirections = false;
            hexTexture = textures[mark];
        }

        public static void LoadContent(ContentManager contentManager)
        {
            try
            {
                textures.Add(MarkerType.Info, contentManager.Load<Texture2D>("Images/TilesetV/Info"));
                textures.Add(MarkerType.Warning, contentManager.Load<Texture2D>("Images/TilesetV/Warning"));
                textures.Add(MarkerType.Fatality, contentManager.Load<Texture2D>("Images/TilesetV/Fatality"));
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, "Die Textur für die Farben einer Nation konnte nicht geladen werden", ex);
            }
        }

        protected override Drawing.DirectionTexture GetDirectionTexture()
        {
            return null;
        }

        public override List<Texture2D> GetTextures()
        {
            return new List<Texture2D>() { hexTexture };
        }

        public override Texture2D GetTexture()
        {
            return hexTexture;
        }
    }
}
