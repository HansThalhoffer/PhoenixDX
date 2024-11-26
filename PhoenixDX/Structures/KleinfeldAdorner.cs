using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PhoenixDX.Structures
{
    public enum Direction 
    {
        NW = 0, NO =1, O=2 , SO=3, SW=4, W=5
    }

    public class AdornerTexture
    {
        public string ImageStartsWith = string.Empty;

        public AdornerTexture(string imageStartsWith)
        {
            ImageStartsWith = imageStartsWith;
        }

        Texture2D[]? _hexTexture;
        public void SetTextures(Texture2D[] texture)
        {
            _hexTexture = texture;
        }
        public Texture2D GetTexture(Direction dir)
        {
            return _hexTexture[(int)dir];
        }
    }

    public abstract class KleinfeldAdorner
    {
        public KleinfeldAdorner()
        {
        }

        public abstract AdornerTexture GetAdornerTexture();

        int[] _value = {0,0,0,0,0,0};

        public KleinfeldAdorner(int? NW, int? NO, int? O, int? SO, int? SW, int? W)
        {
            _value[ (int)Direction.NW] = (int)NW;  
            _value[ (int)Direction.NO] = (int)NO;
            _value[ (int)Direction.O] = (int)O;
            _value[ (int)Direction.SO] = (int)SO;
            _value[ (int)Direction.SW] = (int)SW;
            _value[ (int)Direction.W] = (int)W;
        }

        public List<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (HasDirection(direction) > 0 )
                {
                    textures.Add(GetAdornerTexture().GetTexture(direction));
                }
            }
            return textures;
        }

        public int HasDirection(Direction direction)
        {
            return _value[(int)direction];
        }
        public int SetDirection(Direction direction, int? value)
        {
            return _value[(int)direction] = (int)value;
        }
    }
}
