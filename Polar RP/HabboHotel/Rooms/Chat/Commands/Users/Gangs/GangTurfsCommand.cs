using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Misc;
using Polar.HabboRoleplay.Turfs;
using Polar.HabboHotel.Groups;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangTurfsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_turfs"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Proporciona una lista de todos los barrios de pandillas capturables."; }
        }

        public async Task Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("--- Barrios ---\n\n");

            if (PolarEnvironment.GetGame().GetGangTurfsManager().getTurfs().Count == 0)
                Message.Append("No hay barrios\n");

            lock (PolarEnvironment.GetGame().GetGangTurfsManager().getTurfs())
            {
                foreach (var Turf in PolarEnvironment.GetGame().GetGangTurfsManager().getTurfs())
                {
                    if (Turf == null)
                        continue;

                    if (RoleplayManager.GenerateRoom(Turf.RoomId, out Room TurfRoom, false))
                    {
                        Group Gang = GroupManager.GetGang(Turf.GangId);
                        Message.Append(TurfRoom.Name + " [RoomID: " + Turf.RoomId + "] --- Controlado por: " + Gang.Name + "\n");
                        Message.Append("\n");
                    }
                }
            }

            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}