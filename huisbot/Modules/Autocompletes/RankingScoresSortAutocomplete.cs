using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;

namespace huisbot.Modules.Autocompletes;

/// <summary>
/// Autocomplete for the sort parameter on the score rankings command.
/// </summary>
public class RankingScoresSortAutocomplete : AutocompleteHandler
{
  public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Return the sorting options.
    return Task.FromResult(AutocompletionResult.FromSuccess(Sort.RankingScores.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}