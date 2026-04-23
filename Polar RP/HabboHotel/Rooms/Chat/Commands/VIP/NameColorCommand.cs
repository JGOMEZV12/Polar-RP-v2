using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;

namespace Polar.HabboHotel.Rooms.Chat.Commands.VIP
{
    class NameColorCommand : IChatCommand
    {
        public string PermissionRequired => "command_name_color";
        public string Parameters => "[hex/rainbow]";
        public string Description => "Cambia el color de tu nombre.";

        public async Task Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.GetHabbo().NameColor = "";
                Session.GetHabbo().SaveKey("name_color", "");
                Session.SendWhisper("Has eliminado el color de tu nombre.", 1);

                // Update room
                UpdateUser(Session);
                return;
            }

            string Color = Params[1];

            if (Color.ToLower() != "rainbow" && (Color.Length != 6 || !IsHex(Color)))
            {
                Session.SendWhisper("Color inválido. Usa un código HEX (ej: FF0000) o 'rainbow'.", 1);
                return;
            }

            Session.GetHabbo().NameColor = Color;
            Session.GetHabbo().SaveKey("name_color", Color);
            Session.SendWhisper($"Has cambiado el color de tu nombre a: {Color}", 1);

            UpdateUser(Session);
        }

        private void UpdateUser(GameClient Session)
        {
            if (Session.GetHabbo().InRoom)
            {
                var user = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (user != null)
                    Session.GetHabbo().CurrentRoom.SendMessage(new UsersComposer(user));
            }
        }

        private bool IsHex(string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}
