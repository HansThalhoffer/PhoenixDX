namespace PhoenixDX.Drawing {
    /// <summary>
    /// Abstrakte Basisklasse für Adorner in MonoGame. 
    /// Adorner werden verwendet, um Texturen basierend auf den Eigenschaften einer Struktur hinzuzufügen.
    /// </summary>
    internal abstract class GemarkAdorner
    {
        /// <summary>
        /// Standardkonstruktor für die Klasse <see cref="GemarkAdorner"/>.
        /// </summary>
        public GemarkAdorner()
        { }
        /// <summary>
        /// Abstrakte Methode zur Rückgabe einer Textur für die aktuelle Struktur.
        /// Diese Methode muss in abgeleiteten Klassen implementiert werden.
        /// </summary>
        /// <returns>Eine Instanz von <see cref="BaseTexture"/>, die die zu verwendende Textur darstellt.</returns>
        public abstract BaseTexture GetTexture();
    }  
}
