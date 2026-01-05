using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;
using System.IO;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Roleplay.Web;

namespace Polar.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TimerDialogueWebEvent class.
    /// </summary>
    class TimerDialogueWebEvent : IWebEvent
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

            string Action = Data.Split(',')[0].Split(':')[1];
            string Timer = Data.Split(',')[1].Split(':')[1];
            string Value = Data.Split(',')[2].Split(':')[1];

            Socket.Send("compose_timer|" + Timer + ",action:" + Action + ",value:" + Value);
        }
    }
}
