using Discord;
using Discord.Interactions;
using Pointless.AutoCompletes;
using Pointless.Managements;
using Pointless.Modals;
using Pointless.Utils;

namespace Pointless.Commands.Admins
{
    public partial class AdminCommands
    {
        [Group("리워드", "리워드를 관리할 수 있는 커맨드입니다")]
        public class AdminRewardCommands : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("추가", "서버에 리워드를 추가하는 관리자 전용 커맨드입니다")]
            public async Task AddRewards()
            {
                await Context.Interaction.RespondWithModalAsync<AddRewardModal>("add_reward");
            }

            [ModalInteraction("add_reward", true)]
            public async Task AddRewardModal(AddRewardModal modal)
            {
                if (Rewards.HasReward(Context.Guild.Id, modal.Name))
                {
                    await RespondAsync("같은 이름의 리워드가 있어요", ephemeral: true);

                    return;
                }

                if (!uint.TryParse(modal.Point, out uint point) || point < 1)
                {
                    await RespondAsync("포인트는 1보다 큰 정수여야 해요", ephemeral: true);

                    return;
                }

                if (modal.Name.Length > 256 || modal.Description.Length > 900)
                {
                    await RespondAsync("이름은 256자 이하, 설명은 900자 이하여야 해요", ephemeral: true);

                    return;
                }

                Rewards.AddReward(Context.Guild.Id, new(modal.Name, modal.Description, point));

                EmbedBuilder emb = Context.CreateEmbed();

                emb.AddField(modal.Name, $"{Icons.Point()} {point}\n\n{modal.Description}");

                await Context.RespondEmbedAsync(emb.Build(), "리워드를 추가했어요");
            }

            [SlashCommand("제거", "리워드를 제거하는 관리자 전용 커맨드입니다")]
            public async Task RemoveRewards([Autocomplete(typeof(RewardAutoComplete))] Reward reward)
            {
                Rewards.RemoveReward(Context.Guild.Id, reward.Name);

                EmbedBuilder emb = Context.CreateEmbed();

                emb.AddField(reward.Name, $"{Icons.Point()} {reward.Point}\n\n{reward.Description}");

                await Context.RespondEmbedAsync(emb.Build(), "리워드를 제거했어요");
            }
        }
    }
}
