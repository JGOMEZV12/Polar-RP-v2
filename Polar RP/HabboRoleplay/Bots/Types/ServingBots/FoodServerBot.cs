using System;
using System.Linq;
using System.Text;
using System.Threading;
using Polar.Utilities;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.HabboRoleplay.Food;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;
using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboRoleplay.Combat;
using Polar.HabboHotel.Quests;
using System.Collections.Concurrent;
using static Polar.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;
using Polar.HabboRoleplay.Bots.Manager.TimerHandlers;

namespace Polar.HabboRoleplay.Bots.Types
{
    public class FoodServerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        public ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>> ServingQueue = new ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>>();

        public FoodServerBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {
            //OnDuty = false;
            this.StartActivities();
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
            if (!OnDuty)
                return;
        }

        public override void OnUserEnterRoom(GameClient Client)
        {
            if (!OnDuty)
                return;

            if (!GetRoomUser().IsWalking)
            {
                // Look at the user 
            }
        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            if (!OnDuty)
                return;

            if (Client == null) return;
            if (Client.GetRoomUser() == null) return;

            if (Client == GetBotRoleplay().UserFollowing || Client == GetBotRoleplay().UserAttacking)
                GetBotRoleplay().StartTeleporting(GetRoomUser(), GetRoom(), Params);
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            GameClient Client = User.GetClient();

            if (Client == null)
                return;
            HandleRequest(Client, Message);
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            if (User.GetClient() == null)
                return;
            HandleRequest(User.GetClient(), Message);
        }

        public override void OnMessaged(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;
        }

        public override void OnTimerTick()
        {
            IBotHandler ServingHandler;
            if (this.GetBotData().TryGetHandler(Handlers.FOODSERVE, out ServingHandler))
            {
                if (ServingHandler.Active)
                    return;

                ServingHandler.ExecuteHandler(this.ServingQueue);
            }
        }

        public void BeginServingFood(Food.Food Food, GameClient Client)
        {

            #region Checks
            if (!OnDuty)
                return;

            if (Client.GetRoleplay() == null)
                return;

            if (Client.GetRoomUser() == null)
                return;

            if (Client.LoggingOut)
                return;

            if (Client.GetRoleplay().Hunger <= 0)
                return;
            #endregion

            string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

            this.GetBotRoleplay().WalkingToItem = true;
            this.GetRoomUser().Chat("Cosa segura " + Client.GetHabbo().Username + " sirve una porción de " + RealName + " ¡ya viene!", true);

            var UserPoint = new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y);
            var ServePoint = new System.Drawing.Point(Client.GetRoomUser().SquareBehind.X, Client.GetRoomUser().SquareBehind.Y);

            object[] Params = { Client, Food, ServePoint, UserPoint, RealName };

            /*
            IBotHandler ServingHandler;
            if (!this.GetBotRoleplay().TryGetHandler(Handlers.FOODSERVE, out ServingHandler))
                this.GetBotRoleplay().StartHandler(Handlers.FOODSERVE, out ServingHandler, Params);
            else
                ServingHandler.Active = true;
                */

            this.GetRoomUser().MoveTo(ServePoint);

            GetBotRoleplay().TimerManager.CreateTimer("serving", GetBotRoleplay(), 10, true, Params);
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();
            var ServableFoods = FoodManager.GetServableBotItems("food");

            #region Serving Food
            if (Message.ToLower().Contains("servir") && Message.ToLower() != "servir")
            {
                string[] Params = Message.Split(' ');

                if (Params.Length == 1)
                    return;
                else
                {
                    if (Client.GetRoleplay().Hunger <= 0)
                    {
                        string WhisperMessage = "No tienes hambre en absoluto " + Client.GetHabbo().Username + "!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    if (GetBotRoleplay().WalkingToItem)
                    {
                        string WhisperMessage = "¡Ya estoy en mi camino para servir a alguien! Por favor, espere hasta que esté libre.";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    string DesiredFood = Params[1].ToLower();
                    var Food = FoodManager.GetFoodTwo(DesiredFood);

                    if (Food == null)
                    {
                        string WhisperMessage = "¡Esta comida no existe! Por favor escribe 'food' o vea en el tablero azul.";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    }
                    else
                    {
                        if (FoodManager.CanServe(Client.GetRoomUser()))
                            BeginServingFood(Food, Client);
                        else
                        {
                            string WhisperMessage = "Por favor, encuentre una mesa vacía para que le sirva comida a usted!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        }
                    }
                }
            }
            #endregion

            else if (Message.ToLower() == Name)
                GetRoomUser().Chat("Hey " + Client.GetHabbo().Username + ", ¿Necesitas algo?", true);
            else
                switch (Message.ToLower())
                {
                    #region Food List
                    case "food":
                    case "hunger":
                    case "servir":
					case "comida":
                    case "menu":
                        {
                            /*if (Client.GetRoleplay().Hunger <= 0)
                            {
                                string WhisperMessage = "¡No tienes hambre en absoluto " + Client.GetHabbo().Username + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            }
                            else
                            {
                                GetRoomUser().Chat("Bienvenido " + Client.GetHabbo().Username + ", Tienes hambre ¿Qué te gustaría comer?", true);

                                StringBuilder FoodList = new StringBuilder().Append("--- Menu ---\n");
                                FoodList.Append("Para solicitar cualquiera de los siguientes, escriba 'servir <nombrecomida>' Con una mesa vacía delante de usted\n\n");

                                foreach (var Food in ServableFoods.OrderBy(x => x.Cost))
                                {
                                    if (Food != null)
                                    {
                                        string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

                                        FoodList.Append("--- " + RealName + " ---\n");
                                        FoodList.Append("Costo: $" + Food.Cost + "   | Hambre: -" + Food.Hunger + "\n");
                                        FoodList.Append("Salud: +" + Food.Health + " | Energía: +" + Food.Energy + "\n\n");
                                    }
                                }

                                Client.SendMessage(new MOTDNotificationComposer(FoodList.ToString()));
                            }*/
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_restaurant", "openfood");
                            
                        }
                        break;
                        #endregion
                }
        }

        public override void StopActivities()
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("trabajar"))
                GetBotRoleplay().TimerManager.ActiveTimers["trabajar"].EndTimer();

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("serving"))
                GetBotRoleplay().TimerManager.ActiveTimers["serving"].EndTimer();

            GetRoomUser().Chat("He terminado por hoy. ¡Nos vemos!", true);
            OnDuty = false;
            GetBotRoleplay().WalkingToItem = false;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var Point = new System.Drawing.Point(Item.GetX, Item.GetY);
                GetRoomUser().MoveTo(Point);
                GetBotRoleplay().TimerManager.CreateTimer("notrabajar", GetBotRoleplay(), 10, true, null);
            }
        }

        public override void StartActivities()
        {
            if (OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("notrabajar"))
                GetBotRoleplay().TimerManager.ActiveTimers["notrabajar"].EndTimer();

            GetBotRoleplay().Invisible = false;
            GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            GetRoomUser().Chat("Bien, hora de volver al trabajo", true);
            OnDuty = true;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var ItemPoint = new System.Drawing.Point(Item.GetX, Item.GetY);
                if (GetRoomUser().Coordinate == ItemPoint)
                {
                    Item.ExtraData = "2";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(2, true);
                }
            }

            var Point = new System.Drawing.Point(GetBotRoleplay().oX, GetBotRoleplay().oY);
            GetRoomUser().MoveTo(Point);
            GetBotRoleplay().TimerManager.CreateTimer("trabajar", GetBotRoleplay(), 10, true, null);
        }

    }
}