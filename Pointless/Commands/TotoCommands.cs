using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Pointless.AutoCompletes;
using Pointless.Managements;
using Pointless.Utils;

namespace Pointless.Commands
{
    [Group("토토", "토토 관련 정보를 확인할 수 있는 커맨드입니다")]
    [EnabledInDm(false)]
    public class TotoCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractiveService Interactive { get; set; }

        [SlashCommand("목록", "진행중인 게임의 목록입니다")]
        public async Task TotoList()
        {
            List<Toto> totos = Totos.GetTotos(Context.Guild.Id);

            if (!totos.Any())
            {
                await RespondAsync("서버에서 진행중인 토토가 없어요", ephemeral: true);

                return;
            }

            int elementCountInEachPage = int.Parse(Configs.Get("FIELD_IN_EACH_PAGE"));

            LazyPaginator paginator = Context.CreatePagenator((int)Math.Ceiling((float)totos.Count / elementCountInEachPage) - 1, GeneratePage);

            await Interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

            PageBuilder GeneratePage(int index)
            {
                PageBuilder page = Context.CreatePage(title: "토토");

                for (int i = 0; i < elementCountInEachPage; i++)
                {
                    int totoIndex = index * elementCountInEachPage + i;

                    if (totoIndex >= totos.Count)
                    {
                        break;
                    }

                    Toto toto = totos[totoIndex];

                    page.AddField(toto.Name, $"{(toto.IsPredictionEnded ? "결과 대기중" : "예측 진행중")}\n[바로가기](https://discord.com/channels/{Context.Guild.Id}/{toto.ChannelId}/{toto.MessageId})");
                }

                return page;
            }
        }

        [SlashCommand("정보", "해당 토토의 자신의 예측 정보를 확인합니다")]
        public async Task TotoInfo([Autocomplete(typeof(TotoAutoComplete))] string name)
        {
            if (!Totos.HasToto(Context.Guild.Id, name))
            {
                await RespondAsync("해당 토토를 찾을 수 없어요", ephemeral: true);

                return;
            }

            Toto toto = Totos.GetToto(Context.Guild.Id, name);

            if (!toto.Items.Any(i => i.Value.Any(v => v.UserId == Context.User.Id)))
            {
                await RespondAsync("해당 토토에 포인트를 걸지 않았어요", ephemeral: true);
            }

            string item = toto.Items.Where(i => i.Value.Any(v => v.UserId == Context.User.Id)).First().Key;
            uint point = toto.Items[item].Find(t => t.UserId == Context.User.Id)!.Point;

            long totalPoints = toto.Items.Sum(i => i.Value.Sum(v => v.Point));
            float totalPoint = toto.Items[item].Sum(v => v.Point);

            float ratio = MathF.Round(((totalPoints - totalPoint) / totalPoint + 1) * 10) / 10;
            uint highestBet = toto.Items[item].Any() ? toto.Items[item].Max(v => v.Point) : 0;

            uint earnablePoint = (uint)MathF.Ceiling(point * ratio);

            EmbedBuilder emb = Context.CreateEmbed(title: toto.Name);
            emb.AddField(item, $"{Icons.Point()} {toto.Items[item].Sum(v => v.Point)}\n{Icons.People()} {toto.Items[item].Count}\n{Icons.Ratio()} 1:{ratio}\n{Icons.HighestBet()} {highestBet}\n\n내가 건 포인트: {Icons.Point()} {point}\n얻을 수 있는 포인트: {Icons.Point()} {earnablePoint}");

            await Context.RespondEmbedAsync(emb.Build());
        }
    }
}
