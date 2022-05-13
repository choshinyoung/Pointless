using Discord;
using Discord.Interactions;
using Pointless.Managements;

namespace Pointless.AutoCompletes
{
    public class UserRequestAutoComplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<string> requests = Requests.GetRequests(context.Guild.Id).Where(r => r.Value.UserId == context.User.Id).Select(r => r.Key).ToList();

            return AutocompletionResult.FromSuccess(requests.Select(r => new AutocompleteResult($"#{r}", r)));
        }
    }
}
