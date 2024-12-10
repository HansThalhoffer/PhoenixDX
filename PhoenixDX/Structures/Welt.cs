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
       
        Dictionary<int, Provinz> Provinzen {  get; set; } = new Dictionary<int, Provinz>();

        

        public Welt(Dictionary<string, Gemark> map) 
        {
            Provinzen.Add(701, new Provinz(7, 0, 701));
            Provinzen.Add(901, new Provinz(9, 0, 901));
            Provinzen.Add(712, new Provinz(7, 11, 712));
            Provinzen.Add(912, new Provinz(9, 11, 912));
            Provinzen.Add(6666, new Provinz(15, 11, 6666));
            Provinzen.Add(2001, new Provinz(1, 1, 2001));
            Provinzen.Add(4001, new Provinz(15, 1, 4001));

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
                    int xPos = x; 
                    if (x < 6 )
                    {
                        yPos += 3 - x / 2 - x%2;
                    }
                    if (x > 6 && x <10)
                    {
                        yPos -= x %2;
                    }
                    if (x > 10)
                    {
                        yPos +=  x / 2 -5;
                    }
                    if (Provinzen.ContainsKey(gf) == false)
                        Provinzen.Add(gf, new Provinz(xPos,yPos, gf));
                }
            }

            foreach (Gemark gem in map.Values)
            {
                if (gem != null && gem.gf > 0 && gem.kf > 0 && gem.kf < 50)
                {
                    if (Provinzen.ContainsKey(gem.gf) == false)
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
            }
        }

        public Gemark FindGemarkByPosition(Vector2 position)
        {
            return null;
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            // Provinz.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        public float TileTransparancy { get; set; } = 1f;

        float _previousScaleX = 0f;
        float _previousScaleY = 0f;
        int _trashCount = 0;
        public Kleinfeld? Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, float scaleX, float scaleY, Vector2? mousePos)
        {
            if (_previousScaleX != 0 && _trashCount < 2 && Math.Abs(scaleX - _previousScaleX) > 0.2f)
            {
                ++_trashCount;
            }
            _previousScaleX = scaleX;
            _previousScaleY = scaleY;
            Color colorTiles = Color.White* TileTransparancy;

            SpriteFont font = FontManager.Fonts["Small"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Vector2 offset = new Vector2(30f*scaleX, 10f*scaleY);
            Vector2 mausPos = Vector2.Zero;
            if (mousePos.HasValue)
            {
                mausPos = new Vector2(mousePos.Value.X, mousePos.Value.Y);
            }
            Kleinfeld? selected = null;

            // Draw the map with culling
            foreach (var province in Provinzen.Values)
            {
                
                var posP = province.GetMapPosition(scaleX, scaleY); // aktualisiert die MapSize - Reihenfolge wichtig
                posP.X += offset.X;
                posP.Y += offset.Y; 
                // var sizeP = province.GetMapSize();
                foreach (var gemark in province.Felder.Values)
                {
                    var posG = gemark.GetMapPosition(posP,scaleX, scaleY); // aktualisiert die MapSize - Reihenfolge wichtig
                    var sizeG= Kleinfeld.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = selected == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        selected = gemark;

                    var listTexture = gemark.GetTextures();
                    foreach (var hexTexture in listTexture)
                    {
                        // spriteBatch.Draw(hexTexture, posP, null, Color.Transparent);
                        spriteBatch.Draw(hexTexture, rScreenG, null, inKleinfeld?Color.Plum: colorTiles);
                    }
                }
            }
            spriteBatch.End();
            return selected;
        }    
    }
}
