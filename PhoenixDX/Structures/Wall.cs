using PhoenixModel.dbErkenfara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Wall : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new("wand_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Wall(Gemark gem) : base(gem.Wall_NW, gem.Wall_NO, gem.Wall_O, gem.Wall_SO, gem.Wall_SW, gem.Wall_W)
        { }

        public static string GetimageStartsWith()
        {
            return "wand_";
        }
    }
}
