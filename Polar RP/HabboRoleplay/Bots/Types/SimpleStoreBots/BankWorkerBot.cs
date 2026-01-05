using System;
using System.Linq;
using System.Text;
using System.Threading;
using Polar.Utilities;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.HabboRoleplay.Misc;
using Polar.HabboRoleplay.Weapons;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;
using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboRoleplay.Combat;
using Polar.HabboHotel.Quests;

namespace Polar.HabboRoleplay.Bots.Types
{
    public class BankWorkerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        //private bool CancelWorkMovement = false;

        public BankWorkerBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {

        }

        public override void OnDeath(GameClient Client)
        {
            int Amount = CombatManager.GetCombatType("fist").GetCoins(null, GetBotRoleplay());

            Client.GetHabbo().Credits += Amount;
            Client.GetHabbo().UpdateCreditsBalance();

            PolarEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);

            Client.GetRoleplay().Kills++;
            Client.GetRoleplay().HitKills++;

            CryptoRandom Random = new CryptoRandom();
            int Multiplier = 1;

            int Chance = Random.Next(1, 101);

            if (Chance <= 16)
            {
                if (Chance <= 8)
                    Multiplier = 3;
                else
                    Multiplier = 2;
            }

            LevelManager.AddLevelEXP(Client, CombatManager.GetCombatType("fist").GetEXP(Client, null, GetBotRoleplay()) * Multiplier);

            if (Amount > 0)
                RoleplayManager.Shout(Client, "*Golpea a " + GetBotRoleplay().Name + ", Derribándolo y robando $" + Amount + " De su cartera*", 6);
            else
                RoleplayManager.Shout(Client, "*Golpea  " + GetBotRoleplay().Name + ", Tirandolo al suelo pero su billetera vacía*", 6);


            GetBotRoleplay().InitiateDeath();
        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

            GetBotRoleplay().UserAttacking = Client;

            if (!GetBotRoleplay().ActiveTimers.ContainsKey("attack"))
            {

                GetBotRoleplay().ActiveTimers.TryAdd("attack", GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id));

                if (GetBotRoleplay().UserAttacking == null)
                    GetRoomUser().Chat("¡Bastardo! te voy a agarrar " + Client.GetHabbo().Username + "!", true, 4);
            }
            else
            {
                if (GetBotRoleplay().ActiveTimers["attack"] == null)
                    GetBotRoleplay().ActiveTimers["attack"] = GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id);
            }

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

            if(Client.GetRoleplay().BankAccount == 0)
            {
                Client.SendWhisper("Para aperturar tu cuenta bancaria, toma asiento y di 'corriente' o 'ahorro' dependiendo que tipo de cuenta deseas escoger.", 1);
            }

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

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().WalkingToItem)
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            if (RespondToSpeech(Client, Message))
                return;

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Hey " + Client.GetHabbo().Username + ", ¿Necesitas algo?", true);
            else
                switch (Message.ToLower())
                {
                    #region Chequings Account
                    case "chequings":
                    case "corriente":
                        {
                            if (Client.GetRoleplay().BankAccount > 0)
                            {
                                string WhisperMessage = "¡Ya tienes una cuenta corriente!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            Message = "corriente";
                            bool HasOffer = false;
                            foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == Message.ToLower())
                                    HasOffer = true;
                            }
                            if (!HasOffer)
                            {
                                GetRoomUser().Chat("*Ofrece abrir una cuenta corriente a " + Client.GetHabbo().Username + " ¡Gratis!*", true);
                                Client.GetRoleplay().OfferManager.CreateOffer("corriente", 0, 0, this);
                                Client.SendWhisper("Acaba de recibir una Cuenta Corriente gratis, Escribe ':aceptar corriente' para activarla", 1);
                                break;
                            }
                            else
                            {
                                string WhisperMessage = "Ya se le ha ofrecido una cuenta corriente";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                        }
                    #endregion

                    #region Savings Account
                    case "savings":
                    case "ahorro":
                        {
                            int Cost = 2500;
                            if (Client.GetRoleplay().BankAccount > 1)
                            {
                                string WhisperMessage = "¡Ya tiene una cuenta de ahorros!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            bool HasOffer = false;
                            if (Client.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Message.ToLower())
                                        HasOffer = true;
                                }
                                if (!HasOffer)
                                {
                                    GetRoomUser().Chat("*Ofrece una cuenta de ahorro a " + Client.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", true);
                                    Client.GetRoleplay().OfferManager.CreateOffer("ahorro", 0, Cost, this);
                                    Client.SendWhisper("Se le ha ofrecido una cuenta de ahorros por $" + String.Format("{0:N0}", Cost) + "! Type ':aceptar ahorro' para activarla", 1);
                                    break;
                                }
                                else
                                {
                                    string WhisperMessage = "Ya se le ha ofrecido una cuenta de ahorros";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                            else
                            {
                                string WhisperMessage = "No puedes pagar una Cuenta de Ahorros, cuestan $" + String.Format("{0:N0}", Cost) + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                        }
                    #endregion
                }
        }

        public override void StopActivities()
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("trabajar"))
                GetBotRoleplay().TimerManager.ActiveTimers["trabajar"].EndTimer();

            GetRoomUser().Chat("Bien,  termina mi turno. ¡Nos vemos!", true);
            OnDuty = false;

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