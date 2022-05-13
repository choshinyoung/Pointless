namespace Pointless.Managements
{
    public class Totos
    {
        public static List<Toto> GetTotos(ulong guildId)
        {
            return Guild.Get(guildId).Toto;
        }

        public static Toto GetToto(ulong guildId, string name)
        {
            return GetTotos(guildId).Find(t => t.Name == name)!;
        }

        public static void AddToto(ulong guildId, string name, List<string> items, ulong channelId, ulong messageId)
        {
            List<Toto> totos = GetTotos(guildId);

            totos.Add(new(name, items, channelId, messageId));

            Guild.Set(guildId, g => g.Toto, totos);
        }

        public static void RemoveToto(ulong guildId, string name)
        {
            List<Toto> totos = GetTotos(guildId);

            totos.RemoveAll(t => t.Name == name);

            Guild.Set(guildId, g => g.Toto, totos);
        }

        public static void AddPoint(ulong guildId, string name, ulong userId, string item, uint point = 10)
        {
            List<Toto> totos = GetTotos(guildId);

            int index = totos.FindIndex(t => t.Name == name);
            Toto toto = totos[index];

            if (toto.Items[item].Any(i => i.UserId == userId))
            {
                int userIndex = toto.Items[item].FindIndex(i => i.UserId == userId);

                toto.Items[item][userIndex] = new Toto.Item(userId, toto.Items[item][userIndex].Point + point);
            }
            else
            {
                toto.Items[item].Add(new Toto.Item(userId, point));
            }

            totos[index] = toto;

            Guild.Set(guildId, g => g.Toto, totos);
        }

        public static void EndPrediction(ulong guildId, string name)
        {
            List<Toto> totos = GetTotos(guildId);

            int index = totos.FindIndex(t => t.Name == name);

            totos[index].IsPredictionEnded = true;

            Guild.Set(guildId, g => g.Toto, totos);
        }

        public static void ChangeMessage(ulong guildId, string name, ulong messageId)
        {
            List<Toto> totos = GetTotos(guildId);

            int index = totos.FindIndex(t => t.Name == name);

            totos[index].MessageId = messageId;

            Guild.Set(guildId, g => g.Toto, totos);
        }

        public static bool HasToto(ulong guildId, string name)
        {
            List<Toto> totos = GetTotos(guildId);

            return totos.Any(t => t.Name == name);
        }
    }
}
