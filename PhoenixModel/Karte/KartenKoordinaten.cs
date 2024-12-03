using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Karte
{
    public struct KartenKoordinaten
    {
        public int gf;
        public int kf; 
        public int dbx; 
        public int dby;

        public KartenKoordinaten(int gf, int kf, int dbx, int dby)
        {
            this.gf = gf;
            this.kf = kf;   
            this.dbx = dbx;
            this.dby = dby;
        }

        // gibt Spalte und Reihe in der Provinz zurück
        public static Point GetPositionInProvinz(int i)
        {
            if (i <= 4)
                return new Point(i, 1);
            if (i <= 9)
                return new Point(i - 5, 2);
            if (i <= 15)
                return new Point(i - 10, 3);
            if (i <= 22)
                return new Point(i - 17, 4);
            if (i <= 30)
                return new Point(i - 24, 5);
            if (i <= 37)
                return new Point(i - 32, 6);
            if (i <= 43)
                return new Point(i - 38, 7);

            return new Point(i - 44, 8);
        }
    }
}
