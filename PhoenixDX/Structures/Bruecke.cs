using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Bruecke : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new AdornerTexture("bruecke_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Bruecke(Gemark gem) : base(gem.Bruecke_NW, gem.Bruecke_NO, gem.Bruecke_O, gem.Bruecke_SW, gem.Bruecke_SO, gem.Bruecke_W)
        { }
    }
}
