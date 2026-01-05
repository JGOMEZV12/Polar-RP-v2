using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.Communication.Packets.Outgoing.Rooms.Session;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Users;

namespace Polar.Communication.Packets.Incoming.Navigator
{
    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;


            int OldRoom = Session.GetHabbo().HomeRoom;

            if (OldRoom <= 0)
                OldRoom = 1;

           // Session.SendMessage(new RoomForwardComposer(1));
            //RoleplayManager.SendUserNew(Session, OldRoom);
            /*
            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;

                if (!PolarEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                    return;


                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);

                if (Session.GetRoleplay().ATMRobbery == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo el robo fue cancelado.");
                    Session.GetRoleplay().ATMRobTimer.stopTimer();
                    Session.GetRoleplay().ATMRobbery = false;
                }

                if (Session.GetRoleplay().RobartiendaRobbery == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo el robo de la tienda fue cancelado.");
                    Session.GetRoleplay().RobartiendaTimer.stopTimer();
                    Session.GetRoleplay().RobartiendaRobbery = false;
                }

                if (Session.GetRoleplay().Robbery == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo el robo del banco fue cancelado.");
                    Session.GetRoleplay().bankRobTimer.stopTimer();
                    Session.GetRoleplay().Robbery = false;
                }

                if (Session.GetRoleplay().Learning == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo la lectura ha sido cancelada.");
                    Session.GetRoleplay().learningTimer.stopTimer();
                    Session.GetRoleplay().Learning = false;
                }

            }
            else
            {
                if (Session.GetRoleplay().ATMRobbery == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo el robo fue cancelado.");
                    Session.GetRoleplay().ATMRobTimer.stopTimer();
                    Session.GetRoleplay().ATMRobbery = false;
                }

                if (Session.GetRoleplay().Robbery == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo el robo del banco fue cancelado.");
                    Session.GetRoleplay().bankRobTimer.stopTimer();
                    Session.GetRoleplay().Robbery = false;
                }

                if (Session.GetRoleplay().Learning == true)
                {
                    Session.SendWhisper("Has salido de sala, por tal motivo la lectura ha sido cancelada.");
                    Session.GetRoleplay().learningTimer.stopTimer();
                    Session.GetRoleplay().Learning = false;
                }
            }*/
        }
    }
}
