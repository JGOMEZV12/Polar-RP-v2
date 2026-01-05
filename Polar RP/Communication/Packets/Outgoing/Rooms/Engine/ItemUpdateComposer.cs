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
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.POSTIT:
                    WriteString(Item.ExtraData.Split(' ')[0]);
                    break;

                default:
                    WriteString(Item.ExtraData);
                    break;
            }
            WriteInteger(-1);
            WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            WriteInteger(UserId);
            base.WriteString(Item.Username); // nitro fix
        }
    }
}