using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Structures;

namespace PhoenixModel.Helper
{
    internal class TextureCache
    {

        /// <summary>
        /// Merges a list of textures into a single texture.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice used for rendering.</param>
        /// <param name="textures">The list of Texture2D to be merged.</param>
        /// <returns>A new Texture2D containing the merged content of the input textures.</returns>
        public static Texture2D MergeTextures(GraphicsDevice graphicsDevice, List<Texture2D> textures, int width, int height)
        {
            if (textures == null || textures.Count == 0)
                throw new System.ArgumentException("The textures list must not be null or empty.");

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

            return mergedTexture;
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