using Polar.HabboHotel.BattlePass;
using Polar.HabboHotel.GameClients;
using System.Linq;

namespace Polar.Communication.Packets.Outgoing.BattlePass
{
    public class BattlePassComposer : ServerPacket
    {
        public BattlePassComposer(GameClient session, BattlePassManager manager)
            : base(ServerPacketHeader.BattlePassMessageComposer)
        {
            var userData = session.GetRoleplay().BattlePassData;
            var season = manager.CurrentSeason;
            int ranking = manager.Leaderboard.FindIndex(x => x.Key == session.GetHabbo().Id) + 1;
            if (ranking == 0) ranking = manager.Leaderboard.Count + 1;

            WriteInteger(season?.Chapter ?? 1);
            WriteInteger(season?.Season ?? 1);
            WriteInteger(userData.Level);
            WriteInteger(userData.Exp);
            WriteInteger(manager.Levels.ContainsKey(userData.Level + 1) ? manager.Levels[userData.Level + 1] : 0);
            WriteInteger(ranking);
            WriteBoolean(session.GetHabbo().VIPRank > 0);

            // Rewards
            WriteInteger(manager.Rewards.Count);
            foreach (var reward in manager.Rewards.Values)
            {
                WriteInteger(reward.Level);
                WriteString(reward.NormalRewardType);
                WriteString(reward.NormalRewardValue);
                WriteString(reward.NormalRewardIcon);
                WriteBoolean(userData.ClaimedNormalRewards.Contains(reward.Level));

                WriteString(reward.VipRewardType);
                WriteString(reward.VipRewardValue);
                WriteString(reward.VipRewardIcon);
                WriteBoolean(userData.ClaimedVipRewards.Contains(reward.Level));
            }

            // Categories
            WriteInteger(manager.Categories.Count);
            foreach (var category in manager.Categories)
            {
                WriteInteger(category.Id);
                WriteString(category.Name);
                WriteString(category.Icon);
                WriteString(category.Description);

                // Challenges in this category
                WriteInteger(category.Challenges.Count);
                foreach (var challenge in category.Challenges)
                {
                    WriteInteger(challenge.Id);
                    WriteString(challenge.Name);
                    WriteString(challenge.Icon);
                    WriteString(challenge.Description);
                    WriteInteger(challenge.XpReward);
                    WriteInteger(userData.ChallengeProgress.ContainsKey(challenge.Id) ? userData.ChallengeProgress[challenge.Id] : 0);
                    WriteInteger(challenge.TotalProgress);
                    WriteString(challenge.GoalType);
                }
            }
        }
    }
}
