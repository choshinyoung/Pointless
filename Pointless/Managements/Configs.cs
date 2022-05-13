using Microsoft.Extensions.Configuration;

namespace Pointless.Managements
{
    internal class Configs
    {
        private static readonly IConfigurationRoot config;

        static Configs()
        {
            config = new ConfigurationBuilder().AddJsonFile("Configs/configs.json").Build();
        }

        public static string Get(string key)
        {
            return config.GetSection(key).Value;
        }
    }
}
