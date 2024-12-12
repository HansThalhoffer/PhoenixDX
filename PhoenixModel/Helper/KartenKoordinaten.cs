using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
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
        public Position GetPositionInProvinz()
        {
            if (kf > 48)
                return new Position(0, 0);

            if (kf <= 4)
                return new Position(kf + 4, 1);
            if (kf <= 9)
                return new Position(kf - 1, 2);
            if (kf <= 15)
                return new Position(kf - 7, 3);
            if (kf <= 22)
                return new Position(kf - 15, 4);
            if (kf <= 30)
                return new Position(kf - 22, 5);
            if (kf <= 37)
                return new Position(kf - 30, 6);
            if (kf <= 43)
                return new Position(kf - 35, 7);

            return new Position(kf - 39, 8);
        }
    }
}
