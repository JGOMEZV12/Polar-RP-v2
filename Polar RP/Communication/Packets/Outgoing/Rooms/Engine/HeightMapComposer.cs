using System;
using Polar.HabboHotel.Rooms;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class HeightMapComposer : ServerPacket
    {
        public HeightMapComposer(DynamicRoomModel model)
            : base(ServerPacketHeader.HeightMapMessageComposer)
        {
            WriteInteger(model.MapSizeX);
            WriteInteger(model.MapSizeX * model.MapSizeY);

            for (int y = 0; y < model.MapSizeY; y++)
            {
                for (int x = 0; x < model.MapSizeX; x++)
                {
                    if (model.SqState[x, y] == SquareState.BLOCKED)
                    {
                        WriteShort(-1);
                    }
                    else
                    {
                        WriteShort((short)(model.SqFloorHeight[x, y] * 256));
                    }
                }
            }
        }
    }
}
