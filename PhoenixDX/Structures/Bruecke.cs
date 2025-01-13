using PhoenixDX.Drawing;

namespace PhoenixDX.Structures
{
    public class Bruecke : DirectionAdorner
    {
        public static Drawing.DirectionTexture Texture = new("bruecke_");
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

     

        public Bruecke(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Bruecke_NW, gem.Bruecke_NO, gem.Bruecke_O, gem.Bruecke_SO, gem.Bruecke_SW, gem.Bruecke_W)
        { }
    }
}
