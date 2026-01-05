using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

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
using Polar.HabboHotel.Users;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Users.Messenger;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// GroupsWebEvent class.
    /// </summary>
    class GroupsWebEvent : IWebEvent
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

            if (Client == null || Client.GetRoomUser() == null || Client.GetRoomUser().RoomId <= 0)
                return;

            /*
            if (!Client.GetRoleplay().UsingAtm)
            {
                Client.SendNotification("Buen intento, tratando de injectar el systema, ve a un ATM!");
                return;
            }
            */

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            //Generamos la Sala
            if (!RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId, out Room Room))
                return;

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        string Founder = "Ninguno";

                        List<GroupMember> Administrators = Room.Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();

                        if (Administrators.Count > 0)
                        {
                            Founder = (PolarEnvironment.GetHabboById(Administrators[0].UserId) != null) ? PolarEnvironment.GetHabboById(Administrators[0].UserId).Username : null; // Revisa el Diccionario

                            if (Founder == null)
                                Founder = PolarEnvironment.GetUsernameById(Administrators[0].UserId);// <= Hace SELECT porque puede no estar Online el User a buscar
                        }

                        string SendData = "";
                        SendData += Room.Group.Badge + ";";
                        SendData += Room.Group.Name + ";";
                        SendData += Room.Group.GType + ";";
                        SendData += Room.Group.GroupType + ";";
                        SendData += (Room.Group.IsAdmin(Client.GetHabbo().Id) ? "True;" : "False;");
                        SendData += Room.Group.IsMember(Client.GetHabbo().Id) + ";";
                        SendData += Room.Group.HasRequest(Client.GetHabbo().Id) + ";";
                        Socket.Send("compose_group|open|" + SendData);
                        Client.GetRoleplay().GroupRoom = true;
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetRoleplay().GroupRoom = false;
                        Socket.Send("compose_group|close|");
                        break;
                    }
                #endregion

                #region Send
                case "send":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        if (Client.GetRoleplay().TryGetCooldown("groupinfo"))
                            return;

                        if (Client.GetRoleplay().DrivingCar)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                            return;
                        }

                        Client.GetRoleplay().CooldownManager.CreateCooldown("groupinfo", 1000, 5);

                        string Founder = "Ninguno";

                        List<GroupMember> Administrators = Room.Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();

                        if (Administrators.Count > 0)
                        {
                            Founder = (PolarEnvironment.GetHabboById(Administrators[0].UserId) != null) ? PolarEnvironment.GetHabboById(Administrators[0].UserId).Username : null; // Revisa el Diccionario

                            if (Founder == null)
                                Founder = PolarEnvironment.GetUsernameById(Administrators[0].UserId);// <= Hace SELECT porque puede no estar Online el User a buscar
                        }
                        
                        bool Mine = (Room.Group.IsAdmin(Client.GetHabbo().Id) ? true : false);
                        bool Member = Room.Group.IsMember(Client.GetHabbo().Id);

                        // Si no es dueño del Grupo
                        if (!Mine)
                        {
                            #region If is Job
                            if (Room.Group.GType < 3)
                            {
                                // Verificamos Trabajos Actuales                                
                                //List<Groups.Group> Jobs = PolarEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id); <= Hace SELECT Directo a DB
                                List<Groups.Group> Jobs = PolarEnvironment.GetGame().GetGroupManager().GetJobsForUserDict(Client.GetHabbo().Id);

                                if (Jobs == null)
                                {
                                    Client.SendWhisper("((Ha ocurrido un Error al Obtener información de tus Trabajos. Contacte con un Administrador. [3]))", 1);
                                    return;
                                }
                                if (Room.Group.Name.Contains("Policía"))
                                {
                                    List<Groups.Group> Groups = PolarEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                                    if (Groups != null && Groups.Count > 0)
                                    {
                                        Client.SendWhisper("¡Eres miembro de una banda! No puedes ser contratad@ como policía.", 1);
                                        return;
                                    }
                                }

                                int TotalJobs = Jobs.Count;

                                // Si no es Miembro
                                if (!Member)
                                {


                                    if (Room.Group.GroupType == GroupType.OPEN)
                                    {
                                        #region Special Levels Requirement
                                        if (Room.Group.Name.Contains("Armas"))
                                        {
                                            if (Client.GetRoleplay().Level < 3)
                                            {
                                                Client.SendWhisper("((Necesitas al menos nivel 3 para ser Fabricante de Armas))", 1);
                                                return;
                                            }
                                        }
                                        if (Room.Group.Name.Contains("Hospital"))
                                        {
                                            if (Client.GetRoleplay().Level < 2)
                                            {
                                                Client.SendWhisper("((Necesitas al menos nivel 2 para ser Médico))", 1);
                                                return;
                                            }
                                        }
                                        if (Room.Group.Name.Contains("Policia"))
                                        {
                                            if (Client.GetRoleplay().Level < 15)
                                            {
                                                Client.SendWhisper("((Necesitas al menos nivel 15 para ser Policia))", 1);
                                                return;
                                            }
                                        }
                                        #endregion

                                        #region IsWorking
                                        if (Client.GetRoleplay().IsWorking)
                                        {
                                            WorkManager.RemoveWorkerFromList(Client);
                                            Client.GetRoleplay().IsWorking = false;
                                            Client.GetHabbo().Poof();

                                        }
                                        #endregion

                                            #region Extra Cost
                                            GroupRank Rank = GroupManager.GetJobRank(Room.Group.Id, 1);

                                            if (Rank == null)
                                                return;

                                            if (Room.Group.GType == 2 && Rank.Pay > 0)
                                            {
                                                if (Client.GetHabbo().Credits < Rank.Pay)
                                                {
                                                    Client.SendWhisper("Necesitas $ " + Rank.Pay + " de cooperación para poder unirte a este trabajo.");
                                                    return;
                                                }
                                            }
                                            #endregion

                                            #region DirectJoin

                                            #region SendPackets JoinGroupEvent
                                            
                                        if (Room.Group.HasChat)
                                        {
                                            var Clientx = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().Id);
                                            if (Clientx != null)
                                            {
                                                //MessengerBuddy newgroup = new MessengerBuddy(-Room.Group.Id, Room.Group.Name, Room.Group.Badge, string.Empty, 0, false, true, false);
                                                Clientx.SendMessage(new FriendListUpdateComposer(Room.Group, 0));
                                            }
                                        }

                                            #endregion

                                            #region RP Job Vars
                                            // Actualizamos Información del Rank del User
                                        Client.GetRoleplay().TimeWorked = 0;
                                        Client.GetRoleplay().JobId = Room.Group.Id;
                                        Client.GetRoleplay().JobRank = 1;
                                        Client.GetRoleplay().JobRequest = 0;
                                        Room.Group.AddNewMember(Client.GetHabbo().Id);
                                        Room.Group.UpdateJobMember(Client.GetHabbo().Id);
                                        Room.Group.SendPackets(Client);
                                        #endregion

                                        #endregion

                                        RoleplayManager.Shout(Client, "*Ha conseguido el Trabajo de " + Room.Group.Name + "*", 5);
                                            Client.SendWhisper("¡Muy bien! Ahora tienes comandos nuevos para tu nuevo trabajo. Usa :ayuda para consultarlos.", 1);

                                            if (Room.Group.GType == 2 && Rank.Pay > 0)
                                            {
                                                Client.GetHabbo().Credits -= Rank.Pay;
                                                Client.GetHabbo().UpdateCreditsBalance();
                                                Client.SendWhisper("Has pagado $ " + Rank.Pay + " de cooperación para el trabajo.", 1);
                                            }

                                        RoleplayManager.CheckCorpCarp(Client);
                                    }
                                    else if (Room.Group.GroupType == GroupType.LOCKED)
                                    {
                                        if (Room.Group.HasRequest(Client.GetHabbo().Id))
                                        {
                                            Client.SendWhisper("¡Ya has mandado una Solicitud! Por favor espera a que sea respondida.", 1);
                                            return;
                                        }

                                        GroupRank Rank = GroupManager.GetJobRank(Room.Group.Id, 1);

                                        string SendDatas = "";
                                        SendDatas += Room.Group.Name + ";";
                                        SendDatas += Room.Group.Badge + ";";
                                        SendDatas += Founder + ";";
                                        SendDatas += Rank.Name + ";";
                                        SendDatas += Rank.Pay.ToString("C") + ";";
                                        SendDatas += "0 minutos;";
                                        SendDatas += Room.Group.GType + ";";
                                        Socket.Send("compose_group|solicitud|" + SendDatas);
                                        Client.GetRoleplay().GroupRoom = true;
                                    }
                                }
                                // Dejar Grupo
                                else
                                {
                                    #region IsWorking
                                    if (Client.GetRoleplay().IsWorking)
                                    {
                                        WorkManager.RemoveWorkerFromList(Client);
                                        Client.GetRoleplay().IsWorking = false;
                                        Client.GetHabbo().Poof();

                                    }
                                    #endregion

                                    string ExtraInf = "";

                                        //Sacar del Jobs[0]
                                        #region Sacar
                                        int UserId = Client.GetHabbo().Id;
                                        if (Jobs[0].IsAdmin(UserId))
                                        {
                                            ExtraInf = "Se te ha retirado el Cargo Fundador en " + Jobs[0].Name;
                                        }
                                        {
                                            ExtraInf = "Se te ha retirado el trabajo de " + Jobs[0].Name;
                                        }

                                    if (Jobs[0].IsMember(UserId))
                                    {
                                        Client.GetRoleplay().TimeWorked = 0;
                                        Client.GetRoleplay().JobId = 1;
                                        Client.GetRoleplay().JobRank = 1;
                                        Client.GetRoleplay().JobRequest = 0;

                                        var Job = GroupManager.GetJob(Client.GetRoleplay().JobId);
                                        Job.AddNewMember(Client.GetHabbo().Id);
                                        Job.SendPackets(Client);
                                    }

                                        if (Jobs[0].IsAdmin(UserId))
                                        {
                                            if (Jobs[0].IsAdmin(UserId))
                                                Jobs[0].TakeAdmin(UserId);
                                        }

                                        #endregion

                                        RoleplayManager.Shout(Client, "*Ha renunciado a su trabajo de " + Room.Group.Name + "*", 5);

                                        if (ExtraInf != "")
                                            Client.SendWhisper(ExtraInf, 1);
                                    
                                    RoleplayManager.CheckCorpCarp(Client);
                                }
                            }
                            #endregion

                            #region If is Gang
                            else
                            {
                                List<Groups.Group> Gangs = PolarEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                                if (Gangs == null)
                                {
                                    Client.SendWhisper("((Ha ocurrido un Error al Obtener información de tus bandas. Contacte con un Administrador. [3]))", 1);
                                    return;
                                }

                                // Si no es Miembro
                                if (!Member)
                                {
                                    if (GroupManager.HasJobCommand(Client, "law"))
                                    {
                                        Client.SendWhisper("¡No puedes pertenecer a una banda y ser policía a la vez!", 1);

                                    }
                                    else
                                    {
                                        if (Room.Group.GroupType == GroupType.OPEN)
                                        {
                                            if (Gangs.Count <= 0)
                                            {
                                                #region DirectJoin

                                                #region SendPackets JoinGroupEvent
                                                if (Room.Group.HasChat)
                                                {
                                                    var Clientx = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetHabbo().Id);
                                                    if (Clientx != null)
                                                    {
                                                        //MessengerBuddy newgroup = new MessengerBuddy(-Room.Group.Id, Room.Group.Name, Room.Group.Badge, string.Empty, 0, false, true, false);
                                                        Clientx.SendMessage(new FriendListUpdateComposer(Room.Group, 0));
                                                    }
                                                }
                                                #endregion

                                                #region RP Job Vars
                                                // Actualizamos Información del Rank del User
                                                Client.GetRoleplay().GangId = Room.Group.Id;
                                                Client.GetRoleplay().GangRank = 1;
                                                Client.GetRoleplay().GangRequest = 0;
                                                Room.Group.AddNewMember(Client.GetHabbo().Id);
                                                Room.Group.SendPackets(Client);
                                                #endregion

                                                #endregion

                                                RoleplayManager.Shout(Client, "*Ha ingresado a la banda " + Room.Group.Name + "*", 5);
                                                Client.SendWhisper("¡Muy bien! Ahora perteneces a una nueva banda. ((Da clic en su emblema para ver más info.))", 1);
                                            }

                                        }
                                        else if (Room.Group.GroupType == GroupType.LOCKED)
                                        {
                                            
                                            if (Room.Group.HasRequest(Client.GetHabbo().Id))
                                            {
                                                Client.SendWhisper("¡Ya has mandado una Solicitud! Por favor espera a que sea respondida.", 1);
                                                return;
                                            }

                                            RoleplayManager.Shout(Client, "*Ha solicitado ingresar a la banda " + Room.Group.Name + "*", 5);
                                            Client.SendMessage(new RoomNotificationComposer("gang_request_warning", "message", "¡Bien Hecho!\nAhora debes esperar a que aprueben tu solictud.\n\nToma en cuenta que si te encuentras en otra banda; el Líder de la nueva banda no podrá aceptarte hasta que abandones dicha banda anterior."));
                                            // Ws Groups
                                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");

                                            /*List<GameClient> GroupAdmins = (from Clients in PolarEnvironment.GetGame().GetClientManager().GetClients.ToList() where Clients != null && Clients.GetHabbo() != null && Room.Group.IsAdmin(Clients.GetHabbo().Id) select Clients).ToList();
                                            foreach (GameClient Clients in GroupAdmins)
                                            {
                                                Client.SendMessage(new GroupMembershipRequestedComposer(Room.Group.Id, Client.GetHabbo(), 3));
                                            }*/

                                            UserCache Junk = null;
                                            PolarEnvironment.GetGame().GetCacheManager().TryRemoveUser(Client.GetHabbo().Id, out Junk);
                                            PolarEnvironment.GetGame().GetCacheManager().GenerateUser(Client.GetHabbo().Id);

                                            Client.GetRoleplay().GangRequest = Room.Group.Id;
                                            Room.Group.Requests.Add(Client.GetHabbo().Id);

                                            using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                            {
                                                dbClient.SetQuery("UPDATE `rp_stats` SET `gang_request` = '" + Room.Group.Id + "' WHERE `id` = '" + Client.GetHabbo().Id + "' LIMIT 1");
                                                dbClient.RunQuery();
                                            }
                                        }
                                    }
                                }
                                // Dejar Grupo
                                else
                                {
                                    string ExtraInf = "";
                                    #region Sacar
                                    int UserId = Client.GetHabbo().Id;
                                    if (Gangs[0].IsAdmin(UserId))
                                    {
                                        Client.SendWhisper("No puedes abandonar tu propia banda sin dejar a alguien al mando. O bien, puedes eliminarla desde tu panel de gestión.", 1);
                                        return;
                                    }
                                    {
                                        ExtraInf = "Has abandonado tu banda";
                                    }

                                    if (Gangs[0].IsMember(UserId))
                                    {
                                        Client.GetRoleplay().GangId = 0;
                                        Client.GetRoleplay().GangRank = 0;
                                        Client.GetRoleplay().GangRequest = 0;
                                        Room.Group.DeleteMember(UserId);
                                    }

                                    if (Gangs[0].IsAdmin(UserId))
                                    {
                                        if (Gangs[0].IsAdmin(UserId))
                                            Gangs[0].TakeAdmin(UserId);
                                    }
                                    #endregion

                                    RoleplayManager.Shout(Client, "*Ha abandonado la banda " + Room.Group.Name + "*", 5);
                                    Client.SendWhisper(ExtraInf, 1);

                                    //PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "close");
                                    string SendDatax = "";
                                    SendDatax += Room.Group.Badge + ";";
                                    SendDatax += Room.Group.Name + ";";
                                    SendDatax += Room.Group.GType + ";";
                                    SendDatax += Room.Group.GroupType + ";";
                                    SendDatax += "False;";
                                    SendDatax += "False;";
                                    SendDatax += "False;";
                                    Socket.Send("compose_group|open|" + SendDatax);
                                    Client.GetRoleplay().GroupRoom = true;
                                }
                                
                            }
                            #endregion
                        }
                        else
                        {
                            // Gestionar
                            Client.GetRoleplay().ViewMyCorp = true;
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "open_room");
                        }
                        //PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "close");

                        string SendData = "";
                        SendData += Room.Group.Badge + ";";
                        SendData += Room.Group.Name + ";";
                        SendData += Room.Group.GType + ";";
                        SendData += Room.Group.GroupType + ";";
                        SendData += (Room.Group.IsAdmin(Client.GetHabbo().Id) ? "True;" : "False;");
                        SendData += Room.Group.IsMember(Client.GetHabbo().Id) + ";";
                        SendData += Room.Group.HasRequest(Client.GetHabbo().Id) + ";";
                        Socket.Send("compose_group|open|" + SendData);
                        Client.GetRoleplay().GroupRoom = true;
                    }
                    break;
                #endregion

                #region Request
                case "request":
                    {
                        if (Room == null)
                            return;

                        if (Room.Group == null)
                            return;

                        if (Client.GetRoleplay().TryGetCooldown("grouprequest"))
                            return;

                        Client.GetRoleplay().CooldownManager.CreateCooldown("grouprequest", 1000, 5);

                        // No se supone que esto entre, pero por seguridad...
                        if (Room.Group.HasRequest(Client.GetHabbo().Id))
                        {
                            Socket.Send("compose_group|error|¡Ya has mandado una solicitud!");
                            return;
                        }
                        if (Room.Group.Name.Contains("Policia"))
                        {
                            List<Groups.Group> Groups = PolarEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                            if (Groups != null && Groups.Count > 0)
                            {
                                Socket.Send("compose_group|error|¡Eres miembro de una banda! No puedes ser contratad@ como policía.");
                                return;
                            }
                        }

                        string[] ReceivedData = Data.Split(',');
                        int Hours;

                        if (!int.TryParse(ReceivedData[1], out Hours))
                        {
                            Socket.Send("compose_group|error|Debe ingresar una hora válida.");
                            return;
                        }
                        string Desc = ReceivedData[2];
                        string Region = ReceivedData[3];
                        if (Desc.Length > 250)
                        {
                            Socket.Send("compose_group|error|La explicación no debe exceder los 250 Caracteres.");
                            return;
                        }
                        if (Region.Length > 10)
                        {
                            Socket.Send("compose_group|error|País Erróneo.");
                            return;
                        }

                        // Filter
                        Desc = Regex.Replace(Desc, "<(.|\\n)*?>", string.Empty);
                        Region = Regex.Replace(Region, "<(.|\\n)*?>", string.Empty);
                        
                       /* Room.Group.AddNewMember(Client.GetHabbo().Id, 1, true);// Metemos directo a db por seguridad y evitar bugs

                        List<GameClient> GroupAdmins = (from Clients in PolarEnvironment.GetGame().GetClientManager().GetClients.ToList() where Clients != null && Clients.GetHabbo() != null && Room.Group.IsAdmin(Clients.GetHabbo().Id) select Clients).ToList();
                        foreach (GameClient Clients in GroupAdmins)
                        {
                            Client.SendMessage(new GroupMembershipRequestedComposer(Room.Group.Id, Client.GetHabbo(), 3));
                        }
                        Client.SendMessage(new GroupInfoComposer(Room.Group, Client));
                        Client.GetRoleplay().JobRequest = 0;*/
                        RoleplayManager.Shout(Client, "*Ha enviado una solicitud de Empleo a " + Room.Group.Name + "*", 5);
                        Client.SendMessage(new RoomNotificationComposer("job_request_warning", "message", "¡Bien Hecho!\nAhora debes esperar a que aprueben tu solictud.\n\nToma en cuenta que si te encuentras en otro trabajo que también requirió solicitud de empleo; el Fundador del nuevo trabajo no podrá aceptarte hasta que renuncies a dicho trabajo anterior."));
                        // Ws Groups
                        PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                        Socket.Send("compose_group|close_rq|");
                        Socket.Send("compose_group|close");
                        Socket.Send("compose_group|open");

                        Client.GetRoleplay().JobRequest = Room.Group.Id;
                        Room.Group.Requests.Add(Client.GetHabbo().Id);

                        using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_stats` SET `job_request` = '" + Room.Group.Id + "' WHERE `id` = '" + Client.GetHabbo().Id + "' LIMIT 1");
                            dbClient.RunQuery();
                        }
                        break;
                    }
                    #endregion
            }
        }
    }
}
