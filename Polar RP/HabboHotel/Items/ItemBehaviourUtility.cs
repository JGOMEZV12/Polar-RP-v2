using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Users;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.Items.Data.Toner;
using Polar.HabboHotel.Items.Data.RentableSpace;
using Polar.HabboHotel.Items.Interactor;
using Polar.Communication.Packets.Outgoing;
using Polar.HabboHotel.Cache;
using Polar.HabboHotel.Rooms;

namespace Polar.HabboHotel.Items
{
    static class ItemBehaviourUtility
    {
        
        public static void GenerateExtradata(Item Item, ServerPacket Message)
        {
            if (Item == null)
                throw new ArgumentNullException(nameof(Item), "El objeto Item es null.");

            var baseItem = Item.GetBaseItem();
            if (baseItem == null)
                throw new InvalidOperationException("El objeto BaseItem no puede ser null.");

            if (Message == null)
                throw new ArgumentNullException(nameof(Message), "El objeto Message es null.");


            switch (Item.GetBaseItem().InteractionType)
            {
                default:
                    Message.WriteInteger(1);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.GetBaseItem().InteractionType != InteractionType.FOOTBALL_GATE ? Item.ExtraData : string.Empty);
                    break;
                case InteractionType.WIRED_HIGHSCORE:
                    string username;
                    string name = Item.GetBaseItem().ItemName;
                    string type = name?.Split('*').ElementAtOrDefault(1);

                    if (type != null)
                    {
                        Dictionary<int, KeyValuePair<int, string>> ScoreBordata = new Dictionary<int, KeyValuePair<int, string>>();
                        Message.WriteInteger(0);
                        Message.WriteInteger(6);
                        Message.WriteString(Item.ExtraData ?? string.Empty);
                        if (Item.GetBaseItem().ItemName.StartsWith("highscore_classic"))
                            Message.WriteInteger(2);
                        else if (Item.GetBaseItem().ItemName.StartsWith("highscore_mostwin"))
                            Message.WriteInteger(1);
                        else if (Item.GetBaseItem().ItemName.StartsWith("highscore_perteam"))
                            Message.WriteInteger(0);

                        var room = Item.GetRoom();
                        if (room != null)
                        {
                            switch (type)
                            {
                                case "2":
                                    Message.WriteInteger(1);
                                    Message.WriteInteger(room.WiredScoreBordDay?.Count ?? 0);
                                    ScoreBordata = room.WiredScoreBordDay ?? new Dictionary<int, KeyValuePair<int, string>>();
                                    break;

                                case "3":
                                    Message.WriteInteger(2);
                                    Message.WriteInteger(room.WiredScoreBordWeek?.Count ?? 0);
                                    ScoreBordata = room.WiredScoreBordWeek ?? new Dictionary<int, KeyValuePair<int, string>>();
                                    break;


                                case "4":
                                    Message.WriteInteger(3);
                                    Message.WriteInteger(room.WiredScoreBordMonth?.Count ?? 0);
                                    ScoreBordata = room.WiredScoreBordMonth ?? new Dictionary<int, KeyValuePair<int, string>>();
                                    break;

                                default:
                                    Message.WriteInteger(1);
                                    Message.WriteInteger(0);
                                    ScoreBordata = null;
                                    break;
                            }
                        }
                        else
                        {
                            Message.WriteInteger(1);
                            Message.WriteInteger(1);
                            Message.WriteInteger(0);
                            Message.WriteInteger(1);
                            Message.WriteString("Este marcador no funciona todavía: (");
                        }

                        if (ScoreBordata?.Count > 0)
                        {
                            foreach (var value in ScoreBordata.OrderByDescending(i => i.Value.Key).Select(i => i.Value))
                            {
                                username = value.Value;
                                Message.WriteInteger(value.Key);
                                Message.WriteInteger(1);
                                Message.WriteString(string.IsNullOrEmpty(username) ? string.Empty : username);
                            }
                        }
                    }
                    break;
                /*case InteractionType.CAMERA_PICTURE:
                    var str = (Item.Interactor as InteractorCameraPicture).GetJsonData(Item);
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString(str);
                    break;*/
                case InteractionType.MUSIC_DISC:
                    if (!int.TryParse(Item.ExtraData, out int issx))
                        issx = 0;

                    Message.WriteInteger(issx);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData ?? string.Empty);
                    break;
                case InteractionType.GNOME_BOX:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData ?? string.Empty);
                    break;

                case InteractionType.PET_BREEDING_BOX:
                case InteractionType.PURCHASABLE_CLOTHING:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString("0");
                    break;

                case InteractionType.STACKTOOL:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString("");
                    break;

                case InteractionType.WALLPAPER:
                    Message.WriteInteger(2);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData ?? string.Empty);

                    break;
                case InteractionType.FLOOR:
                    Message.WriteInteger(3);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData ?? string.Empty);
                    break;

                case InteractionType.LANDSCAPE:
                    Message.WriteInteger(4);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData ?? string.Empty);
                    break;
                case InteractionType.GUILD_ITEM:
                case InteractionType.GUILD_GATE:
                case InteractionType.GUILD_FORUM:
                    Group group = null;
                    if (Item.GroupId > 1000)
                        group = GroupManager.GetGang(Item.GroupId);
                    else
                        group = GroupManager.GetJob(Item.GroupId);


                    if (group == null)
                    {
                        Message.WriteInteger(1);
                        Message.WriteInteger(0);
                        Message.WriteString(Item.ExtraData ?? string.Empty);
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(2);
                        Message.WriteInteger(5);
                        Message.WriteString(Item.ExtraData ?? string.Empty);
                        Message.WriteString(group.Id.ToString());
                        Message.WriteString(group.Badge ?? string.Empty);
                        Message.WriteString(group.Colour1 ?? string.Empty);
                        Message.WriteString(group.Colour2 ?? string.Empty);
                        //Message.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true)); // Group Colour 1
                        //Message.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false)); // Group Colour 2
                    }
                    break;

                case InteractionType.BACKGROUND:
                case InteractionType.INFORMATION_TERMINAL:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    if (!String.IsNullOrEmpty(Item.ExtraData))
                    {
                        Message.WriteInteger(Item.ExtraData.Split(Convert.ToChar(9)).Length / 2);

                        for (int i = 0; i <= Item.ExtraData.Split(Convert.ToChar(9)).Length - 1; i++)
                        {
                            Message.WriteString(Item.ExtraData.Split(Convert.ToChar(9))[i]);
                        }
                    }
                    else
                    {
                        Message.WriteInteger(0);
                    }
                    break;

                case InteractionType.GIFT:
                    string[] extraData = Item.ExtraData?.Split(Convert.ToChar(5)) ?? Array.Empty<string>();
                    if (extraData.Length != 7)
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString(Item.ExtraData ?? string.Empty);
                    }
                    else
                    {
                        int style = int.Parse(extraData[6]) * 1000 + int.Parse(extraData[6]);

                        using (UserCache purchaser = PolarEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(extraData[2])))
                        {
                            if (purchaser == null)
                            {
                                Message.WriteInteger(0);
                                Message.WriteInteger(0);
                                Message.WriteString(Item.ExtraData ?? string.Empty);
                            }
                            else
                            {
                                Message.WriteInteger(style);
                                Message.WriteInteger(1);
                                Message.WriteInteger(6);
                                Message.WriteString("EXTRA_PARAM");
                                Message.WriteString(string.Empty);
                                Message.WriteString("MESSAGE");
                                Message.WriteString(extraData[1]);
                                Message.WriteString("PURCHASER_NAME");
                                Message.WriteString(purchaser.Username ?? string.Empty);
                                Message.WriteString("PURCHASER_FIGURE");
                                Message.WriteString(purchaser.Look ?? string.Empty);
                                Message.WriteString("PRODUCT_CODE");
                                Message.WriteString("A1 KUMIANKKA");
                                Message.WriteString("state");
                                Message.WriteString(Item.MagicRemove ? "1" : "0");
                            }
                        }
                    }
                    break;

                case InteractionType.FARMING:
                    Message.WriteInteger(0);
                    int cracks = 0;
                    int cracks_max = 4;

                    int.TryParse(Item.ExtraData, out cracks);

                    string state = "0";

                    if (cracks >= 4)
                        state = "8";
                    else if (cracks >= 3)
                        state = "6";
                    else if (cracks >= 2)
                        state = "4";
                    else if (cracks >= 1)
                        state = "2";

                    Message.WriteInteger(7);
                    Message.WriteString(state); 
                    Message.WriteInteger(cracks); 
                    Message.WriteInteger(cracks_max);
                    break;

                case InteractionType.CRACKABLE_EGG:
                    Message.WriteInteger(0);
                    Message.WriteInteger(7);
                    Message.WriteString("8");
                    Message.WriteInteger(9); 
                    Message.WriteInteger(12); 
                    break;

                case InteractionType.MANNEQUIN:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(3);
                    if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        string[] Stuff = Item.ExtraData.Split(Convert.ToChar(5));
                        Message.WriteString("GENDER");
                        Message.WriteString(Stuff[0]);
                        Message.WriteString("FIGURE");
                        Message.WriteString(Stuff[1]);
                        Message.WriteString("OUTFIT_NAME");
                        Message.WriteString(Stuff[2]);
                    }
                    else
                    {
                        Message.WriteString("GENDER");
                        Message.WriteString("");
                        Message.WriteString("FIGURE");
                        Message.WriteString("");
                        Message.WriteString("OUTFIT_NAME");
                        Message.WriteString("");
                    }
                    break;

                case InteractionType.TONER:
                    if (Item.RoomId != 0)
                    {
                        if (Item.GetRoom().TonerData == null)
                            Item.GetRoom().TonerData = new TonerData(Item.Id);

                        Message.WriteInteger(0);
                        Message.WriteInteger(5);
                        Message.WriteInteger(4);
                        Message.WriteInteger(Item.GetRoom().TonerData.Enabled);
                        Message.WriteInteger(Item.GetRoom().TonerData.Hue);
                        Message.WriteInteger(Item.GetRoom().TonerData.Saturation);
                        Message.WriteInteger(Item.GetRoom().TonerData.Lightness);
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString(string.Empty);
                    }
                    break;

                case InteractionType.BADGE_DISPLAY:
                    Message.WriteInteger(0);
                    Message.WriteInteger(2);
                    Message.WriteInteger(4);

                    string[] BadgeData = Item.ExtraData.Split(Convert.ToChar(9));
                    if (Item.ExtraData.Contains(Convert.ToChar(9).ToString()))
                    {
                        Message.WriteString("0");//No idea
                        Message.WriteString(BadgeData[0]);//Badge name
                        Message.WriteString(BadgeData[1]);//Owner
                        Message.WriteString(BadgeData[2]);//Date
                    }
                    else
                    {
                        Message.WriteString("0");//No idea
                        Message.WriteString("DEV");//Badge name
                        Message.WriteString("Sledmore");//Owner
                        Message.WriteString("13-13-1337");//Date
                    }
                    break;

                case InteractionType.TELEVISION:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(1);

                    Message.WriteString("THUMBNAIL_URL");
                    Message.WriteString("/youtubethumbnail.php?img=" + PolarEnvironment.GetGame().GetTelevisionManager().TelevisionList.OrderBy(x => Guid.NewGuid()).FirstOrDefault().YouTubeId);
                    break;

                case InteractionType.LOVELOCK:
                    if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var EData = Item.ExtraData.Split((char)5);
                        int I = 0;
                        Message.WriteInteger(0);
                        Message.WriteInteger(2);
                        Message.WriteInteger(EData.Length);
                        while (I < EData.Length)
                        {
                            Message.WriteString(EData[I]);
                            I++;
                        }
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString("0");
                    }
                    break;

                case InteractionType.MONSTERPLANT_SEED:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(1);

                    Message.WriteString("rarity");
                    Message.WriteString("1");// Should really be a random generated rarity.
                    break;
            }
        }

        public static void GenerateWallExtradata(Item Item, ServerPacket Message)
        {
            switch (Item.GetBaseItem().InteractionType)
            {
                default:
                    Message.WriteString(Item.ExtraData);
                    break;
                /*case InteractionType.CAMERA_PICTURE:
                    Message.WriteString((Item.Interactor as InteractorCameraPicture).GetJsonData(Item));
                    break;*/
                case InteractionType.POSTIT:
                    Message.WriteString(Item.ExtraData.Split(' ')[0]);
                    break;
            }
        }
    }
}