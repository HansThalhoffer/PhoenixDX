using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Kleinfeld
    {
        public class Fluss:KleinfeldAdorner
        {
            public Fluss(Gemark gem): base(gem.Fluss_NW,gem.Fluss_NO,gem.Fluss_O, gem.Fluss_SW,gem.Fluss_SO, gem.Fluss_W)
            { }
        }
        public class Wall : KleinfeldAdorner
        {
            public Wall(Gemark gem) : base(gem.Wall_NW, gem.Wall_NO, gem.Wall_O, gem.Wall_SW, gem.Wall_SO, gem.Wall_W)
            { }
        }
        public class Bruecke : KleinfeldAdorner
        {
            public  Bruecke(Gemark gem) : base(gem.Bruecke_NW, gem.Bruecke_NO, gem.Bruecke_O, gem.Bruecke_SW, gem.Bruecke_SO, gem.Bruecke_W)
            { }
        }
        public class Strasse : KleinfeldAdorner
        {
            public Strasse(Gemark gem) : base(gem.Strasse_NW, gem.Strasse_NO, gem.Strasse_O, gem.Strasse_SW, gem.Strasse_SO, gem.Strasse_W)
            { }
        }
        public class Kai : KleinfeldAdorner
        {
            public Kai(Gemark gem) : base(gem.Kai_NW, gem.Kai_NO, gem.Kai_O, gem.Kai_SW, gem.Kai_SO, gem.Kai_W)
            { }
        }

        public int ReichKennzahl {  get; set; }

        Fluss _fluss { get; set; }
        Wall _wall {  get; set; }
        Bruecke _bruecke { get; set; }
        Strasse _strasse{ get; set; }
        Kai _kai { get; set; }

        public Kleinfeld(int kf) 
        { 
        }

        public void Initialize(Gemark gem)
        {
            ReichKennzahl = (int) gem.Reich;
            _fluss = new Fluss(gem);
            _wall = new Wall(gem);
            _bruecke = new Bruecke(gem);
            _strasse = new Strasse(gem);
            _kai = new Kai(gem);  
        }
    }
}
