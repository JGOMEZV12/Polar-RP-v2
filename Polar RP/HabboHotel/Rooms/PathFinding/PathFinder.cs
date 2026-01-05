using System;
using System.Collections.Generic;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Pathfinding;
using System.Collections;

namespace Polar.HabboHotel.Pathfinding
{
    public class PathFinder
    {
        public static Vector2D[] DiagMovePoints = new[]
            {
                new Vector2D(-1, -1),
                new Vector2D(0, -1),
                new Vector2D(1, -1),
                new Vector2D(1, 0),
                new Vector2D(1, 1),
                new Vector2D(0, 1),
                new Vector2D(-1, 1),
                new Vector2D(-1, 0)
            };

        public static Vector2D[] NoDiagMovePoints = new[]
            {
                new Vector2D(0, -1),
                new Vector2D(1, 0),
                new Vector2D(0, 1),
                new Vector2D(-1, 0)
            };

        public static List<Vector2D> FindPath(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            var path = new List<Vector2D>();
            var nodes = FindPathReversed(user, diag, map, start, end);
            if (nodes != null)
            {
                path.Add(end);
                while (nodes.Next != null)
                {
                    path.Add(nodes.Next.Position);
                    nodes = nodes.Next;
                }
            }
            return path;
        }

        public static PathFinderNode FindPathReversed(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            var openList = new MinHeap<PathFinderNode>(256);
            var pfMap = new PathFinderNode[map.Model.MapSizeX, map.Model.MapSizeY];
            PathFinderNode node;
            Vector2D tmp;
            int cost;
            int diff;
            var current = new PathFinderNode(start)
            {
                Cost = 0
            };
            var finish = new PathFinderNode(end);
            pfMap[current.Position.X, current.Position.Y] = current;
            openList.Add(current);
            while (openList.Count > 0)
            {
                current = openList.ExtractFirst();
                current.InClosed = true;
                for (var i = 0; diag ? i < DiagMovePoints.Length : i < NoDiagMovePoints.Length; i++)
                {
                    tmp = current.Position + (diag ? DiagMovePoints[i] : NoDiagMovePoints[i]);
                    var isFinalMove = tmp.X == end.X && tmp.Y == end.Y;
                    if (map.IsValidStep(new(current.Position.X, current.Position.Y), tmp, isFinalMove, user.AllowOverride))
                    {
                        if (pfMap[tmp.X, tmp.Y] == null)
                        {
                            node = new(tmp);
                            pfMap[tmp.X, tmp.Y] = node;
                        }
                        else
                            node = pfMap[tmp.X, tmp.Y];
                        if (!node.InClosed)
                        {
                            bool isDiagonal = current.Position.X != node.Position.X && current.Position.Y != node.Position.Y;
                            diff = isDiagonal ? 14 : 10;
                            cost = current.Cost + diff + Math.Abs(node.Position.X - end.X) + Math.Abs(node.Position.Y - end.Y);
                            if (cost < node.Cost)
                            {
                                node.Cost = cost;
                                node.Next = current;
                            }
                            if (!node.InOpen)
                            {
                                if (node.Equals(finish))
                                {
                                    node.Next = current;
                                    return node;
                                }
                                node.InOpen = true;
                                openList.Add(node);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}