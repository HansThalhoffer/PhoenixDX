using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Fluss : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new AdornerTexture("fluss_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Fluss(Gemark gem) : base(gem.Fluss_NW, gem.Fluss_NO, gem.Fluss_O, gem.Fluss_SW, gem.Fluss_SO, gem.Fluss_W)
        { }
    }
}
