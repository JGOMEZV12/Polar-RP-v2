using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.Utilities;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing;

namespace Polar.Communication.Packets.Outgoing.Rooms.Engine
{
    class ObjectsComposer : ServerPacket
    {
        public ObjectsComposer(Item[] Objects, Room Room)
            : base(ServerPacketHeader.ObjectsMessageComposer)
        {
            var owners = new Dictionary<int, string>();
            var filteredItems = new List<Item>();

            foreach (var item in Objects)
            {
                if (item == null) continue;
                if (Room.HideWired && item.IsWired) continue;

                filteredItems.Add(item);

                if (!owners.ContainsKey(item.UserID))
                    owners.Add(item.UserID, item.Username);
            }

            base.WriteInteger(owners.Count);
            foreach (var owner in owners)
            {
                base.WriteInteger(owner.Key);
                base.WriteString(owner.Value);
            }

            base.WriteInteger(filteredItems.Count);
            foreach (var item in filteredItems)
            {
                WriteFloorItem(item, item.UserID);
            }
        }

        private void WriteFloorItem(Item Item, int UserID)
        {
            base.WriteInteger(Item.Id);
            base.WriteInteger(Item.GetBaseItem().SpriteId);
            base.WriteInteger(Item.GetX);
            base.WriteInteger(Item.GetY);
            base.WriteInteger(Item.Rotation);
            base.WriteString(TextHandling.GetString(Item.GetZ));

            ItemBehaviourUtility.GenerateExtradata(Item, this);

            base.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            base.WriteInteger(UserID);
        }
    }
}
