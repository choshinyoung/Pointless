namespace Pointless.Managements
{
    public class Rewards
    {
        public static List<Reward> GetRewards(ulong guildId)
        {
            return Guild.Get(guildId).Rewards;
        }

        public static Reward GetReward(ulong guildId, string name)
        {
            return GetRewards(guildId).Find(r => r.Name == name);
        }

        public static bool HasReward(ulong guildId, string name)
        {
            return GetRewards(guildId).Any(g => g.Name == name);
        }

        public static void AddReward(ulong guildId, Reward reward)
        {
            List<Reward> rewards = GetRewards(guildId);

            rewards.Add(reward);

            Guild.Set(guildId, g => g.Rewards, rewards);
        }

        public static void RemoveReward(ulong guildId, string name)
        {
            List<Reward> rewards = GetRewards(guildId);

            rewards.RemoveAll(r => r.Name == name);

            Guild.Set(guildId, g => g.Rewards, rewards);
        }
    }
}
