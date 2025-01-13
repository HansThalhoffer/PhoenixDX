using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    public class Strasse : DirectionAdorner
    {
        public static Drawing.DirectionTexture Texture = new("strasse_");
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

        public Strasse(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Strasse_NW, gem.Strasse_NO, gem.Strasse_O, gem.Strasse_SO, gem.Strasse_SW, gem.Strasse_W)
        { }
    }
}
