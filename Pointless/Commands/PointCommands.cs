using Discord.Interactions;
using Discord.WebSocket;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands
{
    [Group("포인트", "포인트를 확인할 수 있는 커맨드입니다")]
    [EnabledInDm(false)]
    public class PointCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("확인", "해당 유저의 포인트를 확인합니다")]
        public async Task CheckPoint(SocketGuildUser? user = null)
        {
            user ??= (Context.User as SocketGuildUser)!;

            if (user.IsBot)
            {
                await RespondAsync("봇의 포인트를 확인할 수 없어요", ephemeral: true);

                return;
            }

            await Context.RespondAsync($"{user.Mention}님의 포인트: {Points.GetPoint(Context.Guild.Id, user.Id)}");
        }
    }
}
