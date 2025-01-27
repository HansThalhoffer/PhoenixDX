namespace PhoenixModel.View {
    /// <summary>
    /// Attribut zur Markierung einer Eigenschaft als editierbar.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class Editable : Attribute {
        /// <summary>
        /// Eine optionale Beschreibung zur editierbaren Eigenschaft.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Erstellt eine neue Instanz des Editable-Attributs.
        /// </summary>
        /// <param name="description">Eine optionale Beschreibung der editierbaren Eigenschaft.</param>
        public Editable(string? description = null) {
            Description = description;
        }
    }
}
