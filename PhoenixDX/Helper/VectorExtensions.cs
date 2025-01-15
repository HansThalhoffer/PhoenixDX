using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PhoenixModel.ViewModel;

namespace PhoenixDX.Helper {
    public static class VectorExtensions
    {
        public static void Move(ref this Vector2 v, int value)
        {
            v.X += value;
            v.Y += value;
        }

        public static void Move(ref this Vector2 v, int moveX, int moveY)
        {
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
