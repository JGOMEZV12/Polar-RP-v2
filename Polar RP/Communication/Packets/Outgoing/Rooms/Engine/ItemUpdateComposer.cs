using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Items;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemUpdateComposer : ServerPacket
    {

        public ItemUpdateComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ItemUpdateMessageComposer)
        {
            WriteWallItem(Item, UserId);
        }

        private void WriteWallItem(Item Item, int UserId)
        {
            WriteString(Item.Id.ToString());
            WriteInteger(Item.GetBaseItem().SpriteId);
            WriteString(Item.wallCoord);
            ItemBehaviourUtility.GenerateWallExtradata(Item, this);
            WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            WriteInteger(UserId);
        }
    }
}