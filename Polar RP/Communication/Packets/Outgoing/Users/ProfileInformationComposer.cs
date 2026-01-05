using System;
using System.Linq;
using System.Text;
using Polar.Database.Interfaces;
using System.Collections.Generic;
using Polar.HabboHotel.Cache;
using Polar.Utilities;
using Polar.HabboRoleplay.Bots;
using Polar.HabboRoleplay.Bots.Manager;
using Polar.HabboHotel.Users;
using Polar.HabboHotel.Groups;
using Polar.HabboHotel.GameClients;

namespace Polar.Communication.Packets.Outgoing.Users
{
    internal class ProfileInformationComposer : ServerPacket
    {
        public UserCache Data { get; }
        public List<Group> Groups { get; }
        public int friendCount;
        public RoleplayBot Bot { get; }
        public GameClient Session { get; }
        public ProfileInformationComposer(UserCache data, GameClient session, List<Group> groups, int FriendCount, RoleplayBot bot = null)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            Data = data;
            Session = session;
            Groups = groups;
            friendCount = FriendCount;
            Bot = bot;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            if (Bot != null)
            {
                BotProfile(Session, Groups, friendCount, Bot, packet);
            }
            else
            {
                
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                    dbClient.AddParameter("userid", Data.Id);
                    friendCount = dbClient.getInteger();
                }

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Data.AccountCreated).ToLocalTime();

                packet.WriteInteger(Data.Id);
                packet.WriteString(Data.Username);
                packet.WriteString(Data.Look);
                packet.WriteString(Data.Motto);
                packet.WriteString(origin.ToString("dd/MM/yyyy"));
                packet.WriteInteger((Data.GetStats() == null) ? 0 : Data.GetRoleplay().TimeWorked);
                packet.WriteInteger(friendCount); // Friend Count
                packet.WriteBoolean(Data.Id != Data.Id && Data.GetMessenger().FriendshipExists(Data.Id)); //  Is friend
                packet.WriteBoolean(Data.Id != Data.Id && !Data.GetMessenger().FriendshipExists(Data.Id) && Data.GetMessenger().RequestExists(Data.Id)); // Sent friend request
                packet.WriteBoolean((PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(Data.Id)) != null);

                packet.WriteInteger(Groups.Where(x => x != null).ToList().Count);
                foreach (Group Group in Groups)
                {
                    if (Group == null)
                        continue;

                    packet.WriteInteger(Group.Id);
                    packet.WriteString(Group.Name);
                    packet.WriteString(Group.Badge);
                    packet.WriteString(Group.Colour1);//PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(1, true)
                    packet.WriteString(Group.Colour2);//Group.Colour2
                    //packet.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, false));
                    //packet.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));
                    packet.WriteBoolean((Data.Id == 0 && Group.Id == 1) ? false : ((Group.Id < 1000 && Data.Id > 0) ? false : false)); // todo favs
                    packet.WriteInteger(0);//what the fuck
                    packet.WriteBoolean(Group != null ? Group.ForumEnabled : false);//HabboTalk
                }

                packet.WriteInteger(Convert.ToInt32(PolarEnvironment.GetUnixTimestamp() - Data.LastOnline)); // Last online
                packet.WriteBoolean(true); // Show the profile
            }
        }

        public void BotProfile(GameClient Session, List<Group> Groups, int friendCount, RoleplayBot Bot, ServerPacket packet)
        {
            int FakeBotId = Bot.Id + 1000000;
            Habbo Habbo = PolarEnvironment.GetHabboById(1);

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Habbo.AccountCreated).ToLocalTime();

            packet.WriteInteger(FakeBotId);
            packet.WriteString(Bot.Name);
            packet.WriteString(Bot.Figure);
            packet.WriteString("ciudadano");
            packet.WriteString(origin.ToString("dd/MM/yyyy"));
            packet.WriteInteger(0); // Achievement
            packet.WriteInteger(friendCount); // Friend Count
            packet.WriteBoolean(Session.GetRoleplay().FriendsWithBot(Bot.Id)); //  Is friend
            packet.WriteBoolean(false); // Sent friend request
            packet.WriteBoolean(true);

            packet.WriteInteger(Groups.Count);
            foreach (Group Group in Groups)
            {
                packet.WriteInteger(Group.Id);
                packet.WriteString(Group.Name);
                packet.WriteString(Group.Badge);
               // packet.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true));
                ///packet.WriteString(PolarEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));
                packet.WriteString(Group.Colour1);//PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(1, true)
                packet.WriteString(Group.Colour2);//Group.Colour2
                if (Group.Id == Bot.Corporation)
                    packet.WriteBoolean(true);
                else
                    packet.WriteBoolean(false);
                packet.WriteInteger(0);
                packet.WriteBoolean(Group != null ? Group.ForumEnabled : true);
            }

            packet.WriteInteger(0);
            packet.WriteBoolean(true);
        }
    }
}