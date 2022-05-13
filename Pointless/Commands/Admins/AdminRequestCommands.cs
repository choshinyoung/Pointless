using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Pointless.AutoCompletes;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands.Admins
{
    public partial class AdminCommands
    {
        [Group("요청", "리워드 구매 요청을 확인할 수 있는 커맨드입니다")]
        public class RequestCommands : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractiveService Interactive { get; set; }

            [SlashCommand("목록", "승인을 대기중인 요청의 목록입니다")]
            public async Task CheckRequests()
            {
                List<(string id, Request request)> requests = Requests.GetRequests(Context.Guild.Id).Where(r => Context.Guild.Users.Any(u => u.Id == r.Value.UserId)).Select(r => (r.Key, r.Value)).ToList();

                if (!requests.Any())
                {
                    await RespondAsync("현재 요청이 없어요", ephemeral: true);

                    return;
                }

                int elementCountInEachPage = int.Parse(Configs.Get("FIELD_IN_EACH_PAGE"));

                LazyPaginator paginator = Context.CreatePagenator((int)Math.Ceiling((float)requests.Count / elementCountInEachPage) - 1, GeneratePage);

                await Interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

                PageBuilder GeneratePage(int index)
                {
                    PageBuilder page = Context.CreatePage(title: "요청 리스트");

                    for (int i = 0; i < elementCountInEachPage; i++)
                    {
                        int requestIndex = index * elementCountInEachPage + i;

                        if (requestIndex >= requests.Count)
                        {
                            break;
                        }

                        (string id, Request request) request = requests.ToList()[requestIndex];

                        SocketGuildUser user = Context.Guild.GetUser(request.request.UserId);

                        page.AddField($"#{request.id}", $"{user.Mention}: {request.request.Reward}\n<t:{request.request.Timestamp}>");
                    }

                    return page;
                }
            }

            [SlashCommand("승인", "요청을 승인합니다")]
            public async Task ApproveRequest([Autocomplete(typeof(RequestAutoComplete))] string id)
            {
                if (!Requests.HasRequest(Context.Guild.Id, id))
                {
                    await RespondAsync("해당 요청이 존재하지 않아요", ephemeral: true);

                    return;
                }

                await DeferAsync();

                Request request = Requests.GetRequest(Context.Guild.Id, id);

                SocketGuildUser user = Context.Guild.GetUser(request.UserId);

                ComponentBuilder component = new ComponentBuilder()
                    .WithButton("확인", "ok", ButtonStyle.Success)
                    .WithButton("취소", "cancel", ButtonStyle.Danger);

                EmbedBuilder emb = Context.CreateEmbed(title: $"#{id}", description: $"{user.Mention}: {request.Reward}\n<t:{request.Timestamp}>");

                IUserMessage msg = await FollowupAsync("다음 요청을 승인할까요?\n리워드를 지급한 이후 해당 요청을 승인해주세요", embed: emb.Build(), components: component.Build());

                InteractiveResult<SocketMessageComponent?> result = await Interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id && x.User.Id == Context.User.Id, timeout: TimeSpan.FromSeconds(30));

                if (!result.IsSuccess)
                {
                    return;
                }

                await result.Value.DeferAsync();

                if (result.Value.Data.CustomId == "ok")
                {
                    Requests.Remove(Context.Guild.Id, id);

                    await msg.ModifyAsync(m =>
                    {
                        m.Components = null;
                        m.Content = "다음 요청을 승인했어요";
                    });
                }
                else
                {
                    await msg.ModifyAsync(m =>
                    {
                        m.Components = null;
                        m.Content = "다음 요청의 승인을 취소했어요";
                    });
                }
            }

            [SlashCommand("거부", "요청을 거부합니다")]
            public async Task DisapproveRequest([Autocomplete(typeof(RequestAutoComplete))] string id)
            {
                if (!Requests.HasRequest(Context.Guild.Id, id))
                {
                    await RespondAsync("해당 요청이 존재하지 않아요", ephemeral: true);

                    return;
                }

                await DeferAsync();

                Request request = Requests.GetRequest(Context.Guild.Id, id);
                Reward reward = Rewards.GetReward(Context.Guild.Id, request.Reward);

                SocketGuildUser user = Context.Guild.GetUser(request.UserId);

                ComponentBuilder component = new ComponentBuilder()
                    .WithButton("확인", "ok", ButtonStyle.Success)
                    .WithButton("취소", "cancel", ButtonStyle.Danger);

                EmbedBuilder emb = Context.CreateEmbed(title: $"#{id}", description: $"{user.Mention}: {request.Reward}\n<t:{request.Timestamp}>");

                IUserMessage msg = await FollowupAsync($"다음 요청을 거부할까요?\n해당 유저가 {reward.Point} 포인트를 돌려받게 돼요", embed: emb.Build(), components: component.Build());

                InteractiveResult<SocketMessageComponent?> result = await Interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id && x.User.Id == Context.User.Id, timeout: TimeSpan.FromSeconds(30));

                if (!result.IsSuccess)
                {
                    return;
                }

                await result.Value.DeferAsync();

                if (result.Value.Data.CustomId == "ok")
                {
                    Points.AddPoint(Context.Guild.Id, request.UserId, reward.Point);
                    Requests.Remove(Context.Guild.Id, id);

                    await msg.ModifyAsync(m =>
                    {
                        m.Components = null;
                        m.Content = "다음 요청을 거부했어요";
                    });
                }
                else
                {
                    await msg.ModifyAsync(m =>
                    {
                        m.Components = null;
                        m.Content = "다음 요청의 거부를 취소했어요";
                    });
                }
            }
        }
    }
}
