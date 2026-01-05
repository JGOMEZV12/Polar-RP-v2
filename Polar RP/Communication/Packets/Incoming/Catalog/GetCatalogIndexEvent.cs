using System;
using Polar.Communication.Packets.Incoming;

using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.Communication.Packets.Outgoing.BuildersClub;

namespace Polar.Communication.Packets.Incoming.Catalog
{
    public class GetCatalogIndexEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new CatalogIndexComposer(Session, PolarEnvironment.GetGame().GetCatalog().GetPages(Session, -1)));
            Session.SendMessage(new CatalogItemDiscountComposer());
            Session.SendMessage(new BCBorrowedItemsComposer());
        }
    }
}