using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.GameClients;
using System.IO;
using Polar.HabboHotel.Roleplay.Web;


namespace Polar.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// RetrieveUStatsWebEvent class.
    /// </summary>
    class RetrieveUStatsWebEvent : IWebEvent
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

            if (Client.GetRoleplay().TargetLock)
                return;


            string UserID = Data;
            GameClient TargetClient = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(UserID);

            if (TargetClient == null)
                return;

            string CachedTargetString = GetUserComponent.ReturnUserStatistics(TargetClient);
            if (String.IsNullOrEmpty(CachedTargetString))
                return;

            Socket.Send("compose_characterbar|" + CachedTargetString);
            #region Hospital
            Socket.Send("compose_hospital|close_actionbtn|");
            if (GroupManager.HasJobCommand(Client, "reviewhosp"))
            {
                if (Client.GetRoleplay().IsWorking)
                {
                    // In Hospital
                    if (TargetClient.GetRoleplay().IsDead && !TargetClient.GetRoleplay().BeingHealed)
                    {
                        // Revisar
                        if (Client.GetRoleplay().RevisPaci != TargetClient.GetHabbo().Id)
                        {
                            Socket.Send("compose_hospital|open_actionbtn|Revisar|revisar");
                        }
                        // Atender
                        else
                        {
                            Socket.Send("compose_hospital|open_actionbtn|Atender|atender");
                        }
                    }
                }
            }
            #endregion

        }
    }
}
