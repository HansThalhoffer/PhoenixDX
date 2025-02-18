using PhoenixDX.Drawing;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert eine Mauer, die als Richtungsschmuckelement verwendet wird.
    /// </summary>
    internal class Wall : DirectionAdorner {
        /// <summary>
        /// Die Textur, die für die Wand verwendet wird.
        /// </summary>
        public static Drawing.DirectionTexture Texture = new("wand_");
        public static Drawing.DirectionTexture TextureBaustelle = new("baustelle_wand_");

        /// <summary>
        /// Gibt die Richtungstextur für die Wand zurück.
        /// </summary>
        /// <returns>Die Richtungstextur der Wand.</returns>
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }
        protected override Drawing.DirectionTexture GetBuildDirectionTexture() { return TextureBaustelle; }

        /// <summary>
        /// Erstellt eine neue Instanz einer Wand mit den angegebenen Richtungen.
        /// </summary>
        /// <param name="gem">Das Datenmodell, das die Wandrichtungen definiert.</param>
        public Wall(PhoenixModel.dbErkenfara.KleinFeld gem)
            : base(gem.Wall_NW, gem.Wall_NO, gem.Wall_O, gem.Wall_SO, gem.Wall_SW, gem.Wall_W) { }

        /// <summary>
        /// Gibt den Präfix für Bilddateien zurück, die mit einer Wand beginnen.
        /// </summary>
        /// <returns>Der Präfix-String für Wandbilder.</returns>
        public static string GetimageStartsWith() {
            return "wand_";
        }
    }
}
