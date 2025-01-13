using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Drawing
{
    /// <summary>
    ///  definiert die Schnittstelle zwingend
    /// </summary>
    public abstract class BaseTexture
    {
        public abstract Texture2D Texture { get; }
    }

    public class SimpleTexture:BaseTexture
    {
        private Texture2D _texture;
        public SimpleTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public override Texture2D Texture { get { return _texture; } }
    }

    /// <summary>
    /// Die Form der Texture unterstützt eine Farbe, damit können neutrale Texturen farbig gemalt werden
    /// </summary>
    public class ColoredTexture : SimpleTexture
    {
        public Color Color { get; set; }

        public ColoredTexture(Texture2D texture, Color color) : base(texture)
        {
            this.Color = color;
        }
        public ColoredTexture(Texture2D texture) : base(texture)
        {
            this.Color = Color.White;
        }
    }

    /// <summary>
    /// Windrichtungen für die Texturen
    /// </summary>
    public enum Direction
    {
        NW = 0, NO = 1, O = 2, SO = 3, SW = 4, W = 5
    }

    /// <summary>
    /// DirectionTexture überträgt Windrichtungen in Texturen
    ///  - ist nicht von BaseTexture abgeleitet, da eine andere Funktionalität
    /// </summary>
    public class DirectionTexture
    {
        public string ImageStartsWith = string.Empty;
        public int IndexStartsWith = 0;

        public DirectionTexture(string imageStartsWith)
        {
            ImageStartsWith = imageStartsWith;
        }

        Texture2D[] _hexTexture;
        public void SetTextures(Texture2D[] texture)
        {
            _hexTexture = texture;
        }
        public Texture2D GetTexture(Direction dir)
        {
            return _hexTexture[(int)dir];
        }
    }

   
}
