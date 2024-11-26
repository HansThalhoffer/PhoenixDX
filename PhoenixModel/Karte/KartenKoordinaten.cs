using System;
using System.Collections.Generic;
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
    }
}
