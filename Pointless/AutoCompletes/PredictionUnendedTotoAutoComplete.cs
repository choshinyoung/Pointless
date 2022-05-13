using Discord;
using Discord.Interactions;
using Pointless.Managements;

namespace Pointless.AutoCompletes
{
    public class PredictionUnendedTotoAutoComplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<string> totos = Totos.GetTotos(context.Guild.Id).Where(t => !t.IsPredictionEnded).Select(t => t.Name).ToList();

            return AutocompletionResult.FromSuccess(totos.Select(r => new AutocompleteResult(r, r)));
        }
    }
}
