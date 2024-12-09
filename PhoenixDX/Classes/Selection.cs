using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Classes
{
    public class Selection
    {
        List<Texture2D> hexTextures = new List<Texture2D>();
        int index = 0;
        public void LoadContent(ContentManager contentManager)
        {
            hexTextures.Add(contentManager.Load<Texture2D>("Images/maus1"));
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 posG, GameTime gameTime)
        {
            var sizeG = Kleinfeld.GetMapSize();
            Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
            spriteBatch.Draw(hexTextures[0], rScreenG, null, Color.White);

        }
    }
}
