using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemsComposer : ServerPacket
    {
        public ItemsComposer(Item[] Objects, Room Room)
            : base(ServerPacketHeader.ItemsMessageComposer)
        {
            var owners = new Dictionary<int, string>();
            foreach (var item in Objects)
            {
                if (item == null) continue;
                if (!owners.ContainsKey(item.UserID))
                    owners.Add(item.UserID, item.Username);
            }

            base.WriteInteger(owners.Count);
            foreach (var owner in owners)
            {
                base.WriteInteger(owner.Key);
                base.WriteString(owner.Value);
            }

            base.WriteInteger(Objects.Length);

            foreach (Item Item in Objects)
            {
                WriteWallItem(Item, Item.UserID);
            }
        }

        private void WriteWallItem(Item Item, int UserId)
        {
            this.WriteString(Item.Id.ToString());
            this.WriteInteger(Item.Data.SpriteId);
            this.WriteString(Item.wallCoord ?? string.Empty);
            ItemBehaviourUtility.GenerateWallExtradata(Item, (ServerPacket)this);
            this.WriteInteger(Item.Data.Modes > 1 ? 1 : 0);
            this.WriteInteger(UserId);
        }
    }
}
