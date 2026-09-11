using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Helper;
using PhoenixModel.View;
using System;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Klasse zur Darstellung eines Markers mit unterschiedlichen Farben und Typen.
    /// </summary>
    internal class Marker : ColorAdorner
    {
        static Texture2D _hexTexture = null;
        /// <summary>
        /// Der Typ des Markers.
        /// </summary>
        MarkerType MarkerType { get; set; }
        static ColoredTexture[] coloredTextures = new ColoredTexture[Enum.GetNames(typeof(MarkerType)).Length];

        /// <summary>
        /// Erstellt eine neue Instanz eines Markers mit dem angegebenen Typ.
        /// </summary>
        /// <param name="mark">Der Marker-Typ.</param>
        public Marker(MarkerType mark)
        {
            MarkerType = mark;
        }

        /// <summary>
        /// Lädt die benötigten Inhalte für den Marker.
        /// </summary>
        /// <param name="contentManager">Der ContentManager für das Laden von Ressourcen.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            try
            {
                _hexTexture = contentManager.Load<Texture2D>("Images/TilesetV/Info");
                // Die Texturen werden über den Wert des MarkerTyps indiziert, damit ein neuer Typ
                // im Model nicht stillschweigend die Farben verschiebt.
                SetColor(MarkerType.User, Color.Turquoise);
                SetColor(MarkerType.Info, Color.Yellow);
                SetColor(MarkerType.Warning, Color.Orange);
                SetColor(MarkerType.Fatality, Color.Red);
                SetColor(MarkerType.Bewegung, Color.LightGreen);
                SetColor(MarkerType.Weg, Color.CornflowerBlue);
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, "Die Textur für die Farben einer Nation konnte nicht geladen werden", ex);
            }
        }

        /// <summary>
        /// Erstellt eine farbige Textur basierend auf dem Marker-Typ.
        /// </summary>
        /// <returns>Die entsprechende ColoredTexture oder null.</returns>
        public override ColoredTexture CreateTexture()
        {
            if (MarkerType == MarkerType.None)
                return null;
            int index = (int)MarkerType;
            if (index < 0 || index >= coloredTextures.Length || coloredTextures[index] == null)
                throw new ArgumentException($"Der übergebene Markertyp {MarkerType} wurde in der Kartendarstellung PhoenixDX noch nicht implementiert");
            return coloredTextures[index];
        }

        /// <summary>
        /// Hinterlegt die Farbe für einen Markertyp an der Position seines Enum-Wertes.
        /// </summary>
        private static void SetColor(MarkerType markerType, Color color)
        {
            coloredTextures[(int)markerType] = new ColoredTexture(_hexTexture, color);
        }
    }
}
