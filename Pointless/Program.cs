using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Pointless.Managements;
using Pointless.TypeConverters;
using System.Reflection;

namespace Pointless
{
    public class Program
    {
        public static DiscordSocketClient Client;
        public static InteractionService Interaction;
        public static IServiceProvider Service;

        public static readonly bool IsDebugMode = Configs.Get("DEBUG_MODE") == "True";

        private static readonly DiscordSocketConfig clientConfig = new()
        {
            LogLevel = IsDebugMode ? LogSeverity.Debug : LogSeverity.Info,
            MessageCacheSize = 100000,
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.AllUnprivileged
        };

        private static readonly InteractionServiceConfig InteractionConfig = new()
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = IsDebugMode ? LogSeverity.Debug : LogSeverity.Info,
        };

        private static async Task Main()
        {
            Client = new DiscordSocketClient(clientConfig);
            Interaction = new(Client, InteractionConfig);
            Service = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Interaction)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            Interaction.AddGenericTypeConverter<Reward>(typeof(RewardTypeConverter<>));

            await Interaction.AddModulesAsync(Assembly.GetEntryAssembly(), Service);

            new EventHandler(Client, Interaction).RegisterEvents();

            string token = Configs.Get("TOKEN");
            await Client.LoginAsync(TokenType.Bot, token);

            await Client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
