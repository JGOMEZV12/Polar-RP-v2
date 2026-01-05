using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using System.IO;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Incoming.Groups;
using Polar.Communication.Packets.Outgoing;
using Polar.Communication.Packets.Incoming;
using Polar.Communication.Packets.Outgoing.Groups;
using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Cache;
using Polar.Communication.Packets.Outgoing.Rooms.Permissions;
using Polar.Database.Interfaces;
using System.Text.RegularExpressions;
using Polar.Communication.Packets.Outgoing.Rooms.Notifications;
using Polar.HabboRoleplay.Vehicles;
using System.Data;
using Polar.HabboRoleplay.VehicleOwned;
using Polar.HabboRoleplay.Weapons;
using Polar.HabboRoleplay.RoleplayUsers.Offers;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// WeaponsWebEvent class.
    /// </summary>
    class WeaponsWebEvent : IWebEvent
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
            /*
            if (!Client.GetRoleplay().UsingAtm)
            {
                Client.SendNotification("Buen intento, tratando de injectar el systema, ve a un ATM!");
                return;
            }
            */

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);
        

            switch (Action)
            {
                #region Open Shop
                case "openshop":
                    {
                        #region Conditions & Vars
                        if (!RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId, out Room Room))
                            return;

                        if (Client.GetRoleplay().TryGetCooldown("openshopweapon"))
                            return;
                        #endregion

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes acercarte al despacho para comprar el arma.", 1);
                            return;
                        }
                        #endregion

                        Client.GetRoleplay().ViewWeaponsList = true;

                        #region HTML
                        string html = "";
                        if (Client.GetHabbo().VIPRank > 0 && Client.GetHabbo().GetClubManager().HasSubscription("habbo_vip"))
                        {
                            foreach (var weapon in WeaponManager.Weapons.Values.OrderBy(x => x.Cost).Where(x => x.isVip == true))
                            {

                                html += "<div class=\"ft1\">";
                                html += "<div class=\"circular-box\"><img src=\"" + RoleplayManager.CdnURL2 + "/armas/" + weapon.Name + ".png\" /></div>";
                                html += "<div class=\"datos2\"><span>" + weapon.PublicName + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "<span>" + weapon.Name + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Nivel: <span> " + weapon.LevelRequirement + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Daño: <span>" + weapon.MaxDamage + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Rango: <span>" + weapon.Range + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "Balas: <span>" + weapon.ClipSize + "</span>";
                                html += "<div class=\"hr2\"></div>";
                                html += "<span style=\"color:#339900\"><font color=\"orange\">" + String.Format("{0:N0}", weapon.Cost) + " RB</font></span><br><div id=\"" + weapon.Name + "," + weapon.PublicName + "\" class=\"shopweapon\"></div>";
                                html += "</div>";
                                html += "</div>";
                            }
                        }


                        foreach (var weapon in WeaponManager.Weapons.Values.OrderBy(x => x.Cost).Where(x => x.isVip == false))
                        {

                            html += "<div class=\"ft1\">";
                            html += "<div class=\"circular-box\"><img src=\"" + RoleplayManager.CdnURL2 + "/armas/" + weapon.Name + ".png\" /></div>";
                            html += "<div class=\"datos2\"><span>" + weapon.PublicName + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "<span>" + weapon.Name + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "Nivel: <span>" + weapon.LevelRequirement + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "Daño: <span>" + weapon.MaxDamage + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "Rango: <span>" + weapon.Range + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "Balas: <span>" + weapon.ClipSize + "</span>";
                            html += "<div class=\"hr2\"></div>";
                            html += "<span style=\"color:#339900\"><font color=\"orange\">$" + String.Format("{0:N0}", weapon.Cost) + "</font></span><br><div id=\"" + weapon.Name + "," + weapon.PublicName + "\" class=\"shopweapon\"></div>";
                            html += "</div>";
                            html += "</div>";
                        }
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_shop_weapons|openshop|" + SendData);
                        Client.GetRoleplay().CooldownManager.CreateCooldown("openshopweapon", 1000, 1);
                        break;
                    }
                #endregion

                #region Close Shop
                case "closeshop":
                    {
                        Client.GetRoleplay().ViewWeaponsList = false;
                        Socket.Send("compose_shop_weapons|closeshop|");
                        break;
                    }
                #endregion

                #region Buy Weapon
                case "shop":
                    {
                        if (!RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId, out Room Room))
                            return;

                        Weapon weapon = null;
                        #region Conditions & Vars
                        if (Client.GetRoleplay().TryGetCooldown("buy"))
                            return;

                        string[] ReceivedData = Data.Split(',');
                        int GetEffect;
                        string GetCarModel = ReceivedData[1];
                        weapon = WeaponManager.getWeapon(GetCarModel);

                        if(weapon == null)
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|Ha ocurrido un problema al obtener la Información del Arma. [2]");
                            return;
                        }

                        if (Client.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name.ToLower()))
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|¡Ya tienes está arma!.");
                            return;
                        }

                        if (weapon.Cost > 100)
                        {
                            if (Client.GetRoleplay().BankChequings < weapon.Cost)
                            {
                                Socket.Send("compose_shop_weapons|shopmsg|No tienes dinero suficiente para comprar esa arma.");
                                return;
                            }
                        }
                        else
                        {
                            if (Client.GetHabbo().Diamonds < weapon.Cost)
                            {
                                Socket.Send("compose_shop_weapons|shopmsg|No tienes los Rubies suficientes para comprar esa arma.");
                                return;
                            }
                        }

                        if (weapon.Stock < 1)
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|Lo sentimos, pero esta arma se ha agotado");
                            return;
                        }
                        if (Client.GetRoleplay().BankTarget < 1)
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|Lo sentimos, pero aquí solo aceptamos débito y usted no tiene tarjeta, vaya al banco y solicite");
                            return;
                        }
                        if (Client.GetRoleplay().BankChequings < weapon.Cost)
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|¡Lo siento, no puedes pagar una " + weapon.PublicName + " no tiene dinero en su cuenta bancaria!");
                            return;
                        }
                        if (Client.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name))
                        {
                            Socket.Send("compose_shop_weapons|shopmsg|Ya posees está arma, escoge otra.");
                            return;
                        }
                        #endregion

                        #region Execute
                        if (weapon.Cost > 100)
                        {
                            //Client.GetHabbo().Credits -= weapon.Cost;
                            Client.GetRoleplay().BankChequings -= weapon.Cost;
                            Socket.Send("compose_atm|change_balance_1|" + Client.GetRoleplay().BankChequings);
                            Client.GetHabbo().UpdateCreditsBalance();
                            RoleplayManager.Shout(Client, "*Compra un " + weapon.PublicName + " y paga $" + String.Format("{0:N0}", weapon.Cost) + "*", 5);
                            Socket.Send("compose_atm|change_balance_1|" + Client.GetRoleplay().BankChequings);
                        }
                        else
                        {
                            //Client.GetHabbo().Credits -= weapon.Cost;
                            Client.GetHabbo().Diamonds -= weapon.Cost;
                            Client.GetHabbo().UpdateDiamondsBalance();
                            RoleplayManager.Shout(Client, "*Compra un " + weapon.PublicName + " y paga " + String.Format("{0:N0}", weapon.Cost) + " rubies*", 5);
                        }

                        int CAmount = (weapon.Cost * 2) / 100;
                        RoleplayManager.GiveMoneyToCompany(5, Client, "ammunation", true, CAmount);
                        WeaponManager.Weapons[weapon.Name].Stock--;
                        RoleplayManager.AddWeapon(Client, weapon);
                        Client.GetRoleplay().ClearWebSocketDialogue();
                        Client.GetRoleplay().RefreshStatDialogue();
                        Client.GetRoleplay().CooldownManager.CreateCooldown("buy", 1000, 15);

                        Client.GetRoleplay().UpdateInteractingUserDialogues();
                        Client.GetRoleplay().RefreshStatDialogue();
                        break;
                        #endregion
                    }
                #endregion

            }
        }
    }
}
