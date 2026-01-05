using System;
using Polar.Database.Interfaces;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Handshake;
using Polar.HabboHotel.Users;

namespace Polar.Communication.Packets.Incoming.Handshake
{
    public class UniqueIDEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //string Junk = Packet.PopString();
            string MachineId = Packet.PopString();



            Session.MachineId = MachineId;


            Session.SendMessage(new SetUniqueIdComposer(MachineId));
        }
    }
}