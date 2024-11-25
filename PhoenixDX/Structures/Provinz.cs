// Province.cs
using PhoenixModel.Karte;
using System.Collections.Generic;

namespace PhoenixDX.Structures
{
    public class Provinz
    {
        public int gf {  get; set; }
        public Dictionary<int, Kleinfeld> Felder { get; set; } = new Dictionary<int, Kleinfeld>(); 
        
        public Provinz(int gf) 
        { 
            this.gf = gf;
        }

        public Kleinfeld? GetPKleinfeld(int kf)
        {
            if (Felder.ContainsKey(kf))
                return Felder[kf];
            var p = new Kleinfeld(gf);
            Felder.Add(kf, p);
            return p;
        }
    }
}
