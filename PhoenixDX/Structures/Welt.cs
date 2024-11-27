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

        public void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
           
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Vector2 pos = new Vector2(0, 0);
            // Draw the map with culling
            foreach (var province in Provinzen.Values)
            {
                foreach (var gemark in province.Felder.Values)
                {
                    pos.X += 10;
                    pos.Y += 10;

                    var listTexture = gemark.GetTextures();
                    foreach (var hexTexture in listTexture)
                    {
                        spriteBatch.Draw(hexTexture, pos, null, Color.Transparent); 
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
