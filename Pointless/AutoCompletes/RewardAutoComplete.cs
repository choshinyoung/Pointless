using Discord;
using Discord.Interactions;
using Pointless.Managements;

namespace Pointless.AutoCompletes
{
    public class RewardAutoComplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<Reward> rewards = Rewards.GetRewards(context.Guild.Id);

            return AutocompletionResult.FromSuccess(rewards.Select(r => new AutocompleteResult(r.Name, r.Name)));
        }
    }
}
