// Province.cs
using Microsoft.Xna.Framework.Content;
using PhoenixModel.Karte;
using System.Collections.Generic;
using System.Numerics;
using static PhoenixModel.Karte.Terrain;
using Microsoft.Xna.Framework.Graphics;

namespace PhoenixDX.Structures
{
    public class Provinz : Hex
    {
        private int _gf;
        public int Spalte { get; private set; }
        public int Reihe { get; private set; }

        public int GF
        {
            get { return _gf; }
            private set
            {
                _gf = value;
                Spalte = _gf / 100;
                Reihe = _gf % 100;
            }
        }
        public Dictionary<int, Kleinfeld> Felder { get; set; } = new Dictionary<int, Kleinfeld>();

        public Provinz(int gf) : base(Hex.RadiusProvinz)
        {
            GF = gf;
        }

        public Vector2 GetPosition()
        {
            double canvTop = (Reihe - 1) * RowHeight;
            double canvLeft = (Spalte - 1) * ColumnWidth;

            if (Spalte <= 6)
                canvTop += RowHeight / 2 * (6 - Spalte);
            else if (Spalte <= 10)
            {
                if (Spalte % 2 == 1)
                    canvTop -= RowHeight / 2;
            }
            else
                canvTop += RowHeight / 2 * (Spalte - 10);
            return new Vector2((float)canvTop, (float)canvLeft);
        }


        public Kleinfeld? GetPKleinfeld(int kf)
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
            Texture = contentManager.Load<Texture2D>("Images/reich_braun");
        }


    }
}
