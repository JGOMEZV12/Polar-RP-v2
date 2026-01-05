using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Polar.Core;
using Polar.HabboHotel.GameClients;
using System.Collections.Concurrent;
using Polar.Database.Interfaces;
using log4net;
using System.Threading;
using Polar.HabboRoleplay.Turfs;
using Polar.HabboRoleplay.Houses;
using Polar.HabboHotel.Items;
using Polar.HabboRoleplay.Farming;
using Polar.HabboRoleplay.Gambling;
using Polar.HabboRoleplay.Bots.Manager;
using System.Diagnostics;

namespace Polar.HabboHotel.Rooms
{
    public class RoomManager
    {
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Rooms.RoomManager");

        private Dictionary<string, RoomModel> _roomModels;
        private readonly Object _roomLoadingSync;
        public ConcurrentDictionary<int, Room> _rooms;
        private ConcurrentDictionary<int, RoomData> _loadedRoomData;
        private Dictionary<int, Room> loadedRooms = new Dictionary<int, Room>();

        private DateTime _purgeLastExecution;


        public RoomManager()
        {
            this._roomLoadingSync = new Object();
            this._roomModels = new Dictionary<string, RoomModel>();

            this._rooms = new ConcurrentDictionary<int, Room>();
            this._loadedRoomData = new ConcurrentDictionary<int, RoomData>();

            this._purgeLastExecution = DateTime.Now.AddHours(3);

            //log.Info("Room Manager -> LOADED");
        }

        public int LoadedRoomDataCount
        {
            get { return this._loadedRoomData.Count; }
        }

        public int Count
        {
            get { return this._rooms.Count; }
        }

        public void PreLoadRooms()
        {
            lock (this._roomLoadingSync)
            {
                DataTable Row = null;
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `rooms`");
                    Row = dbClient.getTable();

                    foreach (DataRow R in Row.Rows)
                    {
                        GenerateRoomData(Convert.ToInt32(R["id"]));
                    }
                }
            }
        }

        public void LoadModel(string Id)
        {
            DataRow Row = null;
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = '" + Id + "' LIMIT 1");
                Row = dbClient.getRow();

                if (Row == null)
                    return;

                string Modelname = Convert.ToString(Row["id"]);
                if (!this._roomModels.ContainsKey(Id))
                {
                    this._roomModels.Add(Modelname, new RoomModel(Id, Convert.ToInt32(Row["door_x"]), Convert.ToInt32(Row["door_y"]), Convert.ToDouble(Row["door_z"]), Convert.ToInt32(Row["door_dir"]),
                      Convert.ToString(Row["heightmap"]), Convert.ToInt32(Row["wall_height"]), Convert.ToString(Row["poolmap"])));
                }
            }
        }

        public void LoadModels()
        {
            this._roomModels.Clear();
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,wall_height,poolmap FROM room_models");
                DataTable table = dbClient.getTable();
                if (table == null)
                    return;
                foreach (DataRow dataRow in table.Rows)
                {
                    string str = (string)dataRow["id"];
                    this._roomModels.Add(str,
                        new RoomModel(str, (int)dataRow["door_x"], (int)dataRow["door_y"], (double)dataRow["door_z"],
                            (int)dataRow["door_dir"], (string)dataRow["heightmap"], Convert.ToInt32(dataRow["wall_height"]), Convert.ToString(dataRow["poolmap"])));
                }
            }
        }

        public void ReloadModel(string Id)
        {
            if (!this._roomModels.ContainsKey(Id))
            {
                this.LoadModel(Id);
                return;
            }

            this._roomModels.Remove(Id);
            this.LoadModel(Id);
        }
        
        public bool TryGetModel(string Id, out RoomModel Model)
        {
            return this._roomModels.TryGetValue(Id, out Model);
        }

        public Room GetRoom(int roomID)
        {
            Room room;
            if (this._rooms.TryGetValue(roomID, out room))
            {
                return room;
            }
            return null;
        }

        public async Task UnloadRoom(Room Room, bool RemoveData = false)
        {
            if (Room == null)
                return;

            /*Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Se ha vaciado la [" + Room.RoomId + "] " + Room.RoomData.Name + "");*/

            #region Roleplay Checks


            #region Texas Hold Em
            List<TexasHoldEm> Games = TexasHoldEmManager.GetGamesByRoomId(Room.Id);
            if (Games.Count > 0)
            {
                foreach (TexasHoldEm Game in Games)
                {
                    if (Game != null)
                    {
                        #region PotSquare Check
                        Game.PotSquare.Furni = null;
                        #endregion

                        #region JoinGate Check
                        Game.JoinGate.Furni = null;
                        #endregion

                        #region Player1 Check
                        foreach (TexasHoldEmItem Item in Game.Player1.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Player2 Check
                        foreach (TexasHoldEmItem Item in Game.Player2.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Player3 Check
                        foreach (TexasHoldEmItem Item in Game.Player3.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Banker Check
                        foreach (TexasHoldEmItem Item in Game.Banker.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region Farming
            List<FarmingSpace> FarmingSpaces = FarmingManager.GetFarmingSpacesByRoomId(Room.Id);
            if (FarmingSpaces.Count > 0)
            {
                foreach (FarmingSpace Space in FarmingSpaces)
                {
                    if (Space != null)
                    {
                        Space.Item = null;
                        Space.Spawned = false;
                    }
                }
            }
            #endregion

            #region Houses
            /*List<House> Houses = PolarEnvironment.GetGame().GetHouseManager().GetHousesBySignRoomId(Room.Id);
            if (Houses.Count > 0)
            {
                foreach (House House in Houses)
                {
                    if (House.Sign != null)
                    {
                        House.Sign.Item = null;
                        House.Sign.Spawned = false;
                    }
                }
            }*/
            #endregion

            #region Bots
            //RoleplayBotManager.DeployCachedBots();
            #endregion

            #endregion

           
                Room room = null;
                if (this._rooms.TryRemove(Room.RoomId, out room))
                {
                    await Room.DisposeAsync();

                    if (RemoveData)
                    {
                        RoomData Data = null;
                        this._loadedRoomData.TryRemove(Room.Id, out Data);
                    }
                }
           
        }
        public List<RoomData> SearchGroupRooms(string Query)
        {
            IEnumerable<RoomData> InstanceMatches =
                (from RoomInstance in this._loadedRoomData
                 where /*RoomInstance.Value.UsersNow >= 0 &&*/
                 RoomInstance.Value.State != 3 &&
                 RoomInstance.Value.Group != null &&
                 (RoomInstance.Value.OwnerName.StartsWith(Query) ||
                 RoomInstance.Value.Tags.Contains(Query) ||
                 RoomInstance.Value.Name.Contains(Query))
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> SearchTaggedRooms(string Query)
        {
            IEnumerable<RoomData> InstanceMatches =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.State != 3 &&
                 (RoomInstance.Value.Tags.Contains(Query))
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> GetPopularRooms(int category, int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow > 0 &&
                 (category == -1 || RoomInstance.Value.Category == category) &&
                 RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.Score descending
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRecommendedRooms(int Amount = 50, int CurrentRoomId = 0)
        {
            IEnumerable<RoomData> Rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.Score >= 0 &&
                 RoomInstance.Value.State != 3 &&
                 RoomInstance.Value.Id != CurrentRoomId
                 orderby RoomInstance.Value.Score descending
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return Rooms.ToList();
        }

        public List<RoomData> GetPopularRatedRooms(int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRoomsByCategory(int Category, int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.Category == Category &&
                 RoomInstance.Value.UsersNow > 0 &&
                 RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetOnGoingRoomPromotions(int Mode, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;

            if (Mode == 17)
            {
                Rooms =
                    (from RoomInstance in this._loadedRoomData
                     where (RoomInstance.Value.HasActivePromotion) &&
                     RoomInstance.Value.State != 3
                     orderby RoomInstance.Value.Promotion.TimestampStarted descending
                     select RoomInstance.Value).Take(Amount);
            }
            else
            {
                Rooms =
                    (from RoomInstance in this._loadedRoomData
                     where (RoomInstance.Value.HasActivePromotion) &&
                     RoomInstance.Value.State != 3
                     orderby RoomInstance.Value.UsersNow descending
                     select RoomInstance.Value).Take(Amount);
            }

            return Rooms.ToList();
        }


        public List<RoomData> GetPromotedRooms(int CategoryId, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;

            Rooms =
                (from RoomInstance in this._loadedRoomData
                 where (RoomInstance.Value.HasActivePromotion) &&
                 RoomInstance.Value.Promotion.CategoryId == CategoryId &&
                 RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.Promotion.TimestampStarted descending
                 select RoomInstance.Value).Take(Amount);

            return Rooms.ToList();
        }

        public List<KeyValuePair<string, int>> GetPopularRoomTags()
        {
            IEnumerable<List<string>> Tags =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.UsersNow descending
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value.Tags).Take(50);

            Dictionary<string, int> TagValues = new Dictionary<string, int>();

            foreach (List<string> TagList in Tags)
            {
                foreach (string Tag in TagList)
                {
                    if (!TagValues.ContainsKey(Tag))
                    {
                        TagValues.Add(Tag, 1);
                    }
                    else
                    {
                        TagValues[Tag]++;
                    }
                }
            }

            List<KeyValuePair<string, int>> SortedTags = new List<KeyValuePair<string, int>>(TagValues);
            SortedTags.Sort((FirstPair, NextPair) =>
            {
                return FirstPair.Value.CompareTo(NextPair.Value);
            });

            SortedTags.Reverse();
            return SortedTags;
        }

        public List<RoomData> GetGroupRooms(int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.Group != null &&
                 RoomInstance.Value.State != 3
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public Room TryGetRandomLoadedRoom()
        {
            IEnumerable<Room> room =
                (from RoomInstance in this._rooms
                where (RoomInstance.Value.RoomData.UsersNow > 0 &&
                RoomInstance.Value.RoomData.State == 0 &&
                RoomInstance.Value.RoomData.UsersNow < RoomInstance.Value.RoomData.UsersMax)
                orderby RoomInstance.Value.RoomData.UsersNow descending
                select RoomInstance.Value).Take(1);

            if (room.Count() > 0)
                return room.First();
            else
                return null;
        }

        private static RoomModel GetCustomData(int roomID)
        {
            DataRow row;
            using (IQueryAdapter queryreactor = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor.SetQuery(
                    "SELECT door_x,door_y,door_z,door_dir,heightmap, wall_height FROM room_models_customs WHERE room_id = " +
                    roomID);
                row = queryreactor.getRow();
            }

            if (row == null)
                throw new Exception("El room model de la sala " + roomID + " no ha sido encontrado.");
            return new RoomModel(roomID.ToString(), (int)row["door_x"], (int)row["door_y"], (double)row["door_z"],
                (int)row["door_dir"], (string)row["heightmap"], (int)row["wall_height"], string.Empty);

             }

        public RoomModel GetModel(string Model, int RoomID)
        {
            if (Model == "model_custom")
                return GetCustomData(RoomID);
            if (this._roomModels.ContainsKey(Model))
                return (RoomModel)this._roomModels[Model];
            else
                return (RoomModel)null;
        }

        public void UpdateRoom(Room Room)
        {
            if (_loadedRoomData.ContainsKey(Room.Id))
                _loadedRoomData.TryUpdate(Room.Id, Room.RoomData, _loadedRoomData[Room.Id]);

            if (_rooms.ContainsKey(Room.Id))
                _rooms.TryUpdate(Room.Id, Room, _rooms[Room.Id]);
        }

        public RoomData GenerateRoomData(int RoomId)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
                return (RoomData)_loadedRoomData[RoomId];

            RoomData Data = new RoomData();

            Room Room;

            if (TryGetRoom(RoomId, out Room))
                return Room.RoomData;

            DataRow Row = null;
            DataRow RPRow = null;
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rooms` WHERE `id` = " + RoomId + " LIMIT 1");
                Row = dbClient.getRow();

                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `id` = " + RoomId + " LIMIT 1");
                RPRow = dbClient.getRow();
            }

            if (Row == null || RPRow == null)
                return null;

            Data.Fill(Row);
            Data.FillRP(RPRow);

            if (!_loadedRoomData.ContainsKey(RoomId))
                _loadedRoomData.TryAdd(RoomId, Data);

            return Data;
        }

        public bool TryGetRoomData(int roomId, out RoomData data)
        {
            data = null;
            if (_loadedRoomData.TryGetValue(roomId, out data))
                return true;

            return false;
        }

        public RoomData FetchRoomData(int RoomId, DataRow dRow, DataRow dRowRP)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
                return (RoomData)_loadedRoomData[RoomId];
            else
            {
                RoomData data = new RoomData();

                data.Fill(dRow);
                data.FillRP(dRowRP);

                if (!_loadedRoomData.ContainsKey(RoomId))
                    _loadedRoomData.TryAdd(RoomId, data);
                return data;
            }
        }

        public bool LoadRoom(int Id, out Room Room)
        {
            Room = null;

            if (TryGetRoom(Id, out Room))
                return true;

            if (!_rooms.ContainsKey(Id))
            {
                RoomData Data = null;
                if (TryGetRoomData(Id, out Data))
                {
                    if (Data != null)
                    {
                        Room = new Room(Data);
                        return _rooms.TryAdd(Room.RoomId, Room);
                    }
                }

                var roomData = LoadRoomData(Id);
                if (roomData == null)
                    return false;

                Room = new Room(roomData);
                return _rooms.TryAdd(Room.RoomId, Room);
            }

            return false;
        }

        public Room LoadRoom(int Id, bool BotCheck)
        {
            Room Room = null;

            if (TryGetRoom(Id, out Room))
            {
                return Room;
            }

            RoomData Data = GenerateRoomData(Id);
            if (Data == null)
                return null;

            Room = new Room(Data);

            if (!_rooms.ContainsKey(Room.RoomId))
            {
                _rooms.TryAdd(Room.RoomId, Room);
                if (BotCheck == true)
                {
                    Task.Run(async delegate
                    {
                        await Task.Delay(2000);
                        RoleplayBotManager.DeployCachedBots(Room);
                    });
                }
            }

            return Room;
        }

        public RoomData LoadRoomData(int roomId)
        {
            if (_loadedRoomData.ContainsKey(roomId))
                return _loadedRoomData[roomId];

            var data = new RoomData();
            DataTable table;
            DataRow row;
            DataRow rprow;

            using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT r.*, u.username AS owner_name FROM rooms r JOIN users u ON r.owner = u.id WHERE r.id = @id LIMIT 1");
                dbClient.AddParameter("id", roomId);
                table = dbClient.getTable();
                if (table == null || table.Rows.Count == 0)
                    return null;
                row = table.Rows[0];
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", roomId);
                rprow = dbClient.getRow();
            }

            if (row == null || rprow == null)
                return null;

            data.Fill(row, (string)row["owner_name"]);
            data.FillRP(rprow);

            if (!_loadedRoomData.ContainsKey(roomId))
                _loadedRoomData.TryAdd(roomId, data);

            return data;
        }

        public bool TryGetRoom(int RoomId, out Room Room)
        {
            return this._rooms.TryGetValue(RoomId, out Room);
        }

        public RoomData CreateRoom(GameClient Session, string Name, string Description, string Model, int Category, string City, int MaxVisitors, int TradeSettings)
        {
            if (!_roomModels.ContainsKey(Model))
            {
                Session.SendNotification(PolarEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_model_missing"));
                return null;
            }

            if (Name.Length < 3)
            {
                Session.SendNotification(PolarEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_name_length_short"));
                return null;
            }

            int RoomId = 0;

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `rooms` (`roomtype`,`caption`,`description`,`owner`,`model_name`,`category`,`users_max`,`trade_settings`, `username`) VALUES ('private',@caption,@description,@UserId,@model,@category,@usersmax,@tradesettings,@UserName)");
                dbClient.AddParameter("caption", Name);
                dbClient.AddParameter("description", Description);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.AddParameter("model", Model);
                dbClient.AddParameter("category", Category);
                dbClient.AddParameter("usersmax", MaxVisitors);
                dbClient.AddParameter("tradesettings", TradeSettings);
				dbClient.AddParameter("UserName", Session.GetHabbo().Username);

                RoomId = Convert.ToInt32(dbClient.InsertQuery());
                dbClient.RunQuery("INSERT INTO `rp_rooms` (`id`,`city`, `safezone_enabled`) VALUES ('" + RoomId + "', '" + City + "', '1')");
            }

            RoomData newRoomData = GenerateRoomData(RoomId);
            Session.GetHabbo().UsersRooms.Add(newRoomData);
            return newRoomData;
        }

        public ICollection<Room> GetRooms()
        {
            return this._rooms.Values;
        }

        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach (var room in this._rooms.Values.ToList())
            {
                if (room == null)
                    continue;

                tasks.Add(PolarEnvironment.GetGame().GetRoomManager().UnloadRoom(room));
            }

            await Task.WhenAll(tasks);

            Out.WriteLine("¡He terminado de deshacerse de las habitaciones!", "Polar.HabboHotel", ConsoleColor.DarkGray);
        }
    }
}