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
                        k.Initialize(gem);
                    }
                }
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition, Matrix transformMatrix)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(transformMatrix));
        }

        

        public void Draw(GraphicsDevice graphics, Matrix transformMatrix, SpriteBatch spriteBatch)
        {
           
            spriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);

            // Calculate visible area
            Viewport viewport = graphics.Viewport;
            Vector2 topLeft = ScreenToWorld(Vector2.Zero, transformMatrix);
            Vector2 bottomRight = ScreenToWorld(new Vector2(viewport.Width, viewport.Height), transformMatrix);

            RectangleF visibleArea = new RectangleF(topLeft.X - Kleinfeld.Width, topLeft.Y - Kleinfeld.Height,
                                                    bottomRight.X - topLeft.X + Kleinfeld.Width * 2,
                                                    bottomRight.Y - topLeft.Y + Kleinfeld.Height * 2);

            // Draw the map with culling
            foreach (var province in Provinzen.Values)
            {
                foreach (var gemark in province.Felder.Values)
                {
                    if (visibleArea.Contains(gemark.Position))
                    {
                        Color color = terrainColors[gemark.Terrain];

                        spriteBatch.Draw(hexTexture, gemark.Position, null, color, 0f, new Vector2(hexTexture.Width / 2, hexTexture.Height / 2), 1f, SpriteEffects.None, 0f);
                    }
                }
            }
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
