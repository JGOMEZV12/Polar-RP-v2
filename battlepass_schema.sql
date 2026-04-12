CREATE TABLE IF NOT EXISTS `battlepass_seasons` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `chapter` int(11) NOT NULL DEFAULT 1,
  `season` int(11) NOT NULL DEFAULT 1,
  `is_active` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_levels` (
  `level` int(11) NOT NULL,
  `exp_required` int(11) NOT NULL,
  PRIMARY KEY (`level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_rewards` (
  `level` int(11) NOT NULL,
  `normal_reward_type` varchar(50) DEFAULT 'none',
  `normal_reward_value` varchar(255) DEFAULT '',
  `normal_reward_icon` varchar(255) DEFAULT '',
  `vip_reward_type` varchar(50) DEFAULT 'none',
  `vip_reward_value` varchar(255) DEFAULT '',
  `vip_reward_icon` varchar(255) DEFAULT '',
  PRIMARY KEY (`level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_categories` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `icon` varchar(255) DEFAULT '',
  `description` text DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_challenges` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `category_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `icon` varchar(255) DEFAULT '',
  `description` text DEFAULT NULL,
  `xp_reward` int(11) NOT NULL DEFAULT 0,
  `total_progress` int(11) NOT NULL DEFAULT 1,
  `goal_type` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_user_data` (
  `user_id` int(11) NOT NULL,
  `level` int(11) NOT NULL DEFAULT 1,
  `exp` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_user_rewards` (
  `user_id` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `type` enum('normal','vip') NOT NULL,
  PRIMARY KEY (`user_id`,`level`,`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `battlepass_user_challenges` (
  `user_id` int(11) NOT NULL,
  `challenge_id` int(11) NOT NULL,
  `current_progress` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`user_id`,`challenge_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert a default season
INSERT INTO `battlepass_seasons` (`chapter`, `season`, `is_active`) VALUES (1, 1, 1);

-- Insert some levels
INSERT INTO `battlepass_levels` (`level`, `exp_required`) VALUES
(1, 0), (2, 100), (3, 200), (4, 300), (5, 400), (6, 500), (7, 600), (8, 700), (9, 800), (10, 1000);

-- Insert some dummy rewards (icons would be assets in Nitro)
INSERT INTO `battlepass_rewards` (`level`, `normal_reward_type`, `normal_reward_value`, `normal_reward_icon`, `vip_reward_type`, `vip_reward_value`, `vip_reward_icon`) VALUES
(1, 'credits', '1000', 'https://example.com/icons/credits.png', 'credits', '2000', 'https://example.com/icons/credits_vip.png'),
(2, 'duckets', '500', 'https://example.com/icons/duckets.png', 'duckets', '1000', 'https://example.com/icons/duckets_vip.png'),
(3, 'diamonds', '5', 'https://example.com/icons/diamonds.png', 'diamonds', '10', 'https://example.com/icons/diamonds_vip.png');
