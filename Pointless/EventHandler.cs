using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pointless.Commands.Admins;
using Pointless.Managements;

namespace Pointless
{
    public class EventHandler
    {
        private readonly DiscordSocketClient client;
        private readonly InteractionService interaction;

        public EventHandler(DiscordSocketClient _client, InteractionService _interaction)
        {
            client = _client;
            interaction = _interaction;
        }

        public void RegisterEvents()
        {
            client.Log += OnLog;
            interaction.Log += OnCommandLog;

            client.InteractionCreated += OnInteractionCreated;
            interaction.SlashCommandExecuted += OnSlashCommandExecuted;

            client.MessageReceived += OnMessageReceived;
            client.ButtonExecuted += OnButtonExecuted;

            client.Ready += OnReady;
        }

        private async Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        private async Task OnCommandLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        private async Task OnReady()
        {
            await interaction.RegisterCommandsGloballyAsync();

            await Task.Factory.StartNew(VCCheckRoutine.Start);
        }

        private async Task OnInteractionCreated(SocketInteraction inter)
        {
            IServiceScope scope = Program.Service.CreateScope();
            SocketInteractionContext ctx = new(client, inter);

            await interaction.ExecuteCommandAsync(ctx, scope.ServiceProvider);

            if (ctx.Guild is not null)
            {
                await ctx.Guild.DownloadUsersAsync();
            }
        }

        private async Task OnSlashCommandExecuted(SlashCommandInfo cmd, IInteractionContext ctx, IResult res)
        {
            if (!res.IsSuccess && res.Error == InteractionCommandError.UnmetPrecondition)
            {
                await ctx.Interaction.RespondAsync("권한이 없어 커맨드를 실행할 수 없어요", ephemeral: true);
            }
        }

        private async Task OnMessageReceived(SocketMessage msg)
        {
            Points.AddPoint(msg);

            await Task.CompletedTask;
        }

        private async Task OnButtonExecuted(SocketMessageComponent btn)
        {
            if (btn.IsDMInteraction)
            {
                return;
            }

            await AdminCommands.AdminTotoCommands.HandleButtonEvent(btn, btn.Message.Id, btn.Data.CustomId);
        }
    }
}
