-- BattlePass Configuration SQL
-- This script populates the BattlePass system with a sample Season, Levels, Rewards and Challenges based on server RP functions.

-- 1. Create a Season
INSERT INTO `battlepass_seasons` (`id`, `chapter`, `season`, `is_active`) VALUES
(1, 1, 1, 1);

-- 2. Define Levels (1 to 20)
-- Level 1 starts with 0 EXP. Level 2 requires 1000, and so on.
INSERT INTO `battlepass_levels` (`level`, `exp_required`) VALUES
(1, 0), (2, 1000), (3, 2000), (4, 3000), (5, 4000),
(6, 5500), (7, 7000), (8, 8500), (9, 10000), (10, 12000),
(11, 14500), (12, 17000), (13, 19500), (14, 22000), (15, 25000),
(16, 28500), (17, 32000), (18, 35500), (19, 39000), (20, 43000);

-- 3. Define Rewards for each level
-- Reward types supported: credits, diamonds, duckets, item (base_id), badge (code), vip (days)
INSERT INTO `battlepass_rewards` (`level`, `normal_reward_type`, `normal_reward_value`, `normal_reward_icon`, `vip_reward_type`, `vip_reward_value`, `vip_reward_icon`) VALUES
(1, 'credits', '500', 'credits_icon', 'diamonds', '50', 'diamonds_icon'),
(2, 'duckets', '100', 'duckets_icon', 'credits', '1000', 'credits_icon'),
(3, 'badge', 'BP_LVL3', 'badge_icon', 'badge', 'BP_VIP_LVL3', 'badge_icon'),
(4, 'credits', '750', 'credits_icon', 'item', '210', 'furni_icon'), -- Change 210 for a valid base_item id
(5, 'diamonds', '20', 'diamonds_icon', 'vip', '1', 'vip_icon'),
(6, 'credits', '1000', 'credits_icon', 'diamonds', '100', 'diamonds_icon'),
(7, 'duckets', '200', 'duckets_icon', 'credits', '2000', 'credits_icon'),
(8, 'item', '211', 'furni_icon', 'item', '212', 'furni_icon'),
(9, 'credits', '1500', 'credits_icon', 'diamonds', '150', 'diamonds_icon'),
(10, 'badge', 'BP_HALF', 'badge_icon', 'vip', '3', 'vip_icon'),
(11, 'credits', '2000', 'credits_icon', 'diamonds', '200', 'diamonds_icon'),
(12, 'duckets', '500', 'duckets_icon', 'credits', '3000', 'credits_icon'),
(13, 'item', '213', 'furni_icon', 'item', '214', 'furni_icon'),
(14, 'credits', '2500', 'credits_icon', 'diamonds', '250', 'diamonds_icon'),
(15, 'diamonds', '50', 'diamonds_icon', 'vip', '7', 'vip_icon'),
(16, 'credits', '3000', 'credits_icon', 'diamonds', '300', 'diamonds_icon'),
(17, 'exp', '5000', 'exp_icon', 'exp', '10000', 'exp_icon'),
(18, 'duckets', '1000', 'duckets_icon', 'credits', '5000', 'credits_icon'),
(19, 'item', '215', 'furni_icon', 'item', '216', 'furni_icon'),
(20, 'vip', '1', 'vip_icon', 'vip', '30', 'vip_icon');

-- 4. Define Categories for Challenges
INSERT INTO `battlepass_categories` (`id`, `name`, `icon`, `description`) VALUES
(1, 'Combate y Acción', 'combat_icon', 'Desafíos relacionados con peleas y el bajo mundo.'),
(2, 'Vida y Trabajo', 'work_icon', 'Desafíos sobre empleos y superación personal.'),
(3, 'Crimen y Mafia', 'crime_icon', 'Solo para los más audaces del servidor.'),
(4, 'Naturaleza y Caza', 'hunt_icon', 'Explora y cosecha lo que la ciudad ofrece.');

-- 5. Define Challenges
-- goal_type must match those implemented in C#
INSERT INTO `battlepass_challenges` (`id`, `category_id`, `name`, `icon`, `description`, `xp_reward`, `total_progress`, `goal_type`) VALUES
-- Categoría 1: Combate
(1, 1, 'Puños de Hierro', 'punch_icon', 'Dale un puñetazo a 50 ciudadanos.', 500, 50, 'punch_user'),
(2, 1, 'Asesino a Sueldo', 'kill_icon', 'Elimina a 10 personas en combate.', 1000, 10, 'kill_user'),
(3, 1, 'Caza-Recompensas', 'bounty_icon', 'Cobra una recompensa de un ciudadano.', 800, 1, 'claim_bounty'),

-- Categoría 2: Vida y Trabajo
(4, 2, 'Empleado del Mes', 'work_icon', 'Completa 20 turnos de trabajo.', 1200, 20, 'work_cycle'),
(5, 2, 'Cuerpo Sano', 'gym_icon', 'Entrena tu fuerza 30 veces en el gimnasio.', 600, 30, 'train_strength'),
(6, 2, 'Resistencia Infinita', 'gym_icon', 'Entrena tu stamina 30 veces en el gimnasio.', 600, 30, 'train_stamina'),
(7, 2, 'Mente Brillante', 'school_icon', 'Mejora tu inteligencia 15 veces.', 600, 15, 'learn_intelligence'),
(8, 2, 'Buen Samaritano', 'heal_icon', 'Cura a 10 ciudadanos como médico.', 700, 10, 'heal_user'),
(9, 2, 'Orden Público', 'police_icon', 'Arresta a 5 delincuentes.', 900, 5, 'arrest_user'),

-- Categoría 3: Crimen
(10, 3, 'Amigo de lo Ajeno', 'atm_icon', 'Roba un cajero automático con éxito.', 500, 1, 'rob_atm'),
(11, 3, 'Asalto a la Tienda', 'store_icon', 'Roba una tienda local.', 600, 1, 'rob_store'),
(12, 3, 'Gran Atraco', 'bank_icon', 'Participa en un robo al banco.', 2000, 1, 'rob_bank'),
(13, 3, 'Territorio Marcado', 'turf_icon', 'Captura un territorio para tu pandilla.', 1500, 1, 'capture_turf'),
(14, 3, 'Fuga de Prisión', 'jail_icon', 'Ayuda o escapa en un jailbreak.', 1500, 1, 'jailbreak'),
(15, 3, 'Médico de Guerra', 'gang_heal_icon', 'Cura a un miembro de tu pandilla.', 400, 5, 'gang_heal'),
(20, 3, 'Químico Clandestino (Coca)', 'cocaine_icon', 'Procesa 5 bolsas de cocaína.', 800, 5, 'process_cocaine'),
(21, 3, 'Químico Clandestino (Hero)', 'heroine_icon', 'Procesa 5 dosis de heroína.', 800, 5, 'process_heroine'),
(22, 3, 'Cultivador Ilegal', 'weed_icon', 'Procesa 5 bolsas de marihuana.', 600, 5, 'process_weed'),

-- Categoría 4: Naturaleza
(16, 4, 'Granjero Novato', 'seed_icon', 'Planta 20 semillas en la granja.', 400, 20, 'plant_seed'),
(17, 4, 'Mano Verde', 'water_icon', 'Riega tus plantas 50 veces.', 400, 50, 'water_plant'),
(18, 4, 'Cosecha Abundante', 'harvest_icon', 'Cosecha 30 plantas maduras.', 600, 30, 'harvest_plant'),
(19, 4, 'Gran Cazador', 'hunt_pet_icon', 'Caza 10 animales salvajes en la zona de caza.', 1200, 10, 'hunt_pet'),
(23, 4, 'Comerciante de Flores', 'sell_icon', 'Vende tus plantas cosechadas al mercado.', 500, 1, 'sell_plants'),
(24, 2, 'Minero de Corazón', 'mine_icon', 'Pica piedras en la mina 20 veces.', 800, 20, 'mine_rock'),
(25, 2, 'Buen Provecho', 'eat_icon', 'Come algo para saciar tu hambre.', 300, 5, 'eat_food'),
(26, 2, 'Sed Insaciable', 'drink_icon', 'Bebe algo para recuperar energía.', 300, 5, 'drink_item');
