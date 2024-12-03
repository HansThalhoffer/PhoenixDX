using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PhoenixDX.Structures
{
   public  class Hex
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float ColumnWidth { get; set; }
        public float RowHeight { get; set; }
        public float OuterRadius { get; set; }
        public float InnerRadius { get { return OuterRadius * 0.866025404f; } }

        public static float RadiusProvinz = 240f;
        public static float RadiusGemark = 34.7f;

        public Hex(float radius, bool pointUp) 
        {
            setDimensions(radius, pointUp);
        }

        public Vector2[] Corners { get
            {
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

        private void setDimensions(float radius, bool pointUp)
        {
            OuterRadius = radius;
            Width = OuterRadius * 2f;
            Height = OuterRadius * 2f;
            
            if (pointUp == false)
            {
                ColumnWidth = 1.5f * OuterRadius;
                RowHeight =  Height;
            }
            else
            {
                ColumnWidth = Width;
                RowHeight = 1.5f * OuterRadius;
            }
        }
    }
}
