using Discord;
using Discord.Interactions;
using Pointless.Managements;

namespace Pointless.TypeConverters
{
    public class RewardTypeConverter<T> : TypeConverter<T> where T : Reward
    {
        public override ApplicationCommandOptionType GetDiscordType()
        {
            return ApplicationCommandOptionType.String;
        }

        public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            string value;

            if (option.Value is Optional<object> optional)
            {
                value = optional.IsSpecified ? (string)optional.Value : "";
            }
            else
            {
                value = (string)option.Value;
            }

            if (Rewards.HasReward(context.Guild.Id, value))
            {
                return TypeConverterResult.FromSuccess(Rewards.GetReward(context.Guild.Id, value));
            }
            else
            {
                await context.Interaction.RespondAsync("해당 리워드를 찾을 수 없어요", ephemeral: true);

                return TypeConverterResult.FromError(InteractionCommandError.BadArgs, "해당 리워드를 찾을 수 없어요");
            }
        }
    }
}
