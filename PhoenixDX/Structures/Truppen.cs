﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Program;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static PhoenixDX.Structures.Truppen;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Die Klasse Truppen stellt eine Gruppe von Spielfiguren dar, die als Adorner gezeichnet werden können.
    /// </summary>
    internal class Truppen : SimpleAdorner {
        /// <summary>
        /// Repräsentiert eine Bildreferenz für eine bestimmte Figur.
        /// </summary>
        public class FigurImage
        {
            public FigurType Typ = FigurType.None;
            public string FileName = string.Empty;
            public Texture2D Texture = null;
            public Texture2D InvertedTexture = null;
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

        public static Texture2D GetTexture2D(FigurType type) {
            var image = FigurImages.Where(f => f.Typ == type).FirstOrDefault();
            if (image != null) 
                return image.Texture;

            return null;    
        }

        /// <summary>
        /// Repräsentiert eine einzelne Figur mit einem bestimmten Typ und einer Farbe.
        /// </summary>
        public class Figur
        {
            public FigurType Typ = FigurType.None;
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
            var graphicsDevice = SpielDX.Instance.Graphics.GraphicsDevice;
            foreach (var item in FigurImages)
            {
                string f = $"Images/Symbol/{item.FileName}";
                try {
                    item.Texture = contentManager.Load<Texture2D>(f);
                    f = $"Images/Symbol/i{item.FileName}";
                    item.InvertedTexture = contentManager.Load<Texture2D>(f);
                }
                catch (Exception ex) {
                    MappaMundi.Log($"Die Datei {f} wurde nicht gefunden", ex);
                }
                //item.InvertedTexture = BaseTexture.InvertTexture(item.Texture, graphicsDevice);
            }
        }
        /// <summary>
        /// Erstellt eine Textur für die Truppen.
        /// </summary>
        /// <returns>Die erstellte Textur.</returns>
        public override SimpleTexture CreateTexture()
        {
            return CreateTexture(_truppen);
        }

        /// <summary> 
        /// hier werden die Figuren in eine Texture zusammengefügt
        /// </summary>
        /// <returns>Die erstellte Textur.</returns>
        private static SimpleTexture CreateTexture(List<Figur> truppen)
        {
            if (SpielDX.Instance.Graphics == null || truppen.Count == 0)
                return null;

            // der key für den Cache ist Farbe und dann die Typen
            string cacheKey = $"Truppen:{truppen.Count}";
            foreach (var figur in truppen)
            {
                cacheKey += $" {truppen[0].Color.PackedValue}|{figur.Typ.ToString()}";
            }

            if (TextureCache.TryGet(cacheKey, out BaseTexture baseTexture))
                return baseTexture as SimpleTexture;

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
                new Position(70, 0),
                new Position(140, 70),
                new Position(70, 140),
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

                    bool isDark = ColoredTexture.IsDarkColor(truppen[0].Color);
                    bool isBlack = truppen[0].Color.R <= 10 && truppen[0].Color.G <= 10 && truppen[0].Color.B <= 0;

                    // Draw each texture in the list on top of each other
                    int i = 0;
                    foreach (var figur in truppen)
                    {
                        int index = (int)figur.Typ;
                        var texture = isDark? FigurImages[index].InvertedTexture: FigurImages[index].Texture;
                        string colorKey = $"Figur: {figur.Color.PackedValue}|{figur.Typ}";
                        if (TextureCache.TryGet(colorKey, out baseTexture))
                            texture = baseTexture.Texture;
                        else {
                            texture = ColoredTexture.ColorTexture(texture, graphicsDevice, figur.Color, !isDark);
                            TextureCache.Set(colorKey, texture);
                        }
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
                    var gameTexture = new SimpleTexture(result); 
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
