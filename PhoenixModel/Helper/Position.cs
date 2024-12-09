namespace PhoenixModel.Helper
{
    // die Klasse ist notwendig, weil jeder unbedingt einen eigenen Point programmieren muss
    public class Position
    {
        public Position(int x, int y)
        {
            X = x; Y = y;
        }

        public Position(System.Drawing.Point p)
        {
            X = p.X; Y = p.Y;
        }

        public Position(System.Numerics.Vector2 p)
        {
            X = Convert.ToInt32(p.X); Y = Convert.ToInt32(p.Y);
        }
        public int X {  get; set; }
        public int Y { get; set; }

        public static Position? operator +(Position? p1, Position? p2)
        {
            if (p2 == null)
                return p1;
            if (p1 == null)
                return p2;
            return new Position(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static Position? operator -(Position? p1, Position? p2)
        {
            if (p2 == null)
                return p1;
            if (p1 == null)
                return p2;
            return new Position(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static Position operator *(Position p1, int scale)
        {
            return new Position(p1.X *scale, p1.Y * scale);
        }

        public void Scale(float scale)
        {
            X = Convert.ToInt32((float)X * scale);
            Y  = Convert.ToInt32((float)Y * scale);
        }
        public void Scale(float scaleX, float scaleY)
        {
            X = Convert.ToInt32((float)X * scaleX);
            Y = Convert.ToInt32((float)Y * scaleY);
        }

        public override string ToString()
        {
            return "X: " + X.ToString() + "  Y: " + Y.ToString();
        }

    }
}
