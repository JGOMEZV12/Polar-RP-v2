using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms.Chat.Styles;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboHotel.Groups;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Hospital
{
    class ComprarChalecoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hospital_heal"; }
        }

        public string Parameters
        {
            get { return "%cantidad%"; }
        }

        public string Description
        {
            get { return "Compra un chaleco antibalas en la tienda de armas"; }
        }

        public async Task Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            RoomUser RoomUser = Session.GetRoomUser();
            int Cantidad = Convert.ToInt32(Params[1]);
            

            if (Session.GetHabbo().CurrentRoomId != 6)
            {
                Session.SendWhisper("¡El chaleco solo se compra en la tienda de armas Dirección: [BARRIO] Av. Smelly [22]!", 1);
                return;
            }

            if (Params.Length < 1)
            {
                Session.SendWhisper("Ejecuta bien el comando :comprarchaleco %cantidad%");
                return;
            }

            if (Cantidad > 10)
            {
                Session.SendWhisper("No puedes comprar más de 10 chalecos.");
                return;
            }

            if(Session.GetRoleplay().Armor == 60 || Session.GetRoleplay().Armor > 1)
            {
                Session.SendWhisper("Actualmente tienes "+Session.GetRoleplay().Armor +" chaleco(s). Usalos para poder comprar otros.", 1);
                return;
            }

            if (Session.GetRoleplay().BankChequings < 2000)
            {
                Session.SendWhisper("Necesitas tener 2000$ en tu cuenta bancaria para poder comprar un chaleco ¡Trabaja!", 1);
                return;
            }

            #endregion

            #region Execute
            int precio = 2000 * Cantidad;
            Session.Shout("*Compra chaleco Kevlar por y paga con su tarjeta de débito[-"+ precio +"$]*", 4);
            //Session.SendWhisper("¡Se te ha colocado el equipo Kevlar!", 1);
            Session.GetRoleplay().BankChequings-= precio;
            Session.GetRoomUser().ApplyEffect(603);
            Session.GetRoleplay().Armor = Cantidad;
            Session.GetRoleplay().UpdateInteractingUserDialogues();
            Session.GetRoleplay().RefreshStatDialogue();
            Session.GetHabbo().UpdateCreditsBalance();
            //Session.GetRoleplay().CurHealth = 300;
            //HabboRoleplay.Misc.RoleplayManager.GetLookAndMotto(Session, "poof");

            #region Bank Company Balance
            RoleplayManager.GiveMoneyToCompany(5, Session, "Ammor", true, 2000);
            #endregion Bank Company Balance
            #endregion
        }
    }
}