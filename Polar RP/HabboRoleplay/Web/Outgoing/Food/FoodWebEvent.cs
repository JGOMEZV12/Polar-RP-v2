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
using Polar.HabboRoleplay.Food;
using System.Data;
using Polar.HabboRoleplay.Bots.Manager;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// WeaponsWebEvent class.
    /// </summary>
    class FoodWebEvent : IWebEvent
    {
        public void BeginServingFood(Food Food, GameClient Client)
        {
            var BotUser = RoleplayBotManager.GetDeployedBotById(10);

            #region Checks
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

            BotUser.GetBotRoleplay().WalkingToItem = true;
            BotUser.Chat("Cosa segura " + Client.GetHabbo().Username + " sirve una porción de " + RealName + " ¡ya viene!", true);

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

            BotUser.MoveTo(ServePoint);

            BotUser.GetBotRoleplay().TimerManager.CreateTimer("serving", BotUser.GetBotRoleplay(), 10, true, Params);
        }
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
            var BotUser = RoleplayBotManager.GetDeployedBotById(10);

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            
            switch (Action)
            {
                #region Open Food
                case "open":
                    {

                        #region Conditions & Vars

                        string[] ReceivedData = Data.Split(',');
                        var ServableFoods = FoodManager.GetServableBotItems(ReceivedData[1]);
                        #endregion

                        #region Comodin Conditions
                        /*Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes acercarte al despacho para comprar el arma.", 1);
                            return;
                        }*/
                        #endregion


                        #region HTML
                        string html = "";
                        ItemData Datax = null;
                        
                        foreach (var Food in ServableFoods.OrderBy(x => x.Cost))
                        {
                            PolarEnvironment.GetGame().GetItemManager().GetItem(Food.ItemId, out Datax);
                            html += "<div data-balloon=\""+Food.Name+"\" data-balloon-pos=\"right\" id=\"comprarcomida\" class=\"p-1 w-1/2\" style=\"box-sizing: border-box; width: 11%;\">";
                            html += "<div class=\"box p-4 flex items-center cursor-pointer-r hover:bg-dark-3\" style=\"padding: 0.5rem !important;\" id=\"menu-comida\" comida=\""+Food.Name+"\">";
                            html += "<div class=\"flex justify-center items-center\" style=\"width: 50px;height: 50px;\">";
                            html += "<img  src=\"" + RoleplayManager.CDNSWF + "/dcr/hof_furni/" +Datax.ItemName+"_icon.png\" class=\"mr-2 flex-none\" style=\"left: 15%; position: relative;\">";
                            html += "</div>";
                            html += "</div>";
                            html += "</div>";
                        }
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_shop_restaurant|open|" + SendData);
                        break;
                    }
                #endregion

                #region Open Food
                case "openfood":
                    {
                        Socket.Send("compose_shop_restaurant|openfood|");
                        break;
                    }
                #endregion

                #region Close Food
                case "close":
                    {
                        Socket.Send("compose_shop_restaurant|close|");
                        break;
                    }
                #endregion

                #region Buy Food
                case "shop":
                    {
                        if (!RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId, out Room Room))
                            return;
                        string[] ReceivedData = Data.Split(',');
                        string DesiredFood = ReceivedData[1];

                        RoomUser RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        #region Conditions & Vars
                        RoomUser.OnChat(RoomUser.LastBubble, "servir " + DesiredFood, false);
                        /*
                        string[] ReceivedData = Data.Split(',');
                        string DesiredFood = ReceivedData[1];
                        var Food = FoodManager.GetFoodTwo(DesiredFood);

                        if (Food == null)
                        {
                            string WhisperMessage = "¡Esta comida no existe! Por favor escribe 'food' o vea en el tablero azul.";
                            Client.SendMessage(new WhisperComposer(BotUser.VirtualId, WhisperMessage, 0, 2));
                        }
                        else
                        {
                            Client.GetHabbo().Credits -= Food.Cost;
                            if (Client.GetHabbo().Credits < Food.Cost)
                            {
                                string WhisperMessage = "¡No tienes suficiente dinero!.";
                                Client.SendMessage(new WhisperComposer(BotUser.VirtualId, WhisperMessage, 0, 2));
                            }
                            if (FoodManager.CanServe(Client.GetRoomUser()))
                            {
                                RoleplayManager.GiveMoneyToCompany(6, Client, "mcdonalds", true, Food.Cost);
                                BeginServingFood(Food, Client);
                            }
    
                            else
                            {
                                string WhisperMessage = "Por favor, encuentre una mesa vacía para que le sirva comida a usted!";
                                Client.SendMessage(new WhisperComposer(BotUser.VirtualId, WhisperMessage, 0, 2));
                            }
                        }*/
                        #endregion

                        #region Execute

                        Client.GetRoleplay().ClearWebSocketDialogue();
                        Client.GetRoleplay().RefreshStatDialogue();

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
