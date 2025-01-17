using Microsoft.Xna.Framework;
using PhoenixModel.ViewModel;
using System;

namespace PhoenixDX.Helper {
    /// <summary>
    /// Statische Klasse zur Erweiterung von <see cref="Vector2"/> um Bewegungsfunktionen.
    /// </summary>
    public static class VectorExtensions {
        /// <summary>
        /// Verschiebt einen <see cref="Vector2"/>-Wert um einen bestimmten Betrag in beide Richtungen.
        /// </summary>
        /// <param name="v">Der zu verschiebende Vektor.</param>
        /// <param name="value">Der Wert, um den sich X- und Y-Koordinaten gleichermaßen ändern.</param>
        public static void Move(ref this Vector2 v, int value) {
            v.X += value;
            v.Y += value;
        }

        /// <summary>
        /// Verschiebt einen <see cref="Vector2"/>-Wert um unterschiedliche Werte für X und Y.
        /// </summary>
        /// <param name="v">Der zu verschiebende Vektor.</param>
        /// <param name="moveX">Der Wert, um den sich die X-Koordinate ändert.</param>
        /// <param name="moveY">Der Wert, um den sich die Y-Koordinate ändert.</param>
        public static void Move(ref this Vector2 v, int moveX, int moveY) {
            v.X += moveX;
            v.Y += moveY;
        }
    }

    public static class PositionExtensions
    {
        /// <summary>
        /// Sets the X and Y values of the Position from a Vector2.
        /// </summary>
        /// <param name="position">The Position object to update.</param>
        /// <param name="vector">The Vector2 containing the new X and Y values.</param>
        public static void SetFromVector2(this Position position, Microsoft.Xna.Framework.Vector2 vector)
        {
            position.X = Convert.ToInt32(vector.X);
            position.Y = Convert.ToInt32(vector.Y);
        }
    }
}
