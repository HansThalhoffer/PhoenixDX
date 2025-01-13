using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    public class Fluss : DirectionAdorner
    {
        public static Drawing.DirectionTexture Texture = new("fluss_");
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

        public Fluss(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Fluss_NW, gem.Fluss_NO, gem.Fluss_O, gem.Fluss_SO, gem.Fluss_SW, gem.Fluss_W)
        { }
    }
}
