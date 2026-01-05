using System;
using System.Linq;
using System.Text;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Rooms.AI;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserChangeComposer : ServerPacket
    {
        public RoomUser User { get; }
        public bool Self { get; }

        public RoomBot BotData { get; }
        public int VirtualId { get; }

        public UserChangeComposer(RoomUser User, bool Self)
            : base(ServerPacketHeader.UserChangeMessageComposer)
        {
            this.User = User;
            this.VirtualId = User.VirtualId;
            this.Self = Self;
            Compose(this);
        }

        public UserChangeComposer(int VirtualId, RoomBot BotData)
            : base(ServerPacketHeader.UserChangeMessageComposer)
        {
            this.VirtualId = VirtualId;
            this.BotData = BotData;
        }

        public void Compose(ServerPacket packet)
        {
            if (BotData == null)
            {
                packet.WriteInteger((Self) ? -1 : VirtualId);
                packet.WriteString(User.GetClient().GetHabbo().Look);
                packet.WriteString(User.GetClient().GetHabbo().Gender);
                packet.WriteString(User.GetClient().GetHabbo().Motto);
                packet.WriteInteger(User.GetClient().GetHabbo().GetStats().AchievementPoints);
            }
            else
            {
                packet.WriteInteger(VirtualId);
                packet.WriteString(BotData.Look);
                packet.WriteString(BotData.Gender);
                packet.WriteString(BotData.Motto);
                packet.WriteInteger(0);
            }

        }
    }
}