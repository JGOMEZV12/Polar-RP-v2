using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;

namespace Polar.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomSettingsDataComposer : ServerPacket
    {
        public Room Room { get; }
        public RoomSettingsDataComposer(Room room)
            : base(ServerPacketHeader.RoomSettingsDataMessageComposer)
        {
            this.Room = room;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Room.RoomId);
           packet.WriteString(Room.Name);
           packet.WriteString(Room.Description);
            packet.WriteInteger(Room.State);
            packet.WriteInteger(Room.Category);
            packet.WriteInteger(Room.UsersMax);
            packet.WriteInteger(((Room.RoomData.Model.MapSizeX * Room.RoomData.Model.MapSizeY) > 100) ? 50 : 25);

            packet.WriteInteger(Room.Tags.Count);
            foreach (string Tag in Room.Tags.ToArray())
            {
               packet.WriteString(Tag);
            }

            packet.WriteInteger(Room.TradeSettings); //Trade
            packet.WriteInteger(Room.AllowPets ? 1 : 0); // allows pets in room - pet system lacking, so always off
            packet.WriteInteger(Room.AllowPetsEating ? 0 : 1);// allows pets to eat your food - pet system lacking, so always off
            packet.WriteInteger(Room.RoomBlockingEnabled ? 0 : 1);
            packet.WriteInteger(Room.Hidewall ? 0 : 1);
            packet.WriteInteger(Room.WallThickness);
            packet.WriteInteger(Room.FloorThickness);

            packet.WriteInteger(Room.chatMode);//Chat mode
            packet.WriteInteger(Room.chatSize);//Chat size
            packet.WriteInteger(Room.chatSpeed);//Chat speed
            packet.WriteInteger(Room.chatDistance);//Hearing Distance
            packet.WriteInteger(Room.extraFlood);//Additional Flood

            packet.WriteBoolean(true);

            packet.WriteInteger(Room.WhoCanMute); // who can mute
            packet.WriteInteger(Room.WhoCanKick); // who can kick
            packet.WriteInteger(Room.WhoCanBan); // who can ban

        }
    }
}
