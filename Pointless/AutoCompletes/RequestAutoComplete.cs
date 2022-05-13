using Discord;
using Discord.Interactions;
using Pointless.Managements;

namespace Pointless.AutoCompletes
{
    public class RequestAutoComplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<string> requests = Requests.GetRequests(context.Guild.Id).Where(r => context.Guild.GetUserAsync(r.Value.UserId).IsCompletedSuccessfully).Select(r => r.Key).ToList();

            return AutocompletionResult.FromSuccess(requests.Select(r => new AutocompleteResult($"#{r}", r)));
        }
    }
}
