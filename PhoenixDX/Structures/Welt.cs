using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Classes;
using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace PhoenixDX.Structures
{
    public class Welt
    {
       
        Dictionary<int, Provinz> Provinzen {  get; set; }

        

        public Welt(Dictionary<string, Gemark> map) 
        {
            for (int x = 1; x <= 15; x++)
            {
                for (int y = 1; y <= 11; y++)
                {
                    if ((x == 1 || x== 15) && y > 6)
                        continue;
                    if ((x == 2 || x == 14) && y > 7)
                        continue;
                    if ((x == 3 || x == 13) && y > 8)
                        continue;
                    if ((x == 4 || x == 12) && y > 9)
                        continue;
                    if ((x == 5 || x == 11) && y > 10)
                        continue;
                    int gf = x * 100 + y;
                    int yPos = y ;
                    if (x < 6 )
                    {
                        yPos += 3 - x / 2 - x%2;
                    }
                    if (x > 6 && x <10)
                    {
                        yPos += 3 - x / 2 - x % 2;
                    }

                    int xPos = x;
                    

                   

                }
            }

            /*foreach (Gemark gem in map.Values)
            {
                if (gem != null && gem.gf > 0 && gem.kf > 0)
                {
                    if (Provinzen.ContainsKey(gem.gf))
                    {
                        MessageBox.Show("Großfeld " + gem.gf + " fehlt");
                        continue;
                    }
                    var p = Provinzen[gem.gf];

                    if (p != null)
                    {
                        var k = p.GetPKleinfeld((int) gem.kf);
                        try
                        {
                            if (k.Initialize(gem) ==false)
                                continue; // TODO - doppelte Kleinfelder in der Datenbank
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message);
                        }
                    }
                }
            }*/
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Provinz.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        float _previousScaleX = 0f;
        float _previousScaleY = 0f;
        int _trashCount = 0;
        public void Draw(SpriteBatch spriteBatch, float scaleX, float scaleY)
        {
            if (_previousScaleX != 0 && _trashCount < 2 && Math.Abs(scaleX - _previousScaleX) > 0.2f)
            {
                ++_trashCount;
            }
            _previousScaleX = scaleX;
            _previousScaleY = scaleY;


            SpriteFont font = FontManager.Fonts["Small"];
            spriteBatch.Begin();

            
            // Draw the map with culling
            foreach (var province in Provinzen.Values)
            {
                
                var pos = province.GetMapPosition(scaleX, scaleY);
                var size = province.GetMapSize(scaleX, scaleY);
                Rectangle rScreen = new Rectangle(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), Convert.ToInt32(size.X), Convert.ToInt32(size.Y));
                spriteBatch.Draw(province.Texture, rScreen, null, Color.White);
                pos.Move(Convert.ToInt32(160f * scaleX), 10);
                spriteBatch.DrawString(font, province.Bezeichner, pos, Color.Black);

                /*foreach (var gemark in province.Felder.Values)
                {
                    pos.X += (float)10 * scaleX;
                    pos.Y += (float)10 * scaleY;
                    pos = gemark.Position;

                    var listTexture = gemark.GetTextures();
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, pos, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, pos, null, Color.White, 0f, size, 1f, SpriteEffects.None, 0f);
                    }
                }*/
               
            }
            spriteBatch.End();
        }    
    }
}
