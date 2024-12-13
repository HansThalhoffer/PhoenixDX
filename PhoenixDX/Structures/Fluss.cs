namespace PhoenixDX.Structures
{
    public class Fluss : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new("fluss_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Fluss(PhoenixModel.dbErkenfara.Gemark gem) : base(gem.Fluss_NW, gem.Fluss_NO, gem.Fluss_O, gem.Fluss_SO, gem.Fluss_SW, gem.Fluss_W)
        { }
    }
}
