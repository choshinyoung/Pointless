using Discord.Interactions;
using Discord.WebSocket;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands.Admins
{
    public partial class AdminCommands
    {
        [Group("포인트", "유저들의 포인트를 관리할 수 있는 커맨드입니다")]
        public class AdminPointCommands : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("추가", "유저에게 포인트를 추가하는 관리자 전용 커맨드입니다")]
            public async Task AddPoint(SocketGuildUser user, uint amount)
            {
                if (user.IsBot)
                {
                    await RespondAsync("봇에게 포인트를 추가할 수 없어요", ephemeral: true);

                    return;
                }

                Points.AddPoint(Context.Guild.Id, user.Id, amount);

                await Context.RespondAsync($"{user.Mention}님에게 {amount} 포인트를 추가했어요\n현재 포인트: {Points.GetPoint(Context.Guild.Id, user.Id)}");
            }

            [SlashCommand("제거", "유저에게서 포인트를 제거하는 관리자 전용 커맨드입니다")]
            public async Task RemovePoint(SocketGuildUser user, uint amount)
            {
                if (user.IsBot)
                {
                    await RespondAsync("봇에게서 포인트를 제거할 수 없어요", ephemeral: true);

                    return;
                }

                if (Points.GetPoint(Context.Guild.Id, user.Id) < amount)
                {
                    Dictionary<string, uint> points = Guild.Get(Context.Guild.Id).Points;

                    points[user.Id.ToString()] = 0;

                    Guild.Set(Context.Guild.Id, g => g.Points, points);

                    await Context.RespondAsync($"{user.Mention}님의 포인트를 모두 제거했어요");

                    return;
                }

                Points.RemovePoint(Context.Guild.Id, user.Id, amount);

                await Context.RespondAsync($"{user.Mention}님에게서 {amount} 포인트를 제거했어요\n현재 포인트: {Points.GetPoint(Context.Guild.Id, user.Id)}");
            }
        }
    }
}
