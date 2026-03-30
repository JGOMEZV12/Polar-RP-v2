namespace Polar.HabboHotel.Pathfinding
{
    public sealed class PathFinderNode : IComparable<PathFinderNode>
    {
        public Vector2D Position;
        public PathFinderNode? Next;
        public int  Cost     = int.MaxValue;
        public bool InOpen   = false;
        public bool InClosed = false;

        public PathFinderNode(Vector2D position)
        {
            Position = position;
        }

        // ✅ FIX #11: Reset permite reusar nodos si en el futuro se hace pooling
        //   de PathFinderNode (complemento natural del PathFinderMapPool).
        public void Reset()
        {
            Next     = null;
            Cost     = int.MaxValue;
            InOpen   = false;
            InClosed = false;
        }

        public int CompareTo(PathFinderNode? other)
        {
            if (other == null) return 1;
            return Cost.CompareTo(other.Cost);
        }

        public override bool Equals(object? obj) =>
            obj is PathFinderNode n && n.Position.Equals(Position);

        public bool Equals(PathFinderNode? node) =>
            node != null && node.Position.Equals(Position);

        // ✅ FIX #12: GetHashCode delegaba en Position.GetHashCode(), que a su vez
        //   usaba string allocation. Ahora Vector2D.GetHashCode() es correcto y barato
        //   (fix #2), pero se hace explícito aquí para claridad.
        public override int GetHashCode() => Position.GetHashCode();
    }
}
