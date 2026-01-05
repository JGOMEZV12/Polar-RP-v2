using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;
using System.IO;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// HtmlPageWebEvent class.
    /// </summary>
    class HtmlPageWebEvent : IWebEvent
    {
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = Data.Split(',')[0].Split('|')[1];
            string Page = Data.Split(',')[1].Split('|')[1];

            Socket.Send("compose_htmlpage|" + Page + ",action|" + Action);
        }
    }
}
