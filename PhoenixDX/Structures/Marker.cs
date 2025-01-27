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
                coloredTextures[0] = new ColoredTexture(_hexTexture, Color.Turquoise);
                coloredTextures[1] = new ColoredTexture(_hexTexture, Color.Yellow);
                coloredTextures[2] = new ColoredTexture(_hexTexture, Color.Orange);
                coloredTextures[3] = new ColoredTexture(_hexTexture, Color.Red);
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
            switch (MarkerType)
            {
                case MarkerType.None:
                    return null;
                case MarkerType.User:
                    return coloredTextures[0];
                case MarkerType.Info:
                    return coloredTextures[1];
                case MarkerType.Warning:
                    return coloredTextures[2];
                case MarkerType.Fatality:
                    return coloredTextures[3];
                default:
                    throw new ArgumentException($"Der übergebene Markertyp {MarkerType} wurde in der Kartendarstellung PhoenixDX noch nicht implementiert");
            }
        }
    }
}
