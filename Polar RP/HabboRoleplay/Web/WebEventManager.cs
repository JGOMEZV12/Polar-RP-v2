using Fleck;
using log4net;
using Newtonsoft.Json;
using Polar.Core;
using Polar.HabboHotel.Cache;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Roleplay.Web;
using Polar.HabboHotel.Roleplay.Web.Incoming.General;
using Polar.HabboHotel.Roleplay.Web.Incoming.Others;
using Polar.HabboHotel.Roleplay.Web.Outgoing;
using Polar.HabboHotel.Roleplay.Web.Outgoing.Default;
using Polar.HabboHotel.Roleplay.Web.Outgoing.Misc;
using Polar.HabboHotel.Roleplay.Web.Outgoing.Purse;
using Polar.HabboHotel.Roleplay.Web.OutGoing.Misc;
using Polar.HabboRoleplay.Web.Outgoing.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polar.HabboHotel.Roleplay.Web
{
    /// <summary>
    /// WebSocketUser class.
    /// </summary>
    public class WebSocketUser : IDisposable
    {
        public int Id { get; private set; }
        public string Username { get; set; }
        public bool Closing { get; set; }
        public IWebSocketConnection Connection { get; private set; }
        private bool _disposed;

        public WebSocketUser(int id, string username, IWebSocketConnection connection)
        {
            Id = id;
            Username = username ?? string.Empty;
            Closing = false;
            Connection = connection;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Id = 0;
            Username = null;
            Closing = true;
            Connection = null;
            _disposed = true;
        }
    }

    /// <summary>
    /// WebEventManager class.
    /// </summary>
    public sealed class WebEventManager : IDisposable
    {
        /// <summary>
        /// log4net.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Roleplayer.Web.WebEventManager");

        /// <summary>
        /// _webSocketServer.
        /// </summary>
        public WebSocketServer _webSocketServer;

        /// <summary>
        /// Concurrent dictionary containing websocket connections.
        /// </summary>
        public ConcurrentDictionary<IWebSocketConnection, WebSocketUser> _webSockets;

        /// <summary>
        /// Concurrent dictionary containing web events.
        /// </summary>
        private ConcurrentDictionary<string, IWebEvent> _webEvents;

        private string _sslProtocol;
        private string _sslCertificatePath;
        private string _sslCertificatePassword;
        private CancellationTokenSource _pingCancellationTokenSource;
        private bool _disposed;

        // Cache para búsquedas frecuentes
        private readonly ConcurrentDictionary<int, List<IWebSocketConnection>> _userIdToSocketsCache;

        public WebEventManager()
        {
            _sslProtocol = PolarEnvironment.GetConfig().data["ws.certificate.protocol"];
            _sslCertificatePath = PolarEnvironment.GetConfig().data["ws.certificate.pfx"];
            _sslCertificatePassword = PolarEnvironment.GetConfig().data["ws.certificate.password"];

            string ip = PolarEnvironment.GetConfig().data["ws.tcp.bindip"];
            int port = int.Parse(PolarEnvironment.GetConfig().data["ws.tcp.port"]);

            _webSocketServer = new WebSocketServer($"{_sslProtocol}://{ip}:{port}");
            _webSockets = new ConcurrentDictionary<IWebSocketConnection, WebSocketUser>();
            _webEvents = new ConcurrentDictionary<string, IWebEvent>();
            _userIdToSocketsCache = new ConcurrentDictionary<int, List<IWebSocketConnection>>();
            _pingCancellationTokenSource = new CancellationTokenSource();

            RegisterIncoming();
            RegisterOutgoing();
        }

        /// <summary>
        /// Initializes the websocket connection.
        /// </summary>
        public void Init()
        {
            var certPass = PolarEnvironment.GetConfig().data["ws.certificate.password"].Trim();
            if (_sslProtocol == "wss")
            {
                try
                {
                    X509Certificate2 cert = new X509Certificate2(_sslCertificatePath, _sslCertificatePassword);
                    _webSocketServer.Certificate = cert;
                    _webSocketServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                }
                catch (Exception ex)
                {
                    log.Error("Error al cargar el certificado SSL para WebSocket", ex);
                    return;
                }
            }

            _webSocketServer.ListenerSocket.NoDelay = true;
            _webSocketServer.RestartAfterListenError = true;

            this._webSocketServer.Start(ConnectingSocket =>
            {
                ConnectingSocket.OnOpen = () => {
                    OnSocketAdd(ConnectingSocket);
                };
                ConnectingSocket.OnClose = () => this.OnSocketRemove(ConnectingSocket);
                ConnectingSocket.OnMessage = SocketData => this.OnSocketMessage(ConnectingSocket, SocketData);
                ConnectingSocket.OnError = SocketError => this.OnSocketError(SocketError.Message, SocketError.ToString());
            });

            // Iniciar ping automático
            StartGlobalPingService();
        }

        /// <summary>
        /// Servicio global de ping para mantener conexiones activas
        /// </summary>
        private void StartGlobalPingService()
        {
            Task.Run(async () =>
            {
                while (!_pingCancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), _pingCancellationTokenSource.Token);

                        var socketsToRemove = new List<IWebSocketConnection>();

                        foreach (var socketEntry in _webSockets)
                        {
                            var socket = socketEntry.Key;
                            var user = socketEntry.Value;

                            try
                            {
                                if (socket.IsAvailable)
                                {
                                    socket.Send("ping");
                                }
                                else
                                {
                                    socketsToRemove.Add(socket);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex is IOException || ex is SocketException)
                                {
                                    socketsToRemove.Add(socket);
                                }
                                else
                                {
                                    log.Debug($"Error sending ping to user {user.Id}: {ex.Message}");
                                }
                            }
                        }

                        // Limpiar sockets muertos
                        foreach (var socket in socketsToRemove)
                        {
                            if (_webSockets.TryRemove(socket, out var removedUser))
                            {
                                RemoveFromCache(removedUser.Id, socket);
                                removedUser.Dispose();
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Shutdown normal
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in global ping service", ex);
                    }
                }
            }, _pingCancellationTokenSource.Token);
        }

        /// <summary>
        /// Registers the incoming web events.
        /// </summary>
        public void RegisterIncoming()
        {
            this._webEvents.TryAdd("event_retrieveconnectingstatistics", new RetrieveStatsWebEvent());
            this._webEvents.TryAdd("event_pong", new PongWebEvent());
            //this._webEvents.TryAdd("event_walk", new WalkWebEvent());
        }

        /// <summary>
        /// Registers the outgoing web events.
        /// </summary>
        public void RegisterOutgoing()
        {
            // Misc
            this._webEvents.TryAdd("event_psv", new PSVWebEvent());
            this._webEvents.TryAdd("event_sendjsalert", new SendNotificationWebEvent());
            this._webEvents.TryAdd("event_htmlpage", new HtmlPageWebEvent());
            this._webEvents.TryAdd("event_updateonlinecount", new OnlineCountWebEvent());
            this._webEvents.TryAdd("event_buscados", new WantedWebEvent());
            this._webEvents.TryAdd("event_manejar", new ManejarWebEvent());
            this._webEvents.TryAdd("event_bounty", new BountyWebEvent());
            this._webEvents.TryAdd("event_feedcomposer", new LiveFeedComposer());

            // Roleplay
            this._webEvents.TryAdd("event_vip", new VIPWebEvent());
            this._webEvents.TryAdd("event_changename", new ChangeNameWebEvent());
            this._webEvents.TryAdd("event_phone", new PhoneWebEvent());
            this._webEvents.TryAdd("event_characterbar", new RetrieveUStatsWebEvent());
            this._webEvents.TryAdd("event_charweapons", new RetrieveUWeapons());
            this._webEvents.TryAdd("event_atm", new ATMWebEvent());
            this._webEvents.TryAdd("event_restaurant", new FoodWebEvent());
            this._webEvents.TryAdd("event_house", new HousesWebEvent());
            this._webEvents.TryAdd("event_apart", new ApartmentsWebEvent());
            this._webEvents.TryAdd("event_purge", new PurgeWebEvent());
            this._webEvents.TryAdd("item", new ItemWebEvent());
            this._webEvents.TryAdd("event_item", new ItemWebEvent());
            this._webEvents.TryAdd("event_shop", new WeaponsWebEvent());
            this._webEvents.TryAdd("event_skins", new WSkinWebEvent());
            this._webEvents.TryAdd("event_wizard", new HechizosWebEvent());
            this._webEvents.TryAdd("event_products", new ProductsWebEvent());
            this._webEvents.TryAdd("event_initwsdialogues", new InitWSDialogues());
            this._webEvents.TryAdd("event_actions", new ActionWebEvent());
            this._webEvents.TryAdd("event_moves", new MoveWebEvent());
            this._webEvents.TryAdd("event_timerdialogue", new TimerDialogueWebEvent());
            this._webEvents.TryAdd("event_captcha", new CaptchaWebEvent());
            this._webEvents.TryAdd("event_business", new BusinessWebEvent());
            this._webEvents.TryAdd("event_commands", new CommandsWebEvent());
            this._webEvents.TryAdd("event_gang", new GangsWebEvent());
            this._webEvents.TryAdd("event_stats", new StatsWebEvent());
            this._webEvents.TryAdd("event_target", new TargetWebEvent());
            this._webEvents.TryAdd("event_group", new GroupsWebEvent());
            this._webEvents.TryAdd("event_vehicle", new VehiclesWebEvent());
            this._webEvents.TryAdd("event_camionero", new CamioneroWebEvent());
            this._webEvents.TryAdd("event_basurero", new BasureroWebEvent());
            this._webEvents.TryAdd("event_armero", new ArmeroWebEvent());
            this._webEvents.TryAdd("event_hospital", new HospitalWebEvent());
            this._webEvents.TryAdd("event_driving", new DrivingWebEvent());
            this._webEvents.TryAdd("event_tutorial", new TutorialWebEvent());
            this._webEvents.TryAdd("event_purse", new PurseWebEvent());
            this._webEvents.TryAdd("event_userprofile", new ProfileWebEvent());
            this._webEvents.TryAdd("event_mapa", new MapaWebEvent());
            this._webEvents.TryAdd("event_taxi", new TaxiWebEvent());
            this._webEvents.TryAdd("sendnote", new SendNoteVoice());
        }

        /// <summary>
        /// On socket data received
        /// </summary>
        /// <param name="InteractingSocket"></param>
        /// <param name="SentData"></param>
        private void OnSocketMessage(IWebSocketConnection InteractingSocket, string SentData)
        {
            try
            {
                if (string.IsNullOrEmpty(SentData))
                    return;

                // Manejar ping/pong rápidamente
                if (SentData == "ping" || SentData == "pong")
                    return;

                var ReceivedData = JsonConvert.DeserializeObject<WebEvent>(SentData);

                if (string.IsNullOrEmpty(ReceivedData.EventName))
                    return;

                GameClient InteractingClient = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(ReceivedData.UserId);

                if (InteractingClient == null || InteractingClient.LoggingOut)
                    return;

                if (InteractingClient.GetHabbo().TokenId != ReceivedData.Token)
                    return;

                if (!this._webSockets.ContainsKey(InteractingSocket))
                    return;

                if (_webEvents.TryGetValue(ReceivedData.EventName, out IWebEvent webEvent))
                {
                    webEvent.Execute(InteractingClient, ReceivedData.ExtraData, InteractingSocket);
                }
                else
                {
                    log.Debug("Unrecognized Web Event: '" + ReceivedData.EventName + "'");
                }
            }
            catch (JsonException)
            {
                log.Debug($"Invalid JSON received: {SentData?.Substring(0, Math.Min(100, SentData?.Length ?? 0))}");
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                    return;

                this.OnSocketError(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// OnSocketAdd.
        /// </summary>
        /// <param name="NewUser"></param>
        public void OnSocketAdd(IWebSocketConnection socket)
        {
            if (socket == null || !socket.IsAvailable || socket.ConnectionInfo == null ||
                string.IsNullOrEmpty(socket.ConnectionInfo.Path) ||
                !socket.ConnectionInfo.Path.Trim().Contains("/") ||
                string.IsNullOrEmpty(socket.ConnectionInfo.Path.Trim().Split('/')[1]))
                return;

            try
            {
                int userId = GetSocketsUserID(socket);
                if (userId <= 0)
                    return;

                // Verificar conexiones duplicadas usando cache
                var existingConnections = GetSocketsClient(socket);
                if (existingConnections.Any())
                {
                    DeactivateSocket(socket);
                    return;
                }

                // Remover si ya existe (por seguridad)
                if (_webSockets.ContainsKey(socket))
                    _webSockets.TryRemove(socket, out _);

                var webSocketUser = new WebSocketUser(userId, "", socket);
                if (_webSockets.TryAdd(socket, webSocketUser))
                {
                    AddToCache(userId, socket);
                    log.Info($"WebSocket connected: UserId={userId}, ConnectionId={socket.ConnectionInfo.Id}");
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                    return;

                this.OnSocketError(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// OnSocketRemove.
        /// </summary>
        /// <param name="User"></param>
        public void OnSocketRemove(IWebSocketConnection User)
        {
            if (User == null || !_webSockets.ContainsKey(User))
                return;

            try
            {
                int userId = GetSocketsUserID(User);
                this.CloseSimilarSockets(userId);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                    return;

                this.OnSocketError(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// Closes any similar sockets with the target user ID
        /// </summary>
        /// <param name="Id"></param>
        public void CloseSimilarSockets(int Id)
        {
            if (Id <= 0) return;

            List<IWebSocketConnection> SocketsToClose = GetSimilarSockets(Id);

            foreach (IWebSocketConnection Socket in SocketsToClose)
            {
                this.DeactivateSocket(Socket);
            }
        }

        /// <summary>
        /// Obtiene sockets similares usando cache
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<IWebSocketConnection> GetSimilarSockets(int Id)
        {
            if (_userIdToSocketsCache.TryGetValue(Id, out var cachedSockets))
                return cachedSockets.ToList();

            List<IWebSocketConnection> SimilarSockets = new List<IWebSocketConnection>();

            foreach (KeyValuePair<IWebSocketConnection, WebSocketUser> AvailableSockets in this._webSockets)
            {
                if (AvailableSockets.Value == null || AvailableSockets.Key == null)
                    continue;

                if (AvailableSockets.Value.Id == Id)
                    SimilarSockets.Add(AvailableSockets.Key);
            }

            return SimilarSockets;
        }

        /// <summary>
        /// OnSocketError.
        /// </summary>
        /// <param name="Error"></param>
        public void OnSocketError(string Error, string Exception)
        {
            Logging.LogWebSocketError(Error, Exception);
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="EventName"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool ExecuteWebEvent(GameClient client, string eventName, string receivedData)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;

            var socket = client?.GetRoleplay()?.WebSocketConnection;

            if (!SocketReady(socket))
                return false;

            try
            {
                if (_webEvents.TryGetValue(eventName, out IWebEvent webEvent) &&
                    !(_webSockets[socket].Closing || !socket.IsAvailable))
                {
                    webEvent.Execute(client, receivedData, socket);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                    return false;

                OnSocketError(ex.Message, ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Broadcasts the web event.
        /// </summary>
        /// <param name="EventName"></param>
        /// <param name="Data"></param>
        public void BroadCastWebEvent(string eventName, string extraData)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            try
            {
                if (!_webEvents.TryGetValue(eventName, out var webEvent))
                {
                    log.Warn($"Trying to broadcast unknown event: {eventName}");
                    return;
                }

                // Usar ToList para evitar modificaciones durante la iteración
                foreach (var socket in _webSockets.Keys.ToList())
                {
                    try
                    {
                        if (socket.IsAvailable && !_webSockets[socket].Closing)
                        {
                            webEvent.Execute(null, extraData, socket);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is SocketException)
                            continue;

                        log.Debug($"Error broadcasting to socket: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnSocketError(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// Sends the given user web data.
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Data"></param>
        public void SendDataDirect(GameClient User, string Data)
        {
            if (!this.SocketReady(User, true))
                return;

            var socket = User.GetRoleplay()?.WebSocketConnection;
            if (!this.SocketReady(socket))
                return;

            try
            {
                socket.Send(Data);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                    return;

                this.OnSocketError(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// Versión optimizada de SendDataDirect con validación mínima
        /// </summary>
        public bool SendDataDirectFast(IWebSocketConnection socket, string data)
        {
            if (socket == null || !socket.IsAvailable || string.IsNullOrEmpty(data))
                return false;

            try
            {
                socket.Send(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Broadcasts the web data.
        /// </summary>
        /// <param name="Data"></param>
        public void BroadCastWebData(string Data)
        {
            if (string.IsNullOrEmpty(Data))
                return;

            // Usar lista de claves para evitar modificaciones durante iteración
            var sockets = _webSockets.Keys.ToList();

            foreach (var socket in sockets)
            {
                try
                {
                    if (SocketReady(socket))
                    {
                        socket.Send(Data);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is SocketException)
                        continue;

                    this.OnSocketError(ex.Message, ex.ToString());
                }
            }
        }

        /// <summary>
        /// SocketReady.
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public bool SocketReady(IWebSocketConnection Socket)
        {
            if (Socket == null)
                return false;

            if (!this._webSockets.ContainsKey(Socket))
                return false;

            if (this._webSockets[Socket].Closing)
                return false;

            if (!Socket.IsAvailable)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the socket is ready to be interacted with
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public bool SocketReady(GameClient User, bool Logout = false)
        {
            if (User == null || User.GetHabbo() == null || User.GetRoleplay() == null)
                return false;

            if (Logout && User.LoggingOut)
                return false;

            return SocketReady(User.GetRoleplay().WebSocketConnection);
        }

        /// <summary>
        /// Gets the user connection.
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public IWebSocketConnection GetUsersConnection(GameClient User)
        {
            if (User == null || User.GetHabbo() == null || User.LoggingOut)
                return null;

            // Usar cache para búsqueda rápida
            if (_userIdToSocketsCache.TryGetValue(User.GetHabbo().Id, out var sockets))
            {
                foreach (var socket in sockets)
                {
                    if (SocketReady(socket))
                        return socket;
                }
            }

            // Fallback a búsqueda tradicional
            return _webSockets
                .Where(kv => kv.Value.Id == User.GetHabbo().Id && SocketReady(kv.Key))
                .Select(kv => kv.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the sockets userID from its ConnectionInformation
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public int GetSocketsUserID(IWebSocketConnection Socket)
        {
            if (Socket?.ConnectionInfo?.Path == null)
                return 0;

            string path = Socket.ConnectionInfo.Path.Trim();
            if (string.IsNullOrEmpty(path))
                return 0;

            var parts = path.Split('/');
            if (parts.Length < 2)
                return 0;

            if (int.TryParse(parts[1], out int userId))
                return userId;

            return 0;
        }

        /// <summary>
        /// Gets the client assosciated with the targeted socket
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public List<GameClient> GetSocketsClient(IWebSocketConnection Socket)
        {
            int SocketsUserId = this.GetSocketsUserID(Socket);
            if (SocketsUserId <= 0)
                return new List<GameClient>();

            var game = PolarEnvironment.GetGame();
            if (game == null || game.GetClientManager() == null)
                return new List<GameClient>();

            return game.GetClientManager().GetClients
                .Where(Client => Client != null &&
                       !Client.LoggingOut &&
                       Client.GetHabbo() != null &&
                       Client.GetHabbo().Id == SocketsUserId &&
                       this.SocketReady(Client))
                .ToList();
        }

        /// <summary>
        /// Gets _websockets dictionary
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<IWebSocketConnection, WebSocketUser> GetConnectedUsers()
        {
            return this._webSockets;
        }

        /// <summary>
        /// Completely shuts down a targeted Socket
        /// </summary>
        /// <param name="Socket"></param>
        public void DeactivateSocket(IWebSocketConnection Socket)
        {
            if (Socket == null)
                return;

            if (this._webSockets.TryGetValue(Socket, out WebSocketUser user))
            {
                int userId = user.Id;
                user.Closing = true;
                user.Dispose();
                this._webSockets.TryRemove(Socket, out _);
                RemoveFromCache(userId, Socket);
            }

            try
            {
                Socket.Close();
            }
            catch
            {
                // Ignorar errores al cerrar
            }
        }

        /// <summary>
        /// Closes any sockets assosciated with the character/userID 
        /// </summary>
        /// <param name="SocketUserID"></param>
        public void CloseSocketByGameClient(int SocketUserID)
        {
            if (SocketUserID <= 0)
                return;

            try
            {
                this.CloseSimilarSockets(SocketUserID);
            }
            catch (Exception e)
            {
                log.Debug($"Error closing socket for user {SocketUserID}: {e.Message}");
            }
        }

        #region Cache Management

        private void AddToCache(int userId, IWebSocketConnection socket)
        {
            _userIdToSocketsCache.AddOrUpdate(userId,
                new List<IWebSocketConnection> { socket },
                (key, existingList) =>
                {
                    if (!existingList.Contains(socket))
                        existingList.Add(socket);
                    return existingList;
                });
        }

        private void RemoveFromCache(int userId, IWebSocketConnection socket)
        {
            if (_userIdToSocketsCache.TryGetValue(userId, out var sockets))
            {
                sockets.Remove(socket);
                if (sockets.Count == 0)
                {
                    _userIdToSocketsCache.TryRemove(userId, out _);
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Cancelar ping service
            _pingCancellationTokenSource?.Cancel();
            _pingCancellationTokenSource?.Dispose();

            // Cerrar todas las conexiones
            foreach (var socket in _webSockets.Keys.ToList())
            {
                DeactivateSocket(socket);
            }

            _webSockets.Clear();
            _userIdToSocketsCache.Clear();
            _webEvents.Clear();

            _webSocketServer?.Dispose();

            log.Info("WebEventManager disposed successfully");
        }

        #endregion
    }
}