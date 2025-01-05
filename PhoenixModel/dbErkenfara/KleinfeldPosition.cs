using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara {
    /*  wird aktuell noch nicht gebraucht, Figuren haben eigenes Verzeichnis
    public class Verzeichnis : Dictionary<int, KleinfeldPosition>
    {
       public void Add(KleinfeldPosition gemarkPosition)
        {
            if (this.ContainsKey(gemarkPosition.Key) == false)
            {
                this.Add(gemarkPosition.Key, gemarkPosition);
            }
        }

        public KleinfeldPosition? Get(int gf, int kf)
        {
            int key = gf * 100 + kf; 
            if (this.ContainsKey(key))
                return this[key];
            return null;
        }
    }*/

    [DebuggerDisplay("{CreateBezeichner()}")]
    public class KleinfeldPosition
    {
        /// <summary>
        ///  Provinz
        /// </summary>
        public virtual int gf { get; set; } = 0;
        /// <summary>
        ///  Gemark
        /// </summary>
        public virtual int kf { get; set; } = 0;

        public KleinfeldPosition()
        { }

        public KleinfeldPosition(int gf, int kf)
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

