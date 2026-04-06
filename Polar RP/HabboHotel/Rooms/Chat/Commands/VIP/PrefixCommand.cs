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
    class PrefixCommand : IChatCommand
    {
        public string PermissionRequired => "command_prefix";
        public string Parameters => "[prefix]";
        public string Description => "Cambia tu prefijo de nombre.";

        public async Task Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.GetHabbo().NamePrefix = "";
                Session.GetHabbo().SaveKey("prefix", "");
                Session.SendWhisper("Has eliminado tu prefijo.", 1);
                return;
            }

            string Prefix = CommandManager.MergeParams(Params, 1);
            if (Prefix.Length > 10)
            {
                Session.SendWhisper("El prefijo es demasiado largo.", 1);
                return;
            }

            Session.GetHabbo().NamePrefix = Prefix;
            Session.GetHabbo().SaveKey("prefix", Prefix);
            Session.SendWhisper($"Has cambiado tu prefijo a: {Prefix}", 1);
        }
    }
}
