﻿// Province.cs
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

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

        Microsoft.Xna.Framework.Vector2 _mapCoords = new Microsoft.Xna.Framework.Vector2();
        Microsoft.Xna.Framework.Vector2 _mapSize = new Microsoft.Xna.Framework.Vector2();
        float _scaleX = 0;
        float _scaleY = 0;

        public Microsoft.Xna.Framework.Vector2 GetMapPosition(float scaleX, float scaleY)
        {
            if (scaleX != _scaleX || scaleX != _scaleY)
            {
                _scaleX = scaleX;
                _scaleY = scaleY;
                float x = (X - 1) * ColumnWidth * scaleX;
                float y = (Y - 1) * RowHeight * scaleY;
                if (X % 2 > 0)
                    y += RowHeight * scaleY / 2;
                _mapCoords = new Microsoft.Xna.Framework.Vector2(x, y);
                _mapSize = new Microsoft.Xna.Framework.Vector2(Height * scaleX, Width * scaleY);
            }
            return _mapCoords;
        }

        public Microsoft.Xna.Framework.Vector2 GetMapSize()
        {
            return _mapSize;
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
        static public Texture2D TextureYellow { get; private set; }
        static public Texture2D TextureHalfTop { get; private set; }
        static public Texture2D TextureHalfBottom { get; private set; }

        /*public Texture2D Texture 
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
            TextureYellow = contentManager.Load<Texture2D>("Images/Hexagon_gelb");
            TextureFull = contentManager.Load<Texture2D>("Images/Hexagon");
            TextureNone = contentManager.Load<Texture2D>("Images/HexagonNone");
            TextureHalfBottom = contentManager.Load<Texture2D>("Images/HexagonHalfBottom");
            TextureHalfTop = contentManager.Load<Texture2D>("Images/HexagonHalfTop");
        }
        */

    }
}
