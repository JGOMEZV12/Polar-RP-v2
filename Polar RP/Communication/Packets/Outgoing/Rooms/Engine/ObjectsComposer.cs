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
            base.WriteString(String.Empty);
            base.WriteInteger(0);

            if (Item.LimitedNo > 0)
            {
                base.WriteInteger(1);
                base.WriteInteger(256);
                base.WriteString(Item.ExtraData);
                base.WriteInteger(Item.LimitedNo);
                base.WriteInteger(Item.LimitedTot);
            }
            else if (Item.Data.InteractionType == InteractionType.INFO_TERMINAL)
            {
                base.WriteInteger(0);
                base.WriteInteger(1);
                base.WriteInteger(1);
                base.WriteString("internalLink");
                base.WriteString(Item.ExtraData);
            }
            else if (Item.Data.InteractionType == InteractionType.FX_PROVIDER)
            {
                base.WriteInteger(0);
                base.WriteInteger(1);
                base.WriteInteger(1);
                base.WriteString("effectId");
                base.WriteString(Item.ExtraData);
            }
            else if (Item.Data.InteractionType == InteractionType.PINATA)
            {
                base.WriteInteger(0);
                base.WriteInteger(7);
                base.WriteString("6");
                if (Item.ExtraData.Length <= 0) base.WriteInteger(0);
                else base.WriteInteger(int.Parse(Item.ExtraData));
                base.WriteInteger(100);
            }
            else if (Item.Data.InteractionType == InteractionType.PINATATRIGGERED)
            {
                base.WriteInteger(0);
                base.WriteInteger(7);  // miran2 grafic xq no c acuerda xdddddd kva men xDDDDDDDD esk me mandaron un guasap menju eeeer xqude popddddduddddddddddddddddxdd
                base.WriteString("0");
                if (Item.ExtraData.Length <= 0) base.WriteInteger(0);
                else base.WriteInteger(int.Parse(Item.ExtraData));
                base.WriteInteger(1);
            }
            else if (Item.Data.InteractionType == InteractionType.MAGICEGG)
            {
                base.WriteInteger(0);
                base.WriteInteger(7);
                base.WriteString(Item.ExtraData);
                if (Item.ExtraData.Length <= 0)
                {
                    base.WriteInteger(0);
                }
                else
                {
                    base.WriteInteger(int.Parse(Item.ExtraData));
                }
                base.WriteInteger(23);
            }
            else if (Item.Data.InteractionType == InteractionType.MAGICCHEST)
            {
                base.WriteInteger(0);
                base.WriteInteger(7);
                base.WriteString(Item.ExtraData);
                if (Item.ExtraData.Length <= 0)
                {
                    base.WriteInteger(0);
                }
                else
                {
                    base.WriteInteger(int.Parse(Item.ExtraData));
                }
                base.WriteInteger(1);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(Item, this);
            }

            base.WriteInteger(-1);
            base.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            base.WriteInteger(UserID);
        }
    }
}