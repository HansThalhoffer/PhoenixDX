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
