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
            packet.WriteString(Item.GetZ.ToString());
            packet.WriteString(String.Empty);

            if (Item.LimitedNo > 0)
            {
                packet.WriteInteger(1);
                packet.WriteInteger(256);
                packet.WriteString(Item.ExtraData);
                packet.WriteInteger(Item.LimitedNo);
                packet.WriteInteger(Item.LimitedTot);
            }
            else if (Item.Data.InteractionType == InteractionType.INFO_TERMINAL)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                packet.WriteInteger(1);
                packet.WriteString("internalLink");
                packet.WriteString(Item.ExtraData);
            }
            else if (Item.Data.InteractionType == InteractionType.FX_PROVIDER)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                packet.WriteInteger(1);
                packet.WriteString("effectId");
                packet.WriteString(Item.ExtraData);
            }

            else if (Item.Data.InteractionType == InteractionType.PINATA)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(7);
                if (Item.ExtraData.Length <= 0)
                {
                    packet.WriteString("6");
                    packet.WriteInteger(0);
                }
                else
                {
                    packet.WriteString((int.Parse(Item.ExtraData) == 1) ? "8" : "6");
                    packet.WriteInteger(int.Parse(Item.ExtraData));
                }
                packet.WriteInteger(1);
            }
            else if (Item.Data.InteractionType == InteractionType.PINATATRIGGERED)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(7);  // miran2 grafic xq no c acuerda xdddddd kva men xDDDDDDDD esk me mandaron un guasap menju eeeer xqude popddddduddddddddddddddddxdd
                packet.WriteString((Item.ExtraData.Length <= 0) ? "0" : "2");
                if (Item.ExtraData.Length <= 0) packet.WriteInteger(0);
                else packet.WriteInteger(int.Parse(Item.ExtraData));
                packet.WriteInteger(1);
            }
            else if (Item.Data.InteractionType == InteractionType.MAGICEGG)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(7);
                packet.WriteString(Item.ExtraData);
                if (Item.ExtraData.Length <= 0)
                {
                    packet.WriteInteger(0);
                }
                else
                {
                    packet.WriteInteger(int.Parse(Item.ExtraData));
                }
                packet.WriteInteger(23);
            }
            else if (Item.Data.InteractionType == InteractionType.MAGICCHEST)
            {
                packet.WriteInteger(0);
                packet.WriteInteger(7);
                packet.WriteString(Item.ExtraData);
                if (Item.ExtraData.Length <= 0)
                {
                    packet.WriteInteger(0);
                }
                else
                {
                    packet.WriteInteger(int.Parse(Item.ExtraData));
                }
                packet.WriteInteger(1);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(Item, packet);
            }

            packet.WriteInteger(-1); // to-do: check
            packet.WriteInteger(1); //(Item.GetBaseItem().Modes > 1) ? 1 : 0
            packet.WriteInteger(UserId);
        }
    }
}