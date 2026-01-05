using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using Polar.Utilities;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboRoleplay.Combat;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Quests;
using Polar.HabboRoleplay.Bots.Manager.TimerHandlers;
using static Polar.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;

namespace Polar.HabboRoleplay.Bots.PetBots
{
    public class PetBot : RoleplayBotAI
    {
        int VirtualId;
       
        public PetBot(int VirtualId)
        {
            this.VirtualId = VirtualId;
        }

        public override void OnDeployed(GameClient Client)
        {

            this.GetRoomUser().Chat("hello " + Client);
            this.GetBotRoleplay().MoveRandomly();

        }

        public override void OnDeath(GameClient Client)
        {

        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
          
        }

        public override void OnUserEnterRoom(GameClient Client)
        {
           
        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
           
        }

        public override void OnMessaged(GameClient Client, string Message)
        {
            
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            
        }

        public override void OnTimerTick()
        {

        }

        public override void StopActivities()
        {
           
        }

        public override void StartActivities()
        {
          
        }

    }
}