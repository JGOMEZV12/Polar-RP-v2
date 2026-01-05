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
using Polar.HabboHotel.Groups;

namespace Polar.HabboRoleplay.Bots.Types
{
    public class GunStoreBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        //private bool CancelWorkMovement = false;

        public GunStoreBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {
            OnDuty = true;
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

            Client.SendWhisper("Si deseas comprar un arma, por favor, parate en uno de los cuadros. Para comprar balas di: balas {cantidad}", 1);
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

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            string MessageType = Message;
            string[] Params = MessageType.Split(' ');

            Weapon purchaseweapon = null;
            foreach (var Weapon in WeaponManager.Weapons.Values)
            {
                if (Message.ToLower() == Weapon.Name.ToLower())
                {
                    Params[0] = "purchaseweapon";
                    purchaseweapon = Weapon;
                }
            }

            if (MessageType.ToLower() == Name)
                GetRoomUser().Chat("Hey " + Client.GetHabbo().Username + ", ¿Necesitas ayudas?", true);
            else
                switch (Params[0].ToLower())
                {
                    #region Weapons List
                    case "gun":
                    case "weapon":
                    case "guns":
                    case "weapons":
                    case "armas":
                        {
                            #region Comodin Conditions
                            Item BTile = null;
                            BTile = Client.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                            if (BTile == null)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Debes acercarte a un despacho para comprar el arma.", 0, 2));
                                return;
                            }
                            #endregion

                            /*string WhisperMessage = "Aquí esta la lista de armas disponibles!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));*/

                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_shop", "openshop");
                            /*StringBuilder WeaponsList = new StringBuilder().Append("--- Armas disponibles en la tienda ---\n\n");
                            foreach (var weapon in WeaponManager.Weapons.Values.OrderBy(x => x.Cost))
                            {
                                WeaponsList.Append("CODIGO: [" + weapon.Name + "] " + weapon.PublicName + " PRECIO: $" + String.Format("{0:N0}", weapon.Cost) +"\n");
                                WeaponsList.Append("NIVEL REQUERIDO: " + weapon.LevelRequirement + "\n");
                                WeaponsList.Append("DAÑO: " + weapon.MinDamage + " - " + weapon.MaxDamage + "\n");
                                WeaponsList.Append("ALCANCE: " + weapon.Range + " Balas: " + weapon.ClipSize + "\n\n");
                            }

                            Client.SendMessage(new MOTDNotificationComposer(WeaponsList.ToString()));*/
                            break;
                        }
                    #endregion

                   /* #region Purchasing Weapons
                    case "purchaseweapon":
                    case "arma":
                        {
                            if (purchaseweapon == null)
                                break;

                            if (Client.GetRoleplay().BankTarget < 1)
                            {
                                string WhisperMessage = "¡Hey usted necesita tener una tarjeta de débito, por seguridad no manejamos dinero en efectivo!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (purchaseweapon.Stock < 1)
                            {
                                if (RoleplayManager.CalledDelivery)
                                {
                                    string WhisperMessage2 = "El hombre de entrega ya está en camino, el arma que pediste no la tenemos!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                    break;
                                }

                                if (RoleplayManager.DeliveryWeapon != null)
                                {
                                    if (!RoleplayManager.CalledDelivery)
                                    {
                                        if (GetRoom() != null && GetRoom().GetRoomItemHandler() != null)
                                        {
                                            var Item = GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.DELIVERY_BOX).FirstOrDefault();

                                            if (Item != null)
                                            {
                                                if (!GetBotRoleplay().WalkingToItem && !GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("deliverywait"))
                                                    GetBotRoleplay().TimerManager.CreateTimer("pickupdelivery", GetBotRoleplay(), 10, true);
                                                else
                                                {
                                                    string WhisperMessage2 = "Dame un momento para abrir la caja";
                                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                                }
                                            }
                                            else
                                            {
                                                string WhisperMessage2 = "El hombre de entrega ya está en camino";
                                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                            }
                                        }
                                    }
                                    break;
                                }

                                string WhisperMessage = "No existen " + purchaseweapon.PublicName + "s Restante en la acción! Déjame llamar rápidamente al hombre de entrega para obtener algunos";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));

                                new Thread(() =>
                                {
                                    if (GetRoomUser() != null)
                                    {
                                        GetRoomUser().Chat("*Agarra su teléfono y llama al hombre de la entrega, ordenando un nuevo stock de " + purchaseweapon.PublicName + "s*", true);
                                        GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                                    }

                                    Thread.Sleep(3000);

                                    if (GetRoomUser() != null)
                                        GetRoomUser().ApplyEffect(0);
                                }).Start();

                                RoleplayManager.CalledDelivery = true;
                                RoleplayManager.DeliveryWeapon = purchaseweapon;

                                GetBotRoleplay().TimerManager.CreateTimer("deliverywait", GetBotRoleplay(), 10, true);
                                return;
                            }
                            else if (Client.GetRoleplay().OwnedWeapons.ContainsKey(purchaseweapon.Name) && Client.GetRoleplay().OwnedWeapons[purchaseweapon.Name].CanUse)
                            {
                                string WhisperMessage = "Ya tienes un " + purchaseweapon.PublicName + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            else
                            {
                                int Cost = (!Client.GetRoleplay().OwnedWeapons.ContainsKey(purchaseweapon.Name) ? purchaseweapon.Cost : purchaseweapon.CostFine);
                                bool HasOffer = false;
                                if (Client.GetRoleplay().BankChequings >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                                            HasOffer = true;
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Ofrece un " + purchaseweapon.PublicName + " a " + Client.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer(purchaseweapon.Name.ToLower(), 0, Cost, this);
                                        Client.SendWhisper("Se le ha ofrecido un " + purchaseweapon.PublicName + " por $" + String.Format("{0:N0}", Cost) + "! diga ':aceptar arma' para comprarla", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Ya te han ofrecido un arma, ¡no puedo ofrecerte otra!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "No puedes permitirte un " + purchaseweapon.PublicName + "!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }

                            }
                        }
                    #endregion*/

                    #region Bullets
                    case "bullets":
                    case "balas":
                        {
                            int Amount;

                            if (Client.GetRoleplay().EquippedWeapon == null)
                            {
                                string WhisperMessage = "¡Tienes que tener equipada un arma para comprar sus balas!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            if (Client.GetRoleplay().EquippedWeapon.Name == "martillo" || Client.GetRoleplay().EquippedWeapon.Name == "cuchillo" || Client.GetRoleplay().EquippedWeapon.Name == "poder" ||
                                Client.GetRoleplay().EquippedWeapon.Name == "bate" || Client.GetRoleplay().EquippedWeapon.Name == "sukhoi")
                            {
                                string WhisperMessage = "¡No es un arma que posea balas!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            if (Params.Length == 1)
                            {
                                string WhisperMessage = "¡Por favor ingrese la cantidad de puntos que desea comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            else if (!int.TryParse(Params[1], out Amount))
                            {
                                string WhisperMessage = "¡Por favor ingrese una cantidad válida de balas con la que desea comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (Amount < 10)
                            {
                                string WhisperMessage = "¡Necesitas comprar al menos 10 balas a la vez!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (Client.GetRoleplay().BankTarget < 1)
                            {
                                string WhisperMessage = "¡Hey usted necesita tener una tarjeta de débito, por seguridad no manejamos dinero en efectivo!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Convert.ToInt32(Math.Floor((double)Amount / 2));
                                bool HasOffer = false;

                                if (Client.GetRoleplay().BankChequings >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "balas")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Ofrece" + String.Format("{0:N0}", Amount) + " balas para " + Client.GetHabbo().Username + " por $" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("balas", 0, Amount, this);
                                        Client.SendWhisper("Acaba de ofrecerte " + String.Format("{0:N0}", Amount) + " balas por $" + String.Format("{0:N0}", Cost) + "! diga ':aceptar balas' para comprar", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Lo siento, pero ya le han ofrecido balas";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Lo siento, pero no puede comprar balas su cuenta bancaria no tiene fondos";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                   /* #region Reparar Arma
                    case "reparacion":
                    case "reparar":
                        {
                            #region Armero
                            #region Conditions Arm
                            int Price = Convert.ToInt32(Math.Floor((double)250 / 2));
                            foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == "fix_armero")
                                {
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "¡Ya tienes una reparación de arma pendiente!", 0, 2));
                                    return;
                                }
                            }
                            if (Client.GetRoleplay().EquippedWeapon == null)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "No llevas ningún arma equipada a ser reparada.", 0, 2));
                                return;
                            }
                            if (Client.GetRoleplay().WLife > 0)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Esa arma no necesita una reparación.", 0, 2));
                                return;
                            }
                            if (Client.GetRoleplay().ArmPiecesTo <= 0 || Client.GetRoleplay().ArmUserTo != Client.GetHabbo().Id)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Primero debes revisar el arma. Di: 'revisar'", 0, 2));
                                return;
                            }
                            if (Client.GetRoleplay().ArmPieces < Client.GetRoleplay().ArmPiecesTo)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Necesitas " + Client.GetRoleplay().ArmPiecesTo + " pieza(s) para reparar esa arma.", 0, 2));
                                return;
                            }
                            #endregion

                            #region Execute
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, " * Ofrece una reparación de Arma a " + Client.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 0, 2));
                            Client.GetRoleplay().OfferManager.CreateOffer("fix_armero", 0, Price, this);
                            Client.SendWhisper("Te han ofrecido una reparación de Arma por $" + String.Format("{0:N0}", Price) + ". Escribe ':aceptar reparacion' para aceptarla o en su defecto ':rechazar reparacion'.", 1);
                            break;
                            #endregion
                            #endregion
                        }
                    #endregion

                    #region Revisar Arma
                    case "revisar":
                        {
                            #region Conditions Arm
                            int Price = Convert.ToInt32(Math.Floor((double)250 / 2));
                            if (Client.GetRoleplay().EquippedWeapon == null)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "No llevas ningún arma equipada a ser reparada.", 0, 2));
                                return;
                            }
                            if (Client.GetRoleplay().WLife > 0)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Esa arma no necesita una reparación.", 0, 2));
                                return;
                            }
                            #endregion

                            #region Execute
                            int NeedPieces = Client.GetRoleplay().EquippedWeapon.CostFine / 2;
                            Client.GetRoleplay().ArmPiecesTo = NeedPieces;
                            Client.GetRoleplay().ArmUserTo = Client.GetHabbo().Id;
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "*Observa el arma de " + Client.GetHabbo().Username + " y procede a examinarla*", 0, 2));
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Esta arma necesita " + NeedPieces + " pieza(s) para ser reparada.", 0, 2));
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Para obtener las piezas, compra materiales diciendo 'materiales [cantidad]'.", 0, 2));
                            break;
                            #endregion
                        }
                    #endregion
                        */

                    #region Materiales
                    case "materiales":
                        {
                            if (!GroupManager.HasJobCommand(Client, "armero"))
                            {
                                Client.SendWhisper("Debes tener el trabajo de Armero para usar ese comando.", 1);
                                return;
                            }

                            #region Conditions Arm
                            
                            if (Params.Length == 1)
                            {
                                string WhisperMessage = "¡Por favor ingrese la cantidad de materiales que desea comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }

                            int Cant = Convert.ToInt32(Params[1]);
                            int Price = Cant * RoleplayManager.ArmMatPrice;
                            if ((Client.GetRoleplay().ArmMat + Cant) > RoleplayManager.ArmMatLimit)
                            {
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Solo puedes llevar " + RoleplayManager.ArmMatLimit + " materiales en tu inventario.", 0, 2));
                                return;
                            }
                            // Does user has more credits than Price
                            if (Client.GetHabbo().Credits < Price)
                            {
                                Client.SendWhisper("Necesitas al menos $" + Price + " para comprar 50 materiales.", 1);
                                return;
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Client, "*Compra " + Cant + " materiales y paga $" + Price + " por ellos*", 5);
                            //Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Ahora para crear piezas. Di: 'priezas'.", 0, 2));
                            Client.GetHabbo().Credits -= Price;
                            Client.GetHabbo().UpdateCreditsBalance();
                            Client.GetRoleplay().ArmMat += Cant;
                            break;
                            
                            #endregion
                        }
                    #endregion

                   /* #region Piezas
                    case "piezas":
                        {
                            #region Conditions Arm
                            int Pieces = Client.GetRoleplay().ArmMat;

                            if (Client.GetRoleplay().ArmMat <= 0)
                            {
                                Client.SendWhisper("No tienes materiales para crear piezas.", 1);
                                return;
                            }

                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "*Usa " + Client.GetRoleplay().ArmMat + " materiales para crear " + Pieces + " piezas*", 0, 2));
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, "Ahora di: 'reparar' para proceder a reparar tu arma.", 0, 2));
                            //Client.SendWhisper("¡Bien hecho! Ahora usa el comando ':armas' para ver un listado de ellas y la Cantidad de Piezas que requieren para ser creadas con ':crear [nombre-del-arma]'.", 1);
                            Client.GetRoleplay().ArmPieces += Pieces;
                            Client.GetRoleplay().ArmMat = 0;
                            //RoleplayManager.JobSkills(Session, Session.GetRoleplay().JobId, Session.GetRoleplay().ArmLvl, Session.GetRoleplay().ArmXP);
                            //Client.GetRoleplay().CooldownManager.CreateCooldown("crear", 1000, 3);
                            break;

                            #endregion
                        }
                        #endregion*/


                }
        }

        public override void StopActivities()
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("trabajar"))
                GetBotRoleplay().TimerManager.ActiveTimers["trabajar"].EndTimer();

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("pickupdelivery"))
                GetBotRoleplay().TimerManager.ActiveTimers["pickupdelivery"].EndTimer();

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("deliverywait"))
                GetBotRoleplay().TimerManager.ActiveTimers["deliverywait"].EndTimer();

            GetRoomUser().Chat("Bien, he culminado mi turno. ¡Nos vemos!", true);
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