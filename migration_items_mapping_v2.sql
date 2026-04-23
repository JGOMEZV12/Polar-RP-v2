-- Optimized Migration Script (v2): Map old furniture IDs to new items_base IDs
-- This version addresses 'Lock wait timeout exceeded' by adding indexes and increasing timeouts.

-- STEP 1: Increase Lock Wait Timeout Temporarily (In seconds, 300 = 5 minutes)
SET innodb_lock_wait_timeout = 300;

-- STEP 2: Index the columns used in JOINs to make it much faster
-- This speeds up the lookup from thousands of rows down to just a few milliseconds.
CREATE INDEX IF NOT EXISTS idx_furniture_item_name ON furniture(item_name);
CREATE INDEX IF NOT EXISTS idx_items_base_item_name ON items_base(item_name);
CREATE INDEX IF NOT EXISTS idx_items_base_item ON items(base_item);

-- STEP 3: Ensure the emulator is stopped to prevent concurrent locks!
-- (If you can't stop it, try to update in small batches).

-- STEP 4: Run the UPDATE
UPDATE items
INNER JOIN furniture ON items.base_item = furniture.id
INNER JOIN items_base ON furniture.item_name = items_base.item_name
SET items.base_item = items_base.id;

-- STEP 5: Verify if any items were missed (Option C)
SELECT
    i.id AS instance_id,
    i.base_item AS current_base_id,
    f.item_name AS furniture_item_name,
    i.user_id,
    i.room_id
FROM items i
LEFT JOIN furniture f ON i.base_item = f.id
LEFT JOIN items_base ib ON f.item_name = ib.item_name
WHERE ib.id IS NULL;

-- STEP 6: [Optional] Cleanup Indexes if you don't need them anymore
-- DROP INDEX idx_furniture_item_name ON furniture;
-- DROP INDEX idx_items_base_item_name ON items_base;
-- DROP INDEX idx_items_base_item ON items;
