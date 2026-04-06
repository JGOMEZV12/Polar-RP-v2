using System;
using Polar;
using Polar.Core;
namespace Polar.HabboHotel.Items
{
    public enum InteractionType
    {
        NONE,
        BOT,
        MAGICEGG,
        MAGICCHEST,
        GATE,
        POSTIT,
        MOODLIGHT,
        TROPHY,
        BED,
        BEDEFFECT,
        SCOREBOARD,
        VENDING_MACHINE,
        ALERT,
        ONE_WAY_GATE,
        LOVE_SHUFFLER,
        HABBO_WHEEL,
        DICE,
        BOTTLE,
        HOPPER,
        TELEPORT,
        POOL,
        club_1_month,
        club_3_month,
        club_6_month,
        CLUB_VIP,
        CLUB_VIP2,
        ROLLER,
        FOOTBALL_GATE,
        PET,
        ICE_SKATES,
        NORMAL_SKATES,
        lowpool,
        haloweenpool,
        FOOTBALL,
        FOOTBALL_GOAL_GREEN,
        FOOTBALL_GOAL_YELLOW,
        FOOTBALL_GOAL_BLUE,
        FOOTBALL_GOAL_RED,
        footballcountergreen,
        footballcounteryellow,
        footballcounterblue,
        footballcounterred,
        banzaigateblue,
        banzaigatered,
        banzaigateyellow,
        banzaigategreen,
        banzaifloor,
        banzaiscoreblue,
        banzaiscorered,
        banzaiscoreyellow,
        banzaiscoregreen,
        banzaicounter,
        banzaitele,
        banzaipuck,
        banzaipyramid,
        freezetimer,
        freezeexit,
        freezeredcounter,
        freezebluecounter,
        freezeyellowcounter,
        freezegreencounter,
        FREEZE_YELLOW_GATE,
        FREEZE_RED_GATE,
        FREEZE_GREEN_GATE,
        FREEZE_BLUE_GATE,
        FREEZE_TILE_BLOCK,
        FREEZE_TILE,
        JUKEBOX,
        MUSIC_DISC,
        PUZZLE_BOX,
        TONER,


        PRESSURE_PAD,
        TRAINER_GYM,

        WF_FLOOR_SWITCH_1,
        WF_FLOOR_SWITCH_2,

        GIFT,
        BACKGROUND,
        MANNEQUIN,
        GATE_VIP,
        GUILD_ITEM,
        GUILD_GATE,
        GUILD_FORUM,
        SLIDING_DOORS,

        TENT,
        TENT_SMALL,
        BADGE_DISPLAY,
        STACKTOOL,
        TELEVISION,

        WIRED_EFFECT,
        WIRED_TRIGGER,
        WIRED_CONDITION,
        WIRED_HIGHSCORE,

        WALLPAPER,
        FLOOR,
        LANDSCAPE,

        BADGE,
        CRACKABLE_EGG,
        EFFECT,
        DEAL,

        HORSE_SADDLE_1,
        HORSE_SADDLE_2,
        HORSE_HAIRSTYLE,
        HORSE_BODY_DYE,
        HORSE_HAIR_DYE,

        GNOME_BOX,
        PURCHASABLE_CLOTHING,
        PET_BREEDING_BOX,
        ARROW,
        ARROW2,
        LOVELOCK,
        MONSTERPLANT_SEED,
        CANNON,
        COUNTER,
        CAMERA_PICTURE,
        PINATA,
        INFO_TERMINAL,
        FX_PROVIDER,
        PINATATRIGGERED,
        DA_PROVIDER,
        HI_PROVIDER,
        HCGATE,
        MUTESIGNAL,

        ATM_MACHINE,
        PEPSIMACHINE,
        AGUAENERGY,
        CARAMELOMACHINE,
        CAJERORUBY,
        COMIDAMACHINE,
        RP_NUKE,
        TRASH_CAN,
        HOUSE_SIGN,
        BASURERO,
        TRAGAMONEDAS,
        WEEDMATERIA,
        WEEDPORRO,
        Cocaina,
        HEROINA,
        BASURAENTREGA,
        MINERIA,
        INFORMATION_TERMINAL,
        WHISPER_TILE,
        RENTABLE_SPACE,
        DELIVERY_BOX,
        SHOWER,
        CAGAR,
        FARMING,
        CRAFTING,
        COMODIN,
        CARNEW
    }


    public class InteractionTypes
    {
        public static InteractionType GetTypeFromString(string pType)
        {
            switch (pType.ToLower())
            {
                case "":
                case "default":
                    return InteractionType.NONE;
                case "gate":
                    return InteractionType.GATE;
                case "postit":
                    return InteractionType.POSTIT;
                case "dimmer":
                    return InteractionType.MOODLIGHT;
                case "trophy":
                    return InteractionType.TROPHY;
                case "bed":
                    return InteractionType.BED;
                case "bedeffect":
                    return InteractionType.BEDEFFECT;
                case "scoreboard":
                    return InteractionType.SCOREBOARD;
                case "vendingmachine":
                    return InteractionType.VENDING_MACHINE;
                case "alert":
                    return InteractionType.ALERT;
                case "onewaygate":
                    return InteractionType.ONE_WAY_GATE;
                case "loveshuffler":
                    return InteractionType.LOVE_SHUFFLER;
                case "habbowheel":
                    return InteractionType.HABBO_WHEEL;
                case "dice":
                    return InteractionType.DICE;
                case "hopper":
                    return InteractionType.HOPPER;
                case "bottle":
                    return InteractionType.BOTTLE;
                case "teleport":
                    return InteractionType.TELEPORT;
                case "pool":
                    return InteractionType.POOL;
                case "roller":
                    return InteractionType.ROLLER;
                case "fbgate":
                    return InteractionType.FOOTBALL_GATE;
                case "pet":
                case "habbo_pet":
                    return InteractionType.PET;
                case "iceskates":
                    return InteractionType.ICE_SKATES;
                case "rollerskate":
                    return InteractionType.NORMAL_SKATES;
                case "lowpool":
                    return InteractionType.lowpool;
                case "haloweenpool":
                    return InteractionType.haloweenpool;
                case "ball":
                case "football":
                    return InteractionType.FOOTBALL;

                case "green_goal":
                    return InteractionType.FOOTBALL_GOAL_GREEN;
                case "yellow_goal":
                    return InteractionType.FOOTBALL_GOAL_YELLOW;
                case "red_goal":
                    return InteractionType.FOOTBALL_GOAL_RED;
                case "blue_goal":
                    return InteractionType.FOOTBALL_GOAL_BLUE;

                case "green_score":
                    return InteractionType.footballcountergreen;
                case "yellow_score":
                    return InteractionType.footballcounteryellow;
                case "blue_score":
                    return InteractionType.footballcounterblue;
                case "red_score":
                    return InteractionType.footballcounterred;

                case "bb_blue_gate":
                    return InteractionType.banzaigateblue;
                case "bb_red_gate":
                    return InteractionType.banzaigatered;
                case "bb_yellow_gate":
                    return InteractionType.banzaigateyellow;
                case "bb_green_gate":
                    return InteractionType.banzaigategreen;
                case "bb_patch":
                    return InteractionType.banzaifloor;

                case "bb_blue_score":
                    return InteractionType.banzaiscoreblue;
                case "bb_red_score":
                    return InteractionType.banzaiscorered;
                case "bb_yellow_score":
                    return InteractionType.banzaiscoreyellow;
                case "bb_green_score":
                    return InteractionType.banzaiscoregreen;

                case "banzaicounter":
                    return InteractionType.banzaicounter;
                case "bb_teleport":
                    return InteractionType.banzaitele;
                case "banzaipuck":
                    return InteractionType.banzaipuck;
                case "bb_pyramid":
                    return InteractionType.banzaipyramid;

                case "freezetimer":
                    return InteractionType.freezetimer;
                case "freezeexit":
                    return InteractionType.freezeexit;
                case "freezeredcounter":
                    return InteractionType.freezeredcounter;
                case "freezebluecounter":
                    return InteractionType.freezebluecounter;
                case "freezeyellowcounter":
                    return InteractionType.freezeyellowcounter;
                case "freezegreencounter":
                    return InteractionType.freezegreencounter;
                case "freezeyellowgate":
                    return InteractionType.FREEZE_YELLOW_GATE;
                case "freezeredgate":
                    return InteractionType.FREEZE_RED_GATE;
                case "freezegreengate":
                    return InteractionType.FREEZE_GREEN_GATE;
                case "freezebluegate":
                    return InteractionType.FREEZE_BLUE_GATE;
                case "freezetileblock":
                    return InteractionType.FREEZE_TILE_BLOCK;
                case "freezetile":
                    return InteractionType.FREEZE_TILE;

                case "jukebox":
                    return InteractionType.JUKEBOX;
                case "musicdisc":
                    return InteractionType.MUSIC_DISC;
                case "trainergym":
                    return InteractionType.TRAINER_GYM;
                case "pressure_pad":
                    return InteractionType.PRESSURE_PAD;
                case "wf_floor_switch1":
                    return InteractionType.WF_FLOOR_SWITCH_1;
                case "wf_floor_switch2":
                    return InteractionType.WF_FLOOR_SWITCH_2;
                case "puzzlebox":
                    return InteractionType.PUZZLE_BOX;
                case "water":
                    return InteractionType.POOL;
                case "gift":
                    return InteractionType.GIFT;
                case "background":
                    return InteractionType.BACKGROUND;
                case "mannequin":
                    return InteractionType.MANNEQUIN;
                case "vip_gate":
                    return InteractionType.GATE_VIP;
                case "roombg":
                    return InteractionType.TONER;
                case "gld_item":
                    return InteractionType.GUILD_ITEM;
                case "gld_gate":
                    return InteractionType.GUILD_GATE;
                case "guild_forum":
                    return InteractionType.GUILD_FORUM;
                case "sliding_doors":
                    return InteractionType.SLIDING_DOORS;
                case "tent":
                    return InteractionType.TENT;
                case "tent_small":
                case "bedtent":
                    return InteractionType.TENT_SMALL;

                case "badge_display":
                    return InteractionType.BADGE_DISPLAY;
                case "stacktool":
                    return InteractionType.STACKTOOL;
                case "television":
                    return InteractionType.TELEVISION;


                case "wired_effect":
                    return InteractionType.WIRED_EFFECT;
                case "wired_trigger":
                    return InteractionType.WIRED_TRIGGER;
                case "wired_condition":
                    return InteractionType.WIRED_CONDITION;
                case "wiredhighscore":
                    return InteractionType.WIRED_HIGHSCORE;
                case "wf_highscore":
                    return InteractionType.WIRED_HIGHSCORE;
                case "clothing":
                case "habbo_clothing":
                    return InteractionType.PURCHASABLE_CLOTHING;
                case "mystery_box":
                    return InteractionType.MAGICCHEST;
                case "guild_item":
                case "guild_furni":
                    return InteractionType.GUILD_ITEM;
                case "guild_forum":
                    return InteractionType.GUILD_FORUM;
                case "guild_gate":
                    return InteractionType.GUILD_GATE;
                case "roomads":
                case "external_image":
                    return InteractionType.BACKGROUND;
                case "totem":
                    return InteractionType.NONE; // Generic
                case "monsterplant_seed":
                    return InteractionType.MONSTERPLANT_SEED;

                case "floor":
                    return InteractionType.FLOOR;
                case "wallpaper":
                    return InteractionType.WALLPAPER;
                case "landscape":
                    return InteractionType.LANDSCAPE;

                case "badge":
                    return InteractionType.BADGE;

                case "crackable":
                case "crackable_egg":
                    return InteractionType.CRACKABLE_EGG;
                case "effect":
                    return InteractionType.EFFECT;
                case "deal":
                    return InteractionType.DEAL;

                case "horse_saddle_1":
                    return InteractionType.HORSE_SADDLE_1;
                case "horse_saddle_2":
                    return InteractionType.HORSE_SADDLE_2;
                case "horse_hairstyle":
                    return InteractionType.HORSE_HAIRSTYLE;
                case "horse_body_dye":
                    return InteractionType.HORSE_BODY_DYE;
                case "horse_hair_dye":
                    return InteractionType.HORSE_HAIR_DYE;

                case "gnome_box":
                    return InteractionType.GNOME_BOX;
                case "bot":
                    return InteractionType.BOT;
                case "purchasable_clothing":
                    return InteractionType.PURCHASABLE_CLOTHING;
                case "pet_breeding_box":
                    return InteractionType.PET_BREEDING_BOX;
                case "arrow":
                    return InteractionType.ARROW;
                case "arrow2":
                    return InteractionType.ARROW2;
                case "lovelock":
                    return InteractionType.LOVELOCK;
                case "cannon":
                    return InteractionType.CANNON;
                case "counter":
                    return InteractionType.COUNTER;
                case "camera_picture":
                    return InteractionType.CAMERA_PICTURE;
                case "fx_provider":
                case "provider":
                    return InteractionType.FX_PROVIDER;
                case "hi_provider":
                    return InteractionType.HI_PROVIDER;
                case "da_provider":
                    return InteractionType.DA_PROVIDER;
                case "pinata":
                    return InteractionType.PINATA;
                case "info_terminal":
                    return InteractionType.INFO_TERMINAL;
                case "pinatayihadista":
                    return InteractionType.PINATATRIGGERED;
                case "magicegg":
                    return InteractionType.MAGICEGG;
                case "magicchest":
                    return InteractionType.MAGICCHEST;
                case "pepsi":
                    return InteractionType.PEPSIMACHINE;
                case "aguaenergy":
                    return InteractionType.AGUAENERGY;
                case "caramelomachine":
                    return InteractionType.CARAMELOMACHINE;
                case "cajeroruby":
                    return InteractionType.CAJERORUBY;
                case "comida":
                    return InteractionType.COMIDAMACHINE;
                case "atm_machine":
                    return InteractionType.ATM_MACHINE;
                case "rp_nuke":
                    return InteractionType.RP_NUKE;
                case "trash_can":
                    return InteractionType.TRASH_CAN;
                case "house_sign":
                    return InteractionType.HOUSE_SIGN;
                case "basurero":
                    return InteractionType.BASURERO;
                case "tragamonedas":
                    return InteractionType.TRAGAMONEDAS;
                case "cocaina":
                    return InteractionType.Cocaina;
                case "heroina":
                    return InteractionType.HEROINA;
                case "weedmateria":
                    return InteractionType.WEEDMATERIA;
                case "weedporro":
                    return InteractionType.WEEDPORRO;
                case "basuraentrega":
                    return InteractionType.BASURAENTREGA;
                case "mineria":
                    return InteractionType.MINERIA;
                case "delivery_box":
                    return InteractionType.DELIVERY_BOX;
                case "information_terminal":
                    return InteractionType.INFORMATION_TERMINAL;
                case "rentable_space":
                    return InteractionType.RENTABLE_SPACE;
                case "whisper_tile":
                    return InteractionType.WHISPER_TILE;
                case "shower":
                    return InteractionType.SHOWER;
                case "cagar":
                    return InteractionType.CAGAR;
                case "crafting":
                    return InteractionType.CRAFTING;
                case "moplaseed":
                    return InteractionType.MONSTERPLANT_SEED;
                case "farming":
                    return InteractionType.FARMING;
                case "club_vip":
                    return InteractionType.CLUB_VIP;
                case "club_vip2":
                    return InteractionType.CLUB_VIP2;
                case "club_1_month":
                    return InteractionType.club_1_month;
                case "club_3_month":
                    return InteractionType.club_3_month;
                case "club_6_month":
                    return InteractionType.club_6_month;
                case "comodin":
                    return InteractionType.COMODIN;
                case "carnew":
                    return InteractionType.CARNEW;
                default:
                    {
                        if (pType.StartsWith("pet") && int.TryParse(pType.Replace("pet", ""), out int _))
                            return InteractionType.PET;

                        if (pType.StartsWith("wf_act_"))
                            return InteractionType.WIRED_EFFECT;
                        if (pType.StartsWith("wf_trg_"))
                            return InteractionType.WIRED_TRIGGER;
                        if (pType.StartsWith("wf_cnd_"))
                            return InteractionType.WIRED_CONDITION;
                        if (pType.StartsWith("wf_xtra_"))
                            return InteractionType.WIRED_EFFECT; // Usually addons/effects

                        //Logging.WriteLine("Unknown interaction type in parse code: " + pType, ConsoleColor.Yellow);
                        return InteractionType.NONE;
                    }
            }
        }
    }
}