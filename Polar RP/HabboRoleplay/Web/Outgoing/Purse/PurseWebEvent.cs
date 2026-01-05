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
using System.Data;
using Polar.HabboRoleplay.VehicleOwned;
using Polar.HabboRoleplay.Weapons;
using Polar.HabboRoleplay.RoleplayUsers.Offers;
using Polar.HabboHotel.Global;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Purse
{
    /// <summary>
    /// WeaponsWebEvent class.
    /// </summary>
    class PurseWebEvent : IWebEvent
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
            DateTime future = DateTime.Now;

            switch (Action)
            {
                #region Credits
                case "credits":
                    {
                        Socket.Send("compose_update_purse|credits|" + (Client.GetHabbo().Credits < 0 ? 0 : Client.GetHabbo().Credits));
                        break;
                    }
                #endregion

                #region Duckets
                case "duckets":
                    {
                        Socket.Send("compose_update_purse|duckets|" + (Client.GetHabbo().Duckets < 0 ? 0 : Client.GetHabbo().Duckets));
                        break;
                    }
                #endregion

                #region Diamonds
                case "diamonds":
                    {
                        Socket.Send("compose_update_purse|diamonds|" + (Client.GetHabbo().Diamonds < 0 ? 0 : Client.GetHabbo().Diamonds));
                        break;
                    }
                #endregion

                #region HC
                case "hc":
                    {
                        Double Expire = Client.GetHabbo().GetClubManager().GetSubscription("habbo_vip").ExpireTime;
                        Double TimeLeft = Expire - PolarEnvironment.GetUnixTimestamp();
                        int TotalDaysLeft = (int)Math.Ceiling(TimeLeft / 86400);
                        future = DateTime.Now.AddDays(TotalDaysLeft);
                        Socket.Send("compose_update_purse|hc|" + TotalDaysLeft);
                        break;
                    }
                    #endregion

            }
        }
    }
}
