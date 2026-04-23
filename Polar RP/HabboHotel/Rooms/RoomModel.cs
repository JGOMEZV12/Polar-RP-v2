using System;
using System.Linq;

namespace Polar.HabboHotel.Rooms
{
    public enum SquareState
    {
        OPEN = 0,
        BLOCKED = 1,
        SEAT = 2,
        POOL = 3,
        VIP = 4
    }

    public class RoomModel
    {
        public int DoorOrientation { get; private set; }
        public int DoorX { get; private set; }
        public int DoorY { get; private set; }
        public double DoorZ { get; private set; }
        public int WallHeight { get; private set; }
        public int MapSizeX { get; private set; }
        public int MapSizeY { get; private set; }
        public string Heightmap { get; private set; }
        public bool GotPublicPool { get; private set; }

        public short[,] SqFloorHeight { get; private set; }
        public byte[,] SqSeatRot { get; private set; }
        public SquareState[,] SqState { get; private set; }
        public byte[,] RoomModelFx { get; private set; }

        public RoomModel(string id, int doorX, int doorY, double doorZ, int doorOrientation,
            string heightmap, int wallHeight, string poolmap)
        {
            if (string.IsNullOrEmpty(heightmap))
                throw new ArgumentException("Heightmap cannot be null or empty.", nameof(heightmap));

            DoorX = doorX;
            DoorY = doorY;
            DoorZ = doorZ;
            DoorOrientation = doorOrientation;
            WallHeight = wallHeight;
            Heightmap = heightmap.ToLower();
            GotPublicPool = !string.IsNullOrEmpty(poolmap);

            string[] tmpHeightmap = Heightmap.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Determine MapSizeX by finding the maximum line length
            MapSizeX = tmpHeightmap.Max(line => line.Length);
            MapSizeY = tmpHeightmap.Length;

            SqState = new SquareState[MapSizeX, MapSizeY];
            SqFloorHeight = new short[MapSizeX, MapSizeY];
            SqSeatRot = new byte[MapSizeX, MapSizeY];

            if (GotPublicPool)
                RoomModelFx = new byte[MapSizeX, MapSizeY];

            // Initialize all as BLOCKED
            for (int y = 0; y < MapSizeY; y++)
            {
                for (int x = 0; x < MapSizeX; x++)
                {
                    SqState[x, y] = SquareState.BLOCKED;
                }
            }

            try
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    string line = tmpHeightmap[y];
                    for (int x = 0; x < line.Length; x++)
                    {
                        char square = line[x];
                        if (square != 'x')
                        {
                            SqState[x, y] = SquareState.OPEN;
                            SqFloorHeight[x, y] = Parse(square);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse RoomModel '{id}': {e.Message}", e);
            }
        }

        public static short Parse(char input)
        {
            if (input >= '0' && input <= '9')
                return (short)(input - '0');

            if (input >= 'a' && input <= 'z')
                return (short)(input - 'a' + 10);

            throw new FormatException($"Invalid heightmap character '{input}'. Must be 0-9 or a-z.");
        }

        public static byte ParseByte(char input)
        {
            if (input >= '0' && input <= '9')
                return (byte)(input - '0');

            throw new FormatException($"Invalid byte character '{input}'. Must be 0-9.");
        }

        public void Destroy()
        {
            Heightmap = null;
            SqState = null;
            SqFloorHeight = null;
            SqSeatRot = null;
            RoomModelFx = null;
        }
    }
}
