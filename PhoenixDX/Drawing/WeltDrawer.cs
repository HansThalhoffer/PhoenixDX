using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Classes;
using PhoenixDX.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Drawing
{
    internal static class WeltDrawer
    {
        static float _previousScaleX = 0f;
        static int _trashCount = 0;
        static Texture2D _weiss = null;
        static public bool ShowReichOverlay = false;


        public static void LoadContent(ContentManager contentManager)
        {
            _weiss = contentManager.Load<Texture2D>("Images/hexweiss");
        }


        public static Kleinfeld Draw(SpriteBatch spriteBatch, float scaleX, float scaleY, Vector2? mousePos, bool isMoving, float tileTransparancy, ref Dictionary<int, Provinz> provinzen)
        {
            if (_previousScaleX != 0 && _trashCount < 2 && Math.Abs(scaleX - _previousScaleX) > 0.2f)
            {
                ++_trashCount;
            }
            _previousScaleX = scaleX;
             Color colorTiles = Color.White * tileTransparancy;

            SpriteFont font = FontManager.Fonts["Small"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Vector2 offset = new Vector2(30f * scaleX, 10f * scaleY);
            Vector2 mausPos = Vector2.Zero;
            if (mousePos.HasValue)
            {
                mausPos = new Vector2(mousePos.Value.X, mousePos.Value.Y);
            }
            Kleinfeld selected = null;
            // Draw the map with culling
            foreach (var province in provinzen.Values)
            {

                var posP = province.GetMapPosition(scaleX, scaleY); // aktualisiert die MapSize - Reihenfolge wichtig
                posP.X += offset.X;
                posP.Y += offset.Y;
                // var sizeP = province.GetMapSize();
                foreach (var gemark in province.Felder.Values)
                {
                    var posG = gemark.GetMapPosition(posP, scaleX, scaleY); // aktualisiert die MapSize - Reihenfolge wichtig
                    var sizeG = Kleinfeld.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = isMoving == false && selected == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        selected = gemark;
                  

                    var listTexture = gemark.GetTextures();
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, posP, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, rScreenG, null, inKleinfeld ? Color.Plum : colorTiles);
                    }
                    if (ShowReichOverlay == true && gemark.ReichID > 0 && gemark.Reich != null && inKleinfeld == false)
                    {
                        spriteBatch.Draw(_weiss, rScreenG, null, inKleinfeld ? Color.Plum : gemark.Reich.color * 0.5f);
                    }
                }
            }
            spriteBatch.End();
            return selected;
        }
    }
}
