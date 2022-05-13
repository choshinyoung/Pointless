using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Pointless.Managements
{
    public class DB
    {
        public static MongoClient Client = new("mongodb://localhost");
        public static MongoDatabaseBase Db = (MongoDatabaseBase)Client.GetDatabase("pointlessDb");

        public static IMongoCollection<Guild> Guilds = Db.GetCollection<Guild>("Guilds");
    }

    public class Guild
    {
        public ObjectId Id;

        public ulong GuildId;

        public Dictionary<string, uint> Points;

        public List<Reward> Rewards;
        public Dictionary<string, Request> Requests;

        public List<Toto> Toto;

        public int RequestCount;

        public Guild(ulong id)
        {
            GuildId = id;
            Points = new();
            Rewards = new();

            Requests = new();

            Toto = new();

            RequestCount = 0;
        }

        public static Guild Get(ulong id)
        {
            IFindFluent<Guild, Guild> searchResult = DB.Guilds.Find(g => g.GuildId == id);

            if (searchResult.Any())
            {
                return searchResult.Single();
            }
            else
            {
                Guild guild = new(id);
                DB.Guilds.InsertOne(guild);

                return guild;
            }
        }

        public static void Set<T>(ulong id, Expression<Func<Guild, T>> field, T value)
        {
            DB.Guilds.UpdateOne(g => g.GuildId == id, Builders<Guild>.Update.Set(field, value));
        }
    }

    public class Reward
    {
        public string Name;
        public string Description;
        public uint Point;

        public Reward(string name, string description, uint point)
        {
            Name = name;
            Description = description;
            Point = point;
        }
    }

    public class Request
    {
        public ulong UserId;
        public string Reward;
        public long Timestamp;

        public Request(ulong userId, string reward, long timestamp)
        {
            UserId = userId;
            Reward = reward;
            Timestamp = timestamp;
        }
    }

    public class Toto
    {
        public string Name;

        public Dictionary<string, List<Item>> Items;

        public bool IsPredictionEnded;

        public ulong ChannelId;
        public ulong MessageId;

        public Toto(string name, List<string> items, ulong channelId, ulong messageId)
        {
            Name = name;
            Items = items.ToDictionary(i => i, i => new List<Item>());
            IsPredictionEnded = false;
            ChannelId = channelId;
            MessageId = messageId;
        }

        public class Item
        {
            public ulong UserId;
            public uint Point;

            public Item(ulong userId, uint point)
            {
                UserId = userId;
                Point = point;
            }
        }
    }
}
