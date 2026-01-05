using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Polar.Communication.Packets.Outgoing;
using Polar.Core;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Navigator;
using Polar.Communication.Packets.Outgoing.Navigator;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Communication.Packets.Outgoing.Rooms.Settings;
using Polar.Database.Interfaces;


namespace Polar.Communication.Packets.Incoming.Rooms.Settings
{
    internal class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            int RoomId = Packet.PopInt();

            Room Room = PolarEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            string Name = Packet.PopString();
            string Description = Packet.PopString();
            int State = Packet.PopInt();
            string Password = Packet.PopString();
            int MaxUsers = Packet.PopInt();
            int CategoryId = Packet.PopInt();
            int TagCount = Packet.PopInt();
            List<string> tags = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < TagCount; ++index)
            {
                if (index > 0)
                    stringBuilder.Append(",");
                string tag = Packet.PopString().ToLower();
                tags.Add(tag);
                stringBuilder.Append(tag);
            }

            int TradeSettings = Packet.PopInt();//2 = All can trade, 1 = owner only, 0 = no trading.
            bool AllowPets = Packet.PopBoolean();
            bool AllowPetsEat = Packet.PopBoolean();
            bool RoomBlockingEnabled = Packet.PopBoolean();
            bool Hidewall = Packet.PopBoolean();
            int WallThickness = Packet.PopInt();
            int FloorThickness = Packet.PopInt();
            int WhoMute = Packet.PopInt(); // mute
            int WhoKick = Packet.PopInt(); // kick
            int WhoBan = Packet.PopInt(); // ban

            int chatMode = Packet.PopInt();
            int chatSize = Packet.PopInt();
            int chatSpeed = Packet.PopInt();
            int chatDistance = Packet.PopInt();
            int extraFlood = Packet.PopInt();

            if (chatDistance > 99)
                chatDistance = 99;

            if (TradeSettings < 0 || TradeSettings > 2)
                TradeSettings = 0;

            if (TagCount > 2 || WhoMute != 0 && WhoMute != 1 || WhoKick != 0 && WhoKick != 1 && WhoKick != 2 || WhoBan != 0 && WhoBan != 1)
                return;

            if (WallThickness < -2 || WallThickness > 1)
                WallThickness = 0;

            if (FloorThickness < -2 || FloorThickness > 1)
                FloorThickness = 0;

            if (State < 0 || State > 3)
                return;

            if (Name.Length < 1 || Name.Length > 100)
                return;


            if (MaxUsers < 10 || MaxUsers > 75)
                MaxUsers = 25;

            Room.AllowPets = AllowPets;
            Room.AllowPetsEating = AllowPetsEat;
            Room.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.Hidewall = Hidewall;

            Room.RoomData.AllowPets = AllowPets;
            Room.RoomData.AllowPetsEating = AllowPetsEat;
            Room.RoomData.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.RoomData.Hidewall = Hidewall;

            Room.Name = Name;
            Room.Description = Description;
            Room.Category = CategoryId;
            Room.Password = Password;
            Room.State = State;

            Room.RoomData.Name = Name;
            Room.RoomData.State = State;
            Room.RoomData.Description = Description;
            Room.RoomData.Category = CategoryId;
            Room.RoomData.Password = Password;

            Room.WhoCanBan = WhoBan;
            Room.WhoCanKick = WhoKick;
            Room.WhoCanMute = WhoMute;
            Room.RoomData.WhoCanBan = WhoBan;
            Room.RoomData.WhoCanKick = WhoKick;
            Room.RoomData.WhoCanMute = WhoMute;

            Room.ClearTags();
            Room.AddTagRange(tags);
            Room.UsersMax = MaxUsers;

            Room.RoomData.Tags.Clear();
            Room.RoomData.Tags.AddRange((IEnumerable<string>)tags);
            Room.RoomData.UsersMax = MaxUsers;

            Room.WallThickness = WallThickness;
            Room.FloorThickness = FloorThickness;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.FloorThickness = FloorThickness;

            Room.chatMode = chatMode;
            Room.chatSize = chatSize;
            Room.chatSpeed = chatSpeed;
            Room.chatDistance = chatDistance;
            Room.extraFlood = extraFlood;

            Room.TradeSettings = TradeSettings;

            Room.RoomData.chatMode = chatMode;
            Room.RoomData.chatSize = chatSize;
            Room.RoomData.chatSpeed = chatSpeed;
            Room.RoomData.chatDistance = chatDistance;
            Room.RoomData.extraFlood = extraFlood;

            Room.RoomData.TradeSettings = TradeSettings;

            string str5 = "open";
            if (Room.RoomData.State == 1)
                str5 = "locked";
            else if (Room.RoomData.State == 2)
                str5 = "password";
            else if (Room.RoomData.State == 3)
                str5 = "hide";

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE rooms SET caption = @caption, description = @description, password = @password, category = " +
                    CategoryId + ", state = '" + str5 + "', tags = @tags, users_max = " + MaxUsers +
                    ", allow_pets = '" + (AllowPets ? 1 : 0) + "', allow_pets_eat = '" + (AllowPetsEat ? 1 : 0) + "', room_blocking_disabled = '" +
                    (RoomBlockingEnabled ? 1 : 0) + "', allow_hidewall = '" + (Room.RoomData.Hidewall ? 1 : 0) + "', floorthick = " +
                    Room.FloorThickness + ", wallthick = " + Room.WallThickness + ", mute_settings='" + Room.WhoCanMute +
                    "', kick_settings='" + Room.WhoCanKick + "',ban_settings='" + Room.WhoCanBan + "', `chat_mode` = '" + Room.chatMode + "', `chat_size` = '" + Room.chatSize + "', `chat_speed` = '" + Room.chatSpeed + "', `chat_extra_flood` = '" + Room.extraFlood + "', `chat_hearing_distance` = '" + Room.chatDistance + "', `trade_settings` = '" + Room.TradeSettings + "' WHERE `id` = '" + Room.RoomId + "' LIMIT 1");
                dbClient.AddParameter("caption", Room.Name);
                dbClient.AddParameter("description", Room.Description);
                dbClient.AddParameter("password", Room.Password);
                dbClient.AddParameter("tags", (stringBuilder).ToString());
                dbClient.RunQuery();
            }


            if (Session.GetHabbo().CurrentRoom == null)
            {
                Session.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Session.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, Room.Hidewall));
            }
            else
            {
                Room.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Room.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Room.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, Room.Hidewall));
            }

            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModDoorModeSeen", 1);
            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);

            Session.SendMessage(new GetGuestRoomResultComposer(Session, Room.RoomData, true, false));
        }
    }
}
