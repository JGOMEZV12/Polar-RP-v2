using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms.Chat.Styles;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboHotel.Groups;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;
using Polar.Communication.Packets.Outgoing;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class TextCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_about"; }
        }

        public string Parameters
        {
            get { return "%user% %mensaje%"; }
        }

        public string Description
        {
            get { return "Libera al ciudadano de la cárcel si está encarcelado."; }
        }

        public async Task Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Generate Instances / Sessions

            if (Params[1] == "bot")
            {
                PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_feedcomposer", "alert|" + Session.GetHabbo().Username + "|Bot|" + "Asesinó a");
                await PolarEnvironment.SendMs("**__¡LiveFeed!__ ** `|` **" + Session.GetHabbo().Username + "** **Habla claro**");
                return;
            }
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ejecuta bien el comando :text USUARIO MENSAJE");
                return;
            }

            GameClient TargetSession = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetSession == null)
            {
                Session.SendWhisper("Se ha producido un error al intentar encontrar a ese usuario, tal vez están sin conexión.", 1);
                return;
            }

            int credit = new Random().Next(1, 5);
            string Message = CommandManager.MergeParams(Params, 2);
            #endregion

            #region Conditions
            
            if (Session.GetHabbo().Duckets < credit)
            {
                Session.SendWhisper("¡No te queda ningún crédito telefónico!", 34);
                return;
            }

            if (TargetSession.GetHabbo().AllowConsoleMessages == false) {
                Session.SendWhisper("El destinario tiene su telefono apagado, intenta más tarde.", 34);
                return;
            }

            if (Session.GetHabbo().Username == TargetSession.GetHabbo().Username)
            {
                Session.SendWhisper("No puedes enviarte ni responderte a ti mismo.", 34);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("textcooldown", true))
            {
                Session.SendWhisper("Debe esperar hasta que puedas enviar un mensaje nuevamente! ", 1);
                return;
            }
            #endregion

            #region Execute
            RoomUser u = Session.GetRoomUser();
            RoomUser u2 = TargetSession.GetRoomUser();

            Session.GetRoleplay().EffectSeconds = 3;
            u.ApplyEffect(65);

           
            //Session.GetRoleplay().CheckingMultiCooldown = true;
            //TargetSession.GetRoleplay().LastTexter = Session;

            Session.SendWhisper("*Envía un mensaje de texto a " + TargetSession.GetHabbo().Username + "*", 1);
            RoleplayManager.TakeMoneyCredits(Session, credit);

            if (TargetSession.GetRoomUser().IsAsleep)
                Session.SendWhisper("Este usuario está AFK, ¡pero el mensaje aún se ha enviado!", 1);

            TargetSession.GetRoleplay().EffectSeconds = 3;

            if (u2 != null)
            {
                u2.ApplyEffect(65);
            }

           TargetSession.SendWhisper("*Recibe un nuevo mensaje de texto de " + Session.GetHabbo().Username + "*", 1);

            StringBuilder view = new StringBuilder();
            view.Append("=====================================================\n¡Acabas de recibir un nuevo mensaje de texto!\n=====================================================\n");
            view.Append("De: " + Session.GetHabbo().Username + "\n");
            view.Append("Enviado: " + DateTime.Now + " (Server Time)\n\n");
            view.Append("Mensaje: \n");
            view.Append(Message + "\n\n");
            view.Append("-" + Session.GetHabbo().Username);

            TargetSession.SendMessage(new MOTDNotificationComposer(view.ToString()));
            TargetSession.GetRoleplay().LastMsgText = Session.GetHabbo().Username;

            //TargetSession.SendWhisper(TargetSession.GetRoleplay().LastMsgText, 4);

            var roomUserByRank = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByRank(2);



            //Session.GetRoleplay().MultiCoolDown["textusee"] = 600;
            Session.GetRoleplay().CooldownManager.CreateCooldown("textcooldown", 1000, 3);
            TargetSession.GetRoleplay().CooldownManager.CreateCooldown("textcooldown", 1000, 3);
            //Session.GetRoleplay().SpecialCooldowns.TryUpdate("text_cooldown", 3, Session.GetRoleplay().SpecialCooldowns["text_cooldown"]);

            #endregion
        }
    }
}