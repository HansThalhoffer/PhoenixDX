using PhoenixDX.Drawing;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert eine Brücke als DirectionAdorner.
    /// </summary>
    internal class Bruecke : DirectionAdorner {
        /// <summary>
        /// Statische Textur für die Brücke.
        /// </summary>
        public static Drawing.DirectionTexture Texture = new("bruecke_");

        /// <summary>
        /// Gibt die Richtungs-Textur für die Brücke zurück.
        /// </summary>
        /// <returns>Die Richtungstextur für die Brücke.</returns>
        protected override Drawing.DirectionTexture GetDirectionTexture() { return Texture; }

        /// <summary>
        /// Erstellt eine neue Instanz der Brücke.
        /// </summary>
        /// <param name="gem">Das KleinFeld-Objekt mit Brückeninformationen.</param>
        public Bruecke(PhoenixModel.dbErkenfara.KleinFeld gem)
            : base(gem.Bruecke_NW, gem.Bruecke_NO, gem.Bruecke_O, gem.Bruecke_SO, gem.Bruecke_SW, gem.Bruecke_W) { }
    }
}
