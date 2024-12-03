using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
   public  class Hex
    {
        public bool PointUp { get; set; } = true;
        public double Width { get; set; }
        public double Height { get; set; }
        public double ColumnWidth { get; set; }
        public double RowHeight { get; set; }
        public double Radius { get; set; }

        public static double RadiusProvinz= 240;
        public static double RadiusGemark = 34.7;

        public Hex( double radius) 
        {
            setDimensions(radius);
        }

        
        private void setDimensions(double radius)
        {
            Radius = radius;
            if (PointUp == false)
            {
                Width = 2 * Radius;
                ColumnWidth = 1.5d * Radius;
                RowHeight = Math.Sqrt((Radius * Radius) - ((Radius / 2) * (Radius / 2))) * 2;
                Height = RowHeight;
            }
            else
            {
                Height = 2 * Radius;
                RowHeight = 1.5d * Radius;
                ColumnWidth = Math.Sqrt((Radius * Radius) - ((Radius / 2) * (Radius / 2))) * 2;
                Width = ColumnWidth;
            }
        }
    }
}
