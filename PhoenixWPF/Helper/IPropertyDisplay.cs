using PhoenixModel.Helper;

namespace PhoenixWPF.Helper {
    /// <summary>
    /// Schnittstelle zur Darstellung von Eigenschaften eines Objekts.
    /// </summary>
    public interface IPropertyDisplay {
        /// <summary>
        /// Zeigt die Eigenschaften eines Objekts an.
        /// </summary>
        /// <param name="eigenschaftler">Das Objekt, dessen Eigenschaften angezeigt werden sollen.</param>
        public void Display(IEigenschaftler eigenschaftler);
    }

    /// <summary>
    /// Schnittstelle für Optionen zur Steuerung von Einstellungen, wie z. B. Zoomlevel.
    /// </summary>
    public interface IOptions {
        /// <summary>
        /// Ändert das Zoom-Level der Benutzeroberfläche.
        /// </summary>
        /// <param name="zoomLevel">Der neue Zoom-Level.</param>
        public void ChangeZoomLevel(float zoomLevel);
    }
}
