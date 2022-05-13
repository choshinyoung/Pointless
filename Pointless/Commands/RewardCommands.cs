using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Pointless.AutoCompletes;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands
{
    [Group("리워드", "리워드를 확인하거나 구매할 수 있는 커맨드입니다")]
    [EnabledInDm(false)]
    public class RewardCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractiveService Interactive { get; set; }

        [SlashCommand("목록", "포인트로 구매할 수 있는 리워드의 목록입니다")]
        public async Task RewardList()
        {
            List<Reward> rewards = Rewards.GetRewards(Context.Guild.Id);

            if (!rewards.Any())
            {
                await RespondAsync("서버에 설정된 리워드가 없어요", ephemeral: true);

                return;
            }

            int elementCountInEachPage = int.Parse(Configs.Get("FIELD_IN_EACH_PAGE"));

            LazyPaginator paginator = Context.CreatePagenator((int)Math.Ceiling((float)rewards.Count / elementCountInEachPage) - 1, GeneratePage);

            await Interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

            PageBuilder GeneratePage(int index)
            {
                PageBuilder page = Context.CreatePage(title: "리워드");

                for (int i = 0; i < elementCountInEachPage; i++)
                {
                    int rewardIndex = index * elementCountInEachPage + i;

                    if (rewardIndex >= rewards.Count)
                    {
                        break;
                    }

                    Reward reward = rewards[rewardIndex];

                    page.AddField(reward.Name, $"{Icons.Point()} {reward.Point}\n\n{reward.Description}");
                }

                return page;
            }
        }

        [SlashCommand("구매", "리워드를 구매합니다")]
        public async Task GetReward([Autocomplete(typeof(RewardAutoComplete))] Reward reward)
        {
            uint point = Points.GetPoint(Context.Guild.Id, Context.User.Id);

            if (point < reward.Point)
            {
                await RespondAsync($"{reward.Point - point} 포인트가 부족해서 구매할 수 없어요", ephemeral: true);

                return;
            }

            Points.RemovePoint(Context.Guild.Id, Context.User.Id, reward.Point);
            string id = Requests.Add(Context.Guild.Id, new(Context.User.Id, reward.Name, Context.Interaction.CreatedAt.ToUnixTimeSeconds()));

            await Context.RespondAsync($"{reward.Point} 포인트를 사용해서 {reward.Name} 리워드를 구매했어요\n어드민에게 요청을 보냈어요\n\n요청 id: #{id}");
        }

        [SlashCommand("환불", "관리자에 의해 승인되지 않은 리워드를 환불합니다")]
        public async Task ReturnReward([Autocomplete(typeof(UserRequestAutoComplete))] string id)
        {
            if (!Requests.HasRequest(Context.Guild.Id, id))
            {
                await RespondAsync("해당 요청이 존재하지 않아요", ephemeral: true);

                return;
            }

            Request request = Requests.GetRequest(Context.Guild.Id, id);
            Reward reward = Rewards.GetReward(Context.Guild.Id, request.Reward);

            if (request.UserId != Context.User.Id)
            {
                await RespondAsync("올바르지 않은 요청이에요", ephemeral: true);
            }

            Points.AddPoint(Context.Guild.Id, Context.User.Id, reward.Point);
            Requests.Remove(Context.Guild.Id, id);

            await Context.RespondAsync($"{reward.Name} 리워드를 환불하고 {reward.Point} 포인트를 받았어요");
        }

        [SlashCommand("구매내역", "승인되지 않은 구매한 리워드의 목록입니다")]
        public async Task RequestList()
        {
            List<(string id, Request request)> requests = Requests.GetRequests(Context.Guild.Id).Where(r => r.Value.UserId == Context.User.Id).Select(r => (r.Key, r.Value)).ToList();

            if (!requests.Any())
            {
                await RespondAsync("구매한 요청이 존재하지 않아요", ephemeral: true);

                return;
            }

            int elementCountInEachPage = int.Parse(Configs.Get("FIELD_IN_EACH_PAGE"));

            LazyPaginator paginator = Context.CreatePagenator((int)Math.Ceiling((float)requests.Count / elementCountInEachPage) - 1, GeneratePage);

            await Interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

            PageBuilder GeneratePage(int index)
            {
                PageBuilder page = Context.CreatePage(title: "구매 내역");

                for (int i = 0; i < elementCountInEachPage; i++)
                {
                    int requestIndex = index * elementCountInEachPage + i;

                    if (requestIndex >= requests.Count)
                    {
                        break;
                    }

                    (string id, Request request) request = requests[requestIndex];

                    page.AddField($"#{request.id}", $"{request.request.Reward}\n<t:{request.request.Timestamp}>");
                }

                return page;
            }
        }
    }
}
