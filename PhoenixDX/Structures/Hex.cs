using System.Numerics;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Stellt eine hexagonale Struktur dar.
    /// </summary>
    internal class Hex {
        /// <summary>
        /// Breite des Hexagons.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Höhe des Hexagons.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Breite einer Spalte.
        /// </summary>
        public float ColumnWidth { get; set; }

        /// <summary>
        /// Höhe einer Zeile.
        /// </summary>
        public float RowHeight { get; set; }

        /// <summary>
        /// Äußerer Radius des Hexagons.
        /// </summary>
        public float OuterRadius { get; set; }

        /// <summary>
        /// Innerer Radius des Hexagons (berechnet auf Basis des äußeren Radius).
        /// </summary>
        public float InnerRadius { get { return OuterRadius * 0.866025404f; } }

        /// <summary>
        /// Radius einer Provinz.
        /// </summary>
        public static float RadiusProvinz = 240f;

        /// <summary>
        /// Radius einer Gemarkung.
        /// </summary>
        public static float RadiusGemark = 34.7f;

        /// <summary>
        /// Erstellt ein neues Hexagon mit gegebenem Radius und Ausrichtung.
        /// </summary>
        /// <param name="radius">Der Radius des Hexagons.</param>
        /// <param name="pointUp">Gibt an, ob die Spitze nach oben zeigt.</param>
        public Hex(float radius, bool pointUp) {
            setDimensions(radius, pointUp);
        }

        /// <summary>
        /// Gibt die Ecken des Hexagons als Array von 2D-Vektoren zurück.
        /// </summary>
        public Vector2[] Corners {
            get {
                return new Vector2[] {
                    new Vector2(0f, OuterRadius),
                    new Vector2(InnerRadius, 0.5f * OuterRadius),
                    new Vector2(InnerRadius, -0.5f * OuterRadius),
                    new Vector2(0f, -OuterRadius),
                    new Vector2(-InnerRadius, -0.5f * OuterRadius),
                    new Vector2(-InnerRadius, 0.5f * OuterRadius)
                };
            }
        }

        /// <summary>
        /// Setzt die Dimensionen des Hexagons abhängig von der Ausrichtung.
        /// </summary>
        /// <param name="radius">Der Radius des Hexagons.</param>
        /// <param name="pointUp">Gibt an, ob die Spitze nach oben zeigt.</param>
        private void setDimensions(float radius, bool pointUp) {
            OuterRadius = radius;
            const float factor = 1.14f;
            if (pointUp == false) {
                Width = 1.5f * OuterRadius * 1.05f;
                Height = InnerRadius * 2f * 1.05f;
                ColumnWidth = 1.5f * OuterRadius;
                RowHeight = InnerRadius * 2f;
            }
            else {
                Width = InnerRadius * 2f * factor;
                Height = OuterRadius * 1.5f * factor;
                ColumnWidth = InnerRadius * 2f;
                RowHeight = 1.5f * OuterRadius;
            }
        }
    }
}
