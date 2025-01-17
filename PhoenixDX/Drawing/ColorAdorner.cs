using PhoenixDX.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Drawing
{
    internal abstract class ColorAdorner : GemarkAdorner
    {
        ColoredTexture _ColorTexture = null;

        public abstract ColoredTexture CreateTexture();

        public override BaseTexture GetTexture()
        {
            if (_ColorTexture == null)
                _ColorTexture = CreateTexture();
            return _ColorTexture;
        }
    }
}
