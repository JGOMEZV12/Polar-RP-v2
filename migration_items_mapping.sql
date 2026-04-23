-- Migration Script: Map old furniture IDs to new items_base IDs
-- This script updates the 'item_id' in the 'items' table to match the IDs in the new 'items_base' table.
-- It uses the 'item_name' from the old 'furniture' table as a bridge.

-- 1. [RECOMMENDED] Create a backup of your items table before proceeding
-- CREATE TABLE items_backup_migration AS SELECT * FROM items;

-- 2. Update the item_id in items table
-- We join 'items' with the old 'furniture' table to get the name,
-- then join with the new 'items_base' table to get the new ID.
UPDATE items
INNER JOIN furniture ON items.item_id = furniture.id
INNER JOIN items_base ON furniture.item_name = items_base.item_name
SET items.item_id = items_base.id;

-- 3. IDENTIFY UNMAPPED ITEMS (Option C)
-- Run this query to see which items in your rooms/inventory were NOT updated
-- because their item_name was not found in the new 'items_base' table.
SELECT
    i.id AS instance_id,
    i.item_id AS current_base_id,
    f.item_name AS furniture_item_name,
    i.user_id,
    i.room_id
FROM items i
LEFT JOIN furniture f ON i.item_id = f.id
LEFT JOIN items_base ib ON f.item_name = ib.item_name
WHERE ib.id IS NULL;

-- Note: If you have already updated some items, they might appear in this list
-- if the new ID doesn't exist in the old 'furniture' table.
-- It is best to run the SELECT before the UPDATE to identify potential issues.
