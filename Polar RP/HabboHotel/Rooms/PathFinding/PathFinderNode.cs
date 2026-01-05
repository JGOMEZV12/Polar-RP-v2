using Polar.HabboHotel.Pathfinding;
using System;

namespace Polar.HabboHotel.Pathfinding
{
    public sealed class PathFinderNode : IComparable<PathFinderNode>
    {
        public Vector2D Position;
        public PathFinderNode Next;
        public int Cost = int.MaxValue;
        public bool InOpen = false;
        public bool InClosed = false;

        public PathFinderNode(Vector2D Position)
        {
            this.Position = Position;
        }

        public int CompareTo(PathFinderNode other) => Cost.CompareTo(other.Cost);

        public override bool Equals(object obj) => obj is PathFinderNode && ((PathFinderNode)obj).Position.Equals(Position);

        public bool Equals(PathFinderNode breadcrumb) => breadcrumb.Position.Equals(Position);

        public override int GetHashCode() => Position.GetHashCode();
    }
}
