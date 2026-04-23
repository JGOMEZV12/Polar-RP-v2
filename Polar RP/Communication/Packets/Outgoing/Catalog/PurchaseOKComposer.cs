using Polar.HabboHotel.Catalog;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Catalog.Utilities;

namespace Polar.Communication.Packets.Outgoing.Catalog
{
    public class PurchaseOKComposer : ServerPacket
    {
        public PurchaseOKComposer()
            : base(ServerPacketHeader.PurchaseOKMessageComposer)
        {
            WriteEmptyPurchase();
        }

        public PurchaseOKComposer(CatalogItem item, ItemData baseItem)
            : base(ServerPacketHeader.PurchaseOKMessageComposer)
        {
            if (item == null || baseItem == null)
            {
                WriteEmptyPurchase();
                return;
            }

            WritePurchase(item, baseItem);
        }

        private void WriteEmptyPurchase()
        {
            WriteInteger(0);
            WriteString("");
            WriteBoolean(false);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(1);
            WriteString("s");
            WriteInteger(0);
            WriteString("");
            WriteInteger(1);
            WriteInteger(0);
            WriteString("");
            WriteInteger(1);
        }

        private void WritePurchase(CatalogItem item, ItemData baseItem)
        {
            WriteInteger(item.Id);
            WriteString(item.Name);
            WriteBoolean(false); // rentable
            WriteInteger(item.CostCredits);

            if (item.CostDiamonds > 0)
            {
                WriteInteger(item.CostDiamonds);
                WriteInteger(5); // diamonds
            }
            else
            {
                WriteInteger(item.CostPixels);
                WriteInteger(0); // pixels
            }

            WriteBoolean(ItemUtility.CanGiftItem(item));

            int itemCount = string.IsNullOrEmpty(item.Badge) ? 1 : 2;
            WriteInteger(itemCount);

            // Item Data
            WriteString(baseItem.Type.ToString().ToLower());
            WriteInteger(baseItem.SpriteId);
            WriteString(""); // extra data
            WriteInteger(item.Amount);
            WriteBoolean(item.IsLimited);
            if (item.IsLimited)
            {
                WriteInteger(item.LimitedEditionStack);
                WriteInteger(item.LimitedEditionStack - item.LimitedEditionSells);
            }

            // Badge if any
            if (!string.IsNullOrEmpty(item.Badge))
            {
                WriteString("b");
                WriteString(item.Badge);
            }

            WriteInteger(0); // club level
            WriteBoolean(ItemUtility.CanSelectAmount(item));
            WriteBoolean(true);
            WriteString(""); // preview image
        }
    }
}
