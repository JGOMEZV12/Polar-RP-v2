using System.Collections.Concurrent;
using System.Drawing;
using Polar.Communication.Packets.Outgoing.Rooms.Avatar;
using Polar.Core;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboHotel.Rooms.Pathfinding;
using Polar.HabboHotel.Rooms.AI;
using Polar.HabboRoleplay.Misc;
using Polar.HabboRoleplay.Bots.Manager;
using Polar.Utilities;
using System.Data;
using Polar.Communication.Packets.Outgoing.Rooms.Session;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Communication.Packets.Outgoing.Rooms.Permissions;
using Polar.Communication.Packets.Outgoing.Handshake;
using Polar.HabboHotel.Rooms.Games.Teams;
using Polar.Database.Interfaces;
using Polar.HabboRoleplay.Gambling;
using log4net;

namespace Polar.HabboHotel.Rooms
{
    public class RoomUserManager
    {
        private readonly Room _room;
        public ConcurrentDictionary<int, RoomUser> _users;
        public ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;
        private ConcurrentDictionary<string, RoomUser> _usersByUsername;
        private ConcurrentDictionary<int, RoomUser> _usersByUserID;

        public int primaryPrivateUserID;
        public int secondaryPrivateUserID;
        //private bool BoostingCheck;
        public int userCount;
        private int petCount;

        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Rooms.Room");


        public RoomUserManager(Room room)
        {
            this._room = room;
            this._users = new ConcurrentDictionary<int, RoomUser>();
            this._pets = new ConcurrentDictionary<int, RoomUser>();
            this._bots = new ConcurrentDictionary<int, RoomUser>();
            this._usersByUsername = new ConcurrentDictionary<string, RoomUser>();
            this._usersByUserID = new ConcurrentDictionary<int, RoomUser>();

            this.primaryPrivateUserID = 0;
            this.secondaryPrivateUserID = 0;

            this.petCount = 0;
            this.userCount = 0;

            //Out.WriteLine("RoomUserManager -> CARGADO", "Polar.HabboHotel.Rooms.RoomUseManager", ConsoleColor.DarkGray);
        }

        public void Dispose()
        {
            this._users.Clear();
            this._pets.Clear();
            this._bots.Clear();

            this._usersByUsername.Clear();
            this._usersByUserID.Clear();

            this._users = null;
            this._pets = null;
            this._bots = null;
        }


        public RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            var BotUser = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            Bot.VirtualId = primaryPrivateUserID;

            int PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, BotUser);

            DynamicRoomModel Model = _room.GetGameMap().Model;

            if ((Bot.X > 0 && Bot.Y > 0) && Bot.X < Model.MapSizeX && Bot.Y < Model.MapSizeY)
            {
                BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                BotUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = Model.DoorX;
                Bot.Y = Model.DoorY;

                BotUser.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                BotUser.SetRot(Model.DoorOrientation, false);
            }

            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAI(BotUser.VirtualId);

            if (BotUser.IsPet)
            {
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);
                BotUser.PetData = PetData;
                BotUser.PetData.VirtualId = BotUser.VirtualId;
            }
            else
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);

            //UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;

            _room.SendMessage(new UsersComposer(BotUser));

            if (BotUser.IsPet)
            {
                if (_pets.ContainsKey(BotUser.PetData.PetId)) //Pet allready placed
                    _pets[BotUser.PetData.PetId] = BotUser;
                else
                    _pets.TryAdd(BotUser.PetData.PetId, BotUser);

                petCount++;
            }
            else if (BotUser.IsBot)
            {
                if (_bots.ContainsKey(BotUser.BotData.BotId))
                    _bots[BotUser.BotData.BotId] = BotUser;
                else
                    _bots.TryAdd(BotUser.BotData.Id, BotUser);
                _room.SendMessage(new DanceComposer(BotUser, BotUser.BotData.DanceId));
            }
            return BotUser;
        }


        public void RemoveBot(int VirtualId, bool Kicked)
        {
            RoomUser User = GetRoomUserByVirtualId(VirtualId);
            if (User == null || !User.IsBot)
                return;

            if (User.IsPet)
            {
                RoomUser PetRemoval = null;

                _pets.TryRemove(User.PetData.PetId, out PetRemoval);
                petCount--;
            }
            else
            {
                RoomUser BotRemoval = null;
                _bots.TryRemove(User.BotData.Id, out BotRemoval);
            }

            User.BotAI.OnSelfLeaveRoom(Kicked);

            _room.SendMessage(new UserRemoveComposer(User.VirtualId));

            RoomUser toRemove;

            if (_users != null)
                _users.TryRemove(User.InternalRoomID, out toRemove);

            onRemove(User);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        public bool AddAvatarToRoom(GameClient Session)
        {
            if (_room == null)
                return false;

            if (Session == null)
                return false;

            if (Session.GetHabbo().CurrentRoom == null)
                return false;

            #region Old Stuff
            RoomUser User = new RoomUser(Session.GetHabbo().Id, _room.RoomId, primaryPrivateUserID++, _room);

            if (User == null || User.GetClient() == null)
                return false;

            User.UserId = Session.GetHabbo().Id;

            Session.GetHabbo().TentId = 0;

            int PersonalID = secondaryPrivateUserID++;
            User.InternalRoomID = PersonalID;


            Session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!this._users.TryAdd(PersonalID, User))
                return false;
            #endregion

            string Username = Session.GetHabbo().Username;
            int UserId = Session.GetHabbo().Id;

            if (this._usersByUsername.ContainsKey(Username.ToLower()))
                this._usersByUsername.TryRemove(Username.ToLower(), out User);

            if (this._usersByUserID.ContainsKey(UserId))
                this._usersByUserID.TryRemove(UserId, out User);

            this._usersByUsername.TryAdd(Username.ToLower(), User);
            this._usersByUserID.TryAdd(UserId, User);

            DynamicRoomModel Model = _room.GetGameMap().Model;
            if (Model == null)
                return false;

            if (!_room.PetMorphsAllowed && Session.GetHabbo().PetId != 0)
                Session.GetHabbo().PetId = 0;

            #region disabled temporary
            //if (!Session.GetHabbo().IsTeleporting && !Session.GetHabbo().IsHopping)
            #endregion
            if (Session.GetRoleplay().InsideTaxi || (Session.GetRoleplay().InsideBus || (!Session.GetHabbo().IsTeleporting && !Session.GetHabbo().IsHopping)))

            {
                if (!Model.DoorIsValid())
                {
                    Point Square = _room.GetGameMap().GetRandomWalkableSquare();
                    Model.DoorX = Square.X;
                    Model.DoorY = Square.Y;
                    Model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(Square);
                }
                
                #region Roleplay last spawn coordination

                if (!Session.GetRoleplay().AntiArrowCheck)
                {
                    object[] Coords = Session.GetRoleplay().LastCoordinates.Split(',');
                    int LastX = Convert.ToInt32(Coords[0]);
                    int LastY = Convert.ToInt32(Coords[1]);
                    double LastZ = Convert.ToDouble(Coords[2]);
                    int LastRot = Convert.ToInt32(Coords[3]);

                    if (_room.GetGameMap().IsInMap(LastX, LastY))
                    {
                        if (LastX == 0 && LastY == 0)
                        {
                            User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                            User.SetRot(Model.DoorOrientation, false);
                        }
                        else
                        {
                            User.SetPos(LastX, LastY, LastZ);
                            User.SetRot(LastRot, false);
                            UpdateUserStatus(User, false);
                        }
                    }
                    else
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                    //Session.GetHabbo().IsLogin = false;
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                    User.SetRot(Model.DoorOrientation, false);
                }

                #endregion

                #region Roleplay Exiting Houses
                if (Session.GetRoleplay().ExitingHouse)
                {
                    int LastX = Session.GetRoleplay().HouseX;
                    int LastY = Session.GetRoleplay().HouseY;
                    double LastZ = Session.GetRoleplay().HouseZ;

                    if (LastX == 0 && LastY == 0)
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                    else
                    {
                        User.SetPos(LastX, LastY, LastZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }

                    UpdateUserStatus(User, false);

                    Session.GetRoleplay().ExitingHouse = false;
                    Session.GetRoleplay().HouseX = 0;
                    Session.GetRoleplay().HouseY = 0;
                    Session.GetRoleplay().HouseZ = 0;

                }
                #endregion

                #region Checa este Tutorial by J.E.D.G.V
                if (User.GetClient().GetRoleplay().TutorialStep < RoleplayManager.LastTutorialStep && !User.GetClient().GetRoleplay().InTutorial)
                {
                    User.GetClient().GetRoleplay().InTutorial = true;
                    if (User.GetClient().GetRoleplay().TutorialStep == 13 && _room.WardrobeEnabled && _room.Type.Equals("public"))
                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|13");
                    else if (User.GetClient().GetRoleplay().TutorialStep == 18 && _room.PhoneStoreEnabled && _room.Type.Equals("public"))
                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|18");
                    else if (User.GetClient().GetRoleplay().TutorialStep == 23 && _room.BuyCarEnabled && _room.Type.Equals("public"))
                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|24");
                    else if (User.GetClient().GetRoleplay().TutorialStep == 27 && _room.MallEnabled && _room.Type.Equals("public"))
                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_tutorial|28");
                    else
                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(User.GetClient(), "compose_my_tutorial|" + User.GetClient().GetRoleplay().TutorialStep);
                }
                #endregion

            }
            else if (!User.IsBot && (User.GetClient().GetHabbo().IsTeleporting || User.GetClient().GetHabbo().IsHopping))
            {
                Item Item = null;
                if (Session.GetHabbo().IsTeleporting)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().TeleporterId);
                else if (Session.GetHabbo().IsHopping)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().HopperId);

                if (Item != null)
                {
                    if (Session.GetHabbo().IsTeleporting)
                    {
                            Item.ExtraData = "2";
                            Item.UpdateState(false, true);
                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                            User.SetRot(Item.Rotation, false);
                            Item.InteractingUser2 = Session.GetHabbo().Id;
                            Item.ExtraData = "0";
                            Item.UpdateState(false, true);
                    }
                    else if (Session.GetHabbo().IsHopping)
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        User.AllowOverride = false;
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                    }
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ - 1);
                    User.SetRot(Model.DoorOrientation, false);
                }
            }

            #region Invisible command
             if (User.GetClient() != null)
             {
                 if (User.GetClient().GetRoleplay() != null)
                 {
                     if (User.GetClient().GetRoleplay().Invisible && !_room.TutorialEnabled)
                     {
                        User.GetClient().SendMessage(new UserRemoveComposer(User.GetClient().GetRoomUser().VirtualId));
                        RoleplayManager.SendDelayedWhisper(User.GetClient(), "Reminder: tu eres invisible", 1);

                         foreach (RoomUser roomUser in GetUserList().ToList())
                         {
                             if (roomUser == null)
                                 continue;
                             if (roomUser.GetClient() == null)
                                 continue;
                             if (roomUser.GetClient().GetHabbo() == null)
                                 continue;

                             string cansee = "";
                             if (roomUser.GetClient().GetRoleplay().Invisible && roomUser.GetClient().GetHabbo().Username != User.GetClient().GetHabbo().Username)
                             {
                                 User.GetClient().SendMessage(new UsersComposer(roomUser));
                                 roomUser.GetClient().SendMessage(new UsersComposer(User));
                                 cansee += roomUser.GetClient().GetHabbo().Username + ", ";
                                 RoleplayManager.SendDelayedWhisper(User.GetClient(), "El usuario invisible " + User.GetClient().GetHabbo().Username + " Ha entrado en la habitación y puede verte", 1);
                                 continue;
                             }

                             if (roomUser.GetClient().GetHabbo().Username == User.GetClient().GetHabbo().Username)
                             {
                                 RoleplayManager.SendDelayedWhisper(User.GetClient(), "Los siguientes usuarios pueden ver que son invisibles: " + cansee, 1);
                                 continue;
                             }

                            if (!roomUser.GetClient().GetRoleplay().Invisible)
                            {
                                
                                roomUser.GetClient().SendMessage(new UserRemoveComposer(User.VirtualId));
                            }


                         }
                     }
                     else
                     {
                         if (_room.TutorialEnabled)
                             Session.SendMessage(new UsersComposer(Session.GetRoomUser()));
                         else
                             _room.SendMessage(new UsersComposer(User));
                     }
                 }
             }
             #endregion

            if (_room.CheckRights(Session, true))
            {
                User.SetStatus("flatctrl", "useradmin");
                Session.SendMessage(new YouAreOwnerComposer());
                Session.SendMessage(new YouAreControllerComposer(4));
            }
            else if (_room.CheckRights(Session, false) && _room.Group == null)
            {
                User.SetStatus("flatctrl", "1");
                Session.SendMessage(new YouAreControllerComposer(1));
            }
            else if (_room.Group != null && _room.CheckRights(Session, false, true))
            {
                User.SetStatus("flatctrl", "3");
                Session.SendMessage(new YouAreControllerComposer(3));
            }
            else
                Session.SendMessage(new YouAreNotControllerComposer());

            User.UpdateNeeded = true;

            foreach (RoomUser Bot in this._bots.Values.ToList())
            {
                if (Bot == null || Bot.BotAI == null)
                    continue;

                Bot.BotAI.OnUserEnterRoom(User);
            }

            #region Forcing WebSockets connection
            // Refrescamos WS
            Session.GetRoleplay().UpdateInteractingUserDialogues();
            Session.GetRoleplay().RefreshStatDialogue();

            Session.GetRoleplay().InitWSDialogues();
            Session.GetRoleplay().InitStatDialogue();

            // Refrescamos WS del Celular
            if (Session.GetRoleplay().Phone > 0)
            {
                PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "load_apps");
                PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_phone", "show_button");
            }
            #endregion
            //EventManager.TriggerEvent("OnAddedToRoom", Session, _room);

            return true;
        }

        public void RemoveUserFromRoom(GameClient Session, Boolean NotifyClient, Boolean NotifyKick = false, Boolean IdleKicked = false)
        {
            try
            {
                if (_room == null)
                    return;

                if (Session == null || Session.GetHabbo() == null)
                    return;

                if (NotifyKick == true && IdleKicked == false)
                    Session.SendMessage(new GenericErrorComposer(4008));

                if (NotifyClient)
                    Session.SendMessage(new CloseConnectionComposer());

                if (Session.GetHabbo().TentId > 0)
                    Session.GetHabbo().TentId = 0;

                RoomUser User = GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                {
                    if (User.RidingHorse)
                    {
                        User.RidingHorse = false;
                        RoomUser UserRiding = GetRoomUserByVirtualId(User.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.HorseID = 0;
                        }
                    }

                    if (User.Team != TEAM.NONE)
                    {
                        TeamManager Team = this._room.GetTeamManagerForFreeze();
                        if (Team != null)
                        {
                            Team.OnUserLeave(User);

                            User.Team = TEAM.NONE;

                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                        }
                    }

                    this._usersByUserID.TryRemove(User.UserId, out User);
                    this._usersByUsername.TryRemove(Session.GetHabbo().Username.ToLower(), out User);

                    RemoveRoomUser(User);

                    if (User.CurrentItemEffect != ItemEffectType.NONE)
                    {
                        if (Session.GetHabbo().Effects() != null)
                            Session.GetHabbo().Effects().CurrentEffect = -1;
                    }


                    if (this._room.HasActiveTrade(Session.GetHabbo().Id))
                        this._room.TryStopTrade(Session.GetHabbo().Id);

                    //Session.GetHabbo().CurrentRoomId = 0;

                    if (Session.GetHabbo().GetMessenger() != null)
                        Session.GetHabbo().GetMessenger().OnStatusChanged(true);

                    if (User != null)
                        User.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {
                GameClient session = user.GetClient();
                if (session == null)
                    return;

                List<RoomUser> Bots = new List<RoomUser>();

                try
                {
                    foreach (RoomUser roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                            continue;

                        if (roomUser.IsBot && !roomUser.IsPet)
                        {
                            if (!Bots.Contains(roomUser))
                                Bots.Add(roomUser);
                        }
                    }
                }
                catch { }

                List<RoomUser> PetsToRemove = new List<RoomUser>();
                foreach (RoomUser Bot in Bots.ToList())
                {
                    if (Bot == null || Bot.BotAI == null)
                        continue;

                    Bot.BotAI.OnUserLeaveRoom(session);

                    if (Bot.GetBotRoleplay() != null)
                        if (Bot.GetBotRoleplayAI() != null)
                            Bot.GetBotRoleplayAI().OnUserLeaveRoom(session);

                    if (Bot.IsPet && Bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                    {
                        if (!PetsToRemove.Contains(Bot))
                            PetsToRemove.Add(Bot);
                    }
                }

                foreach (RoomUser toRemove in PetsToRemove.ToList())
                {
                    if (toRemove == null)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                        continue;

                    user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }
        }

        public void ClearUsers(GameClient Session)
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                Session.SendMessage(new UserRemoveComposer(user.VirtualId));
            }
        }
        public void RemoveRoomUser(RoomUser user)
        {
            if (user.SetStep)
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            else
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;

            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            _room.SendMessage(new UserRemoveComposer(user.VirtualId));

            RoomUser toRemove = null;
            if (this._users.TryRemove(user.InternalRoomID, out toRemove))
            {
                //uhmm, could put the below stuff in but idk.
            }

            user.InternalRoomID = -1;
            onRemove(user);
        }

        public bool TryGetPet(int PetId, out RoomUser Pet)
        {
            return this._pets.TryGetValue(PetId, out Pet);
        }

        public bool TryGetBot(int BotId, out RoomUser Bot)
        {
            return this._bots.TryGetValue(BotId, out Bot);
        }

        public RoomUser GetBotByName(string Name)
        {
            return RoleplayBotManager.GetDeployedBotByName(Name);
        }

        public void UpdateUserCount(int count)
        {
            userCount = count;
            _room.RoomData.UsersNow = count;

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            RoomUser User = null;

            if (_users != null)
                if (!_users.TryGetValue(VirtualId, out User))
                    return null;
            return User;
        }

        public RoomUser GetRoomUserByHabboId(int pId)
        {
            if (this._usersByUserID.ContainsKey(pId))
                return (RoomUser)this._usersByUserID[pId];
            else
                return (RoomUser)null;
        }


        public RoomUser GetRoomUserByHabbo(int Id)
        {
            if (this == null)
                return null;

            if (this.GetUserList() == null)
                return null;

            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == Id).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public List<RoomUser> GetRoomUsers()
        {
            if (this.GetUserList() == null)
                return null;

            List<RoomUser> Users = this.GetUserList().Where(x => x != null && !x.IsBot).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoleplayBots()
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.IsBot && x.IsRoleplayBot && x.GetBotRoleplay() != null).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Rank >= minRank).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoomUserBySpecialRights()
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().VIPRank > 0).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public RoomUser GetRoomUserByHabbo(string pName)
        {
            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetRoleplay() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (User != null)
            {
                if (User.GetClient().GetRoleplay().Invisible)
                    return null;
                else
                    return User;
            }
            else
                return null;
        }

        public RoomUser GetRoleplayBotByName(string pName)
        {
            RoomUser User = this.GetUserList().Where(x => x != null && x.IsBot && x.IsRoleplayBot && x.GetBotRoleplay() != null && x.GetBotRoleplay().Name.ToLower() == pName.ToLower()).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public void UpdatePets()
        {
            foreach (Pet Pet in GetPets().ToList())
            {
                if (Pet == null)
                    continue;

                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (Pet.DbState == PetDatabaseUpdateState.NeedsInsert)
                    {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" + Pet.PetId + "','" + Pet.OwnerId + "','" + Pet.RoomId + "',@name,'0','0','0')");
                        dbClient.AddParameter("name", Pet.Name);
                        dbClient.RunQuery();

                        dbClient.SetQuery("INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" + Pet.Type + "',@race,@color,'0','100','" + Pet.CreationStamp + "','0','0')");
                        dbClient.AddParameter(Pet.PetId + "race", Pet.Race);
                        dbClient.AddParameter(Pet.PetId + "color", Pet.Color);
                        dbClient.RunQuery();
                    }
                    else if (Pet.DbState == PetDatabaseUpdateState.NeedsUpdate)
                    {
                        //Surely this can be *99 better?
                        RoomUser User = GetRoomUserByVirtualId(Pet.VirtualId);

                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + Pet.RoomId + ", x = " + (User != null ? User.X : 0) + ", Y = " + (User != null ? User.Y : 0) + ", Z = " + (User != null ? User.Z : 0) + " WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + Pet.Experience + "', `energy` = '" + Pet.Energy + "', `nutrition` = '" + Pet.Nutrition + "', `respect` = '" + Pet.Respect + "' WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                    }

                    Pet.DbState = PetDatabaseUpdateState.Updated;
                }
            }
        }

        public List<Pet> GetPets()
        {
            List<Pet> Pets = new List<Pet>();
            foreach (RoomUser User in this._pets.Values.ToList())
            {
                if (User == null || !User.IsPet)
                    continue;

                Pets.Add(User.PetData);
            }

            return Pets;
        }

        public void SerializeStatusUpdates()
        {
            List<RoomUser> Users = new List<RoomUser>();
            ICollection<RoomUser> RoomUsers = GetUserList();

            if (RoomUsers == null)
                return;

            foreach (RoomUser User in RoomUsers.ToList())
            {
                if (User == null || !User.UpdateNeeded || Users.Contains(User))
                    continue;

                User.UpdateNeeded = false;
                Users.Add(User);
            }

            if (Users.Count > 0)
                _room.SendMessage(new UserUpdateComposer(Users));
        }

        public List<RoomUser> GetBots()
        {
            List<RoomUser> Bots = new List<RoomUser>();

            foreach (RoomUser User in this._bots.Values.ToList())
            {
                if (User == null || !User.IsBot)
                    continue;

                Bots.Add(User);
            }

            return Bots;
        }
        public void UpdateUserStatusses()
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                UpdateUserStatus(user, false);
            }
        }

        private bool isValid(RoomUser user)
        {
            if (user == null)
                return false;
            if (user.IsBot)
                return true;
            if (user.GetClient() == null)
                return false;
            if (user.GetClient().GetHabbo() == null)
                return false;
            if (user.GetClient().GetHabbo().CurrentRoomId != _room.RoomId)
                return false;
            return true;
        }
        public void OnCycle()
        {
            int userCounter = 0;
            int idleCount = 0;

            try
            {
                if (_room != null && _room.DiscoMode && _room.TonerData != null && _room.TonerData.Enabled == 1)
                {
                    Item Item = _room.GetRoomItemHandler().GetItem(_room.TonerData.ItemId);

                    if (Item != null)
                    {
                        _room.TonerData.Hue = PolarEnvironment.GetRandomNumber(0, 255);
                        _room.TonerData.Saturation = PolarEnvironment.GetRandomNumber(0, 255);
                        _room.TonerData.Lightness = PolarEnvironment.GetRandomNumber(0, 255);

                        _room.SendMessage(new ObjectUpdateComposer(Item, _room.OwnerId));
                        Item.UpdateState();
                    }
                }

                List<RoomUser> ToRemove = new List<RoomUser>();

                foreach (RoomUser User in this._users.Values)
                {
                    if (User == null)
                        continue;

                    if (!isValid(User))
                    {
                        if (User.GetClient() != null)
                            RemoveUserFromRoom(User.GetClient(), false, false);
                        else
                            RemoveRoomUser(User);
                    }

                    #region Check Turf Capturing
                    if (_room.TurfCapturing)
                    {
                        if (User.GetClient() != null)
                        {
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "event_gang", "turf_cap_w," + _room.Id + "," + _room.TurfUserAtackerId + "," + _room.Name);
                        }
                    }
                    if (_room.BankCapturing)
                    {
                        if (User.GetClient() != null)
                        {
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "event_gang", "bank_cap_w," + _room.Id + "," + _room.TurfUserAtackerId + "," + _room.Name);
                        }
                    }
                    #endregion

                    /*if (User.NeedsAutokick && !ToRemove.Contains(User))
                    {
                        ToRemove.Add(User);
                        continue;
                    }*/

                    bool updated = false;
                    User.IdleTime++;
                    User.HandleSpamTicks();
                    if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                    {
                        User.IsAsleep = true;
                        _room.SendMessage(new SleepComposer(User, true));

                        if (!User.GetClient().GetRoleplay().IsJailed && !User.GetClient().GetRoleplay().IsDead)
                        {
                            User.GetClient().GetRoleplay().BreakGeneralTimer = true;
                            User.GetClient().GetHabbo().Motto = "[DORMIDO] " + User.GetClient().GetRoleplay().Class;
                            User.GetClient().GetHabbo().Poof(false);
                        }
                    }

                    if (User.CarryItemID > 0)
                    {
                        User.CarryTimer--;
                        if (User.CarryTimer <= 0)
                            User.CarryItem(0);
                    }

                    if (_room.GotFreeze())
                        _room.GetFreeze().CycleUser(User);

                    bool InvalidStep = false;

                    if (User.isRolling)
                    {
                        if (User.rollerDelay <= 0)
                        {
                            UpdateUserStatus(User, false);
                            User.isRolling = false;
                        }
                        else
                            User.rollerDelay--;
                    }

                    if (User.SetStep)
                    {
                        // Importante FastWalinkg 1
                        if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(User.SetX, User.SetY), (User.GoalX == User.SetX && User.GoalY == User.SetY), User.AllowOverride))
                        {
                            if (!User.RidingHorse)
                                _room.GetGameMap().UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y), new Point(User.SetX, User.SetY), User);

                            List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in items.ToList())
                            {
                                Item.UserWalksOffFurni(User);
                            }

                            if (!User.IsBot)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }
                            else if (User.IsBot && !User.RidingHorse)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }

                            if (!User.IsBot && User.RidingHorse)
                            {
                                RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.X = User.SetX;
                                    Horse.Y = User.SetY;
                                }
                            }

                            #region Door Kick from Rooms (OFF)
                            
                            if (User.X == _room.GetGameMap().Model.DoorX && User.Y == _room.GetGameMap().Model.DoorY && !ToRemove.Contains(User) && !User.IsBot)
                            {
                                ToRemove.Add(User);
                                continue;
                            }
                            
                            #endregion

                            List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in Items.ToList())
                            {
                                Item.UserWalksOnFurni(User);
                            }

                            // Guardar las últimas coordenadas del usuario
                            var client = User.GetClient();
                            if (client?.GetRoleplay() != null)
                            {
                                client.GetRoleplay().LastCoordinates = $"{User.X},{User.Y},{User.Z},{User.RotBody}";
                            }

                            UpdateUserStatus(User, true);
                        }
                        else
                            InvalidStep = true;
                        User.SetStep = false;
                    }

                    if (User.PathRecalcNeeded)
                    {
                        User.Path = PathFinder.FindPath(User, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));
                        if (User.Path.Count > 1)
                        {
                            User.PathStep = 1;
                            User.IsWalking = true;
                        }
                        else
                        {
                            User.IsWalking = false;
                            User.Path.Clear();
                        }

                        User.PathRecalcNeeded = false;
                    }



                    if (User.IsWalking && !User.Freezed)
                    {
                        if (InvalidStep || User.PathStep >= User.Path.Count || (User.GoalX == User.X && User.GoalY == User.Y))
                        {
                            User.IsWalking = false;
                            User.RemoveStatus("mv");

                            if (User.Statusses.ContainsKey("sign"))
                                User.RemoveStatus("sign");

                            if (User.IsBot && User.BotData.TargetUser > 0)
                            {
                                if (User.CarryItemID > 0)
                                {
                                    RoomUser Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);
                                    if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                    {
                                        User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                        Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                        Target.CarryItem(User.CarryItemID);
                                    }
                                }

                                User.CarryItem(0);
                                User.BotData.TargetUser = 0;
                            }

                            // Dealing with riding horse (if applicable)
                            if (User.RidingHorse && !User.IsPet && !User.IsBot)
                            {
                                RoomUser linkedHorse = GetRoomUserByVirtualId(User.HorseID);
                                if (linkedHorse != null)
                                {
                                    linkedHorse.IsWalking = false;
                                    linkedHorse.RemoveStatus("mv");
                                    linkedHorse.UpdateNeeded = true;
                                }
                            }
                        }
                        else
                        {
                            // Path calculation for user
                            Vector2D NextStep = User.Path[(User.Path.Count - User.PathStep) - 1];
                            User.PathStep++;

                            // Handling special fast walking logic
                            if (User.IsBot)
                            {
                                if (User.FastWalking)
                                {
                                    string PassengerName = User.BotData.Name.Split('#')[1];
                                    RoomUser Passenger = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(PassengerName).GetRoomUser();
                                    if (Passenger != null && Passenger.FastWalking && Passenger.PathStep < Passenger.Path.Count && Passenger.Path.Count > 3)
                                    {
                                        int s2 = (Passenger.Path.Count - Passenger.PathStep) - 1;
                                        NextStep = Passenger.Path[s2];
                                        User.PathStep++;
                                    }
                                }
                            }
                            else
                            {
                                bool CarFastWalk = false;
                                bool CocaineFastWalk = false;
                                bool HeroinaFastWalk = false;

                                if (User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                                {
                                    if (User.GetClient().GetRoleplay().DrivingCar)
                                        CarFastWalk = true;

                                    if (User.GetClient().GetRoleplay().HighOffCocaine)
                                        CocaineFastWalk = true;

                                    if (User.GetClient().GetRoleplay().HighOffHeroina)
                                        HeroinaFastWalk = true;
                                }

                                if (CarFastWalk && User.PathStep < User.Path.Count && User.Path.Count > 4)
                                {
                                    int s2 = (User.Path.Count - User.PathStep) - 2;
                                    if (s2 >= 0 && s2 < User.Path.Count)
                                    {
                                        NextStep = User.Path[s2];
                                    }
                                    else
                                    {
                                        // Manejo de error o lógica alternativa si el índice está fuera de rango
                                        //Console.WriteLine("Índice fuera de rango en User.Path");
                                    }
                                    User.PathStep++;
                                    if (User.GetClient().GetRoleplay().FastCarNew == 1)
                                    {
                                        User.PathStep++;
                                    }
                                    else if (User.GetClient().GetRoleplay().FastCarNew == 2)
                                    {
                                        User.PathStep++;
                                        User.PathStep++;
                                    }
                                    else if (User.GetClient().GetRoleplay().FastCarNew == 3)
                                    {
                                        User.PathStep++;
                                        User.PathStep++;
                                        User.PathStep++;
                                    }
                                }

                                if ((User.SuperFastWalking || CocaineFastWalk) && User.PathStep < User.Path.Count && User.Path.Count > 4)
                                {
                                    int s2 = (User.Path.Count - User.PathStep) - 2;
                                    if (s2 >= 0 && s2 < User.Path.Count)
                                    {
                                        NextStep = User.Path[s2];
                                    }
                                    else
                                    {
                                        // Manejo de error o lógica alternativa si el índice está fuera de rango
                                        //Console.WriteLine("Índice fuera de rango en User.Path");
                                    }
                                    User.PathStep++;
                                    User.PathStep++;

                                }

                                if ((User.SuperFastWalking || HeroinaFastWalk) && User.PathStep < User.Path.Count && User.Path.Count > 4)
                                {
                                    int s2 = (User.Path.Count - User.PathStep) - 2;
                                    if (s2 >= 0 && s2 < User.Path.Count)
                                    {
                                        NextStep = User.Path[s2];
                                    }
                                    else
                                    {
                                        // Manejo de error o lógica alternativa si el índice está fuera de rango
                                        //Console.WriteLine("Índice fuera de rango en User.Path");
                                    }
                                    User.PathStep++;
                                    User.PathStep++;
                                    User.PathStep++;
                                }
                            }

                            // Validate next step on map
                            int nextX = NextStep.X;
                            int nextY = NextStep.Y;
                            User.RemoveStatus("mv");

                            /*if (User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                            {
                                // Adjust walk direction if necessary
                                if (User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Right || User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Left)
                                    nextX = User.X;
                                if (User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Up || User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Down)
                                    nextY = User.Y;
                            }
                            */
                            if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), (User.GoalX == nextX && User.GoalY == nextY), User.AllowOverride))
                            {
                                double nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                // Handle sitting, lying, and other state changes
                                if (!User.IsBot && (User.isSitting || User.isLying))
                                {
                                    User.Z += 0.35;
                                    User.isSitting = false;
                                    User.UpdateNeeded = true;
                                }

                                User.Statusses.Remove("lay");
                                User.Statusses.Remove("sit");

                                if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                {
                                    if (User.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        User.GetClient().GetHabbo().IsTeleporting = false;
                                        User.GetClient().GetHabbo().TeleporterId = 0;
                                    }
                                    else if (User.GetClient().GetHabbo().IsHopping)
                                    {
                                        User.GetClient().GetHabbo().IsHopping = false;
                                        User.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }

                                if (!User.IsBot && User.RidingHorse && !User.IsPet)
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                        Horse.UpdateNeeded = true;
                                    }
                                    User.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));
                                    User.UpdateNeeded = true;
                                }
                                else
                                {
                                    User.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                    User.UpdateNeeded = true;
                                }

                                // Update rotation and position
                                User.LastRotBody = User.RotBody;  // for debugging
                                int newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);
                                User.RotBody = newRot;
                                User.RotHead = newRot;

                                User.SetStep = true;
                                User.SetX = nextX;
                                User.SetY = nextY;
                                User.SetZ = nextZ;

                                UpdateUserEffect(User, User.SetX, User.SetY);
                            }
                        }

                        // Ensure user is not left in an inconsistent state
                        if (!User.RidingHorse)
                            User.UpdateNeeded = true;
                    }
                    else
                    {
                        if (User.Statusses.ContainsKey("mv"))
                        {
                            User.RemoveStatus("mv");
                            User.UpdateNeeded = true;

                            if (User.RidingHorse)
                            {
                                RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.RemoveStatus("mv");
                                    Horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }

                    if (User.RidingHorse)
                        User.ApplyEffect(77);

                    if (User.IsBot && User.BotAI != null)
                        User.BotAI.OnTimerTick();
                    else
                        userCounter++;
  

                    if (!updated)
                        UpdateUserEffect(User, User.X, User.Y);
                }
                foreach (var userToRemove in ToRemove.ToList())
                {
                    var client = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(userToRemove.HabboId);
                    if (client != null)
                        RemoveUserFromRoom(client, true);
                    else
                        RemoveRoomUser(userToRemove);
                }

                if (userCount != userCounter)
                    UpdateUserCount(userCounter);
            }
            catch (Exception e)
            {
                int rId = 0;
                if (_room != null)
                    rId = _room.Id;

                Logging.LogCriticalException("Affected Room - ID: " + rId + " - " + e.ToString());
            }
        }

        /* public void OnCycle()
         {
             int userCounter = 0;

             try
             {
                 if (_room != null && _room.DiscoMode && _room.TonerData != null && _room.TonerData.Enabled == 1)
                 {
                     Item Item = _room.GetRoomItemHandler().GetItem(_room.TonerData.ItemId);

                     if (Item != null)
                     {
                         _room.TonerData.Hue = PolarEnvironment.GetRandomNumber(0, 255);
                         _room.TonerData.Saturation = PolarEnvironment.GetRandomNumber(0, 255);
                         _room.TonerData.Lightness = PolarEnvironment.GetRandomNumber(0, 255);

                         _room.SendMessage(new ObjectUpdateComposer(Item, _room.OwnerId));
                         Item.UpdateState();
                     }
                 }

                 List<RoomUser> ToRemove = new List<RoomUser>();

                 foreach (RoomUser User in GetUserList().ToList())
                 {
                     if (User == null)
                         continue;

                     if (!isValid(User))
                     {
                         if (User.GetClient() != null)
                             RemoveUserFromRoom(User.GetClient(), false, false);
                         else
                             RemoveRoomUser(User);
                     }

                     #region GodModEnteringRoom *Zedd* V2
                     if (User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().LoginGodProtect && User.GetClient().GetRoleplay().FirstTickBool == true && !User.GetClient().GetRoleplay().DrivingCar && !User.GetClient().GetRoleplay().Pasajero && !User.GetClient().GetRoleplay().IsNoob && !User.GetClient().GetRoleplay().IsGodMode)
                     {
                         if ((_room.SafeZoneEnabled && User.GetClient().GetRoleplay().GodModeTicks > 0))
                         {
                             User.GetClient().GetHabbo().LoginGodProtect = false;
                             User.GetClient().GetRoleplay().GodMode = false;
                             User.GetClient().GetRoleplay().GodModeTicks = 0;
                             User.GetClient().GetRoleplay().FirstTickBool = false;
                             if (User.GetClient().GetRoomUser() != null && User.CurrentEffect == EffectsList.Fireflies)
                                 User.GetClient().GetRoomUser().ApplyEffect(EffectsList.None);
                         }

                         if (!_room.SafeZoneEnabled || User.GetClient().GetRoleplay().GodModeTicks > 0)
                         {
                             if (User.GetClient().GetRoleplay().GodModeTicks <= 10)
                             {
                                 //User.GetClient().GetRoleplay().IsNoob = true; Variable de GodMode
                                 User.GetClient().GetRoleplay().GodMode = true;
                                 User.GetClient().GetRoleplay().GodModeTicks++;
                                 if (User.GetClient().GetRoleplay().GodModeTicks % 2 == 0)
                                     User.GetClient().GetHabbo().GetClient().SendWhisper("((Te quedan " + (10 - User.GetClient().GetRoleplay().GodModeTicks) + " segundos de inmunidad en esta Zona))", 0);
                             }
                             else
                             {
                                 //User.GetClient().GetRoleplay().IsNoob = false;
                                 User.GetClient().GetHabbo().LoginGodProtect = false;
                                 User.GetClient().GetRoleplay().GodMode = false;
                                 User.GetClient().GetRoleplay().GodModeTicks = 0;
                                 User.GetClient().GetHabbo().GetClient().SendWhisper("((Tu inmunidad en esta Zona ha acabado))", 0);
                                 User.GetClient().GetRoleplay().FirstTickBool = false;
                                 if (User.GetClient().GetRoomUser() != null)
                                     User.GetClient().GetRoomUser().ApplyEffect(EffectsList.None);
                             }
                         }
                     }
                     #endregion GodModEnteringRoom *Zedd* V2


                     #region Check Turf Capturing
                     if (_room.TurfCapturing)
                     {
                         if (User.GetClient() != null)
                         {
                             PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "event_gang", "turf_cap_w," + _room.Id + "," + _room.TurfUserAtackerId + "," + _room.Name);
                         }
                     }
                     #endregion

                     if (User.NeedsAutokick && !ToRemove.Contains(User))
                     {
                         ToRemove.Add(User);
                         continue;
                     }

                     bool updated = false;
                     User.IdleTime++;
                     User.HandleSpamTicks();
                     if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                     {
                         User.IsAsleep = true;
                         _room.SendMessage(new SleepComposer(User, true));
                     }

                     if (User.CarryItemID > 0)
                     {
                         User.CarryTimer--;
                         if (User.CarryTimer <= 0)
                             User.CarryItem(0);
                     }

                     if (_room.GotFreeze())
                         _room.GetFreeze().CycleUser(User);

                     bool InvalidStep = false;

                     if (User.isRolling)
                     {
                         if (User.rollerDelay <= 0)
                         {
                             UpdateUserStatus(User, false);
                             User.isRolling = false;
                         }
                         else
                             User.rollerDelay--;
                     }

                     if (User.SetStep)
                     {
                         if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(User.SetX, User.SetY), (User.GoalX == User.SetX && User.GoalY == User.SetY), User.AllowOverride))
                         {
                             if (!User.RidingHorse)
                                 _room.GetGameMap().UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y), new Point(User.SetX, User.SetY), User);

                             List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                             foreach (Item Item in items.ToList())
                             {
                                 Item.UserWalksOffFurni(User);
                             }
                             if (!User.IsBot)
                             {
                                 User.X = User.SetX;
                                 User.Y = User.SetY;
                                 User.Z = User.SetZ;
                             }
                             else if (User.IsBot && !User.RidingHorse)
                             {
                                 User.X = User.SetX;
                                 User.Y = User.SetY;
                                 User.Z = User.SetZ;
                             }

                             if (!User.IsBot && User.RidingHorse)
                             {
                                 RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                 if (Horse != null)
                                 {
                                     Horse.X = User.SetX;
                                     Horse.Y = User.SetY;
                                 }
                             }

                             List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                             foreach (Item Item in Items.ToList())
                             {
                                 Item.UserWalksOnFurni(User);
                             }

                             UpdateUserStatus(User, true);
                         }
                         else
                             InvalidStep = true;
                         User.SetStep = false;
                     }

                     if (User.PathRecalcNeeded)
                     {
                         if (User.Path.Count > 1)
                             User.Path.Clear();

                         User.Path = PathFinder.FindPath(User, this._room.GetGameMap().DiagonalEnabled, this._room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));

                         if (User.Path.Count > 1)
                         {
                             User.PathStep = 1;
                             User.IsWalking = true;
                             User.PathRecalcNeeded = false;
                         }
                         else
                         {
                             User.PathRecalcNeeded = false;
                             if (User.Path.Count > 1)
                                 User.Path.Clear();
                         }
                     }

                     if (User.IsWalking && !User.Freezed)
                     {
                         if (InvalidStep || (User.PathStep >= User.Path.Count) || (User.GoalX == User.X && User.GoalY == User.Y)) //No path found, or reached goal (:
                         {
                             User.IsWalking = false;
                             User.RemoveStatus("mv");

                             if (User.Statusses.ContainsKey("sign"))
                                 User.RemoveStatus("sign");

                             if (User.IsBot && User.BotData.TargetUser > 0)
                             {
                                 if (User.CarryItemID > 0)
                                 {
                                     RoomUser Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);

                                     if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                     {
                                         User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                         Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                         Target.CarryItem(User.CarryItemID);
                                     }
                                 }

                                 User.CarryItem(0);
                                 User.BotData.TargetUser = 0;
                             }

                             if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                             {
                                 RoomUser mascotaVinculada = GetRoomUserByVirtualId(User.HorseID);
                                 if (mascotaVinculada != null)
                                 {
                                     mascotaVinculada.IsWalking = false;
                                     mascotaVinculada.RemoveStatus("mv");
                                     mascotaVinculada.UpdateNeeded = true;
                                 }
                             }
                         }
                         else
                         {

                             Vector2D NextStep = User.Path[(User.Path.Count - User.PathStep) - 1];
                             User.PathStep++;

                             bool CarFastWalk = false;
                             bool CocaineFastWalk = false;

                             if (User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                             {
                                 if (User.GetClient().GetRoleplay().DrivingCar)
                                     CarFastWalk = true;

                                 if (User.GetClient().GetRoleplay().HighOffCocaine)
                                     CocaineFastWalk = true;
                             }

                             if ((User.FastWalking || CarFastWalk) && User.PathStep < User.Path.Count)
                             {
                                 int s2 = (User.Path.Count - User.PathStep) - 1;
                                 NextStep = User.Path[s2];
                                 User.PathStep++;
                             }

                             if ((User.SuperFastWalking || CocaineFastWalk) && User.PathStep < User.Path.Count)
                             {
                                 int s2 = (User.Path.Count - User.PathStep) - 1;
                                 NextStep = User.Path[s2];
                                 User.PathStep++;
                                 User.PathStep++;
                             }


                             int nextX = NextStep.X;
                             int nextY = NextStep.Y;

                             int nextFrontX = SquareInFront(nextX, nextY, User.RotBody, "x");
                             int nextFrontY = SquareInFront(nextX, nextY, User.RotBody, "y");
                             User.RemoveStatus("mv");

                             if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), (User.GoalX == nextX && User.GoalY == nextY), User.AllowOverride))
                             {
                                 double nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                 if (!User.IsBot)
                                 {
                                     if (User.isSitting || User.isLying)
                                     {
                                         User.Z += 0.35;
                                         User.isSitting = false;
                                         User.UpdateNeeded = true;
                                     }

                                     User.Statusses.Remove("lay");
                                     User.Statusses.Remove("sit");
                                 }

                                 if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                 {
                                     if (User.GetClient().GetHabbo().IsTeleporting)
                                     {
                                         User.GetClient().GetHabbo().IsTeleporting = false;
                                         User.GetClient().GetHabbo().TeleporterId = 0;
                                     }
                                     else if (User.GetClient().GetHabbo().IsHopping)
                                     {
                                         User.GetClient().GetHabbo().IsHopping = false;
                                         User.GetClient().GetHabbo().HopperId = 0;
                                     }
                                 }


                                 if (!User.IsBot && User.RidingHorse && User.IsPet == false)
                                 {
                                     RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                     if (Horse != null)
                                         Horse.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                     User.AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));

                                     User.UpdateNeeded = true;
                                     Horse.UpdateNeeded = true;
                                 }
                                 else if (!User.IsBot && User.GetClient().GetRoleplay().Chofer)
                                 {
                                     //Vars
                                     string Pasajeros = User.GetClient().GetRoleplay().Pasajeros;
                                     string[] stringSeparators = new string[] { ";" };
                                     string[] result;
                                     result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                     foreach (string psjs in result)
                                     {
                                         GameClient PJ = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                         if (PJ != null && PJ.GetRoomUser() != null)
                                         {
                                             PJ.GetRoomUser().AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                             PJ.GetRoomUser().UpdateNeeded = true;
                                         }
                                     }

                                     User.AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                     User.UpdateNeeded = true;

                                 }
                                 else
                                     User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                 var newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);
                                 User.RotBody = newRot;
                                 User.RotHead = newRot;
                                 User.SetStep = true;
                                 User.SetX = nextX;
                                 User.SetY = nextY;
                                 User.SetZ = nextZ;
                                 UpdateUserEffect(User, User.SetX, User.SetY);
                                 updated = true;
                                 if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                                 {
                                     var horse = GetRoomUserByVirtualId(User.HorseID);
                                     if (horse != null)
                                     {
                                         horse.RotBody = newRot;
                                         horse.RotHead = newRot;
                                         horse.SetStep = true;
                                         horse.SetX = nextX;
                                         horse.SetY = nextY;
                                         horse.SetZ = nextZ;
                                     }
                                 }

                                 if (_room.Id == Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                                 {
                                     if (!JailbreakManager.FenceBroken)
                                     {
                                         int X = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencex"));
                                         int Y = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencey"));

                                         if (User.X == X && User.Y == Y)
                                         {
                                             _room.GetGameMap().GameMap[X, Y] = 0;
                                             User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                         }
                                         else if (User.X == (X + 1) || User.Y == Y)
                                         {
                                             _room.GetGameMap().GameMap[(X + 1), Y] = 0;
                                             User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                         }
                                         else
                                         {
                                             _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState;
                                             User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                         }
                                     }
                                 }
                                 else
                                 {
                                     _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState; // REstore the old one
                                     User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY]; //Backup the new one

                                 }

                                 if (_room.RoomBlockingEnabled == 0)
                                 {
                                     List<RoomUser> Users = _room.GetRoomUserManager().GetRoomUsers().Where(x => x.X == nextX && x.Y == nextY).ToList();
                                     if (Users != null && Users.Count > 0)
                                         _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                     else
                                         _room.GetGameMap().GameMap[nextX, nextY] = 1;
                                 }
                                 else
                                     _room.GetGameMap().GameMap[nextX, nextY] = 1;
                             }
                         }
                         if (!User.RidingHorse)
                             User.UpdateNeeded = true;
                     }
                     else
                     {
                         if (User.Statusses.ContainsKey("mv"))
                         {
                             User.RemoveStatus("mv");
                             User.UpdateNeeded = true;

                             if (User.RidingHorse)
                             {
                                 RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                 if (Horse != null)
                                 {
                                     Horse.RemoveStatus("mv");
                                     Horse.UpdateNeeded = true;
                                 }
                             }
                         }
                     }

                     if (User.RidingHorse)
                         User.ApplyEffect(77);

                     if (!User.IsBot && User.GetClient() != null && User.GetClient().GetRoleplay() != null && User.GetClient().GetRoleplay().Pasajero)
                         User.ApplyEffect(EffectsList.Invisible);

                     if (User.IsBot && User.BotAI != null)
                         User.BotAI.OnTimerTick();
                     else
                         userCounter++;

                     if (!updated)
                         UpdateUserEffect(User, User.X, User.Y);
                 }

                 foreach (RoomUser toRemove in ToRemove.ToList())
                 {
                     GameClient client = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(toRemove.HabboId);
                     if (client != null)
                     {
                         RemoveUserFromRoom(client, true, false);
                     }
                     else
                         RemoveRoomUser(toRemove);
                 }


                 if (userCount != userCounter)
                     UpdateUserCount(userCounter);
             }
             catch (Exception e)
             {
                 int rId = 0;
                 if (_room != null)
                     rId = _room.Id;

                 Logging.LogCriticalException("Affected Room - ID: " + rId + " - " + e.ToString());
             }
         }

        */
        public void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            if (User == null)
                return;

            try
            {
                bool isBot = User.IsBot;
                if (isBot)
                    cyclegameitems = false;

                if (PolarEnvironment.GetUnixTimestamp() > PolarEnvironment.GetUnixTimestamp() + User.SignTime)
                {
                    if (User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                }

                if ((User.Statusses.ContainsKey("lay") && !User.isLying) || (User.Statusses.ContainsKey("sit") && !User.isSitting))
                {
                    if (User.Statusses.ContainsKey("lay"))
                        User.Statusses.Remove("lay");
                    if (User.Statusses.ContainsKey("sit"))
                        User.Statusses.Remove("sit");
                    User.UpdateNeeded = true;
                }
                else if (User.isLying || User.isSitting)
                    return;

                double newZ;
                List<Item> ItemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
                if (ItemsOnSquare != null || ItemsOnSquare.Count != 0)
                {
                    if (User.RidingHorse && User.IsPet == false)
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList()) + 1;
                    else
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList());
                }
                else
                    newZ = 1;

                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    User.UpdateNeeded = true;
                }

                DynamicRoomModel Model = _room.GetGameMap().Model;
                if (Model.SqState[User.X, User.Y] == SquareState.SEAT)
                {
                    if (!User.Statusses.ContainsKey("sit"))
                        User.Statusses.Add("sit", "1.0");
                    User.Z = Model.SqFloorHeight[User.X, User.Y];
                    User.RotHead = Model.SqSeatRot[User.X, User.Y];
                    User.RotBody = Model.SqSeatRot[User.X, User.Y];

                    User.UpdateNeeded = true;
                }

                if (ItemsOnSquare.Count == 0)
                    User.LastItem = null;

                foreach (Item Item in ItemsOnSquare.ToList())
                {
                    if (Item == null)
                        continue;

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                                User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                        }

                        User.Z = Item.GetZ;
                        User.RotHead = Item.Rotation;
                        User.RotBody = Item.Rotation;
                        User.UpdateNeeded = true;
                    }

                    switch (Item.GetBaseItem().InteractionType)
                    {
                        #region Roleplay

                        #region Shower
                        case InteractionType.SHOWER:
                            {
                                if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                                        continue;

                                    Room Room;

                                    if (!PolarEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        return;

                                    if (User.GetClient().GetRoleplay().Hygiene >= 100)
                                    {
                                        User.GetClient().SendWhisper("Su Higiene ya está en un máximo de 100", 1);
                                        User.GetClient().GetRoleplay().IsWorking = false;
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.InteractingUser != 0)
                                    {
                                        User.GetClient().SendWhisper("Esta ducha ya está en uso por alguien más! Lo siento, no puedes unirte a ellos!", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.ExtraData == "0" || Item.ExtraData == "")
                                    {
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(1, true);
                                    }
                                    if (Item.ExtraData == "1")
                                    {
                                        if (!User.GetClient().GetRoleplay().InShower)
                                        {
                                            User.ClearMovement(true);
                                            Item.InteractingUser = User.GetClient().GetHabbo().Id;
                                            User.GetClient().GetRoleplay().InShower = true;
                                            RoleplayManager.Shout(User.GetClient(), "*Comienza a tomar una buena ducha caliente*", 4);
                                            User.GetClient().GetRoleplay().IsWorking = false;
                                            User.GetClient().GetRoleplay().TimerManager.CreateTimer("shower", 1000, false, Item.Id);
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Cagar
                        case InteractionType.CAGAR:
                            {
                                if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                                        continue;

                                    Room Room;

                                    if (!PolarEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        return;

                                    if (User.GetClient().GetRoleplay().Poop >= 100)
                                    {
                                        User.GetClient().SendWhisper("Su vejiga ya está en un máximo de 100", 1);
                                        User.GetClient().GetRoleplay().IsWorking = false;
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.InteractingUser != 0)
                                    {
                                        User.GetClient().SendWhisper("Este toilet ya está en uso por alguien más! Lo siento, no puedes unirte a ello!", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.ExtraData == "0" || Item.ExtraData == "")
                                    {
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(1, true);
                                    }
                                    if (Item.ExtraData == "1")
                                    {
                                        if (!User.GetClient().GetRoleplay().InCagar)
                                        {
                                            User.ClearMovement(true);
                                            Item.InteractingUser = User.GetClient().GetHabbo().Id;
                                            User.GetClient().GetRoleplay().InCagar = true;
                                            RoleplayManager.Shout(User.GetClient(), "*Comienza a defecar o orinar en el toilet, huele a rayos*", 4);
                                            User.GetClient().GetRoleplay().IsWorking = false;
                                            User.GetClient().GetRoleplay().TimerManager.CreateTimer("cagar", 1000, false, Item.Id);
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Whisper Tile
                        case InteractionType.WHISPER_TILE:
                            {
                                if (!User.IsBot)
                                {
                                    if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                    {
                                        if (Item.WhisperTileData == null)
                                        {
                                            User.GetClient().SendWhisper("¡Vaya, parece que los datos de susurros están rotos!", 1);
                                            break;
                                        }
                                        else
                                        {
                                            if (Item.WhisperTileData.Message == null)
                                                Item.WhisperTileData.Message = "";

                                            if (Item.WhisperTileData.Message == "")
                                            {
                                                //User.GetClient().SendWhisper("Oops, looks like this items whisper data has not been set yet!", 1);
                                                break;
                                            }

                                            if (User.GetClient() != null && Item.WhisperTileData != null && Item.WhisperTileData.Message != null)
                                                User.GetClient().SendWhisper(Item.WhisperTileData.Message, 34);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #endregion

                        #region Beds & Tents
                        case InteractionType.BED:
                        case InteractionType.BEDEFFECT:
                        case InteractionType.TENT_SMALL:
                            {
                                if (!User.Statusses.ContainsKey("lay"))
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");

                                if (Item.GetBaseItem().InteractionType == InteractionType.BEDEFFECT)
                                {
                                    if (User != null && !User.IsBot)
                                    {
                                        if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                            return;

                                        if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                            return;

                                        User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(2, true);
                                    }
                                }

                                User.RotHead = Item.Rotation;
                                User.RotBody = Item.Rotation;
                                User.UpdateNeeded = true;
                                break;
                            }
                        #endregion

                        #region Banzai Gates
                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaigateyellow:
                            {
                            
                                    if (cyclegameitems)
                                    {
                                        int effectID = Convert.ToInt32(Item.team + 32);
                                        TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();

                                        if (User.Team == TEAM.NONE)
                                        {
                                            if (t.CanEnterOnTeam(Item.team))
                                            {
                                                if (User.Team != TEAM.NONE)
                                                    t.OnUserLeave(User);
                                                User.Team = Item.team;

                                                t.AddUser(User);

                                                if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                    User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                            }
                                        }
                                        else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                        {
                                            t.OnUserLeave(User);
                                            User.Team = TEAM.NONE;
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        }
                                        else
                                        {
                                            //usersOnTeam--;
                                            t.OnUserLeave(User);
                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                            User.Team = TEAM.NONE;
                                        }
                                        //Item.ExtraData = usersOnTeam.ToString();
                                        //Item.UpdateState(false, true);                                
                                    }
                                    break;
                                }
                        #endregion

                        #region Freeze Gates
                        case InteractionType.FREEZE_YELLOW_GATE:
                        case InteractionType.FREEZE_RED_GATE:
                        case InteractionType.FREEZE_GREEN_GATE:
                        case InteractionType.FREEZE_BLUE_GATE:
                            {
                                if (User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                                    break;

                                #region Texas Hold 'Em
                                if (TexasHoldEmManager.GameList.Count > 0)
                                {
                                    if (TexasHoldEmManager.GameList.Values.Where(x => x.JoinGate != null && x.JoinGate.Furni != null && x.JoinGate.Furni == Item).ToList().Count > 0)
                                    {
                                        TexasHoldEm Game = TexasHoldEmManager.GameList.Values.Where(x => x.JoinGate != null && x.JoinGate.Furni != null && x.JoinGate.Furni == Item).ToList().FirstOrDefault();

                                        if (Game.GameStarted)
                                            User.GetClient().SendWhisper("Lo siento pero ya hay un juego de Texas Hold 'Em!", 1);
                                        else
                                            Game.AddPlayerToGame(User.GetClient().GetHabbo().Id);
                                        break;
                                    }
                                }
                                #endregion

                                #region Non-Roleplay
                                if (cyclegameitems)
                                {
                                    int effectID = Convert.ToInt32(Item.team + 39);
                                    TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

                                    if (User.Team == TEAM.NONE)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != TEAM.NONE)
                                                t.OnUserLeave(User);
                                            User.Team = Item.team;
                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                    else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = TEAM.NONE;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        User.Team = TEAM.NONE;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                #endregion

                                break;
                            }
                        #endregion

                        #region Banzai Teles
                        case InteractionType.banzaitele:
                            {
                                if (User.Statusses.ContainsKey("mv"))
                                    _room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                                break;
                            }
                        #endregion

                        #region Football Gate

                        #endregion

                        #region Effects
                        case InteractionType.EFFECT:
                            {
                                if (User == null)
                                    return;

                                if (!User.IsBot)
                                {
                                    if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                        return;

                                    if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                        return;

                                    User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                    Item.ExtraData = "1";
                                    Item.UpdateState(false, true);
                                    Item.RequestUpdate(2, true);
                                }
                                break;
                            }
                        #endregion

                        #region Arrows
                        case InteractionType.ARROW:
                            {
                                
                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    if (User == null || (User.GetClient() != null && User.GetClient().GetHabbo() == null && User.GetClient().GetHabbo().IsTeleporting))
                                        continue;

                                    Room Room = _room;

                                    //if (!PolarEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                    //  break;

                                    if (!User.IsBot)
                                    {
                                        if ((User.GetClient().GetRoleplay().IsJailed && !Room.IsPrison && !Room.IsPrison2 && !User.GetClient().GetRoleplay().Jailbroken))
                                        {
                                            User.GetClient().SendWhisper("¡No puedes usar flechas para escapar mientras estás encarcelado!", 1);
                                            break;
                                        }

                                        if (User.GetClient().GetRoleplay().IsDead)
                                        {
                                            User.GetClient().SendWhisper("¡No puedes usar flechas mientras estás muerto!", 1);
                                            break;
                                        }

                                        User.ClearMovement(true);
                                    }

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                        User.UnlockWalking();
                                    else
                                    {
                                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id, Room);
                                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);
                                        /* (User.GetClient() != null)
                                        {

                                            object[] Bits = new object[6];
                                            Bits[0] = Item.GetX;
                                            Bits[1] = Item.GetY;
                                            Bits[2] = Item.RoomId;
                                            Bits[3] = TeleRoomId;
                                            Bits[4] = Item.Id;
                                            Bits[5] = LinkedTele;

                                            EventManager.TriggerEvent("OnTeleport", User.GetClient(), Bits);

                                        }*/
                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            Item TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                    User.GetClient().SendWhisper("¡Eh, esa flecha no está bien!", 1);
                                                break;
                                            }
                                            else
                                            {

                                                if (User.GetClient() != null && User.GetClient().GetRoleplay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetRoleplay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetRoleplay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                Room.GetGameMap().TeleportToItem(PJ.GetRoomUser(), TargetItem);
                                                                PJ.SendMessage(new UserRemoveComposer(PJ.GetRoomUser().VirtualId));
                                                                //PJ.GetRoomUser().MoveTo(TargetItem.SquareBehind.X, TargetItem.SquareBehind.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                Room.GetGameMap().TeleportToItem(User, TargetItem);
                                            }
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
												 if (User.GetClient().GetRoleplay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetRoleplay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetRoleplay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                PJ.GetHabbo().IsTeleporting = true;
                                                                PJ.GetHabbo().TeleportingRoomID = TeleRoomId;
                                                                PJ.GetHabbo().TeleporterId = LinkedTele;
                                                                PJ.SendMessage(new UserRemoveComposer(PJ.GetRoomUser().VirtualId));
                                                                RoleplayManager.SendUserNew(PJ, TeleRoomId);
                                                                //User.MoveTo(User.SquareInFront.X, User.SquareInFront.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
												
                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                                RoleplayManager.SendUserNew(User.GetClient(), TeleRoomId);
                                            }
                                        }
                                        else if (this._room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                            User.UnlockWalking();
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Arrows
                        case InteractionType.ARROW2:
                            {

                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    if (User == null || (User.GetClient() != null && User.GetClient().GetHabbo() == null && User.GetClient().GetHabbo().IsTeleporting))
                                        continue;

                                    Room Room = _room;

                                    //if (!PolarEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                    //  break;

                                    if (!User.IsBot)
                                    {
                                        if ((User.GetClient().GetRoleplay().IsJailed && !Room.IsPrison && !Room.IsPrison2 && !User.GetClient().GetRoleplay().Jailbroken))
                                        {
                                            User.GetClient().SendWhisper("¡No puedes usar flechas para escapar mientras estás encarcelado!", 1);
                                            break;
                                        }

                                        if (User.GetClient().GetRoleplay().IsDead)
                                        {
                                            User.GetClient().SendWhisper("¡No puedes usar flechas mientras estás muerto!", 1);
                                            break;
                                        }

                                        /*if (User.GetClient().GetRoleplay().Robbery == true)
                                        {
                                            Room.BankCapturing = false;
                                            User.GetClient().GetRoleplay().BreakGeneralTimer = true;
                                            User.GetClient().GetRoleplay().Robbery = false;
                                        }

                                        if (User.GetClient().GetRoleplay().InsideTaxi)
                                            User.GetClient().GetRoleplay().InsideTaxi = false;

                                        if (User.GetClient().GetRoleplay().InsideBus)
                                            User.GetClient().GetRoleplay().InsideBus = false;
                                        */
                                        User.ClearMovement(true);
                                    }

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                        User.UnlockWalking();
                                    else
                                    {
                                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id, Room);
                                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);
                                        /* (User.GetClient() != null)
                                        {

                                            object[] Bits = new object[6];
                                            Bits[0] = Item.GetX;
                                            Bits[1] = Item.GetY;
                                            Bits[2] = Item.RoomId;
                                            Bits[3] = TeleRoomId;
                                            Bits[4] = Item.Id;
                                            Bits[5] = LinkedTele;

                                            EventManager.TriggerEvent("OnTeleport", User.GetClient(), Bits);

                                        }*/
                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            Item TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                    User.GetClient().SendWhisper("¡Eh, esa flecha no está bien!", 1);
                                                break;
                                            }
                                            else
                                            {

                                                if (User.GetClient() != null && User.GetClient().GetRoleplay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetRoleplay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetRoleplay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                Room.GetGameMap().TeleportToItem(PJ.GetRoomUser(), TargetItem);
                                                                PJ.SendMessage(new UserRemoveComposer(PJ.GetRoomUser().VirtualId));
                                                                //PJ.GetRoomUser().MoveTo(TargetItem.SquareBehind.X, TargetItem.SquareBehind.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                Room.GetGameMap().TeleportToItem(User, TargetItem);
                                            }
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
                                                if (User.GetClient().GetRoleplay().Chofer)
                                                {
                                                    #region Pasajeros
                                                    //Vars
                                                    string Pasajeros = User.GetClient().GetRoleplay().Pasajeros;
                                                    string[] stringSeparators = new string[] { ";" };
                                                    string[] result;
                                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                                    foreach (string psjs in result)
                                                    {
                                                        GameClient PJ = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                                                        if (PJ != null)
                                                        {
                                                            if (PJ.GetRoleplay().ChoferName == User.GetClient().GetHabbo().Username)
                                                            {
                                                                PJ.GetHabbo().IsTeleporting = true;
                                                                PJ.GetHabbo().TeleportingRoomID = TeleRoomId;
                                                                PJ.GetHabbo().TeleporterId = LinkedTele;
                                                                PJ.SendMessage(new UserRemoveComposer(PJ.GetRoomUser().VirtualId));
                                                                RoleplayManager.SendUserNew(PJ, TeleRoomId);
                                                                //User.MoveTo(User.SquareInFront.X, User.SquareInFront.Y);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }

                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                                RoleplayManager.SendUserNew2(User.GetClient(), TeleRoomId);
                                            }
                                        }
                                        else if (this._room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                            User.UnlockWalking();
                                    }
                                }
                                break;
                            }
                            #endregion

                    }
                }

                if (User.isSitting && User.TeleportEnabled)
                {
                    User.Z -= 0.35;
                    User.UpdateNeeded = true;
                }

                if (cyclegameitems)
                {
                    if (_room.GotSoccer())
                        _room.GetSoccer().OnUserWalk(User);

                    if (_room.GotBanzai())
                        _room.GetBanzai().OnUserWalk(User);

                    if (_room.GotFreeze())
                        _room.GetFreeze().OnUserWalk(User);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                return;

            try
            {
                byte NewCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                        User.CurrentItemEffect = ItemEffectType.NONE;

                    ItemEffectType Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                    User.CurrentItemEffect = ItemEffectType.Iceskates;
                                    break;
                                }

                            case ItemEffectType.Normalskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SWIM:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimLow:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }

                            case ItemEffectType.NONE:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.NONE && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.NONE;
                }
            }
            catch
            {
            }
        }

        public int PetCount
        {
            get { return petCount; }
        }

        public ICollection<RoomUser> GetBotList()
        {
            return this._bots.Values;
        }

        public ICollection<RoomUser> GetUserList()
        {
            return this._users.Values;
        }

        public int SquareInFront(int X, int Y, int RotBody, string find)
        {
            int Sq = 0;
            if (RotBody == 0)
            {
                Y--;
            }
            else if (RotBody == 2)
            {
                X++;
            }
            else if (RotBody == 4)
            {
                Y++;
            }
            else if (RotBody == 6)
            {
                X--;
            }
            if (find == "x") Sq = X;
            else Sq = Y;
            return Sq;
        }
    }
}