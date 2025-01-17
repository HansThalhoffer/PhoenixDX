using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert eine Gemarkung auf der Karte.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay()}")]
    internal class Gemark : Hex {
        /// <summary>
        /// Erstellt eine Debug-String-Darstellung der Gemarkung.
        /// </summary>
        public string DebuggerDisplay() {
            return $"Gemark {Koordinaten.gf}/{Koordinaten.kf} {_terrainType} [{Reich.name}]";
        }
        /// <summary>
        /// Physikalische Position auf der Karte cache member für Geschwindigkeit
        /// </summary>
        private static Microsoft.Xna.Framework.Vector2 _mapCoords = new();
        /// <summary>
        /// Physikalische Größe auf der Karte cache member für Geschwindigkeit
        /// </summary>
        private static Microsoft.Xna.Framework.Vector2 _mapSize = new();
        /// <summary>
        /// aktueller Skalierungsfaktor übergeben von der Drawing Funktion
        /// </summary>
        private static Vektor _scale = new(0, 0);
        /// <summary>
        /// damit die Gemark nur 1x initialisiert wird
        /// </summary>
        private bool _isInitalized = false;
        /// <summary>
        /// Breite der Textur in Pixeln.
        /// </summary>
        public static readonly int TextureWidth = 138;
        /// <summary>
        /// Höhe der Textur in Pixeln.
        /// </summary>
        public static readonly int TextureHeight = 160;
        /// <summary>
        /// X-Koordinate der Gemarkung in einem Raster von Reihen und Spalten
        /// </summary>
        public int X { get; private set; } = 0;
        /// <summary>
        /// Y-Koordinate der Gemarkung  in einem Raster von Reihen und Spalten
        /// </summary>
        public int Y { get; private set; } = 0;
        /// <summary>
        /// ID des zugehörigen Reiches.
        /// </summary>
        public int ReichID { get; private set; }
        /// <summary>
        /// zugeöhriges <see cref="Reich"/>
        /// Das Setzen des Wertes erzeugt den farbigen Layer für das Reich automatisch
        /// </summary>
        private Reich _reich = null;
        public Reich Reich {
            get => _reich;
            set {
                _reich = value;
                // alte Reichszugehörigkeit entfernen, falls vorhanden
                if (Layer_0 != null) {
                    var alteReiche = Layer_0.Where(item => item is Reich);
                    foreach(var alte in alteReiche)
                        Layer_0.Remove(alte);
                }
                Layer_0.Insert(0, value);
            }
        }
        /// <summary>
        /// Gibt an, ob diese Gemarkung ausgewählt ist.
        /// </summary>
        public bool IsSelected { get; set; } = false;
        /// <summary>
        /// Kartenkoordinaten der Gemarkung.
        /// </summary>
        public string Bezeichner { get; private set; }
        /// <summary>
        /// Kennung des zugehörigen Reiches.
        /// </summary>
        public int ReichKennzahl { get; set; }
        /// <summary>
        /// Kartenkoordinaten der Gemarkung, entsprechend dem Kleinfeld
        /// </summary>
        public KartenKoordinaten Koordinaten { get; private set; }
        /// <summary>
        /// Der Typ des Geländes.
        /// </summary>
        TerrainType _terrainType = TerrainType.Default;
        /// <summary>
        /// Liste der Adorner für die erste Ebene.
        /// </summary>
        List<GemarkAdorner> _layer_0 = [];
        /// <summary>
        /// Liste der Adorner für die zweite Ebene.
        /// </summary>
        List<GemarkAdorner> _layer_1 = [];
        /// <summary>
        /// Zugriff auf die Adorner der ersten Ebene.
        /// </summary>
        public List<GemarkAdorner> Layer_0 { get { return _layer_0; } }
        /// <summary>
        /// Zugriff auf die Adorner der zweiten Ebene.
        /// </summary>
        public List<GemarkAdorner> Layer_1 { get { return _layer_1; } }

        /// <summary>
        /// Erstellt eine neue Instanz von <see cref="Gemark"/>.
        /// </summary>
        /// <param name="gf">Globale Feld-ID.</param>
        /// <param name="kf">Klein-Feld-ID.</param>
        public Gemark(int gf, int kf) : base(Hex.RadiusGemark, true) {
            Koordinaten = new KartenKoordinaten(gf, kf, 0, 0);
            _terrainType = TerrainType.Default;
            var pos = Koordinaten.GetPositionInProvinz();
            X = pos.X;
            Y = pos.Y;
            Bezeichner = kf.ToString();
        }

        /// <summary>
        /// Physikalische Größe auf der Karte cache member für Geschwindigkeit
        /// ACHTUNG: damit das funktioniert, muss zuerst <see cref="GetMapPosition"/> aufgerufen werden
        /// denn dort wird alles berechnet
        /// </summary>
        public static Vektor GetMapSize() {
            return _mapSize;
        }
        /// <summary>
        /// Berechnet die physikalische Position auf der Karte und speichert sie im Cache.
        /// </summary>
        /// <param name="provinzCoords">Koordinaten der Provinz.</param>
        /// <param name="scale">Skalierungsfaktor.</param>
        public Vektor GetMapPosition(Microsoft.Xna.Framework.Vector2 provinzCoords, Vektor scale) {
            if (scale.X != _scale.X || scale.X != _scale.Y) {
                _mapSize = new Microsoft.Xna.Framework.Vector2(Height * scale.X, Width * scale.Y);
                _scale.X = scale.X;
                _scale.Y = scale.Y;
                float x = 0;
                if (Y < 4)
                    x = ((X - 1) * ColumnWidth - (6 - Y - x) * ColumnWidth / 2) * scale.X;
                if (Y == 4)
                    x = (X - 1) * ColumnWidth * scale.X;
                if (Y == 5)
                    x = ((X - 1) * ColumnWidth - ColumnWidth / 2) * scale.X;
                if (Y == 6)
                    x = (X - 1) * ColumnWidth * scale.X;
                if (Y == 7)
                    x = ((X - 2) * ColumnWidth - ColumnWidth / 2) * scale.X;
                if (Y == 8)
                    x = ((X - 4) * ColumnWidth) * scale.X;

                float y = (Y - 1) * RowHeight * scale.Y;
                _mapCoords = new Microsoft.Xna.Framework.Vector2(provinzCoords.X + x, provinzCoords.Y + y);
            }
            return _mapCoords;
        }

        /// <summary>
        /// Initialisiert eine Klein-Feld Instanz und lädt die relevanten Daten.
        /// </summary>
        /// <param name="gem">Instanz des KleinFeld-Objekts.</param>
        /// <returns>True, falls erfolgreich initialisiert.</returns>
        public bool Initialize(KleinFeld gem) {
            if (_isInitalized == true)
                return false;
            _isInitalized = true;
            try {
                ReichKennzahl = (int)gem.Reich;
            }
            catch (Exception ex) {
                MappaMundi.Log(gem.gf, gem.kf, $"Beim Anlegen der Nation kam es zu einem Fehler", ex);
            }
            try {
                if (gem.Gelaendetyp <= (int)TerrainType.AuftauchpunktUnbekannt) {
                    if (WeltDrawer.ShowKüsten == true && KleinfeldView.UserHasKuestenrecht(gem))
                        _terrainType = TerrainType.Küste;
                    else
                        _terrainType = (TerrainType)gem.Gelaendetyp;
                }
            }
            catch (Exception ex) {
                MappaMundi.Log(gem.gf, gem.kf, $"Beim Anlegen des Geländes kam es zu einem Fehler", ex);
            }
            Koordinaten = new KartenKoordinaten(Koordinaten.gf, Koordinaten.kf, (int)gem.x, (int)gem.y);
            ReichID = gem.Reich ?? -1;

            try {
                Layer_0.Add(new Fluss(gem));
                Layer_0.Add(new Kai(gem));
                Layer_0.Add(new Bruecke(gem));
                Layer_0.Add(new Strasse(gem));
                Layer_0.Add(new Wall(gem));
            }
            catch (Exception ex) {
                MappaMundi.Log(gem.gf, gem.kf, $"Beim Anlegen der Geländebestandteile kam es zu einem Fehler", ex);
            }
            if (gem.Gebäude != null) {
                try {
                    string name = gem.Gebäude.Rüstort.Bauwerk;
                    if (RuestortSymbol.Ruestorte.ContainsKey(name)) {
                        // Layer_0.Add("Rüstort", RuestortSymbol.Ruestorte[name]);
                        Layer_0.Add(RuestortSymbol.Ruestorte[name]);
                    }
                    else {
                        MappaMundi.Log(this.Koordinaten.gf, this.Koordinaten.kf, new PhoenixModel.Program.LogEntry($"Unbekanntes Gebäude {name}", $"Die Bezeichnung des Gebäudes {name} findet sich nicht in der Referenztabelle für Bauwerke"));
                    }
                }
                catch (Exception ex) {
                    MappaMundi.Log(gem.gf, gem.kf, $"Beim Anlegen der Gebäude kam es zu einem Fehler", ex);
                }
            }

            var spielfiguren = gem.Truppen;
            if (spielfiguren != null && spielfiguren.Count > 0) {
                try {
                    List<Truppen.Figur> truppen = [];
                    foreach (var figur in spielfiguren) {
                        Microsoft.Xna.Framework.Color color = Kolor.Convert(figur.Nation.Farbe);
                        truppen.Add(new Truppen.Figur(figur.Typ, color));
                    }
                    if (truppen.Count > 0) {
                        Layer_1.Add(new Truppen(truppen));
                    }
                }
                catch (Exception ex) {
                    MappaMundi.Log(gem.gf, gem.kf, $"Beim Anlegen der Truppen kam es zu einem Fehler", ex);
                }
            }

            if (gem.Mark != MarkerType.None) {
                Layer_1.Add(new Marker(gem.Mark));
            }
            return true;
        }

        #region Selection

        /// <summary>
        /// Die Funktion überprüft, ob die Maus über der Gemark ist
        /// Die Selektion durch den Mauszeiger erfolgt durch eine quadratische Näherung. 
        /// Dies ist zwar ungenau, aber für die Zwecke vollkommen ausreichend
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public bool InKleinfeld(Microsoft.Xna.Framework.Vector2 mousePos) {
            if (mousePos == Vector2.Zero)
                return false;
            if (_mapCoords.X > mousePos.X || _mapCoords.Y > mousePos.Y)
                return false;

            if (mousePos.X > _mapCoords.X + Width * _scale.X * 0.85f || mousePos.Y > _mapCoords.Y + Height * _scale.Y * 0.85f)
                return false;
                
            /* Hier könnte eine Berechnung des HEX Feldes erfolgen, ist aber praktisch nicht notwendig
             * PointF[] hexVertices = {
                 new PointF(_mapCoords.X, _mapCoords.Y - Height / 2f), // top
                 new PointF(_mapCoords.X + Width / 2f, _mapCoords.Y - Height / 4f), // top-right
                 new PointF(_mapCoords.X + Width / 2f, _mapCoords.Y + Height / 4f), // bottom-right
                 new PointF(_mapCoords.X, _mapCoords.Y + Height / 2f), // bottom
                 new PointF(_mapCoords.X - Width / 2f, _mapCoords.Y + Height / 4f), // bottom-left
                 new PointF(_mapCoords.X - Width / 2f, _mapCoords.Y - Height / 4f)  // top-left
             };*/

            return true;
        }

        #endregion

        #region Content
        /// <summary>
        /// Gibt eine Liste von Texturen für den angegebenen Layer zurück.
        /// Beim Layer 0 wird die Textur des Geländes als erstes eingefügt in die Liste
        /// danach folgen die Adorner in der Reihenfolge des Einfügens in die Liste der Adorner
        /// </summary>
        /// <param name="layer">Die Layer-Nummer.</param>
        /// <returns>Eine Liste von Texturobjekten.</returns>
        public List<BaseTexture> GetTextures(int layer) {
            List<BaseTexture> textures = [];
            try {
                switch (layer) {
                    case 0: {
                            Gelaende gel = GeländeTabelle.Terrains[(int)_terrainType] as Gelaende;
                            if (gel != null)
                                textures.Add(gel.GetTexture());
                            foreach (GemarkAdorner adorner in Layer_0) {
                                var tex = adorner.GetTexture();
                                if (tex != null)
                                    textures.Add(tex);
                            }
                            break;
                        }
                    case 1: {
                            foreach (GemarkAdorner adorner in Layer_1) {
                                var tex = adorner.GetTexture();
                                if (tex != null)
                                    textures.Add(tex);
                            }
                            break;
                        }
                    default:
                        MappaMundi.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler bei der Auswahl des Layers {layer}", "Der Layer {layer} in der DirectX Karte existiert nicht. Daher können dort auch keine Texturen gefunden werden."));
                        break;
                }
            }
            catch (Exception ex) {
                MappaMundi.Log(this.Koordinaten.gf, this.Koordinaten.kf, $"Beim Holen der Texturen kam es zu einem Fehler", ex);
            }
            return textures;
        }

        /// <summary>
        /// Gibt an, ob die Inhalte bereits geladen wurden.
        /// </summary>
        static bool _isLoaded = false;
        /// <summary>
        /// Gibt an, ob die Inhalte bereits geladen wurden.
        /// </summary>
        public static bool IsLoaded { get { return _isLoaded; } }
        /// <summary>
        /// Lädt die benötigten Inhalte und initialisiert die Texturen für alle abgeleiteten Typen von GemarkAdorner.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager zum Laden der Texturen.</param>
        public static void LoadContent(ContentManager contentManager) {
            try {
                // Get all types in the current assembly that derive from the base type
                var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(GemarkAdorner)))
                    .ToList();

                var textureValues = new List<DirectionTexture>();
                foreach (var type in derivedTypes) {
                    var textureField = type.GetField("Texture", BindingFlags.Public | BindingFlags.Static);
                    if (textureField != null && textureField.FieldType == typeof(DirectionTexture)) {
                        var fieldValue = textureField.GetValue(null) as DirectionTexture;
                        if (fieldValue != null) {
                            textureValues.Add(fieldValue);
                        }
                    }
                }

                const string folder = "Images/TilesetV/";
                foreach (var adornerTexture in textureValues) {
                    List<Texture2D> textures = new List<Texture2D>();
                    foreach (string name in Enum.GetNames(typeof(Direction))) {
                        string fileName = folder + adornerTexture.ImageStartsWith + name;
                        try {
                            textures.Add(contentManager.Load<Texture2D>(fileName));
                        }
                        catch (Exception ex) {
                            MappaMundi.Log(0, 0, $"Fehler bei der Laden von Texturem von {name}", ex);
                        }
                    }
                    adornerTexture.SetTextures(textures.ToArray());
                }
            }
            catch (Exception ex) {
                MappaMundi.Log($"Beim Laden der Texturen aus der Bibliothek kam es zu einem Fehler", ex);
            }
            _isLoaded = true;
        }
        #endregion

    }

}
