using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.GameClients;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class SellSkinsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_sell_skins"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Vende todas tus pieles de caza obtenidas."; }
        }

        public async Task Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetRoleplay().HuntSkins <= 0)
            {
                Session.SendWhisper("No tienes ninguna piel de caza para vender.", 1);
                return;
            }

            int Skins = Session.GetRoleplay().HuntSkins;
            int PayPerSkin = 100; // Puedes ajustar el precio por piel aquí
            int TotalPay = Skins * PayPerSkin;

            Session.GetRoleplay().HuntSkins = 0;
            Session.GetHabbo().Credits += TotalPay;
            Session.GetHabbo().UpdateCreditsBalance();

            Session.Shout("*Vende " + Skins + " pieles de caza por $" + String.Format("{0:N0}", TotalPay) + "*", 4);
            Session.SendWhisper("Has vendido " + Skins + " pieles y has recibido $" + String.Format("{0:N0}", TotalPay) + ".", 1);
        }
    }
}