using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
{
    /*  wird aktuell noch nicht gebraucht, Figuren haben eigenes Verzeichnis
    public class Verzeichnis : Dictionary<int, GemarkPosition>
    {
       public void Add(GemarkPosition gemarkPosition)
        {
            if (this.ContainsKey(gemarkPosition.Key) == false)
            {
                this.Add(gemarkPosition.Key, gemarkPosition);
            }
        }

        public GemarkPosition? Get(int gf, int kf)
        {
            int key = gf * 100 + kf; 
            if (this.ContainsKey(key))
                return this[key];
            return null;
        }
    }*/

    public class GemarkPosition
    {
        public virtual int gf { get; set; } = 0;
        public virtual int kf { get; set; } = 0;

        public GemarkPosition()
        { }

        public GemarkPosition(int gf, int kf)
        {
            this.gf = gf;
            this.kf = kf;
        }


        public string CreateBezeichner()
        {
            return $"{gf}/{kf}";
        }
        public static string CreateBezeichner(int gf, int kf)
        {
            return $"{gf}/{kf}";
        }
        public int Key
        {
            get { return gf * 100 + kf; }
        }
    }
}

