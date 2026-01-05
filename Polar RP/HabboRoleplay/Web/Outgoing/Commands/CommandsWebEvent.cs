using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using System.IO;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Incoming.Groups;
using Polar.Communication.Packets.Outgoing;
using Polar.Communication.Packets.Incoming;
using Polar.Communication.Packets.Outgoing.Groups;
using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Cache;
using Polar.Communication.Packets.Outgoing.Rooms.Permissions;
using Polar.Database.Interfaces;
using System.Text.RegularExpressions;
using Polar.Communication.Packets.Outgoing.Rooms.Notifications;
using Polar.HabboHotel.Users;
using System.Collections.Concurrent;
using Group = Polar.HabboHotel.Groups.Group;
using Polar.HabboRoleplay.Houses;
using System.Data;
using Polar.Communication.Packets.Outgoing.Rooms.Session;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// CommandsWebEvent class.
    /// </summary>
    class CommandsWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                #region Police CMDS
                case "show_police_cmds":
                    {
                        Socket.Send("compose_commands|show_police_cmds|");
                    }
                    break;
                case "hide_police_cmds":
                    {
                        Socket.Send("compose_commands|hide_police_cmds|");
                    }
                    break;
                #endregion

                #region Map
                case "map":
                    {
                        Socket.Send("compose_commands|map|");
                    }
                    break;
                #endregion

                #region Open
                case "open":
                    {
                        Socket.Send("compose_commands|open|");
                    }
                    break;
                #endregion

                #region Jobs
                case "jobs":
                    {
                        Socket.Send("compose_commands|jobs|");
                        #region Tutorial Step Check
                        if (Client.GetRoleplay().TutorialStep == 32)
                        {
                            Socket.Send("compose_tutorial|32");
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Houses
                case "houses":
                    {
                        Socket.Send("compose_commands|houses|");
                    }
                    break;
                #endregion

                #region Vehicles
                case "vehicles":
                    {
                        Socket.Send("compose_commands|vehicles|");
                    }
                    break;
                #endregion

                #region Empresas
                case "bussines":
                    {
                        Socket.Send("compose_commands|bussines|");
                    }
                    break;
                #endregion

                #region Terrenos
                case "terrains":
                    {
                        Socket.Send("compose_commands|terrains|");
                    }
                    break;
                #endregion

                #region Marihauana
                case "marijane":
                    {
                        Socket.Send("compose_commands|marijane|");
                    }
                    break;
                #endregion

                #region Bandas
                case "gangs":
                    {
                        Socket.Send("compose_commands|gangs|");
                    }
                    break;
                #endregion

                default:
                    break;
            }
        }
    }
}
