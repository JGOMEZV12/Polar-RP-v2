namespace Polar.HabboHotel.Pathfinding
{
    public class Vector2D
    {
        // ✅ FIX #1: readonly — Zero no debe ser reasignable desde fuera.
        public static readonly Vector2D Zero = new Vector2D(0, 0);

        public Vector2D() { }

        public Vector2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public int GetDistanceSquared(Vector2D point)
        {
            int dx = X - point.X;
            int dy = Y - point.Y;
            return dx * dx + dy * dy;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2D v)
                return v.X == X && v.Y == Y;
            return false;
        }

        // ✅ FIX #2: Antes: (X + " " + Y).GetHashCode()
        //   • Aloca un string en heap en CADA llamada — GC pressure constante.
        //   • Tiene colisiones de cadena: X=1,Y=12 → "112" == X=11,Y=2 → "112".
        //   Ahora: combinación multiplicativa estándar sin allocaciones.
        public override int GetHashCode()
        {
            unchecked { return X * 397 ^ Y; }
        }

        public override string ToString() => $"{X}, {Y}";

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
    }
}
