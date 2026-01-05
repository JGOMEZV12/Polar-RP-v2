using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms.AI;
using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Items;

namespace Polar.Communication.Packets.Incoming.Catalog
{
    public class GetSellablePetBreedsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string Type = Packet.PopString();
            string PacketType = "";
            ItemData item = PolarEnvironment.GetGame().GetItemManager().GetItemByName(Type);
            if (item == null)
                return;

            int petId = item.BehaviourData;

            Session.SendMessage(new SellablePetBreedsComposer(PacketType, petId, PolarEnvironment.GetGame().GetCatalog().GetPetRaceManager().GetRacesForRaceId(petId)));
        }
    }
}