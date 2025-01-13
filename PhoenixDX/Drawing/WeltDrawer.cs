using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Helper;
using PhoenixDX.Structures;
using System;
using System.Collections.Generic;

using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Drawing
{
    internal static class WeltDrawer
    {
        static Texture2D _weiss = null;
        static Texture2D _selection1 = null;
        static Texture2D _selection2 = null;
        static public bool ShowReichOverlay = false;

        // für optimierungen siehe http://rbwhitaker.wikidot.com/render-to-texture

        public static void LoadContent(ContentManager contentManager)
        {
            _weiss = contentManager.Load<Texture2D>("Images/hexweiss");
            _selection1 = contentManager.Load<Texture2D>("Images/maus1");
            _selection2 = contentManager.Load<Texture2D>("Images/maus2");
        }

        static double _tick = 0;
        static double _animated = 0;
        static bool _toggle = false;
        public static Gemark Draw(SpriteBatch spriteBatch, Vektor scale, Vektor? mousePos, bool isMoving,  
            ref Dictionary<int, Provinz> provinzen, TimeSpan gameTime, Gemark selected, Rectangle visibleScreen)
        {         
            _animated += gameTime.TotalMilliseconds - _tick;
            _tick = gameTime.TotalMilliseconds;
            if (_animated > 100d)
            {
                _toggle = !_toggle;
                _animated = 0;
            }
            
            SpriteFont font = FontManager.Fonts["Small"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Vektor offset = new Vektor(30f * scale.X, 10f * scale.Y);
            Vektor mausPos = Vektor.Zero;
            if (mousePos.HasValue)
            {
                mausPos = new Vektor(mousePos.Value.X, mousePos.Value.Y);
            }
            Gemark mouseover = null;
            // Draw the map with culling
            foreach (var province in provinzen.Values)
            {
                var posP = province.GetMapPosition(scale); // aktualisiert die MapSize - Reihenfolge wichtig
                posP.X += offset.X;
                posP.Y += offset.Y;
                if (posP.X > visibleScreen.Right || posP.Y > visibleScreen.Bottom)
                    continue;
                var sizeP = province.GetMapSize();
                if (posP.X + sizeP.X < visibleScreen.Left|| posP.Y + sizeP.Y + 50 < visibleScreen.Top)
                    continue;
                // var sizeP = province.GetMapSize();
                foreach (var gemark in province.Felder.Values)
                {
                    var posG = gemark.GetMapPosition(posP, scale); // aktualisiert die MapSize - Reihenfolge wichtig
                    var sizeG = Gemark.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = isMoving == false && mouseover == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        mouseover = gemark;                  

                    var listTexture = gemark.GetTextures(0);
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, posP, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, rScreenG, null, inKleinfeld ? Color.Plum : Color.White);
                    }
                    
                }
            }
            foreach (var province in provinzen.Values)
            {
                var posP = province.GetMapPosition(scale); // aktualisiert die MapSize - Reihenfolge wichtig
                posP.X += offset.X;
                posP.Y += offset.Y;
                if (posP.X > visibleScreen.Right || posP.Y > visibleScreen.Bottom)
                    continue;
                var sizeP = province.GetMapSize();
                if (posP.X + sizeP.X < visibleScreen.Left || posP.Y + sizeP.Y + 50 < visibleScreen.Top)
                    continue;
                // var sizeP = province.GetMapSize();
                foreach (var gemark in province.Felder.Values)
                {
                    var posG = gemark.GetMapPosition(posP, scale); // aktualisiert die MapSize - Reihenfolge wichtig
                    var sizeG = Gemark.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = isMoving == false && mouseover == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        mouseover = gemark;

                    var listTexture = gemark.GetTextures(1);
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, posP, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, rScreenG, null, inKleinfeld ? Color.Plum : Color.White);
                    }

                    if (ShowReichOverlay == true && gemark.ReichID > 0 && gemark.Reich != null)
                    {
                        spriteBatch.Draw(_weiss, rScreenG, null, inKleinfeld ? Color.Plum : gemark.Reich.color * 0.5f);
                    }
                    if (gemark == selected)
                    {
                        if (_toggle)
                            spriteBatch.Draw(_selection1, rScreenG, null, Color.White);
                        else
                            spriteBatch.Draw(_selection2, rScreenG, null, Color.White);
                    }
                }
            }
            spriteBatch.End();
            return mouseover;
        }
    }
}
