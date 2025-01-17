using System.Collections.Generic;
using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Repräsentiert eine Provinz innerhalb der Spielwelt.
    /// </summary>
    /// <remarks>
    /// Eine Provinz besteht aus mehreren Feldern (Gemarkungen) und hat eine eindeutige Position auf der Karte.
    /// </remarks>
    internal class Provinz : Hex {
        private int _gf;

        /// <summary>
        /// Die X-Koordinate der Provinz.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Die Y-Koordinate der Provinz.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Gibt den Gelände-Faktor der Provinz zurück.
        /// </summary>
        public int GF {
            get { return _gf; }
            private set {
                _gf = value;
            }
        }

        /// <summary>
        /// Gibt die Bezeichnung der Provinz zurück.
        /// </summary>
        public string Bezeichner {
            get { return _gf.ToString(); }
        }

        /// <summary>
        /// Enthält alle Felder (Gemarkungen) der Provinz.
        /// </summary>
        public Dictionary<int, Gemark> Felder { get; set; } = [];

        /// <summary>
        /// Erstellt eine neue Provinz mit den angegebenen Koordinaten und Gelände-Faktor.
        /// </summary>
        /// <param name="x">Die X-Koordinate.</param>
        /// <param name="y">Die Y-Koordinate.</param>
        /// <param name="gf">Der Gelände-Faktor.</param>
        public Provinz(int x, int y, int gf) : base(Hex.RadiusProvinz, false) {
            X = x;
            Y = y;
            GF = gf;
        }

        private Vektor _mapCoords = new();
        private Vektor _mapSize = new();
        private Vektor _scale = new(0, 0);

        /// <summary>
        /// Berechnet die Kartenposition der Provinz basierend auf dem Maßstab.
        /// </summary>
        /// <param name="scale">Der Skalierungsfaktor.</param>
        /// <returns>Die Position der Provinz auf der Karte.</returns>
        public Vektor GetMapPosition(Vektor scale) {
            if (scale.X != _scale.X || scale.X != _scale.Y) {
                _scale.X = scale.X;
                _scale.Y = scale.Y;
                float x = (X - 1) * ColumnWidth * scale.X;
                float y = (Y - 1) * RowHeight * scale.Y;
                if (X % 2 > 0)
                    y += RowHeight * scale.Y / 2;
                _mapCoords = new Microsoft.Xna.Framework.Vector2(x, y);
                _mapSize = new Microsoft.Xna.Framework.Vector2(Height * scale.X, Width * scale.Y);
            }
            return _mapCoords;
        }

        /// <summary>
        /// Gibt die Kartengröße der Provinz zurück.
        /// </summary>
        /// <returns>Die Größe der Provinz auf der Karte.</returns>
        public Microsoft.Xna.Framework.Vector2 GetMapSize() {
            return _mapSize;
        }

        /// <summary>
        /// Gibt eine Gemarkung innerhalb der Provinz zurück.
        /// </summary>
        /// <param name="kf">Die ID der Gemarkung.</param>
        /// <returns>Die entsprechende Gemarkung oder null, falls sie nicht existiert.</returns>
        public Gemark GetKleinfeld(int kf) {
            Gemark gemark = null;
            if (Felder.TryGetValue(kf, out gemark))
                return gemark;
            return null;
        }

        /// <summary>
        /// Gibt eine existierende Gemarkung zurück oder erstellt eine neue, falls sie nicht existiert.
        /// </summary>
        /// <param name="kf">Die ID der Gemarkung.</param>
        /// <returns>Die existierende oder neu erstellte Gemarkung.</returns>
        public Gemark GetOrCreateKleinfeld(int kf) {
            Gemark gemark = null;
            if (Felder.TryGetValue(kf, out gemark))
                return gemark;
            gemark = new Gemark(GF, kf);
            Felder.Add(kf, gemark);
            return gemark;
        }
    }
}
