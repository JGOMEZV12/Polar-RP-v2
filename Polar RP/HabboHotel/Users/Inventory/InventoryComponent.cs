using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polar.HabboHotel.Rooms.AI;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Items.Crafting;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Users.Inventory.Pets;
using Polar.HabboHotel.Users.Inventory.Bots;
using Polar.Communication.Packets.Outgoing.Inventory.Furni;
using Polar.Database.Interfaces;


namespace Polar.HabboHotel.Users.Inventory
{
    public class InventoryComponent
    {
        private int _userId;
        private GameClient _client;
        public int InventaryUserId;

        public ConcurrentDictionary<int, Bot> _botItems;
        public ConcurrentDictionary<int, Pet> _petsItems;
        public ConcurrentDictionary<int, Item> _floorItems;
        public ConcurrentDictionary<int, Item> _wallItems;
        public Dictionary<int, Item> songDisks;

        public InventoryComponent(int UserId, GameClient Client)
        {
            this._client = Client;
            this._userId = UserId;

            this.InventaryUserId = 0;

            this._floorItems = new ConcurrentDictionary<int, Item>();
            this._wallItems = new ConcurrentDictionary<int, Item>();
            this._petsItems = new ConcurrentDictionary<int, Pet>();
            this._botItems = new ConcurrentDictionary<int, Bot>();
            this.songDisks = new Dictionary<int, Item>();

            this.Init();
        }

        public void Init()
        {
            if (this._floorItems.Count > 0)
                this._floorItems.Clear();
            if (this._wallItems.Count > 0)
                this._wallItems.Clear();
            if (this._petsItems.Count > 0)
                this._petsItems.Clear();
            if (this._botItems.Count > 0)
                this._botItems.Clear();
            if (this.songDisks.Count > 0)
                this.songDisks.Clear();

            List<Item> Items = ItemLoader.GetItemsForUser(_userId);
            foreach (Item Item in Items.ToList())
            {
                if (Item.GetBaseItem().InteractionType == InteractionType.MUSIC_DISC)
                {
                    this.songDisks.Add(Item.Id, Item);
                    //this.UpdateJuke();
                }
                if (Item.IsFloorItem)
                {
                    if (!this._floorItems.TryAdd(Item.Id, Item))
                        continue;
                }
                else if (Item.IsWallItem)
                {
                    if (!this._wallItems.TryAdd(Item.Id, Item))
                        continue;
                }
                else
                    continue;
            }

            List<Pet> Pets = PetLoader.GetPetsForUser(Convert.ToInt32(_userId));
            foreach (Pet Pet in Pets)
            {
                if (!this._petsItems.TryAdd(Pet.PetId, Pet))
                {
                    Console.WriteLine("Error whilst loading pet x1: " + Pet.PetId);
                }
            }

            List<Bot> Bots = BotLoader.GetBotsForUser(Convert.ToInt32(_userId));
            foreach (Bot Bot in Bots)
            {
                if (!this._botItems.TryAdd(Bot.Id, Bot))
                {
                    Console.WriteLine("Error whilst loading bot x1: " + Bot.Id);
                }
            }
        }

        public void ClearItems()
        {
            UpdateItems(true);

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE items, wired_items, user_presents, room_items_moodlight, room_items_tele_links, room_items_toner, items_groups FROM items " +
                    "LEFT JOIN wired_items ON(wired_items.id = items.id) " +
                    "LEFT JOIN user_presents ON(user_presents.item_id = items.id) " +
                    "LEFT JOIN room_items_moodlight ON(room_items_moodlight.item_id = items.id) " +
                    "LEFT JOIN room_items_tele_links ON(room_items_tele_links.tele_one_id = items.id OR room_items_tele_links.tele_two_id = items.id) " +
                    "LEFT JOIN room_items_toner ON(room_items_toner.id = items.id) " +
                    "LEFT JOIN items_groups ON(items_groups.id = items.id) " +
                    "WHERE items.room_id = '0' AND items.user_id = '" + _userId + "'");
            }

            this._floorItems.Clear();
            this._wallItems.Clear();
            this.songDisks.Clear();

            if (_client != null)
                _client.SendMessage(new FurniListUpdateComposer());
        }

        public void SetIdleState()
        {
            if (_botItems != null)
                _botItems.Clear();

            if (_petsItems != null)
                _petsItems.Clear();

            if (_floorItems != null)
                _floorItems.Clear();

            if (_wallItems != null)
                _wallItems.Clear();

            if (songDisks != null)
                songDisks.Clear();

            _botItems = null;
            songDisks = null;
            _petsItems = null;
            _floorItems = null;
            _wallItems = null;
            _client = null;
        }

        public void UpdateJuke()
        {
            //GetClient().SendMessage(new LoadJukeboxUserMusicItemsComposer(songDisks));

        }
        public void UpdateItems(bool FromDatabase)
        {
            if (FromDatabase)
                Init();

            if (_client != null)
            {
                _client.SendMessage(new FurniListUpdateComposer());
                //_client.SendMessage(new LoadJukeboxUserMusicItemsComposer(_client.GetHabbo().GetInventoryComponent().songDisks));
            }
        }

        public Item GetItem(int Id)
        {
       
            if (_floorItems.ContainsKey(Id))
                return (Item) _floorItems[Id];
            else if (_wallItems.ContainsKey(Id))
                return (Item) _wallItems[Id];

            return null;
        }

        public IEnumerable<Item> GetItems
        {
            get
            {
                return this._floorItems.Values.Concat(this._wallItems.Values);/*.Concat(this.songDisks.Values);*/
            }
        }
        public void AddItem(Item item)
        {
            this.AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
        }

        public IEnumerable<Item> AllItems => _floorItems.Values.Concat(_wallItems.Values);

        public Item AddNewItem(int Id, int BaseItem, string ExtraData, int Group, bool ToInsert, bool FromRoom, int LimitedNumber, int LimitedStack)
        {
            

            if (ToInsert)
            {
                if (FromRoom)
                {
                    using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `items` SET `room_id` = '0', `user_id` = '" + _userId + "' WHERE `id` = '" + Id + "' LIMIT 1");
                    }
                }
                else
                {
                    using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        if (Id > 0)
                            dbClient.RunQuery("INSERT INTO `items` (`id`,`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" + Id + "', '" + BaseItem + "', '" + _userId + "', '" + LimitedNumber + "', '" + LimitedStack + "')");
                        else
                        {
                            dbClient.SetQuery("INSERT INTO `items` (`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" + BaseItem + "', '" + _userId + "', '" + LimitedNumber + "', '" + LimitedStack + "')");
                            Id = Convert.ToInt32(dbClient.InsertQuery());
                        }

                        SendNewItems(Convert.ToInt32(Id));

                        if (Group > 0)
                            dbClient.RunQuery("INSERT INTO `items_groups` VALUES (" + Id + ", " + Group + ")");

                        if (!string.IsNullOrEmpty(ExtraData))
                        {
                            dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extradata WHERE `id` = '" + Id + "' LIMIT 1");
                            dbClient.AddParameter("extradata", ExtraData);
                            dbClient.RunQuery();
                        }
                    }
                }
            }

            Item ItemToAdd = new Item(Id, 0, BaseItem, ExtraData, 0, 0, 0, 0, _userId, Group, LimitedNumber, LimitedStack, string.Empty);
 
            if (UserHoldsItem(Id))
                RemoveItem(Id);

            if (ItemToAdd.GetBaseItem().InteractionType == InteractionType.MUSIC_DISC)
            {
                this.songDisks.Add(ItemToAdd.Id, ItemToAdd);
                this.UpdateJuke();
            }

            UpdateItems(true);

            if (ItemToAdd.IsWallItem)
                this._wallItems.TryAdd(ItemToAdd.Id, ItemToAdd);
            else
                this._floorItems.TryAdd(ItemToAdd.Id, ItemToAdd);
            return ItemToAdd;

        }

        private bool UserHoldsItem(int itemID)
        {
         
            if (_floorItems.ContainsKey(itemID))
                return true;
            if (_wallItems.ContainsKey(itemID))
                return true;
            if (songDisks.ContainsKey(itemID))
                return true;
            return false;
        }

        public void RemoveItem(int Id)
        {
            //UpdateItems(true);

            if (GetClient() == null)
                return;

            if (GetClient().GetHabbo() == null || GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                GetClient().Disconnect(true);
            }
            /*if (songDisks.ContainsKey(Id))
            {
                songDisks.Remove(Id);
            }*/
            if (this._floorItems.ContainsKey(Id))
            {
                Item ToRemove = null;
                this._floorItems.TryRemove(Id, out ToRemove);
            }

            if (this._wallItems.ContainsKey(Id))
            {
                Item ToRemove = null;
                this._wallItems.TryRemove(Id, out ToRemove);
            }

            if (this.songDisks.ContainsKey(Id))
            {
                Item ToRemove = null;
                this.songDisks.Remove(Id, out ToRemove);
            }

            GetClient().SendMessage(new FurniListRemoveComposer(Id));
        }

        private GameClient GetClient()
        {
            return PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(_userId);
        }

        public void SendNewItems(int Id)
        {
            _client.SendMessage(new FurniListNotificationComposer(Id, 1));
        }

        #region Pet Handling
        public ICollection<Pet> GetPets()
        {
            return this._petsItems.Values;
        }

        public bool TryAddPet(Pet Pet)
        {
            //TODO: Sort this mess.
            Pet.RoomId = 0;
            Pet.PlacedInRoom = false;

            return this._petsItems.TryAdd(Pet.PetId, Pet);
        }

        public bool TryRemovePet(int PetId, out Pet PetItem)
        {
            if (this._petsItems.ContainsKey(PetId))
                return this._petsItems.TryRemove(PetId, out PetItem);
            else
            {
                PetItem = null;
                return false;
            }
        }

        public bool TryGetPet(int PetId, out Pet Pet)
        {
            if (this._petsItems.ContainsKey(PetId))
                return this._petsItems.TryGetValue(PetId, out Pet);
            else
            {
                Pet = null;
                return false;
            }
        }
        #endregion

        #region Bot Handling
        public ICollection<Bot> GetBots()
        {
            return this._botItems.Values;
        }

        public bool TryAddBot(Bot Bot)
        {
            return this._botItems.TryAdd(Bot.Id, Bot);
        }

        public bool TryRemoveBot(int BotId, out Bot Bot)
        {
            if (this._botItems.ContainsKey(BotId))
                return this._botItems.TryRemove(BotId, out Bot);
            else
            {
                Bot = null;
                return false;
            }
        }

        public bool TryGetBot(int BotId, out Bot Bot)
        {
            if (this._botItems.ContainsKey(BotId))
                return this._botItems.TryGetValue(BotId, out Bot);
            else
            {
                Bot = null;
                return false;
            }
        }
        #endregion

        public void LoadUserInventory(int InventaryId)
        {
            // dont need to update Inventary?

            InventaryUserId = InventaryId;
            UpdateItems(true);
        }

        private int GetUserInventaryId()
        {
            if (InventaryUserId > 0)
                return InventaryUserId;
            else
                return _userId;
        }

        internal Item GetFirstItemByBaseId(int id)
        {
            return _floorItems.Values.Where(item => item != null && item.GetBaseItem() != null && item.GetBaseItem().Id == id).FirstOrDefault();
        }

        public bool TryAddItem(Item item)
        {
            /*if (item.Data.Type.ToString().ToLower() == "s" && item.GetBaseItem().InteractionType == InteractionType.MUSIC_DISC)// ItemType.FLOOR)
            {
                this.songDisks.Add(item.Id, item);
                //this.UpdateJuke();
            }*/
            if (item.Data.Type.ToString().ToLower() == "s")// ItemType.FLOOR)
            {
                return this._floorItems.TryAdd(item.Id, item);
            }
            else if (item.Data.Type.ToString().ToLower() == "i")//ItemType.WALL)
            {
                return this._wallItems.TryAdd(item.Id, item);
            }
            else
            {
                throw new InvalidOperationException("Item did not match neither floor or wall item");
            }
        }

        public ICollection<Item> GetFloorItems()
        {
            return this._floorItems.Values;
        }

        public ICollection<Item> GetWallItems()
        {
            return this._wallItems.Values;
        }

        public ICollection<Item> GetSongDisks()
        {
            return songDisks.Values;
        }

        public IEnumerable<Item> GetWallAndFloor
        {
            get
            {
                return this._floorItems.Values.Concat(this._wallItems.Values);/*.Concat(this.songDisks.Values)*/
            }
        }
    }
}