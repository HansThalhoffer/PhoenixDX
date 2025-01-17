using PhoenixDX.Drawing;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Stellt eine Straße dar, die als Richtungsschmuckelement verwendet wird.
    /// </summary>
    internal class Strasse : DirectionAdorner {
        /// <summary>
        /// Die Textur, die für die Straße verwendet wird.
        /// </summary>
        public static Drawing.DirectionTexture Texture = new("strasse_");

        /// <summary>
        /// Gibt die Richtungstextur für die Straße zurück.
        /// </summary>
        /// <returns>Die Richtungstextur der Straße.</returns>
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

        /// <summary>
        /// Erstellt eine neue Instanz einer Straße mit den angegebenen Richtungen.
        /// </summary>
        /// <param name="gem">Das Datenmodell, das die Straßenrichtung definiert.</param>
        public Strasse(PhoenixModel.dbErkenfara.KleinFeld gem)
            : base(gem.Strasse_NW, gem.Strasse_NO, gem.Strasse_O, gem.Strasse_SO, gem.Strasse_SW, gem.Strasse_W) { }
    }
}
