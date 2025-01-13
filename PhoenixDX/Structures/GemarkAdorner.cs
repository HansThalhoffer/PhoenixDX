using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    public abstract class GemarkAdorner
    {
        public GemarkAdorner()
        {
        }

        protected abstract DirectionTexture GetDirectionTexture();

        int[] _value = {0,0,0,0,0,0};

        public GemarkAdorner(int? NW, int? NO, int? O, int? SO, int? SW, int? W)
        {
            _value[ (int)Direction.NW] = (int)NW;  
            _value[ (int)Direction.NO] = (int)NO;
            _value[ (int)Direction.O] = (int)O;
            _value[(int)Direction.SO] = (int)SO;
            _value[ (int)Direction.SW] = (int)SW;
            _value[ (int)Direction.W] = (int)W;
        }

        public bool HasDirections = true;
        virtual public Texture2D GetTexture()
        {
            return null;
        }

        public bool HasColor = false;
        virtual public ColoredTexture GetColoredTexture()
        {
            return null;
        }

        virtual public List<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (HasDirection(direction) > 0 )
                {
                    var t = GetDirectionTexture();
                    textures.Add(t.GetTexture(direction));
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
