using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Polar.HabboHotel.Items.Crafting;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Catalog.Utilities;

namespace Polar.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListComposer : ServerPacket
    {
        public int CraftableItemCount = 0;
        public FurniListComposer(ICollection<Item> Items, int pages, int page, bool CraftingCheck)
            : base(ServerPacketHeader.FurniListMessageComposer)
        {
            base.WriteInteger(pages);//Pages
            base.WriteInteger(page);//Page?

            if (CraftingCheck == false)
                base.WriteInteger(Items.Count);
            else
            {
                foreach (var Item in Items.ToList())
                {
                    if (CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        CraftableItemCount++;
                }

                base.WriteInteger(Items.Count - CraftableItemCount);
            }

            foreach (Item Item in Items.ToList())
            {
                if (CraftingCheck)
                {
                    if (!CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        WriteItem(Item, this);
                }
                else
                    WriteItem(Item, this);
            }
        }

        private void WriteItem(Item Item, ServerPacket packet)
        {
            base.WriteInteger(Item.Id);
            base.WriteString(Item.GetBaseItem().Type.ToString().ToUpper());
            base.WriteInteger(Item.Id);
            base.WriteInteger(Item.GetBaseItem().SpriteId);

            if (Item.LimitedNo > 0)
            {
                base.WriteInteger(1);
                base.WriteInteger(256);
                base.WriteString(Item.ExtraData);
                base.WriteInteger(Item.LimitedNo);
                base.WriteInteger(Item.LimitedTot);
            }
            else
                ItemBehaviourUtility.GenerateExtradata(Item, packet);

            base.WriteBoolean(Item.GetBaseItem().AllowEcotronRecycle);
            base.WriteBoolean(Item.GetBaseItem().AllowTrade);
            base.WriteBoolean(Item.LimitedNo == 0 ? Item.GetBaseItem().AllowInventoryStack : false);
            base.WriteBoolean(ItemUtility.IsRare(Item));
            base.WriteInteger(-1);//Seconds to expiration.
            base.WriteBoolean(true);
            base.WriteInteger(-1);//Item RoomId

            if (!Item.IsWallItem)
            {
                base.WriteString(string.Empty);
                base.WriteInteger(0);
            }
        }
    }
}