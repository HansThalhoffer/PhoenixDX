using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PhoenixDX.Helper {
    /// <summary>
    /// Verwaltet Schriftarten (Fonts) für das Spiel.
    /// </summary>
    internal class FontManager {
        /// <summary>
        /// Eine Sammlung von Schriftarten, die mit ihrem Namen als Schlüssel gespeichert sind.
        /// </summary>
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();

        /// <summary>
        /// Lädt die Schriftarten aus dem Content-Manager und fügt sie der Sammlung hinzu.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager, aus dem die Schriftarten geladen werden.</param>
        public static void LoadContent(ContentManager contentManager) {
            Fonts.Add("Default", contentManager.Load<SpriteFont>("Fonts/DefaultFont"));
            Fonts.Add("Large", contentManager.Load<SpriteFont>("Fonts/LargeFont"));
            Fonts.Add("Small", contentManager.Load<SpriteFont>("Fonts/SmallFont"));
        }
    }
}
