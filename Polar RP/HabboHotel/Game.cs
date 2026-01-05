using System.Threading.Tasks;
﻿using Polar.HabboHotel.Catalog.FurniMatic;
using log4net;
using Polar.Database.Interfaces;
using Polar.Core;
using Polar.Communication.Packets;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Moderation;
using Polar.HabboHotel.Support;
using Polar.HabboHotel.Catalog;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Items.Televisions;
using Polar.HabboHotel.Navigator;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Quests;
using Polar.HabboHotel.Achievements;
using Polar.HabboHotel.LandingView;
using Polar.HabboHotel.Global;
using Polar.HabboHotel.Polls;
using Polar.HabboHotel.Games;
using Polar.HabboHotel.Talents;
using Polar.HabboHotel.Rooms.Chat;
using Polar.HabboHotel.Bots;
using Polar.HabboHotel.Cache;
using Polar.HabboHotel.Rewards;
using Polar.HabboHotel.Badges;
using Polar.HabboHotel.Permissions;
using Polar.HabboHotel.Subscriptions;
using Polar.HabboHotel.Guides;
using Polar.HabboRoleplay.Misc;
using Polar.HabboRoleplay.Events;
using Polar.HabboRoleplay.Weapons;
using Polar.HabboRoleplay.Skins;
using Polar.HabboRoleplay.Wizards;
using Polar.HabboRoleplay.Combat;
using Polar.HabboRoleplay.Food;
using Polar.HabboRoleplay.Turfs;
using Polar.HabboRoleplay.Houses;
using Polar.HabboHotel.Items.Crafting;
using Polar.HabboHotel.Roleplay.Web;
using Polar.HabboRoleplay.Farming;
using Polar.HabboRoleplay.Gambling;
using Polar.HabboRoleplay.Web.Util.ChatRoom;
using Polar.HabboRoleplay.VehicleOwned;
using Polar.HabboRoleplay.Vehicles;
using Polar.HabboRoleplay.Products;
using Polar.HabboRoleplay.RPRoom;
using Polar.HabboRoleplay.Comodin;
using Polar.HabboRoleplay.Phones;
using Polar.HabboRoleplay.PhoneOwned;
using Polar.HabboRoleplay.PhonesApps;
using Polar.HabboRoleplay.PhoneChat;
using Polar.Messages.Net.MusCommunication;
using Polar.HabboRoleplay.PlayInternet;
using System.Diagnostics;
using Polar.HabboHotel.Animations;
using Polar.HabboRoleplay.Apartments;
using Polar.HabboRoleplay.ApartmentsOwned;
using Polar.HabboHotel.Rooms.TraxMachine;
using JNogueira.Discord.Webhook.Client;
using Polar.HabboRoleplay.VehiclesJobs;

namespace Polar.HabboHotel
{
    public class Game
    {
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Game");

        private readonly PacketManager _packetManager;
        private readonly MusPacketManager _muspacketManager;
        private readonly GameClientManager _clientManager;
        private readonly ModerationManager _modManager;
        private readonly ModerationTool _moderationTool;//TODO: Initialize from the moderation manager.
        private readonly ItemDataManager _itemDataManager;
        private readonly CatalogManager _catalogManager;
        private readonly TalentManager _talentManager;
        private readonly TelevisionManager _televisionManager;//TODO: Initialize from the item manager.
        private readonly NavigatorManager _navigatorManager;
        private readonly RoomManager _roomManager;
        private readonly ChatManager _chatManager;
        private readonly GroupManager _groupManager;
       public bool ClientManagerCycleEnded;
        //public bool ClientManagerCycleEnded, RoomManagerCycleEnded;
        private readonly QuestManager _questManager;
        private readonly AnimationManager _animationManager;
        private readonly AchievementManager _achievementManager;
        private readonly TalentTrackManager _talentTrackManager;
        private readonly LandingViewManager _landingViewManager;//TODO: Rename class
        private readonly GameDataManager _gameDataManager;
        private readonly ServerStatusUpdater _globalUpdater;
        private readonly LanguageLocale _languageLocale;
        //private readonly AntiMutant _antiMutant;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        private readonly RewardManager _rewardManager;
        private readonly BadgeManager _badgeManager;
        private readonly PermissionManager _permissionManager;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly GuideManager _guideManager;
        private readonly PollManager _pollManager;
        private readonly HouseManager _houseManager;
        private readonly ApartmentOwnedManager _apartmentownedManager;
        private readonly WebEventManager _webEventManager;
        private readonly CrackableManager _crackableManager;
        private readonly TargetedOffersManager _targetedoffersManager;
        private readonly FurniMaticRewardsManager _furniMaticRewardsManager;
        private readonly PhoneChatManager _phonechatManager;
        private readonly VehiclesOwnedManager _vehiclesownedManager;
        private readonly PhonesOwnedManager _phonesownedManager;
        public readonly RPRoomManager _rproomManager;
        private readonly TurfManager _gangturfsManager;
        private readonly HallOfFame _HallOfFame;

        private Task _gameLoop;
        public static bool gameLoopEnabled = true;
        public bool gameLoopActive;
        public bool gameLoopEnded;
        private bool _cycleEnded;
        private readonly Stopwatch moduleWatch;

        public Game()
        {

            AbstractBar bar = new AnimatedBar();
            const int wait = 15, end = 5;

            Progress(bar, wait, end, "Cargando Packets...");
            this._packetManager = new PacketManager();
            this._muspacketManager = new MusPacketManager();

            Progress(bar, wait, end, "Cargando GameClientManager...");
            this._clientManager = new GameClientManager();

            this._modManager = new ModerationManager();

            Progress(bar, wait, end, "Cargando Herramientas de moderación...");
            this._moderationTool = new ModerationTool();

            Progress(bar, wait, end, "Cargando configuraciones extras...");
            ExtraSettings.RunExtraSettings();
            CatalogSettings.RunCatalogSettings();

            Progress(bar, wait, end, "Cargando Iems...");
            this._itemDataManager = new ItemDataManager();

            Progress(bar, wait, end, "Cargando Catalogo...");
            this._catalogManager = new CatalogManager();

            Progress(bar, wait, end, "Cargando Televisiones...");
            this._televisionManager = new TelevisionManager();

            Progress(bar, wait, end, "Cargando Navegador de salas...");
            this._navigatorManager = new NavigatorManager();

            Progress(bar, wait, end, "Cargando Salas...");
            this._roomManager = new RoomManager();
            this._roomManager.LoadModels();

            Progress(bar, wait, end, "Cargando Adminisrador de chat...");
            this._chatManager = new ChatManager();

            Progress(bar, wait, end, "Cargando Misiones...");
            this._questManager = new QuestManager();

            Progress(bar, wait, end, "Cargando Logros y talentos...");
            this._achievementManager = new AchievementManager();
            this._talentManager = new TalentManager();
            this._talentManager.Initialize();
            this._talentTrackManager = new TalentTrackManager();

            Progress(bar, wait, end, "Cargando Vista del hotel...");
            this._landingViewManager = new LandingViewManager();

            Progress(bar, wait, end, "Cargando Datos del juego...");
            this._gameDataManager = new GameDataManager();

            Progress(bar, wait, end, "Cargando Actualizaciones del sistema...");
            //this._globalUpdater = new ServerStatusUpdater();
            ServerStatusUpdater.Init();

            Progress(bar, wait, end, "Cargando Lenguajes...");
            this._languageLocale = new LanguageLocale();

            /*Progress(bar, wait, end, "Cargando Anti-Mutant...");
            this._antiMutant = new AntiMutant();*/

            Progress(bar, wait, end, "Cargando Bots...");
            this._botManager = new BotManager();

            Progress(bar, wait, end, "Cargando Administrador de caché...");
            this._cacheManager = new CacheManager();

            Progress(bar, wait, end, "Cargando Recompensas...");
            this._rewardManager = new RewardManager();

            Progress(bar, wait, end, "Cargando Jukebox...");
            //Jukebox
            TraxSoundManager.Init();

            Progress(bar, wait, end, "Cargando Administrador de placas...");
            this._badgeManager = new BadgeManager();
            this._badgeManager.Init();

            Progress(bar, wait, end, "Cargando Permisos...");
            this._permissionManager = new PermissionManager();
            this._permissionManager.Init();

            Progress(bar, wait, end, "Cargando Subscripciones...");
            this._subscriptionManager = new SubscriptionManager();
            this._subscriptionManager.Init();

            this._guideManager = new GuideManager();

            Progress(bar, wait, end, "Cargando Salón de la fama...");
            this._HallOfFame = new HallOfFame();

            Progress(bar, wait, end, "Cargando Administrador de encuestas...");
            int pollLoaded;
            this._pollManager = new PollManager();
            this._pollManager.Init(out pollLoaded);

            Progress(bar, wait, end, "Cargando Crafting...");
            this._crackableManager = new CrackableManager();
            this._crackableManager.Initialize(PolarEnvironment.GetDatabaseManager().GetQueryReactor());

            Progress(bar, wait, end, "Cargando Mega Ofertas...");
            this._targetedoffersManager = new TargetedOffersManager();
            this._targetedoffersManager.Initialize(PolarEnvironment.GetDatabaseManager().GetQueryReactor());

            /*Progress(bar, wait, end, "Cargando Furni-Matic...");
            this._furniMaticRewardsManager = new FurniMaticRewardsManager();
            this._furniMaticRewardsManager.Initialize(PolarEnvironment.GetDatabaseManager().GetQueryReactor());*/

            Progress(bar, wait, end, "Cargando Sistema Roleplay...");

            #region Roleplay Section
            RoleplayData.Initialize();
            EventManager.Initialize();
            CombatManager.Initialize();

            this._groupManager = new GroupManager();
            this._groupManager.Initialize();

            TexasHoldEmManager.Initialize();

            this._gangturfsManager = new TurfManager();
            this._gangturfsManager.Initialize();

            WeaponManager.Initialize();
            WSkinManager.Initialize();
            HechizosManager.Initialize();
            FoodManager.Initialize();
            FarmingManager.Initialize();
            CraftingManager.Initialize();
            LotteryManager.Initialize();
            ToDoManager.Initialize();
            BlackListManager.Initialize();
            BountyManager.Initialize();
            WebSocketChatManager.Initialiaze();
            ProductsManager.Initialize();
            RoleplayManager.AssingInventoryProducts();

            this._houseManager = new HouseManager();
            this._houseManager.Init();
            this._rproomManager = new RPRoomManager();
            this._rproomManager.Init();
            VehicleManager.Initialize();
            VehicleJobsManager.Initialize();
            PhoneManager.Initialize();
            PhoneAppManager.Initialize();
            PlayInternetManager.Init();
            this._phonechatManager = new PhoneChatManager();
            this._phonechatManager.Init();

            this._vehiclesownedManager = new VehiclesOwnedManager();
            this._vehiclesownedManager.Init();

            this._phonesownedManager = new PhonesOwnedManager();
            this._phonesownedManager.Init();
            ComodinManager.Initialize();
            ApartmentManager.Init();
            this._webEventManager = new WebEventManager();
            this._webEventManager.Init();

            this._animationManager = new AnimationManager();
            this._animationManager.Init();

            this._apartmentownedManager = new ApartmentOwnedManager();
            this._apartmentownedManager.Init();

            #endregion

            this.moduleWatch = new Stopwatch();

        }

        public HallOfFame GetHallOfFame()
        {
            return _HallOfFame;
        }

        public async Task InitializeAsync()
        {
            await this._itemDataManager.InitAsync();
            await this._catalogManager.InitAsync(this._itemDataManager);
        }


        public async Task SendMsPhoto(GameClient Author, string image)
        {
            string Webhook = PolarEnvironment.GetConfig().data["Webhook"];
            string Webhook_login_logout_ProfilePicture = PolarEnvironment.GetConfig().data["Webhook_Image"];
            string Webhook_login_logout_UserNameD = PolarEnvironment.GetConfig().data["Webhook_Username"];
            string Webhook_login_logout_WebHookurl = PolarEnvironment.GetConfig().data["Webhook_URL"];


                var client = new DiscordWebhookClient(Webhook_login_logout_WebHookurl);

                var message = new DiscordMessage(
                 "Una nueva foto! " + DiscordEmoji.Grinning,
                    username: Webhook_login_logout_UserNameD,
                    avatarUrl: Webhook_login_logout_ProfilePicture,
                    tts: false,
                    embeds: new[]
        {
                                new DiscordMessageEmbed(
                                "Notificacion de login" + DiscordEmoji.Thumbsup,
                                 color: 4833120,
                                author: new DiscordMessageEmbedAuthor(Author.GetHabbo().Username),
                                description: "Ha ingresado al cliente del hotel",
                                image: new DiscordMessageEmbedImage(image),
                                footer: new DiscordMessageEmbedFooter("Creado por "+Webhook_login_logout_UserNameD, Webhook_login_logout_ProfilePicture)
        )
        }
        );

                //agrega un catch

                await client.SendToDiscord(message);

                Console.WriteLine("login enviado a Discord ", ConsoleColor.DarkCyan);
        }
        public static void Progress(AbstractBar bar, int wait, int end, string message)
        {
            bar.PrintMessage(message);
            for (var cont = 0; cont < end; cont++)
                bar.Step();
        }

        public void StartGameLoop()
        {
            this.gameLoopActive = true;
            this._gameLoop = MainGameLoop();
        }
        private async Task MainGameLoop()
        {
            while (this.gameLoopActive)
            {
                this._cycleEnded = false;
                try
                {
                    if (gameLoopEnabled)
                    {
                        moduleWatch.Restart();
                        ServerStatusUpdater.Process();

                        if (moduleWatch.ElapsedMilliseconds > 480)
                            Console.WriteLine("High latency in LowPriorityWorker.Process ({0} ms)", moduleWatch.ElapsedMilliseconds);

                        this.moduleWatch.Restart();

                        PolarEnvironment.GetGame().GetClientManager().OnCycle();

                        if (moduleWatch.ElapsedMilliseconds > 480)
                            Console.WriteLine("High latency in GameClientManager ({0} ms)", moduleWatch.ElapsedMilliseconds);

                    }
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine("Canceled operation {0}", e);
                }
                this._cycleEnded = true;
                await Task.Delay(480);
            }

            Console.WriteLine("MainGameLoop end");
            this.gameLoopEnded = true;
        }

        public PacketManager GetPacketManager()
        {
            return _packetManager;
        }

        public MusPacketManager GetMusPacketManager()
        {
            return _muspacketManager;
        }

        public GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        public RPRoomManager GetRPRoomManager() => this._rproomManager;
        
        public CatalogManager GetCatalog() => _catalogManager;
        
        public NavigatorManager GetNavigator() => _navigatorManager;
        

        public TurfManager GetGangTurfsManager() => this._gangturfsManager;

        public ItemDataManager GetItemManager() => _itemDataManager;
        public RoomManager GetRoomManager() => _roomManager;

        public AnimationManager GetAnimationManager() => this._animationManager;
        public AchievementManager GetAchievementManager() => _achievementManager;
        
        public PhoneChatManager GetPhoneChatManager()
        {
            return this._phonechatManager;
        }
        public PhonesOwnedManager GetPhonesOwnedManager()
        {
            return this._phonesownedManager;
        }
        public VehiclesOwnedManager GetVehiclesOwnedManager()
        {
            return this._vehiclesownedManager;
        }

        public WebEventManager GetWebEventManager()
        {
            return _webEventManager;
        }

        public TalentTrackManager GetTalentTrackManager()
        {
            return _talentTrackManager;
        }

        public ModerationTool GetModerationTool()
        {
            return _moderationTool;
        }

        public ModerationManager GetModerationManager()
        {
            return this._modManager;
        }

        public TalentManager GetTalentManager()
        {
            return _talentManager;

        }

        public PermissionManager GetPermissionManager()
        {
            return this._permissionManager;
        }

        public SubscriptionManager GetSubscriptionManager()
        {
            return this._subscriptionManager;
        }

        public QuestManager GetQuestManager()
        {
            return this._questManager;
        }

        public GroupManager GetGroupManager()
        {
            return _groupManager;
        }

        public LandingViewManager GetLandingManager()
        {
            return _landingViewManager;
        }
        public TelevisionManager GetTelevisionManager()
        {
            return _televisionManager;
        }

        internal GuideManager GetGuideManager()
        {
            return _guideManager;
        }
        internal PollManager GetPollManager()
        {
            return _pollManager;
        }

        public ChatManager GetChatManager()
        {
            return this._chatManager;
        }

        public GameDataManager GetGameDataManager()
        {
            return this._gameDataManager;
        }

        public HouseManager GetHouseManager()
        {
            return this._houseManager;
        }

        public ApartmentOwnedManager GetApartmentOwnedManager()
        {
            return this._apartmentownedManager;
        }

        public LanguageLocale GetLanguageLocale()
        {
            return this._languageLocale;
        }
/*
        public AntiMutant GetAntiMutant()
        {
            return this._antiMutant;
        }
        */
        public static void DatabaseCleanup()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE users SET online = '0' WHERE online = '1'");
                dbClient.RunQuery("UPDATE users SET auth_ticket = '' WHERE auth_ticket != ''");
                dbClient.RunQuery("UPDATE rooms SET users_now = '0' WHERE users_now > '0'");
                dbClient.RunQuery("UPDATE server_status SET users_online = '0', loaded_rooms = '0'");
            }
        }

        public async Task DestroyAsync()
        {
            DatabaseCleanup();

            Console.WriteLine("Disposing RoomManager...");
            await _roomManager.DisposeAsync();

            this.GetClientManager();
            Console.WriteLine("Apagando servidor...");
        }
        public BotManager GetBotManager()
        {
            return this._botManager;
        }

        public TargetedOffersManager GetTargetedOffersManager()
        {
            return this._targetedoffersManager;
        }

        public CacheManager GetCacheManager()
        {
            return this._cacheManager;
        }

        public RewardManager GetRewardManager()
        {
            return this._rewardManager;
        }

        public BadgeManager GetBadgeManager()
        {
            return this._badgeManager;
        }

        public CrackableManager GetPinataManager()
        {
            return this._crackableManager;
        }

        public FurniMaticRewardsManager GetFurniMaticRewardsMnager()
        {
            return this._furniMaticRewardsManager;
        }
    }
}