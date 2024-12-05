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


            for (int y = 1; y <= 12; y++)
            {
                for (int x = 1; x <= 16; x++)
                {
                    GetProvinz(x,y);
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
            foreach (var reihe in Provinzen.Values)
            {
                foreach (var province in reihe.Values)
                {
                    var pos = province.GetMapPosition(scaleX, scaleY);
                    var size = province.GetMapSize(scaleX, scaleY);
                    Rectangle rScreen = new Rectangle(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y), Convert.ToInt32(size.X), Convert.ToInt32(size.Y));
                    spriteBatch.Draw(Provinz.Texture, rScreen, null, Color.White);
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
            var v = Provinz.MapToXY(gf);
            return GetProvinz(v.X,v.Y);
        }
        public Provinz GetProvinz(int x, int y)
        {
            var r = GetReihe(y);
            if (r.ContainsKey(x))
                return r[y];
            var p = new Provinz(x,y);
            r.Add(x, p);
            return p;
        }

       
    
    }
}
