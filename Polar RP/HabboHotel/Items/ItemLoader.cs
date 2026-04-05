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
                dbClient.SetQuery($"SELECT i.*, COALESCE(ig.{Polar.Core.DatabaseCompatibility.ItemsGroupIdColumn}, 0) AS group_id FROM `{Polar.Core.DatabaseCompatibility.ItemsTable}` i LEFT JOIN items_groups ig ON i.id = ig.id WHERE i.room_id = @rid");
                dbClient.AddParameter("rid", roomId);
                table = dbClient.getTable();
            }

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    int baseId = int.Parse(row[Polar.Core.DatabaseCompatibility.ItemsBaseItemColumn].ToString());
                    if (PolarEnvironment.GetGame().GetItemManager().GetItem(baseId, out var data))
                    {
                        items.Add(new Item(int.Parse(row["id"].ToString()), int.Parse(row["room_id"].ToString()), baseId, Convert.ToString(row["extra_data"]),
                            int.Parse(row["x"].ToString()), int.Parse(row["y"].ToString()), Convert.ToDouble(row["z"]), int.Parse(row["rot"].ToString()), int.Parse(row["user_id"].ToString()),
                            int.Parse(row["group_id"].ToString()),
                            row.Table.Columns.Contains("limited_number") ? int.Parse(row["limited_number"].ToString()) : 0,
                            row.Table.Columns.Contains("limited_stack") ? int.Parse(row["limited_stack"].ToString()) : 0,
                            Convert.ToString(row["wall_pos"]), room));
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
                dbClient.SetQuery($"SELECT i.*, COALESCE(ig.{Polar.Core.DatabaseCompatibility.ItemsGroupIdColumn}, 0) AS group_id FROM `{Polar.Core.DatabaseCompatibility.ItemsTable}` i LEFT OUTER JOIN items_groups ig ON i.id = ig.id WHERE i.room_id = 0 AND i.user_id = @uid;");
                dbClient.AddParameter("uid", UserId);
                Items = dbClient.getTable();

                if (Items != null)
                {
                    foreach (DataRow Row in Items.Rows)
                    {
                        ItemData Data = null;
                        int baseId = int.Parse(Row[Polar.Core.DatabaseCompatibility.ItemsBaseItemColumn].ToString());

                        if (PolarEnvironment.GetGame().GetItemManager().GetItem(baseId, out Data))
                        {
                            I.Add(new Item(int.Parse(Row["id"].ToString()), int.Parse(Row["room_id"].ToString()), baseId, Convert.ToString(Row["extra_data"]),
                                int.Parse(Row["x"].ToString()), int.Parse(Row["y"].ToString()), Convert.ToDouble(Row["z"]), int.Parse(Row["rot"].ToString()), int.Parse(Row["user_id"].ToString()),
                                int.Parse(Row["group_id"].ToString()),
                                Row.Table.Columns.Contains("limited_number") ? int.Parse(Row["limited_number"].ToString()) : 0,
                                Row.Table.Columns.Contains("limited_stack") ? int.Parse(Row["limited_stack"].ToString()) : 0,
                                Convert.ToString(Row["wall_pos"])));
                        }
                    }
                }
            }
            return I;
        }
    }
}
