using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    internal class Kai : DirectionAdorner
    {
        public static Drawing.DirectionTexture Texture = new("kai_");
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

        public Kai(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Kai_NW, gem.Kai_NO, gem.Kai_O, gem.Kai_SO, gem.Kai_SW, gem.Kai_W)
        { }
    }
}
