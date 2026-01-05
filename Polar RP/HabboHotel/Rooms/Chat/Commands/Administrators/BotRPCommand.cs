using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polar.Database.Interfaces;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Bots.Manager;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class BotRPCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_override"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Teclea :coordbot o :coorbot info para más información"; }
        }

        public async Task Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Uy, no olvide introducir el nombre del bot!");
                return;
            }

            if (Params.Length == 0 || Params[1].ToLower() == "info")
            {
                StringBuilder Message = new StringBuilder().Append("----- Cambiar coordenadas del bot -----\n\n");
                Message.Append("Para cambiar las coordenadas debes seguir estos pasos:\n\n");
                Message.Append("Si el bot tiene un espacio en su nombre. Ej: <b>'Dr. Sevilla'</b> deberas colocar asi: <b>'Dr.%Sevilla'</b> para que se pueda identificar el nombre del bot con el espacio.\n\n");
                Message.Append("Para ejecutar el comando completo deberas teclear y ejecutar. Ej <b>:coordbot Dr.%Sevilla X,Y,Z,R</b>!\n\n");
                Message.Append("<b>¿Que significan las X,Y,Z,R?</b> Bueno son las coordenadas del bot, para saber en que coordenadas colocaras el bot, lleva a tu keko al lugar deseado y ejecutas <b>:coords</b>, pondras los numeros que aparecen en cada letra menos el de SqState!\n\n");
                Message.Append("<b><font color=\"#FE2E2E\">OJO</font>:</b> Las coordenadas tienen que estar con sus <b>','</b>. Ej: <b>':coordbot (BotName) 19,13,4,0'</b> que equivalen a <b>X,Y,Z,R</b>!");
                Session.SendNotification(Message.ToString());
            }

            string BotName = Params[1].Replace("%", " ");
            string X = CommandManager.MergeParams(Params, 2);
            string Y = CommandManager.MergeParams(Params, 3);
            string Z = CommandManager.MergeParams(Params, 4);
            string R = CommandManager.MergeParams(Params, 5); //X 19 Y 13 Z 4 R 0
            object[] Coords = Params[2].Split(',');
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rp_bots` SET `spawn_x` =  '" + Convert.ToInt32(Coords[0]) + "', `spawn_y` =  '" + Convert.ToInt32(Coords[1]) + "', `spawn_z` =  '" + Convert.ToInt32(Coords[2]) + "', `spawn_rot` =  '" + Convert.ToInt32(Coords[3]) + "' WHERE `name` =  '" + BotName + "' AND  `spawn_id` =  '" + Convert.ToInt32(User.GetRoom().Id) + "'");
                Session.SendWhisper("Le has cambiado las coordenadas al bot: " + BotName + "!");
            }
            RoleplayBotManager.Initialize(true);
        }
    }
}
