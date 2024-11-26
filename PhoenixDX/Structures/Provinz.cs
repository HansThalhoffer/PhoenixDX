// Province.cs
using PhoenixModel.Karte;
using System.Collections.Generic;

namespace PhoenixDX.Structures
{
    public class Provinz
    {
        public int GF {  get; private set; }
        public Dictionary<int, Kleinfeld> Felder { get; set; } = new Dictionary<int, Kleinfeld>(); 
        
        public Provinz(int gf) 
        {
            GF = gf;
        }

        public Kleinfeld? GetPKleinfeld(int kf)
        {
            if (Felder.ContainsKey(kf))
                return Felder[kf];
            var p = new Kleinfeld(GF,kf);
            Felder.Add(kf, p);
            return p;
        }
    }
}
