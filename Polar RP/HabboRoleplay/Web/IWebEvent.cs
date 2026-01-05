using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;

namespace Polar.HabboHotel.Roleplay.Web
{
    public interface IWebEvent
    {
        void Execute(GameClient Client, string Data, IWebSocketConnection Socket);
    }
}
