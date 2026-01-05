using System;
using System.Data;

using Polar.Core;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Rooms.AI;
using Polar.HabboHotel.Rooms.Games;
using Polar.Communication.Interfaces;
using Polar.Communication.Packets.Outgoing;

using Polar.HabboHotel.Rooms.Instance;

using Polar.HabboHotel.Items.Data.Toner;
using Polar.HabboHotel.Items.Data.RentableSpace;
using Polar.HabboHotel.Rooms.Games.Freeze;
using Polar.HabboHotel.Items.Data.Moodlight;

using Polar.Communication.Packets.Outgoing.Rooms.Avatar;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Communication.Packets.Outgoing.Rooms.Session;
using Polar.Communication.Packets.Outgoing.QuickPolls;

using Polar.HabboHotel.Rooms.Games.Football;
using Polar.HabboHotel.Rooms.Games.Banzai;
using Polar.HabboHotel.Rooms.Games.Teams;
using Polar.HabboHotel.Rooms.AI.Speech;
using Polar.Database.Interfaces;
using Polar.HabboRoleplay.Houses;
using Polar.HabboRoleplay.Bots.Manager;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Rooms.TraxMachine;
using System.Net.Sockets;

namespace Polar.HabboHotel.Rooms
{
    public class Room : RoomData, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        public bool isCrashed;
        public bool mDisposed;
        public bool RoomMuted;
        public DateTime lastTimerReset;
        public DateTime lastRegeneration;
        public delegate void FurnisLoaded();
        //public event FurnisLoaded OnFurnisLoad;

        public Task ProcessTask;
        public List<Trade> ActiveTrades { get; set; }
        private RoomTraxManager _traxManager;
        public TonerData TonerData;
        public MoodlightData MoodlightData;

        public Dictionary<int, double> Bans;
        public Dictionary<int, double> MutedUsers;

        private readonly TimeSpan _saveFurnitureTimer = TimeSpan.FromMinutes(2);
        private DateTime _saveFurnitureTimerLast = DateTime.Now;

        private Dictionary<int, List<RoomUser>> Tents;
        private CancellationTokenSource _mainProcessSource;
        private Task _processTask;
        public List<int> UsersWithRights;
        private GameManager _gameManager;
        private Freeze _freeze;
        private Soccer _soccer;
        private BattleBanzai _banzai;

        private Gamemap _gamemap;
        private GameItemHandler _gameItemHandler;
        private RoomData _roomData;
        public TeamManager teambanzai;
        public TeamManager teamfreeze;

        private RoomUserManager _roomUserManager;
        private RoomItemHandling _roomItemHandling;

        private List<string> _wordFilterList;

        private FilterComponent _filterComponent;
        private WiredComponent _wiredComponent;
        public bool mCycleEnded
        {
            get; set;
        }
        internal string poolQuestion;
        internal List<int> yesPoolAnswers;
        internal List<int> noPoolAnswers;
        public int IsLagging { get; set; }
        public int IdleTime { get; set; }
        private bool _processingWireds;
        private bool _hideWired;
        private bool _gamblingRoom;

        public bool DiscoMode;

        public Room(RoomData Data)
        {

            _mainProcessSource = new CancellationTokenSource();
            IsLagging = 0;
            this.IdleTime = 0;

            this._roomData = Data;
            RoomMuted = false;
            mDisposed = false;
            this.mCycleEnded = false;
            this.Id = Data.Id;
            this.Name = Data.Name;
            this.Description = Data.Description;
            this.OwnerName = Data.OwnerName;
            this.OwnerId = Data.OwnerId;

            this.WiredScoreBordDay = Data.WiredScoreBordDay;
            this.WiredScoreBordWeek = Data.WiredScoreBordWeek;
            this.WiredScoreBordMonth = Data.WiredScoreBordMonth;
            this.WiredScoreFirstBordInformation = Data.WiredScoreFirstBordInformation;

            this.Category = Data.Category;
            this.Type = Data.Type;
            this.State = Data.State;
            this.UsersNow = 0;
            this.City = Data.City;
            this.UsersMax = Data.UsersMax;
            this.ModelName = Data.ModelName;
            this.Score = Data.Score;
            this.Tags = new List<string>();
            foreach (string tag in Data.Tags)
            {
                Tags.Add(tag);
            }

            this.AllowPets = Data.AllowPets;
            this.AllowPetsEating = Data.AllowPetsEating;
            this.RoomBlockingEnabled = Data.RoomBlockingEnabled;
            this.Hidewall = Data.Hidewall;
            this.Group = Data.Group;
            this.GroupId = Data.GroupId;

            this.Password = Data.Password;
            this.Wallpaper = Data.Wallpaper;
            this.Floor = Data.Floor;
            this.Landscape = Data.Landscape;
            this._hideWired = Data.HideWired;
            this.WallThickness = Data.WallThickness;
            this.FloorThickness = Data.FloorThickness;

            this.chatMode = Data.chatMode;
            this.chatSize = Data.chatSize;
            this.chatSpeed = Data.chatSpeed;
            this.chatDistance = Data.chatDistance;
            this.extraFlood = Data.extraFlood;

            this.TradeSettings = Data.TradeSettings;

            this.WhoCanBan = Data.WhoCanBan;
            this.WhoCanKick = Data.WhoCanKick;
            this.WhoCanBan = Data.WhoCanBan;

            this.PushEnabled = Data.PushEnabled;
            this.PullEnabled = Data.PullEnabled;
            this.SPullEnabled = Data.SPullEnabled;
            this.SPushEnabled = Data.SPushEnabled;
            this.EnablesEnabled = Data.EnablesEnabled;
            this.RespectNotificationsEnabled = Data.RespectNotificationsEnabled;
            this.PetMorphsAllowed = Data.PetMorphsAllowed;

            this.poolQuestion = string.Empty;
            this.yesPoolAnswers = new List<int>();
            this.noPoolAnswers = new List<int>();
            this.WardrobeEnabled = Data.WardrobeEnabled;
            this.PhoneStoreEnabled = Data.PhoneStoreEnabled;
            this.MallEnabled = Data.MallEnabled;
            this.SupermarketEnabled = Data.SupermarketEnabled;
            this.BuyCarEnabled = Data.BuyCarEnabled;
            this.BankEnabled = Data.BankEnabled;
            //this.BankBalance = Data.BankBalance;
            this.ShootEnabled = Data.ShootEnabled;
            this.HitEnabled = Data.HitEnabled;
            this.SafeZoneEnabled = Data.SafeZoneEnabled;
            this.SexCommandsEnabled = Data.SexCommandsEnabled;
            this.TurfEnabled = Data.TurfEnabled;
            this.RobEnabled = Data.RobEnabled;
            this.GymEnabled = Data.GymEnabled;
            this.DeliveryEnabled = Data.DeliveryEnabled;
            this.TutorialEnabled = Data.TutorialEnabled;
            this.DriveEnabled = Data.DriveEnabled;
            this.TaxiFromEnabled = Data.TaxiFromEnabled;
            this.TaxiToEnabled = Data.TaxiToEnabled;
            this.BusToEnabled = Data.BusToEnabled;
            this.EnterRoomMessage = Data.EnterRoomMessage;
            this.IsHospital = Data.IsHospital;
            this.IsPrison = Data.IsPrison;
            this.IsPrison2 = Data.IsPrison2;
            this.IsPolStation = Data.IsPolStation;
            this.IsCamionero = Data.IsCamionero;
            this.IsMecanico = Data.IsMecanico;
            this.IsBasurero = Data.IsBasurero;
            this.ActiveTrades = new List<Trade>();
            this.Bans = new Dictionary<int, double>();
            this.MutedUsers = new Dictionary<int, double>();
            this.Tents = new Dictionary<int, List<RoomUser>>();
           
            _gamemap = new Gamemap(this);

            if (_roomItemHandling == null)
                _roomItemHandling = new RoomItemHandling(this);

            _roomUserManager = new RoomUserManager(this);

            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            this._traxManager = new RoomTraxManager(this);
            //OnFurnisLoad();

            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();

            this.LoadPromotions();
            this.LoadRights();
            this.LoadBans();
            this.LoadFilter();
            this.InitBots();

            if (RoleplayBotManager.isInit == false)
            {
                RoleplayBotManager.Initialize(false);
            }
            this.InitPets();

            if (this.GetRoomUserManager() != null && this.GetRoomUserManager().GetRoomUsers() != null && this.GetRoomUserManager().GetRoomUsers().Where(x => !x.IsBot) != null)
                Data.UsersNow = this.GetRoomUserManager().GetRoomUsers().Where(x => !x.IsBot).ToList().Count;
            else
                Data.UsersNow = 0;

            StartRoomProcessing();
            //StartWiredsProcess();
        }

        internal void StartRoomProcessing()
        {
            if (_mainProcessSource == null)
                _mainProcessSource = new CancellationTokenSource();

            _processTask = new Task(async () =>
            {
                while (!_mainProcessSource.IsCancellationRequested)
                {
                    try
                    {
                        var start = PolarEnvironment.GetIUnixTimestamp();
                        await ProcessRoom();
                        var end = PolarEnvironment.GetIUnixTimestamp();
                        var wait = 450 - (end - start);
                        if (wait <= 0)
                            wait = 0;
                        await Task.Delay(wait);
                    }
                    catch (Exception e)
                    {
                        Logging.HandleException(e, "RoomProcessing");
                    }
                }
            }, _mainProcessSource.Token);

            _processTask.Start();
        }

        internal void StartWiredsProcess()
        {
            if (_processingWireds || _mainProcessSource == null) return;

            try
            {
                _processingWireds = true;

                new Task(async () =>
                {
                    while (_wiredComponent != null && !_mainProcessSource.IsCancellationRequested)
                    {
                        try
                        {
                            _wiredComponent.OnCycle();
                        }
                        catch (Exception e)
                        {
                            Logging.HandleException(e, "WiredProcess");
                        }

                        await Task.Delay(240);
                    }
                }, _mainProcessSource.Token, TaskCreationOptions.LongRunning).Start();
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "StartWiredProcess");
            }
        }
        public List<string> WordFilterList
        {
            get { return this._wordFilterList; }
            set { this._wordFilterList = value; }
        }

        public bool GamblingRoom
        {
            get { return this._gamblingRoom; }
            set { this._gamblingRoom = value; }
        }

        #region Room Bans

        public bool UserIsBanned(int pId)
        {
            return Bans.ContainsKey(pId);
        }

        public void RemoveBan(int pId)
        {
            Bans.Remove(pId);
        }

        public void AddBan(int pId, long Time)
        {
            if (!Bans.ContainsKey(Convert.ToInt32(pId)))
                Bans.Add(pId, PolarEnvironment.GetUnixTimestamp() + Time);

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `room_bans` VALUES (" + pId + ", " + Id + ", " + (PolarEnvironment.GetUnixTimestamp() + Time) + ")");
            }
        }

        public List<int> BannedUsers()
        {
            var Bans = new List<int>();

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id FROM room_bans WHERE expire > UNIX_TIMESTAMP() AND room_id=" + Id);
                DataTable Table = dbClient.getTable();

                foreach (DataRow Row in Table.Rows)
                {
                    Bans.Add(Convert.ToInt32(Row[0]));
                }
            }

            return Bans;
        }

        public bool HasBanExpired(int pId)
        {
            if (!UserIsBanned(pId))
                return true;

            if (Bans[pId] < PolarEnvironment.GetUnixTimestamp())
                return true;

            return false;
        }

        public void Unban(int UserId)
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `room_bans` WHERE `user_id` = '" + UserId + "' AND `room_id` = '" + Id + "' LIMIT 1");
            }

            if (Bans.ContainsKey(UserId))
                Bans.Remove(UserId);
        }

        #endregion

        #region Trading
        public bool HasActiveTrade(RoomUser User)
        {
            if (User.IsBot)
                return false;
            else
                return this.HasActiveTrade(User.GetClient().GetHabbo().Id);
        }

        public bool HasActiveTrade(int UserId)
        {
            foreach (Trade trade in this.ActiveTrades)
            {
                if (trade.ContainsUser(UserId))
                    return true;
            }

            return false;
        }

        public Trade GetUserTrade(int UserId)
        {
            foreach (Trade trade in this.ActiveTrades)
            {
                if (trade.ContainsUser(UserId))
                    return trade;
            }

            return (Trade)null;
        }

        public void TryStartTrade(RoomUser UserOne, RoomUser UserTwo)
        {
            if (UserOne == null || UserTwo == null)
                return;
            if ((UserOne.IsBot || UserTwo.IsBot) || (UserOne.IsTrading || UserTwo.IsTrading ||
                                                     (this.HasActiveTrade(UserOne) || this.HasActiveTrade(UserTwo))))
                return;

            this.ActiveTrades.Add(new Trade(UserOne.GetClient().GetHabbo().Id, UserTwo.GetClient().GetHabbo().Id,
                this.Id));
        }

        public void TryStopTrade(int UserId)
        {
            Trade userTrade = this.GetUserTrade(UserId);
            if (userTrade == null)
                return;
            userTrade.CloseTrade(UserId);
            this.ActiveTrades.Remove(userTrade);
        }
        #endregion
        public Task RunTask(Func<Task> callBack)
        {
            var task = Task.Run(async () =>
            {
                if (this.mDisposed)
                {
                    return;
                }

                await callBack();

            }, this._cancellationTokenSource.Token);

            return task;
        }

        public List<ServerPacket> HideWiredMessages(bool hideWired)
        {
            List<ServerPacket> list = new List<ServerPacket>();
            Item[] items = this.GetRoomItemHandler().GetFloor.ToArray();
            if (hideWired)
            {
                for (int i = 0; i < items.Count(); i++)
                {
                    Item item = items[i];
                    if (!item.IsWired)
                        continue;
                    list.Add(new ObjectRemoveComposer(item, 0));
                }
            }
            else
            {
                for (int i = 0; i < items.Count(); i++)
                {
                    Item item = items[i];
                    if (!item.IsWired)
                        continue;
                    list.Add(new ObjectAddComposer(item, this));
                }
            }
            return list;
        }

        public bool HideWired
        {
            get { return this._hideWired; }
            set { this._hideWired = value; }
        }

        public RoomTraxManager GetTraxManager()
        {
            return this._traxManager;
        }

        public int UserCount
        {
            get { return _roomUserManager.GetRoomUsers().Count; }
        }

        public int RoomId
        {
            get { return Id; }
        }


        public bool CanTradeInRoom
        {
            get { return true; }
        }

        public RoomData RoomData
        {
            get { return _roomData; }
        }

        public Gamemap GetGameMap()
        {
            return _gamemap;
        }

        public RoomItemHandling GetRoomItemHandler()
        {
            if (_roomItemHandling == null)
            {
                _roomItemHandling = new RoomItemHandling(this);
            }
            return _roomItemHandling;
        }

        public RoomUserManager GetRoomUserManager()
        {
            return _roomUserManager;
        }
        public Soccer GetSoccer()
        {
            if (_soccer == null)
            {
                _soccer = new Soccer(this);
            }

            return this._soccer;
        }

        public TeamManager GetTeamManagerForBanzai()
        {
            if (teambanzai == null)
                teambanzai = TeamManager.createTeamforGame("banzai");
            return teambanzai;
        }

        public TeamManager GetTeamManagerForFreeze()
        {
            if (teamfreeze == null)
                teamfreeze = TeamManager.createTeamforGame("freeze");
            return teamfreeze;
        }

        public BattleBanzai GetBanzai()
        {
            if (_banzai == null)
                _banzai = new BattleBanzai(this);
            return _banzai;
        }

        public Freeze GetFreeze()
        {
            if (_freeze == null)
                _freeze = new Freeze(this);
            return _freeze;
        }

        public GameManager GetGameManager()
        {
            if (_gameManager == null)
                _gameManager = new GameManager(this);
            return _gameManager;
        }

        public GameItemHandler GetGameItemHandler()
        {
            if (_gameItemHandler == null)
                _gameItemHandler = new GameItemHandler(this);
            return _gameItemHandler;
        }

        public bool GotSoccer()
        {
            return (_soccer != null);
        }

        public bool GotBanzai()
        {
            return (_banzai != null);
        }

        public bool GotFreeze()
        {
            return (_freeze != null);
        }

        public void ClearTags()
        {
            Tags.Clear();
        }

        public void setPoolQuestion(String pool)
        {
            this.poolQuestion = pool;
        }

        public void clearPoolAnswers()
        {
            this.yesPoolAnswers.Clear();
            this.noPoolAnswers.Clear();
        }

        public void startQuestion(String question, int Time)
        {
            setPoolQuestion(question);
            clearPoolAnswers();

            SendMessage(new QuickPollMessageComposer(question, Time));
        }
        public void endQuestion()
        {
            setPoolQuestion(string.Empty);
            SendMessage(new QuickPollResultsMessageComposer(yesPoolAnswers.Count, noPoolAnswers.Count));


            clearPoolAnswers();
        }

        public void AddTagRange(List<string> tags)
        {
            Tags.AddRange(tags);
        }


        /*public void InitBots()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_bots` WHERE `spawn_id` > '0'");
                DataTable Data = dbClient.getTable();
                if (Data == null)
                    return;

                Task.Run(async delegate
                {
                    await Task.Delay(2000);
                    RoleplayBotManager.FetchCachedBots();
                    await Task.Delay(100);
                    RoleplayBotManager.DeployCachedBots();
                });
            }

        }*/

        public void InitBots()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` != 'pet'");
                DataTable Data = dbClient.getTable();
                if (Data == null)
                    return;

                foreach (DataRow Bot in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = '" + Convert.ToInt32(Bot["id"]) + "'");
                    DataTable BotSpeech = dbClient.getTable();

                    List<RandomSpeech> Speeches = new List<RandomSpeech>();

                    foreach (DataRow Speech in BotSpeech.Rows)
                    {
                        Speeches.Add(new RandomSpeech(Convert.ToString(Speech["text"]), Convert.ToInt32(Bot["id"])));
                    }

                    _roomUserManager.DeployBot(new RoomBot(Convert.ToInt32(Bot["id"]), Convert.ToInt32(Bot["room_id"]), Convert.ToString(Bot["ai_type"]), Convert.ToString(Bot["walk_mode"]), Convert.ToString(Bot["name"]), Convert.ToString(Bot["motto"]), Convert.ToString(Bot["look"]), int.Parse(Bot["x"].ToString()), int.Parse(Bot["y"].ToString()), int.Parse(Bot["z"].ToString()), int.Parse(Bot["rotation"].ToString()), 0, 0, 0, 0, ref Speeches, "M", 0, Convert.ToInt32(Bot["user_id"].ToString()), Convert.ToBoolean(Bot["automatic_chat"]), Convert.ToInt32(Bot["speaking_interval"]), PolarEnvironment.EnumToBool(Bot["mix_sentences"].ToString()), Convert.ToInt32(Bot["chat_bubble"])), null);
                }
            }
        }

        public void InitPets()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` = 'pet'");
                DataTable Data = dbClient.getTable();

                if (Data == null)
                    return;

                foreach (DataRow Row in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" + Row[0] + "' LIMIT 1");
                    DataRow mRow = dbClient.getRow();
                    if (mRow == null)
                        continue;

                    Pet Pet = new Pet(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]), Convert.ToInt32(Row["room_id"]), Convert.ToString(Row["name"]), Convert.ToInt32(mRow["type"]), Convert.ToString(mRow["race"]),
                        Convert.ToString(mRow["color"]), Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]), Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]),
                        Convert.ToDouble(Row["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]), Convert.ToString(mRow["gnome_clothing"]));

                    var RndSpeechList = new List<RandomSpeech>();

                    _roomUserManager.DeployBot(new RoomBot(Pet.PetId, RoomId, "pet", "freeroam", Pet.Name, "", Pet.Look, Pet.X, Pet.Y, Convert.ToInt32(Pet.Z), 0, 0, 0, 0, 0, ref RndSpeechList, "", 0, Pet.OwnerId, false, 0, false, 0), Pet);
                }
            }
        }

        public FilterComponent GetFilter()
        {
            return _filterComponent;
        }

       /* public WiredComponent GetWired()
        {
            return _wiredComponent;
        }*/

        public WiredComponent GetWired()
        {
            if (_wiredComponent != null)
                return _wiredComponent;

            _wiredComponent = new WiredComponent(this);
            //StartWiredsProcess();

            return _wiredComponent;

        }

        public void LoadPromotions()
        {
            DataRow GetPromotion = null;
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_promotions` WHERE `room_id` = " + this.Id + " LIMIT 1;");
                GetPromotion = dbClient.getRow();

                if (GetPromotion != null)
                {
                    if (Convert.ToDouble(GetPromotion["timestamp_expire"]) > PolarEnvironment.GetUnixTimestamp())
                        RoomData._promotion = new RoomPromotion(Convert.ToString(GetPromotion["title"]), Convert.ToString(GetPromotion["description"]), Convert.ToDouble(GetPromotion["timestamp_start"]), Convert.ToDouble(GetPromotion["timestamp_expire"]), Convert.ToInt32(GetPromotion["category_id"]));
                }
            }
        }

        public void LoadRights()
        {
            UsersWithRights = new List<int>();
            if (Group != null)
                return;

            DataTable Data = null;

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.getTable();
            }

            if (Data != null)
            {
                foreach (DataRow Row in Data.Rows)
                {
                    UsersWithRights.Add(Convert.ToInt32(Row["user_id"]));
                }
            }
        }

        public List<Item> GetItemsByInteraction(InteractionType ItemInteraction)
        {
            if (this.GetRoomItemHandler() == null)
                return new List<Item>();

            if (this.GetRoomItemHandler().GetFloor == null)
                return new List<Item>();

            return this.GetRoomItemHandler().GetFloor
            .Where(Item => Item != null)
            .Where(Item => Item.GetBaseItem() != null)
            .Where(Item => Item.GetBaseItem().InteractionType == ItemInteraction).ToList();
        }

        public List<Item> GetItemsByName(string ItemName)
        {
            if (this.GetRoomItemHandler() == null)
                return new List<Item>();

            if (this.GetRoomItemHandler().GetFloor == null)
                return new List<Item>();

            return this.GetRoomItemHandler().GetFloor
            .Where(Item => Item != null)
            .Where(Item => Item.GetBaseItem() != null)
            .Where(Item => Item.GetBaseItem().ItemName.ToLower() == ItemName.ToLower()).ToList();
        }

        private void LoadFilter()
        {
            this._wordFilterList = new List<string>();

            DataTable Data = null;
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.getTable();
            }

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                this._wordFilterList.Add(Convert.ToString(Row["word"]));
            }
        }

        public void LoadBans()
        {
            this.Bans = new Dictionary<int, double>();

            DataTable Bans;

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id, expire FROM room_bans WHERE room_id = " + Id);
                Bans = dbClient.getTable();
            }

            if (Bans == null)
                return;

            foreach (DataRow ban in Bans.Rows)
            {
                this.Bans.Add(Convert.ToInt32(ban[0]), Convert.ToDouble(ban[1]));
            }
        }

        public bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            return (((Math.Abs((int)(X1 - X2)) <= 1) && (Math.Abs((int)(Y1 - Y2)) <= 1)) || ((X1 == X2) && (Y1 == Y2)));
        }

        public bool CheckRights(GameClient Session)
        {
            return CheckRights(Session, false);
        }

        public bool CheckRights(GameClient Session, bool RequireOwnership, bool CheckForGroups = false)
        {
            try
            {
                if (Session == null || Session.GetHabbo() == null)
                    return false;

                if (Session.GetHabbo().Username == OwnerName && Type == "private")
                    return true;

                if (Session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                    return true;

                if (!RequireOwnership && Type == "private")
                {
                    // Si es dueño de la Casa (Interiores)
                    House House;
                    if (this.TryGetHouse(out House))
                    {
                        if (House != null && House.OwnerId == Session.GetHabbo().Id && !House.ForSale)
                            return true;
                    }

                    if (Session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                        return true;
                }

                if (UsersWithRights.Contains(Session.GetHabbo().Id))
                    return true;

                /*if (CheckForGroups && Type == "private")
                {
                    if (Group == null)
                        return false;

                    if (Group.IsAdmin(Session.GetHabbo().Id))
                        return true;

                    if (Group.AdminOnlyDeco == 0)
                    {
                        if (Group.IsAdmin(Session.GetHabbo().Id))
                            return true;
                    }
                }*/

                return false;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.CheckRights");
            }

            return false;
        }

        // New Poner items en área Terreno
        public bool CheckTerrain(GameClient Session, string[] Data)
        {
            if (Data.Length < 4)
                return false;

            int X = 0;
            int Y = 0;

            if (!int.TryParse(Data[1], out X)) { return false; }
            if (!int.TryParse(Data[2], out Y)) { return false; }

            List<House> Houses = PolarEnvironment.GetGame().GetHouseManager().GetTerrainsBySignRoomId(this.Id);

            if (Houses.Count <= 0)
                return false;

            foreach (House House in Houses)
            {
                if (House.OwnerId == Session.GetHabbo().Id)
                {
                    int lix = 0;// Límite inferior X
                    int lsx = 0;// Límite superior X
                    int liy = 0;// Límite inferior Y
                    int lsy = 0;// Límite superior Y

                    try
                    {
                        int.TryParse(House.Space[0].Split(',')[0], out lix);//1
                        int.TryParse(House.Space[1].Split(',')[0], out lsx);//10
                        int.TryParse(House.Space[2].Split(',')[0], out liy);//1
                        int.TryParse(House.Space[3].Split(',')[0], out lsy);//10

                        if (X >= lix && X <= lsx && Y >= liy && Y <= lsy)
                            return true;
                    }
                    catch { }
                }
            }

            return false;
        }

        // New Poner items en área Terreno
        public bool CheckTerrain(GameClient Session, int furnix, int furniy)
        {
            if (Session == null)
                return false;

            int X = furnix;
            int Y = furniy;

            List<House> Houses = PolarEnvironment.GetGame().GetHouseManager().GetTerrainsBySignRoomId(this.Id);

            if (Houses.Count <= 0)
                return false;

            foreach (House House in Houses)
            {
                if (House.OwnerId == Session.GetHabbo().Id)
                {
                    int lix = 0;// Límite inferior X
                    int lsx = 0;// Límite superior X
                    int liy = 0;// Límite inferior Y
                    int lsy = 0;// Límite superior Y

                    try
                    {
                        int.TryParse(House.Space[0].Split(',')[0], out lix);//1
                        int.TryParse(House.Space[1].Split(',')[0], out lsx);//10
                        int.TryParse(House.Space[2].Split(',')[0], out liy);//1
                        int.TryParse(House.Space[3].Split(',')[0], out lsy);//10

                        if (X >= lix && X <= lsx && Y >= liy && Y <= lsy)
                            return true;
                    }
                    catch { }
                }
            }

            return false;
        }

        public void OnUserShoot(RoomUser User, Item Ball)
        {
            Func<Item, bool> predicate = null;
            string Key = null;
            foreach (Item item in this.GetRoomItemHandler().GetFurniObjects(Ball.GetX, Ball.GetY).ToList())
            {
                if (item.GetBaseItem().ItemName.StartsWith("fball_goal_"))
                {
                    Key = item.GetBaseItem().ItemName.Split(new char[] { '_' })[2];
                    User.UnIdle();
                    User.DanceId = 0;


                    PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(User.GetClient(), "ACH_FootballGoalScored", 1);

                    SendMessage(new ActionComposer(User.VirtualId, 1));
                }
            }

            if (Key != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.GetBaseItem().ItemName == ("fball_score_" + Key);
                }

                foreach (Item item2 in this.GetRoomItemHandler().GetFloor.Where<Item>(predicate).ToList())
                {
                    if (item2.GetBaseItem().ItemName == ("fball_score_" + Key))
                    {
                        if (!String.IsNullOrEmpty(item2.ExtraData))
                            item2.ExtraData = (Convert.ToInt32(item2.ExtraData) + 1).ToString();
                        else
                            item2.ExtraData = "1";
                        item2.UpdateState();
                    }
                }
            }
        }
        public async Task ProcessRoom()
        {
            if (mDisposed)
                return;

            try
            {
                var timeStarted = DateTime.Now;
                if (this.GetRoomUserManager().GetRoomUsers().Count == 0 && this.GetRoomUserManager().GetRoleplayBots().Count == 0)
                    this.IdleTime++;
                else if (this.IdleTime > 0)
                    this.IdleTime = 0;

                if (HasActivePromotion && Promotion.HasExpired) EndPromotion();
                if (IdleTime >= 60 && !HasActivePromotion)
                {
                    await PolarEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
                    return;
                }

                /*if (!this.mCycleEnded)
                {
                if (this.IdleTime >= 60)
                    {
                        PolarEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
                        return Task.CompletedTask;
                    }
                    else
                    {
                        this.GetRoomUserManager().SerializeStatusUpdates();
                    }
                //}*/

                try
                {
                    GetRoomItemHandler().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
                try
                {
                    GetRoomUserManager().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
                try
                {
                    GetRoomUserManager().SerializeStatusUpdates();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }

                try
                {
                    if (_gameItemHandler != null)
                        _gameItemHandler.OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
                try
                {
                    GetWired().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }

                try
                {
                    this._traxManager.OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
                if (timeStarted > this._saveFurnitureTimerLast + this._saveFurnitureTimer)
                {
                    this._saveFurnitureTimerLast = timeStarted;

                    this._roomItemHandling.SaveFurniture();
                }

                var timeEnded = DateTime.Now;

                var timeExecution = timeEnded - timeStarted;

            }
            catch (Exception e)
            {
                Logging.WriteLine("Room ID [" + RoomId + "] se crashed.");
                Logging.LogException("Room ID [" + RoomId + "] se crashed." + e.ToString());
                OnRoomCrash(e);
            }
            return;
        }

        private void OnRoomCrash(Exception e)
        {
            Logging.LogThreadException(e.ToString(), "Tarea de ciclo de habitación para habitación " + RoomId);

            try
            {
                foreach (RoomUser user in _roomUserManager.GetRoomUsers().ToList())
                {
                    if (user == null || user.GetClient() == null)
                        continue;

                    user.GetClient().SendNotification("Lo siento, parece que la habitación se ha estrellado.");//Unhandled exception in room: " + e);

                    try
                    {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                    }
                    catch (Exception e2) { Logging.LogException(e2.ToString()); }
                }
            }
            catch (Exception e3) { Logging.LogException(e3.ToString()); }

            isCrashed = true;
            PolarEnvironment.GetGame().GetRoomManager().UnloadRoom(this, true);
        }

        public bool CheckMute(GameClient Session)
        {
            if (MutedUsers.ContainsKey(Session.GetHabbo().Id))
            {
                if (MutedUsers[Session.GetHabbo().Id] < PolarEnvironment.GetUnixTimestamp())
                    MutedUsers.Remove(Session.GetHabbo().Id);
                else
                    return true;
            }

            if (Session.GetHabbo().TimeMuted > 0 || (RoomMuted && Session.GetHabbo().Username != OwnerName))
                return true;

            return false;
        }

        public void AddChatlog(int Id, string Message)
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `chatlogs` (user_id, room_id, message, timestamp) VALUES (@user, @room, @message, @time)");
                dbClient.AddParameter("user", Id);
                dbClient.AddParameter("room", RoomId);
                dbClient.AddParameter("message", Message);
                dbClient.AddParameter("time", PolarEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetHouse(out House House) => PolarEnvironment.GetGame().GetHouseManager().HouseList.TryGetValue(this.RoomId, out House);


        public void SendObjects(GameClient Session)
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            Session.SendMessage(new HeightMapComposer(Room.GetGameMap().Model.Heightmap));
            Session.SendMessage(new FloorHeightMapComposer(Room, Room.GetGameMap().Model.GetRelativeHeightmap(), Room.GetGameMap().StaticModel.WallHeight));

            foreach (RoomUser RoomUser in _roomUserManager.GetUserList().ToList())
            {
                if (RoomUser == null)
                    continue;

                Session.SendMessage(new UsersComposer(RoomUser));

                if (RoomUser.IsBot && RoomUser.BotData.DanceId > 0)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.BotData.DanceId));
                else if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.IsDancing)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.DanceId));

                if (RoomUser.IsAsleep)
                    Session.SendMessage(new SleepComposer(RoomUser, true));

                if (RoomUser.CarryItemID > 0 && RoomUser.CarryTimer > 0)
                    Session.SendMessage(new CarryObjectComposer(RoomUser.VirtualId, RoomUser.CarryItemID));

                if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.CurrentEffect > 0)
                    Room.SendMessage(new AvatarEffectComposer(RoomUser.VirtualId, RoomUser.CurrentEffect));
            }

            Session.SendMessage(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            Session.SendMessage(new ObjectsComposer(Room.GetRoomItemHandler().GetFloor.ToArray(), this));
            Session.SendMessage(new ItemsComposer(Room.GetRoomItemHandler().GetWall.ToArray(), this));
        }
        /*public void SendObjects(GameClient Session)
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            Session.SendMessage(new HeightMapComposer(Room.GetGameMap().Model.Heightmap));
            Session.SendMessage(new FloorHeightMapComposer(Room, Room.GetGameMap().Model.GetRelativeHeightmap(), Room.GetGameMap().StaticModel.WallHeight));

            foreach (RoomUser RoomUser in _roomUserManager.GetUserList().ToList())
            {
                if (RoomUser == null)
                    continue;

                bool LoadBot = true;
                bool ShowUser = true;

                if (RoomUser.IsBot)
                    if (RoomUser.GetBotRoleplay() != null)
                        if (RoomUser.GetBotRoleplay().Invisible)
                            LoadBot = false;


                if (LoadBot)
                {
                    if (RoomUser.IsBot)
                        Session.SendMessage(new UsersComposer(RoomUser));
                    else
                    {
                        if (RoomUser.GetClient() != null)
                            if (RoomUser.GetClient().GetRoleplay() != null)
                                if (RoomUser.GetClient().GetRoleplay().Invisible)
                                    ShowUser = false;
                    }

                    if (Session.GetRoomUser() == null)
                        return;

                    if (this.TutorialEnabled)
                        ShowUser = false;

                    if (ShowUser)
                        Session.SendMessage(new UsersComposer(RoomUser));
                    else
                        Session.SendMessage(new UsersComposer(Session.GetRoomUser()));
                }

                if (RoomUser.IsBot && RoomUser.BotData.DanceId > 0)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.BotData.DanceId));
                else if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.IsDancing)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.DanceId));

                if (RoomUser.IsAsleep)
                    Session.SendMessage(new SleepComposer(RoomUser, true));

                if (RoomUser.CarryItemID > 0 && RoomUser.CarryTimer > 0)
                    Session.SendMessage(new CarryObjectComposer(RoomUser.VirtualId, RoomUser.CarryItemID));

                if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.CurrentEffect > 0)
                    Room.SendMessage(new AvatarEffectComposer(RoomUser.VirtualId, RoomUser.CurrentEffect));
            }

            Session.SendMessage(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            Session.SendMessage(new ObjectsComposer(Session, Room.GetRoomItemHandler().GetFloor.ToArray(), this));
            Session.SendMessage(new ItemsComposer(Room.GetRoomItemHandler().GetWall.ToArray(), this));
            Session.SendMessage(new Polar.Communication.Packets.Outgoing.HabboCamera.SetCameraPicturePriceComposer(Convert.ToInt32(PolarEnvironment.GetConfig().data["camera.price.coins"]), Convert.ToInt32(PolarEnvironment.GetConfig().data["camera.price.duckets"]), Convert.ToInt32(PolarEnvironment.GetConfig().data["camera.price.publish"])));
        }*/

        #region Tents
        public void AddTent(int TentId)
        {
            if (Tents.ContainsKey(TentId))
                Tents.Remove(TentId);

            Tents.Add(TentId, new List<RoomUser>());
        }

        public void RemoveTent(int TentId, Item Item)
        {
            if (!Tents.ContainsKey(TentId))
                return;

            List<RoomUser> Users = Tents[TentId];
            foreach (RoomUser User in Users.ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                    continue;

                User.GetClient().GetHabbo().TentId = 0;
            }

            if (Tents.ContainsKey(TentId))
                Tents.Remove(TentId);
        }

        public void AddUserToTent(int TentId, RoomUser User, Item Item)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                    Tents.Add(TentId, new List<RoomUser>());

                if (!Tents[TentId].Contains(User))
                    Tents[TentId].Add(User);
                User.GetClient().GetHabbo().TentId = TentId;
            }
        }

        public void RemoveUserFromTent(int TentId, RoomUser User, Item Item)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                    Tents.Add(TentId, new List<RoomUser>());

                if (Tents[TentId].Contains(User))
                    Tents[TentId].Remove(User);

                User.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int Id, int TentId, IServerPacket Packet)
        {
            if (!Tents.ContainsKey(TentId))
                return;

            foreach (RoomUser User in Tents[TentId].ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().MutedUsers.Contains(Id) || User.GetClient().GetHabbo().TentId != TentId)
                    continue;

                User.GetClient().SendMessage(Packet);
            }
        }
        #endregion

        #region Communication (Packets)
        public void SendMessage(IServerPacket Message, bool UsersWithRightsOnly = false)
        {
            if (Message == null)
                return;

            try
            {
                if (this == null || this._roomUserManager == null || this._roomUserManager.GetUserList() == null)
                {
                    //Logging.LogException("Room o RoomUserManager no están inicializados.");
                    return;
                }

                // Obtener la lista de usuarios
                var userList = this._roomUserManager.GetUserList();
                //List<RoomUser> Users = this._roomUserManager.GetUserList().ToList();

                foreach (RoomUser User in userList)
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null)
                        continue;

                    if (UsersWithRightsOnly && !this.CheckRights(User.GetClient()))
                        continue;

                    User.GetClient().SendMessage(Message);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SendMessage");
            }
        }

        public void BroadcastPacket(byte[] Packet)
        {
            foreach (RoomUser User in this._roomUserManager.GetUserList().ToList())
            {
                if (User == null || User.IsBot)
                    continue;

                if (User.GetClient() == null || User.GetClient().GetConnection() == null)
                    continue;

                User.GetClient().GetConnection().SendData(Packet);
            }
        }

        public void SendMessage(List<ServerPacket> Messages)
        {
            if (Messages.Count == 0)
                return;

            try
            {
                byte[] TotalBytes = new byte[0];
                int Current = 0;

                foreach (ServerPacket Packet in Messages.ToList())
                {
                    byte[] ToAdd = Packet.GetBytes();
                    int NewLen = TotalBytes.Length + ToAdd.Length;

                    Array.Resize(ref TotalBytes, NewLen);

                    for (int i = 0; i < ToAdd.Length; i++)
                    {
                        TotalBytes[Current] = ToAdd[i];
                        Current++;
                    }
                }

                this.BroadcastPacket(TotalBytes);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SendMessage List<ServerPacket>");
            }
        }
        #endregion

        private void SaveAI()
        {
            foreach (RoomUser User in GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (User == null || !User.IsBot)
                    continue;

                if (User.IsBot)
                {
                    using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                        dbClient.AddParameter("name", User.BotData.Name);
                        dbClient.AddParameter("look", User.BotData.Look);
                        dbClient.AddParameter("rotation", User.BotData.Rot);
                        dbClient.AddParameter("x", User.X);
                        dbClient.AddParameter("y", User.Y);
                        dbClient.AddParameter("z", User.Z);
                        dbClient.AddParameter("id", User.BotData.BotId);
                        dbClient.RunQuery();
                    }
                }
            }
        }
        
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (mDisposed)
                return;

            SendMessage(new CloseConnectionComposer());

            isCrashed = false;
            mDisposed = true;
            mCycleEnded = true;

            _mainProcessSource?.Cancel();
            if (_processTask != null)
                await _processTask;


            this.GetRoomItemHandler().SaveFurniture();
            
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '" + Id + "' LIMIT 1");
                }

                if (this._roomUserManager.PetCount > 0)
                    this._roomUserManager.UpdatePets();

                this.SaveAI();

                UsersNow = 0;
                RoomData.UsersNow = 0;

                UsersWithRights.Clear();
                Bans.Clear();
                MutedUsers.Clear();
                Tents.Clear();

                this.TonerData = null;
                this.MoodlightData = null;


                if (this._gameItemHandler != null)
                    this._gameItemHandler.Dispose();

                if (this._gameManager != null)
                    this._gameManager.Dispose();

                if (this._freeze != null)
                    this._freeze.Dispose();

                if (this._banzai != null)
                    this._banzai.Dispose();

                if (this._soccer != null)
                    this._soccer.Dispose();

                if (this._gamemap != null)
                    this._gamemap.Dispose();

                if (this._roomUserManager != null)
                    this._roomUserManager.Dispose();

                if (this._roomItemHandling != null)
                    this._roomItemHandling.Dispose();

                _filterComponent?.Cleanup();
                _wiredComponent?.Cleanup();

                this.ActiveTrades.Clear();

            new Task(async () =>
            {
                await Task.Delay(2500);
                _mainProcessSource?.Dispose();
                _mainProcessSource = null;
            }).Start();
        }

    }
}