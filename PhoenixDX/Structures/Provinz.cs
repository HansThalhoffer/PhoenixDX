// Province.cs
using Microsoft.Xna.Framework.Content;
using PhoenixModel.Karte;
using System.Collections.Generic;
using static PhoenixModel.Karte.Terrain;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Classes;
using Microsoft.Xna.Framework;
using PhoenixModel.Helper;

namespace PhoenixDX.Structures
{
    public class Provinz : Hex
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

        public Dictionary<int, Kleinfeld> Felder { get; set; } = new Dictionary<int, Kleinfeld>();

        public Provinz(int x, int y, int gf) : base(Hex.RadiusProvinz, false)
        {
            X = x;
            Y = y;
            GF = gf;
        }

        public Vector2 GetMapPosition(float scaleX, float scaleY)
        {
            float x = (X - 1) * ColumnWidth * scaleX;
            float y = (Y - 1) * RowHeight * scaleY;
            if (X % 2 > 0)
                y += RowHeight * scaleY / 2;

            return new Vector2(x, y);
        }

        public Vector2 GetMapSize(float scaleX, float scaleY)
        {
            float sizeX = Height * scaleX;
            float sizeY = Width * scaleY;
            return new Vector2(sizeX, sizeY);
        }

        public Kleinfeld GetPKleinfeld(int kf)
        {
            if (Felder.ContainsKey(kf))
                return Felder[kf];
            var p = new Kleinfeld(GF, kf);
            Felder.Add(kf, p);
            return p;
        }

        static public Texture2D TextureFull { get; private set; }
        static public Texture2D TextureNone { get; private set; }
        static public Texture2D TextureHalfTop { get; private set; }
        static public Texture2D TextureHalfBottom { get; private set; }

        public Texture2D Texture 
        {
            get
            {
                if (GF == 0)
                    return TextureNone;
                if (GF == 701 || GF == 901)
                    return TextureHalfTop;
                if (GF == 712 || GF == 912)
                    return TextureHalfBottom;
            return TextureFull;
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            TextureFull = contentManager.Load<Texture2D>("Images/Hexagon");
            TextureNone = contentManager.Load<Texture2D>("Images/HexagonNone");
            TextureHalfBottom = contentManager.Load<Texture2D>("Images/HexagonHalfBottom");
            TextureHalfTop = contentManager.Load<Texture2D>("Images/HexagonHalfTop");
        }


    }
}
