namespace PhoenixDX.Helper {
    /// <summary>
    /// Statische Hilfsklasse zur Farbkonvertierung zwischen System.Drawing.Color und Microsoft.Xna.Framework.Color.
    /// </summary>
    internal static class Kolor {
        /// <summary>
        /// Konvertiert eine optionale <see cref="System.Drawing.Color"/> in eine <see cref="Microsoft.Xna.Framework.Color"/>.
        /// </summary>
        /// <param name="color">Die zu konvertierende Farbe. Kann null sein.</param>
        /// <returns>
        /// Eine entsprechende <see cref="Microsoft.Xna.Framework.Color"/>-Instanz.
        /// Falls die Eingabe null ist, wird <see cref="Microsoft.Xna.Framework.Color.Transparent"/> zurückgegeben.
        /// </returns>
        internal static Microsoft.Xna.Framework.Color Convert(System.Drawing.Color? color) {
            if (color == null)
                return Microsoft.Xna.Framework.Color.Transparent;

            return new Microsoft.Xna.Framework.Color(color.Value.R, color.Value.G, color.Value.B);
        }
    }
}
