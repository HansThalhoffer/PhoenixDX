using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Program;
using PhoenixDX.Structures;

namespace PhoenixModel.Helper
{
    internal class TextureCache: Dictionary<string, BaseTexture>
    {
        private static TextureCache _instance = [];
        public static TextureCache Instance => _instance;
     
        public static bool Contains(string key)
        {
            return _instance.ContainsKey(key);
        }

        public static BaseTexture Get(string key)
        {
             return _instance[key];
        }

        public static void Set(string key, Texture2D item, Color farbe)
        {
            _instance[key] = new ColoredTexture(item, farbe);
        }

        public static void Set(string key, Texture2D item)
        {
            _instance[key] = new SimpleTexture(item);
        }

        public static void Set(string key, BaseTexture item)
        {
            _instance[key] = item;
        }

        /// <summary>
        /// Merges a list of textures into a single texture.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice used for rendering.</param>
        /// <param name="textures">The list of Texture2D to be merged.</param>
        /// <returns>A new Texture2D containing the merged content of the input textures.</returns>
        public static SimpleTexture MergeTextures(List<Texture2D> textures)
        {
            if (SpielDX.Instance.Graphics == null || textures == null || textures.Count == 0)
                return null;

            var graphicsDevice = SpielDX.Instance.Graphics.GraphicsDevice;
            int width = textures[0].Width;
            int height = textures[0].Height;
            // Create a new RenderTarget2D where we will draw all the layers
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, width, height);

            // Set the RenderTarget for the GraphicsDevice
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent); // Clear the render target to fully transparent

            // Create a new SpriteBatch to render the textures
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();

            Rectangle rScreenG = new Rectangle(0, 0, width, height);
            // Draw each texture in the list on top of each other
            foreach (var texture in textures)
            {
                spriteBatch.Draw(texture, rScreenG, null, Color.White);
            }
            spriteBatch.End();

            // Unset the render target (set it back to the main screen buffer)
            graphicsDevice.SetRenderTarget(null);

            // Get the final merged texture from the render target
            Texture2D mergedTexture = new Texture2D(graphicsDevice, width, height);

            // Copy the data from the render target to the final texture
            Color[] data = new Color[width * height];
            renderTarget.GetData(data);
            mergedTexture.SetData(data);

            // Dispose of the render target as it is no longer needed
            renderTarget.Dispose();

            return new SimpleTexture(mergedTexture);
        }

        /// <summary>
        /// Converts a MemoryStream (PNG image) to a Texture2D for MonoGame.
        /// </summary>
        /// <param name="graphicsDevice">The MonoGame GraphicsDevice instance.</param>
        /// <param name="stream">The MemoryStream containing the image data.</param>
        /// <returns>A Texture2D that can be used in MonoGame.</returns>
        public static Texture2D MemoryStreamToTexture2D(GraphicsDevice graphicsDevice, MemoryStream stream)
        {
            // Load the image data into a Texture2D
            Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
            return texture;
        }

    }
}