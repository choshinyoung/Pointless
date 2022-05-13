using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Pointless.AutoCompletes;
using Pointless.Managements;
using Pointless.Modals;
using Pointless.Utils;

namespace Pointless.Commands.Admins
{
    public partial class AdminCommands
    {
        [Group("토토", "포인트를 걸고 결과를 예측하는 게임 커맨드입니다")]
        public class AdminTotoCommands : InteractionModuleBase<SocketInteractionContext>
        {
            public InteractiveService Interactive { get; set; }

            [SlashCommand("시작", "포인트를 걸고 결과를 예측하는 토토 게임을 시작합니다")]
            public async Task StartToto(string name, string item1, string item2, string? item3 = null, string? item4 = null, string? item5 = null, string? item6 = null, string? item7 = null, string? item8 = null, string? item9 = null, string? item10 = null)
            {
                if (Totos.HasToto(Context.Guild.Id, name))
                {
                    await RespondAsync("같은 이름의 토토가 진행중이에요", ephemeral: true);

                    return;
                }

                List<string> items = new string[] { item1, item2, item3, item4, item5, item6, item7, item8, item9, item10 }.Where(x => x != null).ToList();

                if (items.Distinct().Count() != items.Count)
                {
                    await RespondAsync("중복된 항목이 있어요", ephemeral: true);

                    return;
                }

                if (name.Length > 256 || items.Any(i => i.Length > 80))
                {
                    await RespondAsync("이름은 256자 이하, 각 항목은 80자 이하여야 해요", ephemeral: true);

                    return;
                }

                if (items.Contains("customPoint"))
                {
                    await RespondAsync("`customPoint`는 항목 이름으로 사용할 수 없어요", ephemeral: true);

                    return;
                }

                EmbedBuilder emb = Context.CreateEmbed("토토를 시작중이에요");

                ulong messageId = (await Context.Channel.SendMessageAsync(embed: emb.Build())).Id;

                Totos.AddToto(Context.Guild.Id, name, items, Context.Channel.Id, messageId);

                await RenderToto(Context.Guild.Id, name);

                await RespondAsync("토토를 시작했어요", ephemeral: true);
            }

            [SlashCommand("마감", "토토의 예측을 마감합니다")]
            public async Task EndPrediction([Autocomplete(typeof(PredictionUnendedTotoAutoComplete))] string name)
            {
                if (!Totos.HasToto(Context.Guild.Id, name))
                {
                    await RespondAsync("해당 토토를 찾을 수 없어요", ephemeral: true);

                    return;
                }

                Toto toto = Totos.GetToto(Context.Guild.Id, name);

                if (toto.IsPredictionEnded)
                {
                    await RespondAsync("이미 예측이 마감된 토토예요", ephemeral: true);

                    return;
                }

                Totos.EndPrediction(Context.Guild.Id, name);

                await RenderToto(Context.Guild.Id, name);

                await RespondAsync($"{name} 토토의 예측을 마감했어요", ephemeral: true);
            }

            [SlashCommand("종료", "예측 결과를 결정하고 토토 게임을 종료합니다")]
            public async Task EndToto([Autocomplete(typeof(TotoAutoComplete))] string name, string result)
            {
                if (!Totos.HasToto(Context.Guild.Id, name))
                {
                    await RespondAsync("해당 토토를 찾을 수 없어요", ephemeral: true);

                    return;
                }

                Toto toto = Totos.GetToto(Context.Guild.Id, name);

                if (!toto.Items.ContainsKey(result))
                {
                    await RespondAsync("존재하지 않는 항목이에요", ephemeral: true);

                    return;
                }

                long totalPoints = toto.Items.Sum(i => i.Value.Sum(v => v.Point));
                float totalPoint = toto.Items[result].Sum(v => v.Point);

                float ratio = totalPoint == 0 ? totalPoints : MathF.Round(((totalPoints - totalPoint) / totalPoint + 1) * 10) / 10;

                foreach (Toto.Item user in toto.Items[result])
                {
                    Points.AddFloatPoint(Context.Guild.Id, user.UserId, user.Point * ratio);
                }

                SocketTextChannel channel = Program.Client.GetGuild(Context.Guild.Id).GetTextChannel(toto.ChannelId);

                MessageReference? messageReference = null;

                if (Context.Guild.CurrentUser.GetPermissions((IGuildChannel)Context.Channel).ReadMessageHistory)
                {
                    messageReference = new MessageReference(toto.MessageId, toto.ChannelId, Context.Guild.Id);
                }

                await channel.SendMessageAsync($"게임이 종료됐어요\n예측 결과: {result}", allowedMentions: AllowedMentions.None, messageReference: messageReference);

                Totos.RemoveToto(Context.Guild.Id, name);

                await RespondAsync("게임을 종료했어요", ephemeral: true);
            }

            public static async Task HandleButtonEvent(SocketInteraction interaction, ulong messageId, string item)
            {
                ulong guildId = ((SocketTextChannel)interaction.Channel).Guild.Id;

                List<Toto> totos = Totos.GetTotos(guildId);

                if (totos.Find(t => t.MessageId == messageId) is not null and var toto)
                {
                    if (item == "customPoint")
                    {
                        await interaction.RespondWithModalAsync<TotoCustomPointModal>($"toto_custom_point:{toto.Name}");
                    }
                    else
                    {
                        await AddPoint(interaction, toto, item, 10);
                    }
                }
            }

            public static async Task AddPoint(SocketInteraction interaction, Toto toto, string item, uint point)
            {
                ulong guildId = ((SocketTextChannel)interaction.Channel).Guild.Id;

                if (!toto.Items.ContainsKey(item))
                {
                    return;
                }

                if (toto.Items.Any(i => i.Key != item && i.Value.Any(v => v.UserId == interaction.User.Id)))
                {
                    string selectedItem = toto.Items.Where(i => i.Value.Any(v => v.UserId == interaction.User.Id)).First().Key;

                    await interaction.RespondAsync($"이미 {selectedItem} 항목을 선택했어요", ephemeral: true);

                    return;
                }

                if (Points.GetPoint(guildId, interaction.User.Id) < point)
                {
                    await interaction.RespondAsync("포인트가 부족해서 예측을 할 수 없어요", ephemeral: true);

                    return;
                }

                Points.RemovePoint(guildId, interaction.User.Id, point);
                Totos.AddPoint(guildId, toto.Name, interaction.User.Id, item, point);

                await RenderToto(guildId, toto.Name);

                uint totalPoint = Totos.GetToto(guildId, toto.Name).Items[item].Find(t => t.UserId == interaction.User.Id)!.Point;

                await interaction.RespondAsync($"{item} 항목에 총 {totalPoint}포인트를 걸었어요", ephemeral: true);
            }

            [ModalInteraction("toto_custom_point:*", true)]
            public async Task TotoCustomPoint(string name, TotoCustomPointModal modal)
            {
                Toto toto = Totos.GetToto(Context.Guild.Id, name);

                if (toto.IsPredictionEnded)
                {
                    return;
                }

                if (!uint.TryParse(modal.Point, out uint point) || point < 1)
                {
                    await RespondAsync("포인트는 1보다 큰 정수여야 해요", ephemeral: true);

                    return;
                }

                if (toto.Items.Any(t => t.Value.Any(i => i.UserId == Context.User.Id)))
                {
                    string item = toto.Items.Where(t => t.Value.Any(i => i.UserId == Context.User.Id)).First().Key;

                    await AddPoint(Context.Interaction, toto, item, point);
                }
                else
                {
                    await DeferAsync();

                    ComponentBuilder component = new ComponentBuilder()
                        .WithSelectMenu(new SelectMenuBuilder("item", toto.Items.Select(t => new SelectMenuOptionBuilder(t.Key, t.Key)).ToList()));

                    IUserMessage msg = await FollowupAsync("어떤 항목에 포인트를 걸지 선택해주세요", ephemeral: true, components: component.Build());

                    InteractiveResult<SocketMessageComponent?> result = await Interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id && x.User.Id == Context.User.Id, timeout: TimeSpan.FromMinutes(5));

                    if (!result.IsSuccess)
                    {
                        return;
                    }

                    await AddPoint(result.Value, toto, result.Value.Data.Values.ToArray()[0], point);
                }
            }

            public static async Task RenderToto(ulong guildId, string name)
            {
                Toto toto = Totos.GetToto(guildId, name);

                EmbedBuilder emb = SocketInteractionContextExtensions.CreateEmbed(toto.IsPredictionEnded ? "결과를 기다리고 있어요" : "예측이 진행중이에요", toto.Name);
                ComponentBuilder component = new();

                long totalPoints = toto.Items.Sum(i => i.Value.Sum(v => v.Point));

                foreach (KeyValuePair<string, List<Toto.Item>> item in toto.Items)
                {
                    float totalPoint = item.Value.Sum(v => v.Point);

                    float ratio = totalPoint == 0 ? totalPoints : MathF.Round(((totalPoints - totalPoint) / totalPoint + 1) * 10) / 10;
                    uint highestBet = item.Value.Any() ? item.Value.Max(v => v.Point) : 0;

                    float percentage = totalPoint == 0 ? 0 : MathF.Round(totalPoint / totalPoints * 100);
                    int percentCount = (int)MathF.Round(percentage / 10);

                    emb.AddField(item.Key, $"{Icons.PercentageBar()} {new string(Enumerable.Repeat('█', percentCount).ToArray())}{new string(Enumerable.Repeat(' ', 10 - percentCount).ToArray())} | {percentage}%\n" +
                        $"{Icons.Point()} {item.Value.Sum(v => v.Point)}\n{Icons.People()} {item.Value.Count}\n{Icons.Ratio()} 1:{ratio}\n{Icons.HighestBet()} {highestBet}", inline: toto.Items.Count == 2);

                    component.WithButton(item.Key, item.Key, disabled: toto.IsPredictionEnded);
                }

                component.WithButton("예측 포인트 직접 지정하기", "customPoint", ButtonStyle.Secondary, disabled: toto.IsPredictionEnded, row: 2);

                SocketTextChannel channel = Program.Client.GetGuild(guildId).GetTextChannel(toto.ChannelId);
                IMessage msg = await channel.GetMessageAsync(toto.MessageId);

                if (msg is SocketUserMessage message)
                {
                    await message.ModifyAsync(m =>
                    {
                        m.Embed = emb.Build();
                        m.Components = component.Build();
                    });
                }
                else
                {
                    await msg.DeleteAsync();
                    ulong messageId = (await channel.SendMessageAsync(embed: emb.Build(), components: component.Build())).Id;

                    Totos.ChangeMessage(guildId, name, messageId);
                }
            }
        }
    }
}
