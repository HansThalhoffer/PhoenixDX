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
    // Windrichtungen für die Texturen
    public enum Direction
    {
        NW = 0, NO = 1, O = 2, SO = 3, SW = 4, W = 5
    }

    /// <summary>
    /// DirectionTexture überträgt Windrichtungen in Texturen
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

    /// <summary>
    /// Die Form der Texture unterstützt eine Farbe, damit können neutrale Texturen farbig gemalt werden
    /// </summary>
    public class ColoredTexture
    {
        public Color Color { get; set; }
        public Texture2D Texture { get; set; }
        public ColoredTexture(Texture2D texture, Color color) 
        {  
            this.Color = color; 
            this.Texture = texture;
        }
        public ColoredTexture(Texture2D texture)
        {
            this.Color = Color.White;
            this.Texture = texture;
        }
    }
}
