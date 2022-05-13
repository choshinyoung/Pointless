using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace Pointless.Managements
{
    public class Points
    {
        private static readonly Dictionary<ulong, Dictionary<ulong, int>> length = new();
        private static readonly Dictionary<ulong, Dictionary<ulong, float>> extraPoints = new();
        private static readonly Dictionary<ulong, Dictionary<ulong, List<(string content, DateTimeOffset time)>>> prvMsgs = new();

        private static readonly char[] IGNORE_CHARACTERS = new[] { '\u200F', '\u200b' };
        private static readonly char[] BLANK_CHARACTERS = new[] { ' ', '\n' };
        private static readonly Regex EMOJI_REGEX = new(@"<a?(:\w*:)\d+>");
        private static readonly Regex MENTION_REGEX = new(@"<[@#]\d+>");

        private static void InitUser(ulong guildId, ulong userId)
        {
            if (!length.ContainsKey(guildId))
            {
                length.Add(guildId, new());
            }

            if (!extraPoints.ContainsKey(guildId))
            {
                extraPoints.Add(guildId, new());
            }

            if (!prvMsgs.ContainsKey(guildId))
            {
                prvMsgs.Add(guildId, new());
            }

            if (!length[guildId].ContainsKey(userId))
            {
                length[guildId].Add(userId, 0);
            }

            if (!extraPoints[guildId].ContainsKey(userId))
            {
                extraPoints[guildId].Add(userId, 0);
            }

            if (!prvMsgs[guildId].ContainsKey(userId))
            {
                prvMsgs[guildId].Add(userId, new());
            }
        }

        private static void InitDbUser(ulong guildId, ulong userId)
        {
            string u = userId.ToString();

            Dictionary<string, uint> points = Guild.Get(guildId).Points;

            if (!points.ContainsKey(u))
            {
                points.Add(u, 0);
            }

            Guild.Set(guildId, g => g.Points, points);
        }

        public static uint GetPoint(ulong guildId, ulong userId)
        {
            InitDbUser(guildId, userId);

            Dictionary<string, uint> points = Guild.Get(guildId).Points;

            return points[userId.ToString()];
        }

        public static void AddPoint(SocketMessage msg)
        {
            if (msg.Author.IsBot)
            {
                return;
            }

            if (msg.Channel is not SocketTextChannel channel)
            {
                return;
            }

            InitUser(channel.Guild.Id, msg.Author.Id);

            length[channel.Guild.Id][msg.Author.Id] += GetLength(msg.Content);

            if ((length[channel.Guild.Id][msg.Author.Id] >= 10 && !IsDuplicated(channel.Guild.Id, msg.Author.Id, msg.Content) && !IsSpam(channel.Guild.Id, msg.Author.Id, msg.CreatedAt)) || msg.Attachments.Any() || msg.Stickers.Any())
            {
                float point = 1;

                SocketGuildUser user = Program.Client.GetGuild(channel.Guild.Id).GetUser(msg.Author.Id);
                if (user.PremiumSince.HasValue)
                {
                    point *= float.Parse(Configs.Get("BOOST_MULTIPLIER"));
                }

                AddFloatPoint(channel.Guild.Id, msg.Author.Id, point);

                length[channel.Guild.Id][msg.Author.Id] = 0;
            }

            prvMsgs[channel.Guild.Id][msg.Author.Id].Add((msg.Content, msg.CreatedAt));
            if (prvMsgs[channel.Guild.Id][msg.Author.Id].Count > 3)
            {
                prvMsgs[channel.Guild.Id][msg.Author.Id].RemoveAt(0);
            }
        }

        public static void AddFloatPoint(ulong guildId, ulong userId, float amount = 1)
        {
            InitUser(guildId, userId);
            InitDbUser(guildId, userId);

            extraPoints[guildId][userId] += amount;
            uint point = (uint)Math.Floor(extraPoints[guildId][userId]);
            extraPoints[guildId][userId] %= 1;

            AddPoint(guildId, userId, point);
        }

        public static void AddPoint(ulong guildId, ulong userId, uint amount = 1)
        {
            InitDbUser(guildId, userId);

            string u = userId.ToString();

            Dictionary<string, uint> points = Guild.Get(guildId).Points;

            points[u] += amount;

            Guild.Set(guildId, g => g.Points, points);
        }

        public static void RemovePoint(ulong guildId, ulong userId, uint amount = 1)
        {
            InitDbUser(guildId, userId);

            string u = userId.ToString();

            Dictionary<string, uint> points = Guild.Get(guildId).Points;

            if (points[u] < amount)
            {
                points[u] = 0;
            }

            points[u] -= amount;

            Guild.Set(guildId, g => g.Points, points);
        }

        public static void SetPoint(ulong guildId, ulong userId, uint point = 1)
        {
            InitDbUser(guildId, userId);

            string u = userId.ToString();

            Dictionary<string, uint> points = Guild.Get(guildId).Points;

            points[u] = point;

            Guild.Set(guildId, g => g.Points, points);
        }

        private static int GetLength(string content)
        {
            content = new(content.Where(c => !IGNORE_CHARACTERS.Contains(c)).ToArray());
            content = EMOJI_REGEX.Replace(content, ".");
            content = MENTION_REGEX.Replace(content, ".");

            int length = content.Length - content.Count(c => BLANK_CHARACTERS.Contains(c)) / 2;

            return length;
        }

        private static bool IsDuplicated(ulong guildId, ulong userId, string content)
        {
            bool isDuplicated = false;

            foreach ((string content, DateTimeOffset time) prvMsg in prvMsgs[guildId][userId])
            {
                if (GetAccuracy(prvMsg.content, content) >= float.Parse(Configs.Get("DUPLICATION_MIN_ACCURACY")))
                {
                    isDuplicated = true;

                    break;
                }
            }

            return isDuplicated;
        }

        private static bool IsSpam(ulong guildId, ulong userId, DateTimeOffset time)
        {
            if (prvMsgs[guildId][userId].Count >= 2)
            {
                if ((time - prvMsgs[guildId][userId].First().time).TotalMilliseconds <= int.Parse(Configs.Get("SPAM_MILISECOND")))
                {
                    return true;
                }
            }

            return false;
        }

        private static float GetAccuracy(string s1, string s2)
        {
            int[,] map = new int[s1.Length + 1, s2.Length + 1];
            int lcs = 0;

            for (int i = 0; i <= s1.Length; i++)
            {
                map[i, 0] = 0;
            }
            for (int i = 0; i <= s2.Length; i++)
            {
                map[0, i] = 0;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                    {
                        map[i, j] = map[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        map[i, j] = Math.Max(map[i - 1, j], map[i, j - 1]);
                    }

                    lcs = Math.Max(lcs, map[i, j]);
                }
            }

            return (float)lcs / Math.Max(s1.Length, s2.Length) * 100;
        }
    }
}
