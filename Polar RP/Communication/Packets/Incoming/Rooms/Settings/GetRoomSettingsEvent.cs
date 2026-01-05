using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.Communication.Packets.Outgoing.Rooms.Settings;

namespace Polar.Communication.Packets.Incoming.Rooms.Settings
{
    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int RoomId = Packet.PopInt();

            if (!HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId, out var Room) || !Room.CheckRights(Session, true))
                return;

            Session.SendMessage(new RoomSettingsDataComposer(Room));
        }
    }
}
