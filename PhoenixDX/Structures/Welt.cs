using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Welt
    {
        Dictionary<int, Provinz> Provinzen {  get; set; }

        public Welt(Dictionary<string, Gemark> map) 
        {
            Provinzen = new Dictionary<int, Provinz>();

            foreach (Gemark gem in map.Values)
            {
                if (gem != null && gem.gf > 0 && gem.kf > 0)
                {
                    if (gem.gf == null)
                        continue;
                    var p = GetProvinz((int) gem.gf);
                    if (p != null)
                    {
                        var k = p.GetPKleinfeld((int) gem.kf);
                        try
                        {
                            k.Initialize(gem);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        public void Draw(SpriteBatch spriteBatch, float scaleX, float scaleY)
        {           
            spriteBatch.Begin();

            Vector2 pos = new Vector2(0, 0);
            Vector2 size = new Vector2((float) (138.0 / 5.0)* scaleX, (float)(160.0 / 5.0)* scaleY);
            // Draw the map with culling
            foreach (var province in Provinzen.Values)
            {
                foreach (var gemark in province.Felder.Values)
                {
                    pos.X += (float)10*scaleX;
                    pos.Y += (float)10 * scaleY;

                    var listTexture = gemark.GetTextures();
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, pos, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, pos, null, Color.White, 0f, size , 1f, SpriteEffects.None, 0f);
                    }
                }
            }
            spriteBatch.End();
        }




        public Provinz? GetProvinz(int gf)
        {
            if (Provinzen.ContainsKey(gf)) 
                return Provinzen[gf];
            var p = new Provinz(gf);
            Provinzen.Add(gf, p);
            return p;
        }
    
    }
}
