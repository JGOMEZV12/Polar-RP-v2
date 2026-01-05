using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using MoreLinq;
using Polar.HabboHotel.Items;
using Polar.Communication.Packets.Outgoing.Inventory.Furni;



namespace Polar.Communication.Packets.Incoming.Inventory.Furni
{
    internal class RequestFurniInventoryEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            IEnumerable<Item> Items = Session.GetHabbo().GetInventoryComponent().GetWallAndFloor;
            bool CraftingCheck = Session.GetRoleplay().CraftingCheck;

            int page = 0;
            int pages = ((Items.Count() - 1) / 700) + 1;

            if (!Items.Any())
            {
                Session.SendMessage(new FurniListComposer(Items.ToList(), 1, 0, CraftingCheck));
            }
            else
            {
                foreach (ICollection<Item> batch in Items.Batch(700))
                {
                    Session.SendMessage(new FurniListComposer(batch.ToList(), pages, page, CraftingCheck));

                    page++;
                }
            }
        }
    }
}
