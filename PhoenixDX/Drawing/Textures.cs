using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixModel.ViewModel;

namespace PhoenixDX.Drawing {
    /// <summary>
    /// Definiert die Basisklasse für Texturen.
    /// Diese Klasse erzwingt eine Schnittstelle für die Ableitungen.
    /// </summary>
    internal abstract class BaseTexture {
        /// <summary>
        /// Gibt die zugehörige Texture2D zurück.
        /// </summary>
        public abstract Texture2D Texture { get; }

        /// <summary>
        /// invertiert die Farben einer Textur
        /// </summary>
        /// <param name="originalTexture"></param>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        public static Texture2D InvertTexture(Texture2D originalTexture, GraphicsDevice graphicsDevice) {
            // 1. Get the pixel data from the original texture
            Color[] originalPixels = new Color[originalTexture.Width * originalTexture.Height];
            originalTexture.GetData(originalPixels);
            // 2. Create an array to hold the inverted pixels
            Color[] invertedPixels = new Color[originalPixels.Length];

            for (int i = 0; i < originalPixels.Length; i++) {
                Color pixel = originalPixels[i];

                // Only invert if the pixel is not fully transparent, 
                // or invert everything - depending on your use case
                if (pixel.A > 0) {
                    // Preserve alpha
                    invertedPixels[i] = new Color((byte)(255 - pixel.R), (byte)(255 - pixel.G), (byte)(255 - pixel.B), pixel.A);
                }
                else {
                    // Keep fully transparent pixels as-is
                    invertedPixels[i] = pixel;
                }
            }

            // 3. Create a new texture (you could overwrite the original if you want)
            Texture2D invertedTexture = new Texture2D(graphicsDevice, originalTexture.Width, originalTexture.Height);

            // 4. Set the inverted pixel data
            invertedTexture.SetData(invertedPixels);

            return invertedTexture;
        }

        /// <summary>
        /// invertiert die Farben einer Textur
        /// </summary>
        /// <param name="originalTexture"></param>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        public static Texture2D ColorTexture(Texture2D originalTexture, GraphicsDevice graphicsDevice, Color replaceGrey, bool replaceWhite) {
            // 1. Get the pixel data from the original texture
            Color[] originalPixels = new Color[originalTexture.Width * originalTexture.Height];
            originalTexture.GetData(originalPixels);
            // 2. Create an array to hold the inverted pixels
            Color[] invertedPixels = new Color[originalPixels.Length];

            for (int i = 0; i < originalPixels.Length; i++) {
                Color pixel = originalPixels[i];

                //visible and all colors are equal == grey
                if (pixel.A > 200 && pixel.R == pixel.G && pixel.G == pixel.B) {
                    if (replaceWhite) {
                        // do not replace black
                        if (pixel.R == 0)
                            invertedPixels[i] = pixel;
                        else {
                            var r= replaceGrey.R + pixel.R;
                            var g = replaceGrey.G + pixel.G;
                            var b = replaceGrey.B + pixel.B;
                            if (r > 128)
                                r -= 128;
                            if (g > 128)
                                g -= 128;
                            if (b > 128)
                                b -= 128;
                            invertedPixels[i] = new Color(r, g, b, pixel.A);
                        }
                    }
                    else
                        invertedPixels[i] = new Color(replaceGrey.R + pixel.R, replaceGrey.G + pixel.G, replaceGrey.B + pixel.B, pixel.A);
                }
                else {
                    invertedPixels[i] = pixel;
                }
            }

            // 3. Create a new texture (you could overwrite the original if you want)
            Texture2D invertedTexture = new Texture2D(graphicsDevice, originalTexture.Width, originalTexture.Height);

            // 4. Set the inverted pixel data
            invertedTexture.SetData(invertedPixels);

            return invertedTexture;
        }

    }

    /// <summary>
    /// Repräsentiert eine einfache Textur.
    /// </summary>
    internal class SimpleTexture : BaseTexture {
        private Texture2D _texture;

        /// <summary>
        /// Erstellt eine neue Instanz von SimpleTexture.
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        public SimpleTexture(Texture2D texture) {
            _texture = texture;
        }

        /// <summary>
        /// Gibt die Texture2D dieser Instanz zurück.
        /// </summary>
        public override Texture2D Texture { get { return _texture; } }
    }

    /// <summary>
    /// Repräsentiert eine farbige Textur, die eine zusätzliche Farbkomponente unterstützt.
    /// Damit können neutrale Texturen farbig dargestellt werden.
    /// </summary>
    internal class ColoredTexture : SimpleTexture {
        /// <summary>
        /// Die Farbe, die auf die Textur angewendet wird.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Erstellt eine neue Instanz von ColoredTexture mit einer spezifischen Farbe.
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        /// <param name="color">Die gewünschte Farbe.</param>
        public ColoredTexture(Texture2D texture, Color color) : base(texture) {
            this.Color = color;
        }

        /// <summary>
        /// Erstellt eine neue Instanz von ColoredTexture mit der Standardfarbe Weiß.
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        public ColoredTexture(Texture2D texture) : base(texture) {
            this.Color = Color.White;
        }

        /// <summary>
        /// ist das eine dunkle Textur?
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        public bool IsDark {
            get { return IsDarkColor(this.Color); }
        }

        /// <summary>
        /// findet raus, ob die Farbe dunkel ist
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsDarkColor(Color color) {
            // Calculate a weighted luminosity that approximates human perception:
            // (These coefficients come from the Rec. 601 luma formula, often used as a quick approximation)
            float luminosity = (0.299f * color.R) + (0.587f * color.G) + (0.114f * color.B);

            // Compare to a midpoint (128) out of 255
            // If it's less than 128, we consider it a "dark" color
            return luminosity < 96f;
        }
    }

    /// <summary>
    /// Repräsentiert eine farbige Textur, die eine zusätzliche Farbkomponente unterstützt.
    /// Damit können neutrale Texturen farbig dargestellt werden.
    /// </summary>
    internal class OpacityTexture : SimpleTexture {
        /// <summary>
        /// Die Farbe, die auf die Textur angewendet wird.
        /// </summary>
        public float Opacity { get; set; }

        /// <summary>
        /// Erstellt eine neue Instanz von OpacityedTexture mit einer spezifischen Farbe.
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        /// <param name="Opacity">Die gewünschte Farbe.</param>
        public OpacityTexture(Texture2D texture, float opacity) : base(texture) {
            this.Opacity = opacity;
        }

        /// <summary>
        /// Erstellt eine neue Instanz von OpacityedTexture mit der Standardfarbe Weiß.
        /// </summary>
        /// <param name="texture">Die zu verwendende Texture2D.</param>
        public OpacityTexture(Texture2D texture) : base(texture) {
            this.Opacity = 1f;
        }

    }

    /// <summary>
    /// Verwaltet Texturen für Windrichtungsdarstellungen.
    /// Diese Klasse ist nicht von BaseTexture abgeleitet, da sie eine andere Funktionalität besitzt.
    /// </summary>
    public class DirectionTexture {
        /// <summary>
        /// Der Präfix der Bildnamen, die für Richtungs-Texturen verwendet werden.
        /// </summary>
        public string ImageStartsWith = string.Empty;

        /// <summary>
        /// Der Index für die erste verwendete Textur.
        /// </summary>
        public int IndexStartsWith = 0;

        /// <summary>
        /// Erstellt eine neue Instanz von DirectionTexture.
        /// </summary>
        /// <param name="imageStartsWith">Der Präfix für die Bildnamen.</param>
        public DirectionTexture(string imageStartsWith) {
            ImageStartsWith = imageStartsWith;
        }

        private Texture2D[] _hexTexture;

        /// <summary>
        /// Setzt die Texturen für verschiedene Richtungen.
        /// </summary>
        /// <param name="texture">Ein Array von Texture2D-Objekten.</param>
        public void SetTextures(Texture2D[] texture) {
            _hexTexture = texture;
        }

        /// <summary>
        /// Gibt die entsprechende Textur für eine gegebene Richtung zurück.
        /// </summary>
        /// <param name="dir">Die gewünschte Richtung.</param>
        /// <returns>Die zugehörige Texture2D.</returns>
        public Texture2D GetTexture(Direction dir) {
            return _hexTexture[(int)dir];
        }
    }
}
