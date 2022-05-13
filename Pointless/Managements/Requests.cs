namespace Pointless.Managements
{
    public class Requests
    {
        public static Dictionary<string, Request> GetRequests(ulong guildId)
        {
            return Guild.Get(guildId).Requests;
        }

        public static Request GetRequest(ulong guildId, string id)
        {
            return GetRequests(guildId)[id];
        }

        public static string Add(ulong guildId, Request item)
        {
            Guild guild = Guild.Get(guildId);
            Dictionary<string, Request> requests = guild.Requests;

            string id = guild.RequestCount.ToString();

            requests.Add(id, item);

            Guild.Set(guildId, g => g.Requests, requests);
            Guild.Set(guildId, g => g.RequestCount, int.Parse(id) + 1);

            return id;
        }

        public static void Remove(ulong guildId, string id)
        {
            Dictionary<string, Request> requests = GetRequests(guildId);

            requests.Remove(id);

            Guild.Set(guildId, g => g.Requests, requests);
        }

        public static bool HasRequest(ulong guildId, string id)
        {
            Dictionary<string, Request> requests = GetRequests(guildId);

            return requests.ContainsKey(id);
        }
    }
}
