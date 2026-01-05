using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;
using System.IO;

namespace Polar.HabboHotel.Roleplay.Web.OutGoing.Misc
{
    /// <summary>
    /// CaptchaWebEvent class.
    /// </summary>
    class CaptchaWebEvent : IWebEvent
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

            string Action = Data.Split(',')[0];
            string Title = Data.Split(',')[1];

            switch (Action)
            {
                case "create":
                    Socket.Send("compose_captchabox|" + Data);
                    Client.GetRoleplay().CaptchaSent = true;
                    break;

                case "complete":
                    Client.GetRoleplay().CaptchaSent = false;
                    Client.GetRoleplay().CaptchaTime = 0;
                    break;

                case "regenerate":
                    Client.GetRoleplay().CreateCaptcha(Title);
                    break;
            }
        }
    }
}
