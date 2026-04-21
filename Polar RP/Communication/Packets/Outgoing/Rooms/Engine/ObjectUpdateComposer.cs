using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.Utilities;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Users;


namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ObjectUpdateComposer : ServerPacket
    {
        public Item Item { get; }
        public int UserId { get; }

        public ObjectUpdateComposer(Item item, int userId)
            : base(ServerPacketHeader.ObjectUpdateMessageComposer)
        {
            this.Item = item;
            this.UserId = userId;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Item.Id);
            packet.WriteInteger(Item.GetBaseItem().SpriteId);
            packet.WriteInteger(Item.GetX);
            packet.WriteInteger(Item.GetY);
            packet.WriteInteger(Item.Rotation);
            packet.WriteString(TextHandling.GetString(Item.GetZ));

            ItemBehaviourUtility.GenerateExtradata(Item, packet);

            packet.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            packet.WriteInteger(UserId);
        }
    }
}
