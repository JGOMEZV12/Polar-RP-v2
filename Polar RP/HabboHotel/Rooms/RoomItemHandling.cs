using System;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using Polar.Utilities;
using Polar.Core;

using Polar.HabboHotel.Items;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Pathfinding;

using Polar.HabboHotel.Items.Wired;
using Polar.HabboHotel.Items.Data.Toner;
using Polar.HabboHotel.Items.Data.RentableSpace;
using Polar.HabboHotel.Items.Data.Moodlight;

using Polar.Communication.Packets.Outgoing.Rooms.Engine;

using Polar.Communication.Packets.Outgoing.Inventory.Furni;
using Polar.Communication.Packets.Outgoing;
using Polar.Database.Interfaces;
using Polar.Communication.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace Polar.HabboHotel.Rooms
{
    public class RoomItemHandling
    {
        private Room _room;

        public int HopperCount;
        public int JukeboxCount;
        private bool mGotRollers;
        private int mRollerSpeed;
        private int mRollerCycle;

        private ConcurrentDictionary<int, Item> _movedItems;

        private ConcurrentDictionary<int, Item> _rollers;
        private ConcurrentDictionary<int, Item> _wallItems = null;
        public ConcurrentDictionary<int, Item> _floorItems = null;

        private readonly List<int> rollerItemsMoved;
        private readonly List<int> rollerUsersMoved;
        private readonly List<ServerPacket> rollerMessages;

        private ConcurrentQueue<Item> _roomItemUpdateQueue;
        public bool usedwiredscorebord;

        public RoomItemHandling(Room Room)
        {
            this._room = Room;

            this.HopperCount = 0;
            this.JukeboxCount = 0;
            this.mGotRollers = false;
            this.mRollerSpeed = 4;
            this.mRollerCycle = 0;

            this._movedItems = new();

            this._rollers = new();
            this._wallItems = new();
            this._floorItems = new();

            this.rollerItemsMoved = new();
            this.rollerUsersMoved = new();
            this.rollerMessages = new();

            this._roomItemUpdateQueue = new();
            this.usedwiredscorebord = false;
        }

        public void TryAddRoller(int ItemId, Item Roller)
        {
            this._rollers.TryAdd(ItemId, Roller);
        }

        public bool GotRollers
        {
            get { return mGotRollers; }
            set { mGotRollers = value; }
        }

        public void QueueRoomItemUpdate(Item item)
        {
            _roomItemUpdateQueue.Enqueue(item);
        }

        public void SetSpeed(int p)
        {
            mRollerSpeed = p;
        }

        public void UpdateWiredScoreBord()
        {
            List<ServerPacket> messages = new List<ServerPacket>();
            foreach (Item scoreitem in this._floorItems.Values)
            {
                if (scoreitem.GetBaseItem().InteractionType == InteractionType.WIRED_HIGHSCORE)
                {
                    var Message = new ObjectUpdateComposer(scoreitem, _room.OwnerId);
                    messages.Add(Message);
                }
            }
            /*
            KeyValuePair<int, string> data;
            DateTime now = DateTime.Now;
            int getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
            int getmonthtoday = Convert.ToInt32(now.ToString("MM"));
            int getweektoday = CultureInfo.GetCultureInfo("Nl-nl").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var itemx = GetFirstHighscore();

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                    this.ScorebordChangeCheck();
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord`  WHERE roomid = ", _room.RoomId, " "));
                    lock (_room.WiredScoreBordDay)
                    {
                        if (itemx.GetBaseItem().ItemName.ToLower() == "highscore_classic*2" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_mostwin*2" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_perteamn*2")
                        {
                            foreach (int mdayuserids in _room.WiredScoreBordDay.Keys)
                            {
                                if (_room.WiredScoreBordDay.ContainsKey(mdayuserids))
                                {
                                    data = _room.WiredScoreBordDay[mdayuserids];
    
                                    dbClient.SetQuery("INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`, `item_id`) VALUES ('" + _room.RoomId + "', '" + mdayuserids + "', @dusername" + mdayuserids + ", '" + data.Key + "', 'day', '" + getdaytoday + "', '" + itemx.Id + "')");
                                    dbClient.AddParameter(string.Concat("dusername", mdayuserids), data.Value);
                                    dbClient.RunQuery();
                                }
                            }
                        }
                    }

                    lock (_room.WiredScoreBordMonth)
                    {
                        if (itemx.GetBaseItem().ItemName.ToLower() == "highscore_classic*4" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_mostwin*4" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_perteamn*4")
                        {
                            foreach (int mmonthuserids in _room.WiredScoreBordMonth.Keys)
                            {

                                if (_room.WiredScoreBordMonth.ContainsKey(mmonthuserids))
                                {
                                    data = _room.WiredScoreBordMonth[mmonthuserids];

                                    dbClient.SetQuery("INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`, `item_id`) VALUES ('" + _room.RoomId + "', '" + mmonthuserids + "', @musername" + mmonthuserids + ", '" + data.Key + "', 'month', '" + getmonthtoday + "', '" + itemx.Id + "')");
                                    dbClient.AddParameter(string.Concat("musername", mmonthuserids), data.Value);
                                    dbClient.RunQuery();
                                }
                            }
                        }
                    }


                    lock (_room.WiredScoreBordWeek)
                    {
                        if (itemx.GetBaseItem().ItemName.ToLower() == "highscore_classic*3" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_mostwin*3" || itemx.GetBaseItem().ItemName.ToLower() == "highscore_perteamn*3")
                        {
                            foreach (int weekuserids in _room.WiredScoreBordWeek.Keys)
                            {
                                if (_room.WiredScoreBordWeek.ContainsKey(weekuserids))
                                {
                                    data = _room.WiredScoreBordWeek[weekuserids];
                                    dbClient.SetQuery("INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`, `item_id`) VALUES ('" + _room.RoomId + "', '" + weekuserids + "', @musername" + weekuserids + ", '" + data.Key + "', 'week', '" + getweektoday + "', '" + itemx.Id + "')");
                                    dbClient.AddParameter(string.Concat("musername", weekuserids), data.Value);
                                    dbClient.RunQuery();
                                }
                            }
                        }
                    }
                }*/
            _room.SendMessage(messages);
        }

        internal void ScorebordChangeCheck()
        {
            if (_room.WiredScoreFirstBordInformation.Count == 3)
            {
                DateTime now = DateTime.Now;
                int getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                int getmonthtoday = Convert.ToInt32(DateTime.Now.ToString("MM"));
                int getweektoday = CultureInfo.GetCultureInfo("Nl-nl").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                List<bool> SuperCheck = new List<bool>()
                {
                    getdaytoday != _room.WiredScoreFirstBordInformation[0],
                    getmonthtoday != _room.WiredScoreFirstBordInformation[1],
                    getweektoday != _room.WiredScoreFirstBordInformation[2]
                };

                _room.WiredScoreFirstBordInformation[0] = getdaytoday;
                _room.WiredScoreFirstBordInformation[1] = getmonthtoday;
                _room.WiredScoreFirstBordInformation[2] = getweektoday;

                if (SuperCheck[0])
                {
                    _room.WiredScoreBordDay.Clear();
                }

                if (SuperCheck[1])
                {
                    _room.WiredScoreBordMonth.Clear();
                }

                if (SuperCheck[2])
                {
                    _room.WiredScoreBordWeek.Clear();
                }
            }
        }

        public string? WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13))) return null;
                if (wallPosition.Contains(Convert.ToChar(9))) return null;
                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;
                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);
                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                    return null;
                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);
                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                    return null;
                return $":w={widthX},{widthY} l={lengthX},{lengthY} {posD[2]}";
            }
            catch
            {
                return null;
            }
        }

        public void LoadFurniture(bool FromDB = false)
        {
            if (this._floorItems.Count > 0)
                this._floorItems.Clear();
            if (this._wallItems.Count > 0)
                this._wallItems.Clear();

            var items = ItemLoader.GetItemsForRoom(this._room.Id, this._room);
            foreach (var item in items)
            {
                if (item == null) continue;

                if (item.UserID == 0)
                {
                    using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `user_id` = @uid WHERE `id` = @id");
                        dbClient.AddParameter("uid", this._room.OwnerId);
                        dbClient.AddParameter("id", item.Id);
                        dbClient.RunQuery();
                    }
                }

                if (item.IsFloorItem)
                {
                    if (!_room.GetGameMap().ValidTile(item.GetX, item.GetY))
                    {
                        using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @id");
                            dbClient.AddParameter("id", item.Id);
                            dbClient.RunQuery();
                        }

                        var client = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(item.UserID);
                        if (client != null)
                        {
                            client.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                            client.GetHabbo().GetInventoryComponent().UpdateItems(false);
                        }
                        continue;
                    }

                    if (!_floorItems.ContainsKey(item.Id))
                        _floorItems.TryAdd(item.Id, item);
                }
                else if (item.IsWallItem)
                {
                    if (string.IsNullOrWhiteSpace(item.wallCoord))
                    {
                        using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wall_pos WHERE `id` = @id");
                            dbClient.AddParameter("wall_pos", ":w=0,2 l=11,53 l");
                            dbClient.AddParameter("id", item.Id);
                            dbClient.RunQuery();
                        }
                    }

                    try
                    {
                        item.wallCoord = WallPositionCheck($":{item.wallCoord.Split(':')[1]}");
                    }
                    catch
                    {
                        using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wall_pos WHERE `id` = @id");
                            dbClient.AddParameter("wall_pos", ":w=0,2 l=11,53 l");
                            dbClient.AddParameter("id", item.Id);
                            dbClient.RunQuery();
                        }

                        item.wallCoord = ":w=0,2 l=11,53 l";
                    }

                    if (!_wallItems.ContainsKey(item.Id))
                        _wallItems.TryAdd(item.Id, item);
                }
            }

            foreach (Item Item in _floorItems.Values.ToList())
            {
                if (Item.IsRoller)
                {
                    mGotRollers = true;
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                {
                    if (_room.MoodlightData == null)
                        _room.MoodlightData = new MoodlightData(Item.Id);
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.TONER)
                {
                    if (_room.TonerData == null)
                        _room.TonerData = new TonerData(Item.Id);
                }
                else if (Item.IsWired)
                {
                    if (_room == null)
                        continue;

                    if (_room.GetWired() == null)
                        continue;

                    _room.GetWired().LoadWiredBox(Item);
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.HOPPER)
                    HopperCount++;
                else if (Item.GetBaseItem().InteractionType == InteractionType.JUKEBOX)
                    JukeboxCount++;
            }
        }

        public Item GetItem(int pId)
        {
            if (_floorItems != null && _floorItems.ContainsKey(pId))
            {
                Item Item = null;
                if (_floorItems.TryGetValue(pId, out Item))
                    return Item;
            }
            else if (_wallItems != null && _wallItems.ContainsKey(pId))
            {
                Item Item = null;
                if (_wallItems.TryGetValue(pId, out Item))
                    return Item;
            }

            return null;
        }

        public void RemoveFurniture(GameClient Session, int pId, bool WasPicked = true)
        {
            Item Item = GetItem(pId);
            if (Item == null)
                return;

            if (Item.GetBaseItem().InteractionType == InteractionType.FOOTBALL_GATE)
                _room.GetSoccer().UnRegisterGate(Item);

            if (Item.GetBaseItem().InteractionType != InteractionType.GIFT)
                Item.Interactor.OnRemove(Session, Item);

            if (Item.GetBaseItem().InteractionType == InteractionType.GUILD_GATE)
            {
                Item.UpdateCounter = 0;
                Item.UpdateNeeded = false;
            }
            if (Item.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || Item.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer" || Item.GetBaseItem().ItemName.ToLower() == "olympics_c16_trampoline" || Item.GetBaseItem().InteractionType == InteractionType.GUILD_GATE
                || Item.GetBaseItem().InteractionType == InteractionType.BASURERO
                || Item.GetBaseItem().InteractionType == InteractionType.SLIDING_DOORS
                || Item.GetBaseItem().InteractionType == InteractionType.SHOWER
                || Item.GetBaseItem().InteractionType == InteractionType.TRASH_CAN
                || Item.GetBaseItem().InteractionType == InteractionType.PEPSIMACHINE
                || Item.GetBaseItem().InteractionType == InteractionType.COMIDAMACHINE
                || Item.GetBaseItem().InteractionType == InteractionType.TRAGAMONEDAS
                || Item.GetBaseItem().InteractionType == InteractionType.BASURAENTREGA
                || Item.GetBaseItem().InteractionType == InteractionType.MINERIA
                || Item.GetBaseItem().InteractionType == InteractionType.CAJERORUBY
                || Item.GetBaseItem().InteractionType == InteractionType.CARAMELOMACHINE
                || Item.GetBaseItem().InteractionType == InteractionType.AGUAENERGY)
            {
                Item.UpdateCounter = 0;
                Item.UpdateNeeded = false;
            }

            RemoveRoomItem(Item);
        }

        public void RemoveRoomItem(Item item)
        {
            if (item.IsFloorItem)
                _room.SendMessage(new ObjectRemoveComposer(item, item.UserID));
            else if (item.IsWallItem)
                _room.SendMessage(new ItemRemoveComposer(item, item.UserID));

            //TODO: Recode this specific part
            if (item.IsWallItem)
                _wallItems.TryRemove(item.Id, out item);
            else
            {
                _floorItems.TryRemove(item.Id, out item);
                //mFloorItems.OnCycle();
                _room.GetGameMap().RemoveFromMap(item);
            }

            RemoveItem(item);
            _room.GetGameMap().GenerateMaps();
            _room.GetRoomUserManager().UpdateUserStatusses();
        }

        private List<ServerPacket> CycleRollers()
        {
            if (!mGotRollers)
                return new List<ServerPacket>();

            if (mRollerCycle >= mRollerSpeed || mRollerSpeed == 0)
            {
                rollerItemsMoved.Clear();
                rollerUsersMoved.Clear();
                rollerMessages.Clear();
                List<Item> itemsOnRoller;
                List<Item> itemsOnNext;
                foreach (var roller in _rollers.Values)
                {
                    if (roller == null)
                        continue;
                    var nextSquare = roller.SquareInFront;
                    itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ);
                    itemsOnNext = _room.GetGameMap().GetAllRoomItemForSquare(nextSquare.X, nextSquare.Y);
                    if (itemsOnRoller.Count > 10)
                        itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ).Take(10).ToList();
                    var nextSquareIsRoller = itemsOnNext.Count(x => x.GetBaseItem().InteractionType == InteractionType.ROLLER) > 0;
                    var nextRollerClear = true;
                    var nextZ = 0.0;
                    var nextRoller = false;
                    foreach (var item in itemsOnNext)
                    {
                        if (item.IsRoller)
                        {
                            if (item.TotalHeight > nextZ)
                                nextZ = item.TotalHeight;
                            nextRoller = true;
                        }
                    }
                    if (nextRoller)
                    {
                        foreach (var item in itemsOnNext)
                        {
                            if (item.TotalHeight > nextZ)
                                nextRollerClear = false;
                        }
                    }
                    if (itemsOnRoller.Count > 0)
                    {
                        foreach (Item rItem in itemsOnRoller)
                        {
                            if (rItem == null)
                                continue;

                            GameClient Session = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(rItem.UserID);

                            if (Session != null)
                            {
                                // New
                                bool Flag = true;
                                Dictionary<int, ThreeDCoord> AffectedTiles = Gamemap.GetAffectedTiles(rItem.GetBaseItem().Length, rItem.GetBaseItem().Width, nextSquare.X, nextSquare.Y, rItem.Rotation);

                                foreach (ThreeDCoord Tile in AffectedTiles.Values)
                                {
                                    // BUGAZO
                                    if (!_room.GetGameMap().ValidTile(Tile.X, Tile.Y) ||
                                        (_room.GetGameMap().SquareHasUsers(Tile.X, Tile.Y) && !rItem.GetBaseItem().IsSeat) ||
                                        (!_room.CheckTerrain(Session, Tile.X, Tile.Y) && _room.OwnerId != Session.GetHabbo().Id && !_room.CheckRights(Session, false, true)))// New Poner items en área Terreno
                                    {
                                        Flag = false;
                                    }
                                }

                                if (Flag)
                                {
                                    if (!rollerItemsMoved.Contains(rItem.Id) && (_room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y, Session) || _room.CheckRights(Session, false, true)) && nextRollerClear && roller.GetZ < rItem.GetZ && _room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) == null)// New Poner items en área Terreno
                                    {
                                        if (!nextSquareIsRoller)
                                            nextZ = rItem.GetZ - roller.GetBaseItem().Height;
                                        else
                                            nextZ = rItem.GetZ;

                                        rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                                        rollerItemsMoved.Add(rItem.Id);
                                    }

                                }
                            }
                            else
                            {
                                // Original Algorithm
                                if (!rollerItemsMoved.Contains(rItem.Id) && _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y, Session) && nextRollerClear && roller.GetZ < rItem.GetZ && _room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) == null)
                                {
                                    if (!nextSquareIsRoller)
                                        nextZ = rItem.GetZ - roller.GetBaseItem().Height;
                                    else
                                        nextZ = rItem.GetZ;

                                    rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                                    rollerItemsMoved.Add(rItem.Id);
                                }
                            }
                        }
                    }
                    var rollerUser = _room.GetGameMap().GetRoomUsers(roller.Coordinate).FirstOrDefault();
                    if (rollerUser != null && !rollerUser.IsWalking && nextRollerClear &&
                        _room.GetGameMap().IsValidStep(new Vector2D(roller.GetX, roller.GetY), new Vector2D(nextSquare.X, nextSquare.Y), true, false, true) &&
                        _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && _room.GetGameMap().GetFloorStatus(nextSquare) != 0)
                    {
                        if (!rollerUsersMoved.Contains(rollerUser.HabboId))
                        {
                            if (!nextSquareIsRoller)
                                nextZ = rollerUser.Z - roller.GetBaseItem().Height;
                            else
                                nextZ = rollerUser.Z;
                            rollerUser.isRolling = true;
                            rollerUser.rollerDelay = 1;
                            rollerMessages.Add(UpdateUserOnRoller(rollerUser, nextSquare, roller.Id, nextZ));
                            rollerUsersMoved.Add(rollerUser.HabboId);
                        }
                    }
                }
                mRollerCycle = 0;
                return rollerMessages;
            }
            mRollerCycle++;

            return new List<ServerPacket>();
        }

        public ServerPacket UpdateItemOnRoller(Item pItem, Point NextCoord, int pRolledID, Double NextZ)
        {
            ServerPacket serverMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
            serverMessage.WriteInteger(pItem.GetX);
            serverMessage.WriteInteger(pItem.GetY);
            serverMessage.WriteInteger(NextCoord.X);
            serverMessage.WriteInteger(NextCoord.Y);
            serverMessage.WriteInteger(1);
            serverMessage.WriteInteger(pItem.Id);
            serverMessage.WriteString(pItem.GetZ.ToString());
            serverMessage.WriteString(NextZ.ToString());
            serverMessage.WriteInteger(0);
            this.SetFloorItem(pItem, NextCoord.X, NextCoord.Y, NextZ);
            return serverMessage;
        }

        public ServerPacket UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, int pRollerID, Double NextZ)
        {
            ServerPacket serverMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
            serverMessage.WriteInteger(pUser.X);
            serverMessage.WriteInteger(pUser.Y);
            serverMessage.WriteInteger(pNextCoord.X);
            serverMessage.WriteInteger(pNextCoord.Y);
            serverMessage.WriteInteger(0); //Count items or Users on roller
            serverMessage.WriteInteger(pRollerID);
            serverMessage.WriteInteger(2); //Type
            serverMessage.WriteInteger(pUser.VirtualId);
            serverMessage.WriteString(pUser.Z.ToString());
            serverMessage.WriteString(NextZ.ToString());

            _room.GetGameMap().UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
            pUser.X = pNextCoord.X;
            pUser.Y = pNextCoord.Y;
            pUser.Z = NextZ;

            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;

            if (pUser != null && pUser.GetClient() != null && pUser.GetClient().GetHabbo() != null)
            {
                List<Item> Items = _room.GetGameMap().GetRoomItemForSquare(pNextCoord.X, pNextCoord.Y);
                foreach (Item IItem in Items.ToList())
                {
                    if (IItem == null)
                        continue;

                    this._room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, pUser.GetClient().GetHabbo(), IItem);
                }

                Item Item = this._room.GetRoomItemHandler().GetItem(pRollerID);
                if (Item != null)
                {
                    this._room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, pUser.GetClient().GetHabbo(), Item);
                }
            }

            return serverMessage;
        }

        public Item GetFirstHighscore()
        {
            using (var enumerator = _floorItems.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current2 = enumerator.Current;
                    if (current2.GetBaseItem().InteractionType != InteractionType.WIRED_HIGHSCORE) continue;
                    var result = current2;
                    return result;
                }
            }
            return null;
        }

        public void SaveFurniture()
        {
            try
            {
                if (_movedItems.Count > 0)
                {
                    foreach (Item Item in _movedItems.Values.ToList())
                    {
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            if (!string.IsNullOrEmpty(Item.ExtraData))
                            {
                                if (Item.GetBaseItem().InteractionType == InteractionType.SHOWER || Item.GetBaseItem().InteractionType == InteractionType.GUILD_GATE || Item.GetBaseItem().InteractionType == InteractionType.SLIDING_DOORS)
                                    Item.ExtraData = "0";

                                if (Item.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || Item.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer")
                                    Item.ExtraData = "0";

                                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @edata WHERE `id` = @id");
                                dbClient.AddParameter("edata", Item.ExtraData);
                                dbClient.AddParameter("id", Item.Id);
                                dbClient.RunQuery();
                            }

                            if (Item.IsWallItem && (!Item.GetBaseItem().ItemName.Contains("wallpaper_single") || !Item.GetBaseItem().ItemName.Contains("floor_single") || !Item.GetBaseItem().ItemName.Contains("landscape_single")))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wallPos WHERE `id` = @id");
                                dbClient.AddParameter("wallPos", Item.wallCoord);
                                dbClient.AddParameter("id", Item.Id);
                                dbClient.RunQuery();
                            }

                            dbClient.SetQuery("UPDATE `items` SET `x` = @x, `y` = @y, `z` = @z, `rot` = @rot WHERE `id` = @id");
                            dbClient.AddParameter("x", Item.GetX);
                            dbClient.AddParameter("y", Item.GetY);
                            dbClient.AddParameter("z", Item.GetZ);
                            dbClient.AddParameter("rot", Item.Rotation);
                            dbClient.AddParameter("id", Item.Id);
                            dbClient.RunQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error al guardar muebles para la habitación " + _room.RoomId + ". Stack: " + e);
            }
        }

        public bool SetFloorItem(GameClient Session, Item Item, int newX, int newY, int newRot, bool newItem, bool OnRoller, bool sendMessage, bool updateRoomUserStatuses = false, bool ToDB = true, Item SpaceItem = null, bool PlacedByRoleplay = false)
        {
           /* double origZ = newZ;
            bool CheckHeight = newZ == 1000.0 ? true : false;*/
            bool NeedsReAdd = false;

            if (newItem && Item.ExtraData == "")
                Item.ExtraData = "0";

            if (newItem)
                if (Item.IsWired)
                    if (Item.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps && _room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps).Count() > 0)
                        return false;

            if (_room.HideWired)
            {
                _room.HideWired = false;
                Session.SendWhisper("La zona ha vuelto a mostrar los Wireds.", 1);
                _room.SendMessage(_room.HideWiredMessages(false));
            }


            List<Item> ItemsOnTile = GetFurniObjects(newX, newY);
            if (Item.GetBaseItem().InteractionType == InteractionType.ROLLER && ItemsOnTile.Where(x => x.GetBaseItem().InteractionType == InteractionType.ROLLER && x.Id != Item.Id).Count() > 0)
                return false;

            if (!newItem)
                NeedsReAdd = _room.GetGameMap().RemoveFromMap(Item);

            Dictionary<int, ThreeDCoord> AffectedTiles = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);

            if (!PlacedByRoleplay)
            {
                if (!_room.GetGameMap().ValidTile(newX, newY) || (_room.GetGameMap().SquareHasUsers(newX, newY) && !Item.GetBaseItem().Walkable && !Item.GetBaseItem().IsSeat))
                {
                    if (NeedsReAdd)
                        _room.GetGameMap().AddToMap(Item);
                    return false;
                }

                foreach (ThreeDCoord Tile in AffectedTiles.Values)
                {
                    if (!_room.GetGameMap().ValidTile(Tile.X, Tile.Y) ||
                        (_room.GetGameMap().SquareHasUsers(Tile.X, Tile.Y) && !Item.GetBaseItem().IsSeat && Session != null) ||
                        (Session != null && !Session.GetRoleplay().isParking && !_room.CheckTerrain(Session, Tile.X, Tile.Y) && _room.OwnerId != Session.GetHabbo().Id && !_room.CheckRights(Session, false, true)))// New Poner items en área Terreno
                    {
                        if (NeedsReAdd)
                        {
                            _room.GetGameMap().AddToMap(Item);
                        }
                        return false;
                    }
                }
            }

            if (SpaceItem != null && Session != null)
            {
                bool CanPlaceHere = true;

                if (AffectedTiles.Values.Select(x => new Point(x.X, x.Y)).Where(x => !SpaceItem.GetAffectedTiles.Contains(x)).ToList().Count > 0)
                    CanPlaceHere = false;

                if (_room.OwnerId == Session.GetHabbo().Id || _room.CheckRights(Session) || Session.GetHabbo().GetPermissions().HasRight("room_item_place_exchange_anywhere"))
                    CanPlaceHere = true;

                if (!CanPlaceHere)
                    return false;
            }

            // Start calculating new Z coordinate
            double newZ = (double)_room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (Session != null)
            {
                if (Session.GetHabbo().DebugStacking == true)
                    newZ = Session.GetHabbo().StackHeight;
            }

            if (!OnRoller && !PlacedByRoleplay)
            {
                // Make sure this tile is open and there are no users here
                if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN && !Item.GetBaseItem().IsSeat && !Item.GetBaseItem().Walkable)
                    return false;

                foreach (ThreeDCoord Tile in AffectedTiles.Values)
                {
                    if (_room.GetGameMap().Model.SqState[Tile.X, Tile.Y] != SquareState.OPEN && !Item.GetBaseItem().IsSeat && !Item.GetBaseItem().Walkable)
                    {
                        if (NeedsReAdd)
                            _room.GetGameMap().AddToMap(Item);
                        return false;
                    }
                }

                // And that we have no users
                if (!Item.GetBaseItem().IsSeat && !Item.IsRoller && !Item.GetBaseItem().Walkable)
                {
                    foreach (ThreeDCoord Tile in AffectedTiles.Values)
                    {
                        if (_room.GetGameMap().GetRoomUsers(new Point(Tile.X, Tile.Y)).Count > 0)
                        {
                            if (NeedsReAdd)
                                _room.GetGameMap().AddToMap(Item);
                            return false;
                        }
                    }
                }
            }

            // Find affected objects
            var ItemsAffected = new List<Item>();
            var ItemsComplete = new List<Item>();


            foreach (ThreeDCoord Tile in AffectedTiles.Values.ToList())
            {
                List<Item> Temp = GetFurniObjects(Tile.X, Tile.Y);

                if (Temp != null)
                {
                    ItemsAffected.AddRange(Temp);
                }
            }


            ItemsComplete.AddRange(ItemsOnTile);
            ItemsComplete.AddRange(ItemsAffected);

             /*bool PileMagic = false;

             if (Item.GetBaseItem().InteractionType == InteractionType.STACKTOOL)
                 PileMagic = true;

             foreach (Item roomItem in ItemsComplete)
             {
                 if (roomItem.GetBaseItem().InteractionType == InteractionType.STACKTOOL)
                 {
                     newZ = roomItem.GetZ;
                     PileMagic = true;
                     break;
                 }
                 if (roomItem.Id != Item.Id && roomItem.TotalHeight > newZ)
                     newZ = roomItem.TotalHeight;
             }*/

            // Are there any higher objects in the stack!?
            foreach (Item I in ItemsComplete.ToList())
            {
                if (I == null)
                    continue;
                if (I.Id == Item.Id)
                    continue;

                if (I.GetBaseItem().InteractionType == InteractionType.STACKTOOL && Session.GetHabbo().StackHeight == 0)
                {
                    newZ = I.GetZ;
                    break;
                }
                if (I.TotalHeight > newZ && Session.GetHabbo().StackHeight != 0)
                {
                    newZ = Session.GetHabbo().StackHeight;
                }
                else if (I.TotalHeight > newZ && Session.GetHabbo().StackHeight == 0)
                {
                    newZ = I.TotalHeight;
                }
            }

            if (!OnRoller)
            {
                foreach (Item roomItem in ItemsComplete)
                {
                    if (roomItem != null && roomItem.Id != Item.Id && (roomItem.GetBaseItem() != null && (!roomItem.GetBaseItem().Stackable/* && !PileMagic*/)))
                    {
                        if (NeedsReAdd)
                        {
                            this.UpdateItem(Item);
                            this._room.GetGameMap().AddToMap(Item);
                        }
                        return false;
                    }
                }
            }

            // Verify the rotation is correct
            if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8 && !Item.GetBaseItem().ExtraRot)
                newRot = 0;

            Item.Rotation = newRot;
            /*int oldX = Item.GetX;
            int oldY = Item.GetY;

            if (!CheckHeight)
                newZ = origZ;*/

            Item.SetState(newX, newY, newZ, AffectedTiles);

            if (!OnRoller && Session != null)
                Item.Interactor.OnPlace(Session, Item);

            if (newItem)
            {
                if (_floorItems.ContainsKey(Item.Id))
                {
                    if (Session != null)
                        Session.SendNotification(PolarEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_item_placed"));
                    _room.GetGameMap().RemoveFromMap(Item);
                    return true;
                }

                if (Item.IsFloorItem && !_floorItems.ContainsKey(Item.Id))
                    _floorItems.TryAdd(Item.Id, Item);
                else if (Item.IsWallItem && !_wallItems.ContainsKey(Item.Id))
                    _wallItems.TryAdd(Item.Id, Item);

                if (sendMessage)
                    _room.SendMessage(new ObjectAddComposer(Item, _room));
            }
            else
            {
                UpdateItem(Item);
                if (!OnRoller && sendMessage)
                    _room.SendMessage(new ObjectUpdateComposer(Item, Item.UserID));
            }
            _room.GetGameMap().AddToMap(Item);

            if (Item.GetBaseItem().InteractionType == InteractionType.FOOTBALL)
            {

                if (Item.GetRoom() != null && Item.GetRoom().GotSoccer() && Session != null)
                {
                    RoomUser user = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (user != null)
                        Item.GetRoom().GetSoccer().MoveBall(Item, newX, newY, user);
                }
            }

            if (Item.GetBaseItem().IsSeat)
                updateRoomUserStatuses = true;

            if (updateRoomUserStatuses)
                _room.GetRoomUserManager().UpdateUserStatusses();

            if (Item.GetBaseItem().InteractionType == InteractionType.TENT || Item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
            {
                _room.RemoveTent(Item.Id, Item);
                _room.AddTent(Item.Id);
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = @rid, `x` = @x, `y` = @y, `z` = @z, `rot` = @rot WHERE `id` = @id");
                dbClient.AddParameter("rid", _room.RoomId);
                dbClient.AddParameter("x", Item.GetX);
                dbClient.AddParameter("y", Item.GetY);
                dbClient.AddParameter("z", Item.GetZ);
                dbClient.AddParameter("rot", Item.Rotation);
                dbClient.AddParameter("id", Item.Id);
                dbClient.RunQuery();
            }
            return true;
        }

        public List<Item> GetFurniObjects(int x, int y) => _room.GetGameMap().GetCoordinatedItems(new(x, y));


        public bool SetFloorItem(Item Item, int newX, int newY, Double newZ)
        {
            if (_room == null)
                return false;

            _room.GetGameMap().RemoveFromMap(Item);

            Item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, Item.Rotation));
            if (Item.GetBaseItem().InteractionType == InteractionType.TONER)
            {
                if (_room.TonerData == null)
                {
                    _room.TonerData = new TonerData(Item.Id);
                }
            }
            UpdateItem(Item);
            _room.GetGameMap().AddItemToMap(Item);
            return true;
        }

        public bool SetWallItem(GameClient Session, Item Item)
        {
            if (!Item.IsWallItem || _wallItems.ContainsKey(Item.Id))
                return false;

            if (_floorItems.ContainsKey(Item.Id))
            {
                Session.SendNotification(PolarEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_item_placed"));
                return true;
            }

            Item.Interactor.OnPlace(Session, Item);
            if (Item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
            {
                if (_room.MoodlightData == null)
                {
                    _room.MoodlightData = new MoodlightData(Item.Id);
                    Item.ExtraData = _room.MoodlightData.GenerateExtraData();
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = @rid, `x` = @x, `y` = @y, `z` = @z, `rot` = @rot, `wall_pos` = @wpos WHERE `id` = @id");
                dbClient.AddParameter("rid", _room.RoomId);
                dbClient.AddParameter("x", Item.GetX);
                dbClient.AddParameter("y", Item.GetY);
                dbClient.AddParameter("z", Item.GetZ);
                dbClient.AddParameter("rot", Item.Rotation);
                dbClient.AddParameter("wpos", Item.wallCoord);
                dbClient.AddParameter("id", Item.Id);
                dbClient.RunQuery();
            }

            _wallItems.TryAdd(Item.Id, Item);

            _room.SendMessage(new ItemAddComposer(Item));

            return true;
        }

        public void UpdateItem(Item item)
        {
            if (item == null)
                return;
            if (!_movedItems.ContainsKey(item.Id))
                _movedItems.TryAdd(item.Id, item);
        }


        public void RemoveItem(Item item)
        {
            if (item == null)
                return;

            if (_movedItems.ContainsKey(item.Id))
                _movedItems.TryRemove(item.Id, out item);
            if (_rollers.ContainsKey(item.Id))
                _rollers.TryRemove(item.Id, out item);
        }

        public void OnCycle()
        {
            if (GotRollers)
            {
                try
                {
                    _room.SendMessage(CycleRollers());
                }
                catch //(Exception e)
                {
                    // Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                    GotRollers = false;
                }
            }
            if (_roomItemUpdateQueue.Count > 0)
            {
                var addItems = new List<Item>();
                while (_roomItemUpdateQueue.Count > 0)
                {
                    var item = (Item)null;
                    if (_roomItemUpdateQueue.TryDequeue(out item))
                    {
                        item.ProcessUpdates();
                        if (item.UpdateCounter > 0)
                            addItems.Add(item);
                    }
                }
                foreach (var item in addItems.ToList())
                {
                    if (item == null)
                        continue;
                    _roomItemUpdateQueue.Enqueue(item);
                }
            }
        }
        public List<Item> RemoveItems(GameClient Session)
        {
            List<Item> Items = new List<Item>();

            foreach (Item Item in this.GetWallAndFloor.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.IsFloorItem)
                {
                    Item I = null;
                    this._floorItems.TryRemove(Item.Id, out I);
                    Session.GetHabbo().GetInventoryComponent()._floorItems.TryAdd(Item.Id, I);
                    this._room.SendMessage(new ObjectRemoveComposer(Item, Item.UserID));
                }
                else if (Item.IsWallItem)
                {
                    Item I = null;
                    this._wallItems.TryRemove(Item.Id, out I);
                    Session.GetHabbo().GetInventoryComponent()._wallItems.TryAdd(Item.Id, I);
                    this._room.SendMessage(new ItemRemoveComposer(Item, Item.UserID));
                }

                this._room.GetGameMap().GenerateMaps();
                Session.SendMessage(new FurniListAddComposer(Item));
            }

            return Items;
        }

        public List<Item> ClearItems(GameClient Session)
        {
            List<Item> Items = new List<Item>();

            foreach (Item Item in this.GetWallAndFloor.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.IsFloorItem)
                {
                    Session.SendMessage(new ObjectRemoveComposer(Item, 0));
                }
                else if (Item.IsWallItem)
                {
                    Session.SendMessage(new ItemRemoveComposer(Item, 0));
                }
            }
            return Items;
        }

        public ICollection<Item> GetFloor
        {
            get
            {
                return (this._floorItems == null ? null : this._floorItems.Values);
            }
        }

        public ICollection<Item> GetWall
        {
            get
            {
                return (this._wallItems == null ? null : this._wallItems.Values);
            }
        }

        public IEnumerable<Item> GetWallAndFloor
        {
            get
            {
                return this._floorItems.Values.Concat(this._wallItems.Values);
            }
        }


        public bool CheckPosItem(GameClient Session, Item Item, int newX, int newY, int newRot, bool newItem, bool SendNotify = true)
        {
            try
            {
                Dictionary<int, ThreeDCoord> dictionary = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);
                if (!this._room.GetGameMap().ValidTile(newX, newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if ((this._room.GetGameMap().Model.DoorX == coord.X) && (this._room.GetGameMap().Model.DoorY == coord.Y))
                        return false;
                }

                if ((this._room.GetGameMap().Model.DoorX == newX) && (this._room.GetGameMap().Model.DoorY == newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if (!this._room.GetGameMap().ValidTile(coord.X, coord.Y))
                        return false;
                }

                double num = this._room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if ((((Item.Rotation == newRot) && (Item.GetX == newX)) && (Item.GetY == newY)) && (Item.GetZ != num))
                    return false;

                if (this._room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN)
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if (this._room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.OPEN)
                        return false;
                }
                /*
                if (!Item.GetBaseItem().IsSeat)
                {
                    if (this._room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;

                    foreach (ThreeDCoord coord in dictionary.Values.ToList())
                    {
                        if (this._room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                            return false;
                    }
                }
                */
                List<Item> furniObjects = this.GetFurniObjects(newX, newY);
                List<Item> collection = new List<Item>();
                List<Item> list3 = new List<Item>();
                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    List<Item> list4 = this.GetFurniObjects(coord.X, coord.Y);
                    if (list4 != null)
                        collection.AddRange(list4);

                }

                if (furniObjects == null)
                    furniObjects = new List<Item>();

                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                foreach (Item item in list3.ToList())
                {
                    if ((item.Id != Item.Id) && !item.GetBaseItem().Stackable)
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public ICollection<Item> GetRollers()
        {
            return this._rollers.Values;
        }

        public void Dispose()
        {
            foreach (Item Item in this.GetWallAndFloor.ToList())
            {
                if (Item == null)
                    continue;

                Item.Destroy();
            }

            _floorItems.Clear();
            _wallItems.Clear();
            _movedItems.Clear();
            this._roomItemUpdateQueue = null;

            _room = null;
            _floorItems = null;
            _wallItems = null;
            _movedItems = null;
            _wallItems = null;
            _roomItemUpdateQueue = null;
        }
    }
}