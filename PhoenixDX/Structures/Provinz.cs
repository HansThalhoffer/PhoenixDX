// Province.cs
using Microsoft.Xna.Framework.Content;
using PhoenixModel.Karte;
using System.Collections.Generic;
using static PhoenixModel.Karte.Terrain;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Classes;
using Microsoft.Xna.Framework;

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

        public static int MapToGF(int x, int y)
        {
            int gf = 0;

            return gf;
        }

        public static Point MapToXY(int gf)
        {
            var pos = new Point(0,0);
         
            return pos;
        }

        public string Bezeichner
        {
            get { return _gf.ToString(); }           
        }

        public Dictionary<int, Kleinfeld> Felder { get; set; } = new Dictionary<int, Kleinfeld>();

        public Provinz(int x, int y) : base(Hex.RadiusProvinz, false)
        {
            X = x;
            Y = y;
        }

        public Vector2 GetMapPosition(float scaleX,float scaleY)
        {
            float x = (X - 1) * ColumnWidth  *scaleX;
            float y = (Y - 1) * RowHeight * scaleY;
            if (X % 2 > 0)
                y += RowHeight * scaleY / 2;

            /*if (Spalte <= 6)
                canvTop += RowHeight / 2 * (6 - Spalte);
            else if (Spalte <= 10)
            {
                if (Spalte % 2 == 1)
                    canvTop -= RowHeight / 2;
            }
            else
                canvTop += RowHeight / 2 * (Spalte - 10);
            */

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

        static public Texture2D Texture{ get; private set; }

        public static void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>("Images/Hexagon");
        }


    }
}
