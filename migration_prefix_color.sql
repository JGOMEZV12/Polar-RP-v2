-- SQL Migration: Add Name Prefix and Name Color support to Users table
-- This adds the necessary columns to store DuckieTM style customization data.

ALTER TABLE `users` ADD COLUMN `prefix` VARCHAR(25) DEFAULT '' AFTER `auth_ticket`;
ALTER TABLE `users` ADD COLUMN `name_color` VARCHAR(25) DEFAULT '' AFTER `prefix`;

-- [Optional] Add permission strings if your emulator uses a permission system for commands
-- INSERT INTO `permissions_commands` (`command`, `rank`) VALUES ('command_prefix', 5);
-- INSERT INTO `permissions_commands` (`command`, `rank`) VALUES ('command_name_color', 5);
