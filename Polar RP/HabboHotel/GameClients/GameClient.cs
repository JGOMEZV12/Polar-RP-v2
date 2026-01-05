using System.Data;
using Polar.Net;
using Polar.Core;
using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;
using Polar.Communication.Interfaces;
using Polar.HabboHotel.Users.UserDataManagement;
using ConnectionManager;
using Polar.Communication.Packets.Outgoing;
using Polar.Communication.Packets.Outgoing.Sound;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;
using Polar.Communication.Packets.Outgoing.Rooms.Notifications;
using Polar.Communication.Packets.Outgoing.Handshake;
using Polar.Communication.Packets.Outgoing.Navigator;
using Polar.Communication.Packets.Outgoing.Moderation;
using Polar.Communication.Packets.Outgoing.BuildersClub;
using Polar.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Polar.Communication.Packets.Outgoing.Inventory.Achievements;
using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.Communication.Packets.Outgoing.Rooms.Session;
using Polar.HabboRoleplay.PhoneOwned;
using Polar.Communication.Encryption.Crypto.Prng;
using Polar.HabboHotel.Users.Messenger.FriendBar;
using Polar.HabboHotel.Moderation;
using Polar.HabboRoleplay.Misc;
using Polar.Database.Interfaces;
using Polar.HabboHotel.Subscriptions;
using Polar.HabboHotel.Permissions;
using Polar.HabboRoleplay.RoleplayUsers;
using Polar.HabboRoleplay.Events;
using SharedPacketLib;

namespace Polar.HabboHotel.GameClients
{
    public class GameClient
    {
        private readonly int _id;
        private Habbo? _habbo;
        public RoleplayUser? _roleplay;
        public string? MachineId;
        private bool _disconnected;
        public ARC4? RC4Client = null;
        private GamePacketParser _packetParser;
        private ConnectionInformation _connection;
        public bool LoggingOut = false;
        public int PingCount { get; set; }
        public int RobberyUsers = 1;
        public string? AuthTicket { get; private set; }
        private readonly Dictionary<string, Subscription>? Subscriptions;
        public GameClient(int ClientId, ConnectionInformation pConnection)
        {
            this._id = ClientId;
            this._connection = pConnection;
            this._packetParser = new GamePacketParser(this);

            this.PingCount = 0;
        }

        private void SwitchParserRequest()
        {
            this._packetParser.OnNewPacket += new GamePacketParser.HandlePacket(this.parser_onNewPacket);

            byte[] packet = (this._connection.parser as InitialPacketParser).currentData;
            this._connection.parser.Dispose();
            this._connection.parser = (IDataParser)this._packetParser;
            this._connection.parser.handlePacketData(packet);
        }

        private void parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                PolarEnvironment.GetGame().GetPacketManager().TryExecutePacket(this, Message);
            }
            catch (Exception e)
            {
                Logging.LogPacketException(Message.ToString(), e.ToString());
            }
        }

        private void PolicyRequest()
        {
            _connection.SendData(PolarEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                   "<allow-intent href=\"wss://*/*\" />\r\n" +
                   "</cross-domain-policy>\x0"));
        }

        internal void SendNotification(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
        }

        /*public void StartConnection()
        {
            if (_connection != null)
            {
                (_connection.parser as InitialPacketParser).pol += PolicyRequest;
                (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
                _connection.startPacketProcessing();
            }
        }*/

        public void StartConnection()
        {
            if (this._connection == null)
                return;

            (this._connection.parser as InitialPacketParser).SwitchParserRequest += new InitialPacketParser.NoParamDelegate(this.SwitchParserRequest);

            this._connection.startPacketProcessing();
        }

        public bool TryAuthenticate(string AuthTicket)
        {
            try
            {
                byte errorCode = 0;
                string ip = GetConnection().getIp();
                UserData userData = UserDataFactory.GetUserData(AuthTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect(true);
                    return false;
                }

                #region Ban Checking
                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord = null;
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (PolarEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out BanRecord))
                    {
                        if (PolarEnvironment.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect(true);
                            return false;
                        }
                    }
                }

                if (userData.user != null)
                {
                    //Now let us check for a username ban record..
                    BanRecord = null;
                    if (PolarEnvironment.GetGame().GetModerationManager().IsBanned(userData.user.Username, out BanRecord))
                    {
                        if (PolarEnvironment.GetGame().GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect(true);
                            return false;
                        }
                    }
                }
                #endregion

                #region Roleplay Data
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `rp_products_owned` WHERE `product_id` = '" + RoleplayManager.BidonID + "' AND `user_id` = '" + userData.userID + "' LIMIT 1");
                    if (dbClient.getRow() == null)
                        dbClient.RunQuery("INSERT INTO `rp_products_owned` (`product_id`,`user_id`,`extradata`) VALUES ('" + RoleplayManager.BidonID + "', '" + userData.userID + "', '0')");

                    dbClient.SetQuery("SELECT `id` FROM `rp_products_owned` WHERE `product_id` = '" + RoleplayManager.MecPartsID + "' AND `user_id` = '" + userData.userID + "' LIMIT 1");
                    if (dbClient.getRow() == null)
                        dbClient.RunQuery("INSERT INTO `rp_products_owned` (`product_id`,`user_id`,`extradata`) VALUES ('" + RoleplayManager.MecPartsID + "', '" + userData.userID + "', '0')");

                    dbClient.SetQuery("SELECT `id` FROM `rp_products_owned` WHERE `product_id` = '" + RoleplayManager.ArmMatID + "' AND `user_id` = '" + userData.userID + "' LIMIT 1");
                    if (dbClient.getRow() == null)
                        dbClient.RunQuery("INSERT INTO `rp_products_owned` (`product_id`,`user_id`,`extradata`) VALUES ('" + RoleplayManager.ArmMatID + "', '" + userData.userID + "', '0')");

                    dbClient.SetQuery("SELECT `id` FROM `rp_products_owned` WHERE `product_id` = '" + RoleplayManager.ArmPiecesID + "' AND `user_id` = '" + userData.userID + "' LIMIT 1");
                    if (dbClient.getRow() == null)
                        dbClient.RunQuery("INSERT INTO `rp_products_owned` (`product_id`,`user_id`,`extradata`) VALUES ('" + RoleplayManager.ArmPiecesID + "', '" + userData.userID + "', '0')");
                    
                    dbClient.SetQuery("SELECT * FROM `rp_stats` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPRow = dbClient.getRow();

                    dbClient.SetQuery("SELECT * FROM `rp_stats_cooldowns` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPCooldowns = dbClient.getRow();

                    if (UserRPCooldowns == null)
                    {
                        dbClient.RunQuery("INSERT INTO `rp_stats_cooldowns` (`id`) VALUES ('" + userData.userID + "')");
                        dbClient.SetQuery("SELECT * FROM `rp_stats_cooldowns` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                        UserRPCooldowns = dbClient.getRow();
                    }

                    dbClient.SetQuery("SELECT * FROM `rp_stats_farming` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPFarming = dbClient.getRow();

                    if (UserRPFarming == null)
                    {
                        dbClient.RunQuery("INSERT INTO `rp_stats_farming` (`id`) VALUES ('" + userData.userID + "')");
                        dbClient.SetQuery("SELECT * FROM `rp_stats_farming` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                        UserRPFarming = dbClient.getRow();
                    }

                    _roleplay = new RoleplayUser(this, UserRPRow, UserRPCooldowns, UserRPFarming);

                    if (UserRPRow != null)
                    {
                        string Number = "";

                        List<PhonesOwned> PO = PolarEnvironment.GetGame().GetPhonesOwnedManager().getMyPhonesOwned(userData.userID);
                        if (PO != null && PO.Count > 0)
                        {
                            Number = PO[0].PhoneNumber;
                        }

                        if (Number.Length > 0)//(xxx)-xxx-xxxx  14 lenght
                            PolarEnvironment.GetGame().GetClientManager().RegisterClientPhone(this, userData.userID, Number);
                    }
                }
                #endregion

                PolarEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.userID, userData.user.Username);
                _habbo = userData.user;
                //_habbo.ssoTicket = AuthTicket;

                if (_habbo != null)
                {
                    //GetRoleplay().UpdateTimerDialogue("Stop-Intro", "remove", 0, 0);

                    userData.user.Init(this, userData);
                    if (!RoleplayManager.PreLoadedRooms)
                    {
                        RoleplayManager.PreLoadedRooms = true;
                        PolarEnvironment.GetGame().GetRoomManager().PreLoadRooms();
                    }
                    SendMessage(new AuthenticationOKComposer());
                    SendMessage(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    //SendMessage(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    SendMessage(new RoomForwardComposer(GetHabbo().HomeRoom == 0 ? 1 : GetHabbo().HomeRoom));
                    //SendMessage(new RoomForwardComposer(_habbo.HomeRoom == 0 ? 1 : _habbo.HomeRoom));


                    SendMessage(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendMessage(new AvailabilityStatusComposer());
                    SendMessage(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingAllParts));

                    SendMessage(new UserRightsComposer(_habbo));

                    SendMessage(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    SendMessage(new BuildersClubMembershipComposer());
                    SendMessage(new CfhTopicsInitComposer());
                    SendMessage(new BadgeDefinitionsComposer(PolarEnvironment.GetGame().GetAchievementManager()._achievements));
                    SendMessage(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference, FriendBarStateUtility.GetInt(_habbo.FriendbarState)));



                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (this._habbo.MachineId != MachineId)
                        {
                            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }
                        }

                        _habbo.MachineId = MachineId;
                    }

                    PermissionGroup PermissionGroup = null;
                    if (PolarEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup))
                    {
                        if (!String.IsNullOrEmpty(PermissionGroup.Badge))
                            if (!_habbo.GetBadgeComponent().HasBadge(PermissionGroup.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(PermissionGroup.Badge, true, this);
                    }

                    SubscriptionData SubData = null;
                    if (PolarEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(this._habbo.VIPRank, out SubData))
                    {
                        if (!String.IsNullOrEmpty(SubData.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(SubData.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(SubData.Badge, true, this);
                        }
                    }

                    if (!PolarEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                        PolarEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);

                    _habbo.InitProcess();
                    

                    if (userData.user.GetPermissions().HasRight("mod_tickets"))
                    {
                        SendMessage(new ModeratorInitComposer(
                          PolarEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                          PolarEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                          PolarEnvironment.GetGame().GetModerationManager().UserActionPresets,
                          PolarEnvironment.GetGame().GetModerationTool().GetTickets));
                    }
                    
                    if (GetHabbo().GetPermissions().HasRight("mod_tool"))
                    {
                        PolarEnvironment.GetGame().GetClientManager().StaffAlert(new RoomBubbleNotificationComposer("ADM", "El staff " + this.GetHabbo().Username + " acaba de conectarse", ""), GetHabbo().Id);
                    }

                    if (GetHabbo().GetClubManager().HasSubscription("habbo_vip") && GetHabbo().VIPRank > 0)
                    {
                        GetHabbo().GetClubManager().TimeExpired("habbo_vip", GetHabbo().GetClubManager().GetSubscription("habbo_vip").ExpireTime, this);
                        //SendMessage(new UserNameChangeComposer(GetRoomUser().GetRoom().Id, GetRoomUser().VirtualId, "[VIP] " + GetHabbo().Username));
                    }

                    _habbo.InitSearches();
                    this.AuthTicket = AuthTicket;

                   

                    if (GetRoleplay().OriginalOutfit == null)
                        GetRoleplay().OriginalOutfit = GetHabbo().Look;

                    DeathCheck(this);
                    CuffCheck(this);
                    JailCheck(this);
                    WantedCheck(this);
                    StunCheck(this);
                    NoobCheck(this);
                    PhoneCheck(this);
                    //SocketConnection(Client);
                    CheckWS(this);

                    PolarEnvironment.GetGame().GetRewardManager().CheckRewards(this);
                    //EventManager.TriggerEvent("OnLogin", this);

                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Bug during user login: " + e);
            }
            return false;
        }

        #region Check WS Connect
        public void CheckWS(GameClient Client)
        {
            if (!PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true))
            {
                //Console.WriteLine("Los WebSockets NO conectaron");
                //Client.SendMessage(new BroadcastMessageAlertComposer("¡Los WebSockets NO conectaron!\n\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\n\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee."));
                //Client.SendNotification("¡Los WebSockets NO conectaron!\n\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\n\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee.");
                Logging.WriteLine(Client.GetHabbo().Username + " se ha conectado.", ConsoleColor.DarkGreen);
            }
            else
            {
                //Client.SendMessage(new BroadcastMessageAlertComposer("¡Los WebSockets NO. conectaron!\nEstos son necesarios para que pueas visualizar Ventanas Roleplay, tus stats, y muchas más herramientas dentro del servidor.\nIntenta reiniciar el Client para intentar reconectar.\nSi el problema persiste, asegurate de no teneer algún programa externo que lo bloquee."));
                SocketConnection(Client);
                Logging.WriteLine(Client.GetHabbo().Username + " se ha conectado. (+WS)", ConsoleColor.DarkGreen);
            }
        }
        #endregion

        #region SocketConnection
        public void SocketConnection(GameClient Client)
        {
            //Client.GetRoleplay().RefreshStatDialogue();
            Client.GetRoleplay().InitWSDialogues();
            Client.GetRoleplay().InitStatDialogue();
            //Logging.WriteLine(Client.GetHabbo().Username + " [ID:" + Client.GetHabbo().Id + "]" + " Se ha conectado " + "[SalaID:" + Client.GetHabbo().HomeRoom + "] ", ConsoleColor.DarkGreen);
        }
        #endregion

        #region CuffCheck
        public void CuffCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().Cuffed)
                return;
            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("cuff"))
                Client.GetRoleplay().TimerManager.CreateTimer("cuff", 1000, true);
        }
        #endregion

        #region DeathCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        public void DeathCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsDead)
                return;

            string MyCity = "heticosrp";

            HabboRoleplay.RPRoom.RPRoom Data;
            int HospitalRID = PolarEnvironment.GetGame().GetRPRoomManager().TryToGetHospital(MyCity, out Data);

            if (Client.GetHabbo().CurrentRoomId != HospitalRID)
            {
                RoleplayManager.SendUser(Client, HospitalRID);
                //Client.SendNotification("¡No puedes dejar el hospital mientras estás muerto!");
            }
            RoleplayManager.GetLookAndMotto(Client);
            RoleplayManager.SpawnBeds(Client, "hosptl_bed");

            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("death"))
                Client.GetRoleplay().TimerManager.CreateTimer("death", 1000, true);
        }
        #endregion

        #region JailCheck
        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void JailCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsJailed)
                return;

            if (JailbreakManager.JailbreakActivated)
            {
                Client.GetRoleplay().Jailbroken = true;
                Client.SendNotification("¡Alguien ha iniciado un jailbreak mientras estabas offline! ¡Corre mejor antes de que te pillen!");
                return;
            }

            if (Client.GetRoleplay().IsWanted || Client.GetRoleplay().WantedLevel != 0 || Client.GetRoleplay().WantedTimeLeft != 0)
            {
                Client.GetRoleplay().IsWanted = false;
                Client.GetRoleplay().WantedLevel = 0;
                Client.GetRoleplay().WantedTimeLeft = 0;
            }

            string MyCity = "heticosrp";

            HabboRoleplay.RPRoom.RPRoom Data;
            int ToRoomId = PolarEnvironment.GetGame().GetRPRoomManager().TryToGetJail(MyCity, out Data);

            if (Client.GetHabbo().HomeRoom != ToRoomId)
                Client.GetHabbo().HomeRoom = ToRoomId;

            RoleplayManager.SendUser(Client, ToRoomId, "");

            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("jail"))
                Client.GetRoleplay().TimerManager.CreateTimer("jail", 1000, true);

        }
        #endregion

        #region StuNCheck
        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void StunCheck(GameClient Client)
        {
            if (Client.GetRoleplay().IsStun == false)
                return;

            if (Client.GetRoleplay().TryGetCooldown("stun"))
            {
                Client.GetRoleplay().IsStun = true;
                Client.GetRoleplay().IsJailed = true;

                if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("jail"))
                    Client.GetRoleplay().TimerManager.CreateTimer("jail", 1000, true);
            }

        }
        #endregion

        #region Wanted Check
        /// <summary>
        /// Checks if the user is wanted
        /// </summary>
        /// <param name="Client"></param>
        public void WantedCheck(GameClient Client)
        {
            // WS Wanted Stars
            if (Client.GetRoleplay().WebSocketConnection != null)
            {
                if (Client.GetRoleplay().WantedLevel > 0)
                {
                    Wanted NewWanted = new Wanted(Convert.ToUInt32(Client.GetHabbo().Id), "Desconocida", Client.GetRoleplay().WantedLevel);
                    Client.GetRoleplay().TimerManager.CreateTimer("wanted", 1000, false);
                    RoleplayManager.WantedList.TryAdd(Client.GetHabbo().Id, NewWanted);
                    PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_wanted_stars|" + Client.GetRoleplay().WantedLevel);
                }
            }

            if (!Client.GetRoleplay().IsJailed)
                return;
            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("jail"))
                Client.GetRoleplay().TimerManager.CreateTimer("jail", 1000, true);
        }
        #endregion

        #region NoobCheck

        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void NoobCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsNoob)
                return;

            Client.GetRoleplay().TimerManager.CreateTimer("noob", 1000, true);
        }
        #endregion

        #region PhoneCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        /// 
        public void PhoneCheck(GameClient Client)
        {
            if (Client.GetRoleplay().Phone <= 0)
                return;

            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "load_apps");
            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "show_button");
        }
        #endregion

        public void SendWhisper(string Message, int Colour = 0)
        {
            if (this == null || GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Id);
            if (User == null)
                return;

            SendMessage(new WhisperComposer(User.VirtualId, Message, 0, (Colour == 0 ? User.LastBubble : Colour)));
        }

        internal void SendNotifWithScroll(string Message)
        {
            SendMessage(new MOTDNotificationComposer(Message));
        }
        public void Shout(string Message, int Colour = 4)
        {
            RoleplayManager.Shout(this, Message, Colour);
        }

        public void SendNotification(string Message)
        {
            SendMessage(new RoomNotificationComposer("Notificación", Message, "", "OK", "event:"));
        }

        public void SendMessage(IServerPacket Message)
        {
            byte[] bytes = Message.GetBytes();

            if (Message == null)
                return;

            if (GetConnection() == null)
                return;

            GetConnection().SendData(bytes);
        }

        public void SendSimple(int result, string message)
        {
            SendMessage(new ModeratorSupportTicketResponseComposer(result, message));
        }

        public void SendHugeNotif(string Message)
        {
            ServerPacket serverPacket = new ServerPacket(ServerPacketHeader.MOTDNotificationMessageComposer);
            serverPacket.WriteInteger(1);
            serverPacket.WriteString(Message);
            SendMessage(serverPacket);
        }

        public int ConnectionID
        {
            get { return _id; }
        }

        public ConnectionInformation GetConnection()
        {
            return _connection;
        }

        public Habbo GetHabbo()
        {
            return _habbo;
        }
        public RoleplayUser GetRoleplay()
        {
            return _roleplay;
        }

        public RoomUser GetRoomUser()
        {
            RoomUser RUser = null;
            try
            {
                if (this == null || GetHabbo() == null || GetHabbo().CurrentRoom == null)
                    return null;

                RUser = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Id);
            }
            catch
            {
                return RUser;
            }

            return RUser;
        }

        public void Disconnect(bool ForcedDisconnect)
        {
            if (LoggingOut)
                return;

            LoggingOut = true;

            if (!_disconnected)
            {
                if (_connection != null)
                    _connection.Dispose();
                _disconnected = true;
            }
        }


        /* public void Disconnect(bool ForcedDisconnect)
         {

             if (LoggingOut)
                 return;

             LoggingOut = true;

             try
             {
                 #region WebSocket 
                 PolarEnvironment.GetGame().GetWebEventManager().CloseSocketByGameClient(((this.GetHabbo() == null) ? 0 : this.GetHabbo().Id));
                 if (GetRoleplay() != null)
                 {
                     foreach (WebSocketChatRoom ChatRoom in GetRoleplay().ChatRooms.Values)
                     {
                         if (ChatRoom == null) { continue; }
                         WebSocketChatManager.Disconnect(this, ChatRoom.ChatName, false, null);
                     }
                 }
                 #endregion

                 if (GetRoomUser() != null && !ForcedDisconnect)
                 {
                     RoleplayManager.Chat(this, GetHabbo().Username + " Se ha desconectado, desaparecerá en 10 segundos", 1);
                     GetRoomUser().ApplyEffect(108);

                     if (GetHabbo() != null && GetHabbo().CurrentRoom != null)
                         GetHabbo().CurrentRoom.SendMessage(new SleepComposer(GetRoomUser(), true));

                     GetRoomUser().CanWalk = false;
                     GetRoomUser().ClearMovement(true);
                 }

                 if (GetRoleplay() != null)
                 {
                     if (GetRoleplay().UserDataHandler != null)
                     {
                         GetRoleplay().UserDataHandler.SaveFarmingData();
                         GetRoleplay().UserDataHandler.SaveCooldownData();
                         GetRoleplay().UserDataHandler.SaveData();
                         GetRoleplay().UserDataHandler = null;
                     }
                 }

                 if (GetHabbo() != null)
                 {
                     using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                         dbClient.RunQuery(GetHabbo().GetQueryString);
                 }

                 if (!ForcedDisconnect)
                 {
                     new Thread(() =>
                     {
                         Thread.Sleep(8000);

                         if (GetRoleplay() != null)
                         {
                             GetRoleplay().UserDataHandler = new UserDataHandler(this, GetRoleplay());
                             GetRoleplay().UserDataHandler.SaveFarmingData();
                             GetRoleplay().UserDataHandler.SaveCooldownData();
                             GetRoleplay().UserDataHandler.SaveData();
                             GetRoleplay().UserDataHandler = null;
                         }

                         if (GetHabbo() != null)
                         {
                             using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                                 dbClient.RunQuery(GetHabbo().GetQueryString);
                         }

                         EventManager.TriggerEvent("OnDisconnect", this);

                         Thread.Sleep(1000);

                         if (GetRoomUser() != null)
                             GetRoomUser().ApplyEffect(108);

                         Thread.Sleep(1000);

                         if (GetHabbo() != null)
                         {
                             using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                             {
                                 dbClient.RunQuery(GetHabbo().GetQueryString);
                             }

                             GetHabbo().OnDisconnect();
                            // this.Dispose();
                         }

                         if (!_disconnected)
                         {
                             if (_connection != null)
                                 _connection.Dispose();
                             _disconnected = true;
                         }
                     }).Start();
                 }
                 else
                 {
                     EventManager.TriggerEvent("OnDisconnect", this);

                     if (GetHabbo() != null)
                     {
                         using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                         {
                             dbClient.RunQuery(GetHabbo().GetQueryString);
                         }

                         GetHabbo().OnDisconnect();
                         //this.Dispose();
                     }

                     if (!_disconnected)
                     {
                         if (_connection != null)
                             _connection.Dispose();
                         _disconnected = true;
                     }
                 }
             }
             catch (Exception e)
             {
                 Logging.LogException(e.ToString());
             }
         }
        */
        public void Dispose(int ClientId)
        {
            PolarEnvironment.GetGame().GetWebEventManager().CloseSocketByGameClient(((this.GetHabbo() == null) ? 0 : this.GetHabbo().Id));
            EventManager.TriggerEvent("OnDisconnect", this);

            //System.Timers.Timer timer1 = new System.Timers.Timer(10000);
            //timer1.Interval = 10000;
            //timer1.Elapsed += delegate
            //{

                if (GetHabbo() != null)
                GetHabbo().OnDisconnect();

            PolarEnvironment.GetGame().GetClientManager().removeConnection(ClientId);
            this.MachineId = string.Empty;
            if (!_disconnected)
            {
                if (_connection != null)
                    _connection.Dispose();
                _disconnected = true;
            }
            this._roleplay = null;
            this._habbo = null;
            this._connection = null;
            this.RC4Client = null;
            this._packetParser = null;
               // timer1.Stop();
            //};
            //timer1.Start();

        }
    }
}