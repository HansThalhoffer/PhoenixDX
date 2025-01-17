// Province.cs
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Structures
{
    internal class Provinz : Hex
    {
        private int _gf;
        public int X { get; private set; }
        public int Y { get; private set; }

        public int GF
        {
            get { return _gf; }
            private set
            {
                _gf = value;
            }
        }

        public string Bezeichner
        {
            get { return _gf.ToString(); }
        }

        public Dictionary<int, Gemark> Felder { get; set; } = [];

        public Provinz(int x, int y, int gf) : base(Hex.RadiusProvinz, false)
        {
            X = x;
            Y = y;
            GF = gf;
        }

        Vektor _mapCoords = new();
        Vektor _mapSize = new();
        Vektor _scale = new(0,0);
        public Vektor GetMapPosition(Vektor scale)
        {
            if (scale.X != _scale.X || scale.X != _scale.Y)
            {
                _scale.X = scale.X;
                _scale.Y = scale.Y;
                float x = (X - 1) * ColumnWidth * scale.X;
                float y = (Y - 1) * RowHeight * scale.Y;
                if (X % 2 > 0)
                    y += RowHeight * scale.Y / 2;
                _mapCoords = new Microsoft.Xna.Framework.Vector2(x, y);
                _mapSize = new Microsoft.Xna.Framework.Vector2(Height * scale.X, Width * scale.Y);
            }
            return _mapCoords;
        }

        public Microsoft.Xna.Framework.Vector2 GetMapSize()
        {
            return _mapSize;
        }

        public Gemark GetKleinfeld(int kf)
        {
            Gemark gemark = null;
            if (Felder.TryGetValue(kf, out gemark))
                return gemark;
            return null;
        }

        public Gemark GetOrCreateKleinfeld(int kf)
        {
            Gemark gemark = null;
            if (Felder.TryGetValue(kf, out gemark))
                return gemark;
            gemark = new Gemark(GF, kf);
            Felder.Add(kf, gemark);
            return gemark;
        }
    }
}
