using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Program;
using System.Collections.Generic;
using System.IO;

namespace PhoenixModel.Helper {
    /// <summary>
    /// Eine Cache-Klasse zur Verwaltung von Texturen.
    /// </summary>
    internal class TextureCache : Dictionary<string, BaseTexture> {
        private static TextureCache _instance = [];
        public static TextureCache Instance => _instance;

        /// <summary>
        /// Überprüft, ob eine Textur mit dem angegebenen Schlüssel existiert.
        /// </summary>
        /// <param name="key">Der Schlüssel der zu überprüfenden Textur.</param>
        /// <returns>True, wenn die Textur existiert, andernfalls false.</returns>
        public static bool Contains(string key) {
            return _instance.ContainsKey(key);
        }

        /// <summary>
        /// Ruft eine Textur aus dem Cache ab.
        /// </summary>
        /// <param name="key">Der Schlüssel der abzurufenden Textur.</param>
        /// <returns>Die entsprechende Textur.</returns>
        public static BaseTexture Get(string key) {
            return _instance[key];
        }

        /// <summary>
        /// Fügt eine farbige Textur in den Cache ein.
        /// </summary>
        /// <param name="key">Der Schlüssel der Textur.</param>
        /// <param name="item">Die Textur, die gespeichert werden soll.</param>
        /// <param name="farbe">Die zu verwendende Farbe.</param>
        public static void Set(string key, Texture2D item, Color farbe) {
            _instance[key] = new ColoredTexture(item, farbe);
        }

        /// <summary>
        /// Fügt eine einfache Textur in den Cache ein.
        /// </summary>
        /// <param name="key">Der Schlüssel der Textur.</param>
        /// <param name="item">Die Textur, die gespeichert werden soll.</param>
        public static void Set(string key, Texture2D item) {
            _instance[key] = new SimpleTexture(item);
        }

        /// <summary>
        /// Fügt eine Basistextur in den Cache ein.
        /// </summary>
        /// <param name="key">Der Schlüssel der Textur.</param>
        /// <param name="item">Die Textur, die gespeichert werden soll.</param>
        public static void Set(string key, BaseTexture item) {
            _instance[key] = item;
        }

        /// <summary>
        /// Verbindet eine Liste von Texturen zu einer einzigen Textur.
        /// </summary>
        /// <param name="graphicsDevice">Das GraphicsDevice für das Rendering.</param>
        /// <param name="textures">Die Liste der zu kombinierenden Texturen.</param>
        /// <returns>Eine neue Texture2D mit dem kombinierten Inhalt der Eingabetexturen.</returns>
        public static SimpleTexture MergeTextures(List<Texture2D> textures) {
            if (SpielDX.Instance.Graphics == null || textures == null || textures.Count == 0)
                return null;

            var graphicsDevice = SpielDX.Instance.Graphics.GraphicsDevice;
            int width = textures[0].Width;
            int height = textures[0].Height;
            // Erstellen eines neuen RenderTarget2D zum Zeichnen der Layer
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, width, height);

            // Setzen des RenderTargets für das GraphicsDevice
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent); // Löscht das RenderTarget und setzt es auf transparent

            // Erstellen eines neuen SpriteBatch zur Darstellung der Texturen
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();

            Rectangle rScreenG = new Rectangle(0, 0, width, height);
            // Zeichnen jeder Textur in der Liste übereinander
            foreach (var texture in textures) {
                spriteBatch.Draw(texture, rScreenG, null, Color.White);
            }
            spriteBatch.End();

            // Zurücksetzen des RenderTargets auf den Hauptbildschirm
            graphicsDevice.SetRenderTarget(null);

            // Extrahieren der endgültigen kombinierten Textur aus dem RenderTarget
            Texture2D mergedTexture = new Texture2D(graphicsDevice, width, height);

            // Kopieren der Daten aus dem RenderTarget in die endgültige Textur
            Color[] data = new Color[width * height];
            renderTarget.GetData(data);
            mergedTexture.SetData(data);

            // Freigeben des RenderTargets, da es nicht mehr benötigt wird
            renderTarget.Dispose();

            return new SimpleTexture(mergedTexture);
        }

        /// <summary>
        /// Konvertiert einen MemoryStream (PNG-Bild) in eine Texture2D für MonoGame.
        /// </summary>
        /// <param name="graphicsDevice">Die Instanz des MonoGame GraphicsDevice.</param>
        /// <param name="stream">Der MemoryStream mit den Bilddaten.</param>
        /// <returns>Eine Texture2D, die in MonoGame verwendet werden kann.</returns>
        public static Texture2D MemoryStreamToTexture2D(GraphicsDevice graphicsDevice, MemoryStream stream) {
            // Laden der Bilddaten in eine Texture2D
            Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
            return texture;
        }
    }
}
