using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemAddComposer : ServerPacket
    {
        public ItemAddComposer(Item Item)
            : base(ServerPacketHeader.ItemAddMessageComposer)
        {
       
            base.WriteString(Item.Id.ToString());
            base.WriteInteger(Item.GetBaseItem().SpriteId);
           base.WriteString(Item.wallCoord != null ? Item.wallCoord : string.Empty);

            ItemBehaviourUtility.GenerateWallExtradata(Item, this);

            base.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            base.WriteInteger(Item.UserID);
        }
    }
}