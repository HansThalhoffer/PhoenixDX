﻿namespace PhoenixDX.Structures
{
    public class Bruecke : KleinfeldAdorner
    {
        public static AdornerTexture Texture = new("bruecke_");
        public override AdornerTexture GetAdornerTexture() { return Texture; }

        public Bruecke(PhoenixModel.dbErkenfara.Gemark gem) : base(gem.Bruecke_NW, gem.Bruecke_NO, gem.Bruecke_O, gem.Bruecke_SO, gem.Bruecke_SW, gem.Bruecke_W)
        { }
    }
}
