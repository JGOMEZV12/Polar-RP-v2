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
        WIRED_ADDON,

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
        CARNEW,

        TELEPORT_TILE,
        PRESSURE_PLATE,
        COLOR_PLATE,
        MULTI_HEIGHT,
        COLOR_WHEEL,
        CRACKABLE,
        NEST,
        PET_DRINK,
        PET_FOOD,
        PET_TOY,
        PET_TREE,
        PET_TRAMPOLINE,
        BREEDING_NEST,
        OBSTACLE,
        STACK_HELPER,
        COSTUME_HOPPER,
        EFFECT_GATE,
        CLUB_HOPPER,
        CLUB_GATE,
        CLUB_TELEPORT_TILE,
        LOVE_LOCK,
        FIREWORKS,
        TALKING_FURNI,
        WATER_ITEM,
        VIKING_COTIE,
        TILE_FXPROVIDER,
        MUTE_AREA,
        BUILD_AREA,
        YOUTUBE,
        SWITCH,
        SWITCH_REMOTE,
        FX_BOX,
        BLACKHOLE,
        EFFECT_TOGGLE,
        ROOM_O_MATIC,
        EFFECT_TILE,
        STICKY_POLE,
        TRAP,
        GYM_EQUIPMENT,
        HANDITEM,
        HANDITEM_TILE,
        EFFECT_GIVER,
        EFFECT_VENDING_MACHINE,
        EFFECT_VENDING_MACHINE_NOSIDES,
        CRACKABLE_MONSTER,
        SNOWBOARD_SLOPE,
        PRESSURE_PLATE_GROUP,
        EFFECT_TILE_GROUP,
        SUBSCRIPTION_BOX,
        RANDOM_STATE,
        VENDING_MACHINE_NOSIDES,
        TILE_WALKMAGIC,
        GAME_TIMER,
        GAME_UPCOUNTER,
        NAME_COLOR,
        NAME_PREFIX,
        WF_ACT_ADD_TAG,
        WF_ACT_ALERTBUBLE,
        WF_ACT_BOT_CLOTHES,
        WF_ACT_BOT_FOLLOW_AVATAR,
        WF_ACT_BOT_GIVE_HANDITEM,
        WF_ACT_BOT_MOVE,
        WF_ACT_BOT_TALK,
        WF_ACT_BOT_TALK_TO_AVATAR,
        WF_ACT_BOT_TELEPORT,
        WF_ACT_CALL_STACKS,
        WF_ACT_CALL_STK_WC,
        WF_ACT_CHASE,
        WF_ACT_CLOSE_DICE,
        WF_ACT_CLOSES_DICES,
        WF_ACT_CONTROL,
        WF_ACT_CONTROL_ID,
        WF_ACT_COUNTER_POINTS,
        WF_ACT_CUSTOM_ADDHIGHSCORE,
        WF_ACT_DAMAGE_USER,
        WF_ACT_DEUX_MESSAGE,
        WF_ACT_EFFECT_MPU,
        WF_ACT_ENABLE_CSTM,
        WF_ACT_EXE_CONDICION,
        WF_ACT_EXE_FLAWLESS,
        WF_ACT_EXE_SUPER,
        WF_ACT_EXECUTE_COMMAND,
        WF_ACT_FLEE,
        WF_ACT_FREEZE,
        WF_ACT_FREEZE_CSTM,
        WF_ACT_FREEZE_DELAY,
        WF_ACT_FURNI_SUPER_CHASE,
        WF_ACT_FURNI_TO_FURNI,
        WF_ACT_FURNI_TO_USER,
        WF_ACT_GIVE_ARMOR,
        WF_ACT_GIVE_BADGE,
        WF_ACT_GIVE_ENERGY,
        WF_ACT_GIVE_HUNT_POINTS,
        WF_ACT_GIVE_REWARD,
        WF_ACT_GIVE_RP_ITEM,
        WF_ACT_GIVE_SCORE,
        WF_ACT_GIVE_SCORE_PP,
        WF_ACT_GIVE_SCORE_TM,
        WF_ACT_GIVE_USERBADGE,
        WF_ACT_HEAL_USER,
        WF_ACT_JOIN_TEAM,
        WF_ACT_KICK_USER,
        WF_ACT_LEAVE_TEAM,
        WF_ACT_MATCH_TO_SSHOT,
        WF_ACT_MATCH_TO_SSHOT_XYZ,
        WF_ACT_MOVE_FURNI_XYZ,
        WF_ACT_MOVE_FURNI_XYZ_SLIDE,
        WF_ACT_MOVE_ROTATE,
        WF_ACT_MOVE_ROTATE_INSTANT,
        WF_ACT_MOVE_TO_DIR,
        WF_ACT_MUTE_TRIGGERER,
        WF_ACT_NEG_CALL_STACKS,
        WF_ACT_PLUS_MATCH_FURNI_STATE,
        WF_ACT_POINTS,
        WF_ACT_PROGRESS_ACHIEVEMENT,
        WF_ACT_REGENERATE_MAP,
        WF_ACT_ROLLER,
        WF_ACT_ROLLER_SPEED,
        WF_ACT_ROTATE_USER,
        WF_ACT_ROTATIONHABBO,
        WF_ACT_SET_POINTS,
        WF_ACT_SET_ROLLER_SPD,
        WF_ACT_SHOW_MESSAGE,
        WF_ACT_SHOW_MESSAGE_ROOM,
        WF_ACT_SIGN_CSTM,
        WF_ACT_SUPER_CHASE,
        WF_ACT_TELEPORT_BLUE,
        WF_ACT_TELEPORT_GREEN,
        WF_ACT_TELEPORT_RED,
        WF_ACT_TELEPORT_TO,
        WF_ACT_TELEPORT_TO_FURNI_HABBO,
        WF_ACT_TELEPORT_YELLOW,
        WF_ACT_TEPORTOROOM,
        WF_ACT_TOGGLE_MOODLIGHT,
        WF_ACT_TOGGLE_NEGA,
        WF_ACT_TOGGLE_NEGAT,
        WF_ACT_TOGGLE_RANDOM,
        WF_ACT_TOGGLE_STATE,
        WF_ACT_TOGGLE_STATE_DOWN,
        WF_ACT_TOGGLE_STATE_RANDOM,
        WF_ACT_TOGGLE_TO_RND,
        WF_ACT_TOROOM,
        WF_ACT_TOROOM_STAFF,
        WF_ACT_TOUR_NE_AVA,
        WF_ACT_TP_FURNI_TO_HABBO,
        WF_ACT_UNFREEZ,
        WF_ACT_UNFREEZE_CSTM,
        WF_ACT_UNFREEZE_DELAY,
        WF_CND_ACTOR_IN_GROUP,
        WF_CND_ACTOR_IN_TEAM,
        WF_CND_FURNIS_HV_AVTRS,
        WF_CND_HABBO_HAS_RANK,
        WF_CND_HABBO_OWNS_BADGE,
        WF_CND_HANDITEM,
        WF_CND_HAS_BADGE_OR_MISSION,
        WF_CND_HAS_FURNI_ON,
        WF_CND_HAS_HANDITEM,
        WF_CND_HAS_JOB,
        WF_CND_HAS_RANK,
        WF_CND_HAS_RP_ITEM,
        WF_CND_HAS_WEAPON,
        WF_CND_IS_AFK,
        WF_CND_IS_DANCINGBB,
        WF_CND_IS_DAY,
        WF_CND_IS_DEAD,
        WF_CND_IS_JAILED,
        WF_CND_IS_NIGHT,
        WF_CND_IS_SITTING,
        WF_CND_MATCH_SNAP_NO_XYZ,
        WF_CND_MATCH_SNAPSHOT,
        WF_CND_MATCH_SNAPSHOT_XYZ,
        WF_CND_NOT_FURNI_ON,
        WF_CND_NOT_HABBO_OWNS_BADGE,
        WF_CND_NOT_HV_AVTRS,
        WF_CND_NOT_IN_GROUP,
        WF_CND_NOT_MATCH_SNAP,
        WF_CND_NOT_MATCH_SNAP_XYZ,
        WF_CND_NOT_STUFF_IS,
        WF_CND_NOT_TRGGRER_ON,
        WF_CND_NOT_USER_COUNT,
        WF_CND_NOT_WEARING_B,
        WF_CND_NOT_WEARING_FX,
        WF_CND_NOT_WRINGBDG,
        WF_CND_STUFF_IS,
        WF_CND_SUPER_WIREDEQUIPO,
        WF_CND_TRGGRER_ON_FRN,
        WF_CND_USER_COUNT_IN,
        WF_CND_USER_HASRIGHTS,
        WF_CND_WEARING_BADG,
        WF_CND_WEARING_BADGE,
        WF_CND_WEARING_EFFECT,
        WF_CSTM_FREEZE,
        WF_CSTM_UFREEZ,
        WF_TRG_AFKKKDORMEUR,
        WF_TRG_ANTI_AFK,
        WF_TRG_AT_GIVEN_TIME,
        WF_TRG_AT_TIME_LONG,
        WF_TRG_CHAT_CMD_OTHER,
        WF_TRG_CHAT_CMD_USER,
        WF_TRG_CLOCK_COUNTER,
        WF_TRG_COLISION_OTHER_USER,
        WF_TRG_COLISION_TEAM_OTHER_USER,
        WF_TRG_COLISION_TEAM_USER_OTHER,
        WF_TRG_COLISION_USER_OTHER,
        WF_TRG_COLLISION,
        WF_TRG_DOUBLE_CLICK_FURNI,
        WF_TRG_ENTER_ROOM,
        WF_TRG_EXIT_ROOM,
        WF_TRG_GAME_ENDS,
        WF_TRG_GAME_STARTS,
        WF_TRG_IDK,
        WF_TRG_IDK_SEG,
        WF_TRG_LEAVE_ROOM,
        WF_TRG_NOT_SAYS_THING,
        WF_TRG_PERIOD_LONG,
        WF_TRG_PERIODICALLY,
        WF_TRG_REPEAT_SHORT,
        WF_TRG_SAYS_NOT_SOMETHING,
        WF_TRG_SAYS_SMTH_SHOW,
        WF_TRG_SAYS_SOMETHING,
        WF_TRG_SAYS_SOMETHING_CONTAINS,
        WF_TRG_SAYS_SOMETHING_EQUAL,
        WF_TRG_STATE_CHANGED,
        WF_TRG_STUFF_STATE,
        WF_TRG_WALKS_OFF_FURNI,
        WF_TRG_WALKS_ON_FURNI,
        WF_XTRA_ALL_EVAL,
        WF_XTRA_ALL_EVAL_NOT,
        WF_XTRA_AND_EVAL,
        WF_XTRA_AND_EVAL_NOT,
        WF_XTRA_ANIMATION_TIME,
        WF_XTRA_COMMENT,
        WF_XTRA_DIAGONALCOLLISION,
        WF_XTRA_EXECUTE_IN_ORDER,
        WF_XTRA_EXECUTION_LIMIT,
        WF_XTRA_FILTER_FURNI,
        WF_XTRA_FILTER_USER,
        WF_XTRA_FLECHE,
        WF_XTRA_MOV_CARRY_USERS,
        WF_XTRA_MOV_NO_ANIMATION,
        WF_XTRA_MOV_PHYSICS,
        WF_XTRA_MOVEITEMWITHUSERS,
        WF_XTRA_NOT_ANIMATE_FURNI,
        WF_XTRA_NOT_ANIMATE_USER,
        WF_XTRA_NOT_EFFECT_TELEPORT,
        WF_XTRA_NOTEFFECTTP,
        WF_XTRA_NOTFREEZETP,
        WF_XTRA_NOTTEAM,
        WF_XTRA_ONE_CONDITION,
        WF_XTRA_OR_EVAL,
        WF_XTRA_PT_UNSEENRANDOM,
        WF_XTRA_RANDOM,
        WF_XTRA_SEQUENCE,
        WF_XTRA_TEXT_OUTPUT_FURNI_NAME,
        WF_XTRA_TEXT_OUTPUT_USERNAME,
        WF_XTRA_UNSEEN,
        WF_XTRA_UNSEEN_RANDOM,
        WF_XTRA_UNSEENRANDOM,
        WF_XTRA_USABILITY,
        WF_XTRA_USABILITYCOND,
        WF_XTRA_USERAROUND,
        WF_XTRA_USERBEHIND,
        WF_XTRA_USERINFRONT,
        WF_XTRA_XIXIXI
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
                case "door":
                    return InteractionType.GATE;
                case "postit":
                case "sticky_note":
                    return InteractionType.POSTIT;
                case "dimmer":
                case "moodlight":
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
                case "vending_machine":
                    return InteractionType.VENDING_MACHINE;
                case "alert":
                    return InteractionType.ALERT;
                case "onewaygate":
                case "one_way_gate":
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
                case "teleporttile":
                    return InteractionType.TELEPORT_TILE;
                case "pressureplate":
                    return InteractionType.PRESSURE_PLATE;
                case "colorplate":
                    return InteractionType.COLOR_PLATE;
                case "multiheight":
                    return InteractionType.MULTI_HEIGHT;
                case "colorwheel":
                    return InteractionType.COLOR_WHEEL;
                case "crackable":
                case "crackable_master":
                    return InteractionType.CRACKABLE;
                case "nest":
                    return InteractionType.NEST;
                case "pet_drink":
                case "pet_water":
                    return InteractionType.PET_DRINK;
                case "pet_food":
                case "pet_food_bowl":
                    return InteractionType.PET_FOOD;
                case "pet_toy":
                    return InteractionType.PET_TOY;
                case "pet_tree":
                    return InteractionType.PET_TREE;
                case "pet_trampoline":
                    return InteractionType.PET_TRAMPOLINE;
                case "breeding_nest":
                    return InteractionType.BREEDING_NEST;
                case "obstacle":
                    return InteractionType.OBSTACLE;
                case "stack_helper":
                case "stackhelper":
                    return InteractionType.STACK_HELPER;
                case "costume_hopper":
                    return InteractionType.COSTUME_HOPPER;
                case "effect_gate":
                    return InteractionType.EFFECT_GATE;
                case "club_hopper":
                    return InteractionType.CLUB_HOPPER;
                case "club_gate":
                    return InteractionType.CLUB_GATE;
                case "club_teleporttile":
                    return InteractionType.CLUB_TELEPORT_TILE;
                case "love_lock":
                    return InteractionType.LOVE_LOCK;
                case "fireworks":
                    return InteractionType.FIREWORKS;
                case "talking_furni":
                    return InteractionType.TALKING_FURNI;
                case "water_item":
                    return InteractionType.WATER_ITEM;
                case "viking_cotie":
                    return InteractionType.VIKING_COTIE;
                case "tile_fxprovider_nfs":
                    return InteractionType.TILE_FXPROVIDER;
                case "mutearea":
                    return InteractionType.MUTE_AREA;
                case "buildarea":
                    return InteractionType.BUILD_AREA;
                case "youtube":
                    return InteractionType.YOUTUBE;
                case "switch":
                    return InteractionType.SWITCH;
                case "switch_remote_control":
                    return InteractionType.SWITCH_REMOTE;
                case "fx_box":
                    return InteractionType.FX_BOX;
                case "blackhole":
                    return InteractionType.BLACKHOLE;
                case "effect_toggle":
                    return InteractionType.EFFECT_TOGGLE;
                case "room_o_matic":
                    return InteractionType.ROOM_O_MATIC;
                case "effect_tile":
                    return InteractionType.EFFECT_TILE;
                case "sticky_pole":
                    return InteractionType.STICKY_POLE;
                case "trap":
                    return InteractionType.TRAP;
                case "gym_equipment":
                    return InteractionType.GYM_EQUIPMENT;
                case "handitem":
                    return InteractionType.HANDITEM;
                case "handitem_tile":
                    return InteractionType.HANDITEM_TILE;
                case "effect_giver":
                    return InteractionType.EFFECT_GIVER;
                case "effect_vendingmachine":
                    return InteractionType.EFFECT_VENDING_MACHINE;
                case "effect_vendingmachine_no_sides":
                    return InteractionType.EFFECT_VENDING_MACHINE_NOSIDES;
                case "crackable_monster":
                case "crackable_subscription_box":
                    return InteractionType.CRACKABLE_MONSTER;
                case "snowboard_slope":
                    return InteractionType.SNOWBOARD_SLOPE;
                case "pressureplate_group":
                    return InteractionType.PRESSURE_PLATE_GROUP;
                case "effect_tile_group":
                    return InteractionType.EFFECT_TILE_GROUP;
                case "crackable_subscription_box_redeem":
                    return InteractionType.SUBSCRIPTION_BOX;
                case "random_state":
                    return InteractionType.RANDOM_STATE;
                case "vendingmachine_no_sides":
                    return InteractionType.VENDING_MACHINE_NOSIDES;
                case "tile_walkmagic":
                    return InteractionType.TILE_WALKMAGIC;
                case "game_timer":
                    return InteractionType.GAME_TIMER;
                case "game_upcounter":
                    return InteractionType.GAME_UPCOUNTER;
                case "namecolor":
                case "name_color":
                    return InteractionType.NAME_COLOR;
                case "nameprefix":
                case "name_prefix":
                case "prefix":
                    return InteractionType.NAME_PREFIX;
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
                case "wf_blob":
                case "wf_blob_invis":
                case "wf_blob2":
                case "wf_blob2_vis":
                    return InteractionType.WIRED_EFFECT;
                case "irinc_xtra_or":
                case "irinc_hideable_barrier":
                case "bsstonino_furni1479":
                    return InteractionType.WIRED_EFFECT;
                case "clothing":
                case "habbo_clothing":
                case "purchasable_clothing":
                    return InteractionType.PURCHASABLE_CLOTHING;
                case "mystery_box":
                    return InteractionType.MAGICCHEST;
                case "guild_item":
                case "guild_furni":
                case "guild_furni_wallpaper":
                case "guild_furni_floor":
                    return InteractionType.GUILD_ITEM;
                case "guild_forum":
                    return InteractionType.GUILD_FORUM;
                case "guild_gate":
                    return InteractionType.GUILD_GATE;
                case "roomads":
                case "external_image":
                case "ads_bg":
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
                case "crackable_furni":
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
                    switch (pType.ToLower())
                    {
                        case "wf_act_add_tag": return InteractionType.WF_ACT_ADD_TAG;
                        case "wf_act_alertbuble": return InteractionType.WF_ACT_ALERTBUBLE;
                        case "wf_act_bot_clothes": return InteractionType.WF_ACT_BOT_CLOTHES;
                        case "wf_act_bot_follow_avatar": return InteractionType.WF_ACT_BOT_FOLLOW_AVATAR;
                        case "wf_act_bot_give_handitem": return InteractionType.WF_ACT_BOT_GIVE_HANDITEM;
                        case "wf_act_bot_move": return InteractionType.WF_ACT_BOT_MOVE;
                        case "wf_act_bot_talk": return InteractionType.WF_ACT_BOT_TALK;
                        case "wf_act_bot_talk_to_avatar": return InteractionType.WF_ACT_BOT_TALK_TO_AVATAR;
                        case "wf_act_bot_teleport": return InteractionType.WF_ACT_BOT_TELEPORT;
                        case "wf_act_call_stacks": return InteractionType.WF_ACT_CALL_STACKS;
                        case "wf_act_call_stk_wc": return InteractionType.WF_ACT_CALL_STK_WC;
                        case "wf_act_chase": return InteractionType.WF_ACT_CHASE;
                        case "wf_act_close_dice": return InteractionType.WF_ACT_CLOSE_DICE;
                        case "wf_act_closes_dices": return InteractionType.WF_ACT_CLOSES_DICES;
                        case "wf_act_control": return InteractionType.WF_ACT_CONTROL;
                        case "wf_act_control_id": return InteractionType.WF_ACT_CONTROL_ID;
                        case "wf_act_counter_points": return InteractionType.WF_ACT_COUNTER_POINTS;
                        case "wf_act_custom_addhighscore": return InteractionType.WF_ACT_CUSTOM_ADDHIGHSCORE;
                        case "wf_act_damage_user": return InteractionType.WF_ACT_DAMAGE_USER;
                        case "wf_act_deux_message": return InteractionType.WF_ACT_DEUX_MESSAGE;
                        case "wf_act_effect_mpu": return InteractionType.WF_ACT_EFFECT_MPU;
                        case "wf_act_enable_cstm": return InteractionType.WF_ACT_ENABLE_CSTM;
                        case "wf_act_exe_condicion": return InteractionType.WF_ACT_EXE_CONDICION;
                        case "wf_act_exe_flawless": return InteractionType.WF_ACT_EXE_FLAWLESS;
                        case "wf_act_exe_super": return InteractionType.WF_ACT_EXE_SUPER;
                        case "wf_act_execute_command": return InteractionType.WF_ACT_EXECUTE_COMMAND;
                        case "wf_act_flee": return InteractionType.WF_ACT_FLEE;
                        case "wf_act_freeze": return InteractionType.WF_ACT_FREEZE;
                        case "wf_act_freeze_cstm": return InteractionType.WF_ACT_FREEZE_CSTM;
                        case "wf_act_freeze_delay": return InteractionType.WF_ACT_FREEZE_DELAY;
                        case "wf_act_furni_super_chase": return InteractionType.WF_ACT_FURNI_SUPER_CHASE;
                        case "wf_act_furni_to_furni": return InteractionType.WF_ACT_FURNI_TO_FURNI;
                        case "wf_act_furni_to_user": return InteractionType.WF_ACT_FURNI_TO_USER;
                        case "wf_act_give_armor": return InteractionType.WF_ACT_GIVE_ARMOR;
                        case "wf_act_give_badge": return InteractionType.WF_ACT_GIVE_BADGE;
                        case "wf_act_give_energy": return InteractionType.WF_ACT_GIVE_ENERGY;
                        case "wf_act_give_hunt_points": return InteractionType.WF_ACT_GIVE_HUNT_POINTS;
                        case "wf_act_give_reward": return InteractionType.WF_ACT_GIVE_REWARD;
                        case "wf_act_give_rp_item": return InteractionType.WF_ACT_GIVE_RP_ITEM;
                        case "wf_act_give_score": return InteractionType.WF_ACT_GIVE_SCORE;
                        case "wf_act_give_score_pp": return InteractionType.WF_ACT_GIVE_SCORE_PP;
                        case "wf_act_give_score_tm": return InteractionType.WF_ACT_GIVE_SCORE_TM;
                        case "wf_act_give_userbadge": return InteractionType.WF_ACT_GIVE_USERBADGE;
                        case "wf_act_heal_user": return InteractionType.WF_ACT_HEAL_USER;
                        case "wf_act_join_team": return InteractionType.WF_ACT_JOIN_TEAM;
                        case "wf_act_kick_user": return InteractionType.WF_ACT_KICK_USER;
                        case "wf_act_leave_team": return InteractionType.WF_ACT_LEAVE_TEAM;
                        case "wf_act_match_to_sshot": return InteractionType.WF_ACT_MATCH_TO_SSHOT;
                        case "wf_act_match_to_sshot_xyz": return InteractionType.WF_ACT_MATCH_TO_SSHOT_XYZ;
                        case "wf_act_move_furni_xyz": return InteractionType.WF_ACT_MOVE_FURNI_XYZ;
                        case "wf_act_move_furni_xyz_slide": return InteractionType.WF_ACT_MOVE_FURNI_XYZ_SLIDE;
                        case "wf_act_move_rotate": return InteractionType.WF_ACT_MOVE_ROTATE;
                        case "wf_act_move_rotate_instant": return InteractionType.WF_ACT_MOVE_ROTATE_INSTANT;
                        case "wf_act_move_to_dir": return InteractionType.WF_ACT_MOVE_TO_DIR;
                        case "wf_act_mute_triggerer": return InteractionType.WF_ACT_MUTE_TRIGGERER;
                        case "wf_act_neg_call_stacks": return InteractionType.WF_ACT_NEG_CALL_STACKS;
                        case "wf_act_plus_match_furni_state": return InteractionType.WF_ACT_PLUS_MATCH_FURNI_STATE;
                        case "wf_act_points": return InteractionType.WF_ACT_POINTS;
                        case "wf_act_progress_achievement": return InteractionType.WF_ACT_PROGRESS_ACHIEVEMENT;
                        case "wf_act_regenerate_map": return InteractionType.WF_ACT_REGENERATE_MAP;
                        case "wf_act_roller": return InteractionType.WF_ACT_ROLLER;
                        case "wf_act_roller_speed": return InteractionType.WF_ACT_ROLLER_SPEED;
                        case "wf_act_rotate_user": return InteractionType.WF_ACT_ROTATE_USER;
                        case "wf_act_rotationhabbo": return InteractionType.WF_ACT_ROTATIONHABBO;
                        case "wf_act_set_points": return InteractionType.WF_ACT_SET_POINTS;
                        case "wf_act_set_roller_spd": return InteractionType.WF_ACT_SET_ROLLER_SPD;
                        case "wf_act_show_message": return InteractionType.WF_ACT_SHOW_MESSAGE;
                        case "wf_act_show_message_room": return InteractionType.WF_ACT_SHOW_MESSAGE_ROOM;
                        case "wf_act_sign_cstm": return InteractionType.WF_ACT_SIGN_CSTM;
                        case "wf_act_super_chase": return InteractionType.WF_ACT_SUPER_CHASE;
                        case "wf_act_teleport_blue": return InteractionType.WF_ACT_TELEPORT_BLUE;
                        case "wf_act_teleport_green": return InteractionType.WF_ACT_TELEPORT_GREEN;
                        case "wf_act_teleport_red": return InteractionType.WF_ACT_TELEPORT_RED;
                        case "wf_act_teleport_to": return InteractionType.WF_ACT_TELEPORT_TO;
                        case "wf_act_teleport_to_furni_habbo": return InteractionType.WF_ACT_TELEPORT_TO_FURNI_HABBO;
                        case "wf_act_teleport_yellow": return InteractionType.WF_ACT_TELEPORT_YELLOW;
                        case "wf_act_teportoroom": return InteractionType.WF_ACT_TEPORTOROOM;
                        case "wf_act_toggle_moodlight": return InteractionType.WF_ACT_TOGGLE_MOODLIGHT;
                        case "wf_act_toggle_nega": return InteractionType.WF_ACT_TOGGLE_NEGA;
                        case "wf_act_toggle_negat": return InteractionType.WF_ACT_TOGGLE_NEGAT;
                        case "wf_act_toggle_random": return InteractionType.WF_ACT_TOGGLE_RANDOM;
                        case "wf_act_toggle_state": return InteractionType.WF_ACT_TOGGLE_STATE;
                        case "wf_act_toggle_state_down": return InteractionType.WF_ACT_TOGGLE_STATE_DOWN;
                        case "wf_act_toggle_state_random": return InteractionType.WF_ACT_TOGGLE_STATE_RANDOM;
                        case "wf_act_toggle_to_rnd": return InteractionType.WF_ACT_TOGGLE_TO_RND;
                        case "wf_act_toroom": return InteractionType.WF_ACT_TOROOM;
                        case "wf_act_toroom_staff": return InteractionType.WF_ACT_TOROOM_STAFF;
                        case "wf_act_tour_ne_ava": return InteractionType.WF_ACT_TOUR_NE_AVA;
                        case "wf_act_tp_furni_to_habbo": return InteractionType.WF_ACT_TP_FURNI_TO_HABBO;
                        case "wf_act_unfreez": return InteractionType.WF_ACT_UNFREEZ;
                        case "wf_act_unfreeze_cstm": return InteractionType.WF_ACT_UNFREEZE_CSTM;
                        case "wf_act_unfreeze_delay": return InteractionType.WF_ACT_UNFREEZE_DELAY;
                        case "wf_cnd_actor_in_group": return InteractionType.WF_CND_ACTOR_IN_GROUP;
                        case "wf_cnd_actor_in_team": return InteractionType.WF_CND_ACTOR_IN_TEAM;
                        case "wf_cnd_furnis_hv_avtrs": return InteractionType.WF_CND_FURNIS_HV_AVTRS;
                        case "wf_cnd_habbo_has_rank": return InteractionType.WF_CND_HABBO_HAS_RANK;
                        case "wf_cnd_habbo_owns_badge": return InteractionType.WF_CND_HABBO_OWNS_BADGE;
                        case "wf_cnd_handitem": return InteractionType.WF_CND_HANDITEM;
                        case "wf_cnd_has_badge_or_mission": return InteractionType.WF_CND_HAS_BADGE_OR_MISSION;
                        case "wf_cnd_has_furni_on": return InteractionType.WF_CND_HAS_FURNI_ON;
                        case "wf_cnd_has_handitem": return InteractionType.WF_CND_HAS_HANDITEM;
                        case "wf_cnd_has_job": return InteractionType.WF_CND_HAS_JOB;
                        case "wf_cnd_has_rank": return InteractionType.WF_CND_HAS_RANK;
                        case "wf_cnd_has_rp_item": return InteractionType.WF_CND_HAS_RP_ITEM;
                        case "wf_cnd_has_weapon": return InteractionType.WF_CND_HAS_WEAPON;
                        case "wf_cnd_is_afk": return InteractionType.WF_CND_IS_AFK;
                        case "wf_cnd_is_dancingbb": return InteractionType.WF_CND_IS_DANCINGBB;
                        case "wf_cnd_is_day": return InteractionType.WF_CND_IS_DAY;
                        case "wf_cnd_is_dead": return InteractionType.WF_CND_IS_DEAD;
                        case "wf_cnd_is_jailed": return InteractionType.WF_CND_IS_JAILED;
                        case "wf_cnd_is_night": return InteractionType.WF_CND_IS_NIGHT;
                        case "wf_cnd_is_sitting": return InteractionType.WF_CND_IS_SITTING;
                        case "wf_cnd_match_snap_no_xyz": return InteractionType.WF_CND_MATCH_SNAP_NO_XYZ;
                        case "wf_cnd_match_snapshot": return InteractionType.WF_CND_MATCH_SNAPSHOT;
                        case "wf_cnd_match_snapshot_xyz": return InteractionType.WF_CND_MATCH_SNAPSHOT_XYZ;
                        case "wf_cnd_not_furni_on": return InteractionType.WF_CND_NOT_FURNI_ON;
                        case "wf_cnd_not_habbo_owns_badge": return InteractionType.WF_CND_NOT_HABBO_OWNS_BADGE;
                        case "wf_cnd_not_hv_avtrs": return InteractionType.WF_CND_NOT_HV_AVTRS;
                        case "wf_cnd_not_in_group": return InteractionType.WF_CND_NOT_IN_GROUP;
                        case "wf_cnd_not_match_snap": return InteractionType.WF_CND_NOT_MATCH_SNAP;
                        case "wf_cnd_not_match_snap_xyz": return InteractionType.WF_CND_NOT_MATCH_SNAP_XYZ;
                        case "wf_cnd_not_stuff_is": return InteractionType.WF_CND_NOT_STUFF_IS;
                        case "wf_cnd_not_trggrer_on": return InteractionType.WF_CND_NOT_TRGGRER_ON;
                        case "wf_cnd_not_user_count": return InteractionType.WF_CND_NOT_USER_COUNT;
                        case "wf_cnd_not_wearing_b": return InteractionType.WF_CND_NOT_WEARING_B;
                        case "wf_cnd_not_wearing_fx": return InteractionType.WF_CND_NOT_WEARING_FX;
                        case "wf_cnd_not_wringbdg": return InteractionType.WF_CND_NOT_WRINGBDG;
                        case "wf_cnd_stuff_is": return InteractionType.WF_CND_STUFF_IS;
                        case "wf_cnd_super_wiredequipo": return InteractionType.WF_CND_SUPER_WIREDEQUIPO;
                        case "wf_cnd_trggrer_on_frn": return InteractionType.WF_CND_TRGGRER_ON_FRN;
                        case "wf_cnd_user_count_in": return InteractionType.WF_CND_USER_COUNT_IN;
                        case "wf_cnd_user_hasrights": return InteractionType.WF_CND_USER_HASRIGHTS;
                        case "wf_cnd_wearing_badg": return InteractionType.WF_CND_WEARING_BADG;
                        case "wf_cnd_wearing_badge": return InteractionType.WF_CND_WEARING_BADGE;
                        case "wf_cnd_wearing_effect": return InteractionType.WF_CND_WEARING_EFFECT;
                        case "wf_cstm_freeze": return InteractionType.WF_CSTM_FREEZE;
                        case "wf_cstm_ufreez": return InteractionType.WF_CSTM_UFREEZ;
                        case "wf_trg_afkkkdormeur": return InteractionType.WF_TRG_AFKKKDORMEUR;
                        case "wf_trg_anti_afk": return InteractionType.WF_TRG_ANTI_AFK;
                        case "wf_trg_at_given_time": return InteractionType.WF_TRG_AT_GIVEN_TIME;
                        case "wf_trg_at_time_long": return InteractionType.WF_TRG_AT_TIME_LONG;
                        case "wf_trg_chat_cmd_other": return InteractionType.WF_TRG_CHAT_CMD_OTHER;
                        case "wf_trg_chat_cmd_user": return InteractionType.WF_TRG_CHAT_CMD_USER;
                        case "wf_trg_clock_counter": return InteractionType.WF_TRG_CLOCK_COUNTER;
                        case "wf_trg_colision_other_user": return InteractionType.WF_TRG_COLISION_OTHER_USER;
                        case "wf_trg_colision_team_other_user": return InteractionType.WF_TRG_COLISION_TEAM_OTHER_USER;
                        case "wf_trg_colision_team_user_other": return InteractionType.WF_TRG_COLISION_TEAM_USER_OTHER;
                        case "wf_trg_colision_user_other": return InteractionType.WF_TRG_COLISION_USER_OTHER;
                        case "wf_trg_collision": return InteractionType.WF_TRG_COLLISION;
                        case "wf_trg_double_click_furni": return InteractionType.WF_TRG_DOUBLE_CLICK_FURNI;
                        case "wf_trg_enter_room": return InteractionType.WF_TRG_ENTER_ROOM;
                        case "wf_trg_exit_room": return InteractionType.WF_TRG_EXIT_ROOM;
                        case "wf_trg_game_ends": return InteractionType.WF_TRG_GAME_ENDS;
                        case "wf_trg_game_starts": return InteractionType.WF_TRG_GAME_STARTS;
                        case "wf_trg_idk": return InteractionType.WF_TRG_IDK;
                        case "wf_trg_idk_seg": return InteractionType.WF_TRG_IDK_SEG;
                        case "wf_trg_leave_room": return InteractionType.WF_TRG_LEAVE_ROOM;
                        case "wf_trg_not_says_thing": return InteractionType.WF_TRG_NOT_SAYS_THING;
                        case "wf_trg_period_long": return InteractionType.WF_TRG_PERIOD_LONG;
                        case "wf_trg_periodically": return InteractionType.WF_TRG_PERIODICALLY;
                        case "wf_trg_repeat_short": return InteractionType.WF_TRG_REPEAT_SHORT;
                        case "wf_trg_says_not_something": return InteractionType.WF_TRG_SAYS_NOT_SOMETHING;
                        case "wf_trg_says_smth_show": return InteractionType.WF_TRG_SAYS_SMTH_SHOW;
                        case "wf_trg_says_something": return InteractionType.WF_TRG_SAYS_SOMETHING;
                        case "wf_trg_says_something_contains": return InteractionType.WF_TRG_SAYS_SOMETHING_CONTAINS;
                        case "wf_trg_says_something_equal": return InteractionType.WF_TRG_SAYS_SOMETHING_EQUAL;
                        case "wf_trg_state_changed": return InteractionType.WF_TRG_STATE_CHANGED;
                        case "wf_trg_stuff_state": return InteractionType.WF_TRG_STUFF_STATE;
                        case "wf_trg_walks_off_furni": return InteractionType.WF_TRG_WALKS_OFF_FURNI;
                        case "wf_trg_walks_on_furni": return InteractionType.WF_TRG_WALKS_ON_FURNI;
                        case "wf_xtra_all_eval": return InteractionType.WF_XTRA_ALL_EVAL;
                        case "wf_xtra_all_eval_not": return InteractionType.WF_XTRA_ALL_EVAL_NOT;
                        case "wf_xtra_and_eval": return InteractionType.WF_XTRA_AND_EVAL;
                        case "wf_xtra_and_eval_not": return InteractionType.WF_XTRA_AND_EVAL_NOT;
                        case "wf_xtra_animation_time": return InteractionType.WF_XTRA_ANIMATION_TIME;
                        case "wf_xtra_comment": return InteractionType.WF_XTRA_COMMENT;
                        case "wf_xtra_diagonalcollision": return InteractionType.WF_XTRA_DIAGONALCOLLISION;
                        case "wf_xtra_execute_in_order": return InteractionType.WF_XTRA_EXECUTE_IN_ORDER;
                        case "wf_xtra_execution_limit": return InteractionType.WF_XTRA_EXECUTION_LIMIT;
                        case "wf_xtra_filter_furni": return InteractionType.WF_XTRA_FILTER_FURNI;
                        case "wf_xtra_filter_user": return InteractionType.WF_XTRA_FILTER_USER;
                        case "wf_xtra_fleche": return InteractionType.WF_XTRA_FLECHE;
                        case "wf_xtra_mov_carry_users": return InteractionType.WF_XTRA_MOV_CARRY_USERS;
                        case "wf_xtra_mov_no_animation": return InteractionType.WF_XTRA_MOV_NO_ANIMATION;
                        case "wf_xtra_mov_physics": return InteractionType.WF_XTRA_MOV_PHYSICS;
                        case "wf_xtra_moveitemwithusers": return InteractionType.WF_XTRA_MOVEITEMWITHUSERS;
                        case "wf_xtra_not_animate_furni": return InteractionType.WF_XTRA_NOT_ANIMATE_FURNI;
                        case "wf_xtra_not_animate_user": return InteractionType.WF_XTRA_NOT_ANIMATE_USER;
                        case "wf_xtra_not_effect_teleport": return InteractionType.WF_XTRA_NOT_EFFECT_TELEPORT;
                        case "wf_xtra_noteffecttp": return InteractionType.WF_XTRA_NOTEFFECTTP;
                        case "wf_xtra_notfreezetp": return InteractionType.WF_XTRA_NOTFREEZETP;
                        case "wf_xtra_notteam": return InteractionType.WF_XTRA_NOTTEAM;
                        case "wf_xtra_one_condition": return InteractionType.WF_XTRA_ONE_CONDITION;
                        case "wf_xtra_or_eval": return InteractionType.WF_XTRA_OR_EVAL;
                        case "wf_xtra_pt_unseenrandom": return InteractionType.WF_XTRA_PT_UNSEENRANDOM;
                        case "wf_xtra_random": return InteractionType.WF_XTRA_RANDOM;
                        case "wf_xtra_sequence": return InteractionType.WF_XTRA_SEQUENCE;
                        case "wf_xtra_text_output_furni_name": return InteractionType.WF_XTRA_TEXT_OUTPUT_FURNI_NAME;
                        case "wf_xtra_text_output_username": return InteractionType.WF_XTRA_TEXT_OUTPUT_USERNAME;
                        case "wf_xtra_unseen": return InteractionType.WF_XTRA_UNSEEN;
                        case "wf_xtra_unseen_random": return InteractionType.WF_XTRA_UNSEEN_RANDOM;
                        case "wf_xtra_unseenrandom": return InteractionType.WF_XTRA_UNSEENRANDOM;
                        case "wf_xtra_usability": return InteractionType.WF_XTRA_USABILITY;
                        case "wf_xtra_usabilitycond": return InteractionType.WF_XTRA_USABILITYCOND;
                        case "wf_xtra_useraround": return InteractionType.WF_XTRA_USERAROUND;
                        case "wf_xtra_userbehind": return InteractionType.WF_XTRA_USERBEHIND;
                        case "wf_xtra_userinfront": return InteractionType.WF_XTRA_USERINFRONT;
                        case "wf_xtra_xixixi": return InteractionType.WF_XTRA_XIXIXI;
                    }

                        if (pType.StartsWith("pet") && int.TryParse(pType.Replace("pet", ""), out int _))
                            return InteractionType.PET;
                        return InteractionType.NONE;
                }
        }
        }
    }
}