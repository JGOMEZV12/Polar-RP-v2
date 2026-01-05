using System;
using System.Data;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.Database.Interfaces;


namespace Polar.HabboHotel.Items
{
    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int roomId, Room room)
        {
            var items = new List<Item>();
            DataTable table;

            using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT i.*, COALESCE(ig.group_id, 0) AS group_id FROM items i LEFT JOIN items_groups ig ON i.id = ig.id WHERE i.room_id = @rid");
                dbClient.AddParameter("rid", roomId);
                table = dbClient.getTable();
            }

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (PolarEnvironment.GetGame().GetItemManager().GetItem(int.Parse(row["base_item"].ToString()), out var data))
                    {
                        items.Add(new Item(int.Parse(row["id"].ToString()), int.Parse(row["room_id"].ToString()), int.Parse(row["base_item"].ToString()), Convert.ToString(row["extra_data"]),
                            int.Parse(row["x"].ToString()), int.Parse(row["y"].ToString()), Convert.ToDouble(row["z"]), int.Parse(row["rot"].ToString()), int.Parse(row["user_id"].ToString()),
                            int.Parse(row["group_id"].ToString()), int.Parse(row["limited_number"].ToString()), int.Parse(row["limited_stack"].ToString()), Convert.ToString(row["wall_pos"]), room));
                    }
                }
            }
            return items;
        }

        public static List<Item> GetItemsForUser(int UserId)
        {
            DataTable Items = null;
            List<Item> I = new List<Item>();

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", UserId);
                Items = dbClient.getTable();

                if (Items != null)
                {
                    foreach (DataRow Row in Items.Rows)
                    {
                        ItemData Data = null;

                        if (PolarEnvironment.GetGame().GetItemManager().GetItem(int.Parse(Row["base_item"].ToString()), out Data))
                        {
                            I.Add(new Item(int.Parse(Row["id"].ToString()), int.Parse(Row["room_id"].ToString()), int.Parse(Row["base_item"].ToString()), Convert.ToString(Row["extra_data"]),
                                int.Parse(Row["x"].ToString()), int.Parse(Row["y"].ToString()), Convert.ToDouble(Row["z"]), int.Parse(Row["rot"].ToString()), int.Parse(Row["user_id"].ToString()),
                                int.Parse(Row["group_id"].ToString()), int.Parse(Row["limited_number"].ToString()), int.Parse(Row["limited_stack"].ToString()), Convert.ToString(Row["wall_pos"])));
                        }
                        else
                        {
                            // Item data does not exist anymore.
                        }
                    }
                }
            }
            return I;
        }
    }
}
