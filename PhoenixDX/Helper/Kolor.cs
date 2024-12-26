using PhoenixModel.dbPZE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Helper
{
    public static class Kolor 
    {
        public static Microsoft.Xna.Framework.Color Convert(System.Drawing.Color? color)
        {
            if (color == null)
                return Microsoft.Xna.Framework.Color.Transparent;

            return new Microsoft.Xna.Framework.Color(color.Value.R, color.Value.G, color.Value.B);
        }
    }
}
