using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Quests;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Bots.Manager;
using Polar.HabboRoleplay.Bots.Manager.TimerHandlers;
using Polar.HabboRoleplay.Combat;
using Polar.HabboRoleplay.Misc;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.Utilities;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
            if (this.GetBotRoleplay().Motto.Contains("[CAZA]"))
            {
                CryptoRandom Random = new CryptoRandom();
                int Puntos = Random.Next(1, 5);
                int Pieles = Random.Next(1, 3);

                Client.GetRoleplay().HuntPoints += Puntos;
                Client.GetRoleplay().HuntSkins += Pieles;

                RoleplayManager.Shout(Client, "*Ha cazado a " + this.GetBotRoleplay().Name + " y obtiene " + Puntos + " puntos y " + Pieles + " pieles*", 4);
                Client.SendWhisper("Has ganado " + Puntos + " puntos de caza y " + Pieles + " pieles. Total: " + Client.GetRoleplay().HuntPoints + " puntos, " + Client.GetRoleplay().HuntSkins + " pieles.", 1);

                // Desplegar de nuevo después de un tiempo o simplemente eliminarlo
                RoleplayBotManager.EjectDeployedBot(this.GetRoomUser(), this.GetRoom());
            }
        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {
            if (this.GetBotRoleplay().Motto.Contains("[CAZA]"))
            {
                GetBotRoleplay().UserAttacking = Client;

                if (!GetBotRoleplay().ActiveTimers.ContainsKey("attack"))
                {
                    GetBotRoleplay().ActiveTimers.TryAdd("attack", GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id));
                    GetRoomUser().Chat("*Gruñe agresivamente hacia " + Client.GetHabbo().Username + "*", true, 4);
                }
            }
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
            base.OnTimerTick();

            if (this.GetBotRoleplay().Dead || this.GetBotRoleplay().Attacking)
                return;

            if (this.GetBotRoleplay().Motto.Contains("[CAZA]"))
            {
                RoomUser target = FindNearbyTarget();
                if (target != null && target.GetClient() != null)
                {
                    OnAttacked(target.GetClient());
                }
            }
        }

        private RoomUser FindNearbyTarget()
        {
            if (this.GetRoomUser() == null || this.GetRoom() == null) return null;

            return this.GetRoom().GetRoomUserManager().GetRoomUsers()
                .Where(u => !u.IsBot && u.GetClient() != null && u.GetClient().GetRoleplay() != null && !u.GetClient().GetRoleplay().IsDead && !u.GetClient().GetRoleplay().IsNoob)
                .Where(u => RoleplayManager.GetDistanceBetweenPoints2D(this.GetRoomUser().Coordinate, u.Coordinate) <= 2)
                .OrderBy(u => RoleplayManager.GetDistanceBetweenPoints2D(this.GetRoomUser().Coordinate, u.Coordinate))
                .FirstOrDefault();
        }

        public override void StopActivities()
        {

        }

        public override void StartActivities()
        {

        }

    }
}