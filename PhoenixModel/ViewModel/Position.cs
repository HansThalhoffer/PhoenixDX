namespace PhoenixModel.ViewModel {
    /// <summary>
    /// die Klasse ist notwendig, weil jeder unbedingt einen eigenen Point programmieren muss
    /// </summary>
    public class Position {
        /// <summary>
        /// Konstruktor, der die Position mit ganzzahligen Werten für X und Y initialisiert.
        /// </summary>
        /// <param name="x">Die X-Koordinate der Position.</param>
        /// <param name="y">Die Y-Koordinate der Position.</param>
        public Position(int x, int y) {
            X = x; Y = y;
        }

        /// <summary>
        /// Konstruktor, der die Position mit Fließkommawerten für X und Y initialisiert und diese auf ganze Zahlen konvertiert.
        /// </summary>
        /// <param name="x">Die X-Koordinate als Fließkommazahl.</param>
        /// <param name="y">Die Y-Koordinate als Fließkommazahl.</param>
        public Position(float x, float y) {
            X = Convert.ToInt32(x);
            Y = Convert.ToInt32(y);
        }

        /// <summary>
        /// Konstruktor, der eine Position aus einem System.Drawing.Point erstellt.
        /// </summary>
        /// <param name="p">Das Point-Objekt aus der System.Drawing-Bibliothek.</param>
        public Position(System.Drawing.Point p) {
            X = p.X; Y = p.Y;
        }

        /// <summary>
        /// Konstruktor, der eine Position aus einem System.Numerics.Vector2 erstellt und die Werte in ganze Zahlen umwandelt.
        /// </summary>
        /// <param name="p">Das Vector2-Objekt aus der System.Numerics-Bibliothek.</param>
        public Position(System.Numerics.Vector2 p) {
            X = Convert.ToInt32(p.X); Y = Convert.ToInt32(p.Y);
        }

        /// <summary>
        /// Die X-Koordinate der Position.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Die Y-Koordinate der Position.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Überladener Operator für die Addition von zwei Positionen.
        /// Wenn eine der Positionen null ist, wird die andere Position zurückgegeben.
        /// </summary>
        /// <param name="p1">Die erste Position.</param>
        /// <param name="p2">Die zweite Position.</param>
        /// <returns>Die neue Position nach der Addition von X- und Y-Koordinaten.</returns>
        public static Position? operator +(Position? p1, Position? p2) {
            if (p2 == null)
                return p1;
            if (p1 == null)
                return p2;
            return new Position(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// Überladener Operator für die Subtraktion von zwei Positionen.
        /// Wenn eine der Positionen null ist, wird die andere Position zurückgegeben.
        /// </summary>
        /// <param name="p1">Die erste Position.</param>
        /// <param name="p2">Die zweite Position.</param>
        /// <returns>Die neue Position nach der Subtraktion der X- und Y-Koordinaten.</returns>
        public static Position? operator -(Position? p1, Position? p2) {
            if (p2 == null)
                return p1;
            if (p1 == null)
                return p2;
            return new Position(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// Überladener Operator für die Multiplikation einer Position mit einem Skalierungsfaktor.
        /// </summary>
        /// <param name="p1">Die Position, die skaliert werden soll.</param>
        /// <param name="scale">Der Skalierungsfaktor.</param>
        /// <returns>Die neue Position nach der Skalierung.</returns>
        public static Position operator *(Position p1, int scale) {
            return new Position(p1.X * scale, p1.Y * scale);
        }

        /// <summary>
        /// Skaliert die Position mit einem einzelnen Skalierungsfaktor für beide Koordinaten.
        /// </summary>
        /// <param name="scale">Der Skalierungsfaktor für beide Koordinaten (X und Y).</param>
        public void Scale(float scale) {
            X = Convert.ToInt32(X * scale);
            Y = Convert.ToInt32(Y * scale);
        }

        /// <summary>
        /// Skaliert die Position mit unterschiedlichen Skalierungsfaktoren für die X- und Y-Koordinaten.
        /// </summary>
        /// <param name="scaleX">Der Skalierungsfaktor für die X-Koordinate.</param>
        /// <param name="scaleY">Der Skalierungsfaktor für die Y-Koordinate.</param>
        public void Scale(float scaleX, float scaleY) {
            X = Convert.ToInt32(X * scaleX);
            Y = Convert.ToInt32(Y * scaleY);
        }

        /// <summary>
        /// Gibt die Position als String im Format "X: <X-Wert>  Y: <Y-Wert>" zurück.
        /// </summary>
        /// <returns>Die Position als String.</returns>
        public override string ToString() {
            return "X: " + X.ToString() + "  Y: " + Y.ToString();
        }
    }
}
