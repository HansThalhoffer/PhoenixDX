using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert ein Geländeobjekt mit einer zugehörigen Textur.
    /// </summary>
    internal class Gelaende : GeländeTabelle {
        private OpacityTexture hexTexture;
        private List<Texture2D> hexTextures = [];

        /// <summary>
        /// Erstellt eine neue Instanz eines Geländes und lädt die zugehörige Textur.
        /// </summary>
        /// <param name="source">Die Quelle der Geländedaten.</param>
        /// <param name="image">Der Bildname der zu ladenden Textur.</param>
        /// <param name="contentManager">Der Content-Manager für das Laden der Texturen.</param>
        public Gelaende(GeländeTabelle source, string image, ContentManager contentManager) :
            base(source.Typ, source.Name, source.Höhe, source.Einwohner, source.Einnahmen, source.Farbe, source.IsWasser) {
            try {
                const string folder = "Images/TilesetV/";
                hexTexture = new OpacityTexture(contentManager.Load<Texture2D>(folder + image),1f);
            }
            catch (Exception ex) {
                MappaMundi.Log(0, 0, $"Die Textur für {source.Name} konnte nicht geladen werden", ex);
            }
        }

        /// <summary>
        /// Gibt die Textur des Geländes zurück.
        /// </summary>
        /// <returns>Die SimpleTexture des Geländes.</returns>
        public OpacityTexture GetTexture() {
            return hexTexture;
        }

        private static bool _isLoaded = false;

        /// <summary>
        /// Gibt an, ob das Gelände geladen wurde.
        /// </summary>
        public static bool IsLoaded { get { return _isLoaded; } }
 
        public static void ChangeOpacity(float opacity) {
            if (!IsLoaded) {
                MappaMundi.Log(new LogEntry(LogEntry.LogType.Error,"Die Transparenz kann nicht gesetzt werden", "Die Terraindaten wurden noch nicht geladen"));
                return;
            }
            if (opacity < 0 || opacity > 1) {
                MappaMundi.Log(new LogEntry(LogEntry.LogType.Error, $"Der übergebene Wert {opacity} für Transparenz ist ungültig", "Bitte den Programmierfehler beheben. Der Wert muss > 0  und <1 sein."));
                return;
            }
            foreach (var gel in GeländeTabelle.Terrains) {
                var terrain = gel as Gelaende;
                if (terrain != null) {
                    terrain.GetTexture().Opacity = opacity;
                }
            }
        }

        /// <summary>
        /// Lädt die Geländetexturen aus den definierten Ressourcen.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager zum Laden der Ressourcen.</param>
        public static void LoadContent(ContentManager contentManager) {
            Terrains[(int)TerrainType.Default] = new Gelaende(Terrains[(int)TerrainType.Default], "defaultTerrain", contentManager);
            Terrains[(int)TerrainType.Wasser] = new Gelaende(Terrains[(int)TerrainType.Wasser], "ocean", contentManager);
            Terrains[(int)TerrainType.Hochland] = new Gelaende(Terrains[(int)TerrainType.Hochland], "highland", contentManager);
            Terrains[(int)TerrainType.Wald] = new Gelaende(Terrains[(int)TerrainType.Wald], "forest", contentManager);
            Terrains[(int)TerrainType.Wüste] = new Gelaende(Terrains[(int)TerrainType.Wüste], "desert", contentManager);
            Terrains[(int)TerrainType.Sumpf] = new Gelaende(Terrains[(int)TerrainType.Sumpf], "swamp", contentManager);
            Terrains[(int)TerrainType.Bergland] = new Gelaende(Terrains[(int)TerrainType.Bergland], "highland", contentManager);
            Terrains[(int)TerrainType.Gebirge] = new Gelaende(Terrains[(int)TerrainType.Gebirge], "mountain", contentManager);
            Terrains[(int)TerrainType.Tiefsee] = new Gelaende(Terrains[(int)TerrainType.Tiefsee], "deepsea", contentManager);
            Terrains[(int)TerrainType.Tiefland] = new Gelaende(Terrains[(int)TerrainType.Tiefland], "tiefland", contentManager);
            Terrains[(int)TerrainType.Auftauchpunkt] = new Gelaende(Terrains[(int)TerrainType.Auftauchpunkt], "auftauchpunkt_ocean", contentManager);
            Terrains[(int)TerrainType.Tiefseeeinbahnpunkt] = new Gelaende(Terrains[(int)TerrainType.Tiefseeeinbahnpunkt], "deepseapoint", contentManager);
            Terrains[(int)TerrainType.AuftauchpunktUnbekannt] = new Gelaende(Terrains[(int)TerrainType.AuftauchpunktUnbekannt], "auftauchpunkt_unbekannt", contentManager);
            Terrains[(int)TerrainType.Küste] = new Gelaende(Terrains[(int)TerrainType.Küste], "coast", contentManager);
            _isLoaded = true;
        }
    }
}
