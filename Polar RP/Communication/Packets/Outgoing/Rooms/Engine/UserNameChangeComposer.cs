using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserNameChangeComposer : ServerPacket
    {
        public int RoomId { get; }
        public int VirtualId { get; }
        public string Username { get; }

        public UserNameChangeComposer(int RoomId, int VirtualId, string Username)
            : base(ServerPacketHeader.UserNameChangeMessageComposer)
        {
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            this.Username = Username;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            packet.WriteInteger(RoomId);
            packet.WriteInteger(VirtualId);
            packet.WriteString(Username);
        }
    }
}