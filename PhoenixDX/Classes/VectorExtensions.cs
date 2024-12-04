using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Classes
{
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
}
