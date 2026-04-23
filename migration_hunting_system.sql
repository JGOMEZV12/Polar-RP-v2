-- SQL Migration to support the new Hunting System and Optimizations

-- 1. Change hunt_skins from INT to TEXT to support serialized format (e.g., Bear:1|Lion:5)
ALTER TABLE `rp_stats` MODIFY COLUMN `hunt_skins` TEXT;

-- 2. Add huntzone_enabled to rooms to allow configuring where pets spawn
ALTER TABLE `rooms` ADD COLUMN `huntzone_enabled` ENUM('0', '1') NOT NULL DEFAULT '0';

-- 3. Optimization: Ensure index on bots for room lookup
ALTER TABLE `bots` ADD INDEX `idx_room_id` (`room_id`);
