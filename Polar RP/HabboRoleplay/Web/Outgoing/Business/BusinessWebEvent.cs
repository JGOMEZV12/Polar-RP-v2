using Fleck;

using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Outgoing.Groups;
using Polar.HabboHotel.Groups;
using Polar.Database.Interfaces;
using System.Text.RegularExpressions;
using Polar.HabboHotel.Users;
using Group = Polar.HabboHotel.Groups.Group;
using System.Data;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// BusinessWebEvent class.
    /// </summary>
    class BusinessWebEvent : IWebEvent
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

            if (Client == null || Client.GetRoomUser() == null)
                return;

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);
            //Generamos la Sala
            if (!RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId, out Room Room))
                return;

            switch (Action)
            {
                #region Open My
                case "open_my":
                    {
                        if (Client.GetRoleplay().JobId <= 0)
                        {
                            Socket.Send("compose_business|create|");
                        }
                        else
                        {
                            Client.GetRoleplay().ViewMyCorp = true;
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "open_room");
                        }
                    }
                    break;
                #endregion

                #region Close My
                case "close_my":
                    {
                        Client.GetRoleplay().ViewMyCorp = false;
                        Client.GetRoleplay().ViewCorpId = 0;
                        Socket.Send("compose_business|close|");
                    }
                    break;
                #endregion

                #region Open Room
                case "open_room":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {

                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            //Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                            Group = Room.Group;
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType == 1)//GType 2 = Secundarios
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;

                        #endregion

                        if (Group.GType == 2)
                        {
                            string Founder = "Gobierno";
                            string Tabs = "";
                            List<GroupMember> Administrators = Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();

                            #region Get Founder Name
                            #region Old (OFF)
                            /* Old
                            if (Group.GetAdministrator.Count > 0)
                            {
                                Founder = (PolarEnvironment.GetHabboById(Group.GetAdministrators[0]) != null) ? PolarEnvironment.GetHabboById(Group.GetAdministrators[0]).Username : null; // Revisa el Diccionario

                                if (Founder == null)
                                    Founder = PolarEnvironment.GetUsernameById(Group.GetAdministrators[0]);// <= Hace SELECT porque puede no estar Online el User a buscar
                            }
                            */
                            #endregion
                            if (Administrators.Count > 0)
                            {
                                Founder = PolarEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Administrators[0].UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                            }
                            #endregion

                            #region Check if is Admin
                            if ((Group.IsAdmin(Client.GetHabbo().Id) || (Group.IsMember(Client.GetHabbo().Id))) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            {
                                Tabs += "<div class=\"tab-2ddeR_0 Request_Tab\">Solicitudes</div>";

                                Tabs += "<div class=\"tab-2ddeR_0 Manage_Tab\">Gestionar</div>";
                            }
                            #endregion

                            Client.GetRoleplay().ViewCorpId = Group.Id;

                            string SendData = "";
                            SendData += Group.Name + ";";
                            SendData += Group.Badge + ";";
                            SendData += Tabs + ";";// Tabs
                            SendData += Group.Members.Count + ";";
                            SendData += Group.Requests.Count + ";";
                            SendData += Group.Ranks.Count + ";";
                            SendData += Group.Balance + ";";
                            SendData += Founder + ";";
                            SendData += (Group.IsAdmin(Client.GetHabbo().Id) == true || Client.GetHabbo().GetPermissions().HasRight("corporation_rights") == true ? "True;" : "False;");
                            SendData += (!Group.IsAdmin(Client.GetHabbo().Id) && !Group.IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override")) ? "False;" : "True;";
                            Socket.Send("compose_business|open|" + SendData);
                        }
                        else
                        {
                            // Gangs
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "open");
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "view_stats," + Group.Id);
                        }
                    }
                    break;
                #endregion

                #region Empoyees
                case "employees":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        bool isAdmin = false;
                        string html = "";

                        #region Check if is Admin
                        if (Group.IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            isAdmin = true;
                        #endregion

                        #region HTML
                        html += "<div>";
                        html += "<div class=\"-m-2\">";

                        var AllRanks = Group.Ranks.OrderBy(o => o.Value.RankId).ToList();
                        AllRanks.Reverse();
                        foreach (var Ranks in AllRanks)
                        {
                            //<!-- Rank box -->
                            html += "<div class=\"m-2\">";
                            //<!-- Rank Header -->
                            html += "<div class=\"heading relative group\">";
                            html += "<div>" + Ranks.Value.Name + "</div>";
                            if (isAdmin && !Group.Name.Contains("Policia"))
                            {
                                html += "<div class=\"absolute pin-t pin-r mr-1 h-full hidden group-hover:block\">";
                                html += "<div class=\"flex items-center h-full\"> ";

                                html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"settings\" class=\"cursor-pointer-r px-1\">";
                                html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/settings.png\">";
                                html += "</div>";

                                // Empresas no removibles no pueden alterar rangos

                                    html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"up\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    html += "</div>";

                                    html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"down\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/down-arrow.png\">";
                                    html += "</div>";

                                    html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"cross\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";


                                html += "</div>";
                                html += "</div>";
                            }

                            html += "</div>";

                            // < !-- User box -->
                            html += "<div class=\"flex flex-wrap -m-1 justify-center\">";

                            foreach (var Members in Group.GetAllMembersDict)
                            {
                                if (Members.Value.UserRank == Ranks.Value.RankId)
                                {
                                    string Name = PolarEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                                    string Look = PolarEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                                    //<!-- User info -->
                                    html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\">";
                                    html += "<div class=\"m-px relative\">";
                                    html += "<div class=\"overflow-hidden bg-light-05 rounded-t\" style=\"height: 55px;\">";
                                    html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                    html += "</div>";

                                    bool IsMember = Group.IsMember(Client.GetHabbo().Id);
                                    bool CanAscDesc = GroupManager.HasJobCommand(Client, "ascdesc");
                                    bool CanFire= GroupManager.HasJobCommand(Client, "fire");

                                    if (((isAdmin || (IsMember && (CanAscDesc || CanFire))) && Members.Value.UserId != Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        html += "<div class=\"absolute pin-b bg-dark-5 w-full hidden group-hover:block\" style=\"padding-top: 4px;padding-bottom: 4px;\">";
                                        html += "<div class=\"flex justify-around\">";
                                        html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"up\" class=\"cursor-pointer-r px-1\">";
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                        html += "</div>";
                                        html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"down\" class=\"cursor-pointer-r px-1\">";
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/down-arrow.png\">";
                                        html += "</div>";
                                        if (isAdmin || CanFire || Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        {
                                            html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"cross\" class=\"cursor-pointer-r px-1\">";
                                            html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                            html += "</div>";
                                        }
                                        html += "</div>";
                                        html += "</div>";
                                    }

                                    html += "</div>";
                                    html += "<div class=\"text-center py-1\">" + Name + "</div>";
                                    html += "</div>";
                                }
                            }

                            html += "</div>";
                            // < !-- End User box -->

                            html += "</div>";
                        }
                        html += "</div>";
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_business|employees|" + SendData);
                    }
                    break;
                #endregion

                #region Rank Tools
                case "rank_tools":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        // Policía no se puede editar
                        if (Group.Name.Contains("Policia"))
                            return;

                        #region Check if is not Admin
                        if (!Group.IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        int GetRank;
                        string WSAction = ReceivedData[2];

                        if (!int.TryParse(ReceivedData[1], out GetRank))
                            return;

                        var AllRanks = Group.Ranks.ToList();
                        var AllMembers = Group.GetAllMembersDict;

                        // Validamos si existe el rango
                        var check = AllRanks.Where(x => x.Value.RankId == GetRank);
                        if (check.Count() <= 0)
                            return;

                        switch (WSAction)
                        {
                            case "up":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.

                                    int newrank = (GetRank + 1);

                                    // Validamos si existe un rango superior. 
                                    var TopRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (TopRank.Count() <= 0)
                                        return;

                                    using (var DB = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifRankE`(" + GetRank + ", " + newrank + ", " + Group.Id + ");");

                                        AllRanks.Where(x => x.Value.RankId == newrank).ToList().ForEach(x => x.Value.RankId = 0);
                                        AllRanks.Where(x => x.Value.RankId == GetRank).ToList().ForEach(x => x.Value.RankId = newrank);
                                        AllRanks.Where(x => x.Value.RankId == 0).ToList().ForEach(x => x.Value.RankId = GetRank);

                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == newrank))
                                        {
                                            Members.Value.UserRank = 0;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                        {
                                            Members.Value.UserRank = newrank;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == 0))
                                        {
                                            Members.Value.UserRank = GetRank;
                                        }
                                    }

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");

                                }
                                break;

                            case "down":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                                    int newrank = (GetRank - 1);

                                    // Validamos si existe un rango inferior. 
                                    var BottomRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (BottomRank == null)
                                        return;

                                    using (var DB = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifRankE`(" + GetRank + ", " + newrank + ", " + Group.Id + ");");

                                        AllRanks.Where(x => x.Value.RankId == newrank).ToList().ForEach(x => x.Value.RankId = 0);
                                        AllRanks.Where(x => x.Value.RankId == GetRank).ToList().ForEach(x => x.Value.RankId = newrank);
                                        AllRanks.Where(x => x.Value.RankId == 0).ToList().ForEach(x => x.Value.RankId = GetRank);

                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == newrank))
                                        {
                                            Members.Value.UserRank = 0;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                        {
                                            Members.Value.UserRank = newrank;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == 0))
                                        {
                                            Members.Value.UserRank = GetRank;
                                        }
                                    }

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");
                                }
                                break;

                            case "cross":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.

                                    // Si tiene un solo rango
                                    if (AllRanks.Count() <= 1)
                                        return;

                                    // Rango clicado + 1 es el rango superior.
                                    int uprank = (GetRank + 1);

                                    // Si hay miembros en ese rango, los cambiamos al rankid = 1

                                    foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                    {
                                        Members.Value.UserRank = 1;
                                    }

                                    // Borramos de DB
                                    using (var DB = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `DropRankE`(" + GetRank + ", " + Group.Id + ");");
                                    }

                                    // Borramos del Diccionario
                                    PolarEnvironment.GetGame().GetGroupManager().DeleteGroupRank(Group.Id, GetRank);

                                    // Fix
                                    AllRanks = Group.Ranks.ToList();

                                    // Si hay rango superior, decrementamos su RankId para cada uno
                                    AllRanks.Where(x => x.Value.RankId >= uprank).ToList().ForEach(x => x.Value.RankId = (x.Value.RankId - 1));

                                    // Si hay usuarios en rangos superiores, decrementamos su RankId para cada uno
                                    AllMembers.Where(x => x.Value.UserRank >= uprank).ToList().ForEach(x => x.Value.UserRank = (x.Value.UserRank - 1));

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");
                                }
                                break;

                            case "settings":
                                {
                                    GroupRank ThisRank = GroupManager.GetJobRank(Group.Id, GetRank);

                                    if (ThisRank == null)
                                        return;

                                    string html = "";

                                    #region HTML

                                    html += "<div class=\"heading\">Nombre del Puesto</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<input id=\"RankNewName\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"" + ThisRank.Name + "\">";
                                    html += "</div>";
                                    html += "<br>";

                                    html += "<div class=\"heading\" style=\"width: 34%;float: left;\">Paga del Puesto</div>";
                                    html += "<div class=\"heading\" style=\"width: 45%;float: right;\">Jornada de Tiempo (Minutos)</div>";

                                    html += "<div class=\"flex\" style=\"width: 45%;float:left;\">";
                                    html += "<input id=\"RankPay\" type=\"number\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" min=\"0\" value=\"" + ThisRank.Pay + "\">";
                                    html += "</div>";

                                    html += "<br>";
                                    html += "<div class=\"flex\">";
                                    html += "<button data-rank=\"" + GetRank + "\" data-action=\"SaveRank\" class=\"dark-button\" style=\"width: 100%;\">Guardar Cambios</button>";
                                    html += "</div>";

                                    html += "<br>";
                                    html += "<div class=\"heading\">Permisos</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<table class=\"dark\">";
                                    html += "<tr class=\"dark2\">";
                                    html += "<th class=\"dark2\">Ascender/Descender</th>";
                                    html += "<th class=\"dark2\">Contratar</th>";
                                    html += "<th class=\"dark2\">Despedir</th>";
                                    html += "<th class=\"dark2\">Abastecer</th>";
                                    html += "</tr>";
                                    html += "<tr class=\"dark2\">";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"ascdesc\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("ascdesc"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"hire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("hire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"fire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("fire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"supply\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("supply"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "</tr>";
                                    html += "</table>";
                                    html += "</div>";
                                    html += "<br>";
                                    #endregion

                                    string Male = "";
                                    string Female = "";

                                    if (ThisRank.MaleFigure.Length > 0)
                                        Male = "." + ThisRank.MaleFigure;
                                    if (ThisRank.FemaleFigure.Length > 0)
                                        Female = "." + ThisRank.FemaleFigure;

                                    string SendData = "";
                                    SendData += html + "|";// EventData[2];
                                    SendData += Male + "|";// EventData[3];
                                    SendData += Female + "|";// EventData[4];
                                    SendData += GetRank;// EventData[5];
                                    Socket.Send("compose_business|settings|" + SendData);
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Member Tools
                case "member_tools":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        bool isAdmin = false;

                        #region Check if is not Admin
                        if (!Group.IsAdmin(Client.GetHabbo().Id) && !Group.IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !GroupManager.HasJobCommand(Client, "ascdesc") && !GroupManager.HasJobCommand(Client, "fire"))
                            return;
                        else
                            isAdmin = true;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        int GetUserId;
                        string WSAction = ReceivedData[2];

                        if (!int.TryParse(ReceivedData[1], out GetUserId))
                            return;

                        var AllRanks = Group.Ranks.ToList();
                        var AllMembers = Group.GetAllMembersDict;

                        // Validamos si existe el usuario
                        if (!Group.IsMember(GetUserId) && !Group.IsAdmin(GetUserId))
                            return;

                        switch (WSAction)
                        {
                            case "up":
                                {
                                    
                                    if (!Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        if (Group.Members[GetUserId].UserId == Client.GetHabbo().Id)
                                            return;
                                    }

                                    //if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PolarEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Group.IsMember(Client.GetHabbo().Id);
                                    bool CanAscDesc = GroupManager.HasJobCommand(Client, "ascdesc");
                                    
                                    if (((!isAdmin && !(IsMember && CanAscDesc)) || Group.Members[GetUserId].UserId == Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    int GetRank = Group.Members[GetUserId].UserRank;
                                    int newrank = (GetRank + 1);
                                    List<GroupMember> Administrators = Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();


                                        if (newrank >= 6 && Administrators.Count > 0)
                                        {
                                            Client.SendWhisper("¡No pueden haber dos Administradores en una misma Empresa!", 1);
                                            return;
                                        }

                                        if (newrank >= 6 && Administrators.Count <= 0 && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        {
                                            Client.SendWhisper("No puedes asignar como Fundador a un empleado. ¡Debe comprar la empresa!", 1);
                                            return;
                                        }


                                    // Validamos si existe un rango superior. 
                                    var TopRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (TopRank.Count() <= 0)
                                        return;

                                    using (var DB = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMemberE`(" + GetRank + ", " + newrank + ", " + Group.Id + ", " + GetUserId + ");");

                                        AllMembers.Where(x => x.Value.UserId == GetUserId).ToList().ForEach(x => x.Value.UserRank = newrank);

                                    }

                                    #region Mensaje de aviso al Target
                                    GameClient TargetSession = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);
                                    if(TargetSession != null)
                                    {
                                        RoleplayManager.Shout(TargetSession, "*Ha sido ascendid@ de puesto en su trabajo "+Group.Name+"*", 5);
                                        TargetSession.SendWhisper("¡Buenas noticias! Has sido ascendid@ de puesto.", 1);

                                        if (TargetSession.GetRoleplay().IsWorking)
                                            TargetSession.SendWhisper("Puedes dejar de trabajar y :trabajar cuando gustes para recibir tu nuevo puesto.", 1);
                                        if (Group.IsAdmin(GetUserId))
                                        {
                                            TargetSession.GetRoleplay().JobId = Group.Id;
                                        }
                                    }
                                    #endregion

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");

                                }
                                break;

                            case "down":
                                {

                                    if (!Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        if (Group.Members[GetUserId].UserId == Client.GetHabbo().Id)
                                            return;
                                    }

                                    //if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PolarEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Group.IsMember(Client.GetHabbo().Id);
                                    bool CanAscDesc = GroupManager.HasJobCommand(Client, "ascdesc");

                                    if (((!isAdmin && !(IsMember && CanAscDesc)) || Group.Members[GetUserId].UserId == Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    int GetRank = Group.Members[GetUserId].UserRank;
                                    int newrank = (GetRank - 1);

                                    if(Group.IsAdmin(GetUserId) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        Client.SendWhisper("No puedes bajar de rango al Propietario de la Empresa");
                                        return;
                                    }

                                    // Validamos si existe un rango superior. 
                                    var Bottom = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (Bottom.Count() <= 0)
                                        return;

                                    using (var DB = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMemberE`(" + GetRank + ", " + newrank + ", " + Group.Id + ", " + GetUserId + ");");

                                        AllMembers.Where(x => x.Value.UserId == GetUserId).ToList().ForEach(x => x.Value.UserRank = newrank);
                                    }

                                    #region Mensaje de aviso al Target
                                    GameClient TargetSession = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);
                                    if (TargetSession != null)
                                    {
                                        RoleplayManager.Shout(TargetSession, "*Ha sido degradad@ de puesto en su trabajo " + Group.Name + "*", 5);
                                        TargetSession.SendWhisper("Has sido degradad@ de puesto.", 1);

                                        #region IsWorking
                                        if (TargetSession != null)
                                        {
                                            if (TargetSession.GetRoleplay().IsWorking)
                                            {
                                                WorkManager.RemoveWorkerFromList(TargetSession);
                                                TargetSession.GetRoleplay().IsWorking = false;
                                                TargetSession.GetHabbo().Poof();

                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");
                                }
                                break;

                            case "cross":
                                {
                                    // if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PolarEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "fire")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Group.IsMember(Client.GetHabbo().Id);
                                    bool CanFire = GroupManager.HasJobCommand(Client, "fire");

                                    if (((!isAdmin && !(IsMember && CanFire)) || Group.Members[GetUserId].UserId == Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    // Si se intenta borrar a sí mismo (Que use :renunciar [id])
                                    // Si es Admin (Fundador) no puede eliminarse así.
                                    if (Group.Members[GetUserId].UserId == Client.GetHabbo().Id)
                                        return;

                                    if (Group.IsAdmin(GetUserId) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        Client.SendWhisper("No puedes despedir al Propietario de la Empresa");
                                        return;
                                    }

                                    GameClient TargetSession = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);

                                    // Si están ON, validamos si se encuentra trabajando.
                                    #region IsWorking
                                    if (TargetSession != null)
                                    {
                                        if (TargetSession.GetRoleplay().IsWorking)
                                        {
                                            WorkManager.RemoveWorkerFromList(TargetSession);
                                            TargetSession.GetRoleplay().IsWorking = false;
                                            TargetSession.GetHabbo().Poof();

                                        }
                                    }
                                    #endregion

                                    string ExtraInf = "";

                                    #region Sacar
                                    if (Group.IsAdmin(GetUserId))
                                    {
                                        ExtraInf = "Se te ha retirado el Cargo Fundador en " + Group.Name;
                                    }
                                    {
                                        ExtraInf = "Se te ha retirado el trabajo de " + Group.Name;
                                    }
                                    if (Group.IsAdmin(GetUserId))
                                        Group.TakeAdmin(GetUserId);

                                    if (Group.IsMember(GetUserId)) {
                                        TargetSession.GetRoleplay().TimeWorked = 0;
                                        TargetSession.GetRoleplay().JobId = 1;
                                        TargetSession.GetRoleplay().JobRank = 1;
                                        TargetSession.GetRoleplay().JobRequest = 0;

                                        var Job = GroupManager.GetJob(TargetSession.GetRoleplay().JobId);
                                        Job.AddNewMember(TargetSession.GetHabbo().Id);
                                        Job.SendPackets(TargetSession);
                                    }
                                        //Group.DeleteMember(GetUserId);
                                    #endregion

                                    // Si está ON, recibe alerta.
                                    if (TargetSession != null)
                                    {
                                        if (ExtraInf != "")
                                            TargetSession.SendNotification(ExtraInf);

                                        // Refrescar información
                                        PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetSession, "event_group", "open");
                                        RoleplayManager.Shout(TargetSession, "*Ha sido despedido del trabajo "+ Group.Name  +"*", 5);
                                        //RoleplayManager.PoliceCMDSCheck(TargetSession);
                                    }

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "employees");
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Requests
                case "requests":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        #region Check if is not Admin
                        if (((!Group.IsAdmin(Client.GetHabbo().Id) || !Group.IsMember(Client.GetHabbo().Id)) && !GroupManager.HasJobCommand(Client, "hire")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            return;
                        #endregion

                        string html = "";
                        var AllRequest = Group.GetRequests;

                        #region HMTL
                        html += "<div>";
                        html += "<div class=\"-m-2\">";
                        //<!-- General box -->
                        html += "<div class=\"m-2\">";

                        //<!-- Header -->
                        html += "<div class=\"heading relative group\">";
                        html += "<div>Solicitudes (" + AllRequest.Count() + ")</div>";
                        html += "</div>";

                        foreach (var Request in AllRequest)
                        {
                            string Name = PolarEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                            string Look = PolarEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                            DataRow Row = null;
                            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM `rp_jobs_requests` WHERE `job_id` = @id AND `user_id` = @userid LIMIT 1");
                                dbClient.AddParameter("id", Group.Id);
                                dbClient.AddParameter("userid", Request);
                                Row = dbClient.getRow();

                                //< !-- User box -->
                                html += "<div class=\"flex flex-wrap -m-1 justify-center\">";
                                //<!-- User info -->
                                html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\" style=\"width: 23%;\">";
                                html += "<div class=\"m-px relative\">";
                                html += "<div class=\"overflow-hidden bg-light-05 rounded-t\">";
                                html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                html += "</div>";
                                html += "<div class=\"absolute pin-b bg-dark-5 w-full group-hover:block\" style=\"padding-top: 4px;padding-bottom: 4px;\">";
                                html += "<div class=\"flex justify-around\">";
                                html += "<div data-user=\"" + Request + "\" data-action=\"accept\" class=\"cursor-pointer-r px-1\">";
                                html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                html += "</div>";
                                html += "<div data-user=\"" + Request + "\" data-action=\"decline\" class=\"cursor-pointer-r px-1\">";
                                html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                html += "<div class=\"text-center py-1\">" + Name + "</div>";
                                html += "</div>";

                                html += "<div class=\"bg-dark-4 rounded m-1 group\" style=\"width: 68%;padding: 5px;\">";
                                html += "<div style=\"max-height: 108px;overflow: auto;\">";
                                html += "<b>Pa&iacute;s: </b>";
                                html += "<div class=\"bsn_s_country\" style=\"display: inline-block;\">" + Row["ws_region"] + "</div>";
                                html += "<br>";
                                html += "<b>Horas Libres: </b>";
                                html += "<div class=\"bsn_s_hours\" style=\"display: inline-block;\">" + Row["ws_hours"] + "</div> Hr(s)";
                                html += "<br>";
                                html += "<b>M&aacute;s Informaci&oacute;n:</b><br>	";
                                html += "<div class=\"bsn_s_info\" style=\"display: inline-block;\">";
                                html += Row["ws_desc"];
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                //<!-- End User box -->
                            }
                        }

                        if (AllRequest.Count() <= 0)
                            html += "<center>No hay solicitudes nuevas.</center>";

                        html += "</div>";

                        html += "</div>";
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_business|requests|" + SendData);
                    }
                    break;
                #endregion

                #region Request Tools
                case "request_tools":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        #region Check if is not Admin
                        if (((!Group.IsAdmin(Client.GetHabbo().Id) || !Group.IsMember(Client.GetHabbo().Id)) && !GroupManager.HasJobCommand(Client, "hire")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        int GetUserId;
                        string WSAction = ReceivedData[2];

                        if (!int.TryParse(ReceivedData[1], out GetUserId))
                            return;

                        if (!Group.HasRequest(GetUserId))
                            return;

                        switch (WSAction)
                        {
                            case "accept":
                                {
                                    GameClient TargetClient = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);

                                    // Obtenemos los Trabajos del Target
                                    List<Group> Jobs = PolarEnvironment.GetGame().GetGroupManager().GetJobsForUser(GetUserId);
                                    if (Jobs == null)
                                    {
                                        Client.SendWhisper("Ocurrió un problema al buscar los Trabajos de esa persona. Intentalo más tarde.", 1);
                                        return;
                                    }
                                    int TotalJobs = Jobs.Count;

                                    if (Group.Name.Contains("Policia"))
                                    {
                                        List<Group> Groups = PolarEnvironment.GetGame().GetGroupManager().GetGangsForUser(GetUserId);
                                        if (Groups != null && Groups.Count > 0)
                                        {
                                            Client.SendWhisper("¡Esa persona es miembro de una banda! No puede ser contratada como policía.", 1);
                                            return;
                                        }
                                    }

                                    // Si está On
                                    if (TargetClient != null)
                                    {
                                            #region DirectJoin

                                            //Si es Empresa (PRIVATE)
                                            // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                                            #region SendPackets AcceptGroupMembershipEvent
                                            if (Group.GroupType == GroupType.LOCKED)
                                            {
                                                int UserId = TargetClient.GetHabbo().Id;
                                                if (!Group.HasRequest(UserId))
                                                    return;

                                                Habbo Habbo = PolarEnvironment.GetHabboById(UserId);
                                                if (Habbo == null)
                                                {
                                                    Client.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Empleo!");
                                                    return;
                                                }

                                                Group.HandleRequest(UserId, true);

                                                Client.SendMessage(new GroupMemberUpdatedComposer(Group.Id, Habbo, 4));
                                            }
                                            #endregion


                                            // Actualizamos Información del Rank del User
                                            TargetClient.GetRoleplay().JobId = Group.Id;
                                            TargetClient.GetRoleplay().JobRank = 1;
                                            Group.UpdateJobMember(TargetClient.GetHabbo().Id);

                                            #endregion
                                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_group", "open");
                                            RoleplayManager.Shout(Client, "*Contrata a " + PolarEnvironment.GetUsernameById(GetUserId) + " en " + Group.Name + "*", 5);
                                            RoleplayManager.Shout(TargetClient, "*Ha sido contratado en " + Group.Name + "*", 5);
                                            TargetClient.SendNotification("¡Felicitaciones! Han aceptado tu solicitud de empleo en " + Group.Name);
                                            //RoleplayManager.PoliceCMDSCheck(TargetClient);
                                    }
                                    // Si está Off
                                    else
                                    {
                                            Group.HandleRequest(GetUserId, true);
                                            RoleplayManager.Shout(Client, "*Contrata a " + PolarEnvironment.GetUsernameById(GetUserId) + " en " + Group.Name + "*", 5);

                                    }

                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "requests");

                                }
                                break;

                            case "decline":
                                {
                                    Group.HandleRequest(GetUserId, false);
                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "requests");
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Manage
                case "manage":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                        #region Check if is not Admin
                        if (!Group.IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');

                        string WSAction = ReceivedData[1];
                        string DataString = "";

                        if (WSAction != "open")
                        {
                            DataString = ReceivedData[2];

                            if (String.IsNullOrWhiteSpace(DataString))
                                return;
                        }

                        switch (WSAction)
                        {
                            case "open":
                                {
                                    Socket.Send("compose_business|manage|");
                                }
                                break;

                            case "addrank":
                                {
                                    if (DataString.Length > 50)
                                    {
                                        Client.SendWhisper("El nombre del puesto es demasiado largo.", 1);
                                        return;
                                    }
                                    if (Group.Ranks.Count() >= 8)
                                    {
                                        Socket.Send("compose_gang|msg_error|Límite de 8 rangos alcanzados.");
                                        return;
                                    }

                                    int NewRank = Group.Ranks.Count() + 1;

                                    string[] commands = null;
                                    string[] workrooms = Group.RoomId.ToString().Split(',');

                                    /*
                                    string[] commands = new string[1];
                                    string[] workrooms = new string[1];
                                    commands[0] = "";
                                    workrooms[0] = "*";
                                    */

                                    Group.AddRank(Group.Id, NewRank, DataString, "", "", 5, commands, workrooms, 0);
                                    Socket.Send("compose_business|manage|");
                                }
                                break;

                            case "savelogo":
                                {
                                    if (DataString.Length > 255)
                                    {
                                        Client.SendWhisper("La URL del logo es demasiado larga.", 1);
                                        return;
                                    }

                                    // Ya se validó si es Admin.
                                    // UpdateJobBadge no hace esa Validación. (Cuidado)
                                    Group.UpdateJobBadge(DataString);
                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "open_room");
                                    Socket.Send("compose_business|manage|");
                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Edit Rank
                case "editrank":
                    {
                        Group Group = null;

                        #region Get Group
                        if (Client.GetRoleplay().ViewMyCorp)
                        {
                            if (Client.GetRoleplay().JobId <= 0)// Por si acaso
                                return;

                            Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().JobId);
                        }
                        else
                        {
                            if (Room.Group == null || Room.Group.GType != 2)//GType 1 = Empresa
                                return;

                            Group = Room.Group;
                        }

                        if (Group == null)
                            return;
                        #endregion

                        // Empresas policía no se puede editar en rangos
                        if (Group.Name.Contains("Policia"))
                            return;

                        #region Check if is not Admin
                        if (!Group.IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');

                        int GetRank = 0;
                        var AllRanks = Group.Ranks.ToList();

                        string WSAction = ReceivedData[1];

                        if (!int.TryParse(ReceivedData[2], out GetRank))
                            return;

                        // Validamos si existe el rango
                        var check = AllRanks.Where(x => x.Value.RankId == GetRank);
                        if (check.Count() <= 0)
                            return;

                        switch (WSAction)
                        {
                            #region Save Rank
                            case "saverank":
                                {
                                    int RankPay = 0, RankTimer = 0;
                                    string RankName = ReceivedData[3];

                                    #region Conditions
                                    if (String.IsNullOrWhiteSpace(RankName))
                                    {
                                        Client.SendWhisper("Ese nombre de puesto no es válido.", 1);
                                        return;
                                    }
                                    if (RankName.Length > 50)
                                    {
                                        Client.SendWhisper("Ese nombre de puesto es demasiado largo.", 1);
                                        return;
                                    }

                                    if (!int.TryParse(ReceivedData[4], out RankPay) || !int.TryParse(ReceivedData[5], out RankTimer))
                                    {
                                        Client.SendWhisper("La paga y el Tiempo de trabajo deben ser números enteros.", 1);
                                        return;
                                    }

                                    if (RankPay < 0)
                                    {
                                        Client.SendWhisper("La paga no puede ser negativa.", 1);
                                        return;
                                    }

                                    if (RankTimer < 5)
                                    {
                                        Client.SendWhisper("El Tiempo de trabajo mínimo es de 5 minutos.", 1);
                                        return;
                                    }
                                    #endregion

                                    // Actualizamos DB y Diccionario en este método.
                                    Group.UpdateJobSettings(GetRank, RankName, RankPay, RankTimer);
                                    Client.SendWhisper("Cambios guardados satisfactoriamente.", 1);
                                }
                                break;
                            #endregion

                            #region Save Look
                            case "savelook":
                                {
                                    string NewLook = ReceivedData[3];
                                    string Figure = "";
                                    string Gender = "";

                                    #region Conditions
                                    if (String.IsNullOrWhiteSpace(NewLook))
                                    {
                                        Client.SendWhisper("Uniforme inválido.", 1);
                                        return;
                                    }

                                    try
                                    {
                                        string[] LookPart = NewLook.Split('=');
                                        Gender = LookPart[1];

                                        string[] GetParts = NewLook.Split('&');
                                        Figure = GetParts[0];
                                    }
                                    catch (Exception)
                                    {
                                        Client.SendWhisper("((No se pudo obtener la información del Uniforme))", 1);
                                    }

                                    if ((Gender != "M" && Gender != "F") || String.IsNullOrWhiteSpace(Figure))
                                    {
                                        Client.SendWhisper("Uniforme inválido.", 1);
                                        return;
                                    }
                                    #endregion

                                    // Actualizamos DB y Diccionario en este método.
                                    Group.UpdateJobLooks(GetRank, Figure, Gender);
                                    Client.SendWhisper("Ropa guardada satisfactoriamente.", 1);
                                }
                                break;
                            #endregion

                            #region Permissions
                            case "permissions":
                                {
                                    string TypeCMD = ReceivedData[3];

                                    if (TypeCMD != "ascdesc" && TypeCMD != "hire" && TypeCMD != "fire" && TypeCMD != "supply")
                                        return;

                                    // Actualizamos DB y Diccionario en este método.
                                    Group.UpdateJobCommads(GetRank, TypeCMD);
                                    Client.SendWhisper("Permisos guardados satisfactoriamente.", 1);

                                    #region Refresh Settings Window
                                    GroupRank ThisRank = GroupManager.GetJobRank(Group.Id, GetRank);

                                    if (ThisRank == null)
                                        return;

                                    string html = "";

                                    #region HTML

                                    html += "<div class=\"heading\">Nombre del Puesto</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<input id=\"RankNewName\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"" + ThisRank.Name + "\">";
                                    html += "</div>";
                                    html += "<br>";

                                    html += "<div class=\"heading\" style=\"width: 34%;float: left;\">Paga del Puesto</div>";
                                    html += "<div class=\"heading\" style=\"width: 45%;float: right;\">Jornada de Tiempo (Minutos)</div>";

                                    html += "<div class=\"flex\" style=\"width: 45%;float:left;\">";
                                    html += "<input id=\"RankPay\" type=\"number\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" min=\"0\" value=\"" + ThisRank.Pay + "\">";
                                    html += "</div>";

                                    html += "<br>";
                                    html += "<div class=\"flex\">";
                                    html += "<button data-rank=\"" + GetRank + "\" data-action=\"SaveRank\" class=\"dark-button\" style=\"width: 100%;\">Guardar Cambios</button>";
                                    html += "</div>";

                                    html += "<br>";
                                    html += "<div class=\"heading\">Permisos</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<table class=\"dark\">";
                                    html += "<tr class=\"dark2\">";
                                    html += "<th class=\"dark2\">Ascender/Descender</th>";
                                    html += "<th class=\"dark2\">Contratar</th>";
                                    html += "<th class=\"dark2\">Despedir</th>";
                                    html += "<th class=\"dark2\">Abastecer</th>";
                                    html += "</tr>";
                                    html += "<tr class=\"dark2\">";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"ascdesc\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("ascdesc"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"hire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("hire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"fire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("fire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"supply\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("supply"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "</tr>";
                                    html += "</table>";
                                    html += "</div>";
                                    html += "<br>";
                                    #endregion

                                    string SendData = "";
                                    SendData += html + "|";// EventData[2];
                                    SendData += ThisRank.MaleFigure + "|";// EventData[3];
                                    SendData += ThisRank.FemaleFigure + "|";// EventData[4];
                                    SendData += GetRank;// EventData[5];
                                    Socket.Send("compose_business|settings|" + SendData);
                                    #endregion
                                }
                                break;
                            #endregion

                            #region Default
                            default:
                                break;
                                #endregion
                        }
                    }
                    break;
                #endregion


                #region Create
                case "create":
                    {
                        /*string[] ReceivedData = Data.Split(',');

                        string GName = Regex.Replace(ReceivedData[1], "<(.|\\n)*?>", string.Empty); ;
                        string GActivity = ReceivedData[2];

                        if (String.IsNullOrWhiteSpace(GName) || GName.Length > 50)
                        {
                            Client.SendWhisper("Ese nombre no es válido. Demasiado corto o demasiado largo.", 1);
                            return;
                        }

                        if (GActivity != "Restaurant" && GActivity != "Concesionario" && GActivity != "Tecnologia" && GActivity != "Ropa" && GActivity != "Spa" && GActivity != "24/7")
                        {
                            Client.SendWhisper("Ha ocurrido un error indeterminado.", 1);
                            return;
                        }

                        House House = PolarEnvironment.GetGame().GetHouseManager().GetTerrainByInsideRoom(Room.Id);

                        if (House == null)
                        {
                            Client.SendWhisper("Debes estar en el interior de algun Terreno a tu nombre.", 1);
                            return;
                        }

                        if (House.OwnerId != Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("No puedes abrir una empresa en un Terreno ajeno.", 1);
                            return;
                        }

                        // Imposible pero por seguridad condicionamos
                        if (Client.GetRoleplay().JobId > 0)
                        {
                            Client.SendWhisper("Ya eres propietario de una empresa. No puedes ser propietario de más de una empresa a la vez.", 1);
                            return;
                        }

                        #region Check my Jobs
                        bool CanCreate = false;
                        // Obtenemos los Trabajos del Target
                        List<Group> Jobs = PolarEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id);
                        if (Jobs == null)
                        {
                            Client.SendWhisper("Ocurrió un problema al buscar tus Trabajos. Intentalo más tarde.", 1);
                            return;
                        }

                        int TotalJobs = Jobs.Count;

                        if (TotalJobs == 0)
                        {
                            CanCreate = true;
                        }
                        else if (TotalJobs == 1)
                        {
                            #region Check Jobs by VIP Type
                            if (Client.GetHabbo().VIPRank > 0)
                            {
                                if (Jobs[0].GType != 1)// Si su primer trabajo no es de tipo EMPRESA
                                {
                                    CanCreate = true;
                                }
                            }
                            #endregion
                        }
                        #endregion

                        if (!CanCreate)
                        {
                            Client.SendWhisper("Ya te encuentras trabajando para una empresa. Primero renuncia a ella para poder ser propietario de una.", 1);
                            return;
                        }

                        // Creamos el Grupo en sala actual
                        Group Group = null;
                        if (!PolarEnvironment.GetGame().GetGroupManager().TryCreateBusiness(Client.GetHabbo(), 1, GName, "Empresa privada de " + GActivity, Room.Id, RoleplayManager.CdnURL + "/ws_overlays/Business/resources/images/new.gif", "", "", GActivity, true, out Group))
                        {
                            Client.SendNotification("Ha ocurrido un error mientras se creaba la Empresa/Negocio\n\nInténtalo de nuevo. Si el problema persiste, reportalo a un Administrador.");
                            return;
                        }

                        Room.Group = Group;

                        if (Client.GetHabbo().CurrentRoomId != Room.Id)
                            Client.SendMessage(new RoomForwardComposer(Room.Id));

                        Client.SendMessage(new NewGroupInfoComposer(Room.Id, Group.Id));*/
                    }
                    break;
                #endregion

                #region Suply
                case "supply":
                    {
                       /* if (Client.GetRoleplay().ViewCorpId <= 0)
                            return;

                        if (Client.GetRoleplay().TryGetCooldown("supply", true))
                            return;

                       Group Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().ViewCorpId);

                        if (Group == null)
                            return;

                        if (Group.Name.Contains("Policía"))
                            return;

                        bool isAdmin = false;

                        #region Check if is not Admin
                        if (!Group.IsAdmin(Client.GetHabbo().Id) && !Group.IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PolarEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "supply"))
                            return;
                        else
                            isAdmin = true;
                        #endregion

                        int StockCost = RoleplayManager.GetStockCost(Group);
                        int Cost = (100 - Group.Stock) * StockCost;

                        if (Cost <= 0)
                        {
                            Client.SendWhisper("Esta empresa no necesita reabastecerse aún. Stock: "+Group.Stock+" / 100", 1);
                            return;
                        }

                        if (Group.Bank < Cost)
                        {
                            Client.SendWhisper("El empresa necesita $ " + String.Format("{0:N0}", Cost) + " para abastecer el Stock.", 1);
                            return;
                        }

                        Group.SetBussines((Group.Bank - Cost), 100);
                        Group.UpdateSpend(Cost);

                        RoleplayManager.Shout(Client, "*Ha reabastecido el Stock de la empresa "+ Group.Name +"*", 5);
                        PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "open_room");
                        PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                        Client.GetRoleplay().CooldownManager.CreateCooldown("supply", 1000, 15);*/
                    }
                    break;
                #endregion

                #region Finance List
                case "finance":
                    {
                        if (Client.GetRoleplay().ViewCorpId <= 0)
                            return;

                        Group Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().ViewCorpId);

                        if (Group == null)
                            return;

                        bool isAdmin = false;

                        #region Check if is Admin
                        if (Group.IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override_finances"))
                            isAdmin = true;
                        #endregion

                        string SendData = "";

                        #region HTML
                        SendData += "<div>";
                        SendData += "<div class=\"mb-2\">";
                        SendData += "<div class=\"heading\">Donar a la Empresa</div>";
                        SendData += "<div class=\"-m-1 flex flex-wrap justify-around\" style=\"margin-bottom: 5px;display:flex;\">";
                        SendData += "<input id=\"Bus_Input_Donation\" type=\"number\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" autocomplete=\"off\" placeholder=\"Cantidad de dinero a donar\">";
                        SendData += "<button id=\"Bus_Btn_Donation\" class=\"dark-button\">Donar</button></div>";
                        if (isAdmin)
                        {
                            SendData += "<div class=\"heading\">Retirar de la Empresa (Solo Fundador)</div>";
                            SendData += "<div class=\"-m-1 flex flex-wrap justify-around\" style=\"margin-bottom: 5px;display:flex;\">";
                            SendData += "<input id=\"Bus_Input_Withdraw\" type=\"number\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" autocomplete=\"off\" placeholder=\"Cantidad de dinero a retirar\">";
                            SendData += "<button id=\"Bus_Btn_Withdraw\" class=\"dark-button\">Retirar</button></div>";
                        }
                        SendData += "</div>";
                        SendData += "</div>";
                        #endregion

                        Socket.Send("compose_business|finance|" + SendData);
                    }
                    break;
                #endregion

                #region Finance Buttons
                case "finan":
                    {
                        if (Client.GetRoleplay().TryGetCooldown("finan", true))
                            return;

                        if (Client.GetRoleplay().ViewCorpId <= 0)
                            return;

                        Group Group = PolarEnvironment.GetGame().GetGroupManager().GetJobByID(Client.GetRoleplay().ViewCorpId);

                        if (Group == null)
                            return;

                        bool isAdmin = false;

                        #region Check if is Admin
                        if (Group.IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override_finances"))
                            isAdmin = true;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        string act = ReceivedData[1];
                        int cant = 0;
                        act = Regex.Replace(act, "<(.|\\n)*?>", string.Empty);

                        if (!int.TryParse(ReceivedData[2], out cant))
                        {
                            Socket.Send("compose_business|msg_error|Cantidad de dinero inválida.");
                            return;
                        }

                        switch (act)
                        {
                            case "donation":
                                {
                                    List<GroupMember> Administrators = Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();
                                    if (Client.GetHabbo().Rank < 3)
                                    {
                                        if (Administrators.Count <= 0)
                                        {
                                            Socket.Send("compose_business|msg_error|Esta empresa pertenece al Gobierno y no necesita donaciones.");
                                            return;
                                        }
                                    }

                                    if (Client.GetRoleplay().Level < 2)
                                    {
                                        Socket.Send("compose_business|msg_error|Necesitar ser al menos nivel 2 para donar.");
                                        return;
                                    }

                                    if (Client.GetHabbo().Credits < cant)
                                    {
                                        Socket.Send("compose_business|msg_error|No tienes esa cantidad de dinero para donar.");
                                        return;
                                    }

                                    if(cant < 100)
                                    {
                                        Socket.Send("compose_business|msg_error|Debes donar al menos una cantidad de $100.");
                                        return;
                                    }

                                    Client.GetHabbo().Credits -= cant;
                                    Client.GetHabbo().UpdateCreditsBalance();

                                    Group.Balance += cant;
                                    Group.SetBussines(Group.Balance);

                                    RoleplayManager.Shout(Client, "*Ha donado $ " + String.Format("{0:N0}", cant) + " a la empresa " + Group.Name + "*", 5);
                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "finance");
                                    Socket.Send("compose_business|msg_success|Donación realizada exitosamente.");

                                }
                                break;
                            case "withdraw":
                                {
                                    List<GroupMember> Administrators = Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();

                                    if (Administrators.Count <= 0)
                                    {
                                        Socket.Send("compose_business|msg_error|Esta empresa pertenece al Gobierno y no es posible retirar dinero.");
                                        return;
                                    }

                                    if (Group.Name.Contains("Policia") && !Client.GetHabbo().GetPermissions().HasRight("group_management_override_finances"))
                                    {
                                        Socket.Send("compose_business|msg_error|No tienes permitido retirar dinero de esta empresa.");
                                        return;
                                    }

                                    if (!isAdmin)
                                    {
                                        Socket.Send("compose_business|msg_error|Solo el fundador de la empresa puede hacer eso.");
                                        return;
                                    }

                                    if (Group.Balance < cant)
                                    {
                                        Socket.Send("compose_business|msg_error|La empresa no cuenta con esa cantidad en su Banco para retirar.");
                                        return;
                                    }

                                    if (cant <= 0)
                                    {
                                        Socket.Send("compose_business|msg_error|Debes retirar una cantidad mayor a $ 0.");
                                        return;
                                    }

                                    Client.GetHabbo().Credits += cant;
                                    Client.GetHabbo().UpdateCreditsBalance();

                                    Group.Balance -= cant;
                                    Group.SetBussines(Group.Balance);

                                    RoleplayManager.Shout(Client, "*Ha retirado $ " + String.Format("{0:N0}", cant) + " de la empresa " + Group.Name + "*", 5);
                                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_business", "finance");
                                    Group.AddLog(Client.GetHabbo().Id, Client.GetHabbo().Username + " retira $" + cant, cant);
                                    Socket.Send("compose_business|msg_success|Retiro realizado exitosamente.");
                                }
                                break;
                            default:
                                break;
                        }

                        Client.GetRoleplay().CooldownManager.CreateCooldown("finan", 1000, 30);
                    }
                    break;
                    #endregion
            }
        }
    }
}
