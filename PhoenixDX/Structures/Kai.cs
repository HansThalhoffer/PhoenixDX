using PhoenixDX.Drawing;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert eine Kai-Struktur als Richtungsschmuckelement.
    /// </summary>
    internal class Kai : DirectionAdorner {
        /// <summary>
        /// Die Textur für die Kai-Richtungen.
        /// </summary>
        public static Drawing.DirectionTexture Texture = new("kai_");
        public static Drawing.DirectionTexture TextureBaustelle = new("baustelle_kai_");

        /// <summary>
        /// Ruft die Richtungstextur für den Kai ab.
        /// </summary>
        /// <returns>Die Richtungstextur für den Kai.</returns>
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }
        protected override Drawing.DirectionTexture GetBuildDirectionTexture() { return TextureBaustelle; }

        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="Kai"/>-Klasse mit den angegebenen Richtungen.
        /// </summary>
        /// <param name="gem">Das zugehörige KleinFeld-Objekt aus dem Modell.</param>
        public Kai(PhoenixModel.dbErkenfara.KleinFeld gem)
            : base(gem.Kai_NW, gem.Kai_NO, gem.Kai_O, gem.Kai_SO, gem.Kai_SW, gem.Kai_W) { }
    }
}
