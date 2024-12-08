using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Strasse : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new AdornerTexture("strasse_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Strasse(Gemark gem) : base(gem.Strasse_NW, gem.Strasse_NO, gem.Strasse_O, gem.Strasse_SO, gem.Strasse_SW, gem.Strasse_W)
        { }
    }
}
