namespace PhoenixDX.Structures
{
    public class Fluss : GemarkAdorner
    {
        public static AdornerTexture Texture = new("fluss_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Fluss(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Fluss_NW, gem.Fluss_NO, gem.Fluss_O, gem.Fluss_SO, gem.Fluss_SW, gem.Fluss_W)
        { }
    }
}
