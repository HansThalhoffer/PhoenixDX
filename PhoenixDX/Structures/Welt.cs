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
        class Reihe : Dictionary<int, Provinz> 
        {
            public Reihe() { }
        }
        Dictionary<int, Reihe> Provinzen {  get; set; }

        

        public Welt(Dictionary<string, Gemark> map) 
        {
            Provinzen = new Dictionary<int, Reihe>();


            for (int x = 1; x <= 20; x++)
            {
                for (int y = 1; y <= 12; y++)
                {
                    // string id = x.ToString() + y.ToString("00");
                    int id = x * 100 + y;
                    GetProvinz(id);
                   
                }
            }

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
                            if (k.Initialize(gem) ==false)
                                continue; // TODO - doppelte Kleinfelder in der Datenbank
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
            Provinz.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        public void Draw(SpriteBatch spriteBatch, float scaleX, float scaleY)
        {           
            spriteBatch.Begin();

            Vector2 pos = new Vector2(0, 0);
           
            // Draw the map with culling
            foreach (var reihe in Provinzen.Values)
            {
                foreach (var province in reihe.Values)
                {
                    pos = province.GetPosition();
                    Vector2 sizeProvinz = new Vector2((float)(province.Width) * scaleX, (float)(province.Height) * scaleY);
                    Rectangle rScreen = new Rectangle(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), Convert.ToInt32(sizeProvinz.X), Convert.ToInt32(sizeProvinz.Y));
                     spriteBatch.Draw(Provinz.Texture, rScreen, null, Color.White);

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
            }
            spriteBatch.End();
        }


        private Reihe GetReihe(int reihe)
        {
            if (Provinzen.ContainsKey(reihe))
                return Provinzen[reihe];
            var p = new Reihe();
            Provinzen.Add(reihe, p);
            return p;
        }

        public Provinz GetProvinz(int gf)
        {
            int spalte = gf / 100;
            int reihe = gf % 100;
            var r = GetReihe(reihe);

            if (r.ContainsKey(spalte)) 
                return r[spalte];
            var p = new Provinz(gf);
            r.Add(spalte, p);
            return p;
        }
    
    }
}
