﻿namespace PhoenixDX.Structures
{
    public class Strasse : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new("strasse_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Strasse(PhoenixModel.dbErkenfara.Gemark gem) : base(gem.Strasse_NW, gem.Strasse_NO, gem.Strasse_O, gem.Strasse_SO, gem.Strasse_SW, gem.Strasse_W)
        { }
    }
}
