using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using PhoenixModel.View;
using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    internal class Marker :ColorAdorner
    {
        static Texture2D _hexTexture = null;
        MarkerType MarkerType { get; set; }
        static ColoredTexture[] coloredTextures = new ColoredTexture[Enum.GetNames(typeof(MarkerType)).Length] ;

        public Marker(MarkerType mark)
        {
            MarkerType = mark;
        }

        public static void LoadContent(ContentManager contentManager)
        {
            try
            {
                _hexTexture = contentManager.Load<Texture2D>("Images/TilesetV/Info");
                coloredTextures[0] = new ColoredTexture(_hexTexture, Color.White);
                coloredTextures[1] = new ColoredTexture(_hexTexture, Color.Orange);
                coloredTextures[2] = new ColoredTexture(_hexTexture, Color.Red);
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, "Die Textur für die Farben einer Nation konnte nicht geladen werden", ex);
            }
        }

        public override ColoredTexture CreateTexture()
        {
            switch (MarkerType)
            {
                case MarkerType.None:
                    return null;
                case MarkerType.Info:
                    return coloredTextures[0];
                case MarkerType.Warning:
                    return coloredTextures[1];
                case MarkerType.Fatality:
                    return coloredTextures[2];
                default: return null;
            }
        }
    }
}
