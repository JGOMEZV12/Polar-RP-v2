using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polar.Database.Interfaces;
using Polar.Communication.Packets.Outgoing.Notifications;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Items.Wired;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Events;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Gangs;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Apartment;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Hospital;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Clothing;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Basurero;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Banking;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.General;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Timers;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Purchasing;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Toggles;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Marriage;

using Polar.HabboHotel.Rooms.Chat.Commands.VIP;
using Polar.HabboHotel.Rooms.Chat.Commands.Moderators;
using Polar.HabboHotel.Rooms.Chat.Commands.Moderators.Trials;
using Polar.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors;
using Polar.HabboHotel.Rooms.Chat.Commands.Administrators;
using Polar.HabboHotel.Rooms.Chat.Commands.Managers;
using Polar.HabboHotel.Rooms.Chat.Commands.Developers;
using Polar.HabboHotel.Rooms.Chat.Commands.Owners;
using Polar.HabboHotel.Rooms.Chat.Commands.SpecialRights;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Basic;
using Polar.HabboHotel.Rooms.Chat.Commands.Users.Generic.Bounties;
using Polar.HabboHotel.Rooms.Chat.Commands.Ambassadors;
using Polar.HabboHotel.Rooms.Chat.Pets.Commands;
using Polar.HabboHotel.Rooms.Chat.Commands.User.Fun;
using Polar.HabboHotel.Rooms.Chat.Commands.User;

namespace Polar.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        /// Command Prefix only applies to custom commands.
        /// </summary>
        private string _prefix = ":";

        /// <summary>
        /// Commands registered for use.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands;
        private readonly Dictionary<string, IChatCommand> _jobcommands;
        private readonly Dictionary<string, IChatCommand> _gangcommands;
        private readonly Dictionary<string, IChatCommand> _staffcommands;
        private readonly Dictionary<string, IChatCommand> _ambassadorcommands;
        private readonly Dictionary<string, IChatCommand> _loggedcommands;
        private readonly Dictionary<string, IChatCommand> _vipcommands;
        private readonly Dictionary<string, IChatCommand> _eventcommands;
        private List<string> _aliases;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string Prefix)
        {
            this._prefix = Prefix;
            this._commands = new Dictionary<string, IChatCommand>();
            this._jobcommands = new Dictionary<string, IChatCommand>();
            this._gangcommands = new Dictionary<string, IChatCommand>();
            this._ambassadorcommands = new Dictionary<string, IChatCommand>();
            this._staffcommands = new Dictionary<string, IChatCommand>();
            this._loggedcommands = new Dictionary<string, IChatCommand>();
            this._vipcommands = new Dictionary<string, IChatCommand>();
            this._eventcommands = new Dictionary<string, IChatCommand>();
            this._aliases = new List<string>();

            this.RegisterUsers();
            this.RegisterUsersGangs();
            this.RegisterUsersJobs();
            this.RegisterVIP();
            this.RegisterAmbassadors();
            this.RegisterTrialModerators();
            this.RegisterModerators();
            this.RegisterSeniorModerators();
            this.RegisterAdministrators();
            this.RegisterManagers();
            this.RegisterDevelopers();
            this.RegisterOwners();
            this.RegisterSpecialRights();
        }

        /// <summary>
        /// Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="Session">Session calling this method.</param>
        /// <param name="Message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public async Task<bool> Parse(GameClient Session, string Message)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return false;

            if (!Message.StartsWith(_prefix))
                return false;

            #region Commands List

            #region :commands
            if (Message.ToLower() == _prefix + "comandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Esta es la lista de comandos que tiene disponible:\n\n");
                List.Append(":tcomandos - Proporciona una lista de todos los comandos de trabajo disponibles.\n");
                List.Append(":pcomandos - Proporciona una lista de todos los comandos de pandilla disponibles.\n");
                List.Append(":vipcommands - Proporciona una lista de todos los comandos VIP disponibles.\n");
                List.Append(":staffcomandos - Proporciona una lista de todos los comandos de personal disponibles.\n------------------------------------------------------------------------------------\n");

                foreach (var CmdList in _commands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired))
                            continue;
                    }

                    //List.Append("\n");
                    List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion

            #region :jobcommands
            if (Message.ToLower() == _prefix + "tcomandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Esta es la lista de comandos de trabajo disponibles:\n\n");
                foreach (var CmdList in _jobcommands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    /*if (CmdList.Key == "trabajar" || CmdList.Key == "notrabajar" || CmdList.Key == "empresas" || CmdList.Key == "cinfo")
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (CmdList.Key == "promover" || CmdList.Key == "degradar")
                    {
                        if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    else if (CmdList.Key == "ra" || CmdList.Key == "togglera")
                    {
                        if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights") || Groups.GroupManager.HasJobCommand(Session, CmdList.Key.ToLower()))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    else
                    {
                        if (Groups.GroupManager.HasJobCommand(Session, CmdList.Key.ToLower()))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }*/
                    List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");

                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion

            #region :gangcommands
            if (Message.ToLower() == _prefix + "pcomandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Estos son los comandos disponibles de pandillas:\n\n");
                foreach (var CmdList in _gangcommands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    if (CmdList.Key == "ginfo" || CmdList.Key == "glist" || CmdList.Key == "turfs" || CmdList.Key == "gcreate" || CmdList.Key == "gleave")
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (CmdList.Key == "gcapture" && GroupManager.GetGang(Session.GetRoleplay().GangId).IsGang)
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (Groups.GroupManager.HasGangCommand(Session, CmdList.Key))
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion

            #region :staffcommands
            if (Message.ToLower() == _prefix + "staffcomandos")
            {
                if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                {
                    Session.SendWhisper("Tu no eres staff", 1);
                    return true;
                }
                else
                {
                    StringBuilder List = new StringBuilder();
                    List.Append("Esta es la lista de comandos de personal que tiene disponibles:\n\n");
                    foreach (var CmdList in _staffcommands.ToList())
                    {
                        if (_aliases.Contains(CmdList.Key.ToLower()))
                            continue;

                        if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                        {
                            if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired) && Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                                continue;
                        }

                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                    return true;
                }
            }
            #endregion

            #region :vipcommands
            if (Message.ToLower() == _prefix + "vipcommands")
            {
                if (Session.GetHabbo().VIPRank < 1 && !Session.GetHabbo().GetClubManager().HasSubscription("habbo_vip"))
                {
                    Session.SendWhisper("No eres VIP", 1);
                    return true;
                }
                else
                {
                    StringBuilder List = new StringBuilder();
                    List.Append("Esta es la lista de comandos vip que tiene disponibles:\n\n");
                    foreach (var CmdList in _vipcommands.ToList())
                    {
                        if (_aliases.Contains(CmdList.Key.ToLower()))
                            continue;

                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                    return true;
                }
            }
            #endregion

            #endregion

            if (Message == _prefix + "commandsnew" || Message == _prefix + "comandosnew")
            {
                // Enviamos WS de ventana de comandos.
                PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "open");
                return true;
            }
            Message = Message.Substring(1);
            string[] Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            IChatCommand Cmd = null;
            IChatCommand LogCmd = null;
            if (_commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                _loggedcommands.TryGetValue(Split[0].ToLower(), out LogCmd);

                if (Cmd == LogCmd)
                {
                    if (_staffcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "staff");
                    else if (_ambassadorcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "ambassador");
                    else if (_jobcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "job");
                    else if (_vipcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "vip");
                    else if (_eventcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "event");
                    else
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "user");
                }
                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                {
                    if (Split[0].ToLower() == "push")
                    {

                            if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                                return false;

                    }
                    else
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                            return false;
                    }
                }

                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                await Cmd.Execute(Session, Session.GetHabbo().CurrentRoom, Split);
                return true;
            }
            return false;
        }

        #region Commands
        /// <summary>
        /// User set of commands
        /// </summary>
        private void RegisterUsers()
        {
            // General
            this.Register("mpu", new MPUCommand());
            this.Register("text", new TextCommand());
            this.Register("re", new ReCommand());
            this.Register("info", new AboutCommand());
            this.Register("inmunidad", new InmunidadCommand());
            this.Register("actualizaciones", new ChangeLogCommand());
            this.Register("yo", new StatsCommand());
            this.Register("hidewired", new HideWiredCommand());
            this.Register("agriculturastats", new FarmingStatsCommand());
            this.Register("farming", new FarmingStatsCommand(), "", true);
            this.Register("misarmas", new WeaponsCommand());
            this.Register("conectados", new OnlineCommand());
            this.Register("online", new OnlineCommand(), "", true);
            this.Register("salasactivas", new HotRoomsCommand());
            this.Register("hr", new HotRoomsCommand(), "", true);
            this.Register("taxi", new TaxiCommand());
            this.Register("ir", new IrCommand());
            this.Register("bus", new BusCommand());
            this.Register("casinoinfo", new CasinoinfoCommand());
            this.Register("disco", new DiscoCommand());
            this.Register("notaxi", new StopTaxiCommand());
            this.Register("estoy", new RoomIDCommand());
            this.Register("salainfo", new RoomInfoCommand());
            this.Register("regenerarmapa", new RegenMapsCommand());
            this.Register("emptyitems", new EmptyItemsCommand());
            this.Register("poof", new PoofCommand());
            this.Register("desconectar", new LogOutCommand());
            this.Register("cambiarclase", new ChangeClassCommand());
            this.Register("flechas", new ArrowCommand());
            this.Register("walk", new ArrowCommand());

            // Timers & Cooldowns
            this.Register("timeleft", new TimeLeftCommand());
            this.Register("cooldowns", new CooldownsCommand());
            this.Register("cds", new CooldownsCommand(), "", true);
            this.Register("cd", new CooldownsCommand(), "", true);

            // Banking
            this.Register("saldo", new BalanceCommand());
            this.Register("tanque", new TanqueCommand());
            this.Register("cedula", new CedulaCommand());
            this.Register("basura", new CamionCommand());
            this.Register("depositar", new DepositCommand());
            this.Register("retirar", new WithdrawCommand());

            // Criminal Activity
            this.Register("leyes", new LawsCommand());
            this.Register("noticias", new TutorialCommand());
            this.Register("tutorialbr", new TutorialbrCommand());
            this.Register("businfo", new BusinfoCommand());
            this.Register("robar", new RobCommand());
            this.Register("robarbanco", new RobBankCommand());
            this.Register("robarcajero", new RobATMCommand());
            this.Register("robartienda", new RobartiendaCommand());
            this.Register("norobarbanco", new RobBankCommand(true));
            this.Register("norobarcajero", new RobATMCommand(true));
            
            this.Register("medicina", new MedicinaCommand());
            this.Register("caramelos", new CaramelosCommand());
            this.Register("botardrogas", new DisposeCommand());
            this.Register("consumir", new SmokeCommand());

            //dd
            this.Register("darpermisos", new GiveRightsCommand());
            this.Register("leer", new LearningCommand());
            this.Register("noleer", new NoleerCommand());
            this.Register("tirarbasura", new TirarBasuraCommand());

            // Purchasing Goods
            this.Register("comprarbalas", new BuyBulletsCommand());
            this.Register("bullets", new BuyBulletsCommand(), "", true);
            this.Register("comprarsaldo", new BuyCreditCommand());
            this.Register("credit", new BuyCreditCommand(), "", true);
            this.Register("comprarcombustible", new BuyFuelCommand());
            this.Register("fuel", new BuyFuelCommand(), "", true);
            this.Register("comprarticket", new BuyTicketCommand(), "userlog");
            this.Register("ticket", new BuyTicketCommand(), "userlog", true);

            // Combat
            this.Register("modocombate", new CombatModeCommand());
            this.Register("cmode", new CombatModeCommand(), "", true);
            this.Register("golpe", new HitCommand());
            this.Register("sacar", new EquipCommand());
            this.Register("hechizo", new HechizosCommand(), "", true);
            this.Register("equipar", new EquipCommand(), "", true);
            this.Register("guardar", new UnEquipCommand());
            this.Register("desequipar", new UnEquipCommand(), "", true);
            this.Register("disparar", new ShootCommand());
            this.Register("recargar", new ReloadGunCommand());
            this.Register("permiso", new PermisoCommand());
            this.Register("permisoweed", new PermisoWeedCommand());

            // Offers
            this.Register("dar", new GiveCommand(), "userlog");
            this.Register("ctransferir", new CtransferirCommand(), "userlog");
            this.Register("atransferir", new AtransferirCommand(), "userlog");
            this.Register("ofertas", new OffersCommand());
           //this.Register("ofrecer", new OfferCommand());
            this.Register("ofrecer", new SellCommand());// Old OfferCommand            
            this.Register("aceptar", new AcceptCommand());
            this.Register("rechazar", new DeclineCommand());
            this.Register("vender", new SellCommand());
            //this.Register("vender", new VenderCommand());
            this.Register("acc", new AcceptWeaponCommand());
            this.Register("renunciar", new RenunciarCommand(), "joblog");

            // Police Related
            this.Register("llamarpolicia", new CallPoliceCommand());
            this.Register("emergencia", new EmergenciaCommand());
            this.Register("911", new CallPoliceCommand());
            this.Register("fianza", new BailCommand());
            this.Register("rendicion", new SurrenderCommand());
            this.Register("buscados", new WantedListCommand());
            this.Register("wl", new WantedListCommand());
            this.Register("escoltar", new EscortCommand());

            #region Basurero
            this.Register("descargarcamion", new ReturnBasuCommand());
            #endregion
            #region Mecanico
            this.Register("reparar", new SellCommand());
            this.Register("mamada", new SellCommand());
            this.Register("revisar", new ReviewMecCommand());
            #endregion
            this.Register("crear", new CreateCommand());
            this.Register("servicio", new ServiceCommand());
            this.Register("mapa", new MapCommand());
            this.Register("map", new MapCommand());
            #region Camionero
            this.Register("cargas", new LoadsCamCommand());
            this.Register("cargarcamion", new CargarCamCommand());
            this.Register("depositarcarga", new DepositCamCommand());
            this.Register("entregarcamion", new ReturnCamCommand());
            this.Register("abandonarcarga", new LeaveCamCommand());
            #endregion
            // Court
            this.Register("juicio", new TrialCommand());
            this.Register("votar", new VoteCommand());

            // Toggles
            this.Register("apagartelefono", new ToggleTextsCommand());
            this.Register("desactivasusurros", new DisableWhispersCommand());
            this.Register("disablemimic", new DisableMimicCommand());

            // Self Interactions
            this.Register("manejar", new DriveCommand());
            this.Register("detener", new DriveCommand());
            //this.Register("nomanejar", new DriveCommand());
            this.Register("subir", new UpCommand());
            this.Register("bajar", new DownCommand());
            this.Register("abrircarro", new OpenCommand());
            this.Register("cerrarcarro", new CloseCommand());
            this.Register("localizar", new LocalizarCommand());
            this.Register("misautos", new MyCarsCommand());
            this.Register("autos", new MyCarsCommand());
            this.Register("comprarcarro", new BuyCarCommand());
            this.Register("sit", new SitCommand());
            this.Register("stand", new StandCommand());
            this.Register("lay", new LayCommand());
            this.Register("dance", new DanceCommand());
            this.Register("me", new MeCommand());

            // Item Interaction
            this.Register("llamarenvio", new CallDeliveryCommand());
            this.Register("comer", new EatCommand());
            this.Register("agarrar", new AgarrarCommand());
            //this.Register("kevlar", new KevlarCommand());
            this.Register("beber", new DrinkCommand());
            this.Register("entrenar", new WorkoutCommand());
            this.Register("plantar", new PlaceCommand());
            this.Register("place", new PlaceCommand());
            this.Register("colocar", new PlaceCommand());
            //this.Register("reparar", new PlaceCommand());
            this.Register("jugar", new JugarCommand());
            this.Register("llorar", new LlorarCommand());
            this.Register("manos", new ManosCommand());
            this.Register("reir", new ReirCommand());

            // Marriage Interaction
            this.Register("casarse", new MarryCommand());
            this.Register("hijo", new HijoCommand());
            this.Register("embarazar", new EmbarazarCommand());
            this.Register("propose", new MarryCommand(), "", true);
            this.Register("divorcio", new DivorceCommand());
            this.Register("abandonar", new AbandonarCommand());
            this.Register("sexo", new SexCommand());

            // User Interaction
            this.Register("buy", new BuyCommand());
            this.Register("comprar", new BuyCommand());
            this.Register("baul", new BaulCommand(), "logged");
            this.Register("maletero", new BaulCommand(), "logged");
            this.Register("usarbidon", new UseBidonCommand());
            this.Register("combustible", new BuyFuelCommand());
            this.Register("llenartanque", new BuyFuelFillCommand());
            this.Register("cachetada", new SlapCommand());
            this.Register("acariciar", new AcariciarCommand());
            this.Register("eyacular", new EyacularCommand());
            this.Register("besar", new KissCommand());
            this.Register("mear", new MearCommand());
            this.Register("masturbarse", new MasturbarseCommand());
            this.Register("tocar", new AgarratetaCommand());
            this.Register("estado", new EstadoCommand());
            this.Register("suicidar", new SuicidarCommand());
            this.Register("secuestrar", new SecuestrarCommand());
            this.Register("escupir", new EscupirCommand());
            this.Register("tequiero", new TequieroCommand());
            this.Register("teamo", new TeamoCommand());
            this.Register("patear", new PatearCommand());
            this.Register("oral", new OralCommand());
            this.Register("anal", new AnalCommand());
            this.Register("abrazar", new HugCommand());
            this.Register("violar", new RapeCommand());
            this.Register("nalgada", new NalgadaCommand());
            this.Register("coquetear", new CoquetearCommand());
            this.Register("masaje", new MasajeCommand());


            // Apartment
            this.Register("kick", new KickCommand());
            this.Register("roomkick", new RoomKickCommand());
            this.Register("pickall", new PickAllCommand(), "userlog");
            //this.Register("chooser", new ChooserCommand());
            this.Register("comprarcasa", new BuyApartmentCommand());
            this.Register("ponerprecio", new SetPriceommand(), "userlog");

            this.Register("entrar", new EnterCommand());
            this.Register("salir", new ExitCommand());

            // Events
            this.Register("eventstore", new PurchaseEventCommand(), "eventlog");
            this.Register("estore", new PurchaseEventCommand(), "eventlog", true);
            //this.Register("comprar", new PurchaseEventCommand(), "eventlog", true);

            // Gambling
            this.Register("apostar", new GamblingCommand(), "eventlog");
            this.Register("pasar", new GamblingCommand(), "eventlog", true);

            // Bounties
            this.Register("recompensa", new AddBountyCommand(), "userlog");
            this.Register("setb", new AddBountyCommand(), "userlog", true);
            this.Register("addbounty", new AddBountyCommand(), "userlog", true);
            this.Register("addb", new AddBountyCommand(), "userlog", true);
            this.Register("norecompensa", new RemoveBountyCommand(), "userlog");
            this.Register("removeb", new RemoveBountyCommand(), "userlog", true);
            this.Register("recompensas", new BountyListCommand(), "");
            this.Register("bl", new BountyListCommand(), "", true);
            this.Register("blist", new BountyListCommand(), "", true);

            // Translation
            this.Register("translate", new TranslateCommand());
            this.Register("trans", new TranslateCommand(), "", true);
            this.Register("stoptranslate", new StopTranslateCommand());
            this.Register("stranslate", new StopTranslateCommand(), "", true);
            this.Register("strans", new StopTranslateCommand(), "", true);

            // Misc
            // this.Register("ayuda", new HelpCommand());
            this.Register("ayuda", new HelpCommand());
            this.Register("helpamb", new AmbassadorHelpCommand());
            this.Register("ride", new RideCommand());

            #region Phones
            this.Register("minumero", new MyNumberCommand());
            this.Register("sms", new SmsCommand());
            this.Register("whats", new WhatsCommand());
            #endregion  

        }

        /// <summary>
        /// User set of gang commands
        /// </summary>
        private void RegisterUsersGangs()
        {
            this.Register("ginfo", new GangInfoCommand(), "gang");
            this.Register("glist", new GangListCommand(), "gang");
            this.Register("turfs", new GangTurfsCommand(), "gang");
            this.Register("gcreate", new GangCreateCommand(), "gang");
            this.Register("ginvite", new GangInviteCommand(), "gang");
            this.Register("gleave", new GangLeaveCommand(), "gang");
            this.Register("gkick", new GangKickCommand(), "gang");
            this.Register("gmsj", new GangMessageCommand(), "gang");
            this.Register("gcapture", new GangCaptureCommand(), "gang");
            this.Register("gbackup", new GangBackupCommand(), "gang");
            this.Register("grank", new GangRankCommand(), "gang");
            this.Register("gtransfer", new GangTransferCommand(), "gang");
            this.Register("gheal", new GangHealCommand(), "gang");
            this.Register("delgang", new DeleteGangCommand(), "gang");
        }

        /// <summary>
        /// User set of job commands
        /// </summary>
        private void RegisterUsersJobs()
        {
            // General
            this.Register("trabajar", new StartWorkCommand(), "joblog");
            this.Register("notrabajar", new StopWorkCommand(), "joblog");
            this.Register("empresas", new CorpListCommand(), "joblog");
            this.Register("clist", new CorpListCommand(), "joblog", true);
            this.Register("infoempresas", new CorpInfoCommand(), "joblog");
            this.Register("cinfo", new CorpInfoCommand(), "joblog", true);
            this.Register("promover", new PromoteCommand(), "joblog");
            this.Register("degradar", new DemoteCommand(), "joblog");
            this.Register("sendhome", new SendhomeCommand(), "joblog");
            this.Register("contratar", new HireCommand(), "joblog");
            this.Register("despedir", new FireCommand(), "joblog");
            this.Register("verminutos", new CheckMinutesCommand(), "joblog");
            this.Register("checkmins", new CheckMinutesCommand(), "joblog", true);

            // Hospital
            this.Register("revivir", new DischargeCommand(), "joblog");
            this.Register("aceptarmuerte", new AcceptDeathCommand());
            this.Register("curar", new HealCommand(), "joblog", true);
            this.Register("ayudar", new AyudarCommand(), "joblog");
            //this.Register("curar", new CurarCommand(), "joblog");
            this.Register("vacuna", new VacunaCommand(), "joblog");
            this.Register("pinchar", new PincharCommand(), "joblog");
            this.Register("ponerchaleco", new PonerchalecoCommand(), "joblog");
            this.Register("comprarchaleco", new ComprarChalecoCommand(), "joblog");
            //this.Register("chaleco", new ChalecopoliciaCommand(), "joblog");
            this.Register("explosivos", new ExplosivosCommand(), "joblog");
            this.Register("hidratar", new HidratacionCommand(), "joblog");
            this.Register("hidratacion", new HidratacionCommand(), "joblog", true);
            


            // Police
            this.Register("radio", new RadioAlertCommand(), "joblog");
            this.Register("r", new RadioAlertCommand(), "joblog");
            this.Register("tradio", new ToggleRadioAlertCommand(), "joblog");
            this.Register("toggleradio", new ToggleRadioAlertCommand(), "joblog");
            this.Register("buscar", new LawCommand(), "joblog");
            this.Register("kevlar", new KevlarCommand(), "joblog");
            this.Register("nobuscar", new UnLawCommand(), "joblog");
            this.Register("paralizar", new StunCommand(), "joblog");
            this.Register("desparalizar", new UnStunCommand(), "joblog");
           /* this.Register("spray", new StunCommand(), "joblog");
            this.Register("nospray", new UnStunCommand(), "joblog");*/
            this.Register("esposar", new CuffCommand(), "joblog");
            this.Register("noesposar", new UnCuffCommand(), "joblog");
            this.Register("cateo", new SearchCommand(), "joblog");
            this.Register("catear", new SearchCommand(), "joblog");
            this.Register("arrestar", new ArrestCommand(), "joblog");
            this.Register("liberar", new ReleaseCommand(), "joblog");
            this.Register("ptrial", new PoliceTrialCommand(), "joblog");
            this.Register("unptrial", new PoliceTrialCommand(), "joblog", true);
            this.Register("limpiarlista", new ClearWantedCommand(), "joblog");
            this.Register("cw", new ClearWantedCommand(), "joblog", true);
            this.Register("flashbang", new FlashBangCommand(), "joblog");
            this.Register("refuerzos", new BackupCommand(), "joblog");
            this.Register("ref", new BackupCommand(), "joblog");
            this.Register("carinfo", new CheckCarInfoCommand(), "joblog");
            this.Register("infocar", new CheckCarInfoCommand(), "joblog");

            // Restaurant & Cafe
            this.Register("servir", new ServeCommand(), "joblog");

            // Banking
            this.Register("abrircuenta", new OpenAccountCommand(), "joblog");
            this.Register("account", new OpenAccountCommand(), "joblog", true);
            this.Register("versaldo", new CheckBalanceCommand(), "joblog");

            // Clothing
            this.Register("descuento", new DiscountCommand(), "joblog");

            /*// Heticos
            this.Register("pasajero", new PasajeroCommand());
            this.Register("subir", new SubirCommand());*/

        }

        /// <summary>
        /// VIP set of commands
        /// </summary>
        private void RegisterVIP()
        {
            // Free fire
            //this.Register("poner", new PonerCommand(), "vip");
           
            this.Register("fastwalk", new FastwalkCommand(), "vip");
            this.Register("push", new PushCommand(), "vip");
            this.Register("pull", new PullCommand(), "vip");
            this.Register("flagme", new FlagMeCommand(), "vip");
            //this.Register("setsh", new SetSHCommand(), "vip");
            this.Register("stopsh", new StopSHCommand(), "vip");
            this.Register("opendimmer", new OpenDimmerCommand(), "vip");
            this.Register("odimmer", new OpenDimmerCommand(), "vip", true);
            this.Register("vipa", new VIPAlertCommand(), "vip");
            this.Register("va", new VIPAlertCommand(), "vip", true);
            this.Register("v", new VIPAlertCommand(), "vip", true);
            this.Register("vipalerta", new ToggleVIPAlertCommand(), "vip");
            this.Register("toggleva", new ToggleVIPAlertCommand(), "vip", true);
            this.Register("togglev", new ToggleVIPAlertCommand(), "vip", true);
            this.Register("moonwalk", new MoonwalkCommand(), "vip", true);
        }

        /// <summary>
        /// Ambassador set of commands
        /// </summary>
        private void RegisterAmbassadors()
        {
            this.Register("aa", new AmbassadorAlertCommand(), "ambassadorlog");
            this.Register("atrabajar", new AmbassadorOnDutyCommand(), "ambassadorlog");
            this.Register("aofftrabajar", new AmbassadorOffDutyCommand(), "ambassadorlog");
        }

        /// <summary>
        /// Trial Moderator set of commands
        /// </summary>
        private void RegisterTrialModerators()
        {
            this.Register("sa", new StaffAlertCommand(), "stafflog");
            this.Register("strabajar", new OnDutyCommand(), "stafflog");
            this.Register("offstrabajar", new OffDutyCommand(), "stafflog");
        }

        /// <summary>
        /// Moderator set of commands
        /// </summary>
        private void RegisterModerators()
        {
            this.Register("alert", new AlertCommand(), "stafflog");
            this.Register("ban", new BanCommand(), "stafflog");
            this.Register("mute", new MuteCommand(), "stafflog");
            this.Register("unmute", new UnmuteCommand(), "stafflog");
            this.Register("userinfo", new UserInfoCommand(), "stafflog");
            this.Register("update", new UpdateCommand(), "stafflog");
            this.Register("poll", new PollCommand(), "stafflog");
            this.Register("givespecial", new GiveSpecialReward(), "stafflog");
        }

        /// <summary>
        /// Senior Moderator set of commands
        /// </summary>
        private void RegisterSeniorModerators()
        {
            this.Register("ha", new HotelAlertCommand(), "stafflog");
            this.Register("wha", new WhisperHotelAlertCommand(), "stafflog");
            this.Register("nha", new NoticeHotelAlertCommand(), "stafflog");
            this.Register("ipban", new IPBanCommand(), "stafflog");
            this.Register("roomalert", new RoomAlertCommand(), "stafflog");
            this.Register("roommute", new RoomMuteCommand(), "stafflog");
            this.Register("roomunmute", new RoomUnmuteCommand(), "stafflog");
            this.Register("summon", new SummonCommand(), "stafflog");
            this.Register("follow", new FollowCommand(), "stafflog");
            this.Register("unload", new UnloadCommand(), "stafflog");
            this.Register("senduser", new SendUserCommand(), "stafflog");
            this.Register("dartrabajo", new SuperHireCommand(), "stafflog");
        }

        /// <summary>
        /// Administrator set of commands
        /// </summary>
        private void RegisterAdministrators()
        {
            this.Register("boveda", new VaultCommand(), "stafflog");
            this.Register("at", new AdminTaxiCommand(), "stafflog");
            this.Register("setz", new SetSHCommand(), "stafflog");
            this.Register("hal", new HALCommand(), "stafflog");
            this.Register("mip", new MIPCommand(), "stafflog");
            this.Register("rpstats", new RPStatsCommand(), "stafflog");
            this.Register("rpweapons", new RPWeaponsCommand(), "stafflog");
            this.Register("rpfarming", new RPFarmingStatsCommand(), "stafflog");
            this.Register("override", new OverrideCommand(), "stafflog");
            this.Register("teleport", new TeleportCommand(), "stafflog");
            this.Register("spull", new SuperPullCommand(), "stafflog");
            this.Register("spush", new SuperPushCommand(), "stafflog");
            this.Register("eventha", new EventAlertCommand(), "stafflog");
            this.Register("restore", new RestoreCommand(), "stafflog");
            this.Register("adminrelease", new AdminReleaseCommand(), "stafflog");
            this.Register("adminjail", new AdminJailCommand(), "stafflog");
            this.Register("roomrestore", new RoomRestoreCommand(), "stafflog");
            this.Register("roomrelease", new RoomReleaseCommand(), "stafflog");
            this.Register("roomheal", new RoomHealCommand(), "stafflog");
            this.Register("warptome", new WarpToMeCommand(), "stafflog");
            this.Register("warpmeto", new WarpMeToCommand(), "stafflog");
            this.Register("blacklist", new BlackListCommand(), "stafflog");
            this.Register("unblacklist", new UnBlackListCommand(), "stafflog");
            this.Register("coordbot", new BotRPCommand(), "stafflog");
        }

        /// <summary>
        /// Manager set of commands
        /// </summary>
        private void RegisterManagers()
        {

            this.Register("givebadge", new GiveBadgeCommand(), "stafflog");
            this.Register("roombadge", new RoomBadgeCommand(), "stafflog");
            this.Register("massbadge", new MassBadgeCommand(), "stafflog");
            this.Register("globalgive", new GlobalGiveCommand(), "stafflog");
            this.Register("freeze", new FreezeCommand(), "stafflog");
            this.Register("unfreeze", new UnFreezeCommand(), "stafflog");
            this.Register("flagother", new FlagOtherCommand(), "stafflog");
            this.Register("flag", new FlagOtherCommand(), "stafflog", true);
            this.Register("mimic", new MimicCommand(), "staff");
            this.Register("togglewhispers", new ToggleWhispersCommand(), "staff");
            this.Register("disconnect", new DisconnectCommand(), "stafflog");
            this.Register("dc", new DisconnectCommand(), "stafflog", true);
            this.Register("purge", new PurgeCommand(), "stafflog");
            this.Register("purga", new PurgeCommand(), "stafflog");
            this.Register("checklottery", new StopEventCommand(), "stafflog", true);
            this.Register("accountcheck", new AccountCheckCommand(), "stafflog");
            this.Register("checkaccount", new AccountCheckCommand(), "stafflog", true);
            this.Register("namecheck", new NameCheckCommand(), "stafflog");
            this.Register("checkname", new NameCheckCommand(), "stafflog", true);
            this.Register("summonstaff", new SummonStaffCommand(), "stafflog");
            this.Register("checkpoll", new CheckPollCommand(), "stafflog");
            this.Register("pollcheck", new CheckPollCommand(), "stafflog", true);
            this.Register("warpalltome", new WarpAllToMeCommand(), "stafflog");
            this.Register("sendroom", new SendRoomCommand(), "stafflog");
            this.Register("freezeroom", new FreezeRoomCommand(), "stafflog");
            this.Register("unfreezeroom", new UnFreezeRoomCommand(), "stafflog");
            //this.Register("wonline", new WOnlineCommand(), "stafflog");
            this.Register("makebota", new MakeBotActionCommand(), "stafflog");
            this.Register("quitarwhatsapp", new BanChatterCommand(), "stafflog");
            this.Register("darwhatsapp", new UnBanChatterCommand(), "stafflog");
            this.Register("deletechat", new DeleteChatCommand(), "stafflog");
            this.Register("tlock", new TLockCommand(), "stafflog");
        }

        /// <summary>
        /// Developer set of commands
        /// </summary>
        private void RegisterDevelopers()
        {
            this.Register("bubble", new BubbleCommand(), "staff");
            this.Register("handitem", new HandItemCommand(), "staff");
            this.Register("enable", new EnableCommand(), "staff");
            this.Register("coords", new CoordsCommand(), "staff");
            this.Register("setspeed", new SetSpeedCommand(), "stafflog");
            this.Register("startquestion", new StartQuestionCommand(), "staff");
            this.Register("kickbots", new KickBotsCommand(), "stafflog");
            this.Register("kickpets", new KickPetsCommand(), "stafflog");
            this.Register("disablediagonal", new DisableDiagonalCommand(), "stafflog");
            this.Register("room", new RoomCommand(), "stafflog");
            this.Register("bot", new BotCommand(), "stafflog");
            this.Register("activebots", new ActiveBotsCommand(), "stafflog");
            this.Register("fixweapons", new FixWeaponsCommand(), "stafflog");
            this.Register("whispertile", new SetWhisperTileCommand(), "stafflog");
            this.Register("page", new HtmlPageCommand(), "stafflog");
            this.Register("upage", new HtmlUPageCommand(), "stafflog");
            this.Register("uipage", new HtmlUIPageCommand(), "stafflog");
            this.Register("rpage", new HtmlRPageCommand(), "stafflog");
            this.Register("maintenance", new MaintenanceCommand(), "stafflog");
            this.Register("maint", new MaintenanceCommand(), "stafflog", true);

            this.Register("todo", new ToDoCommand());
            this.Register("todoadd", new ToDoCommand());
            this.Register("addtodo", new ToDoCommand(), "", true);
            this.Register("tda", new ToDoCommand(), "", true);
            this.Register("tododel", new ToDoCommand());
            this.Register("tododelete", new ToDoCommand(), "", true);
            this.Register("deltodo", new ToDoCommand(), "", true);
            this.Register("deletetodo", new ToDoCommand(), "", true);
            this.Register("tdd", new ToDoCommand(), "", true);

        }

        /// <summary>
        /// Owner set of commands
        /// </summary>
        private void RegisterOwners()
        {
            //this.Register("fastwalk", new FastwalkCommand(), "stafflog");
            this.Register("forcesit", new ForceSitCommand(), "stafflog");
            this.Register("forcelay", new ForceLayCommand(), "stafflog");
            this.Register("allaroundme", new AllAroundMeCommand(), "stafflog");
            this.Register("alleyesonme", new AllEyesOnMeCommand(), "stafflog");
            this.Register("massdance", new MassDanceCommand(), "stafflog");
            this.Register("massenable", new MassEnableCommand(), "stafflog");
            this.Register("summonall", new SummonAllCommand(), "stafflog");
            this.Register("releaseall", new ReleaseAllCommand(), "stafflog");
            this.Register("restoreall", new RestoreAllCommand(), "stafflog");
            this.Register("invisible", new InvisibleCommand(), "stafflog");
            this.Register("visible", new VisibleCommand(), "stafflog");
            this.Register("massact", new MassActionCommand(), "stafflog");
            this.Register("unidle", new UnIdleCommand(), "stafflog");
            this.Register("unban", new UnBanCommand(), "stafflog");
        }

        /// <summary>
        /// Special Right set of commands
        /// </summary>
        private void RegisterSpecialRights()
        {
            this.Register("makepet", new MakePetCommand(), "stafflog");
            this.Register("transformall", new TransformAllCommand(), "stafflog");
            this.Register("roomtransform", new RoomMakePetCommand(), "stafflog");
            this.Register("summonpets", new SummonPetsCommand(), "stafflog");
            this.Register("pet", new PetTransformCommand(), "stafflog");
            this.Register("colour", new ColourChangeCommand(), "stafflog");
            this.Register("color", new ColourChangeCommand(), "stafflog", true);
            this.Register("changeuclass", new ChangeUClassCommand(), "stafflog");
            //this.Register("sfastwalk", new SuperFastwalkCommand(), "stafflog");
            //this.Register("mpu", new MPUCommand(), "stafflog");
            this.Register("makesay", new MakeSayCommand(), "stafflog");
            this.Register("sayall", new SayAllCommand(), "stafflog");
            this.Register("coins", new GiveCoinsCommand(), "stafflog");
            this.Register("duckets", new GiveDucketsCommand(), "stafflog");
            this.Register("diamonds", new GiveDiamondsCommand(), "stafflog");
            this.Register("rcoins", new TakeCoinsCommand(), "stafflog");
            this.Register("rduckets", new TakeDucketsCommand(), "stafflog");
            this.Register("rdiamonds", new TakeDiamondsCommand(), "stafflog");
            this.Register("epoints", new GiveEventPointsCommand(), "stafflog");
            this.Register("rank", new GiveRankCommand(), "stafflog");
            this.Register("kill", new KillCommand(), "stafflog");
            this.Register("setstat", new SetStatCommand(), "stafflog");
            this.Register("sethp", new SetStatCommand(), "stafflog", true);
            this.Register("snap", new KillCommand(), "stafflog");
            this.Register("setenergy", new SetStatCommand(), "stafflog", true);
            this.Register("sethunger", new SetStatCommand(), "stafflog", true);
            this.Register("sethygiene", new SetStatCommand(), "stafflog", true);
            this.Register("givevip", new GiveVIPCommand(), "stafflog");
            this.Register("takevip", new TakeVIPCommand(), "stafflog");
            this.Register("banvip", new BanVIPCommand(), "stafflog");
            this.Register("unbanvip", new UnBanVIPCommand(), "stafflog");
        }
        #endregion

        /// <summary>
        /// Registers a Chat Command.
        /// </summary>
        /// <param name="CommandText">Text to type for this command.</param>
        /// <param name="Command">The command to execute.</param>
        public void Register(string CommandText, IChatCommand Command, string Type = "", bool IsAlias = false)
        {
            if (IsAlias && !this._aliases.Contains(CommandText))
                this._aliases.Add(CommandText);

            switch (Type.ToLower())
            {
                case "job":
                    {
                        this._commands.Add(CommandText, Command);
                        this._jobcommands.Add(CommandText, Command);
                        break;
                    }
                case "gang":
                    {
                        this._commands.Add(CommandText, Command);
                        this._gangcommands.Add(CommandText, Command);
                        break;
                    }
                case "vip":
                    {
                        this._commands.Add(CommandText, Command);
                        this._vipcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "staff":
                    {
                        this._commands.Add(CommandText, Command);
                        this._staffcommands.Add(CommandText, Command);
                        break;
                    }
                case "stafflog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._staffcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "ambassadorlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._ambassadorcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "userlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "joblog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._jobcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "eventlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._eventcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                default:
                    {
                        this._commands.Add(CommandText, Command);
                        break;
                    }
            }
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public static string GenerateRainbowText(string Name)
        {
            StringBuilder NewName = new StringBuilder();

            string[] Colours = { "FF0000", "FFA500", "FFFF00", "008000", "0000FF", "800080" };

            int Count = 0;
            int Count2 = 0;
            while (Count < Name.Length)
            {
                NewName.Append("<font color='#" + Colours[Count2] + "'>" + Name[Count] + "</font>");

                Count++;
                Count2++;

                if (Count2 >= 6)
                    Count2 = 0;
            }

            return NewName.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId, string Type)
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Type.ToLower() == "staff")
                    dbClient.SetQuery("INSERT INTO `command_logs_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "ambassador")
                    dbClient.SetQuery("INSERT INTO `command_logs_ambassador` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "job")
                    dbClient.SetQuery("INSERT INTO `command_logs_jobs` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "vip")
                    dbClient.SetQuery("INSERT INTO `command_logs_vip` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "event")
                    dbClient.SetQuery("INSERT INTO `command_logs_events` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else
                    dbClient.SetQuery("INSERT INTO `command_logs_users` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", PolarEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
        {
            return this._commands.TryGetValue(Command, out IChatCommand);
        }
    }
}