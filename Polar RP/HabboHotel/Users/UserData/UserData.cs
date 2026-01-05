using System.Collections;
using System.Collections.Generic;
using Polar.HabboHotel.Achievements;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Rooms.AI;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users.Badges;
using Polar.HabboHotel.Users.Inventory;
using Polar.HabboHotel.Users.Messenger;
using Polar.HabboHotel.Users.Relationships;
using System.Collections.Concurrent;
using Polar.HabboHotel.Subscriptions;

namespace Polar.HabboHotel.Users.UserDataManagement
{
    public class UserData
    {
        public int userID;
        public Habbo user;
        public Dictionary<string, Subscription> subscriptions;
        //public Dictionary<int, UserTalent> Talents;
        public Dictionary<int, Relationship> Relations;
        public ConcurrentDictionary<string, UserAchievement> achievements;
        public List<Badge> badges;
        public List<int> favouritedRooms;
        public Dictionary<int, MessengerRequest> requests;
        public Dictionary<int, MessengerBuddy> friends;
        public List<int> ignores;
        public Dictionary<int, int> quests;
        public List<RoomData> rooms;
        public HashSet<int> SuggestedPolls;
        //public List<string> Tags;

        public UserData(int userID, ConcurrentDictionary<string, UserAchievement> achievements, List<int> favouritedRooms, List<int> ignores,
            List<Badge> badges, Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests, List<RoomData> rooms, Dictionary<int, int> quests, Habbo user, 
            Dictionary<int, Relationship> Relations, Dictionary<string, Subscription> subscriptions/*, Dictionary<int, UserTalent> talents, List<string> tags*/)
        {
            this.userID = userID;
            this.achievements = achievements;
            this.favouritedRooms = favouritedRooms;
            this.ignores = ignores;
            this.badges = badges;
            this.friends = friends;
            this.requests = requests;
            this.rooms = rooms;
            this.quests = quests;
            this.user = user;
            this.Relations = Relations;
            this.subscriptions = subscriptions;
            //this.Talents = talents;
            //this.Tags = tags; 
        }
    }
}