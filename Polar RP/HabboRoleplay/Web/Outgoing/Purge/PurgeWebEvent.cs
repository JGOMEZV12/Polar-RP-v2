using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Polar.HabboHotel.Items;
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
using Polar.HabboRoleplay.Vehicles;
using Polar.HabboRoleplay.PhoneChat;
using System.Data;
using Polar.HabboHotel.Users.Messenger;
using Polar.Utilities;
using Polar.HabboHotel.Quests;
using Polar.Communication.Packets.Outgoing.Users;
using Polar.HabboRoleplay.Phones;
using Polar.HabboRoleplay.PhoneOwned;
using Polar.HabboRoleplay.PhoneAppOwned;
using Polar.HabboRoleplay.PhonesApps;
using System.Web;
using Polar.Communication.Packets.Incoming.Inventory.Purse;
using Polar.HabboRoleplay.API;
using Polar.HabboRoleplay.PlayInternet;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// PhoneWebEvent class.
    /// </summary>
    class PurgeWebEvent : IWebEvent
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
                #region Open
                case "open":
                    {
                        Socket.Send("compose_purge|open|");
                    }
                    break;
                #endregion


                #region Close
                case "close":
                    {
                        Socket.Send("compose_purge|close|");
                    }
                    break;
                #endregion

                #region Timer
                case "timer":
                    {
                        string[] ReceivedData = Data.Split(',');
                        Socket.Send("compose_purge|timer|" + ReceivedData[1]);
                    }
                    break;
                #endregion

                #region Timer Off
                case "timer_off":
                    {
                        Socket.Send("compose_purge|timer_off|");
                    }
                    break;
                #endregion
            }
        }
    }
}
