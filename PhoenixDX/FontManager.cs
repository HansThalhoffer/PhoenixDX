using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX
{
    internal class FontManager
    {
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();
        
        public static void LoadContent(ContentManager contentManager)
        {
            Fonts.Add("Default",contentManager.Load<SpriteFont>("Fonts/DefaultFont"));
            Fonts.Add("Large", contentManager.Load<SpriteFont>("Fonts/LargeFont"));
            Fonts.Add("Small", contentManager.Load<SpriteFont>("Fonts/SmallFont"));
        }
    }
}
