﻿namespace PhoenixDX.Structures
{
    public class Kai : GemarkAdorner
    {
        public static AdornerTexture Texture = new("kai_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Kai(PhoenixModel.dbErkenfara.KleinFeld gem) : base(gem.Kai_NW, gem.Kai_NO, gem.Kai_O, gem.Kai_SO, gem.Kai_SW, gem.Kai_W)
        { }
    }
}
