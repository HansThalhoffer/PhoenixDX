using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Drawing;
using PhoenixDX.Helper;
using PhoenixDX.Program;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Generic;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Der Reich Adorner lässt sich in Zukunft einfacher, mit weniger Texturen als ColorAdorner abbilden.
    /// </summary>
    internal class Reich : GemarkAdorner {
        /// <summary>
        /// Die Farbe des Reichs.
        /// </summary>
        public Color color;

        /// <summary>
        /// Der Name des Reichs.
        /// </summary>
        public string name;

        /// <summary>
        /// Ein Dictionary mit den Farben der verschiedenen Reiche.
        /// </summary>
        static Dictionary<string, SimpleTexture> reichsFarben = [];

        /// <summary>
        /// Die Textur für das Hex-Element.
        /// </summary>
        private SimpleTexture _hexTexture = null;

        /// <summary>
        /// Erstellt eine neue Instanz eines Reichs basierend auf der übergebenen Nation.
        /// </summary>
        /// <param name="nation">Die Nation, aus der das Reich erstellt wird.</param>
        public Reich(Nation nation) {
            color = Kolor.Convert(nation.Farbe);
            name = nation.Reich;
            if (reichsFarben.TryGetValue(nation.Farbname, out _hexTexture) == false)
                MappaMundi.Log(new PhoenixModel.Program.LogEntry("Fehler", $"Nation hat keine Farbe gefunden: {nation.Farbname}"));
        }

        /// <summary>
        /// Lädt die Texturen für die verschiedenen Reiche aus dem Content Manager.
        /// </summary>
        /// <param name="contentManager">Der Content Manager zum Laden der Texturen.</param>
        public static void LoadContent(ContentManager contentManager) {
            try {
                reichsFarben.Add("blau", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_blau")));
                reichsFarben.Add("braun", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_braun")));
                reichsFarben.Add("gelb", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gelb")));
                reichsFarben.Add("grau", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_grau")));
                reichsFarben.Add("dunkelgrün", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gruen")));
                reichsFarben.Add("hellgrün", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_hellgruen")));
                reichsFarben.Add("hellblau", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_hellblau")));
                reichsFarben.Add("lila", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_lila")));
                reichsFarben.Add("orange", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_orange")));
                reichsFarben.Add("rot", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_rot")));
                reichsFarben.Add("schwarz", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_schwarz")));
                reichsFarben.Add("spielleitung", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_spielleitung")));
                reichsFarben.Add("türkis", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_tuerkis")));
                reichsFarben.Add("weiß", new SimpleTexture(contentManager.Load<Texture2D>("Images/Reichsfarben/reich_weiss")));
            }
            catch (Exception ex) {
                MappaMundi.Log(0, 0, "Die Textur für die Farben einer Nation konnte nicht geladen werden", ex);
            }
        }

        /// <summary>
        /// Gibt die zugehörige Textur des Reichs zurück.
        /// </summary>
        /// <returns>Die Textur des Reichs.</returns>
        public override BaseTexture GetTexture() {
            return _hexTexture;
        }
    }
}
