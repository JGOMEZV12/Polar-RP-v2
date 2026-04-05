using System;
using System.Collections.Generic;
using System.Data;
using Polar.Database.Interfaces;

namespace Polar.Core
{
    public static class DatabaseCompatibility
    {
        public static string FurnitureTable { get; private set; } = "furniture";
        public static string CatalogItemsTable { get; private set; } = "catalog_items";
        public static string CatalogPagesTable { get; private set; } = "catalog_pages";

        // Mappings for Furniture/ItemsBase
        public static string FurniIdColumn { get; private set; } = "id";
        public static string FurniSpriteIdColumn { get; private set; } = "sprite_id";
        public static string FurniItemNameColumn { get; private set; } = "item_name";
        public static string FurniTypeColumn { get; private set; } = "type";

        // Mappings for Catalog
        public static string CatalogItemIdColumn { get; private set; } = "id";
        public static string CatalogItemBaseIdColumn { get; private set; } = "item_id";
        public static string CatalogItemCostDucketsColumn { get; private set; } = "cost_pixels";
        public static string CatalogItemCostDiamondsColumn { get; private set; } = "cost_diamonds";
        public static string CatalogPageLayoutColumn { get; private set; } = "page_layout";
        public static string CatalogPageStrings1Column { get; private set; } = "page_strings_1";
        public static string CatalogPageStrings2Column { get; private set; } = "page_strings_2";
        public static string CatalogPageEnabledColumn { get; private set; } = "enabled";
        public static string CatalogPageVisibleColumn { get; private set; } = "visible";
        public static string CatalogItemOfferActiveColumn { get; private set; } = "offer_active";
        public static string CatalogBotPresetsTable { get; private set; } = "catalog_bot_presets";
        public static string CatalogClothingTable { get; private set; } = "catalog_clothing";
        public static string ItemsTable { get; private set; } = "items";
        public static string ItemsBaseItemColumn { get; private set; } = "base_item";
        public static string ItemsGroupIdColumn { get; private set; } = "group_id";
        public static string MarketplaceDataTable { get; private set; } = "catalog_marketplace_data";

        public static void Initialize()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // Detect Furniture table
                dbClient.SetQuery("SHOW TABLES LIKE 'items_base'");
                if (dbClient.findsResult())
                {
                    FurnitureTable = "items_base";
                }
                else
                {
                    dbClient.SetQuery("SHOW TABLES LIKE 'furniture'");
                    if (dbClient.findsResult())
                        FurnitureTable = "furniture";
                }

                // Determine internal ItemName column (classname in Arcturus, item_name in Plus)
                dbClient.SetQuery($"SHOW COLUMNS FROM `{FurnitureTable}` LIKE 'classname'");
                if (dbClient.findsResult())
                    FurniItemNameColumn = "classname";
                else
                {
                    dbClient.SetQuery($"SHOW COLUMNS FROM `{FurnitureTable}` LIKE 'item_name'");
                    if (dbClient.findsResult())
                        FurniItemNameColumn = "item_name";
                }

                // Detect Catalog Items Columns
                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_items` LIKE 'cost_duckets'");
                if (dbClient.findsResult())
                    CatalogItemCostDucketsColumn = "cost_duckets";

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_items` LIKE 'cost_points'");
                if (dbClient.findsResult())
                    CatalogItemCostDiamondsColumn = "cost_points";

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_items` LIKE 'item_ids'");
                if (dbClient.findsResult())
                    CatalogItemBaseIdColumn = "item_ids";

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_items` LIKE 'offer_active'");
                if (dbClient.findsResult())
                    CatalogItemOfferActiveColumn = "offer_active";
                else
                    CatalogItemOfferActiveColumn = "1"; // Hack for query if column doesn't exist

                // Detect Catalog Pages Columns
                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'layout'");
                if (dbClient.findsResult())
                    CatalogPageLayoutColumn = "layout";

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'page_headline'");
                if (dbClient.findsResult())
                    CatalogPageStrings1Column = "page_headline";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'page_strings_1'");
                    if (dbClient.findsResult())
                        CatalogPageStrings1Column = "page_strings_1";
                }

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'page_teaser'");
                if (dbClient.findsResult())
                    CatalogPageStrings2Column = "page_teaser";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'page_strings_2'");
                    if (dbClient.findsResult())
                        CatalogPageStrings2Column = "page_strings_2";
                }

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'enabled'");
                if (dbClient.findsResult())
                    CatalogPageEnabledColumn = "enabled";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'is_enabled'");
                    if (dbClient.findsResult())
                        CatalogPageEnabledColumn = "is_enabled";
                }

                dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'visible'");
                if (dbClient.findsResult())
                    CatalogPageVisibleColumn = "visible";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `catalog_pages` LIKE 'is_visible'");
                    if (dbClient.findsResult())
                        CatalogPageVisibleColumn = "is_visible";
                }

                // Detect Clothing Table
                dbClient.SetQuery("SHOW TABLES LIKE 'catalog_clothing'");
                if (dbClient.findsResult())
                    CatalogClothingTable = "catalog_clothing";
                else
                {
                    dbClient.SetQuery("SHOW TABLES LIKE 'clothing'");
                    if (dbClient.findsResult())
                        CatalogClothingTable = "clothing";
                }

                // Detect Bot Presets Table
                dbClient.SetQuery("SHOW TABLES LIKE 'catalog_bot_presets'");
                if (dbClient.findsResult())
                    CatalogBotPresetsTable = "catalog_bot_presets";
                else
                {
                    dbClient.SetQuery("SHOW TABLES LIKE 'catalog_bots'");
                    if (dbClient.findsResult())
                        CatalogBotPresetsTable = "catalog_bots";
                }

                // Detect Items Table Columns
                dbClient.SetQuery("SHOW COLUMNS FROM `items` LIKE 'base_item'");
                if (dbClient.findsResult())
                    ItemsBaseItemColumn = "base_item";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `items` LIKE 'item_id'");
                    if (dbClient.findsResult())
                        ItemsBaseItemColumn = "item_id";
                }

                dbClient.SetQuery("SHOW COLUMNS FROM `items` LIKE 'group_id'");
                if (dbClient.findsResult())
                    ItemsGroupIdColumn = "group_id";
                else
                {
                    dbClient.SetQuery("SHOW COLUMNS FROM `items` LIKE 'guild_id'");
                    if (dbClient.findsResult())
                        ItemsGroupIdColumn = "guild_id";
                }

                // Marketplace data table
                dbClient.SetQuery("SHOW TABLES LIKE 'catalog_marketplace_data'");
                if (dbClient.findsResult())
                    MarketplaceDataTable = "catalog_marketplace_data";
                else
                {
                    dbClient.SetQuery("SHOW TABLES LIKE 'catalog_marketplace_items'");
                    if (dbClient.findsResult())
                        MarketplaceDataTable = "catalog_marketplace_items";
                }
            }
        }

        public static bool ColumnExists(string table, string column)
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery($"SHOW COLUMNS FROM `{table}` LIKE @column");
                dbClient.AddParameter("column", column);
                return dbClient.findsResult();
            }
        }
    }
}
