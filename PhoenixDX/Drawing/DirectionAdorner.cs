using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Structures;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;

namespace PhoenixDX.Drawing {
    public abstract class DirectionAdorner : GemarkAdorner
    {
        int[] _value = { 0, 0, 0, 0, 0, 0 };
        bool _empty = false;
        public DirectionAdorner()
        { }

        public DirectionAdorner(int? NW, int? NO, int? O, int? SO, int? SW, int? W) : base()
        {
            _value[(int)Direction.NW] = (int)NW;
            _value[(int)Direction.NO] = (int)NO;
            _value[(int)Direction.O] = (int)O;
            _value[(int)Direction.SO] = (int)SO;
            _value[(int)Direction.SW] = (int)SW;
            _value[(int)Direction.W] = (int)W;
            _empty = _value[0] + _value[1] + _value[2] + _value[3] + _value[4] + _value[5] == 0;

        }

        protected abstract Drawing.DirectionTexture GetDirectionTexture();

        private List<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (HasDirection(direction) > 0)
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

        public bool IsEmpty => _empty;
        
        public BaseTexture CreateTexture()
        {
            if (IsEmpty)
                return null;

            string cacheKey = $"{GetType().Name} {_value[0]} {_value[1]} {_value[2]} {_value[3]} {_value[4]} {_value[5]}";
            if (TextureCache.Contains(cacheKey))
                return TextureCache.Get(cacheKey);
            List<Texture2D> list = GetTextures();
            if (list != null && list.Count > 0)
            {
                var texture = TextureCache.MergeTextures(list);
                TextureCache.Set(cacheKey, texture);
            }
            return null;    
        }

        private BaseTexture _texture = null;
        public override BaseTexture GetTexture()
        {
            if (_texture == null)
                _texture = CreateTexture();
            return _texture;
        }



    }
}
