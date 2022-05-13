using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands
{
    public class RankingCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractiveService Interactive { get; set; }

        [SlashCommand("랭킹", "서버의 포인트 랭킹을 확인합니다")]
        [EnabledInDm(false)]
        public async Task Ranking()
        {
            List<KeyValuePair<string, uint>> points = Guild.Get(Context.Guild.Id).Points.ToList();

            points = points.Where(p => Context.Guild.Users.Any(u => u.Id == ulong.Parse(p.Key)) && p.Value > 0).ToList();

            points.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));

            int elementCountInEachPage = int.Parse(Configs.Get("LINE_IN_EACH_PAGE"));

            LazyPaginator paginator = Context.CreatePagenator((int)Math.Ceiling((float)points.Count / elementCountInEachPage) - 1, GeneratePage);

            await Interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

            PageBuilder GeneratePage(int index)
            {
                PageBuilder page = Context.CreatePage(title: $"{Context.Guild.Name}의 랭킹");

                for (int i = 0; i < elementCountInEachPage; i++)
                {
                    int rankIndex = index * elementCountInEachPage + i;

                    if (rankIndex >= points.Count)
                    {
                        break;
                    }

                    SocketGuildUser? user = Context.Guild.GetUser(ulong.Parse(points[rankIndex].Key));

                    page.Description += $"{rankIndex + 1}위: {user.Mention} - `{MathF.Round(points[rankIndex].Value)}포인트`\n";
                }

                if ((points.FindIndex(p => ulong.Parse(p.Key) == Context.User.Id) is not -1 and var rank) && (rank < index * elementCountInEachPage || rank >= index * elementCountInEachPage + elementCountInEachPage))
                {
                    page.Description += $"\n{rank + 1}위: {((SocketGuildUser)Context.User).DisplayName} - `{MathF.Round(points[rank].Value)}포인트`\n";
                }

                return page;
            }
        }
    }
}
