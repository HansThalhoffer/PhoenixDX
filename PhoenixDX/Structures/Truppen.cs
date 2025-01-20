using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Program;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using static PhoenixDX.Structures.Truppen;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Die Klasse Truppen stellt eine Gruppe von Spielfiguren dar, die als Adorner gezeichnet werden können.
    /// </summary>
    internal class Truppen : ColorAdorner
    {
        /// <summary>
        /// Repräsentiert eine Bildreferenz für eine bestimmte Figur.
        /// </summary>
        public class FigurImage
        {
            public FigurType Typ = FigurType.NaN;
            public string FileName = string.Empty;
            public Texture2D Texture = null;
            public int Index = 0;
            public FigurImage(int index, FigurType typ, string fileName)
            {
                FileName = fileName;
                Typ = typ;
                Index = index;
            }
        }
        /// <summary>
        /// Liste der verfügbaren Figurenbilder.
        /// </summary>
        public static readonly List<FigurImage> FigurImages =
        [
            new FigurImage(0, FigurType.Kreatur, "Monster"),
            new FigurImage(1, FigurType.Krieger, "Krieger"),
            new FigurImage(2, FigurType.Reiter, "Reiter"),
            new FigurImage(4, FigurType.Charakter, "Charakter"),
            new FigurImage(5, FigurType.Zauberer, "Zauberer"),
            new FigurImage(6, FigurType.SchwereArtillerie, "SchweresKatapult"),
            new FigurImage(7, FigurType.LeichteArtillerie, "LeichtesKatapult"),
            new FigurImage(8, FigurType.BeritteneSchwereArtillerie, "ReiterSchweresKatapult"),
            new FigurImage(9, FigurType.BeritteneLeichteArtillerie, "ReiterLeichtesKatapult"),
            new FigurImage(10, FigurType.Schiff, "Schiff"),
            new FigurImage(11, FigurType.SchweresKriegsschiff, "SchweresKriegsschiff"),
            new FigurImage(12, FigurType.LeichtesKriegsschiff, "LeichtesKriegsschiff"),
            new FigurImage(13, FigurType.PiratenSchiff, "PiratenSchiff"),
            new FigurImage(14, FigurType.PiratenSchweresKriegsschiff, "PiratenSchweresKriegsschiff"),
            new FigurImage(15, FigurType.PiratenLeichtesKriegsschiff, "PiratenLeichtesKriegsschiff"),
            new FigurImage(16, FigurType.CharakterZauberer, "CharakterZauberer")
        ];

        /// <summary>
        /// Repräsentiert eine einzelne Figur mit einem bestimmten Typ und einer Farbe.
        /// </summary>
        public class Figur
        {
            public FigurType Typ = FigurType.NaN;
            public Color Color = Color.White;
            public Figur(FigurType typ, Color color)
            {
                Color = color;
                Typ = typ;
            }
        }

        /// <summary>
        /// anwesende Truppen
        /// </summary>
        List<Figur> _truppen = null;

        public Truppen(List<Figur> truppen)
        {
            _truppen = truppen;
        }
        /// <summary>
        /// Lädt die Inhalte für alle Figurenbilder.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager zum Laden der Texturen.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            foreach (var item in FigurImages)
            {
                item.Texture = contentManager.Load<Texture2D>($"Images/Symbol/{item.FileName}");
            }
        }
        /// <summary>
        /// Erstellt eine Textur für die Truppen.
        /// </summary>
        /// <returns>Die erstellte Textur.</returns>
        public override ColoredTexture CreateTexture()
        {
            return CreateTexture(_truppen);
        }

        /// <summary> 
        /// hier werden die Figuren in eine Texture zusammengefügt
        /// </summary>
        /// <returns>Die erstellte Textur.</returns>
        private static ColoredTexture CreateTexture(List<Figur> truppen)
        {
            if (SpielDX.Instance.Graphics == null || truppen.Count == 0)
                return null;

            // der key für den Cache ist Farbe und dann die Typen
            string cacheKey = truppen[0].Color.ToString();
            foreach (var figur in truppen)
            {
                cacheKey += $"{figur.Typ.ToString()}, ";
            }

            if (TextureCache.Contains(cacheKey))
                return TextureCache.Get(cacheKey) as ColoredTexture;

            var graphicsDevice = SpielDX.Instance.Graphics.GraphicsDevice;
            float faktor = truppen.Count > 1 ? 1.2f :0.8f;
            int figurHeight = Convert.ToInt32(719f / faktor);
            int figurWidth = Convert.ToInt32(670f / faktor);
            const int height = 138 *2;
            const int width = 160 *2;

            Position[] pos = [
                new Position(truppen.Count > 1 ? 4:40, truppen.Count > 1 ? -8:20),
                new Position(140, 0),
                new Position(-8, 140),
                new Position(140, 140),
                new Position(70, 70),
            ];

            try
            {
                // Create a new RenderTarget2D where we will draw all the layers
                using (RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, width, height))
                {

                    // Set the RenderTarget for the GraphicsDevice
                    graphicsDevice.SetRenderTarget(renderTarget);
                    graphicsDevice.Clear(Color.Transparent); // Clear the render target to fully transparent

                    // Create a new SpriteBatch to render the textures
                    SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                    spriteBatch.Begin();

                    // Draw each texture in the list on top of each other
                    int i = 0;
                    foreach (var figur in truppen)
                    {
                        int index = (int)figur.Typ;
                        var texture = FigurImages[index].Texture;
                        Rectangle rScreenG = new Rectangle(pos[i].X, pos[i].Y, Convert.ToInt32(figurWidth / 4), Convert.ToInt32(figurHeight /4));
                        spriteBatch.Draw(texture, rScreenG, null, Color.White); // figur.Color);
                        if (++i > pos.Length - 1)
                            i = 0;
                    }
                    spriteBatch.End();

                    // Unset the render target (set it back to the main screen buffer)
                    graphicsDevice.SetRenderTarget(null);

                    // Get the final merged texture from the render target
                    Texture2D result = new Texture2D(graphicsDevice, width, height);

                    // Copy the data from the render target to the final texture
                    Color[] data = new Color[width * height];
                    renderTarget.GetData(data);
                    result.SetData(data);
                    ColoredTexture gameTexture = new ColoredTexture(result, truppen[0].Color);
                    TextureCache.Set(cacheKey, gameTexture);
                    return gameTexture;
                }
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, $"Bei der Erstellung der Textur für die Truppen kam es zu einem Fehler",ex);
            }
            return null;
        }

        
    }
}
