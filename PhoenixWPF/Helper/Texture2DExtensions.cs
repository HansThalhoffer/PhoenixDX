using Microsoft.Xna.Framework.Graphics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = Microsoft.Xna.Framework.Color;

namespace PhoenixDX.Helper {
    public static class Texture2DExtensions {
        public static BitmapSource ToBitmapSource(this Texture2D texture) {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            int width = texture.Width;
            int height = texture.Height;
            Color[] textureData = new Color[width * height];

            // Get the pixel data
            texture.GetData(textureData);

            // Convert to byte array (BGRA format for WPF)
            byte[] pixelData = new byte[width * height * 4];

            for (int i = 0; i < textureData.Length; i++) {
                Color color = textureData[i];
                int index = i * 4;
                pixelData[index] = color.B;        // Blue
                pixelData[index + 1] = color.G;    // Green
                pixelData[index + 2] = color.R;    // Red
                pixelData[index + 3] = color.A;    // Alpha (Transparency)
            }

            // Create a WritableBitmap
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            bitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, width, height),
                pixelData,
                width * 4,  // Stride (bytes per row)
                0
            );

            return bitmap;
        }
    }
}
