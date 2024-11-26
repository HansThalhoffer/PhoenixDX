using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Kai : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new AdornerTexture("kai_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Kai(Gemark gem) : base(gem.Kai_NW, gem.Kai_NO, gem.Kai_O, gem.Kai_SW, gem.Kai_SO, gem.Kai_W)
        { }
    }
}
