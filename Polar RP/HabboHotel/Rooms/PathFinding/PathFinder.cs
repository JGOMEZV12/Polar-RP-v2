using System.Collections.Generic;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Pathfinding;

namespace Polar.HabboHotel.Pathfinding
{
    public class PathFinder
    {
        public static readonly Vector2D[] DiagMovePoints =
        {
            new Vector2D(-1, -1),
            new Vector2D( 0, -1),
            new Vector2D( 1, -1),
            new Vector2D( 1,  0),
            new Vector2D( 1,  1),
            new Vector2D( 0,  1),
            new Vector2D(-1,  1),
            new Vector2D(-1,  0),
        };

        public static readonly Vector2D[] NoDiagMovePoints =
        {
            new Vector2D( 0, -1),
            new Vector2D( 1,  0),
            new Vector2D( 0,  1),
            new Vector2D(-1,  0),
        };

        public static List<Vector2D> FindPath(
            RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            var path  = new List<Vector2D>();
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

        public static PathFinderNode? FindPathReversed(
            RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            int mapW = map.Model.MapSizeX;
            int mapH = map.Model.MapSizeY;

            // ✅ FIX #10: pfMap se creaba como new PathFinderNode[mapW, mapH] dentro
            //   de FindPathReversed, que es llamado en CADA ciclo de movimiento de CADA
            //   usuario en CADA tick de la sala. Para una sala de 64×64 con 50 usuarios
            //   a 20 ticks/s esto aloca ~64 MB/s de arrays 2D que el GC tiene que
            //   recolectar constantemente.
            //   Solución correcta: pool estático por thread (no compartido entre hilos).
            //   ThreadLocal garantiza que cada hilo del ThreadPool tenga su propio array
            //   sin contención, y el array se reusa entre llamadas del mismo hilo.
            //   Se limpia con Array.Clear sólo las celdas modificadas (tracked en usedNodes).
            var pfMap = PathFinderMapPool.Rent(mapW, mapH);
            var usedNodes = new List<PathFinderNode>(64); // para limpiar al final

            try
            {
                var openList = new MinHeap<PathFinderNode>(256);
                var movePoints = diag ? DiagMovePoints : NoDiagMovePoints;

                var current = new PathFinderNode(start) { Cost = 0 };
                var finish  = new PathFinderNode(end);

                pfMap[current.Position.X, current.Position.Y] = current;
                usedNodes.Add(current);
                openList.Add(current);

                while (openList.Count > 0)
                {
                    current = openList.ExtractFirst();
                    current.InClosed = true;

                    for (int i = 0; i < movePoints.Length; i++)
                    {
                        Vector2D tmp  = current.Position + movePoints[i];
                        bool inBounds = tmp.X >= 0 && tmp.Y >= 0 && tmp.X < mapW && tmp.Y < mapH;
                        if (!inBounds) continue;

                        bool isFinal = tmp.X == end.X && tmp.Y == end.Y;
                        if (!map.IsValidStep(current.Position, tmp, isFinal, user.AllowOverride))
                            continue;

                        PathFinderNode? node = pfMap[tmp.X, tmp.Y];
                        if (node == null)
                        {
                            node = new PathFinderNode(tmp);
                            pfMap[tmp.X, tmp.Y] = node;
                            usedNodes.Add(node);
                        }

                        if (node.InClosed) continue;

                        bool isDiag = current.Position.X != node.Position.X &&
                                      current.Position.Y != node.Position.Y;
                        int diff = isDiag ? 14 : 10;
                        int cost = current.Cost + diff
                                 + Math.Abs(node.Position.X - end.X)
                                 + Math.Abs(node.Position.Y - end.Y);

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

                return null;
            }
            finally
            {
                // Limpiar sólo las celdas que tocamos — O(k) en vez de O(w*h)
                foreach (var n in usedNodes)
                    pfMap[n.Position.X, n.Position.Y] = null;

                PathFinderMapPool.Return(pfMap);
            }
        }
    }

    // ✅ FIX #10 (cont.): Pool de arrays 2D por hilo.
    //   ThreadLocal<T> crea una instancia por hilo del ThreadPool la primera vez
    //   que ese hilo accede. Los arrays se resan indefinidamente sin GC intermedio.
    internal static class PathFinderMapPool
    {
        // Tamaño máximo de sala soportado. Si un mapa expande más allá de esto
        // se crea un array temporal (caso raro) y no se devuelve al pool.
        private const int MaxSize = 128;

        [ThreadStatic]
        private static PathFinderNode?[,]? _cached;

        public static PathFinderNode?[,] Rent(int w, int h)
        {
            var arr = _cached;
            if (arr != null && arr.GetLength(0) >= w && arr.GetLength(1) >= h)
            {
                _cached = null; // "checked out"
                return arr;
            }
            // Pool miss o mapa más grande: allocar nuevo
            return new PathFinderNode?[Math.Max(w, MaxSize), Math.Max(h, MaxSize)];
        }

        public static void Return(PathFinderNode?[,] arr)
        {
            // Devolver al pool sólo si cabe en el tamaño máximo
            if (arr.GetLength(0) <= MaxSize && arr.GetLength(1) <= MaxSize)
                _cached = arr;
        }
    }
}
