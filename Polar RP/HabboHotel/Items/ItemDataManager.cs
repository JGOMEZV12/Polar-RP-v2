using System;
using System.Data;
using System.Collections.Generic;

using log4net;
using Polar.Core;

using Polar.Database.Interfaces;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Text;

namespace Polar.HabboHotel.Items
{
    public class ItemDataManager
    {
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Items.ItemDataManager");

        public Dictionary<int, ItemData> _items;
        public Dictionary<int, ItemData> _gifts;//<SpriteId, Item>

        public ItemDataManager()
        {
            this._items = new Dictionary<int, ItemData>();
            this._gifts = new Dictionary<int, ItemData>();
        }

        public async Task InitAsync()
        {
            if (this._items.Count > 0)
                this._items.Clear();

            await Task.Run(() =>
            {
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `furniture`");
                    DataTable ItemData = dbClient.getTable();

                    if (ItemData != null)
                    {
                        foreach (DataRow Row in ItemData.Rows)
                        {
                            try
                            {
                                int id = Convert.ToInt32(Row["id"]);
                                int spriteID = Convert.ToInt32(Row["sprite_id"]);
                                string itemName = Convert.ToString(Row["item_name"]);
                                string publicname = Convert.ToString(Row["public_name"]);
                                string type = Row["type"].ToString();
                                int width = Convert.ToInt32(Row["width"]);
                                int length = Convert.ToInt32(Row["length"]);
                                double height = Convert.ToDouble(Row["stack_height"]);
                                bool allowStack = PolarEnvironment.EnumToBool(Row["can_stack"].ToString());
                                bool allowWalk = PolarEnvironment.EnumToBool(Row["is_walkable"].ToString());
                                bool allowSit = PolarEnvironment.EnumToBool(Row["can_sit"].ToString());
                                bool allowRecycle = PolarEnvironment.EnumToBool(Row["allow_recycle"].ToString());
                                bool allowTrade = PolarEnvironment.EnumToBool(Row["allow_trade"].ToString());
                                bool allowMarketplace = Convert.ToInt32(Row["allow_marketplace_sell"]) == 1;
                                bool allowGift = Convert.ToInt32(Row["allow_gift"]) == 1;
                                bool allowInventoryStack = PolarEnvironment.EnumToBool(Row["allow_inventory_stack"].ToString());
                                InteractionType interactionType = InteractionTypes.GetTypeFromString(Convert.ToString(Row["interaction_type"]));
                                int behaviourData = Convert.ToInt32(Row["behaviour_data"]);
                                int cycleCount = Convert.ToInt32(Row["interaction_modes_count"]);
                                string vendingIDS = Convert.ToString(Row["vending_ids"]);
                                List<double> heightAdjustable = Row["height_adjustable"].ToString() != String.Empty ? Row["height_adjustable"].ToString().Split(',').Select(x => Convert.ToDouble(x)).ToList() : new List<double>();
                                int EffectId = Convert.ToInt32(Row["effect_id"]);
                                bool IsRare = PolarEnvironment.EnumToBool(Row["is_rare"].ToString());
                                int ClothingId = Convert.ToInt32(Row["clothing_id"]);
                                bool ExtraRot = PolarEnvironment.EnumToBool(Row["extra_rot"].ToString());

                                if (!this._gifts.ContainsKey(spriteID))
                                    this._gifts.Add(spriteID, new ItemData(id, spriteID, itemName, publicname, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIDS, heightAdjustable, EffectId, IsRare, ClothingId, ExtraRot));

                                if (!this._items.ContainsKey(id))
                                    this._items.Add(id, new ItemData(id, spriteID, itemName, publicname, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIDS, heightAdjustable, EffectId, IsRare, ClothingId, ExtraRot));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                Console.ReadKey();
                                Logging.WriteLine("Could not load item #" + Convert.ToInt32(Row[0]) + ", please verify the data is okay.");
                            }
                        }
                    }
                }
            });

            //log.Info("Item Manager -> LOADED");
        }

        public bool GetItem(string Name, out ItemData Item)
        {
            Item = null;
            if (this._items.Values.Where(x => x.ItemName.ToLower() == Name.ToLower()).ToList().Count > 0)
            {
                Item = this._items.Values.FirstOrDefault(x => x.ItemName.ToLower() == Name.ToLower());
                return true;
            }

            return false;
        }

        public bool GetItem(int id, out ItemData item)
        {
            return _items.TryGetValue(id, out item);
        }

        public ItemData GetItemByName(string name)
        {
            foreach (var entry in this._items)
            {
                ItemData item = entry.Value;
                if (item.ItemName == name)
                    return item;
            }

            return null;
        }

        public bool GetGift(int SpriteId, out ItemData Item)
        {
            if (this._gifts.TryGetValue(SpriteId, out Item))
                return true;
            return false;
        }

        public void DownloadFurnis()
        {
            #region Variables
            XDocument xDoc = XDocument.Load(@"furnidata_updated.xml");
            string ItemName = "";
            string Type = "s";
            int SpriteId = 1;
            int XDim = 0;
            int YDim = 0;
            string PublicName = "";
            string Description = "";
            string AdURL = "";
            string CustomParams = "";
            int SpecialType = 0;
            bool ExcludedDynamic = false;
            bool CanStandOn = false;
            bool CanSitOn = false;
            bool CanLayOn = false;
            string FurniLine = "";
            #endregion

            #region Execute
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                var downloadRoomList = xDoc.Descendants("roomitemtypes").Descendants("furnitype");
                var downloadWallList = xDoc.Descendants("wallitemtypes").Descendants("furnitype");

                if (downloadRoomList.ToList().Count > 0)
                {
                    foreach (var downloadRoomItem in downloadRoomList)
                    {
                        #region Set Variables
                        try
                        {
                            ItemName = downloadRoomItem
                                .Attribute("classname")
                                .Value;
                            SpriteId = Convert.ToInt32(downloadRoomItem
                                .Attribute("id")
                                .Value);
                            PublicName = downloadRoomItem
                                .Element("name")
                                .Value;
                            Description = downloadRoomItem
                                .Element("description")
                                .Value;
                            SpecialType = Convert.ToInt32(downloadRoomItem
                                .Element("specialtype")
                                .Value);
                            AdURL = downloadRoomItem
                                .Element("adurl")
                                .Value;
                            CustomParams = downloadRoomItem
                                .Element("customparams")
                                .Value;
                            XDim = Convert.ToInt32(downloadRoomItem
                                .Element("xdim")
                                .Value);
                            YDim = Convert.ToInt32(downloadRoomItem
                                .Element("ydim")
                                .Value);
                            ExcludedDynamic = PolarEnvironment.EnumToBool(downloadRoomItem
                                .Element("excludeddynamic")
                                .Value);
                            CanLayOn = PolarEnvironment.EnumToBool(downloadRoomItem
                                .Element("canlayon")
                                .Value);
                            CanSitOn = PolarEnvironment.EnumToBool(downloadRoomItem
                                .Element("cansiton")
                                .Value);
                            CanStandOn = PolarEnvironment.EnumToBool(downloadRoomItem
                                .Element("canstandon")
                                .Value);
                            FurniLine = downloadRoomItem
                                .Element("furniline")
                                .Value;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        #endregion

                        #region Insert Query
                        try
                        {
                            dbClient.SetQuery("INSERT INTO `furniture_new`" +
                                "(id,item_name,public_name,type,width,length,can_sit,can_lay,is_walkable,sprite_id,description,specialtype,customparams,excludeddynamic,adurl,furniline) VALUES " +
                                "(@id,@item_name,@public_name,@type,@width,@length,@can_sit,@can_lay,@is_walkable,@sprite_id,@description,@specialtype,@customparams,@excludeddynamic,@adurl,@furniline) ON DUPLICATE KEY UPDATE " +
                                "id = VALUES(id)," +
                                "item_name = VALUES(item_name)," +
                                "public_name = VALUES(public_name)," +
                                "type = VALUES(type)," +
                                "width = VALUES(width)," +
                                "length = VALUES(length)," +
                                "can_sit = VALUES(can_sit)," +
                                "can_lay = VALUES(can_lay)," +
                                "is_walkable = VALUES(is_walkable)," +
                                "sprite_id = VALUES(sprite_id)," +
                                "description = VALUES(description)," +
                                "specialtype = VALUES(specialtype)," +
                                "customparams = VALUES(customparams)," +
                                "excludeddynamic = VALUES(excludeddynamic)," +
                                "adurl = VALUES(adurl)," +
                                "furniline = VALUES(furniline);");
                            dbClient.AddParameter("id", SpriteId);
                            dbClient.AddParameter("item_name", ItemName);
                            dbClient.AddParameter("public_name", PublicName);
                            dbClient.AddParameter("type", Type);
                            dbClient.AddParameter("width", XDim);
                            dbClient.AddParameter("length", YDim);
                            dbClient.AddParameter("can_sit", PolarEnvironment.BoolToEnum(CanSitOn));
                            dbClient.AddParameter("can_lay", PolarEnvironment.BoolToEnum(CanLayOn));
                            dbClient.AddParameter("is_walkable", PolarEnvironment.BoolToEnum(CanStandOn));
                            dbClient.AddParameter("sprite_id", SpriteId);
                            dbClient.AddParameter("description", Description);
                            dbClient.AddParameter("specialtype", SpecialType);
                            dbClient.AddParameter("customparams", CustomParams);
                            dbClient.AddParameter("excludeddynamic", PolarEnvironment.BoolToEnum(ExcludedDynamic));
                            dbClient.AddParameter("adurl", AdURL);
                            dbClient.AddParameter("furniline", FurniLine);
                            dbClient.RunQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        #endregion
                    }
                    //log.Info("Downloaded Room Items");
                    Out.WriteLine("Elementos de la habitación descargados.", "Polar.HabboHotel", ConsoleColor.DarkGray);
                }

                if (downloadWallList.ToList().Count > 0)
                {
                    foreach (var downloadWallItem in downloadWallList)
                    {
                        #region Set Variables
                        try
                        {
                            ItemName = downloadWallItem
                                .Attribute("classname")
                                .Value;
                            SpriteId = Convert.ToInt32(downloadWallItem
                                .Attribute("id")
                                .Value);
                            PublicName = downloadWallItem
                                .Element("name")
                                .Value;
                            Description = downloadWallItem
                                .Element("description")
                                .Value;
                            SpecialType = Convert.ToInt32(downloadWallItem
                                .Element("specialtype")
                                .Value);
                            FurniLine = downloadWallItem
                                .Element("furniline")
                                .Value;
                            Type = "i";
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        #endregion

                        #region Insert Query
                        try
                        {
                            dbClient.SetQuery("INSERT INTO `furniture_new`" +
                                "(id,item_name,public_name,type,width,length,can_sit,can_lay,is_walkable,sprite_id,description,specialtype,customparams,excludeddynamic,adurl,furniline) VALUES " +
                                "(@id,@item_name,@public_name,@type,@width,@length,@can_sit,@can_lay,@is_walkable,@sprite_id,@description,@specialtype,@customparams,@excludeddynamic,@adurl,@furniline) ON DUPLICATE KEY UPDATE " +
                                "id = VALUES(id)," +
                                "item_name = VALUES(item_name)," +
                                "public_name = VALUES(public_name)," +
                                "type = VALUES(type)," +
                                "width = VALUES(width)," +
                                "length = VALUES(length)," +
                                "can_sit = VALUES(can_sit)," +
                                "can_lay = VALUES(can_lay)," +
                                "is_walkable = VALUES(is_walkable)," +
                                "sprite_id = VALUES(sprite_id)," +
                                "description = VALUES(description)," +
                                "specialtype = VALUES(specialtype)," +
                                "customparams = VALUES(customparams)," +
                                "excludeddynamic = VALUES(excludeddynamic)," +
                                "adurl = VALUES(adurl)," +
                                "furniline = VALUES(furniline);");
                            dbClient.AddParameter("id", (100000 + SpriteId));
                            dbClient.AddParameter("item_name", ItemName);
                            dbClient.AddParameter("public_name", PublicName);
                            dbClient.AddParameter("type", Type);
                            dbClient.AddParameter("width", 0);
                            dbClient.AddParameter("length", 0);
                            dbClient.AddParameter("can_sit", 0);
                            dbClient.AddParameter("can_lay", 0);
                            dbClient.AddParameter("is_walkable", 0);
                            dbClient.AddParameter("sprite_id", SpriteId);
                            dbClient.AddParameter("description", Description);
                            dbClient.AddParameter("specialtype", SpecialType);
                            dbClient.AddParameter("customparams", "");
                            dbClient.AddParameter("excludeddynamic", 0);
                            dbClient.AddParameter("adurl", AdURL);
                            dbClient.AddParameter("furniline", FurniLine);
                            dbClient.RunQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        #endregion
                    }
                    //log.Info("Elementos de pared descargados");
                    Out.WriteLine("Elementos de pared descargados.", "Polar.HabboHotel", ConsoleColor.DarkGray);
                }
            }
            #endregion
        }

        public void UpdateFurniSpecial()
        {
            XDocument xmlFile = XDocument.Load(@"http://localhost/furnidata_xml.xml");
            var query = from c in xmlFile.Descendants("roomitemtypes").Descendants("furnitype") select c;
            var query2 = from c in xmlFile.Descendants("wallitemtypes").Descendants("furnitype") select c;

            #region Room Items
            try
            {
                foreach (XElement book in query)
                {
                    string SpriteId = book.Attribute("id").Value;
                    book.Element("offerid").Value = SpriteId;
                    book.Element("bc").Value = "1";
                    book.Element("buyout").Value = "1";
                    book.Element("rentofferid").Value = "-1";
                    book.Element("rentbuyout").Value = "0";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            log.Info("Updated furnidata special for Room Items");
            #endregion

            #region Wall Items
            try
            {
                foreach (XElement book in query2)
                {
                    string SpriteId = book.Attribute("id").Value;
                    book.Element("offerid").Value = (100000 + Convert.ToInt32(SpriteId)).ToString();
                    book.Element("bc").Value = "1";
                    book.Element("buyout").Value = "1";
                    book.Element("rentofferid").Value = "-1";
                    book.Element("rentbuyout").Value = "0";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            log.Info("Updated furnidata special for Wall Items");
            #endregion

            xmlFile.Save("furnidata_updated.xml");
        }

        public void ProductDataMaker()
        {
            #region Variables
            XDocument xmlFile = XDocument.Load(@"furnidata_updated.xml");
            var query = from c in xmlFile.Descendants("roomitemtypes").Descendants("furnitype") select c;
            var query2 = from c in xmlFile.Descendants("wallitemtypes").Descendants("furnitype") select c;

            Dictionary<string, List<ProductData>> Items = new Dictionary<string, List<ProductData>>();
            #endregion

            #region Room Items
            try
            {
                foreach (XElement book in query)
                {
                    string FurniLine = book.Element("furniline").Value.ToLower();
                    string ItemName = book.Attribute("classname").Value;
                    string PublicName = book.Element("name").Value;
                    string Description = book.Element("description").Value;

                    if (!Items.ContainsKey(FurniLine))
                        Items.Add(FurniLine, new List<ProductData>());

                    var Dictionary = Items[FurniLine];

                    ProductData Data = new ProductData(ItemName, PublicName, Description);

                    if (!Dictionary.Contains(Data))
                        Dictionary.Add(Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            log.Info("Created Productdata Dictionary for Room Items");
            #endregion

            #region Wall Items
            try
            {
                foreach (XElement book in query)
                {
                    string FurniLine = book.Element("furniline").Value.ToLower();
                    string ItemName = book.Attribute("classname").Value;
                    string PublicName = book.Element("name").Value;
                    string Description = book.Element("description").Value;

                    if (!Items.ContainsKey(FurniLine))
                        Items.Add(FurniLine, new List<ProductData>());

                    var Dictionary = Items[FurniLine];

                    ProductData Data = new ProductData(ItemName, PublicName, Description);

                    if (!Dictionary.Contains(Data))
                        Dictionary.Add(Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            log.Info("Created Productdata Dictionary for Floor Items");
            #endregion

            #region Write Productdata
            if (Items.Count > 0)
            {
                StringBuilder String = new StringBuilder();

                foreach (var Item in Items)
                {
                    String.Append("[");

                    string FurniLine = Item.Key;
                    List<ProductData> Data = Item.Value;

                    if (Data.Count > 0)
                    {
                        string quote = @"""";

                        int Count = 0;
                        foreach (var ProductData in Data)
                        {
                            Count++;
                            String.Append("[" + quote + ProductData.ItemName + quote + "," + quote + ProductData.PublicName + quote + "," + quote + ProductData.Description + quote + "]");

                            if (Count < Data.Count)
                                String.Append(",");
                        }
                    }

                    String.Append("]\n");
                }

                ConsoleWriter.Writer.WriteProductData(String.ToString());
                log.Info("Successfully wrote new productdata!");
            }
            else
                log.Info("Dictionary has no values in it!");
            #endregion
        }
    }

    public class ProductData
    {
        public string ItemName;
        public string PublicName;
        public string Description;

        public ProductData(string ItemName, string PublicName, string Description)
        {
            this.ItemName = ItemName;
            this.PublicName = PublicName;
            this.Description = Description;
        }
    }
}