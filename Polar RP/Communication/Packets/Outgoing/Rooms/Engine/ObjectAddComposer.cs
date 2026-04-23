using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Items;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Users;

using Polar.Utilities;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ObjectAddComposer : ServerPacket
    {
        public ObjectAddComposer(Item Item, Room Room)
            : base(ServerPacketHeader.ObjectAddMessageComposer)
        {
           
            base.WriteInteger(Item.Id);
            base.WriteInteger(Item.GetBaseItem().SpriteId);
            base.WriteInteger(Item.GetX);
            base.WriteInteger(Item.GetY);
            base.WriteInteger(Item.Rotation);
            base.WriteString(TextHandling.GetString(Item.GetZ));

            ItemBehaviourUtility.GenerateExtradata(Item, this);

            base.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            base.WriteInteger(Item.UserID);
        }
    }
}
