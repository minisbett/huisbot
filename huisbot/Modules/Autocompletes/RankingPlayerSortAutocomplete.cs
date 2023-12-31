using Discord;
using Discord.Interactions;
using huisbot.Enums;

namespace huisbot.Modules.Autocompletes;

/// <summary>
/// Autocomplete for the sort parameter on the player rankings command.
/// </summary>
public class RankingPlayersSortAutocomplete : AutocompleteHandler
{
  public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Return the sorting options.
    return Task.FromResult(AutocompletionResult.FromSuccess(Sort.RankingPlayers.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}