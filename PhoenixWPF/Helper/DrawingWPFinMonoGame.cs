using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoenixModel.Helper
{
    public class DrawingWPFinMonoGame
    { /// <summary>
      /// Renders a DrawingBrush to a Bitmap and returns it as a MemoryStream.
      /// </summary>
      /// <param name="drawingBrush">The WPF DrawingBrush to render.</param>
      /// <param name="width">The width of the output image.</param>
      /// <param name="height">The height of the output image.</param>
      /// <returns>MemoryStream containing the image data as PNG.</returns>
        public static MemoryStream RenderDrawingBrushToMemoryStream(DrawingBrush drawingBrush, int width, int height)
        {
            // Create a DrawingVisual to render the DrawingBrush
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(drawingBrush, null, new Rect(0, 0, width, height));
            }

            // Render the DrawingVisual to a RenderTargetBitmap
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            // Convert the RenderTargetBitmap to a PNG stream
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin); // Reset stream position for reading
            return stream;
        }
    }
}
