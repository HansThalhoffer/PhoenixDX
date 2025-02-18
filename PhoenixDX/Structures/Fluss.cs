/// <summary>
/// Repräsentiert einen Fluss als eine Richtungskomponente.
/// </summary>
using PhoenixDX.Drawing;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Die Klasse <c>Fluss</c> erweitert die <see cref="DirectionAdorner"/> Klasse,
    /// um einen Fluss mit bestimmten Richtungen darzustellen.
    /// </summary>
    internal class Fluss : DirectionAdorner {
        /// <summary>
        /// Die Textur für den Fluss.
        /// </summary>
        public static Drawing.DirectionTexture Texture = new("fluss_");

        /// <summary>
        /// Gibt die Richtungstextur für den Fluss zurück.
        /// </summary>
        /// <returns>Die Richtungstextur des Flusses.</returns>
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }
        protected override Drawing.DirectionTexture GetBuildDirectionTexture() { return Texture; }

        /// <summary>
        /// Erstellt eine neue Instanz der <c>Fluss</c>-Klasse.
        /// </summary>
        /// <param name="gem">Das zugehörige KleinFeld-Objekt mit Flussrichtungen.</param>
        public Fluss(PhoenixModel.dbErkenfara.KleinFeld gem)
            : base(gem.Fluss_NW, gem.Fluss_NO, gem.Fluss_O, gem.Fluss_SO, gem.Fluss_SW, gem.Fluss_W) { }
    }
}
